(function () {
  const cfg = window.aiChatbotWidgetConfig || {};
  if (!cfg.endpoint) return;

  const root = document.getElementById('aiChatWidgetRoot');
  const panel = document.getElementById('aiChatWidgetPanel');
  const bubble = document.getElementById('aiChatWidgetBubble');
  const closeBtn = document.getElementById('aiChatWidgetClose');
  const stream = document.getElementById('aiChatWidgetStream');
  const input = document.getElementById('aiChatWidgetInput');
  const sendBtn = document.getElementById('aiChatWidgetSend');
  const langs = document.getElementById('aiChatWidgetLanguages');
  const openFullLink = document.getElementById('aiChatWidgetOpenFull');
  if (!root || !panel || !bubble || !stream || !input || !sendBtn) return;

  const storageKey = cfg.storageKey || 'dop_ai_chatbot_session';
  const legacyStorageKey = 'dop_ai_chatbot_widget_session';
  let sessionToken = '';
  let booted = false;
  let realtimeChannel = null;
  let pollTimer = null;
  let handoffMessageShown = false;
  let langChipMap = {}; // code → DOM element
  let selectedLanguage = ''; // explicitly chosen by the user via language chip

  const escapeHtml = (value) => String(value || '').replace(/[&<>"']/g, (m) => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
  }[m]));

  // Build the empty/welcome state HTML from starter prompts in config.
  function buildEmptyStateHtml(starterPrompts) {
    const prompts = Array.isArray(starterPrompts) && starterPrompts.length ? starterPrompts : [];
    const cards = prompts.map((sp) => {
      const title = escapeHtml(sp.title || '');
      const sub = escapeHtml(sp.subtitle || '');
      const prompt = escapeHtml(sp.prompt || '');
      return '<button type="button" class="ai-chat-widget-suggestion" data-widget-suggestion="' + prompt + '">' +
        (title ? '<span class="ai-chat-widget-suggestion-title">' + title + '</span>' : '') +
        (sub   ? '<span class="ai-chat-widget-suggestion-sub">'   + sub   + '</span>' : '') +
        '</button>';
    }).join('');
    return '<div class="ai-chat-widget-empty">' +
      '<div class="ai-chat-widget-empty-icon"><i class="ri-robot-2-line"></i></div>' +
      '<p class="ai-chat-widget-empty-title">How can I help you?</p>' +
      '<p class="ai-chat-widget-empty-sub">Choose a suggestion or type your question below.</p>' +
      (cards ? '<div class="ai-chat-widget-suggestions">' + cards + '</div>' : '') +
      '</div>';
  }

  let emptyStateHtml = buildEmptyStateHtml(cfg.starterPrompts || []);

  // Render a SAFE subset of Markdown to HTML for assistant replies. Everything is
  // escaped first (XSS-safe), then a small set of inline/block patterns is applied:
  // **bold**, *italic*, `code`, bullet/numbered lists, and paragraphs.
  function renderMarkdown(text) {
    let s = escapeHtml(String(text || '').trim());
    // Headings (# .. ######, incl. closed "## Title ##") render as bold lines in chat bubbles.
    s = s.replace(/^\s*#{1,6}\s+(.+?)\s*#*\s*$/gm, '**$1**');
    s = s.replace(/`([^`]+)`/g, '<code>$1</code>');
    s = s.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    // Drop stray/unbalanced ** markers the model sometimes emits.
    s = s.replace(/\*\*/g, '');
    s = s.replace(/(^|[^*])\*([^*\n]+)\*(?!\*)/g, '$1<em>$2</em>');
    // Blocks may mix text and bullets (e.g. an intro line followed by "- item" lines
    // with no blank line between). Group consecutive bullet/numbered lines into
    // lists and the rest into paragraphs.
    const blocks = s.split(/\n{2,}/);
    return blocks.map((block) => {
      const lines = block.split('\n');
      let html = '';
      let para = [];
      let list = [];
      let listTag = '';
      const flushPara = () => { if (para.length) { html += '<p>' + para.join('<br>') + '</p>'; para = []; } };
      const flushList = () => { if (list.length) { html += '<' + listTag + '>' + list.map((i) => '<li>' + i + '</li>').join('') + '</' + listTag + '>'; list = []; listTag = ''; } };
      lines.forEach((l) => {
        const ul = /^\s*[-*]\s+(.*)$/.exec(l);
        const ol = /^\s*\d+[.)]\s+(.*)$/.exec(l);
        if (ul || ol) {
          const tag = ul ? 'ul' : 'ol';
          flushPara();
          if (listTag && listTag !== tag) flushList();
          listTag = tag;
          list.push((ul || ol)[1]);
        } else {
          flushList();
          para.push(l);
        }
      });
      flushPara();
      flushList();
      return html;
    }).join('');
  }

  async function fetchJson(url, options) {
    const response = await fetch(url, options || {});
    const data = await response.json().catch(() => ({}));
    if (!response.ok) {
      throw new Error(data.error || 'Request failed');
    }
    return data;
  }

  function getStoredSessionToken() {
    const keys = [storageKey, legacyStorageKey];
    for (let i = 0; i < keys.length; i += 1) {
      try {
        const value = localStorage.getItem(keys[i]) || '';
        if (value) return value;
      } catch (e) {}
    }
    return '';
  }

  function setStoredSessionToken(token) {
    const value = String(token || '').trim();
    if (!value) return;
    try { localStorage.setItem(storageKey, value); } catch (e) {}
    try { localStorage.setItem(legacyStorageKey, value); } catch (e) {}
  }

  function updateFullChatLink() {
    if (!openFullLink) return;
    const baseHref = openFullLink.getAttribute('href') || 'chatbot.kml';
    try {
      const url = new URL(baseHref, window.location.href);
      if (sessionToken) {
        url.searchParams.set('session_token', sessionToken);
      } else {
        url.searchParams.delete('session_token');
      }
      openFullLink.setAttribute('href', url.toString());
    } catch (e) {
      if (sessionToken) {
        const join = baseHref.indexOf('?') === -1 ? '?' : '&';
        openFullLink.setAttribute('href', baseHref.split('?')[0] + join + 'session_token=' + encodeURIComponent(sessionToken));
      }
    }
  }

  function scrollToBottom() {
    stream.scrollTop = stream.scrollHeight;
  }

  function renderLanguages(languageMap, allowed) {
    if (!langs) return;
    langs.innerHTML = '';
    langChipMap = {};
    (allowed || []).forEach((code) => {
      const meta = languageMap[code];
      if (!meta) return;
      const chip = document.createElement('span');
      chip.className = 'ai-chat-widget-lang';
      chip.dataset.lang = code;
      chip.textContent = meta.label || code;
      langs.appendChild(chip);
      langChipMap[code] = chip;
    });
  }

  function setActiveLanguage(code, userChosen) {
    Object.values(langChipMap).forEach((el) => el.classList.remove('active'));
    if (code && langChipMap[code]) {
      langChipMap[code].classList.add('active');
    }
    if (userChosen) selectedLanguage = code || '';
  }

  // Lock language chips once the session language is committed (first reply received).
  function lockLanguage(code) {
    setActiveLanguage(code);
    selectedLanguage = code || '';
    // Make chips non-interactive — session language is now fixed.
    Object.values(langChipMap).forEach((el) => {
      el.style.pointerEvents = 'none';
      el.style.opacity = el.dataset.lang === code ? '1' : '0.35';
    });
  }

  function appendMessage(role, body, language, citations) {
    clearEmptyState();
    const title = role === 'user' ? 'You' : 'Assistant';
    const cls = role === 'user' ? 'ai-chat-widget-msg-user' : 'ai-chat-widget-msg-assistant';
    const cbRoot = (typeof url_root !== 'undefined' && url_root) ? url_root : '';
    const citationHtml = Array.isArray(citations) && citations.length
      ? '<div class="mt-2">' + citations.map((item) => {
          const label = escapeHtml(item.title || ('Topic #' + item.id));
          const id = parseInt(item.id, 10);
          // Link the citation to its Knowledge Center article when we have an id.
          return id > 0
            ? '<a class="ai-chat-widget-citation" href="' + cbRoot + 'kc/document.kml?id=' + id + '" target="_blank" rel="noopener"><i class="ri-file-text-line me-1"></i>' + label + '</a>'
            : '<span class="ai-chat-widget-citation">' + label + '</span>';
        }).join('') + '</div>'
      : '';
    // Assistant replies render as safe Markdown; user messages stay plain-escaped.
    const bodyHtml = role === 'user'
      ? '<div class="ai-chat-widget-body ai-chat-widget-body-user">' + escapeHtml(body) + '</div>'
      : '<div class="ai-chat-widget-body ai-chat-widget-body-assistant">' + renderMarkdown(body) + '</div>';
    stream.insertAdjacentHTML('beforeend',
      '<div class="ai-chat-widget-msg ' + cls + '">' +
        '<div class="ai-chat-widget-meta">' + escapeHtml(title) + (language ? ' · ' + escapeHtml(language) : '') + '</div>' +
        bodyHtml +
        citationHtml +
      '</div>'
    );
    scrollToBottom();
  }

  function clearEmptyState() {
    const el = stream.querySelector('.ai-chat-widget-empty');
    if (el) el.remove();
  }

  // ── Engagement: progressive reveal, follow-up chips, satisfaction rating ──

  // Reveal multi-block replies one block at a time (streaming feel).
  // Returns the total reveal duration so chips/rating can wait for it.
  function progressiveReveal() {
    const msgs = stream.querySelectorAll('.ai-chat-widget-msg-assistant');
    const node = msgs[msgs.length - 1];
    const container = node ? node.querySelector('.ai-chat-widget-body-assistant') : null;
    if (!container) return 0;
    const blocks = Array.prototype.slice.call(container.children);
    if (blocks.length < 2) return 0;
    blocks.forEach(function (b, i) {
      if (i === 0) return;
      b.style.display = 'none';
      setTimeout(function () { b.style.display = ''; scrollToBottom(); }, 320 * i);
    });
    return 320 * (blocks.length - 1);
  }

  let suggestionWrap = null;
  function clearSuggestions() {
    if (suggestionWrap && suggestionWrap.parentNode) suggestionWrap.parentNode.removeChild(suggestionWrap);
    suggestionWrap = null;
  }
  function renderSuggestions(list, delayMs) {
    if (!Array.isArray(list) || !list.length) return;
    setTimeout(function () {
      clearSuggestions();
      suggestionWrap = document.createElement('div');
      suggestionWrap.style.cssText = 'display:flex;flex-wrap:wrap;gap:6px;margin:4px 0 8px;';
      list.slice(0, 3).forEach(function (q) {
        const chip = document.createElement('button');
        chip.type = 'button';
        chip.textContent = q;
        chip.style.cssText = 'border:1px solid rgba(13,110,253,.35);background:#fff;color:#0d6efd;border-radius:999px;padding:4px 12px;font-size:12px;cursor:pointer;';
        chip.addEventListener('click', function () {
          clearSuggestions();
          input.value = q;
          sendMessage();
        });
        suggestionWrap.appendChild(chip);
      });
      stream.appendChild(suggestionWrap);
      scrollToBottom();
    }, Math.max(0, delayMs || 0));
  }

  let liveAssistantReplies = 0;
  let ratingShown = false;
  function maybeShowRating(delayMs) {
    if (ratingShown || liveAssistantReplies < 3 || !sessionToken) return;
    try {
      if (localStorage.getItem('ai_chat_rated_' + sessionToken) === '1') { ratingShown = true; return; }
    } catch (e) {}
    ratingShown = true;
    setTimeout(function () {
      const wrap = document.createElement('div');
      wrap.style.cssText = 'text-align:center;margin:8px 0;font-size:12px;color:#6c757d;';
      wrap.appendChild(document.createTextNode('Rate this chat: '));
      for (let i = 1; i <= 5; i++) {
        (function (val) {
          const star = document.createElement('button');
          star.type = 'button';
          star.textContent = '★';
          star.setAttribute('aria-label', val + ' star' + (val > 1 ? 's' : ''));
          star.style.cssText = 'border:none;background:none;color:#ced4da;font-size:18px;cursor:pointer;padding:0 2px;line-height:1;';
          star.addEventListener('mouseenter', function () {
            Array.prototype.slice.call(wrap.querySelectorAll('button')).forEach(function (b, idx) {
              b.style.color = idx < val ? '#f5b301' : '#ced4da';
            });
          });
          star.addEventListener('click', function () {
            const endpointBase = cfg.endpoint.replace(/\/api\/public\/.*$/, '/');
            fetch(endpointBase + 'api/public/ai-chatbot/rate', {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ session_token: sessionToken, rating: val }),
            }).catch(function () {});
            try { localStorage.setItem('ai_chat_rated_' + sessionToken, '1'); } catch (e) {}
            wrap.textContent = 'Thanks for your feedback! 🙏';
          });
          wrap.appendChild(star);
        })(i);
      }
      stream.appendChild(wrap);
      scrollToBottom();
    }, Math.max(0, delayMs || 0));
  }

  function renderMessages(messages) {
    stream.innerHTML = '';
    if (!Array.isArray(messages) || !messages.length) {
      if (emptyStateHtml) stream.innerHTML = emptyStateHtml;
      return;
    }
    messages.forEach((message) => {
      appendMessage(message.role === 'user' ? 'user' : 'assistant', message.body, message.language_code, message.citations);
    });
    // Highlight the language of the most recent message
    const last = messages[messages.length - 1];
    if (last && last.language_code) lockLanguage(last.language_code);
  }

  function setSending(isSending) {
    // The AI "thinking" indicator now lives in the chat stream, so the button just
    // disables briefly. Don't rewrite the label's textContent (it holds an icon).
    if (sendBtn) sendBtn.disabled = !!isSending;
  }

  function onAgentReply(msgData) {
    const body = msgData.body || msgData.message || '';
    if (!body) return;
    appendMessage('assistant', body, msgData.language_code || '', []);
  }

  function startPolling() {
    if (pollTimer || !sessionToken) return;
    let lastMessageCount = 0;
    pollTimer = setInterval(async () => {
      if (!sessionToken) return;
      try {
        const data = await fetchJson(cfg.endpoint + '?session_token=' + encodeURIComponent(sessionToken));
        const msgs = data.messages || [];
        if (msgs.length > lastMessageCount) {
          const newMsgs = msgs.slice(lastMessageCount);
          newMsgs.forEach((m) => {
            if (m.source === 'agent') {
              appendMessage('assistant', m.body, m.language_code || '', []);
            }
          });
          lastMessageCount = msgs.length;
        }
      } catch (e) {}
    }, 4000);
  }

  function stopPolling() {
    if (pollTimer) { clearInterval(pollTimer); pollTimer = null; }
  }

  function subscribeRealtime(rtCfg) {
    if (!sessionToken || !rtCfg || !rtCfg.enabled) return;
    const channelName = 'chatbot.' + sessionToken;
    try {
      if (rtCfg.provider === 'pusher') {
        if (typeof window.Pusher === 'undefined') {
          const s = document.createElement('script');
          s.src = 'https://js.pusher.com/8.2.0/pusher.min.js';
          s.onload = () => {
            const pusher = new window.Pusher(rtCfg.key, {
              cluster: rtCfg.cluster || 'eu',
              wsHost: rtCfg.wsHost || undefined,
              wsPort: rtCfg.wsPort || undefined,
              forceTLS: rtCfg.forceTLS !== false,
            });
            realtimeChannel = pusher.subscribe(channelName);
            realtimeChannel.bind('agent-reply', onAgentReply);
          };
          document.head.appendChild(s);
        } else {
          const pusher = new window.Pusher(rtCfg.key, {
            cluster: rtCfg.cluster || 'eu',
            wsHost: rtCfg.wsHost || undefined,
            wsPort: rtCfg.wsPort || undefined,
            forceTLS: rtCfg.forceTLS !== false,
          });
          realtimeChannel = pusher.subscribe(channelName);
          realtimeChannel.bind('agent-reply', onAgentReply);
        }
      } else if (rtCfg.provider === 'ably') {
        if (typeof window.Ably === 'undefined') {
          const s = document.createElement('script');
          s.src = 'https://cdn.ably.com/lib/ably.min-2.js';
          s.onload = () => {
            const ably = new window.Ably.Realtime({ authUrl: cfg.endpoint.replace('/api/public/ai-chatbot/chat', '') + 'api/modules/realtime/auth' });
            realtimeChannel = ably.channels.get(channelName);
            realtimeChannel.subscribe('agent-reply', (msg) => onAgentReply(msg.data || {}));
          };
          document.head.appendChild(s);
        } else {
          const ably = new window.Ably.Realtime({ authUrl: cfg.endpoint.replace('/api/public/ai-chatbot/chat', '') + 'api/modules/realtime/auth' });
          realtimeChannel = ably.channels.get(channelName);
          realtimeChannel.subscribe('agent-reply', (msg) => onAgentReply(msg.data || {}));
        }
      } else {
        startPolling();
      }
    } catch (e) {
      startPolling();
    }
  }

  async function bootstrap() {
    sessionToken = getStoredSessionToken();
    const data = await fetchJson(cfg.endpoint + (sessionToken ? ('?session_token=' + encodeURIComponent(sessionToken)) : ''));
    sessionToken = data.session_token || sessionToken;
    if (sessionToken) setStoredSessionToken(sessionToken);
    // Rebuild empty state from live API starter_prompts if present (overrides PHP-rendered config).
    if (data.bot && Array.isArray(data.bot.starter_prompts)) {
      emptyStateHtml = buildEmptyStateHtml(data.bot.starter_prompts);
    }
    renderLanguages(data.languages || {}, (data.bot && data.bot.allowed_languages) || []);
    renderMessages(data.messages || []);
    // Proactive greeting: when the widget auto-opened a brand-new conversation,
    // show the configured greeting as the first assistant bubble (display-only;
    // it is not persisted — the real welcome/flow takes over on first reply).
    if ((!Array.isArray(data.messages) || !data.messages.length) && window.__aiChatProactiveGreeting) {
      const g = String(window.__aiChatProactiveGreeting || '').trim();
      window.__aiChatProactiveGreeting = '';
      if (g) appendMessage('assistant', g, '', []);
    }
    // Lock to the session's committed language immediately (server only sends this
    // when the session has existing messages, so a stale empty token won't lock).
    if (data.session_language) {
      lockLanguage(data.session_language);
    }
    if (data.session_status === 'handoff') {
      handoffMessageShown = true;
      appendMessage('assistant', '⏳ A support agent has been notified and is handling this conversation. Please hold on.', '', []);
    }
    updateFullChatLink();
    subscribeRealtime(data.realtime || {});
    booted = true;
  }

  // Show an in-chat "thinking" bubble (animated dots) while the AI generates a
  // reply. Returns a remover so it can be replaced by the real answer.
  function showThinking() {
    const node = document.createElement('div');
    node.className = 'ai-chat-widget-msg ai-chat-widget-msg-assistant ai-chat-widget-thinking';
    node.innerHTML =
      '<div class="ai-chat-widget-meta">Assistant</div>' +
      '<div class="ai-chat-widget-dots"><span></span><span></span><span></span></div>';
    stream.appendChild(node);
    scrollToBottom();
    return function remove() { if (node && node.parentNode) node.parentNode.removeChild(node); };
  }

  // Human pacing: keep the typing dots up a moment longer, proportional to how
  // much "typing" the reply would take. Instant walls of text feel robotic.
  function humanTypingDelay(text) {
    // Shorter now that progressive reveal carries part of the pacing.
    const ms = Math.min(1500, Math.max(500, String(text || '').length * 10));
    return new Promise(function (resolve) { setTimeout(resolve, ms); });
  }

  let sending = false;
  async function sendMessage() {
    if (sending) return;            // guard against double-send / re-entry
    const message = input.value.trim();
    if (!message) return;
    sending = true;
    clearSuggestions();
    appendMessage('user', message, '', []);
    input.value = '';
    setSending(true);
    const stopThinking = showThinking();
    try {
      const data = await fetchJson(cfg.endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          session_token: sessionToken,
          message,
          ...(selectedLanguage ? { language_preference: selectedLanguage } : {})
        })
      });
      if (data.session_token) {
        sessionToken = data.session_token;
        setStoredSessionToken(sessionToken);
        updateFullChatLink();
      }
      if (data.reply) {
        await humanTypingDelay(data.reply.body);
        stopThinking();
        appendMessage('assistant', data.reply.body, data.reply.language, data.reply.citations);
        const revealMs = progressiveReveal();
        liveAssistantReplies += 1;
        renderSuggestions(data.reply.suggested_questions, revealMs + 150);
        maybeShowRating(revealMs + 450);
        if (data.reply.language) lockLanguage(data.reply.language);
        if (voiceReplyPending && window._aiWidgetSpeak) {
          window._aiWidgetSpeak(data.reply.body, data.reply.language || 'english');
        }
      } else if (data.handoff) {
        stopThinking();
        if (!handoffMessageShown) {
          handoffMessageShown = true;
          appendMessage('assistant', '⏳ A support agent has been notified and will reply shortly. Please hold on.', '', []);
        } else {
          appendMessage('assistant', '✓ Message received. The agent will reply soon.', '', []);
        }
      } else {
        stopThinking();
      }
    } catch (error) {
      stopThinking();
      appendMessage('assistant', error.message || 'Unable to answer right now.', 'english', []);
    } finally {
      sending = false;
      voiceReplyPending = false;
      setSending(false);
      input.focus();
    }
  }

  async function openPanel() {
    panel.classList.remove('d-none');
    panel.setAttribute('aria-hidden', 'false');
    bubble.classList.add('d-none');
    if (!booted) {
      try {
        await bootstrap();
      } catch (error) {
        stream.innerHTML = '<div class="text-danger small">Unable to load the assistant right now: ' + escapeHtml(error.message || 'Unknown error') + '</div>';
      }
    } else {
      updateFullChatLink();
    }
    setTimeout(scrollToBottom, 80);
    input.focus();
  }

  function closePanel() {
    panel.classList.add('d-none');
    panel.setAttribute('aria-hidden', 'true');
    bubble.classList.remove('d-none');
  }

  const newChatBtn = document.getElementById('aiChatWidgetNewChat');
  if (newChatBtn) {
    newChatBtn.addEventListener('click', () => {
      try { localStorage.removeItem(storageKey); } catch (e) {}
      try { localStorage.removeItem(legacyStorageKey); } catch (e) {}
      sessionToken = '';
      handoffMessageShown = false;
      stopPolling();
      realtimeChannel = null;
      booted = false;
      stream.innerHTML = '<div class="text-muted small"><span class="spinner-border spinner-border-sm me-2"></span>Loading assistant...</div>';
      Object.values(langChipMap).forEach((el) => el.classList.remove('active'));
      bootstrap().catch((err) => {
        stream.innerHTML = '<div class="text-danger small">' + escapeHtml(err.message || 'Unable to load.') + '</div>';
      });
    });
  }

  // Suggestion card click handler (delegated — cards are re-rendered on new chat).
  stream.addEventListener('click', (e) => {
    const btn = e.target.closest('[data-widget-suggestion]');
    if (!btn) return;
    const prompt = btn.getAttribute('data-widget-suggestion');
    if (prompt) { input.value = prompt; sendMessage(); }
  });

  // Language chip click — user explicitly picks a language preference.
  if (langs) {
    langs.addEventListener('click', (e) => {
      const chip = e.target.closest('[data-lang]');
      if (!chip) return;
      setActiveLanguage(chip.dataset.lang, true);
    });
  }

  // ── Voice: mic recording → Whisper STT → auto-send ──────────────────────────
  const micBtn = document.getElementById('aiChatWidgetMic');
  (function initVoice() {
    if (!micBtn) return;
    if (!navigator.mediaDevices || !window.MediaRecorder) {
      micBtn.style.display = 'none';
      return;
    }

    let currentLang = 'english';
    let mediaRecorder = null;
    let audioChunks = [];
    let micStream = null;
    let ttsAudio = null;

    // Speak assistant reply using OpenAI TTS (nova voice).
    function speakReply(text) {
      if (!text) return;
      const clean = text.replace(/<[^>]+>/g, '').replace(/[*_`#]/g, '').trim();
      if (!clean) return;

      if (ttsAudio) {
        ttsAudio.pause();
        ttsAudio = null;
      }

      const endpointBase = cfg.endpoint.replace(/\/api\/public\/.*$/, '/');
      fetch(endpointBase + 'api/public/ai-chatbot/tts', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text: clean, voice: 'nova' }),
      })
        .then(function (resp) {
          if (!resp.ok) throw new Error('TTS error ' + resp.status);
          return resp.blob();
        })
        .then(function (blob) {
          const url = URL.createObjectURL(blob);
          ttsAudio = new Audio(url);
          ttsAudio.addEventListener('ended', function () { URL.revokeObjectURL(url); ttsAudio = null; });
          ttsAudio.play().catch(function () {});
        })
        .catch(function () {});
    }

    function setMicState(state) {
      micBtn.classList.remove('recording', 'processing');
      if (state === 'recording') {
        micBtn.classList.add('recording');
        micBtn.innerHTML = '<i class="ri-stop-circle-line"></i>';
        micBtn.title = 'Stop recording';
      } else if (state === 'processing') {
        micBtn.classList.add('processing');
        micBtn.innerHTML = '<span class="spinner-border spinner-border-sm"></span>';
        micBtn.title = 'Transcribing…';
      } else {
        micBtn.innerHTML = '<i class="ri-mic-line"></i>';
        micBtn.title = 'Voice input';
      }
    }

    async function stopRecording() {
      if (!mediaRecorder || mediaRecorder.state === 'inactive') return;
      mediaRecorder.stop();
    }

    async function sendAudio(blob) {
      setMicState('processing');
      try {
        const endpointBase = cfg.endpoint.replace(/\/api\/public\/.*$/, '/');
        const formData = new FormData();
        formData.append('audio', blob, 'audio.webm');
        if (sessionToken) formData.append('session_token', sessionToken);
        const resp = await fetch(endpointBase + 'api/public/ai-chatbot/transcribe', {
          method: 'POST',
          body: formData,
        });
        const data = await resp.json().catch(() => ({}));
        if (!resp.ok || !data.transcript) {
          throw new Error(data.error || 'Transcription failed');
        }
        input.value = data.transcript;
        input.dispatchEvent(new Event('input'));
        setMicState('idle');
        voiceReplyPending = true;
        setTimeout(() => sendMessage(), 400);
      } catch (err) {
        voiceReplyPending = false;
        setMicState('idle');
        appendMessage('assistant', '🎤 ' + (err.message || 'Could not transcribe audio.'), '', []);
      }
    }

    async function startRecording() {
      try {
        micStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        audioChunks = [];
        const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
          ? 'audio/webm;codecs=opus'
          : MediaRecorder.isTypeSupported('audio/webm') ? 'audio/webm' : '';
        mediaRecorder = mimeType ? new MediaRecorder(micStream, { mimeType }) : new MediaRecorder(micStream);
        mediaRecorder.addEventListener('dataavailable', (e) => {
          if (e.data && e.data.size > 0) audioChunks.push(e.data);
        });
        mediaRecorder.addEventListener('stop', () => {
          micStream.getTracks().forEach((t) => t.stop());
          micStream = null;
          const blob = new Blob(audioChunks, { type: mediaRecorder.mimeType || 'audio/webm' });
          if (blob.size > 500) {
            sendAudio(blob);
          } else {
            setMicState('idle');
          }
        });
        mediaRecorder.start();
        setMicState('recording');
        // Auto-stop after 60 s to prevent runaway recordings.
        setTimeout(() => { if (mediaRecorder && mediaRecorder.state === 'recording') stopRecording(); }, 60000);
      } catch (err) {
        setMicState('idle');
        appendMessage('assistant', '🎤 Microphone access denied or unavailable.', '', []);
      }
    }

    micBtn.addEventListener('click', () => {
      if (mediaRecorder && mediaRecorder.state === 'recording') {
        stopRecording();
      } else {
        startRecording();
      }
    });

    // Expose speakReply so sendMessage can call it.
    window._aiWidgetSpeak = speakReply;
    // Track active language for TTS.
    const _origSetActive = setActiveLanguage;
    setActiveLanguage = function (code) {
      currentLang = code || 'english';
      _origSetActive(code);
    };
  }());

  let voiceReplyPending = false;
  // ─────────────────────────────────────────────────────────────────────────────

  bubble.addEventListener('click', openPanel);
  if (closeBtn) closeBtn.addEventListener('click', closePanel);
  sendBtn.addEventListener('click', sendMessage);
  input.addEventListener('keydown', (event) => {
    // Enter sends; Shift+Enter (or Ctrl/Cmd+Enter) inserts a newline.
    if (event.key === 'Enter' && !event.shiftKey && !event.ctrlKey && !event.metaKey) {
      event.preventDefault();
      sendMessage();
    }
  });

  // Draggable functionality for the widget panel
  const dragHandle = document.getElementById('aiChatWidgetDragHandle');
  if (dragHandle && root) {
    let isDragging = false;
    let pointerOffset = { x: 0, y: 0 };

    function clamp(value, min, max) {
      return Math.max(min, Math.min(max, value));
    }

    dragHandle.addEventListener('mousedown', (e) => {
      if (e.target.closest('#aiChatWidgetClose')) return; // Don't drag from close button
      isDragging = true;
      panel.classList.add('dragging');
      const rect = root.getBoundingClientRect();
      pointerOffset = { x: e.clientX - rect.left, y: e.clientY - rect.top };
      root.style.left = rect.left + 'px';
      root.style.top = rect.top + 'px';
      root.style.right = 'auto';
      root.style.bottom = 'auto';
      e.preventDefault();
    });

    document.addEventListener('mousemove', (e) => {
      if (!isDragging) return;
      const rect = root.getBoundingClientRect();
      const maxLeft = Math.max(12, window.innerWidth - rect.width - 12);
      const maxTop = Math.max(12, window.innerHeight - rect.height - 12);
      root.style.left = clamp(e.clientX - pointerOffset.x, 12, maxLeft) + 'px';
      root.style.top = clamp(e.clientY - pointerOffset.y, 12, maxTop) + 'px';
    });

    document.addEventListener('mouseup', () => {
      if (isDragging) {
        isDragging = false;
        panel.classList.remove('dragging');
      }
    });
  }

  // ── Proactive engagement (tawk.to-style auto-open) ──────────────────────────
  // Fires once per visitor per `frequency_days`, only if they haven't already
  // opened the chat manually. Trigger is one of: time | scroll | exit_intent.
  (function initProactive() {
    const pc = cfg.proactive || {};
    if (!pc.enabled) return;

    const PROACTIVE_KEY = (cfg.storageKey || 'dop_ai_chatbot_session') + '_proactive_at';
    const freqDays = Math.max(0, parseInt(pc.frequency_days, 10) || 0);

    function recentlyShown() {
      if (freqDays === 0) return false; // 0 = may show every page load
      try {
        const last = parseInt(localStorage.getItem(PROACTIVE_KEY) || '0', 10);
        if (!last) return false;
        return (Date.now() - last) < (freqDays * 86400000);
      } catch (e) { return false; }
    }

    let fired = false;
    function fire() {
      if (fired) return;
      // Don't interrupt a visitor who already opened the chat themselves.
      if (!panel.classList.contains('d-none')) return;
      fired = true;
      try { localStorage.setItem(PROACTIVE_KEY, String(Date.now())); } catch (e) {}
      const greeting = (pc.greeting || '').trim();
      if (greeting) { try { window.__aiChatProactiveGreeting = greeting; } catch (e) {} }
      openPanel();
    }

    if (recentlyShown()) return;

    const trigger = pc.trigger || 'time';
    if (trigger === 'scroll') {
      const pct = Math.max(5, Math.min(100, parseInt(pc.scroll_percent, 10) || 45));
      const onScroll = () => {
        const h = document.documentElement;
        const scrolled = (h.scrollTop || document.body.scrollTop);
        const height = (h.scrollHeight - h.clientHeight) || 1;
        if ((scrolled / height) * 100 >= pct) {
          window.removeEventListener('scroll', onScroll);
          fire();
        }
      };
      window.addEventListener('scroll', onScroll, { passive: true });
    } else if (trigger === 'exit_intent') {
      const onLeave = (e) => {
        if (e.clientY <= 0) {
          document.removeEventListener('mouseout', onLeave);
          fire();
        }
      };
      document.addEventListener('mouseout', onLeave);
    } else {
      // time (default)
      const delay = Math.max(2, Math.min(600, parseInt(pc.delay_seconds, 10) || 20));
      setTimeout(fire, delay * 1000);
    }
  })();
})();
