/* eslint-disable */
(function () {
  // If the feature is off (or its owning module is disabled), do not issue timer API calls.
  // This avoids noisy 403s in the console on deployments without time tracking enabled.
  try {
    if (window.crmFeatures && window.crmFeatures.timeTrackingEnabled === false) {
      return;
    }
  } catch (e) {}

  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiIndex = apiMap.timeLogsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/time-logs/index');
  const apiDetail = apiMap.timeLogsDetail || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/time-logs/detail');
  const apiTimers = apiMap.timeTimersIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/time-timers/index');

  const instances = [];
  let editModal = null;
  let switchModal = null;

  function escapeHtml(str) {
    return String(str || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function formatHours(mins) {
    const m = Number(mins || 0);
    if (!m) return '0h';
    const hours = m / 60;
    const rounded = Math.round(hours * 10) / 10;
    return (rounded % 1 === 0 ? rounded.toFixed(0) : rounded.toFixed(1)) + 'h';
  }

  function formatTimer(ms) {
    const totalSeconds = Math.max(0, Math.floor(ms / 1000));
    const hrs = Math.floor(totalSeconds / 3600);
    const mins = Math.floor((totalSeconds % 3600) / 60);
    const secs = totalSeconds % 60;
    return [hrs, mins, secs].map(v => String(v).padStart(2, '0')).join(':');
  }

  function formatHumanMinutes(mins) {
    const m = Math.max(0, parseInt(mins || 0, 10) || 0);
    const hrs = Math.floor(m / 60);
    const rem = m % 60;
    if (hrs <= 0) return `${rem}mins`;
    if (rem === 0) return `${hrs}hr`;
    return `${hrs}hr ${rem}mins`;
  }

  const ACTIVE_TIMER_KEY = 'time_timer_active';

  function getLegacyTimerKey(entityType, entityId) {
    return `time_timer_${entityType}_${entityId}`;
  }

  function buildEntityLink(entityType, entityId) {
    const id = encodeURIComponent(entityId || '');
    const type = String(entityType || '').toLowerCase();
    const enabled = (slug) => !!(window.__mmkModulesEnabled && window.__mmkModulesEnabled[slug]);
    if (type === 'case') return `studio/cases/view.kml?id=${id}`;
    if (type === 'task') return enabled('tasks') ? `studio/tasks/?open=task&id=${id}` : '#';
    if (type === 'ticket') return enabled('tickets') ? `studio/tickets/view.kml?id=${id}` : '#';
    return '#';
  }

  function buildEntityLabel(entityType, entityId) {
    const type = String(entityType || '').toLowerCase();
    const id = entityId || '';
    if (type === 'case') return `Case #${id}`;
    if (type === 'task') return `Task #${id}`;
    if (type === 'ticket') return `Ticket #${id}`;
    return `Item #${id}`;
  }

  function parseMysqlDatetime(value) {
    if (typeof value !== 'string') return null;
    const match = value.trim().match(/^(\d{4})-(\d{2})-(\d{2})[ T](\d{2}):(\d{2})(?::(\d{2}))?/);
    if (!match) return null;
    const year = parseInt(match[1], 10);
    const month = parseInt(match[2], 10) - 1;
    const day = parseInt(match[3], 10);
    const hour = parseInt(match[4], 10);
    const min = parseInt(match[5], 10);
    const sec = parseInt(match[6] || '0', 10);
    const dt = new Date(year, month, day, hour, min, sec);
    const ts = dt.getTime();
    return Number.isNaN(ts) ? null : ts;
  }

  function toMs(value) {
    if (value === null || value === undefined || value === '') return null;
    if (typeof value === 'number') {
      if (value > 20000000000) return value;
      if (value > 1000000000) return value * 1000;
      return null;
    }
    if (typeof value === 'string') {
      if (/^\\d+$/.test(value)) {
        const raw = parseInt(value, 10);
        if (raw > 20000000000) return raw;
        if (raw > 1000000000) return raw * 1000;
      }
      const parsed = parseMysqlDatetime(value) || Date.parse(value.replace(' ', 'T'));
      if (!Number.isNaN(parsed)) return parsed;
    }
    return null;
  }

  function normalizeActive(obj) {
    if (!obj || !obj.entity_type || !obj.entity_id) return null;
    // Prefer epoch fields from API (timezone-safe), fallback to datetime strings.
    let startedMs = toMs(obj.started_at_ts);
    if (!startedMs) {
      startedMs = toMs(obj.started_at);
    }
    if (!startedMs) return null;
    let pauseMs = toMs(obj.pause_started_at_ts);
    if (!pauseMs) {
      pauseMs = toMs(obj.pause_started_at);
    }
    const normalized = {
      entity_type: obj.entity_type,
      entity_id: obj.entity_id,
      started_at: obj.started_at || obj.started_at_ts || startedMs,
      started_at_ts: startedMs,
      paused_total_ms: parseInt(obj.paused_total_ms || 0, 10) || 0,
      pause_started_at: obj.pause_started_at || null,
      pause_started_at_ts: pauseMs,
      status: obj.status || (pauseMs ? 'paused' : 'running')
    };
    return normalized;
  }

  function getActiveTimer() {
    try {
      const raw = localStorage.getItem(ACTIVE_TIMER_KEY);
      if (!raw) return null;
      const obj = JSON.parse(raw);
      const normalized = normalizeActive(obj);
      if (!normalized) {
        localStorage.removeItem(ACTIVE_TIMER_KEY);
        return null;
      }
      return normalized;
    } catch (e) {
      return null;
    }
  }

  function setActiveTimer(entityType, entityId, ts) {
    try {
      localStorage.setItem(ACTIVE_TIMER_KEY, JSON.stringify({
        entity_type: entityType,
        entity_id: entityId,
        started_at: ts,
        started_at_ts: ts,
        paused_total_ms: 0,
        pause_started_at: null,
        pause_started_at_ts: null,
        status: 'running'
      }));
    } catch (e) {}
  }

  function setActiveTimerObj(obj) {
    const normalized = normalizeActive(obj);
    if (!normalized) {
      localStorage.removeItem(ACTIVE_TIMER_KEY);
      return;
    }
    try {
      localStorage.setItem(ACTIVE_TIMER_KEY, JSON.stringify(normalized));
    } catch (e) {}
  }

  function clearActiveTimerIfMatch(entityType, entityId) {
    try {
      const active = getActiveTimer();
      if (!active) return false;
      if (String(active.entity_type) !== String(entityType)) return false;
      if (parseInt(active.entity_id, 10) !== parseInt(entityId, 10)) return false;
      localStorage.removeItem(ACTIVE_TIMER_KEY);
      return true;
    } catch (e) {
      return false;
    }
  }

  function notifyActiveTimerChange() {
    try {
      window.dispatchEvent(new CustomEvent('time-tracking:change', { detail: getActiveTimer() }));
    } catch (e) {}
  }

  function loadTimerStart(entityType, entityId) {
    const active = getActiveTimer();
    if (!active) return null;
    if (String(active.entity_type) !== String(entityType)) return null;
    if (parseInt(active.entity_id, 10) !== parseInt(entityId, 10)) return null;
    const ts = toMs(active.started_at_ts || active.started_at);
    return Number.isFinite(ts) ? ts : null;
  }

  function saveTimerStart(entityType, entityId, ts) {
    setActiveTimer(entityType, entityId, ts);
    notifyActiveTimerChange();
  }

  function clearTimerStart(entityType, entityId) {
    if (clearActiveTimerIfMatch(entityType, entityId)) {
      notifyActiveTimerChange();
    }
  }

  function migrateLegacyTimer(entityType, entityId) {
    try {
      if (getActiveTimer()) return;
      const legacyKey = getLegacyTimerKey(entityType, entityId);
      const raw = localStorage.getItem(legacyKey);
      if (!raw) return;
      const ts = parseInt(raw, 10);
      if (!Number.isFinite(ts)) return;
      localStorage.removeItem(legacyKey);
      setActiveTimer(entityType, entityId, ts);
      notifyActiveTimerChange();
    } catch (e) {}
  }

  let serverSyncing = null;
  async function syncActiveTimerFromServer() {
    if (!apiTimers) return null;
    if (serverSyncing) return serverSyncing;
    serverSyncing = fetch(apiTimers)
      .then(resp => resp.json().catch(() => ({})).then(data => ({ ok: resp.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) return null;
        if (!data || !data.active) {
          localStorage.removeItem(ACTIVE_TIMER_KEY);
          notifyActiveTimerChange();
          return null;
        }
        const prev = getActiveTimer();
        const status = (prev && String(prev.entity_type) === String(data.active.entity_type) &&
          parseInt(prev.entity_id, 10) === parseInt(data.active.entity_id, 10))
          ? (prev.status || (data.active.pause_started_at ? 'paused' : 'running'))
          : (data.active.pause_started_at ? 'paused' : 'running');
        const active = {
          entity_type: data.active.entity_type,
          entity_id: data.active.entity_id,
          started_at: data.active.started_at || null,
          started_at_ts: data.active.started_at_ts || null,
          paused_total_ms: data.active.paused_total_ms || 0,
          pause_started_at: data.active.pause_started_at || null,
          pause_started_at_ts: data.active.pause_started_at_ts || null,
          status: status
        };
        setActiveTimerObj(active);
        notifyActiveTimerChange();
        return active;
      })
      .finally(() => {
        serverSyncing = null;
      });
    return serverSyncing;
  }

  function calcElapsedMs(active) {
    if (!active || !active.started_at_ts) return 0;
    const started = parseInt(active.started_at_ts, 10);
    if (!Number.isFinite(started)) return 0;
    const pausedTotal = parseInt(active.paused_total_ms || 0, 10) || 0;
    const pausedAt = active.pause_started_at_ts ? parseInt(active.pause_started_at_ts, 10) : null;
    const end = Number.isFinite(pausedAt) ? pausedAt : Date.now();
    return Math.max(0, end - started - pausedTotal);
  }

  async function postTimerAction(action, entityType, entityId, force) {
    const payload = {
      entity_type: entityType,
      entity_id: entityId,
      action: action
    };
    if (force) payload.force = true;
    const resp = await fetch(apiTimers, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await resp.json().catch(() => ({}));
    if (!resp.ok) {
      const err = data.error || 'Unable to update timer';
      const e = new Error(err);
      e.data = data;
      throw e;
    }
    return data.active || null;
  }

  async function clearServerTimer(entityType, entityId, force) {
    const params = new URLSearchParams();
    if (entityType && entityId) {
      params.set('entity_type', entityType);
      params.set('entity_id', String(entityId));
    }
    if (force) params.set('force', '1');
    const resp = await fetch(apiTimers + '?' + params.toString(), { method: 'DELETE' });
    await resp.json().catch(() => ({}));
  }

  function ensureEditModal() {
    if (editModal && editModal.el) return editModal;
    const wrapper = document.createElement('div');
    wrapper.innerHTML = `
      <div class="modal fade" id="timeLogEditModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-sm modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Edit Time Log</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
              <div class="mb-2">
                <label class="form-label small text-muted">Minutes</label>
                <input type="number" min="1" class="form-control form-control-sm" id="timeLogEditMinutes">
              </div>
              <div class="mb-2">
                <label class="form-label small text-muted">Note</label>
                <input type="text" class="form-control form-control-sm" id="timeLogEditNote">
              </div>
              <div class="form-check form-switch">
                <input class="form-check-input" type="checkbox" id="timeLogEditBillable">
                <label class="form-check-label" for="timeLogEditBillable">Billable</label>
              </div>
              <div class="small text-danger mt-2 d-none" id="timeLogEditAlert"></div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-light btn-sm" data-bs-dismiss="modal">Cancel</button>
              <button type="button" class="btn btn-primary btn-sm" id="timeLogEditSave">Save</button>
            </div>
          </div>
        </div>
      </div>
    `;
    document.body.appendChild(wrapper.firstElementChild);
    const el = document.getElementById('timeLogEditModal');
    const modal = (typeof bootstrap !== 'undefined') ? new bootstrap.Modal(el) : null;
    editModal = {
      el,
      modal,
      minutes: document.getElementById('timeLogEditMinutes'),
      note: document.getElementById('timeLogEditNote'),
      billable: document.getElementById('timeLogEditBillable'),
      alert: document.getElementById('timeLogEditAlert'),
      save: document.getElementById('timeLogEditSave'),
      currentId: null,
      currentInstance: null
    };
    return editModal;
  }

  function ensureSwitchModal() {
    if (switchModal && switchModal.el) return switchModal;
    const el = document.getElementById('timeTimerSwitchModal');
    if (!el) return null;
    if (el.parentElement !== document.body) {
      document.body.appendChild(el);
    }
    const modal = (typeof bootstrap !== 'undefined') ? new bootstrap.Modal(el) : null;
    switchModal = {
      el,
      modal,
      title: document.getElementById('timeTimerSwitchTitle'),
      message: document.getElementById('timeTimerSwitchMessage'),
      link: document.getElementById('timeTimerSwitchLink'),
      yes: document.getElementById('timeTimerSwitchYes'),
      resolve: null
    };
    if (switchModal.yes) {
      switchModal.yes.addEventListener('click', () => {
        if (switchModal.resolve) switchModal.resolve(true);
        switchModal.resolve = null;
        if (switchModal.modal) switchModal.modal.hide();
      });
    }
    if (switchModal.el) {
      switchModal.el.addEventListener('hidden.bs.modal', () => {
        if (switchModal.resolve) switchModal.resolve(false);
        switchModal.resolve = null;
      });
      switchModal.el.addEventListener('shown.bs.modal', () => {
        const backdrops = document.querySelectorAll('.modal-backdrop');
        const last = backdrops[backdrops.length - 1];
        if (last) last.classList.add('tt-modal-backdrop');
      });
    }
    return switchModal;
  }

  function confirmTimerSwitch(message, linkHref, linkLabel, title) {
    if (typeof window.crmUiConfirm === 'function' && !linkHref) {
      return window.crmUiConfirm(message, title || 'Timer Running', {
        okText: 'Continue',
        cancelText: 'Cancel',
        variant: 'primary',
        icon: 'warning'
      });
    }
    if (typeof bootstrap === 'undefined') {
      return Promise.resolve(window.confirm(message));
    }
    const modal = ensureSwitchModal();
    if (!modal || !modal.modal || !modal.message) {
      return Promise.resolve(window.confirm(message));
    }
    if (modal.title) {
      modal.title.textContent = title || 'Timer Running';
    }
    modal.message.textContent = message;
    if (modal.link) {
      if (linkHref) {
        modal.link.href = linkHref;
        modal.link.classList.remove('d-none');
        modal.link.querySelector('span')?.replaceChildren(document.createTextNode(linkLabel || 'Open'));
      } else {
        modal.link.classList.add('d-none');
      }
    }
    return new Promise((resolve) => {
      modal.resolve = resolve;
      modal.modal.show();
    });
  }

  async function saveEdit() {
    const modal = ensureEditModal();
    if (!modal || !modal.currentId) return;
    const minutes = parseInt((modal.minutes && modal.minutes.value) || '0', 10);
    if (!minutes || minutes <= 0) {
      if (modal.alert) {
        modal.alert.textContent = 'Minutes must be greater than zero.';
        modal.alert.classList.remove('d-none');
      }
      return;
    }
    const payload = {
      minutes: minutes,
      note: modal.note ? modal.note.value : '',
      is_billable: modal.billable ? modal.billable.checked : true
    };
    try {
      const resp = await fetch(apiDetail + '?id=' + encodeURIComponent(modal.currentId), {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await resp.json().catch(() => ({}));
      if (!resp.ok) throw new Error(data.error || 'Unable to update time log');
      if (modal.modal) modal.modal.hide();
      if (modal.currentInstance) modal.currentInstance.refresh();
    } catch (e) {
      if (modal.alert) {
        modal.alert.textContent = e.message || 'Unable to update time log';
        modal.alert.classList.remove('d-none');
      }
    }
  }

  function createInstance(root, opts) {
    const config = opts || {};
    const state = {
      entityType: config.entityType || root.getAttribute('data-entity-type') || '',
      entityId: config.entityId || parseInt(root.getAttribute('data-entity-id') || '0', 10),
      canUse: !!config.canUse || !!config.canManage || root.getAttribute('data-can-use') === '1' || root.getAttribute('data-can-manage') === '1',
      canManage: !!config.canManage || root.getAttribute('data-can-manage') === '1'
    };

    const timerEl = root.querySelector('[data-tt-timer]');
    const toggleBtn = root.querySelector('[data-tt-toggle]');
    const stopBtn = root.querySelector('[data-tt-stop]');
    const resetBtn = root.querySelector('[data-tt-reset]');
    const formWrap = root.querySelector('[data-tt-form]');
    const minutesInput = root.querySelector('[data-tt-minutes]');
    const humanEl = root.querySelector('[data-tt-human]');
    const noteInput = root.querySelector('[data-tt-note]');
    const billableToggle = root.querySelector('[data-tt-billable-toggle]');
    const logBtn = root.querySelector('[data-tt-log]');
    const listEl = root.querySelector('[data-tt-list]');
    const alertEl = root.querySelector('[data-tt-alert]');
    const totalEl = root.querySelector('[data-tt-total]');
    const billableEl = root.querySelector('[data-tt-billable]');

    let timerInterval = null;
    let actionBusy = false;

    if (minutesInput) {
      minutesInput.readOnly = !state.canUse;
      if (!state.canUse) {
        minutesInput.classList.add('bg-light');
        minutesInput.title = 'You do not have permission to log time.';
      }
    }

    function setAlert(msg, opts) {
      if (!alertEl) return;
      const allowHtml = opts && opts.html;
      if (allowHtml) {
        alertEl.innerHTML = msg || '';
      } else {
        alertEl.textContent = msg || '';
      }
      alertEl.classList.toggle('d-none', !msg);
    }

    function setDisabled(disabled) {
      const controls = [toggleBtn, resetBtn, minutesInput, noteInput, billableToggle, logBtn];
      controls.forEach((el) => {
        if (!el) return;
        el.disabled = !!disabled;
      });
    }

    function beginButtonAction(btn, label) {
      if (actionBusy) return false;
      actionBusy = true;
      if (btn) btn.dataset.busy = '1';
      if (btn && window.toggleButtonLoading) window.toggleButtonLoading(btn, true, label || 'Working...');
      return true;
    }

    function endButtonAction(btn) {
      actionBusy = false;
      if (btn) btn.dataset.busy = '0';
      if (btn && window.toggleButtonLoading) window.toggleButtonLoading(btn, false);
    }

    function isEntityReady() {
      return !!(state.entityType && state.entityId && state.entityId > 0);
    }

    function updateTimerDisplay() {
      if (!timerEl) return;
      if (!isEntityReady()) {
        timerEl.textContent = '00:00:00';
        return;
      }
        const active = getActiveTimer();
        if (!active || String(active.entity_type) !== String(state.entityType) ||
          parseInt(active.entity_id, 10) !== parseInt(state.entityId, 10)) {
          timerEl.textContent = '00:00:00';
          return;
        }
        const elapsed = calcElapsedMs(active);
        timerEl.textContent = formatTimer(elapsed);
    }

    function updateHumanMinutes() {
      if (!humanEl || !minutesInput) return;
      const mins = parseInt(minutesInput.value || '0', 10);
      humanEl.textContent = mins > 0 ? formatHumanMinutes(mins) : '';
    }

    function stopTimer(updateInput) {
      if (!isEntityReady()) return;
      const active = getActiveTimer();
      if (!active) return;
      if (String(active.entity_type) !== String(state.entityType) ||
        parseInt(active.entity_id, 10) !== parseInt(state.entityId, 10)) return;
      const elapsed = calcElapsedMs(active);
      active.status = 'stopped';
      setActiveTimerObj(active);
      notifyActiveTimerChange();
      if (timerInterval) {
        clearInterval(timerInterval);
        timerInterval = null;
      }
      updateTimerDisplay();
      updateTimerButtons(active);
      if (updateInput && minutesInput) {
        const mins = Math.max(1, Math.ceil(elapsed / 60000));
        minutesInput.value = mins;
        updateHumanMinutes();
        showForm(true);
      }
    }

    async function startTimer() {
      if (!state.canUse) return;
      if (!isEntityReady()) return;
      const active = getActiveTimer();
      if (active && (String(active.entity_type) !== String(state.entityType) ||
        parseInt(active.entity_id, 10) !== parseInt(state.entityId, 10))) {
        const labelPlain = buildEntityLabel(active.entity_type, active.entity_id);
        const confirmMsg = `Timer already running for ${labelPlain}. Stop timer and start this one? Note. Your time will be logged on previous timer.`;
        const linkHref = buildEntityLink(active.entity_type, active.entity_id);
        const linkLabel = `Open ${labelPlain}`;
        const confirmed = await confirmTimerSwitch(confirmMsg, linkHref, linkLabel);
        if (!confirmed) {
          return;
        }
        const elapsed = calcElapsedMs(active);
        const minutes = Math.max(1, Math.ceil(elapsed / 60000));
        try {
          const payload = {
            entity_type: active.entity_type,
            entity_id: active.entity_id,
            minutes: minutes,
            note: 'Auto-logged from timer switch',
            is_billable: 0
          };
          const resp = await fetch(apiIndex, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
          });
          const data = await resp.json().catch(() => ({}));
          if (!resp.ok) throw new Error(data.error || 'Unable to log previous timer');
          clearTimerStart(active.entity_type, active.entity_id);
          try {
            await clearServerTimer(active.entity_type, active.entity_id, true);
          } catch (e) {}
        } catch (e) {
          setAlert('Unable to log previous timer. Try again.', { html: false });
          return;
        }
      }
      const existing = loadTimerStart(state.entityType, state.entityId);
      if (existing) return;
      setAlert('');
      try {
        const active = await postTimerAction('start', state.entityType, state.entityId, false);
        const timerObj = {
          entity_type: active.entity_type,
          entity_id: active.entity_id,
          started_at: active.started_at || null,
          started_at_ts: active.started_at_ts || null,
          paused_total_ms: active.paused_total_ms || 0,
          pause_started_at: active.pause_started_at || null,
          pause_started_at_ts: active.pause_started_at_ts || null,
          status: 'running'
        };
        setActiveTimerObj(timerObj);
        notifyActiveTimerChange();
        if (timerInterval) clearInterval(timerInterval);
        timerInterval = setInterval(updateTimerDisplay, 1000);
        updateTimerDisplay();
        updateTimerButtons(timerObj);
      } catch (e) {
        setAlert(e.message || 'Unable to start timer');
      }
    }

    function resetTimer() {
      if (!state.canUse) return;
      if (!isEntityReady()) return;
      clearTimerStart(state.entityType, state.entityId);
      if (timerInterval) {
        clearInterval(timerInterval);
        timerInterval = null;
      }
      updateTimerButtons(null);
      updateTimerDisplay();
      showForm(false);
      if (minutesInput) minutesInput.value = '';
      if (noteInput) noteInput.value = '';
    }

    async function loadLogs() {
      if (!isEntityReady()) {
        setDisabled(true);
        return;
      }
      setDisabled(!state.canUse);
      if (listEl) {
        listEl.innerHTML = '<div class="small text-muted">Loading time logs...</div>';
      }
      try {
        const params = new URLSearchParams({ entity_type: state.entityType, entity_id: state.entityId, limit: '50' });
        const resp = await fetch(apiIndex + '?' + params.toString());
        const data = await resp.json().catch(() => ({}));
        if (!resp.ok) throw new Error(data.error || 'Unable to load time logs');
        renderLogs(data.data || [], data.totals || {});
      } catch (e) {
        setAlert(e.message || 'Unable to load time logs');
      }
    }

    function renderLogs(items, totals) {
      if (totalEl) totalEl.textContent = formatHours(totals.total_minutes || 0);
      if (billableEl) billableEl.textContent = formatHours(totals.billable_minutes || 0);
      if (!listEl) return;
      listEl.innerHTML = '';
      if (!items.length) {
        const empty = document.createElement('div');
        empty.className = 'small text-muted';
        empty.textContent = 'No time logs yet.';
        listEl.appendChild(empty);
        return;
      }
      items.forEach((item) => {
        const row = document.createElement('div');
        row.className = 'd-flex justify-content-between align-items-start border-bottom pb-2';
        const left = document.createElement('div');
        const badge = document.createElement('span');
        badge.className = 'badge ' + (item.is_billable ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary');
        badge.textContent = item.is_billable ? 'Billable' : 'Non-billable';

        const top = document.createElement('div');
        top.className = 'fw-semibold';
        top.textContent = formatHumanMinutes(item.minutes || 0);
        top.appendChild(document.createTextNode(' '));
        top.appendChild(badge);

        const meta = document.createElement('div');
        meta.className = 'small text-muted';
        meta.textContent = `${item.user_name || 'User'} • ${item.created_at || ''}`;

        left.appendChild(top);
        left.appendChild(meta);
        if (item.note) {
          const note = document.createElement('div');
          note.className = 'small text-muted';
          note.textContent = item.note;
          left.appendChild(note);
        }

        row.appendChild(left);

        if (state.canManage) {
          const actions = document.createElement('div');
          actions.className = 'btn-group btn-group-sm';
          const editBtn = document.createElement('button');
          editBtn.type = 'button';
          editBtn.className = 'btn btn-light';
          editBtn.textContent = 'Edit';
          editBtn.addEventListener('click', () => {
            if (editBtn.dataset.busy === '1') return;
            openEdit(item);
          });
          const delBtn = document.createElement('button');
          delBtn.type = 'button';
          delBtn.className = 'btn btn-light text-danger';
          delBtn.textContent = 'Del';
          delBtn.addEventListener('click', () => {
            if (delBtn.dataset.busy === '1') return;
            deleteLog(item, delBtn);
          });
          actions.appendChild(editBtn);
          actions.appendChild(delBtn);
          row.appendChild(actions);
        }

        listEl.appendChild(row);
      });
    }

    async function createLog() {
      if (!state.canUse) return false;
      if (!isEntityReady()) return;
      const minutes = parseInt((minutesInput && minutesInput.value) || '0', 10);
      if (!minutes || minutes <= 0) {
        setAlert('Enter minutes to log.');
        return false;
      }
      const note = noteInput ? noteInput.value : '';
      const isBillable = billableToggle ? billableToggle.checked : false;
      setAlert('');
      try {
        const payload = {
          entity_type: state.entityType,
          entity_id: state.entityId,
          minutes: minutes,
          note: note,
          is_billable: isBillable ? 1 : 0
        };
        const resp = await fetch(apiIndex, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
        const data = await resp.json().catch(() => ({}));
        if (!resp.ok) throw new Error(data.error || 'Unable to log time');
      if (minutesInput) minutesInput.value = '';
      if (noteInput) noteInput.value = '';
      if (billableToggle) billableToggle.checked = false;
      await loadLogs();
      return true;
      } catch (e) {
        setAlert(e.message || 'Unable to log time');
        return false;
      }
    }

    async function deleteLog(item, triggerBtn) {
      if (!state.canManage) return;
      if (triggerBtn && triggerBtn.dataset.busy === '1') return;
      const ok = await window.crmUiConfirm('Delete this time log?', 'Delete Time Log', {
        okText: 'Delete',
        cancelText: 'Cancel',
        variant: 'danger',
        icon: 'warning'
      });
      if (!ok) return;
      try {
        if (triggerBtn) {
          triggerBtn.dataset.busy = '1';
          if (window.toggleButtonLoading) window.toggleButtonLoading(triggerBtn, true, 'Deleting...');
        }
        const resp = await fetch(apiDetail + '?id=' + encodeURIComponent(item.id), { method: 'DELETE' });
        const data = await resp.json().catch(() => ({}));
        if (!resp.ok) throw new Error(data.error || 'Unable to delete time log');
        await loadLogs();
      } catch (e) {
        setAlert(e.message || 'Unable to delete time log');
      } finally {
        if (triggerBtn) {
          triggerBtn.dataset.busy = '0';
          if (window.toggleButtonLoading) window.toggleButtonLoading(triggerBtn, false);
        }
      }
    }

    function openEdit(item) {
      if (!state.canManage) return;
      const modal = ensureEditModal();
      if (!modal) return;
      modal.currentId = item.id;
      modal.currentInstance = instance;
      if (modal.minutes) modal.minutes.value = item.minutes || '';
      if (modal.note) modal.note.value = item.note || '';
      if (modal.billable) modal.billable.checked = !!item.is_billable;
      if (modal.alert) {
        modal.alert.textContent = '';
        modal.alert.classList.add('d-none');
      }
      if (modal.modal) {
        modal.modal.show();
      }
    }

    if (toggleBtn) {
      toggleBtn.addEventListener('click', async () => {
        if (!state.canUse) return;
        if (!beginButtonAction(toggleBtn, 'Working...')) return;
        try {
          if (!isEntityReady()) return;
          let active = getActiveTimer();
          if (!active) {
            await syncActiveTimerFromServer();
            active = getActiveTimer();
          }
          const isSame = active && String(active.entity_type) === String(state.entityType) &&
            parseInt(active.entity_id, 10) === parseInt(state.entityId, 10);
          const isPaused = isSame && active.pause_started_at_ts;
          if (!active || !isSame) {
            await startTimer();
            return;
          }
          if (isPaused) {
            try {
              const updated = await postTimerAction('resume', state.entityType, state.entityId, false);
              const timerObj = {
                entity_type: updated.entity_type,
                entity_id: updated.entity_id,
                started_at: updated.started_at || null,
                started_at_ts: updated.started_at_ts || null,
                paused_total_ms: updated.paused_total_ms || 0,
                pause_started_at: updated.pause_started_at || null,
                pause_started_at_ts: updated.pause_started_at_ts || null,
                status: 'running'
              };
              setActiveTimerObj(timerObj);
              notifyActiveTimerChange();
              if (timerInterval) clearInterval(timerInterval);
              timerInterval = setInterval(updateTimerDisplay, 1000);
              updateTimerDisplay();
              updateTimerButtons(timerObj);
            } catch (e) {
              setAlert(e.message || 'Unable to resume timer');
            }
          } else {
            try {
              const updated = await postTimerAction('pause', state.entityType, state.entityId, false);
              const timerObj = {
                entity_type: updated.entity_type,
                entity_id: updated.entity_id,
                started_at: updated.started_at || null,
                started_at_ts: updated.started_at_ts || null,
                paused_total_ms: updated.paused_total_ms || 0,
                pause_started_at: updated.pause_started_at || null,
                pause_started_at_ts: updated.pause_started_at_ts || null,
                status: 'paused'
              };
              setActiveTimerObj(timerObj);
              notifyActiveTimerChange();
              if (timerInterval) {
                clearInterval(timerInterval);
                timerInterval = null;
              }
              updateTimerDisplay();
              updateTimerButtons(timerObj);
            } catch (e) {
              setAlert(e.message || 'Unable to pause timer');
            }
          }
        } finally {
          endButtonAction(toggleBtn);
        }
      });
    }
    if (stopBtn) {
      stopBtn.addEventListener('click', async () => {
        if (!state.canUse) return;
        if (!beginButtonAction(stopBtn, 'Stopping...')) return;
        try {
          if (!isEntityReady()) return;
          const active = getActiveTimer();
          const isSame = active && String(active.entity_type) === String(state.entityType) &&
            parseInt(active.entity_id, 10) === parseInt(state.entityId, 10);
          if (!isSame) return;
          try {
            let updated = active;
            if (!active.pause_started_at_ts) {
              updated = await postTimerAction('stop', state.entityType, state.entityId, false);
            }
            const timerObj = {
              entity_type: updated.entity_type,
              entity_id: updated.entity_id,
              started_at: updated.started_at || null,
              started_at_ts: updated.started_at_ts || null,
              paused_total_ms: updated.paused_total_ms || 0,
              pause_started_at: updated.pause_started_at || null,
              pause_started_at_ts: updated.pause_started_at_ts || null,
              status: 'stopped'
            };
            setActiveTimerObj(timerObj);
            notifyActiveTimerChange();
            if (timerInterval) {
              clearInterval(timerInterval);
              timerInterval = null;
            }
            updateTimerDisplay();
            updateTimerButtons(timerObj);
            const elapsed = calcElapsedMs(timerObj);
            if (minutesInput) minutesInput.value = Math.max(1, Math.ceil(elapsed / 60000));
            updateHumanMinutes();
            showForm(true);
          } catch (e) {
            setAlert(e.message || 'Unable to stop timer');
          }
        } finally {
          endButtonAction(stopBtn);
        }
      });
    }
    if (resetBtn) {
      resetBtn.addEventListener('click', async () => {
        if (!state.canUse) return;
        if (!beginButtonAction(resetBtn, 'Resetting...')) return;
        try {
          if (!isEntityReady()) return;
          const labelPlain = buildEntityLabel(state.entityType, state.entityId);
          const confirmMsg = `Reset timer for ${labelPlain}? This will clear the current timer without logging time.`;
          const confirmed = await confirmTimerSwitch(confirmMsg, null, null, 'Reset Timer');
          if (!confirmed) return;
          try {
            await clearServerTimer(state.entityType, state.entityId, true);
          } catch (e) {}
          resetTimer();
        } finally {
          endButtonAction(resetBtn);
        }
      });
    }
    if (logBtn) {
      logBtn.addEventListener('click', async () => {
        if (!state.canUse) return;
        if (!beginButtonAction(logBtn, 'Saving...')) return;
        try {
          const ok = await createLog();
          if (!ok) return;
          try {
            await clearServerTimer(state.entityType, state.entityId, true);
          } catch (e) {}
          clearTimerStart(state.entityType, state.entityId);
          updateTimerButtons(null);
          showForm(false);
          updateHumanMinutes();
        } finally {
          endButtonAction(logBtn);
        }
      });
    }

    function showForm(show) {
      if (!formWrap) return;
      formWrap.classList.toggle('d-none', !show);
    }

    function updateTimerButtons(active) {
      const same = active && String(active.entity_type) === String(state.entityType) &&
        parseInt(active.entity_id, 10) === parseInt(state.entityId, 10);
      if (!toggleBtn) return;
      if (!same) {
        toggleBtn.textContent = 'Start';
        if (stopBtn) stopBtn.classList.add('d-none');
        return;
      }
      if (active.pause_started_at_ts || active.status === 'paused' || active.status === 'stopped') {
        toggleBtn.textContent = 'Resume';
      } else {
        toggleBtn.textContent = 'Pause';
      }
      if (stopBtn) stopBtn.classList.remove('d-none');
    }

    function syncFromStorage() {
      if (!isEntityReady()) return;
      migrateLegacyTimer(state.entityType, state.entityId);
      const active = getActiveTimer();
      const same = active && String(active.entity_type) === String(state.entityType) &&
        parseInt(active.entity_id, 10) === parseInt(state.entityId, 10);
      if (same) {
        if (active.status === 'stopped') {
          showForm(true);
          updateHumanMinutes();
        }
        updateTimerButtons(active);
        if (active.pause_started_at_ts) {
          if (timerInterval) {
            clearInterval(timerInterval);
            timerInterval = null;
          }
          updateTimerDisplay();
        } else {
          if (timerInterval) clearInterval(timerInterval);
          timerInterval = setInterval(updateTimerDisplay, 1000);
          updateTimerDisplay();
        }
      } else {
        updateTimerButtons(null);
        updateTimerDisplay();
        updateHumanMinutes();
      }
    }

    const instance = {
      setEntity: (etype, eid) => {
        state.entityType = etype || state.entityType;
        state.entityId = eid ? parseInt(eid, 10) : 0;
        root.setAttribute('data-entity-type', state.entityType);
        root.setAttribute('data-entity-id', state.entityId ? String(state.entityId) : '');
        setAlert('');
        if (timerInterval) {
          clearInterval(timerInterval);
          timerInterval = null;
        }
        syncFromStorage();
        loadLogs();
      },
      refresh: () => loadLogs(),
      destroy: () => {
        if (timerInterval) {
          clearInterval(timerInterval);
          timerInterval = null;
        }
      }
    };

    instances.push(instance);

    // initial load
    if (!isEntityReady()) {
      setDisabled(true);
    } else {
      setDisabled(!state.canUse);
      syncActiveTimerFromServer().finally(() => {
        syncFromStorage();
      });
      loadLogs();
      if (minutesInput) {
        minutesInput.addEventListener('input', updateHumanMinutes);
        updateHumanMinutes();
      }
    }

    return instance;
  }

  const modal = ensureEditModal();
  if (modal && modal.save) {
    modal.save.addEventListener('click', async () => {
      if (modal.save.dataset.busy === '1') return;
      modal.save.dataset.busy = '1';
      if (window.toggleButtonLoading) window.toggleButtonLoading(modal.save, true, 'Saving...');
      try {
        await saveEdit();
      } finally {
        modal.save.dataset.busy = '0';
        if (window.toggleButtonLoading) window.toggleButtonLoading(modal.save, false);
      }
    });
  }

  window.TimeTracking = {
    init: createInstance,
    getActiveTimer
  };
})();
