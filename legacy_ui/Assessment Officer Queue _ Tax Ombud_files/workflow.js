(function () {
  const cfg = window.caseWorkflowPage || null;
  if (!cfg) return;
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const rootUrl = (typeof url_root !== 'undefined' ? url_root : '../');
  const apiWorkflowSettings = rootUrl + 'api/modules/cases/workflow_settings';
  const apiAccounts = apiMap.accountsIndex || (rootUrl + 'api/modules/accounts/index');
  const apiCases = apiMap.casesIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/cases/index');
  const alertBox = document.getElementById('workflowQueueAlert') || document.getElementById('workflowSettingsAlert');
  const showError = (message) => {
    if (!alertBox) return;
    alertBox.textContent = message || 'Something went wrong';
    alertBox.classList.remove('d-none');
  };
  const clearError = () => {
    if (!alertBox) return;
    alertBox.classList.add('d-none');
    alertBox.textContent = '';
  };

  const showSuccess = () => {
    window.showSavedModal('Settings Saved', 'Your workflow settings have been saved successfully.');
  };
  const showRoutingSuccess = () => {
    window.showSavedModal('Settings Saved', 'Your routing settings have been saved successfully.');
  };
  const showWorkflowRedirectToast = () => {
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
  };

  if (cfg.mode === 'settings') {
    const featureAccounts = cfg.featureAccounts !== false;
    const approvalsEnabled = cfg.approvalsEnabled !== false;
    const mapping = {
      case_workflow: document.getElementById('flag_case_workflow'),
      'case_workflow.intake': document.getElementById('flag_case_workflow_intake'),
      'case_workflow.b1': document.getElementById('flag_case_workflow_b1'),
      'case_workflow.b2': document.getElementById('flag_case_workflow_b2'),
      'case_workflow.b3': document.getElementById('flag_case_workflow_b3')
    };
    const childWorkflowFlagKeys = ['case_workflow.intake', 'case_workflow.b1', 'case_workflow.b2', 'case_workflow.b3'];
    const accountSelect = document.getElementById('workflow_primary_account_id');
    const accountHint = document.getElementById('workflowPrimaryAccountHint');
    const disabledUsersSelect = document.getElementById('workflow_disabled_users');
    const departmentSelect = document.getElementById('workflow_default_department_id');
    const intakeModeSelect = document.getElementById('workflow_intake_assignment_mode');
    const barrierModeSelect = document.getElementById('workflow_barrier_assignment_mode');
    const routingSection = document.getElementById('workflowRoutingSection');
    const routingBasisSelect = document.getElementById('workflow_case_assignment_basis');
    const routingDistributionSelect = document.getElementById('workflow_case_assignment_distribution_mode');
    const routingWrap = document.getElementById('workflowComplaintTypeRoutingWrap');
    const routingBody = document.getElementById('workflowComplaintTypeRoutingBody');
    const routingSaveBtn = document.getElementById('workflowRoutingSaveBtn');
    const complaintDomainBody = document.getElementById('workflowComplaintDomainBody');
    const complaintDomainAlert = document.getElementById('workflowComplaintDomainAlert');
const complaintDomainSaveBtn = document.getElementById('workflowComplaintDomainSaveBtn');
    const complaintDomainResetBtn = document.getElementById('workflowComplaintDomainResetBtn');
    const routingState = {
      canManage: false,
      complaintTypes: [],
      departments: [],
      routes: {}
    };
    const complaintDomainState = {
      complaintTypes: [],
      serviceDomains: [],
      routes: {},
      originalRoutes: {}
    };
    const csrfToken = String(cfg.csrf || document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') || window.CSRF || '').trim();
    const selectedState = {
      accountId: '',
      accountLabel: ''
    };
    const escapeHtml = (value) => String(value == null ? '' : value)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
    const ensureOption = (select, value, label) => {
      if (!select || value === '' || value === null || value === undefined) return;
      const strValue = String(value);
      let option = Array.from(select.options || []).find((opt) => opt.value === strValue);
      if (!option) {
        option = document.createElement('option');
        option.value = strValue;
        option.textContent = label || strValue;
        select.appendChild(option);
      } else if (label) {
        option.textContent = label;
      }
    };
    const resetSelect = (select, placeholder) => {
      if (!select) return;
      select.innerHTML = `<option value="">${placeholder}</option>`;
    };
    const showComplaintDomainError = (message) => {
      if (!complaintDomainAlert) return;
      complaintDomainAlert.textContent = message || '';
      complaintDomainAlert.classList.toggle('d-none', !message);
    };
    const showComplaintDomainSuccess = () => {
      window.showSavedModal('Settings Saved', 'Your complaint domain mappings have been saved successfully.');
    };
    const fetchJson = async (url, options) => {
      const response = await fetch(url, options);
      const data = await response.json().catch(() => ({}));
      if (!response.ok) {
        throw new Error(data.error || 'Request failed');
      }
      return data;
    };
    const updateRoutingVisibility = () => {
      if (!routingWrap || !routingBasisSelect) return;
      const show = String(routingBasisSelect.value || '') === 'complaint_type';
      routingWrap.classList.toggle('d-none', !show);
    };
    const syncWorkflowStageToggles = () => {
      const workflowEnabled = !!(mapping.case_workflow && mapping.case_workflow.checked);
      childWorkflowFlagKeys.forEach((key) => {
        const input = mapping[key];
        if (!input) return;
        input.disabled = !workflowEnabled;
        input.closest('.form-check')?.classList.toggle('opacity-50', !workflowEnabled);
        if (!workflowEnabled) {
          input.checked = false;
        }
      });
    };
    const updateComplaintDomainVisibility = () => {
      const section = document.getElementById('workflowComplaintDomainSection');
      if (!section) return;
      const hasTypes = Array.isArray(complaintDomainState.complaintTypes) && complaintDomainState.complaintTypes.length > 0;
      section.classList.toggle('d-none', !hasTypes);
    };
    // Renders one row per complaint type with a "Configure" button that opens a modal.
    // The modal holds the actual multi-select — keeps the table tidy when there are many types.
    const renderComplaintDomainTable = () => {
      if (!complaintDomainBody) return;
      const complaintTypes = Array.isArray(complaintDomainState.complaintTypes) ? complaintDomainState.complaintTypes : [];
      if (!complaintTypes.length) {
        complaintDomainBody.innerHTML = '<tr><td colspan="3" class="text-muted py-3">No complaint types configured.</td></tr>';
        updateComplaintDomainVisibility();
        return;
      }
      const serviceDomains = Array.isArray(complaintDomainState.serviceDomains) ? complaintDomainState.serviceDomains : [];
      const domainNameById = {};
      serviceDomains.forEach((d) => { domainNameById[String(d.id || '')] = String(d.name || ('Domain #' + d.id)); });

      complaintDomainBody.innerHTML = complaintTypes.map((item) => {
        const code = String(item.code || '');
        const label = String(item.label || code);
        const mappedIds = Array.isArray(complaintDomainState.routes[code]) ? complaintDomainState.routes[code].map(String) : [];

        let summaryHtml;
        if (mappedIds.length === 0) {
          summaryHtml = '<span class="badge bg-light text-muted border">All domains (default)</span>';
        } else {
          const MAX_CHIPS = 3;
          const chips = mappedIds.slice(0, MAX_CHIPS).map((id) => {
            const name = domainNameById[id] || ('Domain #' + id);
            return `<span class="badge bg-primary-subtle text-primary border border-primary-subtle me-1 mb-1">${escapeHtml(name)}</span>`;
          }).join('');
          const overflow = mappedIds.length > MAX_CHIPS
            ? `<span class="badge bg-secondary-subtle text-secondary border border-secondary-subtle">+${mappedIds.length - MAX_CHIPS} more</span>`
            : '';
          summaryHtml = chips + overflow;
        }

        return `<tr data-complaint-type-row="${escapeHtml(code)}">
          <td><div class="fw-semibold">${escapeHtml(label)}</div><div class="small text-muted">${escapeHtml(code)}</div></td>
          <td><div class="workflow-complaint-domain-summary">${summaryHtml}</div></td>
          <td class="text-end">
            <button type="button" class="btn btn-sm btn-outline-primary workflow-complaint-domain-configure"
                    data-code="${escapeHtml(code)}" data-label="${escapeHtml(label)}">
              <i class="ri-settings-3-line me-1"></i>Configure
            </button>
          </td>
        </tr>`;
      }).join('');

      complaintDomainBody.querySelectorAll('.workflow-complaint-domain-configure').forEach((btn) => {
        btn.addEventListener('click', () => {
          openComplaintDomainModal(String(btn.dataset.code || ''), String(btn.dataset.label || ''));
        });
      });

      updateComplaintDomainVisibility();
    };

    // ── Modal controller ──────────────────────────────────────────────────
    let _wcdModalEl = null;
    let _wcdModalBs = null;
    let _wcdActiveCode = '';
    const openComplaintDomainModal = (code, label) => {
      _wcdModalEl = _wcdModalEl || document.getElementById('workflowComplaintDomainModal');
      if (!_wcdModalEl) return;
      _wcdActiveCode = code;
      const subtitleEl = document.getElementById('workflowComplaintDomainModalSubtitle');
      const selectEl   = document.getElementById('workflowComplaintDomainModalSelect');
      if (subtitleEl) subtitleEl.textContent = label + '  (' + code + ')';
      if (selectEl) {
        const serviceDomains = Array.isArray(complaintDomainState.serviceDomains) ? complaintDomainState.serviceDomains : [];
        const mappedIds = Array.isArray(complaintDomainState.routes[code]) ? complaintDomainState.routes[code].map(String) : [];
        selectEl.innerHTML = serviceDomains.map((d) => {
          const id = String(d.id || '');
          const sel = mappedIds.includes(id) ? ' selected' : '';
          return `<option value="${escapeHtml(id)}"${sel}>${escapeHtml(String(d.name || ('Domain #' + id)))}</option>`;
        }).join('');
      }
      if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        _wcdModalBs = _wcdModalBs || bootstrap.Modal.getOrCreateInstance(_wcdModalEl);
        _wcdModalBs.show();
      }
    };
    const applyComplaintDomainModal = () => {
      const code = _wcdActiveCode;
      const selectEl = document.getElementById('workflowComplaintDomainModalSelect');
      if (!code || !selectEl) return;
      const values = Array.from(selectEl.options || [])
        .filter((opt) => opt.selected)
        .map((opt) => String(opt.value || '').trim())
        .filter(Boolean);
      if (values.length) {
        complaintDomainState.routes[code] = values;
      } else {
        delete complaintDomainState.routes[code];
      }
      if (_wcdModalBs) _wcdModalBs.hide();
      renderComplaintDomainTable();
    };

    const collectComplaintDomainRoutes = () => {
      // Multi-selects no longer live in the DOM rows — read directly from state.
      const routes = {};
      Object.keys(complaintDomainState.routes || {}).forEach((code) => {
        const arr = complaintDomainState.routes[code];
        if (Array.isArray(arr) && arr.length) routes[code] = arr.map(String);
      });
      return routes;
    };
    const resetComplaintDomainMappings = () => {
      complaintDomainState.routes = JSON.parse(JSON.stringify(complaintDomainState.originalRoutes || {}));
      renderComplaintDomainTable();
    };
    const renderRoutingTable = () => {
      if (!routingBody) return;
      const complaintTypes = Array.isArray(routingState.complaintTypes) ? routingState.complaintTypes : [];
      const departments = Array.isArray(routingState.departments) ? routingState.departments : [];
      if (!complaintTypes.length) {
        routingBody.innerHTML = '<tr><td colspan="3" class="text-muted py-3">No complaint types configured.</td></tr>';
        return;
      }
      const departmentsById = {};
      departments.forEach((dept) => {
        const id = String(dept.id || '');
        if (id) departmentsById[id] = dept;
      });
      const deptOptions = ['<option value="">Unassigned</option>']
        .concat(departments.map((dept) => `<option value="${escapeHtml(String(dept.id || ''))}">${escapeHtml(String(dept.name || ('Department #' + dept.id)))}</option>`))
        .join('');
      routingBody.innerHTML = complaintTypes.map((item) => {
        const code = String(item.code || '');
        const label = String(item.label || code);
        const selected = Object.prototype.hasOwnProperty.call(routingState.routes, code)
          ? String(routingState.routes[code] || '')
          : '';
        const selectedDept = selected && departmentsById[selected] ? departmentsById[selected] : null;
        const headLabel = selectedDept
          ? (String(selectedDept.head_name || '').trim() || (selectedDept.department_head_agent_id ? ('Agent #' + selectedDept.department_head_agent_id) : 'No head assigned'))
          : 'No head assigned';
        return `<tr>
            <td><div class="fw-semibold">${escapeHtml(label)}</div><div class="small text-muted">${escapeHtml(code)}</div></td>
            <td><select class="form-select form-select-sm workflow-complaint-route" data-code="${escapeHtml(code)}">${deptOptions}</select></td>
            <td class="small text-muted workflow-complaint-route-head">${escapeHtml(headLabel)}</td>
          </tr>`;
      }).join('');
      routingBody.querySelectorAll('.workflow-complaint-route').forEach((el) => {
        const code = String(el.getAttribute('data-code') || '');
        if (code && Object.prototype.hasOwnProperty.call(routingState.routes, code)) {
          el.value = String(routingState.routes[code] || '');
        }
        el.addEventListener('change', () => {
          if (el.value) {
            routingState.routes[code] = String(el.value);
          } else {
            delete routingState.routes[code];
          }
          const row = el.closest('tr');
          const headCell = row ? row.querySelector('.workflow-complaint-route-head') : null;
          const selectedDept = el.value && departmentsById[el.value] ? departmentsById[el.value] : null;
          if (headCell) {
            headCell.textContent = selectedDept
              ? (String(selectedDept.head_name || '').trim() || (selectedDept.department_head_agent_id ? ('Agent #' + selectedDept.department_head_agent_id) : 'No head assigned'))
              : 'No head assigned';
          }
        });
      });
    };
    const userLabel = (row) => {
      const name = String(row?.display_name || '').trim();
      const email = String(row?.email || '').trim();
      if (name && email) return name + ' <' + email + '>';
      return name || email || ('User #' + String(row?.id || ''));
    };

    // ── Select2 AJAX init for both pickers ─────────────────────────────
    const initSelect2Pickers = () => {
      if (typeof window.jQuery === 'undefined' || typeof window.jQuery.fn.select2 !== 'function') return;
      const $ = window.jQuery;

      if (featureAccounts && accountSelect && !$(accountSelect).hasClass('select2-hidden-accessible')) {
        $(accountSelect).select2({
          placeholder: 'No specific account lane',
          allowClear: true,
          width: '100%',
          ajax: {
            delay: 250,
            transport: (params, success, failure) => {
              const q = (params.data && params.data.term) ? params.data.term : '';
              fetch(apiAccounts + '?lookup=select&limit=25&q=' + encodeURIComponent(q), { credentials: 'same-origin' })
                .then((r) => r.json()).then(success).catch(failure);
            },
            processResults: (data) => ({
              results: (Array.isArray(data.data) ? data.data : []).map((row) => ({
                id: String(row.id || ''),
                text: row.name || ('Account #' + row.id),
              })).filter((r) => r.id !== ''),
            }),
          },
        });
        $(accountSelect).on('change', () => {
          selectedState.accountId = accountSelect.value || '';
          const sel = accountSelect.options[accountSelect.selectedIndex] || null;
          selectedState.accountLabel = sel && sel.value ? sel.textContent : '';
          if (accountHint) {
            accountHint.textContent = selectedState.accountLabel
              ? ('Selected workflow lane: ' + selectedState.accountLabel)
              : 'Optional. Scope the workflow to a single CRM account lane. Leave blank to run workflow across all cases.';
          }
        });
      }

      if (disabledUsersSelect && !$(disabledUsersSelect).hasClass('select2-hidden-accessible')) {
        $(disabledUsersSelect).select2({
          placeholder: 'Search for officers to exclude…',
          allowClear: true,
          width: '100%',
          multiple: true,
          ajax: {
            delay: 250,
            transport: (params, success, failure) => {
              const q = (params.data && params.data.term) ? params.data.term : '';
              fetch(apiWorkflowSettings + '?user_lookup=1&limit=25&q=' + encodeURIComponent(q), { credentials: 'same-origin' })
                .then((r) => r.json()).then(success).catch(failure);
            },
            processResults: (data) => ({
              results: (Array.isArray(data.data) ? data.data : []).map((row) => ({
                id: String(row.id || ''),
                text: userLabel(row),
              })).filter((r) => r.id !== ''),
            }),
          },
        });
      }
    };

    const setSelect2Value = (select, value, label) => {
      if (!select) return;
      ensureOption(select, value, label);
      select.value = value;
      if (typeof window.jQuery === 'function' && window.jQuery.fn.select2) {
        window.jQuery(select).trigger('change.select2');
      }
    };

    const seedDisabledUsers = (rows) => {
      if (!disabledUsersSelect) return;
      // Clear & repopulate with the saved (currently-disabled) users
      disabledUsersSelect.innerHTML = '';
      const values = [];
      rows.forEach((row) => {
        const value = String(row.id || '');
        if (!value) return;
        const opt = document.createElement('option');
        opt.value = value;
        opt.textContent = userLabel(row);
        opt.selected = true;
        disabledUsersSelect.appendChild(opt);
        values.push(value);
      });
      if (typeof window.jQuery === 'function' && window.jQuery.fn.select2) {
        window.jQuery(disabledUsersSelect).val(values).trigger('change.select2');
      }
    };
    const loadFlags = async () => {
      clearError();
      const data = await fetchJson(apiWorkflowSettings);
      const payload = data.data || {};
      const flags = payload.flags || {};
      Object.keys(mapping).forEach((key) => {
        if (mapping[key]) {
          mapping[key].checked = String(flags[key] || '0') === '1';
        }
      });
      syncWorkflowStageToggles();
      const settings = payload.settings || {};
      selectedState.accountId = featureAccounts && settings.case_workflow_primary_account_id ? String(settings.case_workflow_primary_account_id) : '';
      if (departmentSelect) {
        resetSelect(departmentSelect, 'Select default department');
        (payload.departments || []).forEach((row) => {
          const opt = document.createElement('option');
          opt.value = String(row.id || '');
          opt.textContent = row.name || ('Department #' + row.id);
          departmentSelect.appendChild(opt);
        });
        departmentSelect.value = settings.case_workflow_default_department_id ? String(settings.case_workflow_default_department_id) : '';
      }
      if (intakeModeSelect) {
        intakeModeSelect.value = settings.case_workflow_intake_assignment_mode || 'shared';
      }
      if (barrierModeSelect) {
        barrierModeSelect.value = settings.case_workflow_barrier_assignment_mode || 'shared';
      }
      const routing = payload.routing || {};
      routingState.canManage = !!routing.can_manage;
      routingState.complaintTypes = Array.isArray(routing.complaint_types) ? routing.complaint_types : [];
      routingState.departments = Array.isArray(payload.departments) ? payload.departments : [];
      routingState.routes = (routing.complaint_type_routes && typeof routing.complaint_type_routes === 'object')
        ? Object.assign({}, routing.complaint_type_routes)
        : {};
      complaintDomainState.complaintTypes = Array.isArray(routing.complaint_types) ? routing.complaint_types : [];
      complaintDomainState.serviceDomains = Array.isArray(routing.service_domains) ? routing.service_domains : [];
      complaintDomainState.routes = (routing.service_domain_routes && typeof routing.service_domain_routes === 'object')
        ? JSON.parse(JSON.stringify(routing.service_domain_routes))
        : {};
      complaintDomainState.originalRoutes = JSON.parse(JSON.stringify(complaintDomainState.routes));
      if (routingSection) {
        routingSection.classList.toggle('d-none', !routingState.canManage);
      }
      if (routingState.canManage) {
        if (routingBasisSelect) routingBasisSelect.value = routing.case_assignment_basis || 'service_domain';
        if (routingDistributionSelect) routingDistributionSelect.value = routing.case_assignment_distribution_mode || 'least_load';
        renderRoutingTable();
        updateRoutingVisibility();
      }
      updateComplaintDomainVisibility();
      renderComplaintDomainTable();
      if (approvalsEnabled) {
        seedStageAgentState(routing);
      }
      const selectedAccount = featureAccounts ? (payload.selected_account || null) : null;
      if (selectedAccount && accountSelect) {
        selectedState.accountId = String(selectedAccount.id || '');
        selectedState.accountLabel = selectedAccount.name || '';
        setSelect2Value(accountSelect, selectedState.accountId, selectedState.accountLabel);
        if (accountHint) {
          accountHint.textContent = selectedState.accountLabel
            ? ('Selected workflow lane: ' + selectedState.accountLabel)
            : 'Optional. Scope the workflow to a single CRM account lane.';
        }
      }
      // Populate the multi-select with users who are currently OPTED OUT
      seedDisabledUsers(Array.isArray(payload.email_disabled_users) ? payload.email_disabled_users : []);
    };
    const saveBtn = document.getElementById('workflowSettingsSaveBtn');
    // ── Stage-agent mapping by complaint type ─────────────────────────────
    const stageAgentState = {
      complaintTypes: [],
      agents: {},
      mapping: {},
      originalMapping: {},
    };
    const stageAgentAlert = document.getElementById('workflowStageAgentAlert');
    const showStageAgentError = (msg) => {
      if (!stageAgentAlert) return;
      stageAgentAlert.textContent = msg || '';
      stageAgentAlert.classList.toggle('d-none', !msg);
    };
    const stageList = ['intake_verify', 'b1', 'b2', 'b3'];

    const renderStageAgentTable = (stage) => {
      const body = document.getElementById('stageAgentBody-' + stage);
      if (!body) return;
      const types = Array.isArray(stageAgentState.complaintTypes) ? stageAgentState.complaintTypes : [];
      if (!types.length) {
        body.innerHTML = '<tr><td colspan="3" class="text-muted py-3">No complaint types configured.</td></tr>';
        return;
      }
      const stageMap = (stageAgentState.mapping[stage] && typeof stageAgentState.mapping[stage] === 'object')
        ? stageAgentState.mapping[stage] : {};
      body.innerHTML = types.map((item) => {
        const code = String(item.code || '');
        const label = String(item.label || code);
        const agentIds = Array.isArray(stageMap[code]) ? stageMap[code] : [];
        let summaryHtml;
        if (!agentIds.length) {
          summaryHtml = '<span class="badge bg-light text-muted border">Full pool (default)</span>';
        } else {
          const MAX_CHIPS = 3;
          const chips = agentIds.slice(0, MAX_CHIPS).map((id) => {
            const agent = stageAgentState.agents[String(id)] || null;
            const name = agent ? String(agent.display_name || ('Agent #' + id)) : ('Agent #' + id);
            return `<span class="badge bg-primary-subtle text-primary border border-primary-subtle me-1 mb-1">${escapeHtml(name)}</span>`;
          }).join('');
          const overflow = agentIds.length > MAX_CHIPS
            ? `<span class="badge bg-secondary-subtle text-secondary border border-secondary-subtle">+${agentIds.length - MAX_CHIPS} more</span>`
            : '';
          summaryHtml = chips + overflow;
        }
        return `<tr data-stage-agent-row="${escapeHtml(stage + '|' + code)}">
          <td><div class="fw-semibold">${escapeHtml(label)}</div><div class="small text-muted">${escapeHtml(code)}</div></td>
          <td><div class="workflow-stage-agent-summary">${summaryHtml}</div></td>
          <td class="text-end">
            <button type="button" class="btn btn-sm btn-outline-primary workflow-stage-agent-configure"
                    data-stage="${escapeHtml(stage)}" data-code="${escapeHtml(code)}" data-label="${escapeHtml(label)}">
              <i class="ri-settings-3-line me-1"></i>Configure
            </button>
          </td>
        </tr>`;
      }).join('');
      body.querySelectorAll('.workflow-stage-agent-configure').forEach((btn) => {
        btn.addEventListener('click', () => {
          openStageAgentModal(String(btn.dataset.stage || ''), String(btn.dataset.code || ''), String(btn.dataset.label || ''));
        });
      });
    };
    const renderAllStageAgentTables = () => { stageList.forEach((s) => renderStageAgentTable(s)); };

    let _saModalEl = null;
    let _saModalBs = null;
    let _saActiveStage = '';
    let _saActiveCode = '';
    const openStageAgentModal = (stage, code, label) => {
      _saModalEl = _saModalEl || document.getElementById('workflowStageAgentModal');
      if (!_saModalEl) return;
      _saActiveStage = stage;
      _saActiveCode = code;
      const subtitleEl = document.getElementById('workflowStageAgentModalSubtitle');
      const selectEl   = document.getElementById('workflowStageAgentModalSelect');
      if (subtitleEl) subtitleEl.textContent = label + '  (' + code + ')';
      const stageMap = (stageAgentState.mapping[stage] && typeof stageAgentState.mapping[stage] === 'object')
        ? stageAgentState.mapping[stage] : {};
      const assignedIds = Array.isArray(stageMap[code]) ? stageMap[code].map(String) : [];
      if (selectEl && typeof window.jQuery === 'function' && window.jQuery.fn.select2) {
        const $ = window.jQuery;
        if ($(selectEl).hasClass('select2-hidden-accessible')) $(selectEl).select2('destroy');
        $(selectEl).empty();
        assignedIds.forEach((id) => {
          const agent = stageAgentState.agents[id] || null;
          const text = agent ? String(agent.display_name || ('Agent #' + id)) : ('Agent #' + id);
          $(selectEl).append(new Option(text, id, true, true));
        });
        $(selectEl).select2({
          placeholder: 'Search agents…',
          allowClear: true,
          width: '100%',
          multiple: true,
          dropdownParent: $(_saModalEl),
          ajax: {
            delay: 250,
            transport: (params, success, failure) => {
              const q = (params.data && params.data.term) ? params.data.term : '';
              fetch(
                apiWorkflowSettings +
                '?stage_agent_lookup=1' +
                '&stage=' + encodeURIComponent(_saActiveStage) +
                '&complaint_type=' + encodeURIComponent(_saActiveCode) +
                '&limit=25&q=' + encodeURIComponent(q),
                { credentials: 'same-origin' }
              ).then((r) => r.json()).then(success).catch(failure);
            },
            processResults: (data) => ({
              results: (Array.isArray(data.data) ? data.data : []).map((row) => ({
                id: String(row.id || ''), text: userLabel(row),
              })).filter((r) => r.id !== ''),
            }),
          },
        });
        $(selectEl).val(assignedIds).trigger('change.select2');
      }
      if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        _saModalBs = _saModalBs || bootstrap.Modal.getOrCreateInstance(_saModalEl);
        _saModalBs.show();
      }
    };
    const applyStageAgentModal = () => {
      const stage = _saActiveStage;
      const code  = _saActiveCode;
      const selectEl = document.getElementById('workflowStageAgentModalSelect');
      if (!stage || !code || !selectEl) return;
      let values;
      if (typeof window.jQuery === 'function' && window.jQuery.fn.select2) {
        const $ = window.jQuery;
        const selected = $(selectEl).select2('data') || [];
        selected.forEach((item) => {
          if (item.id) stageAgentState.agents[String(item.id)] = { id: String(item.id), display_name: String(item.text || '') };
        });
        values = ($(selectEl).val() || []).map(String).filter(Boolean);
      } else {
        values = Array.from(selectEl.options || []).filter((o) => o.selected).map((o) => String(o.value).trim()).filter(Boolean);
      }
      if (!stageAgentState.mapping[stage]) stageAgentState.mapping[stage] = {};
      if (values.length) {
        stageAgentState.mapping[stage][code] = values.map(Number).filter(Boolean);
      } else {
        delete stageAgentState.mapping[stage][code];
      }
      if (_saModalBs) _saModalBs.hide();
      renderStageAgentTable(stage);
    };
    const collectStageAgentMapping = () => {
      const out = {};
      stageList.forEach((s) => {
        out[s] = {};
        const sm = (stageAgentState.mapping[s] && typeof stageAgentState.mapping[s] === 'object') ? stageAgentState.mapping[s] : {};
        Object.keys(sm).forEach((code) => {
          const ids = Array.isArray(sm[code]) ? sm[code].filter(Boolean) : [];
          if (ids.length) out[s][code] = ids;
        });
      });
      return out;
    };
    const resetStageAgentMapping = () => {
      stageAgentState.mapping = JSON.parse(JSON.stringify(stageAgentState.originalMapping || {}));
      renderAllStageAgentTables();
    };
    const seedStageAgentState = (routing) => {
      const raw = routing.stage_complaint_type_agents;
      stageAgentState.complaintTypes = Array.isArray(routing.complaint_types) ? routing.complaint_types : [];
      if (raw && typeof raw === 'object') {
        stageAgentState.mapping = JSON.parse(JSON.stringify(raw));
        stageAgentState.originalMapping = JSON.parse(JSON.stringify(raw));
        const allIds = [];
        stageList.forEach((s) => {
          const sm = raw[s] || {};
          Object.values(sm).forEach((ids) => { if (Array.isArray(ids)) ids.forEach((id) => allIds.push(String(id))); });
        });
        [...new Set(allIds)].filter(Boolean).forEach((agentId) => {
          if (!stageAgentState.agents[agentId]) {
            fetch(apiWorkflowSettings + '?agent_id=' + encodeURIComponent(agentId), { credentials: 'same-origin' })
              .then((r) => r.json())
              .then((d) => {
                const agent = (d.data || {}).agent;
                if (agent && agent.id) {
                  stageAgentState.agents[String(agent.id)] = { id: String(agent.id), display_name: String(agent.display_name || ('Agent #' + agent.id)) };
                  renderAllStageAgentTables();
                }
              })
              .catch(() => {});
          }
        });
      }
      renderAllStageAgentTables();
    };
    // ── End stage-agent setup ────────────────────────────────────────────

    const buildSavePayload = () => {
      const payload = {
        csrf: csrfToken,
        settings: {
          case_workflow_default_department_id: departmentSelect && departmentSelect.value ? String(departmentSelect.value) : '',
        },
        routing: {
          case_assignment_basis: routingBasisSelect && routingBasisSelect.value ? String(routingBasisSelect.value) : 'service_domain',
          case_assignment_distribution_mode: routingDistributionSelect && routingDistributionSelect.value ? String(routingDistributionSelect.value) : 'least_load',
          complaint_type_routes: routingState.canManage ? routingState.routes : {},
          service_domain_routes: collectComplaintDomainRoutes(),
        },
      };
      if (approvalsEnabled) {
        const flags = {};
        Object.entries(mapping).forEach(([key, input]) => { flags[key] = input && input.checked ? '1' : '0'; });
        if (flags.case_workflow !== '1') childWorkflowFlagKeys.forEach((key) => { flags[key] = '0'; });
        payload.flags = flags;
        payload.settings.case_workflow_primary_account_id = featureAccounts && accountSelect && accountSelect.value ? String(accountSelect.value) : '';
        payload.settings.case_workflow_intake_assignment_mode = intakeModeSelect && intakeModeSelect.value ? String(intakeModeSelect.value) : 'shared';
        payload.settings.case_workflow_barrier_assignment_mode = barrierModeSelect && barrierModeSelect.value ? String(barrierModeSelect.value) : 'shared';
        payload.routing.stage_complaint_type_agents = collectStageAgentMapping();
        if (disabledUsersSelect) {
          payload.disabled_user_ids = Array.from(disabledUsersSelect.selectedOptions || []).map((o) => String(o.value)).filter(Boolean);
        }
      }
      return payload;
    };

    const saveAllSettings = async (source) => {
      clearError();
      try {
        await fetchJson(apiWorkflowSettings, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrfToken },
          body: JSON.stringify(buildSavePayload()),
        });
        if (source === 'routing') showRoutingSuccess();
        else if (source === 'domain') showComplaintDomainSuccess();
        else showSuccess();
        await loadFlags();
      } catch (error) {
        showError(error.message || 'Unable to save workflow settings');
        showComplaintDomainError(error.message || 'Unable to save workflow settings');
      }
    };
    if (saveBtn) {
      saveBtn.addEventListener('click', () => saveAllSettings('settings'));
    }
    if (mapping.case_workflow) {
      mapping.case_workflow.addEventListener('change', syncWorkflowStageToggles);
    }
    if (routingSaveBtn) {
      routingSaveBtn.addEventListener('click', () => saveAllSettings('routing'));
    }
    // Select2 handles all the search/AJAX wiring above; nothing extra to bind here.
    if (routingBasisSelect) {
      routingBasisSelect.addEventListener('change', updateRoutingVisibility);
    }
    if (complaintDomainSaveBtn) {
      complaintDomainSaveBtn.addEventListener('click', () => saveAllSettings('domain'));
    }
    if (complaintDomainResetBtn) {
      complaintDomainResetBtn.addEventListener('click', () => {
        resetComplaintDomainMappings();
        showComplaintDomainSuccess();
      });
    }
    // Apply button inside the configure-domains modal
    const _wcdApplyBtn = document.getElementById('workflowComplaintDomainModalSaveBtn');
    if (_wcdApplyBtn) {
      _wcdApplyBtn.addEventListener('click', applyComplaintDomainModal);
    }
    // Stage-agent modal apply button
    const _saApplyBtn = document.getElementById('workflowStageAgentModalSaveBtn');
    if (_saApplyBtn) {
      _saApplyBtn.addEventListener('click', applyStageAgentModal);
    }
    // Stage-agent save/reset buttons (dedicated save for stage card)
    const stageAgentSaveBtn = document.getElementById('workflowStageAgentSaveBtn');
    const stageAgentResetBtn = document.getElementById('workflowStageAgentResetBtn');
    if (stageAgentSaveBtn) {
      stageAgentSaveBtn.addEventListener('click', async () => {
        showStageAgentError('');
        try {
          await fetchJson(apiWorkflowSettings, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrfToken },
            body: JSON.stringify(buildSavePayload()),
          });
          window.showSavedModal('Saved', 'Stage agent mapping saved successfully.');
          stageAgentState.originalMapping = JSON.parse(JSON.stringify(stageAgentState.mapping));
        } catch (err) {
          showStageAgentError(err.message || 'Unable to save stage agent mapping.');
        }
      });
    }
    if (stageAgentResetBtn) {
      stageAgentResetBtn.addEventListener('click', resetStageAgentMapping);
    }

    // Init Select2 pickers before fetching data so seedDisabledUsers can trigger.change.select2
    initSelect2Pickers();
    loadFlags().catch((error) => showError(error.message || 'Unable to load workflow flags'));
    return;
  }

  if (cfg.mode === 'queue') {
    showWorkflowRedirectToast();
    const body = document.getElementById('workflowQueueBody');
    const render = (rows) => {
      if (!body) return;
      if ($.fn.DataTable.isDataTable('#workflowQueueTable')) {
        $('#workflowQueueTable').DataTable().clear().destroy();
      }
      body.innerHTML = '';
      rows.forEach((row) => {
        const complainant = Array.isArray(row.contacts) && row.contacts.length
          ? (((row.contacts[0].first_name || '') + ' ' + (row.contacts[0].last_name || '')).trim() || row.contacts[0].email || '-')
          : (row.owner_contact_name || row.organization_name || row.account_name || '-');
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${row.case_number || '-'}</td>
          <td>${row.subject || '-'}</td>
          <td>${complainant}</td>
          <td>${row.complaint_type || '-'}</td>
          <td>${row.category_name || '-'}</td>
          <td>${row.created_at || '-'}</td>
          <td class="text-end"><a class="btn btn-sm btn-primary" href="studio/cases/view.kml?id=${encodeURIComponent(row.id_s || row.id)}">Open</a></td>
        `;
        body.appendChild(tr);
      });
      $('#workflowQueueTable').DataTable({
        destroy: true,
        pageLength: 25,
        order: [[5, 'desc']],
        columnDefs: [{ orderable: false, targets: 6 }],
        language: { emptyTable: 'No cases in this queue.' }
      });
    };
    const loadQueue = async () => {
      clearError();
      const response = await fetch(apiCases + '?workflow_stage=' + encodeURIComponent(cfg.stage) + '&limit=250');
      const data = await response.json();
      if (!response.ok) throw new Error(data.error || 'Unable to load workflow queue');
      render(data.data || []);
    };
    loadQueue().catch((error) => showError(error.message || 'Unable to load workflow queue'));
  }
})();
