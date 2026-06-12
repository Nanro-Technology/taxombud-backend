(function() {
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
  if (window.jQuery && window.jQuery.fn && window.jQuery.fn.DataTable && window.jQuery.fn.DataTable.isDataTable('#contactTable')) {
    return;
  }
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiDetail = apiMap.contactsDetail || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/contacts/detail');
  const apiList = apiMap.contactsList || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/contacts/index');
  const apiExport = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/contacts/export';
  const apiDelete = apiDetail;
  const params = new URLSearchParams(window.location.search);
  const cfg = window.contactsPageConfig || {};
  const featureAccounts = cfg.featureAccounts !== false;
  const featureLeads = cfg.featureLeads !== false;
  const inboundPhone = params.get('phone');
  let suppressTableOverlay = !!inboundPhone;

  const alertBox = document.getElementById('contactAlert');
  const contactOffcanvas = new bootstrap.Offcanvas('#contactOffcanvas');

  const searchInput = document.getElementById('contactSearch');
  const searchBySelect = document.getElementById('contactSearchBy');
  const searchBtn = document.getElementById('contactSearchBtn');
  const resetBtn = document.getElementById('contactResetBtn');
  const quickTableSearch = document.getElementById('quickTableSearch');
  const leadFilter = document.getElementById('contactLeadFilter');
  const contactNameHeading = document.getElementById('contactNameHeading');
  const accountFilter = document.getElementById('contactAccountFilter');
  const bpaBranchFilter = document.getElementById('contactBpaBranchFilter');
  const contactDateRange = document.getElementById('contactDateRange');
  const inboundAccountId = params.get('account_id');
  const inboundAccountName = params.get('account_name');
  const datePicker = contactDateRange ? flatpickr(contactDateRange, { mode: 'range', dateFormat: 'Y-m-d', allowInput: true }) : null;
  const apiAccounts = apiMap.accountsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/accounts/index');
  let accountRequestSeq = 0;
  let manualTableReload = false;
  const contactForm = setupContactForm({
    offcanvasSelector: '#contactFormOffcanvas',
    onSaved: (data, isNew) => {
      if (isNew && data && data.id) {
        window.location.assign('studio/contacts/view.kml?id=' + encodeURIComponent(data.id_s || data.id));
      } else {
        window.location.reload();
      }
    }
  });

  if (inboundPhone) {
    if (contactForm) {
      let stripped = inboundPhone.trim();
      if (stripped.startsWith('+234')) {
        stripped = stripped.slice(4);
      } else {
        stripped = stripped.replace(/^0+/, '');
      }
      contactForm.openCreate({ phone: stripped || inboundPhone }).finally(() => {
        suppressTableOverlay = false;
      });
    } else {
      suppressTableOverlay = false;
    }
  } else {
    suppressTableOverlay = false;
  }

  if (featureAccounts && accountFilter && inboundAccountId) {
    const existing = Array.from(accountFilter.options || []).find((o) => String(o.value) === String(inboundAccountId));
    if (!existing) {
      const opt = document.createElement('option');
      opt.value = String(inboundAccountId);
      opt.textContent = inboundAccountName && String(inboundAccountName).trim()
        ? String(inboundAccountName).trim()
        : (moduleLabel('account', 'singular') + ' #' + String(inboundAccountId));
      accountFilter.appendChild(opt);
    }
    accountFilter.value = String(inboundAccountId);
  }

  const tableNode = $('#contactTable');
  const contactTable = tableNode.DataTable({
    serverSide: true,
    processing: true,
    searching: false,
    columnDefs: [
      { targets: 'lead-count-col', visible: false, searchable: false },
      { targets: 'account-col', visible: featureAccounts, searchable: false },
      { targets: 'account-id-col', visible: false, searchable: false },
      { targets: -1, orderable: false, searchable: false }
    ],
    order: [[0, 'asc']],
    pageLength: (window.appConfig && window.appConfig.dataTablePageSize) ? window.appConfig.dataTablePageSize : 250,
    lengthMenu: [[250],[250]],
    ajax: {
      url: apiList,
      type: 'GET',
      data: function(d) {
        const term = searchInput ? searchInput.value.trim() : '';
        const searchBy = searchBySelect ? searchBySelect.value : 'all';
        if (term) d.q = term;
        if (term && searchBy && searchBy !== 'all') d.search_by = searchBy;
        if (featureLeads && leadFilter && leadFilter.value) {
          if (leadFilter.value === 'leads') d.has_lead = '1';
          if (leadFilter.value === 'contacts') d.has_lead = '0';
        }
        if (featureAccounts && accountFilter && accountFilter.value) {
          d.account_id = accountFilter.value;
        }
        if (bpaBranchFilter && bpaBranchFilter.value) {
          d.bpa_branch_code = bpaBranchFilter.value;
        }
        // Dynamic custom-field filters (rendered from fields flagged "Show in search bar").
        document.querySelectorAll('.contact-cf-filter').forEach(function (sel) {
          var key = sel.getAttribute('data-cf-key');
          if (key && sel.value) {
            d['cf[' + key + ']'] = sel.value;
          }
        });
        if (datePicker && datePicker.selectedDates.length) {
          const [start, end] = datePicker.selectedDates;
          const s = formatDate(start);
          const e = formatDate(end || start);
          if (s) d.start_date = s;
          if (e) d.end_date = e;
        }
        d.limit = d.length;
      },
      dataSrc: function(json) {
        return json.data || [];
      },
      error: function() {
        setTableLoading(false);
        manualTableReload = false;
        if (typeof window.setGlobalUiLoading === 'function') {
          window.setGlobalUiLoading('dt:contactTable-manual', false);
          window.setGlobalUiLoading('dt:contactTable', false);
        }
        showPageError('Unable to load contacts.');
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
          const name = ((row.first_name || '') + ' ' + (row.middle_name || '') + ' ' + (row.last_name || '')).trim() || (row.phone || row.email || (moduleLabel('contact', 'singular') + ' #' + id));
          const isLead = rowHasLead(row);
          const subtitle = escapeHtml(row.email || row.phone || '');
          return `
            <div class="entity-name-cell">
              ${avatarHtml(name)}
              <div>
                <div class="fw-semibold">
                  <a href="studio/contacts/view.kml?id=${encodeURIComponent(id)}" class="text-decoration-underline">${escapeHtml(name)}</a>
                  ${isLead ? '<span class="badge bg-warning-subtle text-warning ms-1">Lead</span>' : ''}
                </div>
                ${subtitle ? `<div class="small text-muted">${subtitle}</div>` : ''}
                ${buildInlineActions(encodeURIComponent(id))}
              </div>
            </div>`;
        }
      },
      {
        data: 'account_name',
        render: function(_data, _type, row) {
          return renderAccountCell(row);
        }
      },
      {
        data: 'account_id',
        render: function(data) {
          return data || '';
        }
      },
      {
        data: 'phone',
        render: function(data) {
          const phone = normalizePhone(data);
          return phone
            ? `<a href="${outboundCallHref(phone)}" class="confirm-call" data-phone="${escapeHtml(phone)}">${escapeHtml(phone)}</a>`
            : 'N/A';
        }
      },
      {
        data: 'email',
        render: function(data) {
          const email = normalizeEmail(data);
          return email
            ? `<a href="${outboundEmailHref(email)}">${escapeHtml(email)}</a>`
            : 'N/A';
        }
      },
      {
        data: 'created_at',
        render: function(data) {
          return escapeHtml(data || '');
        }
      },
      {
        data: 'lead_count',
        render: function(data) {
          return data || 0;
        }
      },
      {
        data: null,
        render: function(data, type, row) {
          const id = row.id_s || row.id;
          return buildActions(encodeURIComponent(id));
        }
      }
    ]
  });

  function syncLeadModeHeading() {
    if (!contactNameHeading) return;
    contactNameHeading.textContent = (leadFilter && leadFilter.value === 'leads') ? 'Lead Contact' : 'Name';
  }

  function rowHasLead(row) {
    const value = (row && Object.prototype.hasOwnProperty.call(row, 'has_active_lead'))
      ? row.has_active_lead
      : (row ? row.is_lead : '');
    return featureLeads && String(value || '') === '1';
  }

  function setTableLoading(show) {
    if (!manualTableReload) {
      return;
    }
    if (suppressTableOverlay) {
      return;
    }
    if (typeof window.setGlobalUiLoading === 'function') {
      window.setGlobalUiLoading('dt:contactTable-manual', !!show, 30000);
      if (!show) {
        manualTableReload = false;
      }
      return;
    }
    const pageLoader = document.getElementById('loader');
    if (!pageLoader) return;
    pageLoader.style.display = show ? 'flex' : 'none';
    pageLoader.style.pointerEvents = 'none';
    if (!show) {
      manualTableReload = false;
    }
  }

  tableNode.on('preXhr.dt', function() { setTableLoading(true); });
  tableNode.on('xhr.dt', function() { setTableLoading(false); });
  tableNode.on('error.dt', function() { setTableLoading(false); });
  tableNode.on('draw.dt', function() { applyQuickTableFilter(); });
  contactTable.on('processing.dt', function(_e, _settings, processing) {
    setTableLoading(!!processing);
  });

  function escapeHtml(str) {
    return String(str || '').replace(/[&<>"']/g, (m) => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]));
  }

  const AVATAR_COLORS = ['#4f46e5','#0891b2','#059669','#d97706','#dc2626','#7c3aed','#db2777','#0284c7','#16a34a','#ca8a04'];
  function avatarColor(name) {
    let h = 0;
    for (let i = 0; i < (name||'').length; i++) h = ((h << 5) - h) + name.charCodeAt(i);
    return AVATAR_COLORS[Math.abs(h) % AVATAR_COLORS.length];
  }
  function avatarInitials(name) {
    const parts = (name||'').trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return '?';
    return (parts[0][0] + (parts[1] ? parts[1][0] : '')).toUpperCase();
  }
  function avatarHtml(name) {
    return `<div class="entity-avatar" style="background:${avatarColor(name)}">${avatarInitials(name)}</div>`;
  }

  function normalizePhone(value) {
    return String(value || '').replace(/^tel:\s*/i, '').trim();
  }

  function normalizeEmail(value) {
    return String(value || '').replace(/^mailto:\s*/i, '').trim();
  }

  function renderAccountCell(row) {
    if (!featureAccounts) return '';
    const accountName = String(row && row.account_name ? row.account_name : '').trim();
    const accountIdSalted = String(row && row.account_id_s ? row.account_id_s : '').trim();
    const accountIdRaw = String(row && row.account_id ? row.account_id : '').trim();
    if (!accountName) return '-';
    const targetId = accountIdSalted || accountIdRaw;
    if (!targetId) return escapeHtml(accountName);
    return `<a href="studio/accounts/view.kml?id=${encodeURIComponent(targetId)}" class="text-decoration-underline">${escapeHtml(accountName)}</a>`;
  }

  const outboundCallHref = (window.crmComms && window.crmComms.outboundCallHref)
    ? window.crmComms.outboundCallHref
    : (phone) => '../studio/outbound.kml?call=1&phone=' + encodeURIComponent(phone || '');
  const outboundEmailHref = (window.crmComms && window.crmComms.outboundEmailHref)
    ? window.crmComms.outboundEmailHref
    : (email) => '../studio/outbound.kml?email=1&to=' + encodeURIComponent(email || '');

  function showPageError(msg) {
    if (!alertBox) return;
    alertBox.textContent = msg || '';
    alertBox.classList.toggle('d-none', !msg);
  }

  const canEdit = !!cfg.canEdit;
  const canDelete = !!cfg.canDelete;

  function buildActions(id) {
    return `
      <div class="contact-actions d-flex align-items-center justify-content-start gap-1 flex-wrap w-100">
        <button class="btn btn-light btn-sm btn-view-contact" data-contact-id="${id}">
          <span class="spinner-border spinner-border-sm d-none qv-spinner" role="status" aria-hidden="true"></span>
          <i class="ri-eye-line me-1 qv-icon"></i>Summary
        </button>
        <button class="btn btn-soft-primary btn-sm dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
          <i class="ri-more-fill"></i> More
        </button>
        <div class="dropdown-menu dropdown-menu-start">
          <a class="dropdown-item" href="studio/contacts/view.kml?id=${id}"><i class="ri-file-list-3-line me-1"></i>Details</a>
          ${canEdit ? `<button class="dropdown-item btn-edit-contact" data-contact-id="${id}"><i class="ri-edit-line me-1"></i>Edit<span class="spinner-border spinner-border-sm d-none ms-2" role="status" aria-hidden="true"></span></button>` : ''}
          ${canDelete ? `<button class="dropdown-item text-danger btn-delete-contact" data-contact-id="${id}"><i class="ri-delete-bin-6-line me-1"></i>Delete</button>` : ''}
        </div>
      </div>`;
  }

  function buildInlineActions(id) {
    const parts = [
      `<a href="javascript:void(0);" class="btn-view-contact" data-contact-id="${id}">Summary</a>`,
      `<a href="studio/contacts/view.kml?id=${id}">View</a>`
    ];
    if (canEdit) parts.push(`<a href="javascript:void(0);" class="btn-edit-contact" data-contact-id="${id}">Edit<span class="spinner-border spinner-border-sm d-none ms-1" role="status" aria-hidden="true"></span></a>`);
    return `<div class="contact-inline-actions small mt-1">${parts.join(' <span class="text-muted">|</span> ')}</div>`;
  }

  function renderRows(rows) {
    contactTable.clear();
    (rows || []).forEach((c, idx) => {
      const id = c.id_s || c.id;
      const name = ((c.first_name || '') + ' ' + (c.middle_name || '') + ' ' + (c.last_name || '')).trim() || (c.phone || c.email || (moduleLabel('contact', 'singular') + ' #' + id));
      const isLead = rowHasLead(c);
      const nameHtml = `<div><a href="studio/contacts/view.kml?id=${encodeURIComponent(id)}" class="text-decoration-underline">${escapeHtml(name)}</a>${isLead ? ' <span class="badge bg-warning-subtle text-warning ms-2">Lead</span>' : ''}</div>${buildInlineActions(encodeURIComponent(id))}`;
      const phone = normalizePhone(c.phone);
      const email = normalizeEmail(c.email);
      const phoneHtml = phone
        ? `<a href="${outboundCallHref(phone)}" class="confirm-call" data-phone="${escapeHtml(phone)}">${escapeHtml(phone)}</a>`
        : 'N/A';
      const emailHtml = email
        ? `<a href="${outboundEmailHref(email)}">${escapeHtml(email)}</a>`
        : 'N/A';
      contactTable.row.add([
        idx + 1,
        nameHtml,
        renderAccountCell(c),
        c.account_id || '',
        phoneHtml,
        emailHtml,
        escapeHtml(c.tin_number || ''),
        escapeHtml(c.created_at || ''),
        c.lead_count || 0,
        buildActions(encodeURIComponent(id))
      ]);
    });
    contactTable.draw(false);
  }

  function applySearch() {
    syncLeadModeHeading();
    manualTableReload = true;
    setTableLoading(true);
    contactTable.ajax.reload();
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

  if (searchBtn) {
    searchBtn.addEventListener('click', () => {
      applySearch();
    });
  }

  if (searchInput) searchInput.addEventListener('keypress', (e) => { if (e.key === 'Enter') { e.preventDefault(); applySearch(); } });
  if (searchBySelect) searchBySelect.addEventListener('change', () => {
    // Search-by should only apply when user explicitly clicks Search (or presses Enter in search input).
  });
  if (featureLeads && leadFilter) leadFilter.addEventListener('change', () => applySearch());
  if (featureAccounts && accountFilter) accountFilter.addEventListener('change', () => applySearch());
  if (bpaBranchFilter) bpaBranchFilter.addEventListener('change', () => applySearch());

  // Dynamic custom-field filters: search on change, and when a parent filter changes,
  // reset any child filter that depends on it (so stale child selections don't yield
  // empty results). data-cf-parent points at the parent field's key.
  (function initCfFilters() {
    const cfFilters = Array.prototype.slice.call(document.querySelectorAll('.contact-cf-filter'));
    if (!cfFilters.length) return;

    // Filter a child select's options to only those whose data-parent-value matches
    // the parent's current value (cascade). Empty parent value = show all.
    function filterChildOptions(child, parentVal) {
      Array.prototype.forEach.call(child.options, function (opt) {
        if (!opt.value) { opt.hidden = false; return; } // keep the "All ..." option
        const pv = opt.getAttribute('data-parent-value') || '';
        opt.hidden = parentVal !== '' && pv !== '' && pv !== parentVal;
      });
      // If the child's current selection is now hidden, clear it.
      if (child.value) {
        const cur = child.querySelector('option[value="' + child.value.replace(/"/g, '\\"') + '"]');
        if (cur && cur.hidden) child.value = '';
      }
    }

    function cascadeFrom(parentKey, parentVal) {
      cfFilters.forEach(function (child) {
        if (child.getAttribute('data-cf-parent') === parentKey) {
          filterChildOptions(child, parentVal);
          // Cascade further down the chain (e.g. Program -> Zone -> Branch).
          cascadeFrom(child.getAttribute('data-cf-key'), child.value);
        }
      });
    }

    cfFilters.forEach(function (sel) {
      sel.addEventListener('change', function () {
        cascadeFrom(sel.getAttribute('data-cf-key'), sel.value);
        applySearch();
      });
    });

    // Apply initial cascade state on load (in case a parent has a preset value).
    cfFilters.forEach(function (sel) {
      if (!sel.getAttribute('data-cf-parent')) cascadeFrom(sel.getAttribute('data-cf-key'), sel.value);
    });
  })();

  // Populate BPA branch filter (when BPA module active and select present)
  if (bpaBranchFilter) {
    fetch((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/bpa/index?action=branches_cached', { credentials: 'same-origin' })
      .then((r) => r.ok ? r.json() : null)
      .then((j) => {
        if (!j || !Array.isArray(j.data)) return;
        const current = String(bpaBranchFilter.value || '');
        bpaBranchFilter.innerHTML = '<option value="">All branches</option>';
        j.data.forEach((b) => {
          if (!b || !b.code) return;
          const opt = document.createElement('option');
          opt.value = String(b.code);
          opt.textContent = b.name ? (b.name + ' (' + b.code + ')') : String(b.code);
          bpaBranchFilter.appendChild(opt);
        });
        bpaBranchFilter.value = current;
      })
      .catch(() => {});
  }

  function updateAccountOptions(rows) {
    if (!featureAccounts || !accountFilter) return;
    const selected = accountFilter.value || '';
    const selectedLabel = selected && accountFilter.selectedOptions && accountFilter.selectedOptions.length
      ? accountFilter.selectedOptions[0].textContent
      : '';
    const choices = [{
      value: '',
      label: 'All accounts',
      selected: selected === ''
    }];
    let selectedFound = false;
    (rows || []).forEach((acc) => {
      if (!acc || !acc.id) return;
      const value = String(acc.id);
      if (selected && value === String(selected)) selectedFound = true;
      choices.push({
        value: value,
        label: acc.name || (moduleLabel('account', 'singular') + ' #' + acc.id),
        selected: selected && value === String(selected)
      });
    });
    if (selected && !selectedFound) {
      choices.push({
        value: String(selected),
        label: selectedLabel || (moduleLabel('account', 'singular') + ' #' + selected),
        selected: true
      });
    }
    accountFilter.innerHTML = '';
    choices.forEach((item) => {
      const opt = document.createElement('option');
      opt.value = item.value;
      opt.textContent = item.label;
      opt.selected = !!item.selected;
      accountFilter.appendChild(opt);
    });
  }

  async function loadAccountFilterOptions(query) {
    if (!featureAccounts || !accountFilter) return;
    const reqSeq = ++accountRequestSeq;
    const params = new URLSearchParams();
    params.set('lookup', 'select');
    params.set('limit', '50');
    if (query && query.trim().length > 0) params.set('q', query.trim());
    try {
      const res = await fetch(apiAccounts + '?' + params.toString());
      const data = await res.json().catch(() => ({}));
      if (reqSeq !== accountRequestSeq) return;
      if (!res.ok) return;
      updateAccountOptions(data.data || []);
    } catch (_e) {
      // no-op: keep current options
    }
  }

  function initAccountFilterSelect2() {
    if (!featureAccounts || !accountFilter || !window.jQuery || !window.jQuery.fn || !window.jQuery.fn.select2) return;
    const $account = window.jQuery(accountFilter);
    if ($account.hasClass('select2-hidden-accessible')) {
      $account.select2('destroy');
    }
    $account.select2({
      width: '100%',
      placeholder: 'All accounts',
      allowClear: true,
      ajax: {
        url: apiAccounts,
        dataType: 'json',
        delay: 250,
        data: function(params) {
          return {
            lookup: 'select',
            limit: 50,
            q: (params && params.term) ? params.term : ''
          };
        },
        processResults: function(data) {
          const rows = (data && Array.isArray(data.data)) ? data.data : [];
          return {
            results: rows.map((acc) => ({
              id: String(acc.id || ''),
              text: acc.name || (moduleLabel('account', 'singular') + ' #' + acc.id)
            }))
          };
        }
      }
    });
  }

  if (resetBtn) {
    resetBtn.addEventListener('click', () => {
      if (searchInput) searchInput.value = '';
      if (searchBySelect) searchBySelect.value = 'all';
      if (featureLeads && leadFilter) leadFilter.value = 'contacts';
      if (featureAccounts && accountFilter) accountFilter.value = '';
      if (bpaBranchFilter) bpaBranchFilter.value = '';
      if (quickTableSearch) quickTableSearch.value = '';
      if (datePicker) datePicker.clear();
      if (featureAccounts && window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 && accountFilter) {
        window.jQuery(accountFilter).val('').trigger('change');
      } else if (featureAccounts) {
        loadAccountFilterOptions('');
      }
      applySearch();
    });
  }

  syncLeadModeHeading();

  function formatDate(d) {
    if (!d) return '';
    return d.toISOString().slice(0,10);
  }

  const exportBtn = document.getElementById('contactExportBtn');
  if (exportBtn) {
    exportBtn.addEventListener('click', () => {
      const params = new URLSearchParams();
      const term = searchInput ? searchInput.value.trim() : '';
      const searchBy = searchBySelect ? searchBySelect.value : 'all';
      if (term) params.set('q', term);
      if (term && searchBy && searchBy !== 'all') params.set('search_by', searchBy);
      if (featureLeads && leadFilter && leadFilter.value) params.set('lead_filter', leadFilter.value);
      if (datePicker && datePicker.selectedDates.length) {
        const [start, end] = datePicker.selectedDates;
        const s = formatDate(start);
        const e = formatDate(end || start);
        if (s) params.set('start_date', s);
        if (e) params.set('end_date', e);
      }
      const url = apiExport + (params.toString() ? `?${params.toString()}` : '');
      window.location.href = url;
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

  function loadQuickView(id) {
    if (!id) return;
    const loader = document.getElementById('contactViewLoading');
    const body = document.getElementById('contactViewBody');
    if (loader) loader.classList.remove('d-none');
    if (body) body.classList.add('d-none');
    fetch(apiDetail + '?id=' + encodeURIComponent(id) + '&case_limit=4')
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error(data?.error || 'Unable to load contact');
        if (typeof applyContactQuickView === 'function') {
          applyContactQuickView(data);
        }
        contactOffcanvas.show();
      })
      .catch(err => showPageError(err.message || 'Unable to load contact'))
      .finally(() => {
        if (loader) loader.classList.add('d-none');
        if (body) body.classList.remove('d-none');
      });
  }

  document.addEventListener('click', async (e) => {
    const btn = e.target.closest('.btn-view-contact');
    if (!btn) return;
    const spinner = btn.querySelector('.qv-spinner');
    const icon = btn.querySelector('.qv-icon');
    btn.disabled = true;
    if (spinner) spinner.classList.remove('d-none');
    if (icon) icon.classList.add('d-none');
    try {
      await loadQuickView(btn.getAttribute('data-contact-id'));
    } finally {
      btn.disabled = false;
      if (spinner) spinner.classList.add('d-none');
      if (icon) icon.classList.remove('d-none');
    }
  });

  const btnNewContact = document.getElementById('btnNewContact');
  if (btnNewContact) {
    btnNewContact.addEventListener('click', () => {
      if (!contactForm) return;
      contactForm.openCreate().catch(() => {});
    });
  }

  document.addEventListener('click', async (e) => {
    const editBtn = e.target.closest('.btn-edit-contact');
    if (editBtn && contactForm) {
      await contactForm.openEdit(editBtn.dataset.contactId, editBtn);
    }
  });

  let pendingDeleteId = null;
  const confirmDeleteBtn = document.getElementById('confirmDeleteContact');
  const deleteModalEl = document.getElementById('deleteContactModal');
  const deleteModal = deleteModalEl ? bootstrap.Modal.getOrCreateInstance(deleteModalEl) : null;

  document.addEventListener('click', (e) => {
    const delBtn = e.target.closest('.btn-delete-contact');
    if (delBtn) {
      pendingDeleteId = delBtn.dataset.contactId;
      const deleteAlert = document.getElementById('deleteContactAlert');
      if (deleteAlert) { deleteAlert.classList.add('d-none'); deleteAlert.textContent = ''; }
      deleteModal?.show();
    }
  });

  if (confirmDeleteBtn) {
    confirmDeleteBtn.addEventListener('click', () => {
      if (!pendingDeleteId) return;
      confirmDeleteBtn.disabled = true;
      const spinner = confirmDeleteBtn.querySelector('.spinner-border');
      const label = confirmDeleteBtn.querySelector('.btn-label');
      if (spinner) spinner.classList.remove('d-none');
      if (label) label.classList.add('opacity-50');
      const deleteAlert = document.getElementById('deleteContactAlert');
      fetch(apiDelete + '?id=' + encodeURIComponent(pendingDeleteId), { method: 'DELETE' })
        .then(r => r.json().then(data => ({ ok: r.ok, data })))
        .then(({ ok, data }) => {
          if (!ok) throw new Error(data?.error || 'Unable to delete contact');
          window.location.assign('studio/contacts/');
        })
        .catch(err => {
          if (deleteAlert) {
            deleteAlert.textContent = err.message || 'Unable to delete contact';
            deleteAlert.classList.remove('d-none');
          } else {
            showPageError(err.message || 'Unable to delete contact');
          }
        })
        .finally(() => {
          confirmDeleteBtn.disabled = false;
          if (spinner) spinner.classList.add('d-none');
          if (label) label.classList.remove('opacity-50');
        });
    });
  }

  if (featureAccounts) {
    loadAccountFilterOptions('');
    initAccountFilterSelect2();
  }

})();
