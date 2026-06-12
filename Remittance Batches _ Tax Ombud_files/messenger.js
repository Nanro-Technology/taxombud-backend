/**
 * Floating Messenger — Facebook-style chat widget
 * Polls api/modules/agent-chats/poll for unread + new messages,
 * lets the user open multiple conversation windows side by side.
 */
(function () {
  'use strict';

  var root = document.getElementById('fmsgRoot');
  if (!root) return;

  var CSRF = root.dataset.csrf || '';
  var ME_AGENT_ID = parseInt(root.dataset.agentId, 10) || 0;
  var ME_USER_ID = parseInt(root.dataset.userId, 10) || 0;
  var CACHE_VERSION = String(root.dataset.cacheVersion || '1');
  var PUSHER_ENABLED = String(root.dataset.pusherEnabled || '') === '1';
  var PUSHER_KEY = String(root.dataset.pusherKey || '');
  var PUSHER_CLUSTER = String(root.dataset.pusherCluster || 'eu');

  var POLL_ACTIVE_MS = window.appConfig?.chatRefreshMs || 60000;  // active fallback when realtime is offline
  var POLL_HIDDEN_MS = window.appConfig?.chatRefreshHiddenMs || 300000; // hidden tab fallback
  var POLL_BURST_MS  = 800;   // quick follow-up poll after new activity
  var BOOT_CACHE_TTL_MS = window.appConfig?.chatBootCacheMs || 45000;
  var SETTINGS_CACHE_TTL_MS = window.appConfig?.chatSettingsCacheMs || 300000;
  var TYPE_DEBOUNCE_MS = 250;
  var CACHE_PREFIX = 'fmsg:' + CACHE_VERSION + ':agent:' + ME_AGENT_ID + ':';

  var state = {
    panelOpen: false,
    pickerOpen: false,
    threads: [],                // [{id, title, peer_avatar, last_preview, last_message_at, unread_count}]
    openWindows: new Map(),     // threadId -> { el, messages:[], peer:{display_name, avatar_url}, lastMsgIso, scrollPinned:true }
    lastSince: '',
    pollTimer: null,
    pickerTimer: null,
    pollErrorCount: 0,
    initialPollComplete: false,
  };

  // ── DOM refs ───────────────────────────────────────────────────────────────
  var bubble = document.getElementById('fmsgBubble');
  var bubbleBadge = document.getElementById('fmsgBubbleBadge');
  var panel = document.getElementById('fmsgPanel');
  var threadList = document.getElementById('fmsgThreadList');
  var listEmpty = document.getElementById('fmsgListEmpty');
  var newBtn = document.getElementById('fmsgNewBtn');
  var closePanelBtn = document.getElementById('fmsgClosePanel');
  var picker = document.getElementById('fmsgPicker');
  var pickerInput = document.getElementById('fmsgPickerInput');
  var pickerList = document.getElementById('fmsgPickerList');
  var windowsHost = document.getElementById('fmsgWindows');
  var settingsBtn = document.getElementById('fmsgSettingsBtn');
  var settingsMenu = document.getElementById('fmsgSettingsMenu');

  // ── Utilities ──────────────────────────────────────────────────────────────
  function esc(s) {
    return String(s == null ? '' : s)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }

  function timeAgo(iso) {
    if (!iso) return '';
    var d = new Date((iso || '').replace(' ', 'T') + 'Z');
    if (isNaN(d.getTime())) return '';
    var diffSec = (Date.now() - d.getTime()) / 1000;
    if (diffSec < 60) return 'now';
    if (diffSec < 3600) return Math.floor(diffSec / 60) + 'm';
    if (diffSec < 86400) return Math.floor(diffSec / 3600) + 'h';
    if (diffSec < 604800) return Math.floor(diffSec / 86400) + 'd';
    return d.toLocaleDateString();
  }

  function timeStamp(iso) {
    if (!iso) return '';
    var d = new Date((iso || '').replace(' ', 'T') + 'Z');
    if (isNaN(d.getTime())) return '';
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  function cacheRead(key, ttlMs) {
    try {
      var raw = localStorage.getItem(CACHE_PREFIX + key);
      if (!raw) return null;
      var packed = JSON.parse(raw);
      if (!packed || !packed.saved_at || !packed.data) return null;
      if (ttlMs && (Date.now() - parseInt(packed.saved_at, 10)) > ttlMs) return null;
      return packed.data;
    } catch (e) {
      return null;
    }
  }

  function cacheWrite(key, data) {
    try {
      localStorage.setItem(CACHE_PREFIX + key, JSON.stringify({
        saved_at: Date.now(),
        data: data,
      }));
    } catch (e) {}
  }

  function applyPollSnapshot(d) {
    if (!d || d.disabled) return;
    var n = parseInt(d.unread_total, 10) || 0;
    bubbleBadge.textContent = n > 99 ? '99+' : n;
    bubble.classList.toggle('has-unread', n > 0);
    state.lastUnreadTotal = n;
    if (Array.isArray(d.threads)) {
      state.threads = d.threads;
      if (state.panelOpen && !state.pickerOpen) renderThreads();
    }
    if (d.server_time) state.lastSince = d.server_time;
  }

  function updatePollCacheFromState(extra) {
    var n = parseInt(bubbleBadge.textContent.replace('+', ''), 10) || 0;
    cacheWrite('poll', Object.assign({
      ok: true,
      unread_total: n,
      threads: state.threads || [],
      new_messages: {},
      server_time: state.lastSince || new Date().toISOString().replace('T', ' ').slice(0, 19),
    }, extra || {}));
  }

  function bumpUnreadBadge(delta) {
    var current = parseInt(bubbleBadge.textContent.replace('+', ''), 10) || 0;
    var next = Math.max(0, current + delta);
    bubbleBadge.textContent = next > 99 ? '99+' : next;
    bubble.classList.toggle('has-unread', next > 0);
    state.lastUnreadTotal = next;
    updatePollCacheFromState({ unread_total: next });
  }

  function applyRealtimeMessageToThreads(msg, markUnread) {
    var tid = parseInt(msg.thread_id, 10);
    var thread = state.threads.find(function (t) { return parseInt(t.id, 10) === tid; });
    if (!thread) return false;
    thread.last_preview = msg.message || thread.last_preview || '';
    thread.last_message_at = msg.created_at || thread.last_message_at;
    thread.last_sender_id = parseInt(msg.sender_agent_id, 10) || thread.last_sender_id;
    if (markUnread) thread.unread_count = (parseInt(thread.unread_count, 10) || 0) + 1;
    state.threads.sort(function (a, b) {
      return String(b.last_message_at || '').localeCompare(String(a.last_message_at || ''));
    });
    if (state.panelOpen && !state.pickerOpen) renderThreads();
    updatePollCacheFromState();
    return true;
  }

  // ── Audio (synthesized via WebAudio — no asset files needed) ──────────────
  var _audioCtx = null;
  function audioCtx() {
    if (_audioCtx) return _audioCtx;
    var Ctx = window.AudioContext || window.webkitAudioContext;
    if (!Ctx) return null;
    try { _audioCtx = new Ctx(); } catch (e) { return null; }
    return _audioCtx;
  }
  function playTone(freq, duration, volume, type) {
    var ctx = audioCtx();
    if (!ctx) return;
    if (ctx.state === 'suspended') { try { ctx.resume(); } catch (e) {} }
    var osc = ctx.createOscillator();
    var gain = ctx.createGain();
    osc.type = type || 'sine';
    osc.frequency.value = freq;
    gain.gain.setValueAtTime(0, ctx.currentTime);
    gain.gain.linearRampToValueAtTime(volume, ctx.currentTime + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + duration);
    osc.connect(gain).connect(ctx.destination);
    osc.start();
    osc.stop(ctx.currentTime + duration);
  }
  function playPing() {
    // Soft two-note "ding" — gentle attention
    playTone(880, 0.12, 0.08, 'sine');
    setTimeout(function () { playTone(1175, 0.16, 0.08, 'sine'); }, 90);
  }
  function playNudge() {
    // Loud, longer attention sequence — three escalating notes
    playTone(660, 0.18, 0.18, 'square');
    setTimeout(function () { playTone(880, 0.18, 0.18, 'square'); }, 150);
    setTimeout(function () { playTone(1175, 0.28, 0.20, 'square'); }, 300);
  }
  // Buzz: continuous ringing pattern — repeats until user interacts or timeout
  var _buzzTimer = null;
  function playBuzz() {
    stopBuzz();
    var i = 0;
    function ring() {
      // 3-note urgent pattern, then short pause, repeat
      playTone(880, 0.14, 0.30, 'square');
      setTimeout(function () { playTone(660, 0.14, 0.30, 'square'); }, 160);
      setTimeout(function () { playTone(880, 0.18, 0.30, 'square'); }, 320);
    }
    ring();
    _buzzTimer = setInterval(function () {
      if (++i >= 6) { stopBuzz(); return; } // ~12 seconds total
      ring();
    }, 2000);
  }
  function stopBuzz() {
    if (_buzzTimer) { clearInterval(_buzzTimer); _buzzTimer = null; }
  }

  // ── Browser notifications (when tab hidden) ────────────────────────────────
  var _notifPermissionAsked = false;
  function requestNotificationPermission() {
    if (_notifPermissionAsked) return;
    _notifPermissionAsked = true;
    if (!('Notification' in window)) return;
    if (Notification.permission === 'default') {
      try { Notification.requestPermission(); } catch (e) {}
    }
  }
  function showBrowserNotification(title, body, threadId) {
    if (!('Notification' in window) || Notification.permission !== 'granted') return;
    if (!document.hidden) return; // only when tab not focused
    if (state.mySettings && !state.mySettings.notify_browser) return;
    try {
      var n = new Notification(title, { body: body, tag: 'fmsg-thread-' + threadId, icon: 'assets/images/users/user-dummy-img.jpg' });
      n.onclick = function () {
        window.focus();
        n.close();
        // Open the conversation if not open
        var t = state.threads.find(function (x) { return parseInt(x.id, 10) === parseInt(threadId, 10); });
        if (t) openWindow(t.id, { display_name: t.title, avatar_url: t.peer_avatar });
        togglePanel(true);
      };
      setTimeout(function () { try { n.close(); } catch (e) {} }, 6000);
    } catch (e) {}
  }

  // ── My chat settings (DND/Away/Sound/Notify) ───────────────────────────────
  function loadMySettings() {
    var cached = cacheRead('settings', SETTINGS_CACHE_TTL_MS);
    if (cached) {
      state.mySettings = cached;
      reflectMyStatus();
      reflectSettingsMenu();
      return;
    }
    api('api/modules/agent-chats/settings').then(function (d) {
      state.mySettings = d.data || { sound_enabled: 1, notify_browser: 1, do_not_disturb: 0, away: 0 };
      cacheWrite('settings', state.mySettings);
      reflectMyStatus();
      reflectSettingsMenu();
    }).catch(function () {});
  }
  function saveMySettings(patch) {
    return api('api/modules/agent-chats/settings', {
      method: 'PATCH',
      body: JSON.stringify(patch),
    }).then(function (d) {
      state.mySettings = d.data;
      cacheWrite('settings', state.mySettings);
      reflectMyStatus();
      reflectSettingsMenu();
      return d.data;
    });
  }
  function reflectSettingsMenu() {
    if (!settingsMenu || !state.mySettings) return;
    ['do_not_disturb', 'away', 'sound_enabled', 'notify_browser'].forEach(function (key) {
      var toggle = settingsMenu.querySelector('[data-toggle="' + key + '"]');
      var row = settingsMenu.querySelector('[data-key="' + key + '"]');
      var active = !!parseInt(state.mySettings[key], 10);
      if (toggle) {
        toggle.classList.toggle('on', active);
        toggle.setAttribute('aria-checked', active ? 'true' : 'false');
        toggle.setAttribute('role', 'switch');
        toggle.setAttribute('tabindex', '0');
      }
      if (row) row.classList.toggle('active', active);
    });
  }
  function closeSettingsMenu() {
    if (settingsMenu) settingsMenu.classList.remove('open');
  }
  function toggleSetting(key) {
    if (!settingsMenu || !key) return;
    var allowed = ['do_not_disturb', 'away', 'sound_enabled', 'notify_browser'];
    if (allowed.indexOf(key) === -1) return;
    var current = state.mySettings || { sound_enabled: 1, notify_browser: 1, do_not_disturb: 0, away: 0 };
    var next = parseInt(current[key], 10) ? 0 : 1;
    if (key === 'notify_browser' && next === 1) requestNotificationPermission();
    var previous = state.mySettings;
    state.mySettings = Object.assign({}, current, (function () {
      var patch = {};
      patch[key] = next;
      return patch;
    })());
    reflectMyStatus();
    reflectSettingsMenu();
    saveMySettings((function () {
      var patch = {};
      patch[key] = next;
      return patch;
    })()).catch(function () {
      state.mySettings = previous || current;
      reflectMyStatus();
      reflectSettingsMenu();
    });
  }
  function reflectMyStatus() {
    if (!state.mySettings) return;
    var dnd = state.mySettings.do_not_disturb;
    var away = state.mySettings.away;
    bubble.classList.toggle('fmsg-dnd', !!dnd);
    bubble.classList.toggle('fmsg-away', !!away);
    bubble.title = dnd ? 'Messenger (Do Not Disturb)' : (away ? 'Messenger (Away)' : 'Messenger');
  }
  function shouldPlaySound() {
    if (!state.mySettings) return true;
    if (state.mySettings.do_not_disturb) return false;
    if (!state.mySettings.sound_enabled) return false;
    return true;
  }

  // Unlock AudioContext on first user interaction (required by browsers)
  function unlockAudio() {
    var ctx = audioCtx();
    if (ctx && ctx.state === 'suspended') ctx.resume();
    document.removeEventListener('click', unlockAudio);
    document.removeEventListener('keydown', unlockAudio);
  }
  document.addEventListener('click', unlockAudio, { once: false });
  document.addEventListener('keydown', unlockAudio, { once: false });

  function api(path, opts) {
    opts = opts || {};
    var headers = opts.headers || {};
    if (opts.body && !headers['Content-Type']) headers['Content-Type'] = 'application/json';
    if (CSRF) headers['X-CSRF-Token'] = CSRF;
    return fetch(path, {
      method: opts.method || 'GET',
      credentials: 'same-origin',
      headers: headers,
      body: opts.body || undefined,
    }).then(function (r) {
      return r.json().then(function (d) {
        if (!r.ok) throw new Error(d && (d.error || d.message) || ('HTTP ' + r.status));
        return d;
      });
    });
  }

  // ── Panel toggle ───────────────────────────────────────────────────────────
  function togglePanel(open) {
    state.panelOpen = (open === undefined) ? !state.panelOpen : !!open;
    panel.classList.toggle('open', state.panelOpen);
    if (state.panelOpen) {
      requestNotificationPermission();
      state.pickerOpen = false;
      picker.style.display = 'none';
      threadList.style.display = '';
      // Fresh poll immediately + refresh any open (non-draft) conversation windows
      poll({ force: true, silent: true });
      state.openWindows.forEach(function (w, tid) {
        if (w.isDraft) return;
        w.scrollPinned = true;
        loadInitialMessages(tid);
      });
    } else {
      state.pickerOpen = false;
      picker.style.display = 'none';
      threadList.style.display = '';
      if (settingsMenu) settingsMenu.classList.remove('open');
    }
  }

  // ── Drag-to-move (persists in localStorage) ────────────────────────────────
  var POS_KEY = 'fmsgRootPos.v1';
  var DRAG_THRESHOLD = 5;

  function applyPosition(rightPx, bottomPx) {
    // Clamp to viewport so it never lands off-screen
    var bubbleSize = bubble.offsetWidth || 56;
    var maxRight  = Math.max(0, window.innerWidth  - bubbleSize);
    var maxBottom = Math.max(0, window.innerHeight - bubbleSize);
    var r = Math.max(0, Math.min(rightPx,  maxRight));
    var b = Math.max(0, Math.min(bottomPx, maxBottom));
    root.style.right  = r + 'px';
    root.style.bottom = b + 'px';
    // Keep conversation windows aligned to the bubble
    if (windowsHost) {
      windowsHost.style.right  = (r + bubbleSize + 12) + 'px';
      windowsHost.style.bottom = b + 'px';
    }
  }

  function loadSavedPosition() {
    try {
      var raw = localStorage.getItem(POS_KEY);
      if (!raw) return;
      var p = JSON.parse(raw);
      if (typeof p.right === 'number' && typeof p.bottom === 'number') {
        applyPosition(p.right, p.bottom);
      }
    } catch (e) {}
  }

  function savePosition() {
    try {
      var r = parseInt(root.style.right,  10) || 22;
      var b = parseInt(root.style.bottom, 10) || 22;
      localStorage.setItem(POS_KEY, JSON.stringify({ right: r, bottom: b }));
    } catch (e) {}
  }

  loadSavedPosition();

  // Re-clamp on viewport resize so the bubble never drifts off-screen
  window.addEventListener('resize', function () {
    var r = parseInt(root.style.right,  10);
    var b = parseInt(root.style.bottom, 10);
    if (!isNaN(r) && !isNaN(b)) applyPosition(r, b);
  });

  var dragState = null; // { startX, startY, startRight, startBottom, moved }
  function dragStart(clientX, clientY) {
    var bubbleSize = bubble.offsetWidth || 56;
    var startRight  = window.innerWidth  - clientX - bubbleSize / 2;
    var startBottom = window.innerHeight - clientY - bubbleSize / 2;
    dragState = {
      startX: clientX, startY: clientY,
      startRight: startRight, startBottom: startBottom,
      moved: false,
    };
  }
  function dragMove(clientX, clientY) {
    if (!dragState) return;
    var dx = clientX - dragState.startX;
    var dy = clientY - dragState.startY;
    if (!dragState.moved && Math.hypot(dx, dy) < DRAG_THRESHOLD) return;
    dragState.moved = true;
    bubble.classList.add('dragging');
    var bubbleSize = bubble.offsetWidth || 56;
    applyPosition(
      window.innerWidth  - clientX - bubbleSize / 2,
      window.innerHeight - clientY - bubbleSize / 2
    );
  }
  function dragEnd() {
    if (!dragState) return;
    var moved = dragState.moved;
    dragState = null;
    bubble.classList.remove('dragging');
    if (moved) {
      savePosition();
      // Suppress the click that would otherwise fire after drag
      bubble.dataset.suppressClick = '1';
      setTimeout(function () { delete bubble.dataset.suppressClick; }, 50);
    }
  }

  bubble.addEventListener('mousedown', function (e) {
    if (e.button !== 0) return;
    dragStart(e.clientX, e.clientY);
  });
  document.addEventListener('mousemove', function (e) {
    if (dragState) { e.preventDefault(); dragMove(e.clientX, e.clientY); }
  });
  document.addEventListener('mouseup', dragEnd);

  bubble.addEventListener('touchstart', function (e) {
    var t = e.touches[0]; if (!t) return;
    dragStart(t.clientX, t.clientY);
  }, { passive: true });
  document.addEventListener('touchmove', function (e) {
    if (!dragState) return;
    var t = e.touches[0]; if (!t) return;
    e.preventDefault();
    dragMove(t.clientX, t.clientY);
  }, { passive: false });
  document.addEventListener('touchend', dragEnd);

  bubble.addEventListener('click', function (e) {
    e.preventDefault();
    e.stopPropagation();
    if (bubble.dataset.suppressClick) return; // ignore the click that follows a drag
    togglePanel();
  });
  closePanelBtn.addEventListener('click', function (e) {
    e.preventDefault();
    e.stopPropagation();
    togglePanel(false);
  });

  if (settingsBtn && settingsMenu) {
    settingsBtn.addEventListener('click', function (e) {
      e.stopPropagation();
      settingsMenu.classList.toggle('open');
      reflectSettingsMenu();
    });

    settingsMenu.addEventListener('click', function (e) {
      e.stopPropagation();
      var row = e.target.closest('[data-key]');
      if (!row) return;
      toggleSetting(row.dataset.key);
      closeSettingsMenu();
    });

    settingsMenu.addEventListener('keydown', function (e) {
      if (e.key !== 'Enter' && e.key !== ' ') return;
      var row = e.target.closest('[data-key]');
      var toggle = e.target.closest('[data-toggle]');
      var key = row ? row.dataset.key : (toggle ? toggle.dataset.toggle : '');
      if (!key) return;
      e.preventDefault();
      toggleSetting(key);
      closeSettingsMenu();
    });

    document.addEventListener('click', function (e) {
      if (!settingsMenu.classList.contains('open')) return;
      if (settingsMenu.contains(e.target) || settingsBtn.contains(e.target)) return;
      closeSettingsMenu();
    });
  }

  newBtn.addEventListener('click', function () {
    closeSettingsMenu();
    state.pickerOpen = !state.pickerOpen;
    picker.style.display = state.pickerOpen ? 'flex' : 'none';
    threadList.style.display = state.pickerOpen ? 'none' : '';
    if (state.pickerOpen) {
      pickerInput.value = '';
      pickerInput.focus();
      pickerList.innerHTML = '<div class="fmsg-empty">Type to search agents</div>';
    }
  });

  pickerInput.addEventListener('input', function () {
    var q = pickerInput.value.trim();
    clearTimeout(state.pickerTimer);
    state.pickerTimer = setTimeout(function () { searchAgents(q); }, TYPE_DEBOUNCE_MS);
  });

  function searchAgents(q) {
    if (!q) {
      pickerList.innerHTML = '<div class="fmsg-empty">Type to search agents</div>';
      return;
    }
    pickerList.innerHTML = '<div class="fmsg-empty">Searching…</div>';
    api('api/modules/agents/index?limit=30&q=' + encodeURIComponent(q))
      .then(function (d) { renderPicker(d.data || []); })
      .catch(function () { pickerList.innerHTML = '<div class="fmsg-empty">Search failed.</div>'; });
  }

  function renderPicker(agents) {
    agents = agents.filter(function (a) { return parseInt(a.id, 10) !== ME_AGENT_ID; });
    if (!agents.length) {
      pickerList.innerHTML = '<div class="fmsg-empty">No matching agents.</div>';
      return;
    }
    pickerList.innerHTML = '';
    // Look up presence for these agents in one call
    var ids = agents.map(function (a) { return a.id; });
    var presenceQs = ids.map(function (i) { return 'agent_ids[]=' + i; }).join('&');
    api('api/modules/presence/ping?' + presenceQs).then(function (pres) {
      var map = (pres && pres.agents) || {};
      agents.forEach(function (a) {
        var row = document.createElement('div');
        row.className = 'fmsg-picker-row';
        var avatarBg = a.avatar_url ? 'background-image:url(' + esc(a.avatar_url) + ');' : '';
        var status = map[a.id] || 'offline';
        row.innerHTML =
          '<div class="fmsg-avatar" style="' + avatarBg + '"><span class="fmsg-status-dot ' + status + '"></span></div>' +
          '<div class="fmsg-thread-info">' +
            '<div class="fmsg-thread-name">' + esc(a.display_name || 'Agent') + '</div>' +
            '<div class="fmsg-thread-preview">' + esc(status === 'online' ? 'Active now' : (status === 'away' ? 'Away' : (a.email || ''))) + '</div>' +
          '</div>';
        row.addEventListener('click', function () { startConversation(a); });
        pickerList.appendChild(row);
      });
    }).catch(function () {
      // Fallback: render without presence
      agents.forEach(function (a) {
        var row = document.createElement('div');
        row.className = 'fmsg-picker-row';
        var avatarBg = a.avatar_url ? 'background-image:url(' + esc(a.avatar_url) + ');' : '';
        row.innerHTML =
          '<div class="fmsg-avatar" style="' + avatarBg + '"></div>' +
          '<div class="fmsg-thread-info">' +
            '<div class="fmsg-thread-name">' + esc(a.display_name || 'Agent') + '</div>' +
            '<div class="fmsg-thread-preview">' + esc(a.email || '') + '</div>' +
          '</div>';
        row.addEventListener('click', function () { startConversation(a); });
        pickerList.appendChild(row);
      });
    });
  }

  function startConversation(agent) {
    state.pickerOpen = false;
    picker.style.display = 'none';
    threadList.style.display = '';
    togglePanel(false);
    // Open a DRAFT window — no DB row created until first message is sent.
    // Use a synthetic key 'draft-<agentId>' so two drafts to different people coexist.
    var draftKey = 'draft-' + parseInt(agent.id, 10);
    openWindow(draftKey, {
      display_name: agent.display_name,
      avatar_url:   agent.avatar_url,
      target_agent_id: parseInt(agent.id, 10),
      is_draft: true,
    });
  }

  // ── Threads list rendering ─────────────────────────────────────────────────
  function renderThreads() {
    if (!state.threads.length) {
      threadList.innerHTML = '<div class="fmsg-empty">No conversations yet.<br>Click <b>✏︎</b> to start one.</div>';
      return;
    }
    threadList.innerHTML = '';
    state.threads.forEach(function (t) {
      var row = document.createElement('div');
      row.className = 'fmsg-thread-row' + (t.unread_count > 0 ? ' unread' : '');
      var avatarBg = t.peer_avatar ? 'background-image:url(' + esc(t.peer_avatar) + ');' : '';
      var preview = t.last_preview || '';
      if (t.last_sender_id === ME_AGENT_ID && preview) preview = 'You: ' + preview;
      var status = t.peer_status || 'offline';
      row.innerHTML =
        '<div class="fmsg-avatar" style="' + avatarBg + '"><span class="fmsg-status-dot ' + status + '"></span></div>' +
        '<div class="fmsg-thread-info">' +
          '<div class="fmsg-thread-name">' + esc(t.title) + '</div>' +
          '<div class="fmsg-thread-preview">' + esc(preview) + '</div>' +
        '</div>' +
        '<div class="fmsg-thread-meta">' +
          '<div class="fmsg-thread-time">' + esc(timeAgo(t.last_message_at)) + '</div>' +
          (t.unread_count > 0 ? '<div class="fmsg-thread-badge">' + (t.unread_count > 99 ? '99+' : t.unread_count) + '</div>' : '') +
        '</div>';
      row.addEventListener('click', function () {
        closeSettingsMenu();
        openWindow(t.id, { display_name: t.title, avatar_url: t.peer_avatar, peer_status: status });
      });
      threadList.appendChild(row);
    });
  }

  // ── Windows ────────────────────────────────────────────────────────────────
  function isMobile() { return window.matchMedia('(max-width: 640px)').matches; }

  function syncMobileState() {
    if (!isMobile()) {
      panel.classList.remove('conv-open');
      return;
    }
    panel.classList.toggle('conv-open', state.openWindows.size > 0);
  }

  function openWindow(threadId, peer) {
    var isDraft = peer && peer.is_draft;
    threadId = isDraft ? threadId : parseInt(threadId, 10);
    if (state.openWindows.has(threadId)) {
      var w = state.openWindows.get(threadId);
      w.el.classList.remove('minimized');
      w.scrollPinned = true;
      if (!isDraft) loadInitialMessages(threadId);
      syncMobileState();
      return;
    }
    var maxWindows = isMobile() ? 1 : 3;
    while (state.openWindows.size >= maxWindows) {
      var firstKey = state.openWindows.keys().next().value;
      closeWindow(firstKey);
    }

    var win = document.createElement('div');
    win.className = 'fmsg-window';
    win.dataset.threadId = String(threadId);
    var avatarBg = peer && peer.avatar_url ? 'background-image:url(' + esc(peer.avatar_url) + ');' : '';
    var bodyId = 'fmsgBody-' + String(threadId).replace(/[^a-zA-Z0-9_-]/g, '_');
    var displayName = (peer && peer.display_name) || 'Conversation';
    var initialStatus = (peer && peer.peer_status) || 'offline';
    win.innerHTML =
      '<div class="fmsg-window-header" title="' + esc(displayName) + '">' +
        '<div class="fmsg-avatar" style="width:30px;height:30px;' + avatarBg + '"><span class="fmsg-status-dot ' + initialStatus + '" data-role="status-dot"></span></div>' +
        '<div class="fmsg-window-name" title="' + esc(displayName) + '">' + esc(displayName) + '</div>' +
        '<div class="fmsg-window-actions">' +
          '<button type="button" data-act="buzz"  title="Buzz — ring the recipient until they respond"><i class="ri-alarm-warning-line"></i></button>' +
          '<button type="button" data-act="nudge" title="Nudge — short attention sound"><i class="ri-vibrate-line"></i></button>' +
          '<button type="button" data-act="min"   title="Minimize"><i class="ri-subtract-line"></i></button>' +
          '<button type="button" data-act="open"  title="Open full"><i class="ri-external-link-line"></i></button>' +
          '<button type="button" data-act="close" title="Close"><i class="ri-close-line"></i></button>' +
        '</div>' +
      '</div>' +
      '<div class="fmsg-window-body" id="' + bodyId + '">' +
        '<div class="fmsg-empty" style="padding:20px 8px;">' + (isDraft ? 'Send the first message to start the conversation.' : 'Loading…') + '</div>' +
      '</div>' +
      '<div class="fmsg-window-input">' +
        '<button type="button" data-act="emoji" title="Emoji" class="fmsg-emoji-toggle"><i class="ri-emotion-happy-line"></i></button>' +
        '<textarea placeholder="Type a message…" rows="1"></textarea>' +
        '<button type="button" data-act="send" disabled title="Send"><i class="ri-send-plane-fill"></i></button>' +
      '</div>';

    windowsHost.appendChild(win);
    state.openWindows.set(threadId, {
      el: win,
      messages: [],
      peer: peer || {},
      scrollPinned: true,
      bodyId: bodyId,
      isDraft: !!isDraft,
      targetAgentId: peer && peer.target_agent_id ? parseInt(peer.target_agent_id, 10) : null,
    });
    syncMobileState();

    var header = win.querySelector('.fmsg-window-header');
    var body = win.querySelector('.fmsg-window-body');
    var ta = win.querySelector('textarea');
    var sendBtn = win.querySelector('button[data-act="send"]');

    // Resolve the CURRENT key for this window (drafts get re-keyed after promotion)
    function currentKey() {
      var raw = win.dataset.threadId;
      return /^\d+$/.test(raw) ? parseInt(raw, 10) : raw;
    }

    header.addEventListener('click', function (e) {
      var btn = e.target.closest('button[data-act]');
      if (btn) {
        e.stopPropagation();
        var act = btn.dataset.act;
        var key = currentKey();
        if (act === 'close') {
          closeWindow(key);
          return;
        }
        if (act === 'min') {
          win.classList.toggle('minimized');
          return;
        }
        if (act === 'open')  {
          var w = state.openWindows.get(key);
          if (w && w.isDraft) return; // can't open draft
          window.location.href = 'studio/private_chats/?thread=' + key;
          return;
        }
        if (act === 'nudge') return sendNudge(key);
        if (act === 'buzz')  return sendBuzz(key);
      }
      // Click on header (not on button) toggles minimize
      win.classList.toggle('minimized');
      // Any header interaction stops a buzz that's playing locally
      stopBuzz();
    });

    body.addEventListener('scroll', function () {
      var w = state.openWindows.get(currentKey());
      if (!w) return;
      w.scrollPinned = (body.scrollHeight - body.scrollTop - body.clientHeight) < 30;
    });

    var emojiBtn = win.querySelector('button[data-act="emoji"]');
    if (emojiBtn) {
      emojiBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        toggleEmojiPicker(win, ta);
      });
    }

    ta.addEventListener('input', function () {
      sendBtn.disabled = ta.value.trim().length === 0;
    });
    ta.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        if (!sendBtn.disabled) sendMessage(currentKey());
      }
    });
    sendBtn.addEventListener('click', function (e) {
      e.preventDefault();
      e.stopPropagation();
      sendMessage(currentKey());
    });

    if (!isDraft) loadInitialMessages(threadId);
  }

  function closeWindow(threadId) {
    var w = state.openWindows.get(threadId);
    if (!w) {
      // Try integer key fallback
      var asInt = parseInt(threadId, 10);
      if (!isNaN(asInt)) w = state.openWindows.get(asInt);
      if (w) threadId = asInt;
    }
    if (!w) return;
    w.el.remove();
    state.openWindows.delete(threadId);
    syncMobileState();
  }

  function loadInitialMessages(threadId) {
    var w = state.openWindows.get(threadId);
    if (!w || w.isDraft) return;
    api('api/modules/agent-chats/messages?thread_id=' + threadId + '&limit=50')
      .then(function (d) {
        var w2 = state.openWindows.get(threadId);
        if (!w2) return;
        w2.messages = d.data || [];
        renderMessages(threadId);
        // Local optimistic unread clear (poll will reconcile)
        clearLocalUnread(threadId);
      })
      .catch(function () {
        var body = document.getElementById(w.bodyId);
        if (body) body.innerHTML = '<div class="fmsg-empty">Failed to load messages.</div>';
      });
  }

  function clearLocalUnread(threadId) {
    var t = state.threads.find(function (x) { return parseInt(x.id, 10) === parseInt(threadId, 10); });
    if (t && t.unread_count > 0) {
      var prev = t.unread_count;
      t.unread_count = 0;
      var current = parseInt(bubbleBadge.textContent.replace('+', ''), 10) || 0;
      var next = Math.max(0, current - prev);
      bubbleBadge.textContent = next > 99 ? '99+' : next;
      bubble.classList.toggle('has-unread', next > 0);
      if (state.panelOpen && !state.pickerOpen) renderThreads();
      updatePollCacheFromState();
    }
  }

  function renderMessages(threadId) {
    var w = state.openWindows.get(threadId);
    if (!w) return;
    var body = document.getElementById(w.bodyId);
    if (!body) return;
    if (!w.messages.length) {
      body.innerHTML = '<div class="fmsg-empty" style="padding:20px 8px;">Say hi 👋</div>';
      return;
    }
    body.innerHTML = '';
    var lastDay = '';
    w.messages.forEach(function (m) {
      var d = new Date((m.created_at || '').replace(' ', 'T') + 'Z');
      var day = d.toLocaleDateString();
      if (day !== lastDay) {
        var dayEl = document.createElement('div');
        dayEl.className = 'fmsg-msg-day';
        dayEl.textContent = day;
        body.appendChild(dayEl);
        lastDay = day;
      }
      var mine = parseInt(m.sender_agent_id, 10) === ME_AGENT_ID;
      var msg = document.createElement('div');
      msg.className = 'fmsg-msg ' + (mine ? 'me' : 'them');
      msg.textContent = m.message || '';
      body.appendChild(msg);
      var t = document.createElement('div');
      t.className = 'fmsg-msg-time';
      t.style.alignSelf = mine ? 'flex-end' : 'flex-start';
      t.textContent = timeStamp(m.created_at);
      body.appendChild(t);
    });
    if (w.scrollPinned) body.scrollTop = body.scrollHeight;
  }

  function promoteDraftToThread(draftKey) {
    var w = state.openWindows.get(draftKey);
    if (!w || !w.isDraft || !w.targetAgentId) return Promise.reject(new Error('Invalid draft'));
    return api('api/modules/agent-chats/find_or_create', {
      method: 'POST',
      body: JSON.stringify({ target_agent_id: w.targetAgentId, csrf: CSRF }),
    }).then(function (d) {
      var realTid = parseInt(d.thread_id, 10);
      // Re-key the window from draft → real thread id
      state.openWindows.delete(draftKey);
      w.isDraft = false;
      w.el.dataset.threadId = String(realTid);
      // Fix bodyId reference too
      var newBodyId = 'fmsgBody-' + realTid;
      var bodyEl = document.getElementById(w.bodyId);
      if (bodyEl) bodyEl.id = newBodyId;
      w.bodyId = newBodyId;
      state.openWindows.set(realTid, w);
      // Rebind handlers (existing event listeners reference the old `threadId` closure
      // — easiest fix is to dispatch sendMessage with the new id externally)
      return realTid;
    });
  }

  function sendMessage(threadId) {
    var w = state.openWindows.get(threadId);
    if (!w) return;
    var ta = w.el.querySelector('textarea');
    var sendBtn = w.el.querySelector('button[data-act="send"]');
    var text = ta.value.trim();
    if (!text) return;

    // If draft, materialize the thread first then re-enter sendMessage with real id
    if (w.isDraft) {
      sendBtn.disabled = true;
      promoteDraftToThread(threadId).then(function (realTid) {
        // Keep the typed text — re-set after re-key
        var w2 = state.openWindows.get(realTid);
        var ta2 = w2.el.querySelector('textarea');
        ta2.value = text;
        doSend(realTid);
      }).catch(function (err) {
        sendBtn.disabled = false;
        alert(err.message || 'Unable to start chat');
      });
      return;
    }
    doSend(threadId);

    function doSend(tid) {
      var ww = state.openWindows.get(tid);
      if (!ww) return;
      var ta3 = ww.el.querySelector('textarea');
      var sendBtn3 = ww.el.querySelector('button[data-act="send"]');
      var t = ta3.value.trim();
      if (!t) { sendBtn3.disabled = true; return; }
      sendBtn3.disabled = true;
      var optimistic = {
        id: 'tmp-' + Date.now(),
        thread_id: tid,
        sender_agent_id: ME_AGENT_ID,
        message: t,
        created_at: new Date().toISOString().replace('T', ' ').slice(0, 19),
      };
      ww.messages.push(optimistic);
      ww.scrollPinned = true;
      renderMessages(tid);
      ta3.value = '';

      api('api/modules/agent-chats/messages', {
        method: 'POST',
        body: JSON.stringify({ thread_id: tid, message: t, csrf: CSRF }),
      }).then(function (d) {
        var idx = ww.messages.findIndex(function (m) { return m.id === optimistic.id; });
        if (idx >= 0) ww.messages[idx] = d.data;
        renderMessages(tid);
        sendBtn3.disabled = true;
        poll({ force: true, silent: true });
      }).catch(function (err) {
        // Visible failure: drop the optimistic message, restore typed text, alert user
        var idx = ww.messages.findIndex(function (m) { return m.id === optimistic.id; });
        if (idx >= 0) ww.messages.splice(idx, 1);
        renderMessages(tid);
        ta3.value = t;       // restore so user doesn't lose what they typed
        sendBtn3.disabled = false;
        var msg = (err && err.message) || 'Failed to send. Try again.';
        // Inline visible toast inside the chat body so it's impossible to miss
        var body = document.getElementById(ww.bodyId);
        if (body) {
          var warn = document.createElement('div');
          warn.className = 'fmsg-empty';
          warn.style.color = '#dc2626';
          warn.style.padding = '6px 10px';
          warn.textContent = '⚠ ' + msg;
          body.appendChild(warn);
          setTimeout(function () { warn.remove(); }, 4000);
        }
      });
    }
  }

  function sendNudge(threadId) {
    var w = state.openWindows.get(threadId);
    if (!w || w.isDraft) return; // can't nudge before first message
    var nudgeText = '👋 Nudge!';
    api('api/modules/agent-chats/messages', {
      method: 'POST',
      body: JSON.stringify({
        thread_id: threadId,
        message: nudgeText,
        metadata: { kind: 'nudge' },
        csrf: CSRF,
      }),
    }).then(function (d) {
      w.messages.push(d.data);
      w.scrollPinned = true;
      renderMessages(threadId);
      playPing(); // confirmation tone for sender
      poll({ force: true, silent: true });
    });
  }

  function sendBuzz(threadId) {
    var w = state.openWindows.get(threadId);
    if (!w || w.isDraft) return;
    if (!confirm('Send an urgent BUZZ? This will ring on the recipient\'s end.')) return;
    var buzzText = '🚨 Buzz! Need your attention.';
    api('api/modules/agent-chats/messages', {
      method: 'POST',
      body: JSON.stringify({
        thread_id: threadId,
        message: buzzText,
        metadata: { kind: 'buzz' },
        csrf: CSRF,
      }),
    }).then(function (d) {
      w.messages.push(d.data);
      w.scrollPinned = true;
      renderMessages(threadId);
      playNudge();
      poll({ force: true, silent: true });
    });
  }

  // ── Emoji picker ───────────────────────────────────────────────────────────
  var EMOJIS = [
    '😀','😂','🤣','😊','😍','😘','🥰','😎','🤩','🤔',
    '😴','😅','😭','😡','🤯','🥳','😱','😬','🤗','🤝',
    '👍','👎','👏','🙏','💪','🔥','✨','🎉','💯','✅',
    '❌','❓','❗','💡','📌','📎','🚀','💼','📞','📧',
    '❤️','💔','💜','💚','💙','🧡','💛','🤍','🖤','💖',
    '👋','🤞','✌️','🤟','🤘','👌','👀','🙌','🤷','🤦',
  ];
  function toggleEmojiPicker(win, ta) {
    var existing = win.querySelector('.fmsg-emoji-picker');
    if (existing) { existing.remove(); return; }
    var picker = document.createElement('div');
    picker.className = 'fmsg-emoji-picker open';
    EMOJIS.forEach(function (e) {
      var b = document.createElement('button');
      b.type = 'button';
      b.textContent = e;
      b.addEventListener('click', function (ev) {
        ev.stopPropagation();
        // Insert at cursor position
        var start = ta.selectionStart || 0;
        var end = ta.selectionEnd || 0;
        ta.value = ta.value.substring(0, start) + e + ta.value.substring(end);
        ta.selectionStart = ta.selectionEnd = start + e.length;
        ta.focus();
        ta.dispatchEvent(new Event('input'));
      });
      picker.appendChild(b);
    });
    win.querySelector('.fmsg-window-input').appendChild(picker);
    // Click outside to close
    setTimeout(function () {
      var off = function (ev) {
        if (!picker.contains(ev.target) && !ev.target.closest('button[data-act="emoji"]')) {
          picker.remove();
          document.removeEventListener('click', off);
        }
      };
      document.addEventListener('click', off);
    }, 50);
  }

  // ── Polling ────────────────────────────────────────────────────────────────
  function shakeWindow(threadId) {
    var w = state.openWindows.get(threadId);
    if (!w) return;
    w.el.classList.remove('fmsg-shake');
    void w.el.offsetWidth; // reflow → restart animation
    w.el.classList.add('fmsg-shake');
    setTimeout(function () { w.el.classList.remove('fmsg-shake'); }, 700);
  }

  function poll(options) {
    options = options || {};
    var openIds = Array.from(state.openWindows.keys()).filter(function (k) {
      var n = parseInt(k, 10);
      return !isNaN(n) && n > 0 && String(n) === String(k); // skip draft-* keys
    });
    if (!options.force && !openIds.length) {
      var cached = cacheRead('poll', BOOT_CACHE_TTL_MS);
      if (cached) {
        applyPollSnapshot(cached);
        state.initialPollComplete = true;
        return Promise.resolve(cached);
      }
    }
    var qs = openIds.map(function (id) { return 'open[]=' + id; }).join('&');
    var since = state.lastSince ? '&since=' + encodeURIComponent(state.lastSince) : '';
    var url = 'api/modules/agent-chats/poll' + (qs || since ? '?' + qs + since : '');

    return api(url)
      .then(function (d) {
        state.pollErrorCount = 0;
        if (d && d.disabled) {
          stopPolling();
          root.style.display = 'none';
          return;
        }
        var n = parseInt(d.unread_total, 10) || 0;
        var prevUnread = parseInt(state.lastUnreadTotal || 0, 10);
        var wasInitialPoll = !state.initialPollComplete;
        applyPollSnapshot(d);

        // Threads — server returns `null` when nothing has changed since `since`,
        // in which case we keep the existing cached list.
        if (Array.isArray(d.threads)) {
          state.threads = d.threads;
          if (state.panelOpen && !state.pickerOpen) renderThreads();
        }

        // Detect any incoming-from-others (for sound + threads-list ping)
        var anyNewFromOthers = false;
        var anyNudgeFromOthers = false;

        // New messages for open windows
        var anyNew = false;
        var anyBuzzFromOthers = false;
        var threadsToMarkRead = [];
        var notifyQueue = []; // for browser notifications when tab hidden
        if (d.new_messages && typeof d.new_messages === 'object') {
          Object.keys(d.new_messages).forEach(function (tidStr) {
            var tid = parseInt(tidStr, 10);
            var w = state.openWindows.get(tid);
            if (!w) return;
            var arr = d.new_messages[tidStr] || [];
            var added = false;
            arr.forEach(function (m) {
              if (!w.messages.some(function (x) { return parseInt(x.id, 10) === parseInt(m.id, 10); })) {
                w.messages.push(m);
                added = true;
                anyNew = true;
                if (parseInt(m.sender_agent_id, 10) !== ME_AGENT_ID) {
                  anyNewFromOthers = true;
                  notifyQueue.push({ tid: tid, sender: m.sender_name || 'New message', body: m.message || '' });
                  if (m.metadata && m.metadata.kind === 'nudge') {
                    anyNudgeFromOthers = true;
                    shakeWindow(tid);
                  } else if (m.metadata && m.metadata.kind === 'buzz') {
                    anyBuzzFromOthers = true;
                    shakeWindow(tid);
                  }
                }
              }
            });
            if (added) {
              renderMessages(tid);
              // If window is open & not minimized, mark thread read immediately
              if (!w.el.classList.contains('minimized')) threadsToMarkRead.push(tid);
            }
          });
        }

        // Detect new unread that we don't have open — also queue notification
        if (!anyNewFromOthers) {
          var totalUnreadFromServer = parseInt(d.unread_total, 10) || 0;
          if (totalUnreadFromServer > prevUnread) {
            anyNewFromOthers = true;
            // Find the most recently updated thread for the notification fallback
            var freshest = (d.threads || []).find(function (t) { return t.unread_count > 0; });
            if (freshest) notifyQueue.push({ tid: freshest.id, sender: freshest.title, body: freshest.last_preview || 'New message' });
          }
        }
        state.lastUnreadTotal = n;

        // Sound (DND/sound_off respected)
        if (!options.silent && !wasInitialPoll && shouldPlaySound()) {
          if (anyBuzzFromOthers) playBuzz();
          else if (anyNudgeFromOthers) playNudge();
          else if (anyNewFromOthers) playPing();
        }

        // Browser notifications (only when tab hidden)
        if (anyNewFromOthers && document.hidden) {
          notifyQueue.slice(0, 3).forEach(function (n) {
            showBrowserNotification(n.sender, n.body, n.tid);
          });
        }

        // Mark-read for visibly-open windows (so badge clears immediately)
        threadsToMarkRead.forEach(function (tid) {
          api('api/modules/agent-chats/mark_read', {
            method: 'POST',
            body: JSON.stringify({ thread_id: tid, csrf: CSRF }),
          }).then(function () { clearLocalUnread(tid); }).catch(function () {});
        });

        if (d.server_time) state.lastSince = d.server_time;
        state.initialPollComplete = true;
        cacheWrite('poll', {
          ok: true,
          unread_total: n,
          threads: state.threads || [],
          new_messages: {},
          server_time: state.lastSince,
        });

        // Burst mode: schedule quick follow-up poll if anything new arrived
        if (anyNew && !document.hidden) {
          setTimeout(function () { poll({ force: true }); }, POLL_BURST_MS);
        }
        return d;
      })
      .catch(function () {
          state.pollErrorCount = Math.min((state.pollErrorCount || 0) + 1, 6);
        if (state.pollErrorCount >= 3) {
          stopPolling();
          state.pollTimer = setTimeout(function () {
            state.pollTimer = null;
            poll({ force: true, silent: true });
            startPolling();
          }, window.appConfig?.chatRefreshRetryMs || 300000);
        }
      });
  }

  function startPolling() {
    stopPolling();
    var interval = document.hidden ? POLL_HIDDEN_MS : POLL_ACTIVE_MS;
    state.pollTimer = setInterval(poll, interval);
  }
  function stopPolling() {
    if (state.pollTimer) { clearInterval(state.pollTimer); state.pollTimer = null; }
  }

  document.addEventListener('visibilitychange', function () {
    startPolling();
    if (!document.hidden) poll({ force: true, silent: true });
  });

  window.addEventListener('resize', function () {
    syncMobileState();
  });

  // ── DopRealtime wiring (provider-agnostic real-time for messenger) ──────────
  // Visual indicator: green = live, amber = connecting, gray = polling, red = failed
  function setTransport(state, label) {
    var dot = document.getElementById('fmsgTransportDot');
    if (!dot) return;
    dot.classList.remove('live', 'connecting', 'polling', 'failed');
    dot.classList.add(state);
    var retryHint = (state === 'polling' || state === 'failed') ? ' — click to retry realtime' : '';
    dot.title = label + retryHint;
    dot.setAttribute('aria-label', 'Transport: ' + label + retryHint);
  }

  function retryRealtime() {
    var rt = window.DopRealtime;
    if (!rt || !ME_USER_ID) {
      setTransport('polling', 'Polling — real-time not configured');
      poll({ force: true, silent: true });
      return;
    }

    setTransport('connecting', 'Retrying realtime connection…');
    var ok = false;
    if (typeof rt.retry === 'function') {
      ok = rt.retry();
    } else if (typeof rt.init === 'function' && window.REALTIME_CONFIG) {
      ok = rt.init(window.REALTIME_CONFIG);
    }

    if (!ok) {
      POLL_ACTIVE_MS = window.appConfig?.chatRefreshMs || 60000;
      POLL_HIDDEN_MS = window.appConfig?.chatRefreshHiddenMs || 300000;
      startPolling();
      setTransport('failed', 'Real-time retry failed — polling');
      return;
    }

    setTimeout(function () {
      if (!rt.isConnected || !rt.isConnected()) {
        POLL_ACTIVE_MS = window.appConfig?.chatRefreshMs || 60000;
        POLL_HIDDEN_MS = window.appConfig?.chatRefreshHiddenMs || 300000;
        startPolling();
        setTransport('polling', 'Still polling');
      }
    }, 2500);
  }

  function bindTransportRetry() {
    var dot = document.getElementById('fmsgTransportDot');
    if (!dot || dot.dataset.retryBound === '1') return;
    dot.dataset.retryBound = '1';
    dot.setAttribute('role', 'button');
    dot.setAttribute('tabindex', '0');
    dot.addEventListener('click', retryRealtime);
    dot.addEventListener('keydown', function (e) {
      if (e.key !== 'Enter' && e.key !== ' ') return;
      e.preventDefault();
      retryRealtime();
    });
  }

  function realtimeProviderLabel() {
    var cfg = window.REALTIME_CONFIG || {};
    return cfg.providerLabel || cfg.provider || 'realtime';
  }

  function initPusher() {
    var rt = window.DopRealtime;
    if (!rt || !ME_USER_ID) {
      setTransport('polling', 'Polling — real-time not configured');
      return false;
    }

    setTransport('connecting', 'Connecting…');

    rt.on('new-message', onPusherMessage);

    // thread:updated — lightweight signal pushed to all participants (including sender)
    // when any message is sent in a thread. The full `new-message` event updates
    // recipients locally; senders already refresh after POST, so this stays DB-free.
    rt.on('thread:updated', function (payload) {
      if (!payload || parseInt(payload.sender_agent_id, 10) === ME_AGENT_ID) return;
      applyRealtimeMessageToThreads({
        thread_id: payload.thread_id,
        sender_agent_id: payload.sender_agent_id,
        message: payload.preview || '',
        created_at: payload.sent_at || new Date().toISOString().replace('T', ' ').slice(0, 19),
      }, false);
    });

    document.addEventListener('realtime:connected', function () {
      // Real-time is live — stop the polling timer entirely
      stopPolling();
      setTransport('live', 'Live via ' + realtimeProviderLabel());
    });
    document.addEventListener('realtime:disconnected', function () {
      POLL_ACTIVE_MS = window.appConfig?.chatRefreshMs || 60000;
      POLL_HIDDEN_MS = window.appConfig?.chatRefreshHiddenMs || 300000;
      startPolling();
      setTransport('polling', 'Disconnected — polling');
    });
    document.addEventListener('realtime:failed', function () {
      POLL_ACTIVE_MS = window.appConfig?.chatRefreshMs || 60000;
      POLL_HIDDEN_MS = window.appConfig?.chatRefreshHiddenMs || 300000;
      startPolling();
      setTransport('failed', 'Real-time failed — polling');
    });

    // If already connected by the time messenger boots, stop polling immediately
    if (rt.isConnected()) {
      stopPolling();
      setTransport('live', 'Live via ' + realtimeProviderLabel());
    }
    return true;
  }

  function onPusherMessage(msg) {
    if (!msg || !msg.thread_id) return;
    var tid = parseInt(msg.thread_id, 10);
    var senderIsMe = parseInt(msg.sender_agent_id, 10) === ME_AGENT_ID;
    var w = state.openWindows.get(tid);
    var shouldMarkUnread = !senderIsMe && (!w || w.el.classList.contains('minimized'));
    var threadKnown = applyRealtimeMessageToThreads(msg, shouldMarkUnread);
    if (shouldMarkUnread) bumpUnreadBadge(1);
    if (!threadKnown && state.panelOpen) poll({ force: true, silent: true });

    // 1) If the thread has a conversation window open, append the message live
    if (w) {
      if (!w.messages.some(function (x) { return parseInt(x.id, 10) === parseInt(msg.id, 10); })) {
        w.messages.push(msg);
        renderMessages(tid);
        if (!w.el.classList.contains('minimized')) {
          // Mark read in background — the window is visible
          api('api/modules/agent-chats/mark_read', {
            method: 'POST',
            body: JSON.stringify({ thread_id: tid, csrf: CSRF }),
          }).then(function () { clearLocalUnread(tid); }).catch(function () {});
        }
      }
    }

    // 2) If it's from someone else, play sound + maybe browser notification
    if (!senderIsMe) {
      if (shouldPlaySound()) {
        if (msg.metadata && msg.metadata.kind === 'buzz')       playBuzz();
        else if (msg.metadata && msg.metadata.kind === 'nudge') playNudge();
        else                                                    playPing();
      }
      if (msg.metadata && (msg.metadata.kind === 'buzz' || msg.metadata.kind === 'nudge')) {
        shakeWindow(tid);
      }
      if (document.hidden) {
        showBrowserNotification(msg.sender_name || 'New message', msg.message || '', tid);
      }
    }

    // 3) Thread list refresh is handled by the thread:updated event pushed to all
    //    participants. When realtime is down the polling interval covers it.
  }

  // ── Boot ───────────────────────────────────────────────────────────────────
  loadMySettings();
  bindTransportRetry();
  initPusher();
  poll({ silent: true });
  if (!window.DopRealtime || !window.DopRealtime.isConnected || !window.DopRealtime.isConnected()) {
    startPolling();
  }

})();
