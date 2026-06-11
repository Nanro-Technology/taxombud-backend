// Lightweight chat wiring using provided layout/styles
(function() {
  const api = typeof apiEndpoints === 'function' ? apiEndpoints() : {};
  const threadsUrl = api.agentChatThreads;
  const messagesUrl = api.agentChatMessages;
  const uploadUrl = api.agentChatUpload;
  const downloadUrl = api.agentChatDownload;
  if (!threadsUrl || !messagesUrl) return;

  const currentAgentId = typeof window.currentAgentId === 'number' ? window.currentAgentId : null;

  const userListEl = document.getElementById('userList');
  const channelListEl = document.getElementById('channelList');
  const agentContactsEl = document.getElementById('agentContacts');
  const usersConversationEl = document.getElementById('users-conversation');
  const channelConversationEl = document.getElementById('channel-conversation');
  const chatInput = document.getElementById('chat-input');
  const chatForm = document.getElementById('chatinput-form');
  const chatInputSection = document.getElementById('chat-input-section');
  const sendBtn = chatForm ? chatForm.querySelector('button[type="submit"]') : null;
  const attachBtn = document.getElementById('attach-btn');
  const attachInput = document.getElementById('chat-attachment-input');
  const attachmentsWrap = document.getElementById('chat-attachments');
  const uploadStatusEl = document.getElementById('chat-uploading');
  const loaderEl = document.getElementById('userLoader');
  const loaderEl2 = document.getElementById('channelLoader');
  const copyToast = document.getElementById('copyClipBoard');
  const copyToastChannel = document.getElementById('copyClipBoardChannel');
  const userTopbarName = document.querySelector('#users-chat .user-chat-topbar .username');
  const channelTopbarName = document.querySelector('#channel-chat .user-chat-topbar .username');
  const userAvatarImg = document.querySelector('#users-chat .user-chat-topbar .avatar-xs');
  const channelAvatarImg = document.querySelector('#channel-chat .user-chat-topbar .avatar-xs');
  const userStatusEl = document.querySelector('#users-chat .userStatus');
  const channelStatusEl = document.querySelector('#channel-chat .userStatus');
  const btnNewDirect = document.getElementById('btnNewDirect');
  const btnNewDirect2 = document.getElementById('btnNewDirect2');
  const btnNewGroup = document.getElementById('btnNewGroup');
  const contactsTabLink = document.querySelector('a[data-bs-toggle="tab"][href="#contacts"]');
  const chatSearchInput = document.getElementById('chatSearch');
  const newChatModalEl = document.getElementById('newChatModal');
  const newGroupModalEl = document.getElementById('newGroupModal');
  const newChatAgentSel = document.getElementById('newChatAgent');
  const newGroupAgentsSel = document.getElementById('newGroupAgents');
  const newGroupTopicInput = document.getElementById('newGroupTopic');
  const startDirectChatBtn = document.getElementById('startDirectChat');
  const createGroupChatBtn = document.getElementById('createGroupChat');
  const removeMemberModalEl = document.getElementById('removeMemberModal');
  const removeMemberListEl = document.getElementById('removeMemberList');
  const removeMemberEmptyEl = document.getElementById('removeMemberEmpty');
  const leaveGroupModalEl = document.getElementById('leaveGroupModal');
  const leaveGroupConfirmBtn = document.getElementById('confirmLeaveGroup');
  const addMemberModalEl = document.getElementById('addMemberModal');
  const addMemberSelect = document.getElementById('addMemberSelect');
  const addMemberBtn = document.getElementById('addMemberBtn');
  const addMemberNone = document.getElementById('addMemberNone');
  const infoModalEl = document.getElementById('userProfileCanvasExample');
  const infoModal = infoModalEl && typeof bootstrap !== 'undefined' ? new bootstrap.Modal(infoModalEl) : null;
  const infoBtn = document.getElementById('channelInfoBtn');
  const infoName = document.getElementById('infoGroupName');
  const infoAdmin = document.getElementById('infoGroupAdmin');
  const infoCreated = document.getElementById('infoGroupCreated');
  const infoCount = document.getElementById('infoGroupCount');
  const infoMembers = document.getElementById('infoGroupMembers');

  const defaultAvatarMale = 'assets/images/avatar_male.jpg';
  const defaultAvatarFemale = 'assets/images/avatar_female.jpg';
  const chatLeftSidebar = document.querySelector('.chat-leftsidebar');
  const userChatWrapper = document.querySelector('.user-chat');

  const state = {
    threads: [],
    currentThreadId: null,
    currentType: 'dm', // 'dm' or 'channel'
    agents: [],
    pendingDmAgentId: null
  };
  const REQUEST_TIMEOUT_MS = window.appConfig?.chatRequestTimeoutMs ?? 20000;
  const pendingUploads = [];
  let uploading = false;
  let threadsLoading = false;
  let messagesLoading = false;
  const deleteActions = document.querySelectorAll('.delete-thread');
  const removeActions = document.querySelectorAll('.remove-member-thread');
  const leaveActions = document.querySelectorAll('.leave-thread');
  const addActions = document.querySelectorAll('.add-member-thread');
  const agentListLoader = document.getElementById('agentListLoader');

  function clearBlockingLoaders() {
    try {
      if (typeof window.clearAllGlobalUiLoading === 'function') {
        window.clearAllGlobalUiLoading();
      }
      if (typeof window.hideGlobalLoading === 'function') {
        window.hideGlobalLoading();
      }
      const globalLoader = document.getElementById('loader');
      if (globalLoader) {
        globalLoader.style.display = 'none';
      }
    } catch (_e) {
      // keep chat resilient if loader helper is unavailable
    }
  }

  function escapeHtml(str) {
    return (str || '').replace(/[&<>"']/g, function(m) {
      const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' };
      return map[m] || m;
    });
  }

  function getThreadParticipantLabel(participant, creatorId) {
    const base = participant?.display_name || ('Agent #' + (participant?.agent_id || ''));
    return creatorId && parseInt(participant?.agent_id, 10) === creatorId ? (base + ' (admin)') : base;
  }

  function formatParticipantPreview(participants, creatorId, limit = 4) {
    const list = Array.isArray(participants) ? participants : [];
    const labels = list.map(p => getThreadParticipantLabel(p, creatorId)).filter(Boolean);
    if (!labels.length) return '';
    if (labels.length <= limit) return labels.join(', ');
    const head = labels.slice(0, limit).join(', ');
    const remaining = labels.length - limit;
    return `${head}, and ${remaining} more...`;
  }

  function normalizeMeta(meta) {
    if (!meta) return {};
    if (typeof meta === 'string') {
      try {
        return JSON.parse(meta);
      } catch (e) {
        return {};
      }
    }
    return meta;
  }

  const EMOJIS = [
    '😀','😁','😂','🤣','😃','😄','😅','😉','😊','😎','😍','😘','🤗','🤔','🤨','😐','😑','😶','🙄','😏','😣','😥','😮','🤐','😯','😪','😴',
    '😷','🤒','🤕','🤢','🤮','🤧','🥵','🥶','🥳','🤩','🥺','😤','😡','😢','😭',
    '❤️','💕','💖','💘','🙏','👍','👎','👏','🙌','👌','🤝','💪',
    '🔥','✨','🎉','🏆','💯','⚡','⏳','📌','✏️','📝','📎','📁','📂','📤','📥','🔒','🔓'
  ];
  let emojiPanel = null;

  function ensureEmojiPanel() {
    if (emojiPanel) return emojiPanel;
    emojiPanel = document.createElement('div');
    emojiPanel.className = 'shadow-sm border bg-white rounded p-2';
    emojiPanel.style.position = 'absolute';
    emojiPanel.style.zIndex = '1200';
    emojiPanel.style.display = 'none';
    emojiPanel.style.minWidth = '220px';
    emojiPanel.innerHTML = `<div class="d-flex flex-wrap gap-1">${EMOJIS.map(e => `<button type="button" class="btn btn-light btn-sm emoji-pick" style="line-height:1.2">${e}</button>`).join('')}</div>`;
    document.body.appendChild(emojiPanel);
    emojiPanel.addEventListener('click', (ev) => {
      const btn = ev.target.closest('.emoji-pick');
      if (!btn) return;
      insertEmoji(btn.textContent || '');
    });
    document.addEventListener('click', (e) => {
      if (!emojiPanel) return;
      const isBtn = e.target.closest('#emoji-btn');
      const inPanel = e.target.closest('.emoji-pick') || e.target === emojiPanel || emojiPanel.contains(e.target);
      if (!isBtn && !inPanel) {
        emojiPanel.style.display = 'none';
      }
    });
    return emojiPanel;
  }

  function positionEmojiPanel(btn) {
    const panel = ensureEmojiPanel();
    const rect = btn.getBoundingClientRect();
    // show briefly to measure height
    const prevDisplay = panel.style.display;
    panel.style.visibility = 'hidden';
    panel.style.display = 'block';
    const height = panel.offsetHeight || 0;
    panel.style.display = prevDisplay;
    panel.style.visibility = '';
    const top = rect.top + window.scrollY - height - 6;
    const left = rect.left + window.scrollX;
    panel.style.top = `${Math.max(top, 8)}px`;
    panel.style.left = `${left}px`;
  }

  function toggleEmojiPanel() {
    if (!emojiPanel) ensureEmojiPanel();
    if (!emojiPanel) return;
    if (emojiPanel.style.display === 'none' || emojiPanel.style.display === '') {
      positionEmojiPanel(emojiBtn);
      emojiPanel.style.display = 'block';
    } else {
      emojiPanel.style.display = 'none';
    }
  }

  function insertEmoji(emoji) {
    if (!chatInput || !emoji) return;
    const start = chatInput.selectionStart || chatInput.value.length;
    const end = chatInput.selectionEnd || chatInput.value.length;
    const value = chatInput.value;
    chatInput.value = value.slice(0, start) + emoji + value.slice(end);
    const newPos = start + emoji.length;
    chatInput.focus();
    if (chatInput.setSelectionRange) {
      chatInput.setSelectionRange(newPos, newPos);
    }
  }

  const emojiBtn = document.getElementById('emoji-btn');

  function setUploadingState(state) {
    uploading = !!state;
    if (sendBtn) sendBtn.disabled = uploading;
    if (attachBtn) attachBtn.disabled = uploading;
    if (uploadStatusEl) {
      if (uploading) {
        uploadStatusEl.classList.remove('d-none');
        uploadStatusEl.classList.add('d-flex');
      } else {
        uploadStatusEl.classList.add('d-none');
        uploadStatusEl.classList.remove('d-flex');
      }
    }
  }

  function toggleChatInput(force) {
    if (!chatInputSection) return;
    const shouldShow = typeof force === 'boolean' ? force : !!state.currentThreadId;
    chatInputSection.style.display = shouldShow ? '' : 'none';
  }

  function fetchJsonWithTimeout(url, options = {}, timeoutMs = REQUEST_TIMEOUT_MS) {
    const reqOptions = { credentials: 'same-origin', ...options };
    if (typeof AbortController === 'undefined') {
      return fetch(url, reqOptions).then((r) => {
        if (!r.ok) throw new Error(`Request failed: ${r.status}`);
        return r.json();
      });
    }
    const controller = new AbortController();
    const timerId = setTimeout(() => controller.abort(), timeoutMs);
    const req = { ...reqOptions, signal: controller.signal };
    return fetch(url, req)
      .then((r) => {
        if (!r.ok) throw new Error(`Request failed: ${r.status}`);
        return r.json();
      })
      .finally(() => clearTimeout(timerId));
  }

  function refreshAttachmentsUI() {
    if (!attachmentsWrap) return;
    attachmentsWrap.innerHTML = '';
    pendingUploads.forEach((att, idx) => {
      const pill = document.createElement('div');
      pill.className = 'chat-attachment-pill';
      const name = escapeHtml(att.name || att.orig_name || att.stored_name || 'Attachment');
      pill.innerHTML = `<i class="ri-attachment-2"></i><span>${name}</span><span class="remove-attach" data-idx="${idx}">&times;</span>`;
      attachmentsWrap.appendChild(pill);
    });
  }

  function clearAttachments() {
    pendingUploads.length = 0;
    if (attachInput) attachInput.value = '';
    refreshAttachmentsUI();
  }

  function copyTextToClipboard(text) {
    if (typeof text !== 'string') text = '';
    if (navigator.clipboard && navigator.clipboard.writeText) {
      return navigator.clipboard.writeText(text).catch(() => fallbackCopy(text));
    }
    return fallbackCopy(text);
  }

  function fallbackCopy(text) {
    try {
      const ta = document.createElement('textarea');
      ta.value = text;
      ta.setAttribute('readonly', '');
      ta.style.position = 'absolute';
      ta.style.left = '-9999px';
      document.body.appendChild(ta);
      const sel = document.getSelection();
      const prevRange = sel && sel.rangeCount ? sel.getRangeAt(0) : null;
      ta.select();
      document.execCommand('copy');
      if (prevRange && sel) {
        sel.removeAllRanges();
        sel.addRange(prevRange);
      }
      document.body.removeChild(ta);
    } catch (e) {
      // ignore
    }
    return Promise.resolve();
  }

  async function uploadFiles(fileList) {
    if (!uploadUrl || !fileList || !fileList.length) return;
    setUploadingState(true);
    try {
      for (const file of Array.from(fileList)) {
        const fd = new FormData();
        fd.append('file', file);
        const resp = await fetch(uploadUrl, {
          method: 'POST',
          body: fd
        });
        if (!resp.ok) {
          throw new Error('upload failed');
        }
        const data = await resp.json();
        const saved = data?.data || {};
        pendingUploads.push({
          name: saved.name || file.name,
          url: saved.url || '',
          rel_path: saved.rel_path || '',
          mime: saved.mime || '',
          size: saved.size || file.size || null,
        });
        refreshAttachmentsUI();
      }
    } catch (err) {
      window.crmUiAlert('Unable to upload file. Please try again.');
    } finally {
      setUploadingState(false);
    }
  }

  function setupLoader(el) {
    if (!el) return null;
    el.style.position = 'absolute';
    el.style.inset = '0';
    el.style.background = 'rgba(255,255,255,0.7)';
    el.style.zIndex = '5';
    el.style.width = '100%';
    el.style.height = '100%';
    el.style.justifyContent = 'center';
    el.style.alignItems = 'center';
    if (!el.classList.contains('d-flex')) el.classList.add('d-flex');
    const parent = el.parentElement;
    if (parent) parent.style.position = 'relative';
    return el;
  }

  const userLoader = setupLoader(loaderEl);
  const channelLoader = setupLoader(loaderEl2);

  function showLoader(show, type) {
    const hide = (el) => { if (el) el.classList.add('d-none'); };
    const showEl = (el) => { if (el) el.classList.remove('d-none'); };
    if (!show) {
      hide(userLoader);
      hide(channelLoader);
      return;
    }
    if (type === 'channel') {
      showEl(channelLoader);
      hide(userLoader);
    } else {
      showEl(userLoader);
      hide(channelLoader);
    }
  }
  function toggleLiveBar() { /* no live bars */ }

  // ensure blank state on load
  if (usersConversationEl) usersConversationEl.innerHTML = '';
  if (channelConversationEl) channelConversationEl.innerHTML = '';
  if (userTopbarName) userTopbarName.textContent = 'Select conversation';
  if (channelTopbarName) channelTopbarName.textContent = 'Select channel';



  function avatarFallback(letter) {
    const l = (letter || '?').toString().charAt(0).toUpperCase();
    return `<div class="avatar-title rounded-circle bg-primary text-white fs-10">${l}</div><span class="user-status"></span>`;
  }

  function resolveAvatar(participant) {
    if (participant?.avatar_url) return participant.avatar_url;
    // If we ever store gender, plug it here; default male
    return defaultAvatarMale;
  }

  function formatRelativeTime(ts) {
    if (!ts) return '';
    const d = new Date(ts.replace(' ', 'T'));
    if (isNaN(d.getTime())) return ts;
    const now = new Date();
    const diffMs = now - d;
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffH = Math.floor(diffMin / 60);
    const diffD = Math.floor(diffH / 24);
    if (diffSec < 60) return 'just now';
    if (diffMin < 60) return `${diffMin} min${diffMin === 1 ? '' : 's'} ago`;
    if (diffH < 24) return `${diffH} hr${diffH === 1 ? '' : 's'} ago`;
    const day = String(d.getDate());
    const month = d.toLocaleString('en', { month: 'short' });
    return `${day} ${month}`;
  }

  function threadLastPreview(thread) {
    const last = thread && thread.last_message ? thread.last_message : null;
    if (!last) return '';
    const msg = String(last.message || '').trim();
    if (msg !== '') return msg;
    const meta = normalizeMeta(last.metadata);
    const atts = Array.isArray(meta.attachments) ? meta.attachments.filter(a => a && (a.url || a.public_url || a.rel_path || a.path)) : [];
    if (atts.length > 1) return `${atts.length} attachments`;
    if (atts.length === 1) return 'Attachment';
    return 'No messages yet';
  }

  function shouldShowThreadInSidebar(thread, type) {
    // Keep visibility stable: users should always see their thread list, including new/empty threads.
    if (!thread) return false;
    return true;
  }

  function renderThreadItem(thread, type) {
    const lastMsg = threadLastPreview(thread);
    const updatedAt = thread.last_message?.created_at || thread.last_message_at || '';
    const participants = thread.participants || [];
    let displayName = thread.topic || ('Thread #' + thread.id);
    let peerStatus = 'offline';
    if (type === 'dm') {
      const other = participants.find(p => parseInt(p.agent_id, 10) !== currentAgentId) || participants[0];
      displayName = other?.display_name || displayName;
      peerStatus = other?.peer_status || 'offline';
    }
    const safeName = escapeHtml(displayName);
    const avatarSrc = type === 'dm'
      ? resolveAvatar(participants.find(p => parseInt(p.agent_id, 10) !== currentAgentId) || participants[0] || {})
      : defaultAvatarMale;
    const unread = thread.unread_count || 0;
    const timeLabel = formatRelativeTime(updatedAt);
    // The theme CSS rule is .chat-user-img.online/.away/.offline .user-status — status class on the parent div
    const statusClass = (type === 'dm' && peerStatus !== 'offline') ? (' ' + peerStatus) : '';
    const li = document.createElement('li');
    li.dataset.threadId = thread.id;
    li.dataset.type = type;
    li.innerHTML = `
      <a href="javascript: void(0);" class="thread-link">
        <div class="d-flex align-items-center">
          <div class="flex-shrink-0 chat-user-img${statusClass} align-self-center me-2 ms-0">
            <div class="avatar-xxs">
              <img src="${avatarSrc}" class="rounded-circle img-fluid userprofile" alt="">
              <span class="user-status"></span>
            </div>
          </div>
          <div class="flex-grow-1 overflow-hidden">
            <p class="d-flex justify-content-between align-items-center mb-0">
              <span class="d-flex align-items-center flex-grow-1">
                <span class="text-truncate me-2">${safeName}</span>
                ${unread > 0 ? `<span style="color:red">(${unread})</span>` : ''}
              </span>
              <span class="text-muted small">${timeLabel ? escapeHtml(timeLabel) : ''}</span>
            </p>
            <div class="text-muted text-truncate small mb-0">${escapeHtml(lastMsg)}</div>
          </div>
        </div>
      </a>
    `;
    li.addEventListener('click', () => openThread(thread.id, type));
    return li;
  }

  function renderThreadLists(threads) {
    if (userListEl) userListEl.innerHTML = '';
    if (channelListEl) channelListEl.innerHTML = '';
    const query = (chatSearchInput?.value || '').toLowerCase();
    threads.forEach(t => {
      const participantCount = (t.participants || []).length;
      const type = participantCount > 2 ? 'channel' : 'dm';
      if (!shouldShowThreadInSidebar(t, type)) return;
      const target = type === 'channel' ? channelListEl : userListEl;
      const name = (t.topic || '').toLowerCase();
      const other = (t.participants || []).find(p => parseInt(p.agent_id, 10) !== currentAgentId);
      const otherName = (other?.display_name || '').toLowerCase();
      const lastMsg = threadLastPreview(t).toLowerCase();
      if (query && !name.includes(query) && !otherName.includes(query) && !lastMsg.includes(query)) return;
      if (target) target.appendChild(renderThreadItem(t, type));
    });
  }

  function resolveSenderMeta(senderId, threadId) {
    const thread = state.threads.find(t => parseInt(t.id, 10) === parseInt(threadId, 10));
    if (!thread) return {};
    const p = (thread.participants || []).find(x => parseInt(x.agent_id, 10) === parseInt(senderId, 10));
    return p || {};
  }

  function renderAgentContacts() {
    if (!agentContactsEl) return;
    agentContactsEl.innerHTML = '';
    const query = (chatSearchInput?.value || '').toLowerCase();
    if (!state.agents.length) {
      agentContactsEl.innerHTML = '<div class="px-4 py-2 text-muted small">No agents</div>';
      return;
    }
    const grouped = {};
    state.agents.forEach(a => {
      const letter = (a.display_name || '').charAt(0).toUpperCase() || '#';
      if (!grouped[letter]) grouped[letter] = [];
      grouped[letter].push(a);
    });
    Object.keys(grouped).sort().forEach(letter => {
      const ulId = `contact-sort-${letter}`;
      const block = document.createElement('div');
      block.className = 'mt-3';
      block.innerHTML = `
        <div class="contact-list-title">${letter}</div>
        <ul id="${ulId}" class="list-unstyled contact-list"></ul>
      `;
      agentContactsEl.appendChild(block);
      const ul = block.querySelector('ul');
      grouped[letter].forEach(a => {
        if (query) {
          const name = (a.display_name || '').toLowerCase();
          const email = (a.email || '').toLowerCase();
          if (!name.includes(query) && !email.includes(query)) return;
        }
        const li = document.createElement('li');
        const avatar = a.avatar_url || defaultAvatarMale;
        li.innerHTML = `
          <div class="d-flex align-items-center contact-item" data-agent-id="${a.id}">
            <div class="flex-shrink-0 me-2">
              <div class="avatar-xxs">
                <img src="${avatar}" class="img-fluid rounded-circle" alt="">
              </div>
            </div>
            <div class="flex-grow-1">
              <p class="text-truncate contactlist-name mb-0">${escapeHtml(a.display_name || 'Agent')}</p>
            </div>
          </div>
        `;
        li.addEventListener('click', () => startDirectWithAgent(a.id));
        ul.appendChild(li);
      });
    });
  }

  function populateAgentSelects() {
    if (newChatAgentSel) {
      newChatAgentSel.innerHTML = '';
      state.agents.forEach(a => {
        if (a.id === currentAgentId) return;
        const opt = document.createElement('option');
        opt.value = a.id;
        opt.textContent = a.display_name || `Agent #${a.id}`;
        newChatAgentSel.appendChild(opt);
      });
    }
    if (newGroupAgentsSel) {
      newGroupAgentsSel.innerHTML = '';
      state.agents.forEach(a => {
        if (a.id === currentAgentId) return;
        const opt = document.createElement('option');
        opt.value = a.id;
        opt.textContent = a.display_name || `Agent #${a.id}`;
        newGroupAgentsSel.appendChild(opt);
      });
    }
  }

  function loadAgents() {
    const url = (typeof apiEndpoints === 'function') ? apiEndpoints().agentsIndex : null;
    if (!url) return;
    if (agentListLoader) agentListLoader.classList.remove('d-none');
    fetchJsonWithTimeout(url)
      .then(data => {
        state.agents = data.data || [];
        renderAgentContacts();
        populateAgentSelects();
      })
      .catch(() => {
        if (agentContactsEl) agentContactsEl.innerHTML = '<div class="px-4 py-2 text-muted small">Unable to load agents</div>';
      })
      .finally(() => {
        if (agentListLoader) agentListLoader.classList.add('d-none');
      });
  }

  function findExistingDm(agentId) {
    return state.threads.find(t => {
      const participants = t.participants || [];
      if (participants.length !== 2) return false;
      const ids = participants.map(p => parseInt(p.agent_id, 10));
      return ids.includes(agentId) && ids.includes(currentAgentId);
    });
  }

  function startDirectWithAgent(agentId) {
    if (!agentId) return;
    const existing = findExistingDm(parseInt(agentId, 10));
    if (existing) {
      state.pendingDmAgentId = null;
      openThread(existing.id, 'dm');
      return;
    }
    const targetId = parseInt(agentId, 10);
    const targetAgent = (state.agents || []).find(a => parseInt(a.id, 10) === targetId) || null;
    state.pendingDmAgentId = targetId;
    state.currentThreadId = null;
    state.currentType = 'dm';
    clearAttachments();
    toggleMobileChat(true, 'dm');
    showChatPane('dm');
    toggleChatInput(true);
    if (usersConversationEl) {
      usersConversationEl.innerHTML = '<li class="text-muted small px-2 py-2">No messages yet. Send a message to start this conversation.</li>';
    }
    setActiveTopbar(targetAgent ? {
      id: null,
      participants: [
        { agent_id: currentAgentId, display_name: 'You' },
        { agent_id: targetId, display_name: targetAgent.display_name || 'Direct Message', avatar_url: targetAgent.avatar_url || null }
      ]
    } : null, 'dm');
  }

  async function createDirectThread(agentId) {
    const res = await fetch(threadsUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({
        participant_ids: [agentId]
      })
    });
    const data = await res.json().catch(() => ({}));
    if (!res.ok) {
      throw new Error(data.error || 'Unable to start conversation');
    }
    const threadId = parseInt(data.id, 10);
    if (!threadId) throw new Error('Conversation was created without a valid thread id');
    return threadId;
  }

  function renderMessages(messages, type) {
    const container = type === 'channel' ? channelConversationEl : usersConversationEl;
    if (!container) return;
    const urlRegex = /((https?:\/\/|www\.)[^\s<]+)/gi;
    const linkify = (text) => {
      const raw = text || '';
      return escapeHtml(raw).replace(urlRegex, (match) => {
        const href = match.startsWith('http') ? match : `http://${match}`;
        return `<a href="${href}" target="_blank" rel="noopener noreferrer">${escapeHtml(match)}</a>`;
      }).replace(/\n/g, '<br>');
    };
    container.innerHTML = '';
    messages.forEach(msg => {
      const mine = currentAgentId && msg.sender_agent_id && parseInt(msg.sender_agent_id, 10) === currentAgentId;
      const side = mine ? 'right' : 'left';
      const senderMeta = resolveSenderMeta(msg.sender_agent_id, state.currentThreadId);
      const name = senderMeta.display_name || msg.sender_name || 'Agent';
      const avatar = resolveAvatar(senderMeta);
      const safeName = escapeHtml(name);
      const li = document.createElement('li');
      li.className = `chat-list ${side}`;
      const meta = normalizeMeta(msg.metadata);
      const attachments = Array.isArray(meta.attachments) ? meta.attachments.filter(a => a && (a.url || a.public_url || a.rel_path || a.path)) : [];
      let safeMsg = linkify(msg.message || '');
      const buildAttachment = (att) => {
        const attName = escapeHtml(att.name || att.orig_name || att.stored_name || 'Attachment');
        const dl = downloadUrl && (att.path || att.rel_path)
          ? `${downloadUrl}?thread_id=${encodeURIComponent(msg.thread_id || state.currentThreadId || '')}&path=${encodeURIComponent(att.path || att.rel_path)}`
          : '';
        const url = att.url || att.public_url || dl || (att.rel_path ? ('storage/' + att.rel_path) : (att.path || ''));
        if (!url) return '';
        const view = typeof window.resolveFileViewUrl === 'function' ? window.resolveFileViewUrl(attName, url, url) : { url, type: 'other' };
        const icon = view.type === 'image' ? 'ri-image-2-line'
          : view.type === 'pdf' ? 'ri-file-pdf-line'
          : view.type === 'office' ? 'ri-file-word-2-line'
          : 'ri-attachment-2';
        return `<div class="d-flex align-items-center gap-2 small bg-light rounded px-2 py-1 mb-1">
          <i class="${icon}"></i>
          <a href="${view.url || url}" target="_blank" rel="noopener noreferrer">${attName}</a>
        </div>`;
      };
      const attachmentsHtml = attachments.map(buildAttachment).join('');
      if (!safeMsg && attachmentsHtml) {
        safeMsg = '<span class="text-muted">Attachment</span>';
      }
      const nameLine = type === 'channel' ? `<div class="text-muted small mb-1">${safeName}</div>` : '';
      li.innerHTML = `
        <div class="conversation-list">
          <div class="user-chat-content">
            <div class="d-flex ${mine ? 'justify-content-end' : 'justify-content-start'} align-items-start">
              ${mine ? '' : `<div class="me-2"><img src="${avatar}" class="rounded-circle avatar-xs" alt=""></div>`}
              <div class="ctext-wrap">
                <div class="ctext-wrap-content">
                  ${nameLine}
                  <p class="mb-0 ctext-content">${safeMsg}</p>
                  ${attachmentsHtml ? `<div class="mt-2">${attachmentsHtml}</div>` : ''}
                  <div class="text-muted small mt-1">${msg.created_at || ''}</div>
                </div>
                <div class="dropdown align-self-start message-box-drop">
                  <a class="dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                    <i class="ri-more-2-fill"></i>
                  </a>
                  <div class="dropdown-menu">
                    <a class="dropdown-item copy-message" href="#"><i class="ri-file-copy-line me-2 text-muted align-bottom"></i>Copy</a>
                  </div>
                </div>
              </div>
              ${mine ? `<div class="ms-2"><img src="${avatar}" class="rounded-circle avatar-xs" alt=""></div>` : ''}
            </div>
          </div>
        </div>
      `;
      li.querySelectorAll('.copy-message').forEach(btn => {
        btn.addEventListener('click', (ev) => {
          ev.preventDefault();
          ev.stopPropagation();
          const rawText = (msg && typeof msg.message === 'string') ? msg.message : '';
          copyTextToClipboard(rawText || '');
          if (copyToast) copyToast.style.display = 'block';
          if (copyToastChannel) copyToastChannel.style.display = 'block';
          setTimeout(() => {
            if (copyToast) copyToast.style.display = 'none';
            if (copyToastChannel) copyToastChannel.style.display = 'none';
          }, 1000);
        });
      });
      container.appendChild(li);
    });
    const wrapper = container.closest('.simplebar-content-wrapper');
    if (wrapper) {
      wrapper.scrollTop = wrapper.scrollHeight;
    } else {
      container.scrollTop = container.scrollHeight;
    }
  }

  function setActiveTopbar(thread, type) {
    const name = thread ? (type === 'dm'
      ? ((thread.participants || []).find(p => parseInt(p.agent_id, 10) !== currentAgentId)?.display_name || 'Direct Message')
      : (thread.topic || 'Channel')) : '';
    const avatar = thread && type === 'dm'
      ? resolveAvatar((thread.participants || []).find(p => parseInt(p.agent_id, 10) !== currentAgentId) || {})
      : (thread && type === 'channel' ? defaultAvatarMale : null);
    if (type === 'dm') {
      if (userTopbarName) userTopbarName.textContent = name || 'Direct Message';
      if (userStatusEl) userStatusEl.textContent = '';
      if (userAvatarImg) {
        if (avatar) {
          userAvatarImg.style.display = '';
          userAvatarImg.src = avatar;
        } else {
          userAvatarImg.style.display = 'none';
        }
      }
      const userChat = document.getElementById('users-chat');
      const channelChat = document.getElementById('channel-chat');
      if (userChat) userChat.style.display = 'block';
      if (channelChat) channelChat.style.display = 'none';
    } else {
      if (channelTopbarName) channelTopbarName.textContent = name || 'Channel';
      if (channelStatusEl) {
        const creatorId = thread ? parseInt(thread.created_by_agent_id, 10) : null;
        const preview = formatParticipantPreview(thread?.participants || [], creatorId, 4);
        channelStatusEl.textContent = preview;
        channelStatusEl.style.width = '100%';
        channelStatusEl.style.display = 'block';
        channelStatusEl.style.whiteSpace = 'normal';
        channelStatusEl.style.wordBreak = 'break-word';
        channelStatusEl.style.overflowWrap = 'break-word';
      }
      if (channelAvatarImg) {
        if (avatar) {
          channelAvatarImg.style.display = '';
          channelAvatarImg.src = avatar;
        } else {
          channelAvatarImg.style.display = 'none';
        }
      }
      const userChat = document.getElementById('users-chat');
      const channelChat = document.getElementById('channel-chat');
      if (userChat) userChat.style.display = 'none';
      if (channelChat) channelChat.style.display = 'block';
    }
    updateActionVisibility(thread);
    populateInfoPanel(thread);
  }

  function updateActionVisibility(thread) {
    const isCreator = thread && parseInt(thread.created_by_agent_id, 10) === currentAgentId;
    deleteActions.forEach(el => { el.style.display = isCreator ? '' : 'none'; });
    removeActions.forEach(el => { el.style.display = isCreator ? '' : 'none'; });
    addActions.forEach(el => { el.style.display = isCreator ? '' : 'none'; });
    leaveActions.forEach(el => { el.style.display = (!isCreator && thread) ? '' : 'none'; });
  }

  function populateInfoPanel(thread) {
    if (!infoName || !infoAdmin || !infoCreated || !infoCount || !infoMembers) return;
    if (!thread) {
      infoName.textContent = '-';
      infoAdmin.textContent = '-';
      infoCreated.textContent = '-';
      infoCount.textContent = '-';
      infoMembers.innerHTML = '';
      return;
    }
    infoName.textContent = thread.topic || ('Thread #' + thread.id);
    const creatorId = parseInt(thread.created_by_agent_id, 10);
    const adminPart = (thread.participants || []).find(p => parseInt(p.agent_id, 10) === creatorId);
    infoAdmin.textContent = adminPart ? getThreadParticipantLabel(adminPart, creatorId) : 'Admin';
    infoCreated.textContent = thread.created_at ? new Date(thread.created_at).toLocaleString() : '-';
    const participants = Array.isArray(thread.participants) ? thread.participants : [];
    infoCount.textContent = String(participants.length || 0);
    infoMembers.innerHTML = '';
    if (!participants.length) {
      infoMembers.innerHTML = '<div class="group-info-empty">No members found.</div>';
      return;
    }
    participants.forEach(p => {
      const chip = document.createElement('div');
      chip.className = 'group-member-chip';
      const label = document.createElement('span');
      label.className = 'text-truncate';
      label.textContent = getThreadParticipantLabel(p, creatorId);
      chip.appendChild(label);
      if (creatorId && parseInt(p.agent_id, 10) === creatorId) {
        const badge = document.createElement('span');
        badge.className = 'badge bg-primary-subtle text-primary';
        badge.textContent = 'Admin';
        chip.appendChild(badge);
      }
      infoMembers.appendChild(chip);
    });
  }

  function loadMessages(threadId, type, opts = {}) {
    const { silent = false } = opts;
    messagesLoading = true;
    if (!silent) showLoader(true, type);
    toggleMobileChat(true, type);
    showChatPane(type);
    fetchJsonWithTimeout(`${messagesUrl}?thread_id=${encodeURIComponent(threadId)}`)
      .then(data => {
        renderMessages(data.data || [], type);
      })
      .catch(() => {
        const container = type === 'channel' ? channelConversationEl : usersConversationEl;
        if (container && !container.children.length) {
          container.innerHTML = '<li class="text-muted small px-2">Unable to load messages.</li>';
        }
      })
      .finally(() => {
        messagesLoading = false;
        showLoader(false, type);
        // live bar disabled
      });
  }

  function openThread(threadId, type) {
    showLoader(true, type);
    state.pendingDmAgentId = null;
    state.currentThreadId = threadId;
    state.currentType = type;
    const thread = state.threads.find(t => parseInt(t.id, 10) === parseInt(threadId, 10));
    setActiveTopbar(thread, type);
    clearAttachments();
    toggleMobileChat(true, type);
    showChatPane(type);
    toggleChatInput(true);
    loadMessages(threadId, type);
  }

  function loadThreads(opts = {}) {
    const { resetMobile = false } = opts;
    if (threadsLoading) return;
    threadsLoading = true;
    if (resetMobile) toggleMobileChat(false);
    fetchJsonWithTimeout(threadsUrl)
      .then(data => {
        state.threads = data.data || [];
        try {
          renderThreadLists(state.threads);
          if (userListEl && channelListEl) {
            const dmCount = userListEl.querySelectorAll('li').length;
            const chCount = channelListEl.querySelectorAll('li').length;
            if (dmCount === 0 && chCount === 0) {
              userListEl.innerHTML = '<li class="px-3 text-muted small">No chats yet. Start one from Users.</li>';
            }
          }
        } catch (_e) {
          if (userListEl) userListEl.innerHTML = '<li class="px-3 text-muted small">Unable to render chats</li>';
          if (channelListEl) channelListEl.innerHTML = '';
        }
        updateActionVisibility(state.threads.find(t => parseInt(t.id, 10) === parseInt(state.currentThreadId || 0, 10)));
        if (!state.currentThreadId && state.threads.length) {
          // do not auto-select; wait for user click
          toggleChatInput(false);
        } else if (!state.threads.length) {
          if (usersConversationEl) usersConversationEl.innerHTML = '';
          if (channelConversationEl) channelConversationEl.innerHTML = '';
          if (userTopbarName) userTopbarName.textContent = 'No conversation';
          if (channelTopbarName) channelTopbarName.textContent = 'No channel';
          if (userAvatarImg) userAvatarImg.style.display = 'none';
          if (channelAvatarImg) channelAvatarImg.style.display = 'none';
          toggleChatInput(false);
        }
      })
      .catch(() => {
        if (userListEl) userListEl.innerHTML = '<li class="px-3 text-muted small">Unable to load chats</li>';
        if (channelListEl) channelListEl.innerHTML = '';
      })
      .finally(() => {
        threadsLoading = false;
        clearBlockingLoaders();
      });
  }

  if (chatForm) {
    chatForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      if ((!state.currentThreadId && !state.pendingDmAgentId) || uploading) return;
      const msg = (chatInput ? chatInput.value : '').trim();
      const hasAttachments = pendingUploads.length > 0;
      if (!msg && !hasAttachments) return;

      if (sendBtn) sendBtn.setAttribute('disabled', 'disabled');
      try {
        if (!state.currentThreadId && state.pendingDmAgentId) {
          const newThreadId = await createDirectThread(state.pendingDmAgentId);
          state.currentThreadId = newThreadId;
          state.currentType = 'dm';
          state.pendingDmAgentId = null;
          await loadThreads();
        }
        const payload = {
          thread_id: state.currentThreadId,
          message: msg,
        };
        if (hasAttachments) {
          payload.metadata = {
            attachments: pendingUploads.map(att => ({
              name: att.name || att.orig_name || att.stored_name || 'Attachment',
              url: att.url || '',
              path: att.rel_path || att.path || '',
              mime: att.mime || '',
              size: att.size || null,
            }))
          };
        }
        await fetch(messagesUrl, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'same-origin',
          body: JSON.stringify(payload)
        }).then(async (r) => {
          const data = await r.json().catch(() => ({}));
          if (!r.ok || data?.error) {
            throw new Error(data?.error || 'Unable to send message');
          }
          return data;
        });
        if (chatInput) chatInput.value = '';
        clearAttachments();
        loadMessages(state.currentThreadId, state.currentType);
        loadThreads();
      } catch (err) {
        window.crmUiAlert('Unable to send message.');
      } finally {
        if (sendBtn) sendBtn.removeAttribute('disabled');
      }
    });
  }

  if (emojiBtn) {
    emojiBtn.addEventListener('click', (e) => {
      e.preventDefault();
      ensureEmojiPanel();
      toggleEmojiPanel();
    });
  }

  if (attachBtn && attachInput) {
    attachBtn.addEventListener('click', (e) => {
      e.preventDefault();
      attachInput.click();
    });
    attachInput.addEventListener('change', (e) => {
      const files = e.target.files;
      if (files && files.length) {
        uploadFiles(files);
      }
    });
  }

  if (attachmentsWrap) {
    attachmentsWrap.addEventListener('click', (e) => {
      const rm = e.target.closest('.remove-attach');
      if (!rm) return;
      const idx = parseInt(rm.getAttribute('data-idx'), 10);
      if (Number.isInteger(idx)) {
        pendingUploads.splice(idx, 1);
        refreshAttachmentsUI();
      }
    });
  }

  // Poll every 10s to simulate live chat updates
  function refreshCurrentThread() {
    if (!state.currentThreadId) return;
    if (document.hidden) return;
    if (threadsLoading || messagesLoading) return;
    loadThreads();
    loadMessages(state.currentThreadId, state.currentType, { silent: true, isRefresh: true });
  }
  const chatRefreshInterval = window.appConfig?.chatRefreshMs ?? 60000;
  const chatRefreshHiddenInterval = window.appConfig?.chatRefreshHiddenMs ?? 300000;
  let chatRefreshTimer = null;
  function stopChatPolling() {
    if (chatRefreshTimer) {
      clearInterval(chatRefreshTimer);
      chatRefreshTimer = null;
    }
  }
  function startChatPolling() {
    stopChatPolling();
    const rt = window.DopRealtime;
    if (rt && rt.isConnected && rt.isConnected()) return;
    const interval = document.hidden ? chatRefreshHiddenInterval : chatRefreshInterval;
    chatRefreshTimer = setInterval(refreshCurrentThread, interval);
  }
  startChatPolling();

  // Real-time: use shared DopRealtime (provider-agnostic — Pusher or Ably).
  // DopRealtime is initialised by footer_logged.php which loads after this script,
  // so we defer by listening for realtime:connected if it isn't ready yet.
  (function initChatRealtime() {
    const dot = document.getElementById('chatConnDot');
    function setConn(live) {
      if (!dot) return;
      dot.style.background = live ? '#22c55e' : '#94a3b8';
      dot.title = live ? 'Real-time' : 'Polling for updates';
    }
    function hookRealtime(rt) {
      rt.on('new-message', function () {
        loadThreads();
        if (state.currentThreadId) {
          loadMessages(state.currentThreadId, state.currentType, { silent: true, isRefresh: true });
        }
      });
      rt.on('presence-update', function () {
        loadThreads();
      });
      if (rt.isConnected()) setConn(true);
    }
    document.addEventListener('realtime:connected',    function () { setConn(true); startChatPolling();  });
    document.addEventListener('realtime:disconnected', function () { setConn(false); startChatPolling(); });
    document.addEventListener('realtime:failed',       function () { setConn(false); startChatPolling(); });
    if (window.DopRealtime) {
      hookRealtime(window.DopRealtime);
    } else {
      document.addEventListener('realtime:connected', function () {
        if (window.DopRealtime) hookRealtime(window.DopRealtime);
      }, { once: true });
    }
  })();

  document.addEventListener('visibilitychange', function () {
    startChatPolling();
    if (!document.hidden && state.currentThreadId) {
      refreshCurrentThread();
    }
  });

  // bind search to filter chats/agents
  if (chatSearchInput) {
    chatSearchInput.addEventListener('input', () => {
      renderThreadLists(state.threads);
      renderAgentContacts();
    });
  }

  loadThreads({ resetMobile: true });
  loadAgents();
  toggleChatInput(false);
  clearBlockingLoaders();

  // New chat buttons
  function showModal(el) {
    if (!el || typeof bootstrap === 'undefined') return;
    const modal = bootstrap.Modal.getOrCreateInstance(el);
    modal.show();
    document.body.classList.remove('user-chat-show');
  }

  function openAgentsTab() {
    if (newChatModalEl && typeof bootstrap !== 'undefined') {
      bootstrap.Modal.getOrCreateInstance(newChatModalEl).show();
      return;
    }
    if (contactsTabLink && typeof bootstrap !== 'undefined') {
      bootstrap.Tab.getOrCreateInstance(contactsTabLink).show();
      if (agentContactsEl && typeof agentContactsEl.scrollIntoView === 'function') {
        agentContactsEl.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }

  if (btnNewDirect) btnNewDirect.addEventListener('click', openAgentsTab);
  if (btnNewDirect2) btnNewDirect2.addEventListener('click', openAgentsTab);
  if (btnNewGroup) btnNewGroup.addEventListener('click', () => showModal(newGroupModalEl));

  if (startDirectChatBtn) {
    startDirectChatBtn.addEventListener('click', () => {
      const agentId = newChatAgentSel ? parseInt(newChatAgentSel.value, 10) : null;
      if (!agentId) return;
      startDirectWithAgent(agentId);
      if (newChatModalEl && typeof bootstrap !== 'undefined') {
        bootstrap.Modal.getOrCreateInstance(newChatModalEl).hide();
      }
    });
  }

  if (createGroupChatBtn) {
    createGroupChatBtn.addEventListener('click', () => {
      const topic = newGroupTopicInput ? newGroupTopicInput.value.trim() : '';
      const selected = Array.from(newGroupAgentsSel ? newGroupAgentsSel.selectedOptions : []).map(o => parseInt(o.value, 10)).filter(Boolean);
      if (!topic) {
        if (typeof window.crmUiAlert === 'function') {
          window.crmUiAlert('Please enter a group name before creating the group.', 'Create Group', {
            type: 'warning',
            stackedBackdrop: true
          });
        }
        if (newGroupTopicInput) newGroupTopicInput.focus();
        return;
      }
      if (!selected.length) {
        if (typeof window.crmUiAlert === 'function') {
          window.crmUiAlert('Please select at least one other agent to create the group.', 'Create Group', {
            type: 'warning',
            stackedBackdrop: true
          });
        }
        return;
      }
      toggleButtonLoading(createGroupChatBtn, true, 'Creating...');
      fetch(threadsUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify({
          topic: topic || null,
          participant_ids: selected
        })
      })
        .then(r => {
          if (!r.ok) throw new Error('create group failed');
          return r.json();
        })
        .then(() => {
          if (newGroupModalEl && typeof bootstrap !== 'undefined') {
            bootstrap.Modal.getOrCreateInstance(newGroupModalEl).hide();
          }
          if (newGroupTopicInput) newGroupTopicInput.value = '';
          if (newGroupAgentsSel) newGroupAgentsSel.selectedIndex = -1;
          loadThreads();
        })
        .catch(() => {})
        .finally(() => toggleButtonLoading(createGroupChatBtn, false));
    });
  }

  function deleteCurrentThread() {
    if (!state.currentThreadId) return;
    const thread = state.threads.find(t => parseInt(t.id, 10) === parseInt(state.currentThreadId, 10));
    const label = thread?.topic || 'this chat';
    const doDelete = async () => {
      fetch(`${threadsUrl}?id=${encodeURIComponent(state.currentThreadId)}`, { method: 'DELETE', credentials: 'same-origin' })
      .then(r => {
        if (!r.ok) throw new Error('delete failed');
        return r.json();
      })
      .then(() => {
        state.currentThreadId = null;
        state.currentType = 'dm';
        loadThreads();
        if (usersConversationEl) usersConversationEl.innerHTML = '';
        if (channelConversationEl) channelConversationEl.innerHTML = '';
        if (userTopbarName) userTopbarName.textContent = 'Select conversation';
        if (channelTopbarName) channelTopbarName.textContent = 'Select channel';
        toggleChatInput(false);
      })
      .catch(() => {});
    };
    if (typeof window.crmUiConfirm === 'function') {
      window.crmUiConfirm(`Delete ${label}? This will remove the group chat and all messages.`, 'Delete Group', {
        okText: 'Delete',
        cancelText: 'Cancel',
        variant: 'danger',
        stackedBackdrop: true
      }).then(ok => {
        if (ok) doDelete();
      });
      return;
    }
    doDelete();
  }

  document.querySelectorAll('.delete-thread').forEach(el => {
    el.addEventListener('click', (e) => {
      e.preventDefault();
      deleteCurrentThread();
    });
  });

  function leaveCurrentThread() {
    if (!state.currentThreadId) return;
    // handled via modal confirm
    fetch(threadsUrl, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'same-origin',
      body: JSON.stringify({ id: state.currentThreadId, action: 'leave' })
    })
      .then(r => {
        if (!r.ok) throw new Error('leave failed');
        return r.json();
      })
      .then(() => {
        state.currentThreadId = null;
        loadThreads();
        if (usersConversationEl) usersConversationEl.innerHTML = '';
        if (channelConversationEl) channelConversationEl.innerHTML = '';
        if (userTopbarName) userTopbarName.textContent = 'Select conversation';
        if (channelTopbarName) channelTopbarName.textContent = 'Select channel';
        toggleChatInput(false);
      })
      .catch(() => {});
  }

  function removeMemberFromThread() {
    if (!state.currentThreadId) return;
    const thread = state.threads.find(t => parseInt(t.id, 10) === parseInt(state.currentThreadId, 10));
    if (!thread || parseInt(thread.created_by_agent_id, 10) !== currentAgentId) {
      return;
    }
    if (!removeMemberModalEl || !removeMemberListEl) return;
    removeMemberListEl.innerHTML = '';
    const members = (thread.participants || []).filter(p => parseInt(p.agent_id, 10) !== currentAgentId);
    if (!members.length) {
      if (removeMemberEmptyEl) removeMemberEmptyEl.classList.remove('d-none');
    } else {
      if (removeMemberEmptyEl) removeMemberEmptyEl.classList.add('d-none');
      members.forEach(m => {
        const item = document.createElement('div');
        item.className = 'list-group-item d-flex justify-content-between align-items-center';
        item.innerHTML = `<span>${m.display_name || ('Agent #' + m.agent_id)}</span>
          <button type="button" class="btn btn-sm btn-outline-danger remove-member-btn" data-agent-id="${m.agent_id}"><i class="ri-user-unfollow-line me-1"></i>Remove</button>`;
        removeMemberListEl.appendChild(item);
      });
    }
    if (typeof bootstrap !== 'undefined') {
      bootstrap.Modal.getOrCreateInstance(removeMemberModalEl).show();
    }
  }

  if (removeMemberListEl) {
    removeMemberListEl.addEventListener('click', async (e) => {
      const btn = e.target.closest('.remove-member-btn');
      if (!btn) return;
      const targetId = parseInt(btn.getAttribute('data-agent-id'), 10);
      if (!targetId) return;
      const ok = await window.crmUiConfirm('Remove this member from the group?', 'Remove Member', {
        okText: 'Remove',
        cancelText: 'Cancel',
        variant: 'danger',
        icon: 'warning',
        stackedBackdrop: true
      });
      if (!ok) return;
      fetch(threadsUrl, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify({ id: state.currentThreadId, action: 'remove_member', target_agent_id: targetId })
      })
        .then(r => {
          if (!r.ok) throw new Error('remove failed');
          return r.json();
        })
        .then(() => {
          loadThreads();
          if (typeof bootstrap !== 'undefined' && removeMemberModalEl) {
            bootstrap.Modal.getOrCreateInstance(removeMemberModalEl).hide();
          }
        })
        .catch(() => {});
    });
  }

  document.querySelectorAll('.leave-thread').forEach(el => {
    el.addEventListener('click', (e) => {
      e.preventDefault();
      const thread = state.threads.find(t => parseInt(t.id, 10) === parseInt(state.currentThreadId || 0, 10));
      // Only show leave for non-creators; creators should delete instead.
      if (thread && parseInt(thread.created_by_agent_id, 10) === currentAgentId) return;
      if (leaveGroupModalEl && typeof bootstrap !== 'undefined') {
        bootstrap.Modal.getOrCreateInstance(leaveGroupModalEl).show();
      } else {
        leaveCurrentThread();
      }
    });
  });

  if (leaveGroupConfirmBtn) {
    leaveGroupConfirmBtn.addEventListener('click', () => {
      const sp = leaveGroupConfirmBtn.querySelector('.spinner-border');
      if (sp) sp.classList.remove('d-none');
      leaveGroupConfirmBtn.disabled = true;
      leaveCurrentThread();
      setTimeout(() => {
        if (leaveGroupModalEl && typeof bootstrap !== 'undefined') {
          bootstrap.Modal.getOrCreateInstance(leaveGroupModalEl).hide();
        }
        if (sp) sp.classList.add('d-none');
        leaveGroupConfirmBtn.disabled = false;
      }, 400);
    });
  }

  document.querySelectorAll('.remove-member-thread').forEach(el => {
    el.addEventListener('click', (e) => {
      e.preventDefault();
      removeMemberFromThread();
    });
  });

  document.querySelectorAll('.add-member-thread').forEach(el => {
    el.addEventListener('click', (e) => {
      e.preventDefault();
      const thread = state.threads.find(t => parseInt(t.id, 10) === parseInt(state.currentThreadId || 0, 10));
      if (!thread || parseInt(thread.created_by_agent_id, 10) !== currentAgentId) return;
      if (!addMemberModalEl || !addMemberSelect) return;
      addMemberSelect.innerHTML = '<option value="">-- Select agent --</option>';
      const memberIds = new Set((thread.participants || []).map(p => parseInt(p.agent_id, 10)));
      let added = 0;
      (state.agents || []).forEach(a => {
        const aid = parseInt(a.id, 10);
        if (!aid || memberIds.has(aid)) return;
        const opt = document.createElement('option');
        opt.value = aid;
        opt.textContent = a.display_name || ('Agent #' + aid);
        addMemberSelect.appendChild(opt);
        added++;
      });
      if (addMemberNone) addMemberNone.classList.toggle('d-none', added > 0);
      if (typeof bootstrap !== 'undefined') {
        bootstrap.Modal.getOrCreateInstance(addMemberModalEl).show();
      }
    });
  });

  if (addMemberBtn && addMemberSelect) {
    addMemberBtn.addEventListener('click', () => {
      const targetId = parseInt(addMemberSelect.value, 10);
      if (!targetId) return;
      const sp = addMemberBtn.querySelector('.spinner-border');
      if (sp) sp.classList.remove('d-none');
      addMemberBtn.disabled = true;
      fetch(threadsUrl, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify({ id: state.currentThreadId, action: 'add_member', target_agent_id: targetId })
      })
        .then(r => {
          if (!r.ok) throw new Error('add failed');
          return r.json();
        })
        .then(() => {
          loadThreads();
          if (typeof bootstrap !== 'undefined' && addMemberModalEl) {
            bootstrap.Modal.getOrCreateInstance(addMemberModalEl).hide();
          }
        })
        .catch(() => {})
        .finally(() => {
          if (sp) sp.classList.add('d-none');
          addMemberBtn.disabled = false;
        });
    });
  }

  if (infoBtn && infoModal) {
    infoBtn.addEventListener('click', () => {
      const thread = state.threads.find(t => parseInt(t.id, 10) === parseInt(state.currentThreadId || 0, 10));
      populateInfoPanel(thread);
      infoModal.show();
    });
  }

  // Move modals to body to avoid overlay/z-index issues
  ['removeMemberModal', 'leaveGroupModal', 'userProfileCanvasExample', 'addMemberModal'].forEach(id => {
    const el = document.getElementById(id);
    if (el && el.parentElement !== document.body) {
      document.body.appendChild(el);
    }
  });

  // Prevent placeholder dropdown actions from navigating
  document.querySelectorAll('.archive-thread, .mute-thread').forEach(el => {
    el.addEventListener('click', (e) => e.preventDefault());
  });

  // Close chat on mobile
  document.querySelectorAll('.user-chat-remove').forEach(el => {
    el.addEventListener('click', (e) => {
      e.preventDefault();
      toggleMobileChat(false);
    });
  });

  function toggleMobileChat(show, type) {
    const isMobile = window.innerWidth <= 767;
    if (!isMobile) {
      // Desktop: always keep sidebar visible and chat pane inline
      document.body.classList.remove('user-chat-show');
      if (userChatWrapper) {
        userChatWrapper.classList.remove('user-chat-show');
        userChatWrapper.style.display = '';
      }
      if (chatLeftSidebar) chatLeftSidebar.style.display = '';
      showChatPane(type);
      return;
    }
    if (show) {
      document.body.classList.add('user-chat-show');
      if (userChatWrapper) {
        userChatWrapper.classList.add('user-chat-show');
        userChatWrapper.style.display = 'block';
      }
      if (chatLeftSidebar) chatLeftSidebar.style.display = 'none';
      showChatPane(type);
    } else {
      document.body.classList.remove('user-chat-show');
      if (userChatWrapper) {
        userChatWrapper.classList.remove('user-chat-show');
        userChatWrapper.style.display = '';
      }
      if (chatLeftSidebar) chatLeftSidebar.style.display = '';
      showChatPane();
    }
  }

  // Ensure layout resets if viewport is resized to desktop while sidebar was hidden
  window.addEventListener('resize', () => {
    if (window.innerWidth > 767) {
      document.body.classList.remove('user-chat-show');
      if (userChatWrapper) {
        userChatWrapper.classList.remove('user-chat-show');
        userChatWrapper.style.display = '';
      }
      if (chatLeftSidebar) chatLeftSidebar.style.display = '';
      showChatPane(state.currentType);
    }
  });

  function showChatPane(type) {
    const userChat = document.getElementById('users-chat');
    const channelChat = document.getElementById('channel-chat');
    if (type === 'channel') {
      if (userChat) userChat.style.display = 'none';
      if (channelChat) channelChat.style.display = 'block';
    } else {
      if (userChat) userChat.style.display = 'block';
      if (channelChat) channelChat.style.display = 'none';
    }
  }

})();
