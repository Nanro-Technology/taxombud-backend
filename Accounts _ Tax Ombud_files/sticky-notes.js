(function () {
  let stickyNotesBooted = false;
  function bootStickyNotes() {
    if (stickyNotesBooted) return;
    const widget = document.getElementById('stickyNotesWidget');
    if (!widget) return;
    stickyNotesBooted = true;

    const launcher = document.getElementById('stickyNotesLauncher');
  const panelEl = document.getElementById('stickyNotesPanel');
  const closeBtn = document.getElementById('stickyNotesCloseBtn');
  if (!launcher || !panelEl) return;

  const alertEl = document.getElementById('stickyNotesAlert');
  const listWrap = document.getElementById('stickyNotesListWrap');
  const listEl = document.getElementById('stickyNotesList');
  const newBtn = document.getElementById('stickyNotesNewBtn');
  const hideBtn = document.getElementById('stickyNotesHideBtn');
  const editorWrap = document.getElementById('stickyNotesEditorWrap');
  const backBtn = document.getElementById('stickyNotesBackBtn');
  const titleInput = document.getElementById('stickyNotesTitle');
  const bodyInput = document.getElementById('stickyNotesBody');
  const pinnedInput = document.getElementById('stickyNotesPinned');
  const deleteBtn = document.getElementById('stickyNotesDeleteBtn');
  const saveState = document.getElementById('stickyNotesSaveState');

  const userId = String(widget.getAttribute('data-user-id') || '').trim();
  const canManage = String(widget.getAttribute('data-can-manage') || '0') === '1';
  const apiBase = String(widget.getAttribute('data-api') || '').trim();
  if (!apiBase || !userId) return;

  const posKey = 'sticky_notes_pos_' + userId;
  const lastKey = 'sticky_notes_last_' + userId;
  const hiddenKey = 'sticky_notes_hidden_' + userId;
  const viewportPad = 24;

  let notes = [];
  let currentId = null;
  let saveTimer = null;
  let lastBody = '';
  let lastTitle = '';
  let lastPinned = false;

  if (!canManage) {
    if (titleInput) titleInput.setAttribute('disabled', 'disabled');
    if (bodyInput) bodyInput.setAttribute('disabled', 'disabled');
    if (pinnedInput) pinnedInput.setAttribute('disabled', 'disabled');
    if (deleteBtn) deleteBtn.classList.add('d-none');
  }

  function esc(v) {
    return String(v == null ? '' : v).replace(/[&<>"']/g, function (ch) {
      return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[ch] || ch;
    });
  }

  function asPinned(value) {
    if (value === true || value === 1 || value === '1') return 1;
    if (typeof value === 'string' && value.toLowerCase() === 'true') return 1;
    return 0;
  }

  function showAlert(msg) {
    if (!alertEl) return;
    if (!msg) {
      alertEl.classList.add('d-none');
      alertEl.textContent = '';
      return;
    }
    alertEl.textContent = msg;
    alertEl.classList.remove('d-none');
  }

  function setHidden(hidden) {
    const isHidden = !!hidden;
    try {
      localStorage.setItem(hiddenKey, isHidden ? '1' : '0');
    } catch (e) {
      // ignore storage failures
    }
    if (isHidden) {
      panelEl.classList.add('d-none');
      widget.classList.add('d-none');
    } else {
      widget.classList.remove('d-none');
    }
    window.dispatchEvent(new CustomEvent('sticky-notes-visibility', {
      detail: { userId: userId, hidden: isHidden }
    }));
  }

  function isHidden() {
    try {
      return localStorage.getItem(hiddenKey) === '1';
    } catch (e) {
      return false;
    }
  }

  function setSaveState(text, tone) {
    if (!saveState) return;
    saveState.textContent = text || '';
    saveState.classList.remove('text-muted', 'text-success', 'text-danger');
    saveState.classList.add(tone || 'text-muted');
  }

  async function request(url, options) {
    const resp = await fetch(url, options || {});
    const data = await resp.json().catch(() => ({}));
    if (!resp.ok) {
      throw new Error(data && data.error ? data.error : 'Request failed');
    }
    return data || {};
  }

  function renderList() {
    if (!listEl) return;
    listEl.innerHTML = '';
    if (!notes.length) {
      listEl.innerHTML = '<div class="text-muted small p-2">No notes yet.</div>';
      return;
    }
    notes.forEach(function (n) {
      const title = (n.title || '').trim() || ('Note #' + n.id);
      const updated = n.updated_at || '';
      const pinBadge = asPinned(n.is_pinned) === 1 ? '<span class="badge bg-warning-subtle text-warning ms-2">Pinned</span>' : '';
      const row = document.createElement('button');
      row.type = 'button';
      row.className = 'list-group-item list-group-item-action text-start';
      row.setAttribute('data-id', String(n.id));
      row.innerHTML = '<div class="fw-semibold">' + esc(title) + pinBadge + '</div><div class="small text-muted">' + esc(updated) + '</div>';
      listEl.appendChild(row);
    });
  }

  function showList() {
    if (listWrap) listWrap.classList.remove('d-none');
    if (editorWrap) editorWrap.classList.add('d-none');
    currentId = null;
    setSaveState('Saved', 'text-muted');
  }

  function showEditor(note) {
    if (!note) return;
    if (listWrap) listWrap.classList.add('d-none');
    if (editorWrap) editorWrap.classList.remove('d-none');
    currentId = parseInt(note.id, 10) || null;
    if (titleInput) titleInput.value = note.title || '';
    if (bodyInput) bodyInput.value = note.body || '';
    if (pinnedInput) pinnedInput.checked = asPinned(note.is_pinned) === 1;
    lastTitle = titleInput ? titleInput.value : '';
    lastBody = bodyInput ? bodyInput.value : '';
    lastPinned = pinnedInput ? !!pinnedInput.checked : false;
    localStorage.setItem(lastKey, String(currentId || ''));
    setSaveState('Saved', 'text-muted');
  }

  async function loadNotes() {
    const data = await request(apiBase + '?limit=5');
    notes = Array.isArray(data.data) ? data.data : [];
    notes.sort(function (a, b) {
      const ap = parseInt(a.is_pinned || 0, 10);
      const bp = parseInt(b.is_pinned || 0, 10);
      if (ap !== bp) return bp - ap;
      return String(b.updated_at || '').localeCompare(String(a.updated_at || ''));
    });
    renderList();
    if (!canManage && newBtn) {
      newBtn.classList.add('d-none');
    }
    return notes;
  }

  async function createNoteAndOpen() {
    const currentCount = notes.length;
    if (currentCount >= 5) {
      showAlert('Maximum of 5 sticky notes reached.');
      return;
    }
    if (!canManage) {
      showAlert('You do not have permission to create notes.');
      return;
    }
    showAlert('');
    const data = await request(apiBase, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title: '', body: 'New sticky note', is_pinned: 0 })
    });
    const newNote = data.data || null;
    await loadNotes();
    if (newNote) {
      const found = notes.find(function (n) { return parseInt(n.id, 10) === parseInt(newNote.id, 10); }) || newNote;
      showEditor(found);
    }
  }

  async function openSmart() {
    showAlert('');
    await loadNotes();
    if (!notes.length) {
      if (canManage) {
        await createNoteAndOpen();
      } else {
        showList();
      }
      return;
    }
    if (notes.length === 1) {
      showEditor(notes[0]);
      return;
    }
    // Multiple notes: always show list first.
    showList();
  }

  async function autosaveNow() {
    if (!canManage || !currentId) return;
    const title = titleInput ? titleInput.value.trim() : '';
    const body = bodyInput ? bodyInput.value : '';
    const pin = pinnedInput ? (pinnedInput.checked ? 1 : 0) : 0;
    if (title === lastTitle && body === lastBody && !!pin === !!lastPinned) {
      return;
    }
    setSaveState('Saving...', 'text-muted');
    try {
      const data = await request(apiBase + '?id=' + encodeURIComponent(String(currentId)), {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: title, body: body, is_pinned: pin })
      });
      const row = data.data || null;
      if (row) {
        const idx = notes.findIndex(function (n) { return parseInt(n.id, 10) === parseInt(row.id, 10); });
        if (idx >= 0) notes[idx] = row;
      }
      lastTitle = title;
      lastBody = body;
      lastPinned = !!pin;
      setSaveState('Saved', 'text-success');
      renderList();
    } catch (e) {
      setSaveState('Error', 'text-danger');
      showAlert(e.message || 'Unable to save note');
    }
  }

  function scheduleAutosave() {
    if (!canManage || !currentId) return;
    setSaveState('Typing...', 'text-muted');
    if (saveTimer) clearTimeout(saveTimer);
    saveTimer = setTimeout(autosaveNow, 700);
  }

  async function flushAutosave() {
    if (!canManage || !currentId) return;
    if (saveTimer) {
      clearTimeout(saveTimer);
      saveTimer = null;
    }
    await autosaveNow();
  }

  async function deleteCurrent() {
    if (!canManage || !currentId) return;
    try {
      await request(apiBase + '?id=' + encodeURIComponent(String(currentId)), { method: 'DELETE' });
      await loadNotes();
      if (notes.length === 1) {
        showEditor(notes[0]);
      } else {
        showList();
      }
    } catch (e) {
      showAlert(e.message || 'Unable to delete note');
    }
  }

  function getDefaultPosition() {
    return {
      x: Math.max(0, window.innerWidth - widget.offsetWidth - viewportPad),
      y: Math.max(0, Math.round((window.innerHeight - widget.offsetHeight) / 2))
    };
  }

  function getMessengerRect() {
    const root = document.getElementById('fmsgRoot');
    if (!root || root.classList.contains('d-none')) return null;
    const rect = root.getBoundingClientRect();
    if (!rect || (rect.width === 0 && rect.height === 0)) return null;
    return rect;
  }

  function overlapsMessenger(x, y) {
    const rect = getMessengerRect();
    if (!rect) return false;
    const width = widget.offsetWidth || 52;
    const height = widget.offsetHeight || 52;
    const pad = 12;
    return !(
      (x + width + pad) < rect.left ||
      (x - pad) > rect.right ||
      (y + height + pad) < rect.top ||
      (y - pad) > rect.bottom
    );
  }

  function normalizePosition(x, y) {
    const maxX = Math.max(0, window.innerWidth - widget.offsetWidth);
    const maxY = Math.max(0, window.innerHeight - widget.offsetHeight);
    let nextX = Math.max(0, Math.min(maxX, x));
    let nextY = Math.max(0, Math.min(maxY, y));

    if (!overlapsMessenger(nextX, nextY)) {
      return { x: nextX, y: nextY };
    }

    const defaultPos = getDefaultPosition();
    if (!overlapsMessenger(defaultPos.x, defaultPos.y)) {
      return defaultPos;
    }

    const messengerRect = getMessengerRect();
    if (messengerRect) {
      const aboveMessenger = {
        x: Math.max(0, window.innerWidth - widget.offsetWidth - viewportPad),
        y: Math.max(8, Math.round(messengerRect.top - widget.offsetHeight - 16))
      };
      if (!overlapsMessenger(aboveMessenger.x, aboveMessenger.y)) {
        return aboveMessenger;
      }

      const leftOfMessenger = {
        x: Math.max(8, Math.round(messengerRect.left - widget.offsetWidth - 16)),
        y: Math.max(8, Math.min(maxY, Math.round(messengerRect.top + ((messengerRect.height - widget.offsetHeight) / 2))))
      };
      if (!overlapsMessenger(leftOfMessenger.x, leftOfMessenger.y)) {
        return leftOfMessenger;
      }
    }

    return defaultPos;
  }

  function setWidgetPosition(x, y) {
    const pos = normalizePosition(x, y);
    widget.style.left = pos.x + 'px';
    widget.style.top = pos.y + 'px';
    widget.style.right = 'auto';
    widget.style.bottom = 'auto';
    return pos;
  }

  function applyStoredPosition() {
    try {
      const raw = localStorage.getItem(posKey);
      if (!raw) {
        setWidgetPosition(getDefaultPosition().x, getDefaultPosition().y);
        return;
      }
      const p = JSON.parse(raw);
      if (!p || typeof p.x !== 'number' || typeof p.y !== 'number') {
        setWidgetPosition(getDefaultPosition().x, getDefaultPosition().y);
        return;
      }
      const applied = setWidgetPosition(p.x, p.y);
      if (applied.x !== p.x || applied.y !== p.y) {
        localStorage.setItem(posKey, JSON.stringify(applied));
      }
    } catch (e) {
      setWidgetPosition(getDefaultPosition().x, getDefaultPosition().y);
    }
  }

  function positionPanelNearLauncher() {
    if (!panelEl || panelEl.classList.contains('d-none')) return;
    const iconRect = widget.getBoundingClientRect();
    const panelWidth = panelEl.offsetWidth || 380;
    const panelHeight = panelEl.offsetHeight || 420;
    const margin = 12;

    let left = iconRect.left - panelWidth - margin;
    if (left < 8) {
      left = iconRect.right + margin;
    }
    if (left + panelWidth > window.innerWidth - 8) {
      left = Math.max(8, window.innerWidth - panelWidth - 8);
    }

    let top = iconRect.top - 20;
    if (top + panelHeight > window.innerHeight - 8) {
      top = Math.max(8, window.innerHeight - panelHeight - 8);
    }
    if (top < 8) {
      top = 8;
    }

    panelEl.style.left = left + 'px';
    panelEl.style.top = top + 'px';
    panelEl.style.right = 'auto';
    panelEl.style.bottom = 'auto';
  }

  function enableDrag() {
    let dragging = false;
    let moved = false;
    let offsetX = 0;
    let offsetY = 0;

    function onDown(e) {
      const evt = e.touches && e.touches[0] ? e.touches[0] : e;
      const rect = widget.getBoundingClientRect();
      dragging = true;
      moved = false;
      offsetX = evt.clientX - rect.left;
      offsetY = evt.clientY - rect.top;
      launcher.classList.add('dragging');
      e.preventDefault();
    }

    function onMove(e) {
      if (!dragging) return;
      const evt = e.touches && e.touches[0] ? e.touches[0] : e;
      let x = evt.clientX - offsetX;
      let y = evt.clientY - offsetY;
      moved = true;
      setWidgetPosition(x, y);
      positionPanelNearLauncher();
    }

    function onUp() {
      if (!dragging) return;
      dragging = false;
      launcher.classList.remove('dragging');
      const rect = widget.getBoundingClientRect();
      const applied = setWidgetPosition(rect.left, rect.top);
      localStorage.setItem(posKey, JSON.stringify(applied));
      if (moved) {
        launcher.dataset.dragged = '1';
        setTimeout(function () {
          delete launcher.dataset.dragged;
        }, 120);
      }
    }

    launcher.addEventListener('mousedown', onDown);
    launcher.addEventListener('touchstart', onDown, { passive: false });
    window.addEventListener('mousemove', onMove);
    window.addEventListener('touchmove', onMove, { passive: false });
    window.addEventListener('mouseup', onUp);
    window.addEventListener('touchend', onUp);
  }

  function openPanel() {
    if (isHidden()) return;
    panelEl.classList.remove('d-none');
    positionPanelNearLauncher();
    openSmart().catch(function (e) {
      showAlert(e.message || 'Unable to load notes');
      showList();
    });
  }

  function closePanel() {
    flushAutosave().finally(function () {
      panelEl.classList.add('d-none');
    });
  }

  launcher.addEventListener('click', function () {
    if (launcher.classList.contains('dragging')) return;
    if (launcher.dataset.dragged === '1') return;
    if (panelEl.classList.contains('d-none')) {
      openPanel();
    } else {
      closePanel();
    }
  });

  if (closeBtn) {
    closeBtn.addEventListener('click', function () {
      if (editorWrap && !editorWrap.classList.contains('d-none')) {
        flushAutosave().finally(function () {
          showList();
        });
        return;
      }
      closePanel();
    });
  }

  document.addEventListener('mousedown', function (e) {
    if (panelEl.classList.contains('d-none')) return;
    const t = e.target;
    if (panelEl.contains(t) || widget.contains(t)) return;
    closePanel();
  });

  window.addEventListener('resize', function () {
    const rect = widget.getBoundingClientRect();
    setWidgetPosition(rect.left, rect.top);
    positionPanelNearLauncher();
  });
  panelEl.addEventListener('mouseleave', function () {
    flushAutosave().catch(function () {});
  });

  if (listEl) {
    listEl.addEventListener('click', function (e) {
      const btn = e.target.closest('.list-group-item');
      if (!btn) return;
      const id = parseInt(btn.getAttribute('data-id') || '', 10);
      if (!id) return;
      const note = notes.find(function (n) { return parseInt(n.id, 10) === id; });
      if (!note) return;
      flushAutosave().finally(function () {
        showEditor(note);
      });
    });
  }

  if (newBtn) {
    newBtn.addEventListener('click', function () {
      flushAutosave().finally(function () {
        createNoteAndOpen().catch(function (e) {
          showAlert(e.message || 'Unable to create note');
        });
      });
    });
  }
  if (hideBtn) {
    hideBtn.addEventListener('click', function () {
      setHidden(true);
    });
  }
  if (backBtn) {
    backBtn.addEventListener('click', function () {
      flushAutosave().finally(function () {
        showList();
      });
    });
  }
  if (deleteBtn) {
    deleteBtn.addEventListener('click', function () { deleteCurrent(); });
  }
  if (titleInput) {
    titleInput.addEventListener('input', function () { setSaveState('Unsaved', 'text-muted'); });
  }
  if (bodyInput) {
    bodyInput.addEventListener('input', function () { setSaveState('Unsaved', 'text-muted'); });
  }
  if (pinnedInput) {
    pinnedInput.addEventListener('change', function () { setSaveState('Unsaved', 'text-muted'); });
  }

    if (isHidden()) {
      widget.classList.add('d-none');
      panelEl.classList.add('d-none');
    }
    applyStoredPosition();
    enableDrag();

    window.addEventListener('storage', function (e) {
      if (e.key !== hiddenKey) return;
      if (isHidden()) {
        panelEl.classList.add('d-none');
        widget.classList.add('d-none');
      } else {
        widget.classList.remove('d-none');
      }
    });

    window.addEventListener('sticky-notes-visibility', function (e) {
      const detail = e && e.detail ? e.detail : null;
      if (!detail || String(detail.userId || '') !== String(userId)) return;
      if (detail.hidden) {
        panelEl.classList.add('d-none');
        widget.classList.add('d-none');
      } else {
        widget.classList.remove('d-none');
      }
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', bootStickyNotes);
  } else {
    bootStickyNotes();
  }
  window.addEventListener('load', bootStickyNotes);
})();
