function setupContactForm(options) {
  const opts = options || {};
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
  const apiContactsIndex = apiMap.contactsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/contacts/index');
  const apiContactsDetail = apiMap.contactsDetail || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/contacts/detail');
  const apiAccountsIndex = apiMap.accountsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/accounts/index');
  const apiLoanTypes = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/config/loan_types';
  const pageCfg = window.contactsPageConfig || window.contactViewConfig || {};
  const featureAccounts = pageCfg.featureAccounts !== false;

  const form = document.querySelector(opts.formSelector || '#contactForm');
  const saveBtn = document.querySelector(opts.saveBtn || '#saveContactBtn');
  const alertBox = document.querySelector(opts.alertSelector || '#contactFormAlert');
  const offcanvasEl = document.querySelector(opts.offcanvasSelector || '#contactFormOffcanvas');
  if (!form || !saveBtn || !offcanvasEl) return null;
  const offcanvas = new bootstrap.Offcanvas(offcanvasEl);

  const accountSelect = form.querySelector('#ct_account');
  const newAccountWrap = form.querySelector('#ct_account_new_wrap');
  const newAccountInput = form.querySelector('#ct_account_new');
  const phoneCodeSelect = form.querySelector('#ct_phone_code');
  const phoneInput = form.querySelector('#ct_phone');
  const phoneFullInput = form.querySelector('#ct_phone_full');
  const phoneAltCodeSelect = form.querySelector('#ct_phone_alt_code');
  const phoneAltInput = form.querySelector('#ct_phone_alt');
  const phoneAltFullInput = form.querySelector('#ct_phone_alt_full');
  const countrySelect = form.querySelector('#ct_country');
  const countryNameInput = form.querySelector('#ct_country_name');
  const stateSelect = form.querySelector('#ct_state');
  const stateNameInput = form.querySelector('#ct_state_name');
  const citySelect = form.querySelector('#ct_city');
  const cityNameInput = form.querySelector('#ct_city_name');
  const genderSelect = form.querySelector('#ct_gender');
  const bpaBranchWrap = form.querySelector('#ct_bpa_branch_wrap');
  const bpaBranchSelect = form.querySelector('#ct_bpa_branch_code');
  const bpaBranchStatus = form.querySelector('#ct_bpa_branch_status');
  const loanTypeSelect = form.querySelector('#ct_loan_type');
  const loanTypeStatus = form.querySelector('#ct_loan_type_status');
  const customFieldsWrap = form.querySelector('#ct_custom_fields_wrap');
  const customFieldsContainer = form.querySelector('#ct_custom_fields_container');
  const verificationWrap = document.getElementById('contactIdentityVerificationWrap');
  const verificationTokenInput = document.getElementById('contact_identity_verification_token');
  const verificationConfig = window.contactIdentityVerificationConfig || {};
  let identityVerificationBlock = null;

  function enforcePhoneInputLimit(inputEl, min, max) {
    if (!inputEl) return;
    const minLen = Number(min) > 0 ? Number(min) : 5;
    const maxLen = Number(max) > 0 ? Number(max) : 15;
    inputEl.setAttribute('maxlength', String(maxLen));
    inputEl.setAttribute('minlength', String(minLen));
    inputEl.setAttribute('pattern', '[0-9]{' + minLen + ',' + maxLen + '}');
    inputEl.dataset.minLength = String(minLen);
    inputEl.dataset.maxLength = String(maxLen);
    inputEl.addEventListener('input', function () {
      const cleaned = String(inputEl.value || '').replace(/\D+/g, '').slice(0, maxLen);
      if (inputEl.value !== cleaned) inputEl.value = cleaned;
    });
  }

  enforcePhoneInputLimit(phoneInput, 5, 15);
  enforcePhoneInputLimit(phoneAltInput, 5, 15);
  const phoneHelper = window.PhoneHelper ? window.PhoneHelper.bind({
    countriesUrl: apiMap.countries || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/geo/index?action=countries'),
    codeSelect: phoneCodeSelect,
    localInput: phoneInput,
    fullInput: phoneFullInput,
    countrySelect: null,
    defaultDial: '+234',
    defaultCountry: 'NG'
  }) : null;
  const phoneAltHelper = window.PhoneHelper ? window.PhoneHelper.bind({
    countriesUrl: apiMap.countries || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/geo/index?action=countries'),
    codeSelect: phoneAltCodeSelect,
    localInput: phoneAltInput,
    fullInput: phoneAltFullInput,
    countrySelect: null,
    defaultDial: '+234',
    defaultCountry: 'NG'
  }) : null;
  const geoHelper = window.GeoHelper ? window.GeoHelper.bind({
    countrySelect: countrySelect,
    stateSelect: stateSelect,
    citySelect: citySelect,
    countryNameInput: countryNameInput,
    stateNameInput: stateNameInput,
    cityNameInput: cityNameInput,
    apiCountries: apiMap.geoCountries || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/geo/index?action=countries'),
    apiStates: apiMap.geoStates || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/geo/index?action=states'),
    apiCities: apiMap.geoCities || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/geo/index?action=cities'),
    defaultCountryCode: 'NG',
    onCountryMeta: (meta) => {
      if (!meta || !phoneCodeSelect) return;
      const phonecode = String(meta.phonecode || '').trim();
      if (!phonecode) return;
      const target = '+' + phonecode;
      const ensureDialOption = (selectEl) => {
        if (!selectEl) return;
        if (!Array.from(selectEl.options || []).some((opt) => opt.value === target)) {
          const opt = document.createElement('option');
          opt.value = target;
          opt.textContent = target;
          selectEl.appendChild(opt);
        }
        selectEl.value = target;
        selectEl.dispatchEvent(new Event('change', { bubbles: true }));
      };
      ensureDialOption(phoneCodeSelect);
      ensureDialOption(phoneAltCodeSelect);
    }
  }) : null;

  let currentId = null;
  let accountsLoaded = false;
  let accountRowsCache = [];
  let accountChoices = null;
  let accountSearchTimer = null;
  let accountRequestSeq = 0;
  let bpaBranchesPromise = null;
  let loanTypesPromise = null;
  let customFieldDefs = null;
  let openingRequest = null;
  let pendingVerificationPrefill = null;

  function toggleTriggerBusy(trigger, busy) {
    if (!trigger) return;
    const spinner = trigger.querySelector('.spinner-border');
    const label = trigger.querySelector('.btn-label, .btn-text');
    const icon = trigger.querySelector('.edit-icon');
    if ('disabled' in trigger) {
      trigger.disabled = !!busy;
    } else {
      trigger.classList.toggle('disabled', !!busy);
      if (busy) trigger.setAttribute('aria-disabled', 'true');
      else trigger.removeAttribute('aria-disabled');
    }
    trigger.dataset.busy = busy ? '1' : '0';
    trigger.setAttribute('aria-busy', busy ? 'true' : 'false');
    if (spinner) spinner.classList.toggle('d-none', !busy);
    if (icon) icon.classList.toggle('d-none', !!busy);
    if (label) label.classList.toggle('opacity-50', !!busy);
  }

  function ensureAccountChoices() {
    if (!featureAccounts || !accountSelect || typeof Choices === 'undefined' || accountChoices) return;
    accountChoices = new Choices(accountSelect, {
      searchEnabled: true,
      searchResultLimit: 50,
      shouldSort: false,
      itemSelectText: '',
      allowHTML: false,
      noResultsText: 'No account found',
      noChoicesText: 'No accounts',
      searchPlaceholderValue: 'Type to search account',
      placeholder: false
    });
  }

  function parsePreloadedAccounts() {
    if (!featureAccounts || !accountSelect) return [];
    const rows = [];
    Array.from(accountSelect.options || []).forEach((opt) => {
      const val = String(opt.value || '');
      if (!val || val === '__new__') return;
      rows.push({ id: val, name: (opt.textContent || '').trim() || (moduleLabel('account', 'singular') + ' #' + val) });
    });
    return rows;
  }

  function renderAccountChoices(selectedValue) {
    if (!featureAccounts || !accountSelect) return;
    const selected = (selectedValue === null || selectedValue === undefined)
      ? String(accountSelect.value || '')
      : String(selectedValue);
    const rows = [];
    const seen = new Set();
    const addChoice = (value, label, isSelected) => {
      const val = String(value || '');
      if (seen.has(val)) return;
      seen.add(val);
      rows.push({ value: val, label: label, selected: !!isSelected });
    };
    addChoice('', 'Select ' + moduleLabel('account', 'singular', 'lower') + ' (optional)', selected === '');
    addChoice('__new__', '+ Create new ' + moduleLabel('account', 'singular', 'lower'), selected === '__new__');
    (accountRowsCache || []).forEach((acc) => {
      const val = String(acc.id || '');
      if (!val) return;
      addChoice(val, acc.name || (moduleLabel('account', 'singular') + ' #' + val), selected === val);
    });
    ensureAccountChoices();
    if (accountChoices) {
      const seen = new Set();
      const normalized = [];
      rows.forEach((r) => {
        const val = String((r && r.value) || '');
        const key = val === '' ? '__empty__' : val;
        if (seen.has(key)) return;
        seen.add(key);
        normalized.push(r);
      });
      if (typeof accountChoices.clearStore === 'function') {
        accountChoices.clearStore();
      } else if (typeof accountChoices.clearChoices === 'function') {
        accountChoices.clearChoices();
      }
      accountChoices.setChoices(normalized, 'value', 'label', true);
    } else {
      accountSelect.innerHTML = '';
      rows.forEach((item) => {
        const opt = document.createElement('option');
        opt.value = item.value;
        opt.textContent = item.label;
        opt.selected = !!item.selected;
        accountSelect.appendChild(opt);
      });
    }
  }

  function showAlert(msg) {
    if (!alertBox) return;
    alertBox.textContent = msg;
    alertBox.classList.remove('d-none');
  }

  function clearAlert() {
    if (!alertBox) return;
    alertBox.textContent = '';
    alertBox.classList.add('d-none');
  }

  function escapeHtml(val) {
    return String(val ?? '').replace(/[&<>"']/g, (ch) => ({
      '&': '&amp;',
      '<': '&lt;',
      '>': '&gt;',
      '"': '&quot;',
      "'": '&#39;'
    }[ch]));
  }

  function setTitle(text) {
    const titleEl = offcanvasEl.querySelector('#contactFormOffcanvasLabel');
    if (titleEl) titleEl.textContent = text;
  }

  function setFieldValue(selector, value) {
    const el = form.querySelector(selector);
    if (el) el.value = value === null || value === undefined ? '' : String(value);
  }

  function resetForm(defaults) {
    form.reset();
    clearAlert();
    pendingVerificationPrefill = null;
    if (verificationTokenInput) verificationTokenInput.value = '';
    if (identityVerificationBlock) identityVerificationBlock.reset();
    if (accountSelect) {
      if (accountChoices) {
        accountChoices.removeActiveItems();
        accountChoices.setChoiceByValue('');
      } else {
        accountSelect.value = '';
      }
    }
    if (newAccountWrap) newAccountWrap.classList.add('d-none');
    if (newAccountInput) newAccountInput.value = '';
    if (phoneHelper && phoneFullInput) phoneFullInput.value = '';
    if (phoneAltHelper && phoneAltFullInput) phoneAltFullInput.value = '';
    else if (phoneCodeSelect && !phoneCodeSelect.value) phoneCodeSelect.value = '+234';
    if (phoneAltCodeSelect && !phoneAltCodeSelect.value) phoneAltCodeSelect.value = '+234';
    if (countrySelect) countrySelect.value = '';
    if (stateSelect) stateSelect.value = '';
    if (citySelect) citySelect.value = '';
    if (countryNameInput) countryNameInput.value = '';
    if (stateNameInput) stateNameInput.value = '';
    if (cityNameInput) cityNameInput.value = '';
    if (genderSelect) genderSelect.value = '';
    const _vd = document.querySelector('#contactIdentityVerificationBlock [data-role="verified-detail"]');
    if (_vd) { _vd.textContent = ''; _vd.classList.add('d-none'); }
    if (bpaBranchStatus) bpaBranchStatus.textContent = 'Loading BPA branches...';
    setBpaBranchValue('');
    if (customFieldsContainer) {
      customFieldsContainer.innerHTML = '<div class="text-muted small">Loading custom fields...</div>';
      if (customFieldsWrap) customFieldsWrap.classList.add('d-none');
    }
    if (defaults) {
      if (defaults.phone && phoneHelper) phoneHelper.setFull(defaults.phone);
      if (defaults.phone_alt && phoneAltHelper) phoneAltHelper.setFull(defaults.phone_alt);
      else if (defaults.phone && phoneInput) phoneInput.value = defaults.phone;
      else if (defaults.phone_alt && phoneAltInput) phoneAltInput.value = defaults.phone_alt;
      if (defaults.first_name) setFieldValue('#ct_first', defaults.first_name);
      if (defaults.middle_name) setFieldValue('#ct_middle', defaults.middle_name);
      if (defaults.last_name) setFieldValue('#ct_last', defaults.last_name);
      if (defaults.email) setFieldValue('#ct_email', defaults.email);
      if (defaults.tin_number) setFieldValue('#ct_tin', defaults.tin_number);
      if (defaults.nin_number) setFieldValue('#ct_nin', defaults.nin_number);
      if (defaults.bvn_number) setFieldValue('#ct_bvn', defaults.bvn_number);
      if (defaults.address_line1) setFieldValue('#ct_address', defaults.address_line1);
      if (defaults.city && cityNameInput) cityNameInput.value = defaults.city;
      if (defaults.state && stateNameInput) stateNameInput.value = defaults.state;
      if (defaults.country && countryNameInput) countryNameInput.value = defaults.country;
      if (defaults.gender && genderSelect) genderSelect.value = defaults.gender;
      renderCustomFields(customFieldDefs, defaults.custom_fields || {});
      applyBpaBranchDefaults(defaults.custom_fields || {});
    } else {
      renderCustomFields(customFieldDefs, {});
    }
    renderAccountChoices('');
  }

  async function loadAccounts(query) {
    if (!featureAccounts || !accountSelect) return;
    const term = String(query || '').trim();
    if (accountSelect.dataset.preloaded === '1' && accountSelect.options.length > 1 && term === '') {
      if (!accountRowsCache.length) {
        accountRowsCache = parsePreloadedAccounts();
      }
      renderAccountChoices(accountSelect.value || '');
      accountsLoaded = true;
      return;
    }
    if (accountsLoaded && term === '') {
      renderAccountChoices(accountSelect.value || '');
      return;
    }
    try {
      const reqSeq = ++accountRequestSeq;
      const params = new URLSearchParams();
      params.set('lookup', 'select');
      params.set('limit', '50');
      if (term !== '') params.set('q', term);
      const res = await fetch(apiAccountsIndex + '?' + params.toString());
      const data = await res.json();
      if (reqSeq !== accountRequestSeq) return;
      const list = Array.isArray(data?.data) ? data.data : (Array.isArray(data) ? data : []);
      accountRowsCache = list.map((acc) => ({
        id: acc.id,
        name: acc.name || (moduleLabel('account', 'singular') + ' #' + acc.id)
      }));
      renderAccountChoices(accountSelect.value || '');
      if (term === '') accountsLoaded = true;
    } catch (err) {
      // leave select as-is
    }
  }

  async function loadPhoneData() {
    const waits = [];
    if (phoneHelper && phoneHelper.ready) waits.push(phoneHelper.ready);
    if (phoneAltHelper && phoneAltHelper.ready) waits.push(phoneAltHelper.ready);
    if (waits.length) await Promise.all(waits);
  }

  function renderCustomFields(defs, values) {
    if (!customFieldsContainer || typeof CustomFieldRenderer === 'undefined') return;
    CustomFieldRenderer.render(customFieldsContainer, defs, values, {
      wrapEl: customFieldsWrap,
      idPrefix: 'cf_',
      labelClass: 'form-label small text-muted',
      inputClass: 'form-control',
      selectClass: 'form-select',
      textareaRows: 3,
      emptyHtml: '<div class="text-muted small">No custom fields defined.</div>',
      hideWhenEmpty: true
    });
  }

  function setBpaBranchValue(value) {
    if (!bpaBranchSelect) return;
    const next = value === null || value === undefined ? '' : String(value).trim();
    if (window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 && window.jQuery(bpaBranchSelect).hasClass('select2-hidden-accessible')) {
      window.jQuery(bpaBranchSelect).val(next).trigger('change.select2');
    } else {
      bpaBranchSelect.value = next;
    }
  }

  function getBpaBranchValue() {
    if (!bpaBranchSelect) return '';
    return String(bpaBranchSelect.value || '').trim();
  }

  function initBpaBranchSelect2() {
    if (!(window.jQuery && window.jQuery.fn && window.jQuery.fn.select2 && bpaBranchSelect)) return;
    const $el = window.jQuery(bpaBranchSelect);
    if ($el.hasClass('select2-hidden-accessible')) {
      $el.select2('destroy');
    }
    $el.select2({
      width: '100%',
      allowClear: true,
      placeholder: 'Select branch',
      dropdownParent: offcanvasEl ? window.jQuery(offcanvasEl) : undefined
    });
  }

  function applyBpaBranchDefaults(values) {
    const branchCode = String(
      (values && values.branch_code) ||
      (values && values.bpa && values.bpa.branch_code) ||
      ''
    ).trim();
    if (branchCode) {
      setBpaBranchValue(branchCode);
    }
  }

  function setLoanTypeValue(value) {
    if (!loanTypeSelect) return;
    loanTypeSelect.value = value === null || value === undefined ? '' : String(value).trim();
  }

  function getLoanTypeValue() {
    if (!loanTypeSelect) return '';
    return String(loanTypeSelect.value || '').trim();
  }

  async function loadLoanTypes() {
    if (!loanTypeSelect) return [];
    if (loanTypesPromise) return loanTypesPromise;
    loanTypesPromise = (async () => {
      try {
        const res = await fetch(apiLoanTypes);
        const data = await res.json().catch(() => ({}));
        const rows = Array.isArray(data?.data) ? data.data : [];
        const loanTypes = rows.map((row) => {
          const code = String(row?.code || '').trim();
          const label = String(row?.label || code).trim();
          return code ? { code, label } : null;
        }).filter(Boolean);
        if (!loanTypes.length) {
          if (loanTypeStatus) loanTypeStatus.textContent = 'No loan types were returned.';
          return [];
        }
        loanTypeSelect.innerHTML = '<option value="">Select loan type</option>' + loanTypes.map((item) => {
          const text = item.label === item.code ? item.label : (item.label + ' (' + item.code + ')');
          return `<option value="${escapeHtml(item.code)}">${escapeHtml(text)}</option>`;
        }).join('');
        if (loanTypeStatus) loanTypeStatus.textContent = 'Choose the loan type for this contact.';
        return loanTypes;
      } catch (err) {
        if (loanTypeStatus) loanTypeStatus.textContent = 'Loan types unavailable.';
        return [];
      }
    })();
    return loanTypesPromise;
  }

  async function loadBpaBranches() {
    if (!bpaBranchSelect) return [];
    if (bpaBranchesPromise) return bpaBranchesPromise;
    bpaBranchesPromise = (async () => {
      try {
        const res = await fetch((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/bpa/index?action=branches_cached');
        const data = await res.json().catch(() => ({}));
        const rows = Array.isArray(data?.data) ? data.data : (Array.isArray(data) ? data : []);
        const branches = rows.map((row) => {
          const code = String(row?.branch_code || row?.code || row?.id || '').trim();
          const label = String(row?.name || row?.branch_name || row?.branch || code).trim();
          return code ? { code, label } : null;
        }).filter(Boolean);
        if (!branches.length) {
          if (bpaBranchStatus) bpaBranchStatus.textContent = 'No BPA branches were returned.';
          return [];
        }
        bpaBranchSelect.innerHTML = '<option value="">Select branch</option>' + branches.map((branch) => {
        const text = branch.code === branch.label ? branch.label : (branch.label + ' (' + branch.code + ')');
        return `<option value="${escapeHtml(branch.code)}">${escapeHtml(text)}</option>`;
      }).join('');
        if (bpaBranchWrap) bpaBranchWrap.classList.remove('d-none');
        if (bpaBranchStatus) bpaBranchStatus.textContent = 'Choose the BPA branch for this contact.';
        initBpaBranchSelect2();
        return branches;
      } catch (err) {
        if (bpaBranchStatus) bpaBranchStatus.textContent = 'BPA branches unavailable.';
        return [];
      }
    })();
    return bpaBranchesPromise;
  }

  async function loadCustomFieldDefs() {
    if (customFieldDefs !== null) return customFieldDefs;
    try {
      const res = await fetch(apiContactsIndex + '?with_defs=1&defs_only=1');
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

  async function openCreate(defaults, trigger) {
    if (openingRequest) return openingRequest;
    currentId = null;
    setTitle('New ' + moduleLabel('contact', 'singular'));
    resetForm(defaults);
    if (verificationWrap) verificationWrap.classList.toggle('d-none', !(verificationConfig && verificationConfig.enabled));
    offcanvas.show();
    openingRequest = (async () => {
      toggleTriggerBusy(trigger, true);
      try {
        await Promise.all([loadAccounts(), loadPhoneData(), loadCustomFieldDefs(), loadBpaBranches(), loadLoanTypes()]);
        if (geoHelper) {
          await geoHelper.init({
            country: (defaults && defaults.country) ? defaults.country : 'Nigeria',
            state: (defaults && defaults.state) ? defaults.state : '',
            city: (defaults && defaults.city) ? defaults.city : ''
          });
        }
        // Ensure phone selects are initialized and keep inbound/default phone when provided.
        if (phoneHelper) {
          if (defaults && defaults.phone) {
            phoneHelper.setFull(defaults.phone);
          } else if (!phoneFullInput || !phoneFullInput.value) {
            phoneHelper.setFull('');
          }
        }
        if (phoneAltHelper) {
          if (defaults && defaults.phone_alt) {
            phoneAltHelper.setFull(defaults.phone_alt);
          } else if (!phoneAltFullInput || !phoneAltFullInput.value) {
            phoneAltHelper.setFull('');
          }
        } else if (phoneCodeSelect && !phoneCodeSelect.value) {
          phoneCodeSelect.value = '+234';
        }
        renderCustomFields(customFieldDefs, (defaults && defaults.custom_fields) ? defaults.custom_fields : {});
        applyBpaBranchDefaults((defaults && defaults.custom_fields) ? defaults.custom_fields : {});
        setLoanTypeValue(
          (defaults && defaults.custom_fields && defaults.custom_fields.loan_type)
            ? defaults.custom_fields.loan_type
            : ((defaults && defaults.loan_type) || '')
        );
        if (pendingVerificationPrefill) {
          applyVerificationPrefill(pendingVerificationPrefill);
        }
      } catch (_e) {
        // Keep form usable even if supporting data fails to load.
        if (customFieldsContainer && customFieldsContainer.innerHTML.trim() === '') {
          customFieldsContainer.innerHTML = '<div class="text-muted small">Custom fields unavailable.</div>';
        }
      } finally {
        toggleTriggerBusy(trigger, false);
        openingRequest = null;
      }
    })();
    return openingRequest;
  }

  function fillForm(data) {
    setFieldValue('#contact_id', data.id || '');
    setFieldValue('#ct_first', data.first_name || '');
    setFieldValue('#ct_middle', data.middle_name || '');
    setFieldValue('#ct_last', data.last_name || '');
    if (phoneHelper) {
      phoneHelper.setFull(data.phone || '');
    } else if (phoneInput) {
      phoneInput.value = (data.phone || '').replace(/\D+/g, '');
    }
    if (phoneAltHelper) {
      phoneAltHelper.setFull(data.phone_alt || '');
    } else if (phoneAltInput) {
      phoneAltInput.value = (data.phone_alt || '').replace(/\D+/g, '');
    }
    setFieldValue('#ct_email', data.email || '');
    setFieldValue('#ct_tin', data.tin_number || '');
    setFieldValue('#ct_nin', data.nin_number || '');
    setFieldValue('#ct_bvn', data.bvn_number || '');
    if (genderSelect) genderSelect.value = data.gender || '';
    setFieldValue('#ct_address', data.address_line1 || '');
    if (countryNameInput) countryNameInput.value = data.country || '';
    if (stateNameInput) stateNameInput.value = data.state || '';
    if (cityNameInput) cityNameInput.value = data.city || '';
    if (featureAccounts && accountSelect && data.account_id) {
      const selectedId = String(data.account_id);
      if (accountChoices) {
        accountChoices.removeActiveItems();
        accountChoices.setChoiceByValue(selectedId);
      } else {
        accountSelect.value = selectedId;
      }
    } else if (featureAccounts && accountSelect) {
      if (accountChoices) {
        accountChoices.removeActiveItems();
        accountChoices.setChoiceByValue('');
      } else {
        accountSelect.value = '';
      }
    }
  }

  function applyVerificationPrefill(prefill) {
    if (!prefill || typeof prefill !== 'object') return;
    pendingVerificationPrefill = Object.assign({}, prefill);
    if (form.querySelector('#ct_first')) form.querySelector('#ct_first').value = prefill.first_name || '';
    if (form.querySelector('#ct_middle')) form.querySelector('#ct_middle').value = prefill.middle_name || '';
    if (form.querySelector('#ct_last')) form.querySelector('#ct_last').value = prefill.last_name || '';
    if (phoneHelper) {
      phoneHelper.setFull(prefill.phone || '');
    } else if (phoneInput) {
      phoneInput.value = (prefill.phone || '').replace(/\D+/g, '');
    }
    if (form.querySelector('#ct_email')) form.querySelector('#ct_email').value = prefill.email || '';
    if (form.querySelector('#ct_tin')) form.querySelector('#ct_tin').value = prefill.tin_number || '';
    if (form.querySelector('#ct_nin')) form.querySelector('#ct_nin').value = prefill.nin_number || '';
    if (form.querySelector('#ct_bvn')) form.querySelector('#ct_bvn').value = prefill.bvn_number || '';
    if (genderSelect) genderSelect.value = prefill.gender || '';
    if (form.querySelector('#ct_address')) form.querySelector('#ct_address').value = prefill.address_line1 || '';
    if (countryNameInput) countryNameInput.value = prefill.country || '';
    if (stateNameInput) stateNameInput.value = prefill.state || '';
    if (cityNameInput) cityNameInput.value = prefill.city || '';
    applyBpaBranchDefaults(prefill.custom_fields || {});
    if (geoHelper) {
      geoHelper.init({
        country: prefill.country || 'Nigeria',
        state: prefill.state || '',
        city: prefill.city || ''
      }).catch(function () {
        // Keep the already assigned values even if geo data fails to refresh.
      });
    }
    const verifiedDetail = document.querySelector('#contactIdentityVerificationBlock [data-role="verified-detail"]');
    if (verifiedDetail) {
      if (prefill.birthdate) {
        verifiedDetail.textContent = 'Date of Birth: ' + prefill.birthdate;
        verifiedDetail.classList.remove('d-none');
      } else {
        verifiedDetail.textContent = '';
        verifiedDetail.classList.add('d-none');
      }
    }
  }

  async function openEdit(id, trigger) {
    if (!id) return;
    if (openingRequest) return openingRequest;
    currentId = id;
    clearAlert();
    setTitle('Edit ' + moduleLabel('contact', 'singular'));
    resetForm();
    if (verificationWrap) verificationWrap.classList.add('d-none');
    offcanvas.show();
    openingRequest = (async () => {
      toggleTriggerBusy(trigger, true);
      try {
        await Promise.allSettled([loadAccounts(), loadPhoneData(), loadCustomFieldDefs(), loadBpaBranches(), loadLoanTypes()]);
        const res = await fetch(apiContactsDetail + '?id=' + encodeURIComponent(id));
        const data = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(data?.error || 'Unable to load contact');
        if (geoHelper) {
          await geoHelper.init({
            country: data.country || '',
            state: data.state || '',
            city: data.city || ''
          }).catch(() => {});
        }
        fillForm(data);
        renderCustomFields(customFieldDefs || [], data.custom_fields || {});
        applyBpaBranchDefaults(data.custom_fields || {});
        setLoanTypeValue(
          (data.custom_fields && data.custom_fields.loan_type)
            ? data.custom_fields.loan_type
            : (data.loan_type || '')
        );
      } catch (err) {
        showAlert(err.message || 'Unable to load contact');
      } finally {
        toggleTriggerBusy(trigger, false);
        openingRequest = null;
      }
    })();
    return openingRequest;
  }

  function buildContactPayload(opts) {
    const cfg = Object.assign({
      first: '#ct_first',
      middle: '#ct_middle',
      last: '#ct_last',
      phone: '#ct_phone',
      phoneAlt: '#ct_phone_alt',
      email: '#ct_email',
      gender: genderSelect ? () => genderSelect.value : () => '',
      tin: '#ct_tin',
      nin: '#ct_nin',
      bvn: '#ct_bvn',
      address: '#ct_address',
      city: cityNameInput ? () => cityNameInput.value : '#ct_city',
      state: stateNameInput ? () => stateNameInput.value : () => '',
      country: countryNameInput ? () => countryNameInput.value : (countrySelect ? () => countrySelect.value : () => ''),
      branchCode: '#ct_bpa_branch_code',
      loanType: '#ct_loan_type'
    }, opts || {});
    const val = (selOrFn) => typeof selOrFn === 'function'
      ? (selOrFn() || '').toString().trim()
      : (document.querySelector(selOrFn)?.value?.trim() || '');
    const phoneHidden = phoneFullInput ? phoneFullInput.value.trim() : '';
    const phoneRaw = val(cfg.phone).replace(/\D+/g, '');
    const phoneAltHidden = phoneAltFullInput ? phoneAltFullInput.value.trim() : '';
    const phoneAltRaw = val(cfg.phoneAlt).replace(/\D+/g, '');
    let fullPhone = phoneHidden || (phoneHelper ? phoneHelper.getFull() : phoneRaw);
    let fullPhoneAlt = phoneAltHidden || (phoneAltHelper ? phoneAltHelper.getFull() : phoneAltRaw);
    return {
      first_name: val(cfg.first),
      middle_name: val(cfg.middle),
      last_name: val(cfg.last),
      phone: fullPhone || null,
      phone_alt: fullPhoneAlt || null,
      email: val(cfg.email),
      gender: val(cfg.gender),
      tin_number: val(cfg.tin),
      nin_number: val(cfg.nin),
      bvn_number: val(cfg.bvn),
      address_line1: val(cfg.address),
      city: val(cfg.city),
      state: val(cfg.state),
      country: val(cfg.country),
      is_lead: opts && typeof opts.is_lead !== 'undefined' ? (opts.is_lead ? 1 : 0) : 0,
      branch_code: val(cfg.branchCode),
      loan_type: val(cfg.loanType)
    };
  }

  // expose helper for other modules
  window.buildContactPayload = buildContactPayload;

  function getPayload() {
    const payload = buildContactPayload({ is_lead: 0 });
    const selectedAcc = featureAccounts && accountSelect ? accountSelect.value : '';
    const newAccName = newAccountInput ? newAccountInput.value.trim() : '';
    if (selectedAcc === '__new__' && newAccName) {
      payload.account_name = newAccName;
    } else if (selectedAcc) {
      payload.account_id = parseInt(selectedAcc, 10);
    }
    const cf = collectCustomFields();
    if (cf) payload.custom_fields = cf;
    // Branch is now selected via the Program -> Zone -> Branch custom-field cascade.
    // Bridge the cascade 'branch' value into custom_fields.bpa.branch_code so BPA
    // sync, reports and filters (which read $.bpa.branch_code) keep working unchanged.
    // Fall back to the legacy hardcoded BPA dropdown if it's still present.
    const cascadeBranch = (payload.custom_fields && typeof payload.custom_fields === 'object')
      ? String(payload.custom_fields.branch || '').trim()
      : '';
    const branchCode = cascadeBranch || getBpaBranchValue();
    if (branchCode) {
      payload.branch_code = branchCode;
      if (!payload.custom_fields || typeof payload.custom_fields !== 'object') {
        payload.custom_fields = {};
      }
      if (!payload.custom_fields.bpa || typeof payload.custom_fields.bpa !== 'object') {
        payload.custom_fields.bpa = {};
      }
      payload.custom_fields.bpa.branch_code = branchCode;
    }
    const loanType = getLoanTypeValue();
    if (loanType) {
      if (!payload.custom_fields || typeof payload.custom_fields !== 'object') {
        payload.custom_fields = {};
      }
      payload.custom_fields.loan_type = loanType;
    }
    if (verificationTokenInput && verificationTokenInput.value) {
      payload.identity_verification_token = verificationTokenInput.value;
    }
    return payload;
  }

  async function save() {
    clearAlert();
    const firstVal = form.querySelector('#ct_first')?.value?.trim() || '';
    const phoneVal = (phoneHelper ? phoneHelper.getFull() : (phoneFullInput ? phoneFullInput.value : (phoneInput ? phoneInput.value : ''))).trim();
    const emailVal = form.querySelector('#ct_email')?.value?.trim() || '';
    const genderVal = genderSelect ? genderSelect.value : '';
    if (!firstVal || !phoneVal || !genderVal) {
      showAlert('Please provide first name, phone, and gender.');
      return;
    }
    saveBtn.disabled = true;
    const originalHtml = saveBtn.innerHTML;
    const spinner = saveBtn.querySelector('.spinner-border');
    const textEl = saveBtn.querySelector('.btn-text');
    if (spinner) spinner.classList.remove('d-none');
    if (textEl) textEl.classList.add('opacity-50');
    const isNew = !currentId;
    const payload = getPayload();
    try {
      const method = currentId ? 'PATCH' : 'POST';
      const url = currentId ? (apiContactsDetail + '?id=' + encodeURIComponent(currentId)) : apiContactsIndex;
      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data?.error || 'Unable to save contact');
      const enriched = Object.assign({}, data || {}, { id: data?.id || currentId });
      if (typeof opts.onSaved === 'function') opts.onSaved(enriched, isNew);
      offcanvas.hide();
    } catch (err) {
      showAlert(err.message || 'Unable to save contact');
    } finally {
      saveBtn.disabled = false;
      if (spinner) spinner.classList.add('d-none');
      if (textEl) textEl.classList.remove('opacity-50');
      else saveBtn.innerHTML = originalHtml;
    }
  }

  if (verificationWrap) {
    verificationWrap.classList.toggle('d-none', !(verificationConfig && verificationConfig.enabled));
  }
  if (window.IdentityVerification && verificationWrap && verificationConfig && verificationConfig.enabled) {
    identityVerificationBlock = window.IdentityVerification.bindBlock({
      root: document.getElementById('contactIdentityVerificationBlock'),
      mode: 'agent',
      endpoint: verificationConfig.endpoint || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/identity_verification'),
      sectionType: 'personal',
      entityMode: 'personal',
      availableMethods: Array.isArray(verificationConfig.personalMethods) ? verificationConfig.personalMethods : null,
      onPrefill: function (prefill) {
        applyVerificationPrefill(prefill || {});
        if (verificationTokenInput) {
          verificationTokenInput.value = identityVerificationBlock ? identityVerificationBlock.getToken() : '';
        }
      },
      onReset: function () {
        if (verificationTokenInput) verificationTokenInput.value = '';
      }
    });
  }

  if (featureAccounts && accountSelect) {
    accountSelect.addEventListener('change', () => {
      if (accountSelect.value === '__new__') {
        newAccountWrap.classList.remove('d-none');
      } else {
        newAccountWrap.classList.add('d-none');
        if (newAccountInput) newAccountInput.value = '';
      }
    });
    accountSelect.addEventListener('search', (ev) => {
      const term = ev && ev.detail && typeof ev.detail.value === 'string' ? ev.detail.value : '';
      clearTimeout(accountSearchTimer);
      accountSearchTimer = setTimeout(() => {
        loadAccounts(term);
      }, 250);
    });
  }

  saveBtn.addEventListener('click', save);

  return { openCreate, openEdit, resetForm };
}
