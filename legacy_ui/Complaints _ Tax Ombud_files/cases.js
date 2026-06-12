/* eslint-disable */
(function() {
  if (window.__casesPageInitialized) return;
  window.__casesPageInitialized = true;

  const cfg = window.casesConfig || {};
  const moduleLabel = (typeof window.moduleLabel === 'function')
    ? window.moduleLabel
    : (key, form) => {
      const defaults = {
        account: { singular: 'Account', plural: 'Accounts' },
        contact: { singular: 'Contact', plural: 'Contacts' },
        organization: { singular: 'Organization', plural: 'Organizations' },
        case: { singular: 'Case', plural: 'Cases' }
      };
      const k = String(key || '').toLowerCase();
      const f = String(form || 'plural').toLowerCase() === 'singular' ? 'singular' : 'plural';
      return (defaults[k] && defaults[k][f]) || k;
    };
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiCases = cfg.apiCases || apiMap.casesIndex;
  const apiDetail = cfg.apiDetail || apiMap.casesDetail;
  const apiStatuses = cfg.apiStatuses || apiMap.casesStatuses;
  const apiExport = cfg.apiExport || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/cases/export');
  const apiCategories = cfg.apiCategories || apiMap.caseCategories || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/case-categories/index');
  const fallbackBase = (typeof url_root !== 'undefined' ? url_root : '../');
  const canDelete = !!cfg.canDelete;
  const canFilterAgents = !!cfg.canFilterAgents;
  const canReadAllCases = !!cfg.canReadAll;
  const isSuperAdmin = !!cfg.isSuperAdmin;
  const hasReadAll = !!cfg.hasReadAll;
  const agentDomainId = parseInt(cfg.agentDomainId || 0, 10) || 0;
  const agentDomainIds = Array.isArray(cfg.agentDomainIds)
    ? cfg.agentDomainIds.map(v => parseInt(v, 10)).filter(v => v && !Number.isNaN(v))
    : [];
  const defaultAssignedToMe = !!cfg.defaultAssignedToMe;
  const timeTrackingEnabled = !!cfg.timeTrackingEnabled;
  const timeTrackingCanManage = !!cfg.timeTrackingCanManage;
  const timeTrackingCanUse = !!cfg.timeTrackingCanUse || timeTrackingCanManage;

  if (typeof window.renderStatusBadge !== 'function' || typeof window.renderPriorityBadge !== 'function' || typeof window.setupCaseForm !== 'function') {
    return;
  }

  const caseAlert = document.getElementById('caseAlert');
  const filterText = document.getElementById('filterText');
  const resetFilters = document.getElementById('btnResetFilters');
  const btnSearch = document.getElementById('btnSearch');
  const filterStatus = document.getElementById('filterStatus');
  const filterSearchBy = document.getElementById('filterSearchBy');
  const filterMine = document.getElementById('filterMine');
  const agentList = document.getElementById('agentList');
  const filterCategory = document.getElementById('filterCategory');
  const quickTableSearch = document.getElementById('quickTableSearch');
  const exportBtn = document.getElementById('caseExportBtn');
  const tableEl = document.getElementById('caseTable');
  if (!tableEl) return;
  if ($.fn.dataTable.isDataTable(tableEl)) return;
  const tableNode = $('#caseTable');

  // Apply mine-default before DataTable init so first request honors it.
  if (filterMine && defaultAssignedToMe) {
    filterMine.checked = true;
    if (!canReadAllCases) {
      filterMine.disabled = true;
      filterMine.title = 'Assigned-to-me is enforced for your role.';
    }
  }

  let suppressAutoReload = true;
  const caseDateRange = document.getElementById('caseDateRange');
  const datePicker = caseDateRange ? flatpickr(caseDateRange, {
    mode: 'range',
    dateFormat: 'Y-m-d',
    allowInput: true,
    onClose: () => {
      if (!suppressAutoReload) reloadCases();
    }
  }) : null;

  const table = tableNode.DataTable({
    serverSide: true,
    processing: true,
    searching: false,
    pageLength: (window.appConfig && window.appConfig.dataTablePageSize) ? window.appConfig.dataTablePageSize : 250,
    lengthMenu: [[250],[250]],
    ajax: {
      url: apiCases,
      type: 'GET',
      data: function(d) {
        const q = filterText ? filterText.value.trim() : '';
        const searchBy = filterSearchBy ? filterSearchBy.value : 'all';
        const statusVal = filterStatus ? filterStatus.value : '';
        if (q) d.q = q;
        if (q && searchBy && searchBy !== 'all') d.search_by = searchBy;
        if (statusVal) d.status = statusVal;
        if (filterCategory && filterCategory.value) d.domain_id = filterCategory.value;
        if (canFilterAgents && agentList && agentList.value) d.agent_id = agentList.value;
        if (filterMine) {
          d.assigned_to_me = filterMine.checked ? '1' : '0';
        }
        // Default list view hides closed cases; searching should include all statuses.
        if (!q && !statusVal) {
          d.exclude_closed = '1';
        }
        if (datePicker && datePicker.selectedDates.length) {
          const [start, end] = datePicker.selectedDates;
          const startLocal = formatLocalDate(start);
          const endLocal = end ? formatLocalDate(end) : startLocal;
          if (startLocal) d.start_date = startLocal;
          if (endLocal) d.end_date = endLocal;
        }
        d.limit = d.length;
      },
      dataSrc: function(json) {
        return json.data || [];
      },
      error: function() {
        showCaseAlert('Unable to load cases');
      }
    },
    columns: [
      {
        data: null,
        render: function(data, type, row, meta) {
          return meta.row + meta.settings._iDisplayStart + 1;
        }
      },
      {
        data: null,
        render: function(data, type, row) {
          const id = row.id_s || row.id;
          const label = row.case_number || row.id || '';
          const link = id ? `<div style="white-space:nowrap"><a href="studio/cases/view.kml?id=${encodeURIComponent(id)}">${escapeHtml(label)}</a></div>${buildInlineActions(row)}` : escapeHtml(label);
          return link;
        }
      },
      {
        data: 'subject',
        render: function(data, type, row) {
          const full = data || '';
          const truncated = full.length > 80 ? escapeHtml(full.slice(0, 80)) + '&hellip;' : escapeHtml(full);
          const attention = renderWorkflowAttentionBadge(row);
          const subjectHtml = `<span title="${escapeHtml(full)}">${truncated}</span>`;
          if (!attention) return subjectHtml;
          return `<div>${subjectHtml}</div>${attention}`;
        }
      },
      {
        data: null,
        render: function(data, type, row) {
          const assigned = row.assigned_agent_names ? `Assigned: ${escapeHtml(row.assigned_agent_names)}` : 'Not Assigned';
          return `<div class="small text-muted">${assigned}</div>
         <span class="status-priority mt-1">${renderStatusBadge(row.status)} ${renderPriorityBadge(row.priority)}</span>`;
        }
      },
      {
        data: null,
        render: function(data, type, row) {
          if ((window.casesConfig && window.casesConfig.featureLeads) && (row.lead_id || row.lead_id_s)) {
            const leadId = row.lead_id_s || row.lead_id;
            const leadLabel = escapeHtml(row.lead_title || ('Lead #' + (row.lead_id || '')));
            return `<a href="studio/leads/view.kml?id=${encodeURIComponent(leadId)}">${leadLabel}</a><br>
          <span class="badge bg-warning-subtle text-warning me-1">Lead</span>`;
          }
          if (row.organization_id || row.organization_id_s) {
            const orgId = row.organization_id_s || row.organization_id;
            const orgLabel = escapeHtml(row.organization_name || ('organization #' + (row.organization_id || '')));
            const canLinkOrg = !!(window.casesConfig && window.casesConfig.featureOrganizations);
            return `${canLinkOrg ? `<a href="studio/organizations/view.kml?id=${encodeURIComponent(orgId)}">${orgLabel}</a>` : orgLabel}<br>
          <span class="badge bg-primary-subtle text-primary me-1">${escapeHtml(moduleLabel('organization', 'singular'))}</span>`;
          }
          if (row.owner_contact_id || row.owner_contact_id_s) {
            const contactId = row.owner_contact_id_s || row.owner_contact_id;
            return `<a href="studio/contacts/view.kml?id=${encodeURIComponent(contactId)}">${escapeHtml(row.owner_contact_name || (moduleLabel('contact', 'singular') + ' #' + (row.owner_contact_id || '')))}</a> <br>
          <span class="badge bg-info-subtle text-info me-1">${escapeHtml(moduleLabel('contact', 'singular'))}</span>`;
          }
          if (row.owner_user_id) {
            return `<span class="badge bg-success-subtle text-success me-1">User</span><a href="studio/settings/setting-users.kml?id=${encodeURIComponent(row.owner_user_id)}">${escapeHtml(row.owner_user_name || ('User #' + row.owner_user_id))}</a>`;
          }
          return 'N/A';
        }
      },
      {
        data: 'created_at',
        render: function(data) {
          return escapeHtml(data || '');
        }
      },
      {
        data: null,
        orderable: false,
        searchable: false,
        render: function(data, type, row) {
          const id = row.id_s || row.id;
          return `<div class="d-inline-flex justify-content-end align-items-center gap-1 flex-nowrap actions-col">
            <button class="btn btn-soft-primary btn-sm btn-quick-view" data-case-id="${row.id}" title="Summary"><i class="ri-eye-2-line me-1"></i>Summary</button>
            <a href="studio/cases/view.kml?id=${encodeURIComponent(id)}" class="btn btn-primary btn-sm" title="View"><i class="ri-eye-line me-1"></i>View</a>
            ${canDelete && row.id ? `<button class="btn btn-soft-danger btn-sm btn-delete-case" data-case-id="${row.id}" title="Delete"><i class="ri-delete-bin-6-line"></i></button>` : ''}
        </div>`;
        }
      }
    ]
  });

  const modal = new bootstrap.Modal(document.getElementById('caseModal'));
  const loadingCover = document.createElement('div');
  loadingCover.style.position = 'absolute';
  loadingCover.style.inset = '0';
  loadingCover.style.background = 'rgba(255,255,255,0.7)';
  loadingCover.style.display = 'none';
  loadingCover.style.zIndex = '10';
  loadingCover.style.justifyContent = 'center';
  loadingCover.style.alignItems = 'center';
  loadingCover.innerHTML = '<div class="d-flex justify-content-center align-items-center h-100 text-primary"><span class="spinner-border me-2" role="status"></span><span>Loading Data. Please Wait...</span></div>';
  const tableCard = document.querySelector('#caseTable')?.closest('.card');
  if (tableCard) {
    tableCard.style.position = 'relative';
    tableCard.appendChild(loadingCover);
  }
  function setTableLoading(show) {
    if (!loadingCover) return;
    loadingCover.style.display = show ? 'flex' : 'none';
  }

  // Show as soon as request starts, hide when response lands.
  tableNode.on('preXhr.dt', function() { setTableLoading(true); });
  tableNode.on('xhr.dt', function() { setTableLoading(false); });
  tableNode.on('error.dt', function() { setTableLoading(false); });
  tableNode.on('draw.dt', function() { applyQuickTableFilter(); });
  table.on('processing.dt', function(_e, _settings, processing) {
    setTableLoading(!!processing);
  });

  function populateFilterStatuses(list) {
    if (!filterStatus) return;
    const current = filterStatus.value;
    filterStatus.innerHTML = '<option value="">All status</option>';
    (list || []).forEach(s => {
      const val = s.code || s.label || '';
      if (!val) return;
      const opt = document.createElement('option');
      opt.value = val;
      opt.textContent = s.label || s.code || '';
      filterStatus.appendChild(opt);
    });
    if (current) filterStatus.value = current;
  }

  function loadCategories() {
    if (!filterCategory) return;
    fetch(apiCategories)
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) return;
        const list = data.data || data || [];
        const current = filterCategory.value;
        filterCategory.innerHTML = '<option value="">All categories</option>';
        list.forEach(cat => {
          if (!cat.id) return;
          const opt = document.createElement('option');
          opt.value = cat.id;
          opt.textContent = cat.name || ('Category #' + cat.id);
          filterCategory.appendChild(opt);
        });
        if (!isSuperAdmin && agentDomainIds.length) {
          const allowed = list.filter(c => agentDomainIds.includes(parseInt(c.id, 10)));
          filterCategory.innerHTML = '<option value="">All categories</option>';
          allowed.forEach(cat => {
            const opt = document.createElement('option');
            opt.value = cat.id;
            opt.textContent = cat.name || ('Category #' + cat.id);
            filterCategory.appendChild(opt);
          });
          if (current && allowed.find(c => String(c.id) === String(current))) {
            filterCategory.value = current;
          } else if (allowed.length === 1) {
            filterCategory.value = String(allowed[0].id);
          }
          filterCategory.disabled = allowed.length <= 1;
        } else {
          if (current) filterCategory.value = current;
          filterCategory.disabled = false;
        }
      })
      .catch(() => {});
  }

  function loadStatusOptions() {
    const primaryUrl = apiStatuses || (fallbackBase + 'assets/json/case_statuses.json');
    const jsonUrl = fallbackBase + 'assets/json/case_statuses.json';

    function applyStatuses(list) {
      if (typeof window.setCaseStatusConfig === 'function') window.setCaseStatusConfig(list);
      populateFilterStatuses(list);
    }

    fetch(primaryUrl)
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error();
        const list = Array.isArray(data?.data) ? data.data : Array.isArray(data) ? data : [];
        if (!list.length) throw new Error();
        applyStatuses(list);
      })
      .catch(() => {
        // Primary (API) failed — fall back to the JSON source of truth directly
        if (primaryUrl === jsonUrl) return;
        fetch(jsonUrl)
          .then(r => { if (!r.ok) throw new Error(); return r.json(); })
          .then(data => { applyStatuses(Array.isArray(data) ? data : []); })
          .catch(() => {});
      });
  }

  function showCaseAlert(msg) {
    if (!caseAlert) return;
    caseAlert.textContent = msg || '';
  }

  function escapeHtml(str) {
    return String(str || '').replace(/[&<>"']/g, (m) => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]));
  }

  function formatCustomFieldLabelStandard(label, key) {
    if (label !== undefined && label !== null && String(label).trim() !== '') {
      return String(label).trim();
    }
    return String(key || '')
      .replace(/[_-]+/g, ' ')
      .replace(/\s+/g, ' ')
      .trim();
  }

  function renderWorkflowAttentionBadge(row) {
    const key = String((row && row.workflow_attention) || '').trim().toLowerCase();
    if (key === 'returned_to_you') {
      const label = String((row && row.workflow_attention_label) || 'Returned to You');
      return `<span class="badge bg-warning-subtle text-warning mt-1">${escapeHtml(label)}</span>`;
    }
    if (key === 'passed_for_recommendation') {
      const label = String((row && row.workflow_attention_label) || 'Passed for Recommendation');
      return `<span class="badge bg-danger-subtle text-danger fw-semibold mt-1">${escapeHtml(label)}</span>`;
    }
    if (key === 'pending_verification') {
      const label = String((row && row.workflow_attention_label) || 'Workflow: Verification');
      return `<span class="badge bg-info-subtle text-info fw-semibold mt-1">${escapeHtml(label)}</span>`;
    }
    return '';
  }

  function renderCaseCustomFieldsHtml(defs, values) {
    const list = Array.isArray(defs) ? defs : [];
    const fieldValues = (values && typeof values === 'object') ? values : {};
    const rows = [];
    list.forEach((def) => {
      const key = String((def && (def.field_key || def.key)) || '').trim();
      if (!key) return;
      const rawVal = fieldValues[key];
      if (rawVal === undefined || rawVal === null || rawVal === '') return;
      let displayVal = '';
      if (typeof CustomFieldRenderer !== 'undefined' && CustomFieldRenderer && typeof CustomFieldRenderer.formatDisplayValue === 'function') {
        displayVal = CustomFieldRenderer.formatDisplayValue(def, rawVal);
      } else if (Array.isArray(rawVal)) {
        displayVal = rawVal.join(', ');
      } else {
        displayVal = String(rawVal);
      }
      if (!displayVal) return;
      rows.push(
        `<div class="d-flex align-items-start gap-2 mb-1">` +
        `<span class="qv-label mb-0" style="min-width:140px;">${escapeHtml(formatCustomFieldLabelStandard(def.label, key))}:</span>` +
        `<div class="qv-value">${escapeHtml(displayVal)}</div>` +
        `</div>`
      );
    });
    return rows.join('');
  }

  function reloadCases() {
    table.ajax.reload();
  }

  function applyQuickTableFilter() {
    if (!quickTableSearch) return;
    const query = String(quickTableSearch.value || '').trim().toLowerCase();
    const rows = tableNode.find('tbody tr');
    if (!rows.length) return;
    if (!query) {
      rows.show();
      return;
    }
    rows.each(function() {
      const rowText = String($(this).text() || '').toLowerCase();
      $(this).toggle(rowText.indexOf(query) !== -1);
    });
  }

  function updateAgentFilterState() {
    if (!agentList) return;
    if (isSuperAdmin) {
      agentList.disabled = false;
      return;
    }
    const mine = filterMine ? filterMine.checked : false;
    let disable = false;
    if (!hasReadAll) {
      disable = true;
    } else if (mine && agentDomainIds.length === 0) {
      disable = true;
    }
    agentList.disabled = disable;
    if (disable) {
      agentList.value = '';
    }
  }

  function buildInlineActions(row) {
    const id = row.id_s || row.id;
    const viewId = id ? encodeURIComponent(id) : '';
    const parts = [];
    if (row.id) {
      parts.push(`<a href="javascript:void(0);" class="btn-quick-view" data-case-id="${row.id}">Summary</a>`);
    }
    if (viewId) {
      parts.push(`<a href="studio/cases/view.kml?id=${viewId}">View</a>`);
    }
    if (!parts.length) return '';
    return `<div class="case-inline-actions small mt-1">${parts.join(' <span class="text-muted">|</span> ')}</div>`;
  }

  function formatLocalDate(d) {
    if (!d) return null;
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  function buildExportParams() {
    const params = new URLSearchParams();
    const q = filterText ? filterText.value.trim() : '';
    const searchBy = filterSearchBy ? filterSearchBy.value : 'all';
    if (q) params.set('q', q);
    if (q && searchBy && searchBy !== 'all') params.set('search_by', searchBy);
    if (filterStatus && filterStatus.value) params.set('status', filterStatus.value);
    if (filterCategory && filterCategory.value) params.set('domain_id', filterCategory.value);
    if (canFilterAgents && agentList && agentList.value) params.set('agent_id', agentList.value);
    if (filterMine) {
      params.set('assigned_to_me', filterMine.checked ? '1' : '0');
    }
    if (datePicker && datePicker.selectedDates.length) {
      const [start, end] = datePicker.selectedDates;
      const startLocal = formatLocalDate(start);
      const endLocal = end ? formatLocalDate(end) : startLocal;
      if (startLocal) params.set('start_date', startLocal);
      if (endLocal) params.set('end_date', endLocal);
    }
    return params;
  }

  function showWorkflowRedirectToast() {
    const params = new URLSearchParams(window.location.search || '');
    const action = String(params.get('workflow_action') || '').toLowerCase();
    if (action !== 'pass') return;
    if (typeof window.showSavedModal === 'function') {
      window.showSavedModal('Workflow', 'Recommendation submitted forward.');
    }
    params.delete('workflow_action');
    params.delete('workflow_from');
    params.delete('case_number');
    if (window.history && typeof window.history.replaceState === 'function') {
      const qs = params.toString();
      const nextUrl = window.location.pathname + (qs ? ('?' + qs) : '');
      window.history.replaceState({}, '', nextUrl);
    }
  }

  const caseForm = setupCaseForm({
    formSelector: '#caseForm',
    saveBtn: '#caseSaveBtn',
    defaultParentType: 'organization',
    onSaved: () => {
      modal.hide();
      reloadCases();
    }
  });

  const btnNewCase = document.getElementById('btnNewCase');
  if (btnNewCase) {
    btnNewCase.addEventListener('click', () => {
      if (caseForm && typeof caseForm.reset === 'function') {
        caseForm.reset({ parentType: 'organization' });
      }
      modal.show();
    });
  }

  let deleteId = null;
  const deleteModalEl = document.getElementById('deleteCaseModal');
  const deleteModal = deleteModalEl ? new bootstrap.Modal(deleteModalEl) : null;
  const deleteConfirmBtn = document.getElementById('confirmDeleteCase');
  const deleteAlert = document.getElementById('deleteCaseAlert');

  const setDeleteAlert = (msg) => {
    if (!deleteAlert) return;
    deleteAlert.textContent = msg || '';
    deleteAlert.classList.toggle('d-none', !msg);
  };

  table.on('click', '.btn-delete-case', function() {
    const id = this.getAttribute('data-case-id');
    if (!id || !deleteModal) return;
    deleteId = id;
    setDeleteAlert('');
    deleteModal.show();
  });

  if (deleteConfirmBtn) {
    deleteConfirmBtn.addEventListener('click', () => {
      if (!deleteId) return;
      setDeleteAlert('');
      deleteConfirmBtn.disabled = true;
      fetch(apiCases + '?id=' + encodeURIComponent(deleteId), { method: 'DELETE' })
        .then(r => r.ok ? reloadCases() : r.json().then(d => { throw new Error(d.error || 'Unable to delete'); }))
        .then(() => { deleteModal && deleteModal.hide(); })
        .catch(err => setDeleteAlert(err.message || 'Unable to delete case'))
        .finally(() => { deleteConfirmBtn.disabled = false; });
    });
  }

  if (btnSearch) btnSearch.addEventListener('click', () => reloadCases());
  if (filterText) filterText.addEventListener('keypress', (e) => { if (e.key === 'Enter') { e.preventDefault(); reloadCases(); }});
  if (filterMine) filterMine.addEventListener('change', () => {
    updateAgentFilterState();
    reloadCases();
  });
  if (filterStatus) filterStatus.addEventListener('change', () => reloadCases());
  if (filterCategory) filterCategory.addEventListener('change', () => reloadCases());
  if (datePicker && caseDateRange) {
    caseDateRange.addEventListener('change', () => {
      if (!suppressAutoReload) reloadCases();
    });
  }
  if (exportBtn) {
    exportBtn.addEventListener('click', () => {
      const params = buildExportParams();
      const url = apiExport + (params.toString() ? ('?' + params.toString()) : '');
      window.location.href = url;
    });
  }
  if (resetFilters) {
    resetFilters.addEventListener('click', () => {
      if (filterText) filterText.value = '';
      if (filterStatus) filterStatus.value = '';
      if (filterSearchBy) filterSearchBy.value = 'all';
      if (agentList) agentList.value = '';
      if (datePicker) datePicker.clear();
      if (filterMine) filterMine.checked = defaultAssignedToMe;
      if (quickTableSearch) quickTableSearch.value = '';
      updateAgentFilterState();
      reloadCases();
    });
  }
  if (quickTableSearch) {
    quickTableSearch.addEventListener('input', applyQuickTableFilter);
    quickTableSearch.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') {
        quickTableSearch.value = '';
        applyQuickTableFilter();
      }
    });
  }
  loadStatusOptions();
  updateAgentFilterState();

  const quickOffcanvasEl = document.getElementById('caseQuickOffcanvas');
  const quickOffcanvas = quickOffcanvasEl ? new bootstrap.Offcanvas(quickOffcanvasEl) : null;
  const quickBody = document.getElementById('caseQuickBody');
  const quickTitle = document.getElementById('caseQuickTitle');
  const quickTimeTrackingTemplate = document.getElementById('caseQuickTimeTrackingTemplate');
  let quickTimeTracking = null;
  table.on('click', '.btn-quick-view', function() {
    const id = this.getAttribute('data-case-id');
    if (!id || !apiDetail || !quickOffcanvas) return;
    if (quickBody) quickBody.innerHTML = '<div class="text-center text-muted py-3"><span class="spinner-border spinner-border-sm me-1"></span>Loading...</div>';
    quickOffcanvas.show();
    fetch(apiDetail + '?id=' + encodeURIComponent(id))
      .then(r => { if (!r.ok) throw new Error('Unable to load case'); return r.json(); })
      .then(data => {
        if (quickTitle) quickTitle.textContent = (data.case_number ? data.case_number + ' - ' : '') + (data.subject || moduleLabel('case', 'singular'));
        if (quickTimeTracking && typeof quickTimeTracking.destroy === 'function') {
          quickTimeTracking.destroy();
          quickTimeTracking = null;
        }
        if (quickBody) {
          const customFieldsHtml = renderCaseCustomFieldsHtml(data.custom_field_defs || [], data.custom_fields || {});
          quickBody.innerHTML = `
            <div class="d-flex align-items-center mb-3">
              <div class="avatar-sm bg-primary-subtle text-primary rounded-circle d-flex align-items-center justify-content-center me-2">
                <i class="ri-briefcase-line fs-18"></i>
              </div>
              <div>
                <div class="fw-semibold">${(data.subject || moduleLabel('case', 'singular'))}</div>
                <div class="qv-subtle">${data.case_number || ''}</div>
              </div>
            </div>
            <div class="mb-3">${renderStatusBadge(data.status)} ${renderPriorityBadge(data.priority)}</div>
            <div class="qv-divider"></div>
            <div class="fw-semibold mb-1">Description</div>
            <div class="text-muted small case-desc">${data.description ? data.description.replace(/\n/g,'<br>') : 'No description'}</div>
            <div class="qv-section mt-2">
            <div class="qv-divider"></div>
              <span class="qv-label">Owner</span>
              <div class="qv-value">
                ${data.organization_id || data.organization_id_s ? (() => { const label = (data.organization_name || 'organization #' + (data.organization_id || '')); const canLinkOrg = !!(window.casesConfig && window.casesConfig.featureOrganizations); const href = `studio/organizations/view.kml?id=${encodeURIComponent(data.organization_id_s || data.organization_id)}`; return canLinkOrg ? `<a href=\"${href}\">${label}</a>` : label; })() : (data.contacts && data.contacts.length ? `<a href="studio/contacts/view.kml?id=${encodeURIComponent(data.contacts[0].id_s || data.contacts[0].id)}">${((data.contacts[0].first_name||'')+' '+(data.contacts[0].last_name||'')).trim() || data.contacts[0].phone || data.contacts[0].email}</a>` : 'Personal')}
              </div>
              ${data.contacts && data.contacts.length ? `<div class="qv-subtle">Phone: ${data.contacts[0].phone || 'N/A'} • Email: ${data.contacts[0].email || 'N/A'}</div>` : ''}
            </div>
            <div class="qv-section">
              <span class="qv-label">Assigned</span>
              <div class="qv-value">
                ${(data.assigned_agents && data.assigned_agents.length)
                  ? data.assigned_agents.map(a => {
                      const name = a.display_name || ('Agent #' + a.id);
                      const link = a.user_id ? `<a href="studio/settings/setting-users.kml?id=${encodeURIComponent(a.user_id)}" class="text-primary">${name}</a>` : name;
                      return `
                        <div class="agent-card">
                          <div class="name">${link}</div>
                          <div class="meta">Agent ID: ${a.id}</div>
                        </div>
                      `;
                    }).join('')
                  : 'Unassigned'}
              </div>
            </div>
            <div class="qv-section">
              <span class="qv-label">Created</span>
              <div class="qv-value">${data.created_at || '-'}</div>
              <span class="qv-label mt-2">Updated</span>
              <div class="qv-value">${data.updated_at || '-'}</div>
            </div>
            ${customFieldsHtml ? `
            <div class="qv-section">
              ${customFieldsHtml}
            </div>
            ` : ''}
            <div class="qv-divider"></div>
            <div class="qv-section mt-4">
              <span class="qv-label">${escapeHtml(moduleLabel('contact', 'plural'))}</span>
              <div class="text-muted small mb-2">
                ${(data.contacts || []).map(c => `<div class="d-flex justify-content-between align-items-center border rounded p-2 mb-2">
                  <div>
                    <div class="fw-semibold mb-2"><a href="studio/contacts/view.kml?id=${encodeURIComponent(c.id)}">${((c.first_name||'')+' '+(c.last_name||'')).trim() || c.phone || c.email}</a></div>
                    <div class="qv-subtle">Phone: ${c.phone || 'N/A'} <br> Email: ${c.email || 'N/A'}</div>
                  </div>
                </div>`).join('') || '<div class="qv-subtle">None</div>'}
              </div>
            </div>
            <div class="qv-section">
              <span class="qv-label">Files</span>
              <div class="text-muted small mb-2">
                ${(data.files||[]).map(f => `<div class="d-flex justify-content-between align-items-center border rounded p-2 mb-2">
                  <div><i class="ri-attachment-line me-1"></i>${f.file_url ? `<a href="${f.file_url}" target="_blank">${f.file_name||('File #'+f.id)}</a>` : (f.file_name||('File #'+f.id))}</div>
                </div>`).join('') || '<div class="qv-subtle">None</div>'}
              </div>
            </div>
          `;
          if (timeTrackingEnabled && window.TimeTracking && quickTimeTrackingTemplate) {
            const card = quickTimeTrackingTemplate.firstElementChild
              ? quickTimeTrackingTemplate.firstElementChild.cloneNode(true)
              : null;
            if (card) {
              card.dataset.entityId = data.id || '';
              card.dataset.entityType = 'case';
              card.dataset.canUse = timeTrackingCanUse ? '1' : '0';
              card.dataset.canManage = timeTrackingCanManage ? '1' : '0';
              quickBody.appendChild(card);
              quickTimeTracking = window.TimeTracking.init(card, {
                entityType: 'case',
                entityId: data.id,
                canUse: timeTrackingCanUse,
                canManage: timeTrackingCanManage
              });
            }
          }
        }
      })
      .catch(() => {
        if (quickBody) quickBody.innerHTML = '<div class="text-danger">Unable to load case.</div>';
      });
  });

  // Initial load is handled by DataTables; no extra reload needed.
  loadCategories();
  // Prevent duplicate DataTable requests during initial widget setup.
  setTimeout(() => { suppressAutoReload = false; }, 0);
  showWorkflowRedirectToast();
})();
