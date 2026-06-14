function setupCaseForm(options) {
  const opts = options || {};
  const moduleLabel = (typeof window.moduleLabel === 'function')
    ? window.moduleLabel
    : (key, form, casing) => {
      const defaults = {
        account: { singular: 'Account', plural: 'Accounts' },
        contact: { singular: 'Contact', plural: 'Contacts' },
        organization: { singular: 'Organization', plural: 'Organizations' },
        lead: { singular: 'Lead', plural: 'Leads' },
        case: { singular: 'Case', plural: 'Cases' }
      };
      const k = String(key || '').toLowerCase();
      const f = String(form || 'plural').toLowerCase() === 'singular' ? 'singular' : 'plural';
      let label = (defaults[k] && defaults[k][f]) || k;
      if (String(casing || '').toLowerCase() === 'lower') label = label.toLowerCase();
      return label;
    };
  const form = document.querySelector(opts.formSelector || '#caseForm');
  if (!form) return null;

  const saveBtn = document.querySelector(opts.saveBtn || '#caseSaveBtn');
  const statusSelect = form.querySelector('#case_status');
  const prioritySelect = form.querySelector('#case_priority');
  const subjectInput = form.querySelector('#case_subject');
  const subjectCountEl = form.querySelector('#case_subject_count');
  const descInput = form.querySelector('#case_description');
  const descEditorEl = form.querySelector('#case_description_editor');
  const descCountEl = form.querySelector('#case_description_count');
  const categorySelect = form.querySelector('#domain_id');
  const subDomainSelect = form.querySelector('#case_sub_domain_id');
  const subDomainWrap = form.querySelector('#case_sub_domain_wrap');
  const complaintSelect = form.querySelector('#case_complaint_type');
  const subComplaintSelect = form.querySelector('#case_sub_complaint_type');
  const subComplaintWrap = subComplaintSelect ? (subComplaintSelect.closest('.col-md-4') || subComplaintSelect.closest('.mb-3')) : null;
  const customFieldsWrap = form.querySelector('#case_custom_fields_wrap');
  const customFieldsContainer = form.querySelector('#case_custom_fields');
  let customFieldDefs = null;
  let quillEditor = null;
  const approvalWrap = form.querySelector('#case_requires_approval_wrap');
  const approvalToggle = form.querySelector('#case_requires_approval');
  const approvalsEnabled = approvalWrap && approvalWrap.dataset.approvalsEnabled === '1';
  const approvalsGlobalRequired = approvalWrap && approvalWrap.dataset.approvalsGlobal === '1';
  const approvalsAgentDomain = approvalWrap ? parseInt(approvalWrap.dataset.approvalsDomain || '0', 10) || 0 : 0;
  const approvalsAgentDomains = approvalWrap
    ? (approvalWrap.dataset.approvalsDomains || '')
        .split(',')
        .map(v => parseInt(v.trim(), 10))
        .filter(v => v && !Number.isNaN(v))
    : [];
  const approvalsCanReadAll = approvalWrap && approvalWrap.dataset.approvalsReadall === '1';
  const approvalsIsSuperAdmin = approvalWrap && approvalWrap.dataset.approvalsSuper === '1';
  const parentTypeSelect = form.querySelector('#case_parent_type');
  const parentSearchInput = form.querySelector('#case_parent_search');
  const parentSearchStatus = form.querySelector('#case_parent_search_status');
  const parentSelect = form.querySelector('#case_parent_select');
  const parentSelected = form.querySelector('#case_parent_selected');
  const agentSelect = form.querySelector('#case_agent');
  const agentWrap = form.querySelector('#case_agent_wrap');
  const fileInput = form.querySelector('#case_file_upload');
  const fileList = form.querySelector('#case_file_list');
  const agentHint = form.querySelector('#case_agent_hint');
  const filesEnabled = !(window.crmFeatures && window.crmFeatures.filesEnabled === false);
  const modalHost = form.closest('.modal-content') || form.closest('.modal');

  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const fallbackBase = (typeof url_root !== 'undefined' ? url_root : '../');
  const apiCases = apiMap.casesIndex || (fallbackBase + 'api/modules/cases/index');
  const apiCaseDetail = opts.caseDetailUrl || apiMap.casesDetail || (fallbackBase + 'api/modules/cases/detail');
  const apiStatuses = apiMap.casesStatuses || (fallbackBase + 'api/modules/cases/statuses');
  const apiCategories = apiMap.caseCategories || (fallbackBase + 'api/modules/case-categories/index');
  const apiComplaintTypes = apiMap.complaintTypes || (fallbackBase + 'api/modules/config/complaint_types');
  const apiFiles = apiMap.filesIndex || (fallbackBase + 'api/modules/files/index');
  const apiAgentsByDomain = apiMap.caseAgentsByDomain || (fallbackBase + 'api/modules/cases/agents');
  const featureSource = opts.featureSource || window.casesConfig || window.caseViewConfig || window.accountsViewConfig || {};
  const featureAccounts = Object.prototype.hasOwnProperty.call(opts, 'featureAccounts')
    ? opts.featureAccounts !== false
    : featureSource.featureAccounts !== false;
  const featureLeads = Object.prototype.hasOwnProperty.call(opts, 'featureLeads')
    ? opts.featureLeads !== false
    : featureSource.featureLeads !== false;

  const parentMeta = {
    organization: { lookup: 'organizations', label: moduleLabel('organization', 'singular'), searchPlaceholder: 'Search ' + moduleLabel('organization', 'singular', 'lower'), selectPlaceholder: 'Select ' + moduleLabel('organization', 'singular', 'lower') },
    contact: { lookup: 'contacts', label: moduleLabel('contact', 'singular'), searchPlaceholder: 'Search ' + moduleLabel('contact', 'singular', 'lower'), selectPlaceholder: 'Select ' + moduleLabel('contact', 'singular', 'lower') }
  };
  if (featureAccounts) {
    parentMeta.account = { lookup: 'accounts', label: moduleLabel('account', 'singular'), searchPlaceholder: 'Search ' + moduleLabel('account', 'singular', 'lower'), selectPlaceholder: 'Select ' + moduleLabel('account', 'singular', 'lower') };
  }
  if (featureLeads) {
    parentMeta.lead = { lookup: 'leads', label: moduleLabel('lead', 'singular'), searchPlaceholder: 'Search ' + moduleLabel('lead', 'singular', 'lower'), selectPlaceholder: 'Select ' + moduleLabel('lead', 'singular', 'lower') };
  }
  const defaultParentType = parentMeta.organization ? 'organization' : Object.keys(parentMeta)[0];
  if (parentTypeSelect) {
    Array.from(parentTypeSelect.options || []).forEach((opt) => {
      if (opt.value && !parentMeta[opt.value]) opt.remove();
    });
  }
  const minSearchLen = 3;
  const searchLimit = 10;
  let cachedStatuses = [];
  let parentLookupSeq = 0;
  let uploadingFiles = false;
  let uploadedFiles = [];
  let uploadedFileIds = [];
  const caseId = opts.caseId || null;
  const optDefaultOrganizationId = opts.defaultOrganizationId || opts.defaultorganizationId || null;
  const optDefaultOrganizationLabel = opts.defaultOrganizationLabel || opts.defaultorganizationLabel || '';
  const optDefaultAccountId = opts.defaultAccountId || null;
  const optDefaultAccountLabel = opts.defaultAccountLabel || '';
  const optDefaultParentType = opts.defaultParentType || null;
  const optDefaultParentId = opts.defaultParentId || null;
  const optDefaultParentLabel = opts.defaultParentLabel || '';
  let defaults = {
    parentType: optDefaultParentType || null,
    parentId: optDefaultParentId || null,
    parentLabel: optDefaultParentLabel || '',
    accountId: optDefaultAccountId || null,
    accountLabel: optDefaultAccountLabel || '',
    contactId: opts.defaultContactId || null,
    contactLabel: opts.defaultContactLabel || '',
    organizationId: optDefaultOrganizationId || null,
    organizationLabel: optDefaultOrganizationLabel || '',
    leadId: opts.defaultLeadId || null,
    leadLabel: opts.defaultLeadLabel || '',
    domain_id: opts.defaultCaseCategoryId || null,
    sub_domain_id: opts.defaultSubCaseCategoryId || null,
    complaint_type: null,
    sub_complaint_type: null,
    subject: '',
    description: '',
    priority: 'medium',
    status: 'new',
    assigned_agents: [],
    requires_approval: 0,
    custom_fields: {}
  };
  let categoryMeta = {};
  let categorySourceList = [];
  let categorySubDomainMap = {};
  let serviceDomainSettings = { enable_sub_domains: 1 };
  let serviceDomainRoutes = {};
  let complaintTypeMap = {};
  let complaintTypeSettings = { enable_sub_types: 0 };
  const agentsCache = {};
  let categorySelect2Ready = false;

  const DESCRIPTION_LIMIT = 2000;
  const WARNING_THRESHOLD = 1900;

  function updateCounter(el, count) {
    if (!el) return;
    const safeCount = Math.max(0, Number(count) || 0);
    el.textContent = safeCount.toLocaleString() + ' / 2,000';
    el.classList.toggle('text-danger', safeCount >= WARNING_THRESHOLD);
    el.classList.toggle('text-muted', safeCount < WARNING_THRESHOLD);
  }

  function getEditorPlainText() {
    if (quillEditor && typeof quillEditor.getText === 'function') {
      return quillEditor.getText().replace(/\n$/, '');
    }
    const raw = descInput ? String(descInput.value || '') : '';
    const tmp = document.createElement('div');
    tmp.innerHTML = raw;
    return (tmp.textContent || tmp.innerText || '').replace(/\u00a0/g, ' ');
  }

  function updateDescriptionCounter() {
    updateCounter(descCountEl, getEditorPlainText().length);
  }

  function enforceDescriptionLimit() {
    if (quillEditor && typeof quillEditor.getLength === 'function') {
      const length = Math.max(0, quillEditor.getLength() - 1);
      if (length > DESCRIPTION_LIMIT) {
        quillEditor.deleteText(DESCRIPTION_LIMIT, length - DESCRIPTION_LIMIT, 'user');
      }
      if (descInput) {
        descInput.value = quillEditor.root.innerHTML;
      }
      updateDescriptionCounter();
      return;
    }
    if (descInput) {
      const text = getEditorPlainText();
      if (text.length > DESCRIPTION_LIMIT) {
        descInput.value = text.slice(0, DESCRIPTION_LIMIT);
      }
    }
    updateDescriptionCounter();
  }
  function renderCustomFields(defs, values) {
    if (!customFieldsContainer || typeof CustomFieldRenderer === 'undefined') return;
    CustomFieldRenderer.render(customFieldsContainer, defs, values, {
      idPrefix: 'case_cf_',
      labelClass: 'form-label',
      inputClass: 'form-control',
      selectClass: 'form-select',
      textareaRows: 3,
      emptyHtml: '<div class="text-muted small">No custom fields defined.</div>',
      hideWhenEmpty: true,
      wrapEl: customFieldsWrap
    });
  }

  async function loadCustomFieldDefs() {
    if (customFieldDefs !== null) return customFieldDefs;
    try {
      const res = await fetch(apiCases + '?limit=1&with_defs=1');
      const data = await res.json();
      customFieldDefs = data.custom_field_defs || [];
    } catch (err) {
      customFieldDefs = [];
    }
    return customFieldDefs;
  }

  function collectCustomFields() {
    if (!customFieldsContainer || !customFieldDefs || !customFieldDefs.length || typeof CustomFieldRenderer === 'undefined') return null;
    return CustomFieldRenderer.collect(customFieldsContainer);
  }

  function showError(msg) {
    const message = String(msg || '').trim();
    if (!message) return;
    if (typeof window.crmUiAlert === 'function') {
      window.crmUiAlert(message, 'Case', { stackedBackdrop: true });
      return;
    }
    if (typeof window.showSavedModal === 'function') {
      window.showSavedModal('Case', message);
      return;
    }
    if (window.console && typeof window.console.error === 'function') {
      window.console.error(message);
    }
  }

  function populateStatuses(list) {
    if (!statusSelect) return;
    statusSelect.innerHTML = '';
    (list || []).forEach((s, idx) => {
      const opt = document.createElement('option');
      opt.value = s.code || s.label || '';
      opt.textContent = s.label || s.code || '';
      if (idx === 0) opt.selected = true;
      statusSelect.appendChild(opt);
    });
    if (!statusSelect.options.length) {
      const opt = document.createElement('option');
      opt.value = 'new';
      opt.textContent = 'New';
      statusSelect.appendChild(opt);
    }
  }

  function populateComplaintTypes(list) {
    if (!complaintSelect) return;
    complaintTypeMap = {};
    complaintSelect.innerHTML = '<option value="">Select complaint type</option>';
    (list || []).forEach(item => {
      const code = item.code || item.label || '';
      if (!code) return;
      complaintTypeMap[String(code)] = {
        label: item.label || code,
        sub_types: Array.isArray(item.sub_types) ? item.sub_types : []
      };
      const opt = document.createElement('option');
      opt.value = code;
      opt.textContent = item.label || code;
      complaintSelect.appendChild(opt);
    });
  }

  function getAllowedCategoryIdsForComplaintType(complaintType) {
    const code = String(complaintType || '').trim();
    if (!code) return [];
    const mapped = Array.isArray(serviceDomainRoutes[code]) ? serviceDomainRoutes[code] : [];
    return Array.from(new Set(mapped.map((value) => String(value).trim()).filter(Boolean)));
  }

  function renderCategoriesForComplaintType(selectedValue) {
    if (!categorySelect) return;
    const complaintType = String(complaintSelect ? (complaintSelect.value || '') : '');
    const allowedIds = getAllowedCategoryIdsForComplaintType(complaintType);
    const hasMapping = allowedIds.length > 0;
    const allowedSet = new Set(allowedIds);
    const current = String(categorySelect.value || defaults.domain_id || '');
    const filtered = hasMapping
      ? categorySourceList.filter((c) => allowedSet.has(String(c.id || '')))
      : categorySourceList.slice();

    categorySelect.innerHTML = '<option value="">Select category</option>';
    categoryMeta = {};
    filtered.forEach((c) => {
      if (!c.id) return;
      const opt = document.createElement('option');
      opt.value = c.id;
      opt.textContent = c.name || ('Category #' + c.id);
      const allocation = (c.case_allocation_mode || 'manual').toLowerCase();
      opt.dataset.allocation = allocation;
      opt.dataset.escalate = (c.escalate_domain ? 1 : 0);
      categoryMeta[String(c.id)] = {
        allocation,
        escalate: (c.escalate_domain ? 1 : 0)
      };
      categorySelect.appendChild(opt);
    });

    let nextValue = '';
    if (selectedValue !== null && selectedValue !== undefined && String(selectedValue).trim() !== '') {
      nextValue = String(selectedValue).trim();
    } else if (current && (!hasMapping || allowedSet.has(String(current)))) {
      nextValue = String(current);
    }
    if (nextValue && !filtered.some((c) => String(c.id || '') === nextValue)) {
      nextValue = '';
    }
    if (nextValue !== (categorySelect.value || '')) {
      categorySelect.value = nextValue;
    }
    refreshCategorySelectUi();
    const categoryChanged = String(categorySelect.value || '') !== current;
    if (categoryChanged) {
      updateSubDomains(subDomainSelect ? subDomainSelect.value : '');
    }
  }

  function updateSubComplaintTypes(selectedValue) {
    if (!subComplaintSelect || !subComplaintWrap) return;
    const complaintType = String(complaintSelect ? (complaintSelect.value || '') : '');
    const meta = complaintType ? complaintTypeMap[complaintType] : null;
    const options = meta && Array.isArray(meta.sub_types) ? meta.sub_types : [];
    const enabled = !!(complaintTypeSettings && Number(complaintTypeSettings.enable_sub_types) === 1);
    const shouldShow = enabled && options.length > 0;

    subComplaintWrap.classList.toggle('d-none', !shouldShow);
    subComplaintSelect.disabled = !shouldShow;
    subComplaintSelect.required = shouldShow;
    subComplaintSelect.innerHTML = '<option value="">Select sub complaint type</option>';

    if (!shouldShow) {
      subComplaintSelect.value = '';
      return;
    }

    options.forEach((item) => {
      const code = item.code || item.label || '';
      if (!code) return;
      const opt = document.createElement('option');
      opt.value = code;
      opt.textContent = item.label || code;
      subComplaintSelect.appendChild(opt);
    });

    if (selectedValue) {
      subComplaintSelect.value = String(selectedValue);
      if (subComplaintSelect.value !== String(selectedValue)) {
        subComplaintSelect.value = '';
      }
    } else {
      subComplaintSelect.value = '';
    }
  }

  function populateSubDomains(list) {
    categorySubDomainMap = {};
    (Array.isArray(list) ? list : []).forEach((item) => {
      const domainId = String(item && item.id ? item.id : '').trim();
      const subs = Array.isArray(item && item.sub_domains) ? item.sub_domains : [];
      if (!domainId) return;
      categorySubDomainMap[domainId] = subs
        .map((sub) => {
          const id = String(sub && sub.id ? sub.id : '').trim();
          const name = String(sub && (sub.name || sub.label) ? (sub.name || sub.label) : '').trim();
          if (!id || !name) return null;
          return {
            id,
            name,
            is_active: Number(sub && sub.is_active ? sub.is_active : 0) === 1 ? 1 : 0,
            sort_order: Number(sub && sub.sort_order ? sub.sort_order : 0) || 0
          };
        })
        .filter(Boolean);
    });
  }

  function updateSubDomains(selectedValue) {
    if (!subDomainSelect || !subDomainWrap) return;
    if (!serviceDomainSettings.enable_sub_domains) {
      subDomainWrap.classList.add('d-none');
      subDomainSelect.disabled = true;
      subDomainSelect.required = false;
      subDomainSelect.value = '';
      return;
    }
    const domainId = String(categorySelect ? (categorySelect.value || '') : '').trim();
    const options = domainId && Array.isArray(categorySubDomainMap[domainId]) ? categorySubDomainMap[domainId] : [];
    const activeOptions = options.filter((item) => Number(item.is_active || 0) === 1);
    const shouldShow = !!domainId;
    subDomainWrap.classList.toggle('d-none', !shouldShow);
    subDomainSelect.disabled = !activeOptions.length;
    subDomainSelect.required = activeOptions.length > 0;
    subDomainSelect.classList.toggle('d-none', activeOptions.length === 0);
    subDomainSelect.innerHTML = '<option value="">Select sub service domain</option>';

    if (!shouldShow) {
      subDomainSelect.value = '';
      return;
    }

    activeOptions.forEach((item) => {
      const opt = document.createElement('option');
      opt.value = item.id;
      opt.textContent = item.name || ('Sub Service Domain #' + item.id);
      subDomainSelect.appendChild(opt);
    });

    const nextValue = selectedValue !== null && selectedValue !== undefined && String(selectedValue).trim() !== ''
      ? String(selectedValue).trim()
      : String(defaults.sub_domain_id || '').trim();
    if (nextValue) {
      subDomainSelect.value = nextValue;
      if (subDomainSelect.value !== nextValue) {
        subDomainSelect.value = '';
      }
    } else {
      subDomainSelect.value = '';
    }
  }

  function loadComplaintTypes() {
    if (!complaintSelect) return Promise.resolve();
    return fetch(apiComplaintTypes)
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error();
        complaintTypeSettings = data?.settings || { enable_sub_types: 0 };
        serviceDomainRoutes = (data?.service_domain_routes && typeof data.service_domain_routes === 'object')
          ? JSON.parse(JSON.stringify(data.service_domain_routes))
          : {};
        populateComplaintTypes(data?.data || []);
        updateSubComplaintTypes(defaults.sub_complaint_type || '');
        renderCategoriesForComplaintType(categorySelect ? categorySelect.value : '');
      })
      .catch(() => {
        complaintSelect.innerHTML = '<option value="">Select complaint type</option>';
        complaintTypeMap = {};
        complaintTypeSettings = { enable_sub_types: 0 };
        serviceDomainRoutes = {};
        updateSubComplaintTypes('');
        renderCategoriesForComplaintType(categorySelect ? categorySelect.value : '');
      });
  }

  function loadStatuses() {
    const jsonFallbackUrl = fallbackBase + 'assets/json/case_statuses.json';
    function applyStatuses(list) {
      cachedStatuses = list;
      populateStatuses(list);
      if (statusSelect && defaults && defaults.status) statusSelect.value = defaults.status;
    }
    fetch(apiStatuses)
      .then(r => { if (!r.ok) throw new Error(); return r.json(); })
      .then(data => {
        const list = Array.isArray(data?.data) ? data.data : Array.isArray(data) ? data : [];
        if (!list.length) throw new Error();
        applyStatuses(list);
      })
      .catch(() => {
        fetch(jsonFallbackUrl)
          .then(r => { if (!r.ok) throw new Error(); return r.json(); })
          .then(data => { applyStatuses(Array.isArray(data) ? data : []); })
          .catch(() => {});
      });
  }

  function populateCategories(list) {
    categorySourceList = Array.isArray(list) ? list.map((c) => ({
      id: c.id,
      name: c.name,
      case_allocation_mode: c.case_allocation_mode,
      escalate_domain: c.escalate_domain,
      sub_domains: Array.isArray(c.sub_domains) ? c.sub_domains : []
    })) : [];
    populateSubDomains(categorySourceList);
    renderCategoriesForComplaintType(defaults.complaint_type || (complaintSelect ? complaintSelect.value : ''));
    updateSubDomains(defaults.sub_domain_id || '');
  }

  function initCategorySelect2() {
    if (!categorySelect || !(window.jQuery && window.jQuery.fn && window.jQuery.fn.select2)) return;
    const $category = window.jQuery(categorySelect);
    if ($category.data('select2')) {
      $category.select2('destroy');
    }
    const opts = {
      width: '100%',
      placeholder: 'Select Service Domain',
      allowClear: true
    };
    if (modalHost) {
      opts.dropdownParent = window.jQuery(modalHost);
    }
    $category.select2(opts);
    categorySelect2Ready = true;
    refreshCategorySelectUi();
  }

  function refreshCategorySelectUi() {
    if (!categorySelect || !categorySelect2Ready || !(window.jQuery && window.jQuery.fn && window.jQuery.fn.select2)) return;
    window.jQuery(categorySelect).trigger('change.select2');
  }

  let agentSelect2Ready = false;
  function initAgentSelect2() {
    if (!agentSelect || !(window.jQuery && window.jQuery.fn && window.jQuery.fn.select2)) return;
    const $agent = window.jQuery(agentSelect);
    if ($agent.data('select2')) {
      $agent.select2('destroy');
    }
    const opts = {
      width: '100%',
      placeholder: 'Select agents',
      allowClear: true,
      closeOnSelect: false
    };
    if (modalHost) {
      opts.dropdownParent = window.jQuery(modalHost);
    }
    $agent.select2(opts);
    agentSelect2Ready = true;
  }

  function refreshAgentSelect2() {
    if (!agentSelect || !agentSelect2Ready || !(window.jQuery && window.jQuery.fn && window.jQuery.fn.select2)) return;
    window.jQuery(agentSelect).trigger('change.select2');
  }

  function loadCategories() {
    if (!categorySelect) return Promise.resolve();
    return fetch(apiCategories)
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error();
        if (data && data.settings && typeof data.settings.enable_sub_domains !== 'undefined') {
          serviceDomainSettings = { enable_sub_domains: Number(data.settings.enable_sub_domains) === 1 ? 1 : 0 };
        }
        populateCategories(data.data || data || []);
        initCategorySelect2();
      })
      .catch(() => {
        populateCategories([]);
        initCategorySelect2();
        showError('Unable to load categories. Please refresh.');
      });
  }

  function setAgentHint(message, isError) {
    if (!agentHint) return;
    agentHint.textContent = message || 'Select one or more; leave empty for unassigned.';
    agentHint.classList.toggle('text-danger', !!isError);
  }

  function renderAgentOptions(list, previousSelection = new Set()) {
    if (!agentSelect) return;
    const $agent = window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 ? window.jQuery(agentSelect) : null;
    if ($agent && $agent.data('select2')) $agent.select2('destroy');
    const defaultSelected = new Set(Array.isArray(defaults.assigned_agents) ? defaults.assigned_agents.map(String) : []);
    agentSelect.innerHTML = '';
    const hasAgents = Array.isArray(list) && list.length > 0;
    if (agentWrap) {
      agentWrap.classList.toggle('d-none', !hasAgents);
    }
    if (!hasAgents) {
      const opt = document.createElement('option');
      opt.value = '';
      opt.textContent = 'No agents available';
      agentSelect.appendChild(opt);
      initAgentSelect2();
      setAgentHint('No agents available for this service domain.', true);
      return;
    }
    list.forEach(agent => {
      const opt = document.createElement('option');
      opt.value = agent.id;
      opt.textContent = agent.display_name || ('Agent #' + agent.id);
      if (previousSelection.has(String(agent.id)) || defaultSelected.has(String(agent.id))) {
        opt.selected = true;
      }
      agentSelect.appendChild(opt);
    });
    initAgentSelect2();
    refreshAgentSelect2();
  }

  function setAgentSelect2State(disabled, placeholder) {
    if (!agentSelect) return;
    const $agent = window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 ? window.jQuery(agentSelect) : null;
    agentSelect.disabled = disabled;
    if ($agent && $agent.data('select2')) {
      $agent.select2('destroy');
    }
    const opts = {
      width: '100%',
      placeholder: placeholder || 'Select agents',
      allowClear: !disabled,
      closeOnSelect: false,
      disabled: disabled
    };
    if (modalHost) opts.dropdownParent = window.jQuery(modalHost);
    if ($agent) {
      $agent.select2(opts);
      agentSelect2Ready = true;
    }
  }

  async function loadAgentsForDomain(domainId) {
    if (!agentSelect) return [];
    const prevSelections = new Set(Array.from(agentSelect.selectedOptions).map(opt => opt.value));
    if (!domainId) {
      agentSelect.innerHTML = '';
      if (agentWrap) agentWrap.classList.add('d-none');
      setAgentSelect2State(true, 'Select a service domain first');
      setAgentHint('Select a service domain to load agents.', false);
      return [];
    }
    setAgentHint('Loading agents...', false);
    let agents = agentsCache[domainId];
    if (!agents) {
      try {
        const response = await fetch(apiAgentsByDomain + '?domain_id=' + encodeURIComponent(domainId));
        const payload = await response.json().catch(() => ({}));
        if (!response.ok) {
          throw new Error(payload.error || payload.message || 'Unable to load agents');
        }
        agents = payload.data || [];
        agentsCache[domainId] = agents;
      } catch (err) {
        setAgentHint('Unable to load agents for this domain.', true);
        agentSelect.innerHTML = '';
        if (agentWrap) agentWrap.classList.add('d-none');
        setAgentSelect2State(true, 'Unable to load agents');
        return [];
      }
    }
    renderAgentOptions(agents, prevSelections);
    setAgentHint('Agents filtered to this domain. Leave empty for unassigned.', false);
    return agents;
  }

  async function updateAgentControls() {
    if (!categorySelect || !agentSelect) return;
    const domainId = categorySelect.value || '';
    const meta = domainId && categoryMeta[String(domainId)] ? categoryMeta[String(domainId)] : null;
    const allocation = (meta?.allocation || 'manual').toLowerCase();
    if (allocation === 'automatic') {
      agentSelect.innerHTML = '';
      if (agentWrap) agentWrap.classList.add('d-none');
      setAgentSelect2State(true, 'Auto-allocation enabled for this domain');
      setAgentHint('Automatic allocation is enabled — assignments are determined automatically.', false);
      return;
    }
    await loadAgentsForDomain(domainId);
  }

  function canToggleApproval(domainId) {
    if (!approvalsEnabled) return false;
    if (approvalsIsSuperAdmin) return true;
    if (!approvalsCanReadAll) return false;
    if (!domainId) return false;
    if (approvalsAgentDomains && approvalsAgentDomains.length) {
      return approvalsAgentDomains.map(Number).includes(Number(domainId));
    }
    if (!approvalsAgentDomain) return false;
    return Number(approvalsAgentDomain) === Number(domainId);
  }

  function updateApprovalToggleState() {
    if (!approvalWrap || !approvalToggle) return;
    if (!approvalsEnabled) {
      approvalWrap.classList.add('d-none');
      return;
    }
    approvalWrap.classList.remove('d-none');
    if (approvalsGlobalRequired) {
      approvalToggle.checked = true;
      approvalToggle.disabled = true;
      return;
    }
    const domainId = categorySelect ? categorySelect.value : null;
    const allowed = canToggleApproval(domainId);
    approvalToggle.disabled = !allowed;
  }

  function handleCategoryChange() {
    updateAgentControls();
    updateApprovalToggleState();
    updateSubDomains(subDomainSelect ? subDomainSelect.value : '');
  }

  const quillCss = opts.quillCss || (fallbackBase + 'assets/libs/quill/quill.snow.css');
  const quillJs = opts.quillJs || (fallbackBase + 'assets/libs/quill/quill.min.js');
  function ensureQuill(cb) {
    if (window.Quill) { cb(); return; }
    if (!document.getElementById('quill-style')) {
      const link = document.createElement('link');
      link.id = 'quill-style';
      link.rel = 'stylesheet';
      link.href = quillCss;
      document.head.appendChild(link);
    }
    let script = document.getElementById('quill-script');
    if (!script) {
      script = document.createElement('script');
      script.id = 'quill-script';
      script.src = quillJs;
      document.head.appendChild(script);
    }
    script.addEventListener('load', () => { if (window.Quill) cb(); }, { once: true });
  }

  function initEditor() {
    if (!descEditorEl || quillEditor) return;
    // Ensure Quill CSS is present even when Quill JS was loaded by the page already.
    if (!document.getElementById('quill-style')) {
      const link = document.createElement('link');
      link.id = 'quill-style';
      link.rel = 'stylesheet';
      link.href = quillCss;
      document.head.appendChild(link);
    }
    const create = () => {
      quillEditor = new Quill(descEditorEl, {
        theme: 'snow',
        placeholder: 'Describe the issue',
        modules: {
          toolbar: [
            [{ header: [1, 2, 3, false] }],
            ['bold', 'italic', 'underline', 'strike'],
            [{ list: 'ordered' }, { list: 'bullet' }],
            ['link'],
            ['clean']
          ]
        }
      });
      if (descInput && descInput.value) {
        quillEditor.root.innerHTML = descInput.value;
      }
      quillEditor.on('text-change', () => {
        enforceDescriptionLimit();
      });
      enforceDescriptionLimit();
    };
    if (window.Quill) create(); else ensureQuill(create);
  }

  function renderFileList() {
    if (!fileList) return;
    fileList.innerHTML = '';
    if (uploadingFiles) {
      const span = document.createElement('span');
      span.className = 'text-muted';
      span.textContent = 'Uploading...';
      fileList.appendChild(span);
      return;
    }
    if (!uploadedFiles.length) {
      fileList.textContent = 'No files uploaded yet.';
      return;
    }
    uploadedFiles.forEach(f => {
      const row = document.createElement('div');
      row.className = 'd-flex align-items-center text-success';
      const icon = document.createElement('i');
      icon.className = 'ri-attachment-line me-1';
      const name = document.createElement('span');
      name.textContent = f.name || ('File #' + f.id);
      const idTag = document.createElement('span');
      idTag.className = 'ms-1 text-muted';
      idTag.textContent = '#' + f.id;
      row.appendChild(icon);
      row.appendChild(name);
      row.appendChild(idTag);
      fileList.appendChild(row);
    });
  }

  async function uploadSelectedFiles() {
    if (!filesEnabled) return;
    if (!fileInput || !fileInput.files || !fileInput.files.length) return;
    if (saveBtn) {
      saveBtn.disabled = true;
    }
    uploadingFiles = true;
    renderFileList();
    const picked = Array.from(fileInput.files);
    for (const f of picked) {
      const fd = new FormData();
      const subj = (subjectInput && subjectInput.value.trim()) ? subjectInput.value.trim() : 'case';
      const ext = (f.name && f.name.includes('.')) ? f.name.substring(f.name.lastIndexOf('.')) : '';
      const niceName = `${subj.replace(/\s+/g, '_')}_${Date.now()}${ext}`;
      fd.append('file', f, niceName);
      try {
        const res = await fetch(apiFiles, { method: 'POST', body: fd });
        const data = await res.json().catch(() => ({}));
        if (!res.ok) {
          const msg = data.error || data.message || 'Upload failed';
          throw new Error(msg);
        }
        uploadedFiles.push({ id: data.id, name: data.file_name || f.name });
      } catch (err) {
        showError(err.message || 'Unable to upload file');
      }
    }
    uploadedFileIds = uploadedFiles.map(f => f.id);
    if (fileInput) fileInput.value = '';
    uploadingFiles = false;
    renderFileList();
    if (saveBtn) {
      saveBtn.disabled = false;
    }
  }

  function setSearchStatus(el, msg, isError) {
    if (!el) return;
    el.textContent = msg || '';
    el.classList.toggle('text-danger', !!isError);
    el.classList.toggle('text-muted', !isError);
  }

  function inferParentState(def) {
    const data = def || {};
    const explicitType = data.parentType || data.parent_type || '';
    const explicitIdRaw = data.parentId ?? data.parent_id;
    const explicitId = (explicitIdRaw !== null && explicitIdRaw !== undefined && String(explicitIdRaw).trim() !== '')
      ? String(explicitIdRaw).trim()
      : null;
    const explicitLabel = data.parentLabel || data.parent_label || '';
    if (explicitType && explicitId && parentMeta[explicitType]) {
      return { type: explicitType, id: explicitId, label: explicitLabel };
    }
    const accountRaw = data.accountId ?? data.account_id;
    const contactRaw = data.contactId ?? data.contact_id;
    const organizationRaw = data.organizationId ?? data.organization_id;
    const leadRaw = data.leadId ?? data.lead_id;
    const accountId = (accountRaw !== null && accountRaw !== undefined && String(accountRaw).trim() !== '') ? String(accountRaw).trim() : null;
    const contactId = (contactRaw !== null && contactRaw !== undefined && String(contactRaw).trim() !== '') ? String(contactRaw).trim() : null;
    const organizationId = (organizationRaw !== null && organizationRaw !== undefined && String(organizationRaw).trim() !== '') ? String(organizationRaw).trim() : null;
    const leadId = (leadRaw !== null && leadRaw !== undefined && String(leadRaw).trim() !== '') ? String(leadRaw).trim() : null;
    if (accountId && parentMeta.account) return { type: 'account', id: accountId, label: data.accountLabel || data.account_name || '' };
    if (leadId && parentMeta.lead) return { type: 'lead', id: leadId, label: data.leadLabel || data.lead_title || '' };
    if (contactId) return { type: 'contact', id: contactId, label: data.contactLabel || data.contact_name || '' };
    if (organizationId) return { type: 'organization', id: organizationId, label: data.organizationLabel || data.organization_name || '' };
    return { type: defaultParentType, id: null, label: '' };
  }

  function parentText(type, key) {
    const meta = parentMeta[type] || parentMeta[defaultParentType];
    return meta[key] || '';
  }

  function setParentTypeUi(type) {
    const resolved = parentMeta[type] ? type : defaultParentType;
    if (parentTypeSelect) parentTypeSelect.value = resolved;
    if (parentSearchInput) parentSearchInput.placeholder = parentText(resolved, 'searchPlaceholder');
    if (parentSelect) {
      if (!parentSelect.options.length) {
        parentSelect.innerHTML = `<option value="">${parentText(resolved, 'selectPlaceholder')}</option>`;
      } else {
        parentSelect.options[0].textContent = parentText(resolved, 'selectPlaceholder');
      }
    }
  }

  function setParentSelection(type, id, label) {
    const resolved = parentMeta[type] ? type : defaultParentType;
    const parentId = (id !== null && id !== undefined && String(id).trim() !== '') ? String(id).trim() : null;
    setParentTypeUi(resolved);
    if (!parentSelect) return;
    const placeholder = parentText(resolved, 'selectPlaceholder');
    parentSelect.innerHTML = `<option value="">${placeholder}</option>`;
    if (parentId) {
      const opt = document.createElement('option');
      opt.value = parentId;
      opt.textContent = label || (parentText(resolved, 'label') + ' #' + parentId);
      parentSelect.appendChild(opt);
      parentSelect.value = parentId;
      if (parentSelected) parentSelected.textContent = 'Selected: ' + opt.textContent;
    } else {
      parentSelect.value = '';
      if (parentSelected) parentSelected.textContent = '';
    }
  }

  async function loadParentOptions(type, term) {
    if (!parentSelect) return;
    const resolved = parentMeta[type] ? type : defaultParentType;
    const q = (term || '').trim();
    if (q !== '' && q.length < minSearchLen) {
      setSearchStatus(parentSearchStatus, `Type at least ${minSearchLen} characters, then press Enter.`, false);
      return;
    }
    setSearchStatus(parentSearchStatus, 'Searching...', false);
    const currentValue = String(parentSelect.value || '');
    const currentLabel = currentValue && parentSelect.selectedOptions.length
      ? parentSelect.selectedOptions[0].textContent
      : '';
    const seq = ++parentLookupSeq;
    try {
      const params = new URLSearchParams();
      params.set('lookup', parentMeta[resolved].lookup);
      params.set('limit', String(searchLimit));
      if (q) params.set('q', q);
      const res = await fetch(apiCases + '?' + params.toString());
      const data = await res.json().catch(() => ({}));
      if (seq !== parentLookupSeq) return;
      if (!res.ok) throw new Error(data.error || 'Lookup failed');
      const rows = Array.isArray(data.data) ? data.data : [];
      parentSelect.innerHTML = `<option value="">${parentText(resolved, 'selectPlaceholder')}</option>`;
      rows.forEach((row) => {
        const opt = document.createElement('option');
        opt.value = String(row.id);
        opt.textContent = row.label || (parentText(resolved, 'label') + ' #' + row.id);
        parentSelect.appendChild(opt);
      });
      if (currentValue) {
        const hasCurrent = Array.from(parentSelect.options).some((opt) => String(opt.value) === currentValue);
        if (!hasCurrent) {
          const opt = document.createElement('option');
          opt.value = currentValue;
          opt.textContent = currentLabel || (parentText(resolved, 'label') + ' #' + currentValue);
          parentSelect.appendChild(opt);
        }
        parentSelect.value = currentValue;
      }
      if (rows.length === 0) {
        setSearchStatus(parentSearchStatus, 'No result found.', true);
      } else {
        setSearchStatus(parentSearchStatus, '', false);
      }
    } catch (err) {
      const msg = (err && err.message) ? err.message : 'Lookup unavailable.';
      setSearchStatus(parentSearchStatus, msg, true);
    }
  }

  function setDefaults(def) {
    const d = def || defaults;
    defaults = d;
    if (form) form.reset();
    showError('');
    uploadedFiles = [];
    uploadedFileIds = [];
    uploadingFiles = false;
    renderFileList();
    if (prioritySelect) prioritySelect.value = 'medium';
    populateStatuses(cachedStatuses);
    const parent = inferParentState(d);
    if (parentSearchInput) parentSearchInput.value = '';
    setParentSelection(parent.type, parent.id, parent.label);
    setSearchStatus(parentSearchStatus, `Type at least ${minSearchLen} characters, then press Enter.`, false);
    if (subjectInput) subjectInput.value = d.subject || '';
    if (prioritySelect) prioritySelect.value = d.priority || 'medium';
    if (statusSelect) statusSelect.value = d.status || 'new';
    if (approvalToggle) {
      approvalToggle.checked = !!d.requires_approval;
      updateApprovalToggleState();
    }
    if (complaintSelect) {
      const complaint = d.complaint_type || (d.custom_fields ? d.custom_fields.complaint_type : '') || '';
      complaintSelect.value = complaint;
      updateSubComplaintTypes(d.sub_complaint_type || '');
    }
    if (categorySelect) {
      categorySelect.value = d.domain_id || '';
      renderCategoriesForComplaintType(d.domain_id || '');
      updateSubDomains(d.sub_domain_id || '');
    }
    if (agentSelect) {
      const selected = Array.isArray(d.assigned_agents) ? d.assigned_agents.map(String) : [];
      Array.from(agentSelect.options).forEach(opt => {
        opt.selected = selected.includes(String(opt.value));
      });
      refreshAgentSelect2();
    }
    if (descInput) {
      descInput.value = d.description || '';
    }
    if (quillEditor) {
      quillEditor.root.innerHTML = descInput ? (descInput.value || '') : '';
    }
    updateDescriptionCounter();
    const values = d.custom_fields || {};
    const defsToUse = def && def.custom_field_defs ? def.custom_field_defs : customFieldDefs;
    if (defsToUse) {
      customFieldDefs = defsToUse;
      renderCustomFields(defsToUse, values);
    } else {
      loadCustomFieldDefs().then(defs => renderCustomFields(defs, values));
    }
  }

  async function handleSave() {
    showError('');
    if (!subjectInput || !statusSelect || !prioritySelect) return;
    const originalHtml = saveBtn ? saveBtn.innerHTML : '';
    if (saveBtn) {
      saveBtn.disabled = true;
      saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Saving...';
    }
    if (uploadingFiles) {
      showError('Please wait for files to finish uploading.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }

    const parentType = parentTypeSelect ? String(parentTypeSelect.value || defaultParentType) : defaultParentType;
    const parentId = parentSelect ? String(parentSelect.value || '').trim() : '';
    const payload = {
      subject: subjectInput.value.trim(),
      description: quillEditor ? quillEditor.root.innerHTML.trim() : (descInput ? descInput.value.trim() : ''),
      priority: prioritySelect.value,
      status: statusSelect.value,
      domain_id: categorySelect ? parseInt(categorySelect.value, 10) || null : null,
      sub_domain_id: subDomainSelect && !subDomainSelect.disabled ? (parseInt(subDomainSelect.value, 10) || null) : null,
      complaint_type: complaintSelect ? (complaintSelect.value || '') : '',
      sub_complaint_type: subComplaintSelect && !subComplaintSelect.disabled ? (subComplaintSelect.value || '') : '',
      parent_type: parentType,
      parent_id: parentId || null,
      account_id: parentType === 'account' ? parentId : null,
      organization_id: parentType === 'organization' ? parentId : null,
      lead_id: parentType === 'lead' ? parentId : null,
      contact_ids: parentType === 'contact' && parentId ? [parentId] : [],
      assigned_agents: agentSelect ? Array.from(agentSelect.selectedOptions).map(o => parseInt(o.value, 10)).filter(n => n) : [],
      file_ids: uploadedFileIds.slice()
    };
    if (approvalToggle && approvalsEnabled && !approvalToggle.disabled) {
      payload.requires_approval = approvalToggle.checked ? 1 : 0;
    }
    const cf = collectCustomFields();
    if (cf !== null) {
      payload.custom_fields = cf;
    }

    if (!payload.subject) {
      showError('Subject is required.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }
    if (!getEditorPlainText().trim()) {
      showError('Description is required.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }
    if (!payload.parent_id || !parentMeta[payload.parent_type]) {
      const labels = Object.keys(parentMeta).map((key) => moduleLabel(key, 'singular'));
      showError('Please select one parent record (' + labels.join(', ') + ').');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }
    if (!payload.domain_id || !payload.complaint_type) {
      showError('Please select a service domain and complaint type.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }
    if (subDomainSelect && !subDomainSelect.disabled && !(payload.sub_domain_id || '').toString().trim()) {
      showError('Please select a sub service domain.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }
    if (subComplaintSelect && !subComplaintSelect.disabled && !(payload.sub_complaint_type || '').trim()) {
      showError('Please select a sub complaint type.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }
    if (uploadingFiles) {
      showError('Please wait for files to finish uploading.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      return;
    }

    const url = caseId ? (apiCaseDetail + '?id=' + encodeURIComponent(caseId)) : apiCases;
    const method = caseId ? 'PATCH' : 'POST';

    if (typeof opts.beforeSubmit === 'function') {
      const proceed = opts.beforeSubmit(payload);
      if (proceed === false) {
        if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
        return;
      }
    }

    fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
      .then(async r => {
        const data = await r.json().catch(() => ({}));
        if (!r.ok) {
          const msg = data.error || data.message || 'Unable to create case.';
          throw new Error(msg);
        }
        if (typeof opts.onSaved === 'function') opts.onSaved(data);
      })
      .catch(err => showError(err.message || 'Unable to create case.'))
      .finally(() => {
        if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = originalHtml; }
      });
  }

  if (parentTypeSelect) {
    parentTypeSelect.addEventListener('change', () => {
      const type = parentTypeSelect.value || defaultParentType;
      setParentSelection(type, null, '');
      if (parentSearchInput) parentSearchInput.value = '';
      setSearchStatus(parentSearchStatus, `Type at least ${minSearchLen} characters, then press Enter.`, false);
    });
  }
  if (parentSearchInput) {
    const performParentSearch = () => {
      const type = parentTypeSelect ? parentTypeSelect.value : defaultParentType;
      loadParentOptions(type, parentSearchInput.value || '');
    };
    parentSearchInput.addEventListener('keydown', (e) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        performParentSearch();
      }
    });
    parentSearchInput.addEventListener('blur', performParentSearch);
  }
  if (parentSelect) {
    parentSelect.addEventListener('change', () => {
      const selected = parentSelect.selectedOptions && parentSelect.selectedOptions.length
        ? parentSelect.selectedOptions[0].textContent
        : '';
      if (parentSelected) {
        parentSelected.textContent = parentSelect.value ? ('Selected: ' + selected) : '';
      }
    });
  }
  if (categorySelect) {
    categorySelect.addEventListener('change', handleCategoryChange);
    if (window.jQuery && window.jQuery.fn) {
      window.jQuery(categorySelect)
        .off('.caseFormDomain')
        .on('change.caseFormDomain select2:select.caseFormDomain select2:clear.caseFormDomain', handleCategoryChange);
    }
  }
  if (complaintSelect) {
    complaintSelect.addEventListener('change', () => {
      updateSubComplaintTypes('');
      renderCategoriesForComplaintType(categorySelect ? categorySelect.value : '');
    });
  }
  if (fileInput) {
    if (!filesEnabled) {
      fileInput.disabled = true;
    } else {
      fileInput.addEventListener('change', uploadSelectedFiles);
    }
  }
  if (saveBtn) saveBtn.addEventListener('click', handleSave);

  const parentModal = form ? form.closest('.modal') : null;
  const parentOffcanvas = form ? form.closest('.offcanvas') : null;
  if (parentModal) {
    parentModal.addEventListener('shown.bs.modal', initEditor, { once: false });
  } else if (parentOffcanvas) {
    parentOffcanvas.addEventListener('shown.bs.offcanvas', initEditor, { once: false });
  }

  loadStatuses();
  (async () => {
    await loadCategories().catch(() => {});
    await updateAgentControls().catch(() => {});
    setDefaults(defaults);
  })();
  loadComplaintTypes().then(() => {
    if (complaintSelect) {
      complaintSelect.value = defaults.complaint_type || '';
      updateSubComplaintTypes(defaults.sub_complaint_type || '');
      renderCategoriesForComplaintType(defaults.domain_id || '');
      updateSubDomains(defaults.sub_domain_id || '');
    }
  });
  // Editor is initialized on shown event for modal/offcanvas (or immediately if inline).
  if (!parentModal && !parentOffcanvas) {
    initEditor();
  }
  updateDescriptionCounter();
  renderFileList();
  loadCustomFieldDefs().then(defs => renderCustomFields(defs, defaults.custom_fields || {}));

  return {
    reset: setDefaults,
    setDefaults,
    setStatusList: populateStatuses
  };
}
