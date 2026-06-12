/* eslint-disable */
(function () {
  "use strict";
  const pageData = window.ticketListData || {};
  const apiBase = pageData.apiBase || (typeof url_root !== 'undefined' ? url_root : '../');
  const ticketView = pageData.ticketView || 'received';
  const agentDeptId = pageData.agentDeptId || 0;
  const currentAgentId = pageData.currentAgentId || pageData.currentUserId || null;
  const hasTicketBroadcast = !!pageData.hasTicketBroadcast;
  const actorIsOwnDeptHead = !!pageData.actorIsOwnDeptHead;
  const actorOwnDeptId = parseInt(pageData.actorOwnDeptId || '0', 10) || 0;

  const apiTickets = apiBase + 'api/modules/tickets/index';
  const apiStatuses = apiBase + 'api/modules/tickets/statuses';
  const apiAgents = apiBase + 'api/modules/tickets/agents';
  const apiTicketTypes = apiBase + 'api/modules/config/ticket_types';
  const apiFiles = apiBase + 'api/modules/files/index';

  const ticketsBody = document.getElementById('ticketsBody');
  const loading = document.getElementById('ticketsLoading');
  const filterStatus = document.getElementById('filterStatus');
  const filterPriority = document.getElementById('filterPriority');
  const filterDepartment = document.getElementById('filterDepartment');
  const filterAgent = document.getElementById('filterAgent');
  const filterAssigned = document.getElementById('filterAssigned');
  const filterQuery = document.getElementById('filterQuery');
  const filterSearch = document.getElementById('filterSearch');

  const ticketDepartment = document.getElementById('ticketDepartment');
  const ticketAgent = document.getElementById('ticketAgent');
  const ticketType = document.getElementById('ticketType');
  const ticketFiles = document.getElementById('ticketFiles');
  const copyAllAgents = document.getElementById('copyAllAgents');
  const copyAllAgentsWrap = document.getElementById('copyAllAgentsWrap');
  const ticketCreateBtn = document.getElementById('ticketCreateBtn');
  const ticketCreateError = document.getElementById('ticketCreateError');
  const ticketForm = document.getElementById('newTicketForm');
  let currentDeptAgents = [];
  let ticketAgentSelect2Ready = false;
  let ticketAgentMulti = false;

  function selectedTicketAgentIds() {
    if (!ticketAgent) return [];
    if (ticketAgent.multiple && window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 && window.jQuery(ticketAgent).data('select2')) {
      const values = window.jQuery(ticketAgent).val();
      return Array.isArray(values) ? values.map((v) => String(v)).filter(Boolean) : (values ? [String(values)] : []);
    }
    const value = ticketAgent.value ? String(ticketAgent.value) : '';
    return value ? [value] : [];
  }

  function setTicketAgentIds(values) {
    if (!ticketAgent) return;
    const ids = Array.isArray(values) ? values.map((v) => String(v)).filter(Boolean) : (values ? [String(values)] : []);
    const nextIds = ticketAgent.multiple ? ids : (ids.length ? [ids[0]] : []);
    if (window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 && window.jQuery(ticketAgent).data('select2')) {
      window.jQuery(ticketAgent).val(ticketAgent.multiple ? nextIds : (nextIds[0] || null)).trigger('change.select2');
    } else if (ticketAgent.multiple) {
      Array.from(ticketAgent.options || []).forEach((opt) => {
        opt.selected = nextIds.includes(String(opt.value));
      });
    } else {
      ticketAgent.value = nextIds[0] || '';
    }
  }

  function setTicketAgentMode(allowMultiple, placeholderText) {
    if (!ticketAgent) return;
    const nextMulti = !!allowMultiple;
    const nextPlaceholder = placeholderText || (nextMulti ? 'Select one or more users' : 'Ticket To');
    if (ticketAgentMulti === nextMulti && ticketAgentSelect2Ready && window.jQuery && window.jQuery(ticketAgent).data('select2')) {
      return;
    }
    const selected = selectedTicketAgentIds();
    if (window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 && window.jQuery(ticketAgent).data('select2')) {
      try {
        window.jQuery(ticketAgent).select2('destroy');
      } catch (e) {}
    }
    if (nextMulti) {
      ticketAgent.setAttribute('multiple', 'multiple');
    } else {
      ticketAgent.removeAttribute('multiple');
    }
    ticketAgentMulti = nextMulti;
    if (window.jQuery && window.jQuery.fn && window.jQuery.fn.select2) {
      window.jQuery(ticketAgent).select2({
        width: '100%',
        dropdownParent: window.jQuery('#newTicketModal'),
        placeholder: nextPlaceholder,
        allowClear: true
      });
      ticketAgentSelect2Ready = true;
      setTicketAgentIds(selected);
    }
  }

  function buildAgentOptions(selectEl, agents) {
    if (!selectEl) return;
    const keepFirst = selectEl === ticketAgent;
    selectEl.innerHTML = keepFirst ? '<option value="">Select recipient</option>' : '<option value="">All agents</option>';
    (agents || []).forEach((a) => {
      const opt = document.createElement('option');
      const agentId = a.id || a.agent_id || a.user_id || '';
      opt.value = agentId;
      opt.textContent = a.display_name || a.name || ('Agent #' + agentId);
      if (a.department_id) {
        opt.dataset.departmentId = String(a.department_id);
      }
      selectEl.appendChild(opt);
    });
    if (typeof window.normalizeSelectOptions === 'function') {
      window.normalizeSelectOptions(selectEl);
    }
    if (selectEl === ticketAgent && window.jQuery && window.jQuery.fn && window.jQuery.fn.select2) {
      if (ticketAgentSelect2Ready && window.jQuery(selectEl).data('select2')) {
        window.jQuery(selectEl).trigger('change.select2');
      }
    }
  }

  function initTicketAgentSelect2() {
    if (!ticketAgent || !(window.jQuery && window.jQuery.fn && window.jQuery.fn.select2)) return;
    setTicketAgentMode(ticketAgentMulti, ticketAgentMulti ? 'Select one or more users' : 'Ticket To');
  }

  function selectedDepartmentOption() {
    if (!ticketDepartment || !ticketDepartment.selectedOptions || !ticketDepartment.selectedOptions.length) return null;
    return ticketDepartment.selectedOptions[0] || null;
  }

  function selectedDepartmentHeadAgentId() {
    const opt = selectedDepartmentOption();
    if (!opt || !opt.dataset || !opt.dataset.departmentHeadAgentId) return null;
    const headId = parseInt(opt.dataset.departmentHeadAgentId, 10);
    return Number.isFinite(headId) && headId > 0 ? headId : null;
  }

  function isCivilServiceDepartment() {
    const opt = selectedDepartmentOption();
    return !!(opt && opt.dataset && String(opt.dataset.ticketRoutingEnabled || '') === '1');
  }

  function canBroadcastSelectedDepartment() {
    if (hasTicketBroadcast) return true;
    if (!actorIsOwnDeptHead || actorOwnDeptId <= 0) return false;
    const opt = selectedDepartmentOption();
    const selectedDeptId = opt ? parseInt(opt.value || '0', 10) : 0;
    return selectedDeptId > 0 && selectedDeptId === actorOwnDeptId;
  }

  function updateBroadcastControls() {
    if (!copyAllAgentsWrap || !copyAllAgents) return;
    const deptSelected = !!(ticketDepartment && String(ticketDepartment.value || '').trim() !== '');
    const canBroadcast = deptSelected && canBroadcastSelectedDepartment();
    copyAllAgentsWrap.classList.toggle('d-none', !canBroadcast);
    if (!canBroadcast) {
      copyAllAgents.checked = false;
    }
  }

  function selectedAgentDepartmentId(selectEl) {
    if (!selectEl || !selectEl.selectedOptions || !selectEl.selectedOptions.length) return null;
    const opt = selectEl.selectedOptions[0];
    if (!opt || !opt.dataset || !opt.dataset.departmentId) return null;
    const num = parseInt(opt.dataset.departmentId, 10);
    return Number.isFinite(num) && num > 0 ? num : null;
  }

  function loadTicketTypes() {
    if (!ticketType) return;
    fetch(apiTicketTypes, { credentials: 'same-origin' })
      .then((r) => r.json())
      .then((data) => {
        const items = data.data || [];
        items.forEach((t) => {
          const opt = document.createElement('option');
          opt.value = t.code;
          opt.textContent = t.label || t.code;
          ticketType.appendChild(opt);
        });
      })
      .catch(() => {});
  }

  function loadAgents(deptId, selectEl, options = {}) {
    if (!selectEl) return;
    const url = new URL(apiAgents, window.location.origin);
    const mode = options.mode || 'filter';
    if (deptId) url.searchParams.set('department_id', deptId);
    if (mode) url.searchParams.set('mode', mode);
    return fetch(url.toString(), { credentials: 'same-origin' })
      .then((r) => r.json())
      .then((data) => {
        const list = data.data || [];
        if (selectEl === ticketAgent) currentDeptAgents = list;
        buildAgentOptions(selectEl, list);
        return list;
      })
      .catch(() => {
        buildAgentOptions(selectEl, []);
        return [];
      });
  }

  function syncCreateAssignees(preserveAgentId = null, preserveAgentName = '') {
    if (!ticketAgent || !ticketDepartment) return;
    const deptId = ticketDepartment.value || '';
    const hasDept = String(deptId || '').trim() !== '';
    const allowMultiple = hasDept && canBroadcastSelectedDepartment();
    setTicketAgentMode(allowMultiple, allowMultiple ? 'Select one or more users' : 'Ticket To');
    const loadDeptId = hasTicketBroadcast ? '' : deptId;
    loadAgents(loadDeptId, ticketAgent, { mode: loadDeptId ? 'picker' : 'filter' }).then((list) => {
      const defaultRow = Array.isArray(list) && list.length ? list[0] : null;
      const desiredIds = Array.isArray(preserveAgentId)
        ? preserveAgentId.map((v) => String(v)).filter(Boolean)
        : (preserveAgentId ? [String(preserveAgentId)] : selectedTicketAgentIds());
      const hasOption = Array.isArray(list) && list.some((row) => desiredIds.includes(String(row.id || row.agent_id || '')));
      const defaultId = defaultRow && defaultRow.agent_id ? String(defaultRow.agent_id) : '';
      if (allowMultiple) {
        const kept = Array.isArray(list)
          ? desiredIds.filter((desired) => list.some((row) => String(row.id || row.agent_id || '') === desired))
          : [];
        const nextIds = kept.length ? kept : (defaultId ? [defaultId] : []);
        setTicketAgentIds(nextIds);
      } else {
        let nextId = '';
        if (desiredIds.length) {
          nextId = hasOption ? desiredIds[0] : '';
        }
        if (!nextId) {
          nextId = defaultId;
        }
        if (!nextId && !hasDept && desiredIds.length) {
          nextId = desiredIds[0];
          const opt = document.createElement('option');
          opt.value = nextId;
          opt.textContent = preserveAgentName || ('Agent #' + nextId);
          ticketAgent.appendChild(opt);
        }
        setTicketAgentIds(nextId ? [nextId] : []);
      }
      updateBroadcastControls();
    });
  }

  function syncCreateDepartmentFromAgent() {
    if (!ticketDepartment || !ticketAgent) return;
    if (ticketAgentMulti || isCivilServiceDepartment()) return;
    const agentDeptId = selectedAgentDepartmentId(ticketAgent);
    if (agentDeptId && String(ticketDepartment.value || '') !== String(agentDeptId)) {
      ticketDepartment.value = String(agentDeptId);
    }
  }

  function loadStatuses() {
    if (!filterStatus) return;
    fetch(apiStatuses, { credentials: 'same-origin' })
      .then((r) => r.json())
      .then((data) => {
        const items = data.data || [];
        items.forEach((s) => {
          const opt = document.createElement('option');
          opt.value = s.code;
          opt.textContent = s.label || s.code;
          filterStatus.appendChild(opt);
        });
      })
      .catch(() => {});
  }

  let ticketsTable = null;
  function ensureTicketsTable() {
    if (ticketsTable) return ticketsTable;
    if (!window.jQuery || !$.fn.DataTable) return null;
    ticketsTable = $('#ticketsTable').DataTable({
      processing: true,
      serverSide: true,
      searching: false,
      lengthChange: false,
      pageLength: (window.appConfig && window.appConfig.dataTablePageSize) ? window.appConfig.dataTablePageSize : 250,
      order: [[7, 'desc']],
      ajax: {
        url: apiTickets,
        type: 'GET',
        data: function (d) {
          if (ticketView) d.view = ticketView;
          if (filterStatus && filterStatus.value) d.status = filterStatus.value;
          if (filterPriority && filterPriority.value) d.priority = filterPriority.value;
          if (filterDepartment && filterDepartment.value) d.department_id = filterDepartment.value;
          if (filterAgent && filterAgent.value) d.agent_id = filterAgent.value;
          if (filterAssigned && filterAssigned.checked) d.assigned_to_me = '1';
          if (ticketView === 'sent') d.created_by = 'me';
          if (filterQuery && filterQuery.value.trim()) d.q = filterQuery.value.trim();
        }
      },
      columns: [
        { data: null, orderable: false, render: (d, t, row, meta) => meta.row + meta.settings._iDisplayStart + 1 },
        { data: null, orderable: false, render: (data, type, row) => {
            const viewId = row.id_s || row.id_salt || row.id;
            const link = viewId ? `studio/tickets/view.kml?id=${encodeURIComponent(viewId)}` : '#';
            return `<div><a href="${link}">${row.ticket_number || ('Ticket #' + row.id)}</a></div>${buildInlineActions(row)}`;
          }
        },
        { data: null, orderable: false, render: (data, type, row) => {
            const yourTurnBadge = row.needs_action ? '<br><span class="badge bg-danger text-white fw-semibold">Your Turn</span>' : '';
            return `${row.subject || '-'}${yourTurnBadge}`;
          }
        },
        { data: null, orderable: false, render: (data, type, row) => {
            const senderName = row.created_by_name || row.created_by_email || 'Unknown';
            const senderDept = row.created_by_department || '-';
            return `${senderName}<br><small class="text-muted">${senderDept}</small>`;
          }
        },
        { data: null, orderable: false, render: (data, type, row) => {
            const assignedLabel = currentAssigneeName(row);
            return `${assignedLabel}<br><small class="text-muted">${row.department_name || '-'}</small>`;
          }
        },
        { data: null, orderable: false, render: (data, type, row) => {
            const targetDept = row.route_target_department_name || row.department_name || '-';
            const intended = row.intended_agent_name || '';
            const intendedLine = intended ? `<br><small class="text-muted">${intended}</small>` : '';
            return `${targetDept}${intendedLine}`;
          }
        },
        { data: null, orderable: false, render: (data, type, row) => {
            const status = (row.status || '').toLowerCase();
            const needsAttention = ['new', 'pending'].includes(status);
            const attentionBadge = needsAttention ? '<span class="badge bg-danger-subtle text-danger ms-1">Attention</span>' : '';
            return `${statusBadge(row.status)}${attentionBadge}<br>${priorityBadge(row.priority)}`;
          }
        },
        { data: 'created_at' },
        { data: null, orderable: false, render: (data, type, row) => {
            const viewId = row.id_s || row.id_salt || row.id;
            const link = viewId ? `studio/tickets/view.kml?id=${encodeURIComponent(viewId)}` : '#';
            return `<div class="text-end"><a class="btn btn-sm btn-primary" href="${link}"><i class="ri-eye-line me-1"></i>View</a></div>`;
          }
        }
      ]
    });
    if (loading) {
      $('#ticketsTable').on('processing.dt', function (e, settings, processing) {
        loading.classList.toggle('d-none', !processing);
      });
    }
    return ticketsTable;
  }

  function statusBadge(code) {
    const val = (code || '').toLowerCase();
    const map = {
      new: 'bg-primary text-white',
      open: 'bg-info text-white',
      pending: 'bg-warning text-white',
      resolved: 'bg-success text-white',
      closed: 'bg-danger text-white'
    };
    const cls = map[val] || 'bg-soft-secondary text-secondary';
    return `<span class="badge ${cls}">${val || '-'}</span>`;
  }

  function priorityBadge(code) {
    const val = (code || '').toLowerCase();
    const map = {
      low: 'bg-success text-white',
      medium: 'bg-primary text-white',
      high: 'bg-warning text-white',
      urgent: 'bg-danger text-white'
    };
    const cls = map[val] || 'bg-secondary text-secondary';
    return `<span class="badge ${cls}">${val || '-'}</span>`;
  }

  function buildInlineActions(row) {
    const viewId = row.id_s || row.id_salt || row.id;
    if (!viewId) return '';
    return `<div class="ticket-inline-actions small mt-1">
      <a href="studio/tickets/view.kml?id=${encodeURIComponent(viewId)}">View Ticket</a>
    </div>`;
  }

  function currentAssigneeName(row) {
    if (row.assigned_agent_current) return row.assigned_agent_current;
    if (row.assigned_agent_name) return row.assigned_agent_name;
    if (row.current_assignee_name) return row.current_assignee_name;
    if (row.assigned_agent_names) {
      const parts = String(row.assigned_agent_names).split(',');
      return parts[parts.length - 1].trim();
    }
    return row.department_name ? `All Users (${row.department_name})` : 'All Users';
  }

  function loadTickets() {
    const existed = !!ticketsTable;
    const table = ensureTicketsTable();
    if (!table) return;
    // DataTable init already issues the first request; avoid duplicate draw on first load.
    if (existed) {
      table.ajax.reload();
    }
  }

  if (filterSearch) filterSearch.addEventListener('click', () => loadTickets());
  if (filterDepartment) {
    filterDepartment.addEventListener('change', () => {
      loadAgents(filterDepartment.value, filterAgent);
    });
  }
  if (filterStatus) filterStatus.addEventListener('change', () => loadTickets());
  if (filterPriority) filterPriority.addEventListener('change', () => loadTickets());
  if (filterAssigned) filterAssigned.addEventListener('change', () => loadTickets());

  if (ticketDepartment) {
    ticketDepartment.addEventListener('change', () => {
      if (ticketAgent) {
        setTicketAgentIds([]);
      }
      syncCreateAssignees(ticketAgent ? ticketAgent.value : null, ticketAgent && ticketAgent.selectedOptions && ticketAgent.selectedOptions[0] ? ticketAgent.selectedOptions[0].textContent : '');
      updateBroadcastControls();
    });
  }

  if (ticketAgent) {
    ticketAgent.addEventListener('change', () => {
      const beforeDept = ticketDepartment ? String(ticketDepartment.value || '') : '';
      syncCreateDepartmentFromAgent();
      const afterDept = ticketDepartment ? String(ticketDepartment.value || '') : '';
      if (beforeDept !== afterDept && !ticketAgentMulti) {
        const selectedOpt = ticketAgent && ticketAgent.selectedOptions && ticketAgent.selectedOptions.length
          ? ticketAgent.selectedOptions[0]
          : null;
        syncCreateAssignees(ticketAgent.value || null, selectedOpt ? selectedOpt.textContent : '');
      }
      updateBroadcastControls();
    });
  }

  if (window.jQuery && window.jQuery.fn && window.jQuery.fn.select2) {
    initTicketAgentSelect2();
    if (ticketAgent && ticketDepartment) {
      const modalEl = document.getElementById('newTicketModal');
      if (modalEl) {
        modalEl.addEventListener('shown.bs.modal', initTicketAgentSelect2);
      }
    }
  }

  async function uploadTicketFiles() {
    if (!ticketFiles || !ticketFiles.files || !ticketFiles.files.length) return [];
    const newIds = [];
    for (const f of Array.from(ticketFiles.files)) {
      const fd = new FormData();
      const subjText = (ticketForm?.querySelector('input[name="subject"]')?.value || 'ticket').trim();
      const ext = (f.name && f.name.includes('.')) ? f.name.substring(f.name.lastIndexOf('.')) : '';
      const niceName = `${subjText.replace(/\s+/g, '_')}_${Date.now()}${ext}`;
      fd.append('file', f, niceName);
      const res = await fetch(apiFiles, { method: 'POST', credentials: 'same-origin', body: fd });
      const data = await res.json().catch(() => ({}));
      if (!res.ok || data.status === 'error') throw new Error(data?.message || 'Upload failed');
      if (data.id) newIds.push(data.id);
    }
    return newIds;
  }

  async function handleCreateTicket() {
    if (!ticketForm || !ticketCreateBtn) return;
    ticketCreateError.classList.add('d-none');
    const btnLabel = ticketCreateBtn.querySelector('.label');
    const btnSpinner = ticketCreateBtn.querySelector('.spinner-border');
    if (btnLabel && btnSpinner) {
      btnLabel.classList.add('d-none');
      btnSpinner.classList.remove('d-none');
    }
    ticketCreateBtn.disabled = true;

    try {
      const payload = {
        subject: ticketForm.subject.value.trim(),
        description: ticketForm.description.value.trim(),
        department_id: ticketDepartment ? (ticketDepartment.value || null) : null,
        intended_agent_id: ticketAgent ? (selectedTicketAgentIds()[0] || null) : null,
        assigned_agent: ticketAgent ? (ticketAgent.multiple ? (selectedTicketAgentIds()[0] || null) : (ticketAgent.value || null)) : null,
        assigned_agents: ticketAgent ? selectedTicketAgentIds() : [],
        priority: ticketForm.priority.value,
        ticket_type: ticketType ? (ticketType.value || null) : null,
        copy_all: copyAllAgents && copyAllAgents.checked ? 1 : 0,
        copy_all_dept: copyAllAgents && copyAllAgents.checked ? 1 : 0
      };

      const fileIds = await uploadTicketFiles();
      if (fileIds && fileIds.length) payload.file_ids = fileIds;

      const res = await fetch(apiTickets, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify(payload)
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok || data.status === 'error') {
        throw new Error(data.message || data.error || data.detail || ('Unable to create ticket' + (res.status ? ` (${res.status})` : '')));
      }
      const modal = bootstrap.Modal.getInstance(document.getElementById('newTicketModal'));
      if (modal) modal.hide();
      ticketForm.reset();
      if (ticketFiles) ticketFiles.value = '';
      if (ticketType) ticketType.value = '';
      // Redirect to sent view to show the newly created ticket immediately
      window.location.href = '/studio/tickets/index.kml?view=sent';
    } catch (err) {
      ticketCreateError.textContent = err.message || 'Unable to create ticket';
      ticketCreateError.classList.remove('d-none');
    } finally {
      if (btnLabel && btnSpinner) {
        btnLabel.classList.remove('d-none');
        btnSpinner.classList.add('d-none');
      }
      ticketCreateBtn.disabled = false;
    }
  }

  if (ticketCreateBtn) {
    ticketCreateBtn.addEventListener('click', (e) => {
      e.preventDefault();
      handleCreateTicket();
    });
  }

  if (ticketForm) {
    ticketForm.addEventListener('submit', (e) => {
      e.preventDefault();
      handleCreateTicket();
    });
  }

  if (ticketView === 'received' && filterAssigned) {
    const hasDeptScope = agentDeptId > 0;
    if (!hasDeptScope) {
      filterAssigned.checked = true;
      filterAssigned.disabled = true;
    } else {
      filterAssigned.checked = false;
      filterAssigned.disabled = false;
    }
  }

  loadStatuses();
  loadTicketTypes();
  if (filterAgent) loadAgents(agentDeptId || '', filterAgent);
  if (ticketDepartment && agentDeptId) {
    const deptOption = Array.from(ticketDepartment.options || []).find((opt) => String(opt.value) === String(agentDeptId));
    if (deptOption) {
      ticketDepartment.value = String(agentDeptId);
    }
  }
  if (ticketAgent) {
    syncCreateAssignees();
  }
  updateBroadcastControls();
  loadTickets();
})();
