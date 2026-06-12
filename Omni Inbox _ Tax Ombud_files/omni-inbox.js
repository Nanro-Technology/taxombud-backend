(function() {
  const cfg = window.omniInboxConfig || {};
  if (!cfg.enabled) return;

  // ── DOM refs ──────────────────────────────────────────────────────────────
  const listEl        = document.getElementById('inboxList');
  const loadMoreBtn   = document.getElementById('inboxLoadMore');
  const messagesEl    = document.getElementById('inboxMessages');
  const titleEl       = document.getElementById('inboxActiveTitle');
  const metaEl        = document.getElementById('inboxActiveMeta');
  const contactEl     = document.getElementById('inboxActiveContact');
  const convoAvatarEl = document.getElementById('convoAvatarLg');
  const sendBtn       = document.getElementById('inboxSendBtn');
  const replyEl       = document.getElementById('inboxReplyBody');
  const searchEl      = document.getElementById('inboxSearch');
  const refreshBtn    = document.getElementById('inboxRefresh');
  const linkBtn       = document.getElementById('inboxLinkBtn');
  const deleteBtn     = document.getElementById('inboxDeleteBtn');
  const linkModalEl   = document.getElementById('linkContactModal');
  const linkModal     = linkModalEl ? new bootstrap.Modal(linkModalEl) : null;
  const linkInput     = document.getElementById('linkContactId');
  const lastRefreshEl = document.getElementById('inboxLastRefresh');
  const linkError     = document.getElementById('linkContactError');
  const linkSave      = document.getElementById('linkContactSave');
  const linkSearch    = document.getElementById('linkContactSearch');
  const linkResults   = document.getElementById('linkContactResults');
  const linkFilters   = document.querySelectorAll('.inbox-link-filter');
  const assignFilters = document.querySelectorAll('.inbox-assign-filter');
  const channelFilters= document.querySelectorAll('.inbox-channel-filter');
  const layoutEl      = document.getElementById('omniInboxLayout');
  const backBtn       = document.getElementById('inboxBackBtn');

  // ── State ─────────────────────────────────────────────────────────────────
  let conversations = [];
  let pageSize = 50;
  let nextOffset = 0;
  let hasMore = false;
  let activeConvo = null;
  let linkFilter = 'all';
  let assignFilter = 'all';
  let channelFilter = '';
  let searchTimer = null;
  let didSync = false;
  let messageRefreshTimer = null;

  const canReply  = cfg.canReply;
  const canManage = !!cfg.canManage;
  const replyChannels = Array.isArray(cfg.replyChannels) ? cfg.replyChannels.map(ch => String(ch || '').toLowerCase()) : [];

  // ── Helpers ───────────────────────────────────────────────────────────────
  const AVATAR_COLORS = [
    '#4f46e5','#0891b2','#059669','#d97706','#dc2626',
    '#7c3aed','#db2777','#0284c7','#16a34a','#9333ea'
  ];
  function avatarColor(name) {
    let h = 0;
    const s = String(name || '');
    for (let i = 0; i < s.length; i++) h = ((h << 5) - h) + s.charCodeAt(i);
    return AVATAR_COLORS[Math.abs(h) % AVATAR_COLORS.length];
  }
  function avatarInitials(name) {
    const parts = String(name || '').trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return '?';
    return (parts[0][0] + (parts[1] ? parts[1][0] : '')).toUpperCase();
  }

  function relativeTime(raw) {
    if (!raw) return '';
    const parsed = new Date(String(raw).replace(' ', 'T'));
    if (isNaN(parsed)) return raw;
    const now = Date.now();
    const diff = now - parsed.getTime();
    const m = Math.floor(diff / 60000);
    if (m < 1)   return 'Just now';
    if (m < 60)  return m + 'm ago';
    const h = Math.floor(m / 60);
    if (h < 24)  return h + 'h ago';
    const d = Math.floor(h / 24);
    if (d === 1) return 'Yesterday';
    if (d < 7)   return d + 'd ago';
    return parsed.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  }

  function dateLabel(raw) {
    if (!raw) return null;
    const parsed = new Date(String(raw).replace(' ', 'T'));
    if (isNaN(parsed)) return null;
    const today = new Date(); today.setHours(0,0,0,0);
    const yest  = new Date(today); yest.setDate(yest.getDate() - 1);
    const d     = new Date(parsed); d.setHours(0,0,0,0);
    if (d.getTime() === today.getTime()) return 'Today';
    if (d.getTime() === yest.getTime())  return 'Yesterday';
    return parsed.toLocaleDateString(undefined, { weekday: 'long', month: 'short', day: 'numeric' });
  }

  const normalizeChannel = (value) => {
    const v = String(value || '').toLowerCase();
    const map = {
      'imap':'email','pop3':'email','smtp':'email','mail':'email','gmail':'email','outlook':'email','exchange':'email',
      'system':'voice','pbx':'voice','call':'voice','calls':'voice',
      'sms':'sms','text':'sms','twilio':'sms','africastalking':'sms','kull':'sms',
      'whatsapp':'whatsapp','facebook':'facebook','instagram':'instagram','telegram':'telegram','twitter':'twitter',
      'tawk':'chat'
    };
    return map[v] || v;
  };

  const channelMeta = (rawValue) => {
    const key = normalizeChannel(rawValue || '');
    const map = {
      chat:      { icon: 'ri-chat-3-line',          cls: 'omni-channel-chat',      label: 'Chat'      },
      email:     { icon: 'ri-mail-line',             cls: 'omni-channel-email',     label: 'Email'     },
      voice:     { icon: 'ri-phone-line',            cls: 'omni-channel-voice',     label: 'Calls'     },
      sms:       { icon: 'ri-message-2-line',        cls: 'omni-channel-sms',       label: 'SMS'       },
      whatsapp:  { icon: 'ri-whatsapp-line',         cls: 'omni-channel-whatsapp',  label: 'WhatsApp'  },
      facebook:  { icon: 'ri-facebook-circle-line',  cls: 'omni-channel-facebook',  label: 'Facebook'  },
      instagram: { icon: 'ri-instagram-line',        cls: 'omni-channel-instagram', label: 'Instagram' },
      telegram:  { icon: 'ri-telegram-line',         cls: 'omni-channel-telegram',  label: 'Telegram'  },
      twitter:   { icon: 'ri-twitter-x-line',        cls: 'omni-channel-twitter',   label: 'X'         }
    };
    return map[key] || { icon: 'ri-apps-2-line', cls: 'omni-channel-default', label: rawValue || 'Channel' };
  };

  const canReplyChannel = (channelKey) => {
    const key = String(channelKey || '').toLowerCase();
    if (!replyChannels.length) return ['email','whatsapp','facebook','instagram','telegram','twitter','sms'].includes(key);
    return replyChannels.includes(key);
  };

  const enabledChannels = Array.isArray(cfg.enabledChannels)
    ? cfg.enabledChannels.map(ch => normalizeChannel(ch)).filter(Boolean)
    : [];
  const isChannelEnabled = (channelKey) => {
    if (!enabledChannels.length) return true;
    return enabledChannels.includes(normalizeChannel(channelKey));
  };

  const escapeHtml = (str) => String(str || '').replace(/[&<>"']/g, (m) => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]));

  const linkifyText = (text) => {
    const escaped = escapeHtml(String(text || ''));
    return escaped.replace(/((?:https?:\/\/|www\.)[^\s<]+)/gi, (match) => {
      const href = /^https?:\/\//i.test(match) ? match : 'https://' + match;
      return `<a href="${escapeHtml(href)}" target="_blank" rel="noopener noreferrer">${match}</a>`;
    });
  };

  const normalizeEmail = (value) => {
    const raw = String(value || '');
    const match = raw.match(/[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}/i);
    return match ? match[0].toLowerCase() : '';
  };

  const setLastRefresh = (label) => {
    if (!lastRefreshEl) return;
    lastRefreshEl.textContent = 'Last refreshed: ' + (label || new Date().toLocaleTimeString());
  };

  const setPressedState = (buttons, activeButton) => {
    buttons.forEach((b) => b.setAttribute('aria-pressed', b === activeButton ? 'true' : 'false'));
  };

  function sanitizeHtml(html) {
    let raw = String(html || '').trim();
    if (!raw) return '';
    if (/&lt;|&gt;|&amp;lt;|&amp;gt;/.test(raw)) {
      const txt = document.createElement('textarea');
      txt.innerHTML = raw;
      raw = txt.value;
    }
    let doc;
    try { doc = new DOMParser().parseFromString(raw, 'text/html'); } catch (e) { return raw; }
    if (!doc || !doc.body) return raw;
    doc.querySelectorAll('script,style,iframe,object,embed,link,meta,title').forEach(el => el.remove());
    const walker = document.createTreeWalker(doc.body, NodeFilter.SHOW_ELEMENT, null);
    while (walker.nextNode()) {
      const el = walker.currentNode;
      Array.from(el.attributes).forEach((attr) => {
        const name = attr.name.toLowerCase();
        const value = String(attr.value || '');
        if (name.startsWith('on') || name === 'style') { el.removeAttribute(attr.name); return; }
        if ((name === 'href' || name === 'src') && (value.trim().toLowerCase().startsWith('javascript:') || value.trim().toLowerCase().startsWith('data:text/html'))) {
          el.removeAttribute(attr.name);
        }
      });
    }
    return doc.body.innerHTML || raw;
  }

  // ── Mobile layout ─────────────────────────────────────────────────────────
  const isMobileView  = () => window.matchMedia('(max-width: 991.98px)').matches;
  const showListPane  = () => layoutEl && layoutEl.classList.remove('omni-mobile-detail');
  const showDetailPane= () => layoutEl && layoutEl.classList.add('omni-mobile-detail');
  function syncLayout() {
    if (!layoutEl) return;
    if (!isMobileView()) { layoutEl.classList.remove('omni-mobile-detail'); return; }
    if (!activeConvo) layoutEl.classList.remove('omni-mobile-detail');
  }
  window.addEventListener('resize', syncLayout);
  if (backBtn) backBtn.addEventListener('click', showListPane);

  // ── List rendering ────────────────────────────────────────────────────────
  function setLoadingList() {
    if (!listEl) return;
    listEl.innerHTML = '<div class="p-4 text-center text-muted"><span class="spinner-border spinner-border-sm me-2"></span>Loading…</div>';
    if (loadMoreBtn) { loadMoreBtn.disabled = true; loadMoreBtn.textContent = 'Load more'; }
  }

  function formatSender(name, email, fallback) {
    const cleanEmail = normalizeEmail(email);
    const cleanName  = String(name || '').trim();
    if (cleanName && cleanEmail && !cleanName.toLowerCase().includes(cleanEmail)) return cleanName;
    if (cleanName)  return cleanName;
    if (cleanEmail) return cleanEmail;
    return fallback || 'Unlinked';
  }

  function renderList() {
    if (!listEl) return;
    listEl.innerHTML = '';
    const term = (searchEl?.value || '').toLowerCase();
    const filtered = conversations.filter(c => {
      const linked = !!c.contact_id;
      if (linkFilter === 'linked'   && !linked) return false;
      if (linkFilter === 'unlinked' && linked)  return false;
      if (assignFilter === 'assigned'   && (!cfg.agentId || c.assignee_id !== cfg.agentId)) return false;
      if (assignFilter === 'unassigned' && c.assignee_id) return false;
      const rawChannel  = c.channel_provider || c.channel_type || c.channel;
      const channelValue= normalizeChannel(rawChannel);
      if (channelFilter) {
        const cf = normalizeChannel(channelFilter);
        const isSocial = (c.channel_type === 'social') || ['facebook','instagram','telegram','twitter','whatsapp'].includes(channelValue);
        const isChat   = (c.channel_type === 'chat')   || ['tawk'].includes(channelValue);
        if      (cf === 'social' && !isSocial)     return false;
        else if (cf === 'chat'   && !isChat)        return false;
        else if (cf !== 'social' && cf !== 'chat' && channelValue !== cf) return false;
      }
      const hay = [c.subject, c.contact_name, c.channel_label, c.channel_provider, c.channel_type, c.channel, channelValue, c.assignee, c.last_sender_name, c.last_sender_email, normalizeEmail(c.last_sender_email)]
        .map(x => (x || '').toLowerCase()).join(' ');
      return !term || hay.includes(term);
    });

    if (!filtered.length) {
      listEl.innerHTML = '<div class="p-4 text-center text-muted" style="font-size:.85rem;">No conversations found.</div>';
      if (loadMoreBtn) loadMoreBtn.disabled = true;
      return;
    }

    filtered.forEach(c => {
      const senderLine  = formatSender(c.last_sender_name, c.last_sender_email, c.contact_name || 'Unlinked');
      const channelLabel= c.channel_label || c.channel_provider || c.channel || '';
      const channel     = channelMeta(channelLabel);
      const subject     = c.subject || ('Conversation #' + c.id);
      const isUnread    = c.unread && c.unread > 0;
      const isActive    = activeConvo && String(activeConvo.id) === String(c.id);
      const avatarName  = c.contact_name || senderLine || '?';

      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = 'omni-convo-item' + (isActive ? ' active' : '');
      btn.dataset.convoId = c.id;
      btn.innerHTML = `
        <div class="omni-item-avatar-wrap">
          <div class="omni-entity-avatar" style="background:${avatarColor(avatarName)}">${avatarInitials(avatarName)}</div>
          ${isUnread ? '<span class="omni-unread-dot" aria-label="Unread"></span>' : ''}
        </div>
        <div class="omni-item-body">
          <div class="omni-item-top">
            <span class="omni-item-subject${isUnread ? ' unread' : ''}">${escapeHtml(subject)}${isUnread ? ` <span class="badge rounded-pill bg-primary ms-1" style="font-size:.6rem;vertical-align:middle;">${c.unread}</span>` : ''}</span>
            <span class="omni-item-time">${relativeTime(c.last_message_at)}</span>
          </div>
          <div class="omni-item-bottom">
            <span class="omni-item-sender">${escapeHtml(senderLine)}</span>
            <span class="omni-channel-badge ${channel.cls}"><i class="${channel.icon} me-1"></i>${escapeHtml(channel.label)}</span>
          </div>
        </div>
      `;
      btn.addEventListener('click', () => selectConversation(c));
      listEl.appendChild(btn);
    });

    if (loadMoreBtn) {
      loadMoreBtn.disabled = !hasMore;
      loadMoreBtn.classList.toggle('d-none', !!term);
    }
  }

  // ── Conversations load ────────────────────────────────────────────────────
  async function loadConversations({ reset = false } = {}) {
    if (reset) { conversations = []; nextOffset = 0; hasMore = false; setLoadingList(); }
    try {
      if (!didSync) {
        await fetch(cfg.conversations, {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: new URLSearchParams({ action: 'sync_all' })
        });
      }
      const resp = await fetch(cfg.conversations + '?limit=' + pageSize + '&offset=' + nextOffset);
      const data = await resp.json();
      if (!resp.ok) throw new Error(data.error || 'Unable to load conversations');
      const rows = data.data || [];
      conversations = reset ? rows : conversations.concat(rows);
      hasMore = rows.length === pageSize;
      nextOffset += rows.length;
      renderList();
      didSync = true;
      setLastRefresh();
    } catch (e) {
      if (listEl) listEl.innerHTML = '<div class="p-4 text-danger text-center">' + escapeHtml(e.message || 'Unable to load conversations') + '</div>';
      if (loadMoreBtn) loadMoreBtn.disabled = true;
      setLastRefresh('Refresh failed');
    }
  }

  // ── Conversation select ───────────────────────────────────────────────────
  function stopMessageAutoRefresh() {
    if (messageRefreshTimer) { clearInterval(messageRefreshTimer); messageRefreshTimer = null; }
  }

  async function selectConversation(c) {
    activeConvo = c;
    renderList();
    if (isMobileView()) showDetailPane();

    const avatarName = c.contact_name || formatSender(c.last_sender_name, c.last_sender_email, '') || '?';

    if (titleEl)   titleEl.textContent = c.subject || ('Conversation #' + c.id);
    if (metaEl) {
      const channelLabel = c.channel_label || c.channel_provider || c.channel || '';
      metaEl.textContent = [channelLabel, c.contact_name || formatSender(c.last_sender_name, c.last_sender_email, ''), c.state].filter(Boolean).join(' · ');
    }
    if (contactEl) contactEl.textContent = c.contact_name || '';
    if (convoAvatarEl) {
      convoAvatarEl.textContent    = avatarInitials(avatarName);
      convoAvatarEl.style.background = avatarColor(avatarName);
    }
    if (linkBtn)   { c.contact_id ? linkBtn.classList.add('d-none') : linkBtn.classList.remove('d-none'); }
    if (deleteBtn) { canManage ? deleteBtn.classList.remove('d-none') : deleteBtn.classList.add('d-none'); }

    const channelKey = normalizeChannel(c.channel_provider || c.channel_type || c.channel || c.channel_label || '');
    const allowReply = canReply && canReplyChannel(channelKey) && isChannelEnabled(channelKey);
    if (replyEl) {
      replyEl.disabled = !allowReply;
      replyEl.placeholder = allowReply ? 'Type a reply…'
        : (!isChannelEnabled(channelKey) ? 'Channel disabled by admin'
        : (canReply ? 'Replies not configured for this channel' : 'Requires omni.reply permission'));
    }
    if (sendBtn) {
      sendBtn.disabled = !allowReply;
      sendBtn.title = allowReply ? ''
        : (!isChannelEnabled(channelKey) ? 'Channel disabled by admin'
        : (canReply ? 'Replies not configured for this channel' : 'Requires omni.reply permission'));
    }

    stopMessageAutoRefresh();
    await fetchActiveMessages(true);
    await markConversationRead(c);
    const refreshMs = window.appConfig?.omniInboxRefreshMs ?? 120000;
    messageRefreshTimer = setInterval(() => { if (!document.hidden) fetchActiveMessages(false); }, refreshMs);
  }

  function clearActiveConversation() {
    stopMessageAutoRefresh();
    activeConvo = null;
    if (titleEl)   titleEl.textContent = 'Select a conversation';
    if (metaEl)    metaEl.textContent  = 'Channel · Contact · Status';
    if (contactEl) contactEl.textContent = '';
    if (convoAvatarEl) { convoAvatarEl.textContent = '?'; convoAvatarEl.style.background = '#94a3b8'; }
    if (messagesEl) messagesEl.innerHTML = '<div class="omni-empty-state"><i class="ri-chat-3-line"></i><p>Choose a conversation to view messages.</p></div>';
    if (replyEl) { replyEl.value = ''; replyEl.disabled = true; replyEl.placeholder = 'Select a conversation…'; replyEl.style.height = ''; }
    if (sendBtn) { sendBtn.disabled = true; }
    if (linkBtn)   linkBtn.classList.add('d-none');
    if (deleteBtn) deleteBtn.classList.add('d-none');
  }

  // ── Messages ──────────────────────────────────────────────────────────────
  async function fetchActiveMessages(showLoading = false) {
    if (!activeConvo || !messagesEl) return;
    if (showLoading) {
      messagesEl.innerHTML = '<div class="p-4 text-center text-muted"><span class="spinner-border spinner-border-sm me-2"></span>Loading…</div>';
    }
    try {
      const resp = await fetch(cfg.messages + '?conversation_id=' + encodeURIComponent(activeConvo.id));
      const data = await resp.json();
      if (!resp.ok) throw new Error(data.error || 'Unable to load messages');
      renderMessages(data.data || []);
    } catch (e) {
      messagesEl.innerHTML = '<div class="text-danger text-center mt-3 p-3">' + escapeHtml(e.message || 'Unable to load messages') + '</div>';
    }
  }

  async function markConversationRead(c) {
    if (!c || !(Number(c.unread) > 0)) return;
    try {
      const resp = await fetch(cfg.messages + '?conversation_id=' + encodeURIComponent(c.id), {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ action: 'mark_read' })
      });
      const raw = await resp.text();
      let data = {};
      try { data = JSON.parse(raw || '{}'); } catch (e) {}
      if (!resp.ok) throw new Error((data && data.error) || ('Unable to mark conversation as read (' + resp.status + ')'));
      c.unread = 0;
      if (activeConvo && String(activeConvo.id) === String(c.id)) {
        activeConvo.unread = 0;
      }
      const idx = conversations.findIndex((row) => String(row.id) === String(c.id));
      if (idx >= 0) conversations[idx].unread = 0;
      renderList();
    } catch (e) {
      console.warn('Unable to mark conversation as read', e);
    }
  }

  function renderMessages(msgs) {
    if (!messagesEl) return;
    messagesEl.innerHTML = '';
    if (!msgs.length) {
      messagesEl.innerHTML = '<div class="omni-empty-state"><i class="ri-message-3-line"></i><p>No messages yet.</p></div>';
      return;
    }

    const normalizeNewlines = (v) => v.replace(/\r\n/g, '\n').replace(/\n{3,}/g, '\n\n');

    let lastDateLabel = null;

    msgs.forEach(m => {
      // Date separator
      const dl = dateLabel(m.created_at);
      if (dl && dl !== lastDateLabel) {
        lastDateLabel = dl;
        const sep = document.createElement('div');
        sep.className = 'omni-date-sep';
        sep.innerHTML = `<span>${escapeHtml(dl)}</span>`;
        messagesEl.appendChild(sep);
      }

      const isOut = m.direction === 'outbound';
      const group = document.createElement('div');
      group.className = 'omni-msg-group ' + (isOut ? 'outbound' : 'inbound');

      // Metadata line
      const meta = document.createElement('div');
      meta.className = 'omni-msg-meta';
      const autoTag = m.auto_reply ? ' <span class="badge bg-secondary-subtle text-secondary ms-1" style="font-size:.62rem;">Auto</span>' : '';
      meta.innerHTML = escapeHtml(m.sender || '') + (m.sender ? ' · ' : '') + escapeHtml(m.created_at || '') + autoTag;
      group.appendChild(meta);

      // Bubble
      const rawBody = String(m.body || '');
      let bodyHtml = '';
      if (m.body_html) {
        bodyHtml = sanitizeHtml(m.body_html);
      } else if ((rawBody.includes('<') && rawBody.includes('>')) || /&lt;|&gt;|&amp;lt;|&amp;gt;/.test(rawBody)) {
        bodyHtml = sanitizeHtml(rawBody);
      } else {
        bodyHtml = linkifyText(normalizeNewlines(rawBody));
      }
      if (/<br\s*\/?>/i.test(bodyHtml)) bodyHtml = bodyHtml.replace(/\r?\n/g, '');
      bodyHtml = bodyHtml.replace(/(<br\s*\/?\s*>\s*){3,}/gi, '<br><br>');

      const bubble = document.createElement('div');
      bubble.className = 'omni-msg-bubble';
      bubble.innerHTML = bodyHtml;
      group.appendChild(bubble);

      messagesEl.appendChild(group);
    });

    messagesEl.scrollTop = messagesEl.scrollHeight;
  }

  // ── Send ──────────────────────────────────────────────────────────────────
  async function sendReply() {
    if (!canReply || !activeConvo) return;
    const channelKey = normalizeChannel(activeConvo.channel_provider || activeConvo.channel_type || activeConvo.channel || '');
    if (!isChannelEnabled(channelKey)) { window.crmUiAlert('Channel disabled by admin'); return; }
    if (!canReplyChannel(channelKey)) return;
    const text = replyEl?.value?.trim() || '';
    if (!text) return;

    const spinner = sendBtn?.querySelector('.spinner-border');
    const label   = sendBtn?.querySelector('.btn-text');
    if (sendBtn) sendBtn.disabled = true;
    if (spinner) spinner.classList.remove('d-none');
    if (label)   label.style.display = 'none';
    try {
      const resp = await fetch(cfg.messages + '?conversation_id=' + encodeURIComponent(activeConvo.id), {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ body: text })
      });
      const raw = await resp.text();
      let data = {};
      try { data = JSON.parse(raw || '{}'); } catch (e) {}
      if (!resp.ok) throw new Error((data && data.error) || ('Send failed (' + resp.status + ')'));
      if (replyEl) { replyEl.value = ''; replyEl.style.height = ''; }
      await selectConversation(activeConvo);
    } catch (e) {
      window.crmUiAlert(e.message || 'Unable to send');
    } finally {
      if (sendBtn) sendBtn.disabled = false;
      if (spinner) spinner.classList.add('d-none');
      if (label)   label.style.display = '';
    }
  }

  // ── Delete ────────────────────────────────────────────────────────────────
  async function deleteConversation() {
    if (!canManage || !activeConvo) return;
    const ok = await window.crmUiConfirm('Delete this conversation and all its messages? This cannot be undone.', 'Delete Conversation', { okText: 'Delete', cancelText: 'Cancel', variant: 'danger', icon: 'warning' });
    if (!ok) return;
    if (deleteBtn) deleteBtn.disabled = true;
    try {
      const resp = await fetch(cfg.conversations, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ action: 'delete_conversation', conversation_id: activeConvo.id })
      });
      const data = await resp.json();
      if (!resp.ok) throw new Error(data.error || 'Unable to delete conversation');
      conversations = conversations.filter(r => String(r.id) !== String(activeConvo.id));
      clearActiveConversation();
      renderList();
    } catch (e) {
      window.crmUiAlert(e.message || 'Unable to delete conversation');
    } finally {
      if (deleteBtn) deleteBtn.disabled = false;
    }
  }

  // ── Textarea: auto-grow + enter-to-send ──────────────────────────────────
  if (replyEl) {
    replyEl.addEventListener('input', () => {
      replyEl.style.height = 'auto';
      replyEl.style.height = Math.min(replyEl.scrollHeight, 120) + 'px';
    });
    replyEl.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendReply();
      }
    });
  }

  // ── Filters ───────────────────────────────────────────────────────────────
  linkFilters.forEach(btn => {
    btn.addEventListener('click', () => {
      linkFilters.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      setPressedState(linkFilters, btn);
      linkFilter = btn.dataset.filter || 'all';
      clearActiveConversation();
      renderList();
    });
  });

  assignFilters.forEach(btn => {
    btn.addEventListener('click', () => {
      assignFilters.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      setPressedState(assignFilters, btn);
      assignFilter = btn.dataset.filter || 'all';
      clearActiveConversation();
      renderList();
    });
  });

  channelFilters.forEach(btn => {
    const btnChannel = (btn.dataset.channel || '').toLowerCase();
    if (btnChannel && !isChannelEnabled(btnChannel)) btn.setAttribute('disabled', 'disabled');
    btn.addEventListener('click', () => {
      if (btnChannel && !isChannelEnabled(btnChannel)) return;
      channelFilters.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      channelFilter = btnChannel;
      clearActiveConversation();
      renderList();
    });
  });

  if (searchEl) searchEl.addEventListener('input', () => { clearTimeout(searchTimer); searchTimer = setTimeout(renderList, 120); });
  if (refreshBtn) refreshBtn.addEventListener('click', () => { didSync = false; loadConversations({ reset: true }); });

  // ── Load more ─────────────────────────────────────────────────────────────
  if (loadMoreBtn) {
    loadMoreBtn.addEventListener('click', () => {
      loadMoreBtn.disabled = true;
      loadMoreBtn.textContent = 'Loading…';
      loadConversations({ reset: false }).finally(() => { loadMoreBtn.textContent = 'Load more'; });
    });
  }

  // ── Send button ───────────────────────────────────────────────────────────
  if (sendBtn) sendBtn.addEventListener('click', sendReply);
  if (deleteBtn) deleteBtn.addEventListener('click', deleteConversation);

  // ── Link contact modal ────────────────────────────────────────────────────
  function showLinkError(msg) {
    if (!linkError) return;
    if (!msg) { linkError.classList.add('d-none'); linkError.textContent = ''; return; }
    linkError.textContent = msg;
    linkError.classList.remove('d-none');
  }

  function renderLinkResults(items) {
    if (!linkResults) return;
    linkResults.innerHTML = '';
    if (!items.length) { linkResults.innerHTML = '<div class="list-group-item text-center text-muted">No results</div>'; return; }
    items.forEach(row => {
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = 'list-group-item list-group-item-action';
      const name = escapeHtml(((row.first_name || '') + ' ' + (row.last_name || '')).trim() || row.email || row.phone || 'Contact');
      const meta = [row.email, row.phone].filter(Boolean).join(' • ');
      btn.innerHTML = `<div class="fw-semibold">${name}</div><div class="small text-muted">${escapeHtml(meta)}</div>`;
      btn.addEventListener('click', () => { if (linkInput) linkInput.value = row.id_s || row.id; if (linkSearch) linkSearch.value = name; showLinkError(''); });
      linkResults.appendChild(btn);
    });
  }

  async function searchContacts(term) {
    if (!cfg.contactsSearch) return;
    if (!term || term.length < 4) {
      if (linkResults) linkResults.innerHTML = '<div class="list-group-item text-center text-muted">Type at least 4 characters</div>';
      return;
    }
    try {
      const resp = await fetch(cfg.contactsSearch + '?q=' + encodeURIComponent(term) + '&limit=5');
      const text = await resp.text();
      let data;
      try { data = JSON.parse(text); } catch (e) { throw new Error(text.slice(0, 120) || 'Search failed'); }
      if (!resp.ok) throw new Error((data && data.error) || 'Search failed');
      renderLinkResults(Array.isArray(data) ? data : (data.data || []));
    } catch (e) { showLinkError(e.message || 'Search failed'); }
  }

  async function linkConversation() {
    if (!activeConvo || !linkInput) return;
    const val = linkInput.value.trim();
    if (!val) { showLinkError('Contact is required'); return; }
    showLinkError('');
    const spinner = linkSave?.querySelector('.spinner-border');
    const label   = linkSave?.querySelector('.btn-text');
    if (linkSave) linkSave.disabled = true;
    if (spinner) spinner.classList.remove('d-none');
    if (label)   label.textContent = '';
    try {
      const resp = await fetch(cfg.conversations, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({ conversation_id: activeConvo.id, contact_id: val })
      });
      const data = await resp.json();
      if (!resp.ok) throw new Error(data.error || 'Unable to link contact');
      activeConvo.contact_id   = val;
      activeConvo.contact_name = data.contact_name || activeConvo.contact_name || 'Linked contact';
      if (contactEl)     contactEl.textContent = activeConvo.contact_name;
      if (convoAvatarEl) { convoAvatarEl.textContent = avatarInitials(activeConvo.contact_name); convoAvatarEl.style.background = avatarColor(activeConvo.contact_name); }
      if (linkBtn)       linkBtn.classList.add('d-none');
      if (linkModal)     linkModal.hide();
    } catch (e) {
      showLinkError(e.message || 'Unable to link contact');
    } finally {
      if (linkSave) linkSave.disabled = false;
      if (spinner)  spinner.classList.add('d-none');
      if (label)    label.textContent = 'Link';
    }
  }

  if (linkBtn && linkModal) {
    linkBtn.addEventListener('click', () => {
      if (linkInput)  linkInput.value  = '';
      if (linkSearch) linkSearch.value = '';
      showLinkError('');
      renderLinkResults([]);
      linkModal.show();
      setTimeout(() => linkSearch?.focus(), 150);
    });
  }
  if (linkSave)   linkSave.addEventListener('click', linkConversation);
  if (linkSearch) {
    linkSearch.addEventListener('input', () => {
      clearTimeout(searchTimer);
      searchTimer = setTimeout(() => searchContacts(linkSearch.value.trim()), 200);
    });
  }

  // ── Boot ──────────────────────────────────────────────────────────────────
  syncLayout();
  loadConversations({ reset: true });
})();
