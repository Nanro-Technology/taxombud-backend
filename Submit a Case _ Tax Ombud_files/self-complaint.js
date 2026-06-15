(function () {
  const cfg = window.selfComplaintConfig || {};
  const contactDefs = cfg.contactDefs || [];
  const organizationDefs = cfg.organizationDefs || [];
  const caseDefs = cfg.caseDefs || [];
  const countriesJson = cfg.countriesUrl || 'api/public/geo?action=countries';
  const statesJson = cfg.statesUrl || 'api/public/geo?action=states';
  const organizationsEnabled = cfg.organizationsEnabled !== false;
  const identityVerificationPublicEnabled = cfg.identityVerificationPublicEnabled === true;
  const identityVerificationContactsEnabled = cfg.identityVerificationContactsEnabled !== false;
  const identityVerificationOrganizationsEnabled = cfg.identityVerificationOrganizationsEnabled !== false;
  const identityVerificationEnabled = identityVerificationPublicEnabled && (identityVerificationContactsEnabled || identityVerificationOrganizationsEnabled);
  const showModeStep = identityVerificationEnabled || organizationsEnabled;
  const verificationApi = cfg.verificationApi || 'api/public/identity-verification';
  const forcedMode = String(cfg.forcedMode || '').toLowerCase();
  let skipOrganizationStep = !!cfg.skipOrganizationStep;
  const complaintTypeSettings = cfg.complaintTypeSettings || { enable_sub_types: 0 };
  const serviceDomainSettings = cfg.serviceDomainSettings || { enable_sub_domains: 1 };
  const complaintTypeMap = (Array.isArray(cfg.complaintTypes) ? cfg.complaintTypes : []).reduce((acc, item) => {
    const code = String(item?.code || item?.label || '').trim();
    if (!code) return acc;
    acc[code] = {
      label: item?.label || code,
      sub_types: Array.isArray(item?.sub_types) ? item.sub_types : []
    };
    return acc;
  }, {});
  const serviceDomainRoutes = (cfg.serviceDomainRoutes && typeof cfg.serviceDomainRoutes === 'object')
    ? cfg.serviceDomainRoutes : {};
  const serviceCategorySource = Array.isArray(cfg.serviceCategories) ? cfg.serviceCategories : [];

  const form = document.getElementById('publicCaseForm');
  if (!form) return;

  const alertBox = document.getElementById('publicAlert');
  const successBox = document.getElementById('publicSuccess');
  const btnPrev = document.getElementById('btnPrev');
  const btnNext = document.getElementById('btnNext');
  const btnSubmit = document.getElementById('btnSubmit');
  const spinner = btnSubmit ? btnSubmit.querySelector('.spinner-border') : null;
  const submitLabel = btnSubmit ? btnSubmit.querySelector('.btn-label') : null;
  const submitDefaultLabel = submitLabel ? submitLabel.textContent : '';
  const phoneInput = document.getElementById('public_phone');
  const phoneCode = document.getElementById('public_phone_code');
  const phoneHidden = document.getElementById('public_phone_full');
  const organizationPhoneInput = document.getElementById('organization_phone_local');
  const organizationPhoneCode = document.getElementById('organization_phone_code');
  const organizationPhoneHidden = document.getElementById('organization_phone_full');
  const organizationFields = document.getElementById('organization_fields');
  const organizationInputs = Array.from(document.querySelectorAll('[data-organization-field]'));
  const caseForValue = document.getElementById('case_for_value');
  const attachmentsInput = document.getElementById('case_attachments');
  const attachmentsList = document.getElementById('case_attachments_list');
  const contactCustomWrap = document.getElementById('contact_custom_fields_wrap');
  const contactCustomContainer = document.getElementById('contact_custom_fields');
  const organizationCustomWrap = document.getElementById('organization_custom_fields_wrap');
  const organizationCustomContainer = document.getElementById('organization_custom_fields');
  const caseCustomWrap = document.getElementById('case_custom_fields_wrap');
  const caseCustomContainer = document.getElementById('case_custom_fields');
  const complaintTypeSelect = form.querySelector('select[name="complaint_type"]');
  const subComplaintTypeWrap = document.getElementById('public_sub_complaint_type_wrap');
  const subComplaintTypeSelect = document.getElementById('public_sub_complaint_type');
  const domainSelect = form.querySelector('select[name="domain_id"]');
  const subDomainSelect = form.querySelector('select[name="sub_domain_id"]');
  const subDomainWrap = document.getElementById('public_sub_domain_wrap');
  // Snapshot all domain options once so we can re-filter without re-fetching
  const domainSourceList = serviceCategorySource.length
    ? serviceCategorySource.map((item) => ({
        id: String(item?.id || ''),
        name: String(item?.name || '').trim(),
        sub_domains: Array.isArray(item?.sub_domains) ? item.sub_domains : []
      })).filter(item => item.id !== '' && item.name !== '')
    : (domainSelect
        ? Array.from(domainSelect.options).filter(o => o.value !== '').map(o => ({ id: String(o.value), name: o.textContent, sub_domains: [] }))
        : []);
  const requiredInputs = Array.from(form.querySelectorAll('[required]'));
  requiredInputs.forEach(input => { input.dataset.wasRequired = '1'; });
  const contactCountry = form.querySelector('select[name="country"]');
  const contactState = form.querySelector('select[name="state"]');
  const organizationCountry = form.querySelector('select[name="organization_country"]');
  const organizationState = form.querySelector('select[name="organization_state"]');
  const organizationSameAddress = document.getElementById('organization_same_address');
  const contactAddress = form.querySelector('input[name="address_line1"]');
  const organizationAddress = form.querySelector('input[name="organization_address_line1"]');
  const headingEl = document.getElementById('publicCaseHeading');
  const subheadingEl = document.getElementById('publicCaseSubheading');
  const modeNoticeEl = document.getElementById('publicCaseModeNotice');
  const stepIndicator2 = document.getElementById('step_indicator_2');
  const stepIndicator3 = document.querySelector('#step_indicator_3 .step-dot');
  const organizationModeIndividual = document.getElementById('organization_mode_individual');
  const organizationModeLabelIndividual = document.querySelector('label[for="organization_mode_individual"]');
  const organizationModeOrganization = document.getElementById('organization_mode_organization');
  const organizationModeLabelOrganization = document.querySelector('label[for="organization_mode_organization"]');
  const publicPersonalVerificationBlock = document.getElementById('public_personal_verification_block');
  const publicCorporateVerificationBlock = document.getElementById('public_corporate_verification_block');
  const publicCorporateVerificationCol = document.getElementById('public_corporate_verification_col');
  const publicIdentityFormFields = document.getElementById('public_identity_form_fields');
  const personalTokenInput = document.getElementById('identity_personal_token');
  const corporateTokenInput = document.getElementById('identity_corporate_token');
  const personalFields = Array.from(form.querySelectorAll('[name="first_name"], [name="last_name"], [name="email"], [name="phone_local"], [name="phone"], [name="gender"], [name="tin_number"], [name="nin_number"], [name="bvn_number"], [name="address_line1"], [name="country"], [name="state"]'));
  const organizationIdentityFields = Array.from(form.querySelectorAll('[name="organization_name"], [name="organization_phone_local"], [name="organization_phone"], [name="organization_email"], [name="organization_website"], [name="organization_cac_number"], [name="organization_tin"], [name="organization_address_line1"], [name="organization_country"], [name="organization_state"]'));
  let personalVerification = null;
  let corporateVerification = null;

  let step = showModeStep ? 0 : 1;
  let quillEditor = null;

  function getComplaintMode() {
    const mode = form.querySelector('input[name="organization_mode"]:checked')?.value || 'individual';
    return mode === 'organization' ? 'organization' : 'personal';
  }

  function personalVerificationRequired() {
    return identityVerificationEnabled && identityVerificationContactsEnabled;
  }

  function corporateVerificationRequired() {
    return identityVerificationEnabled
      && getComplaintMode() === 'organization'
      && identityVerificationOrganizationsEnabled;
  }

  function setFieldLock(fields, locked) {
    fields.forEach((field) => {
      if (!field) return;
      if (field.tagName === 'SELECT') {
        field.disabled = !!locked;
      } else {
        field.readOnly = !!locked;
        field.classList.toggle('bg-light', !!locked);
      }
    });
  }

  function applyPersonalPrefill(prefill) {
    const _set = (name, val) => { const el = form.querySelector('[name="' + name + '"]'); if (el) el.value = val || ''; };
    _set('first_name', prefill.first_name);
    _set('last_name', prefill.last_name);
    _set('email', prefill.email);
    _set('tin_number', prefill.tin_number);
    _set('nin_number', prefill.nin_number);
    _set('bvn_number', prefill.bvn_number);
    _set('address_line1', prefill.address_line1);
    if (phoneHelper) {
      phoneHelper.setFull(prefill.phone || '');
    } else if (phoneHidden) {
      phoneHidden.value = prefill.phone || '';
    }
    const genderField = form.querySelector('[name="gender"]');
    if (genderField) genderField.value = prefill.gender || '';
    if (contactCountry && prefill.country) contactCountry.value = prefill.country;
    if (contactState && prefill.state) contactState.value = prefill.state;
    setFieldLock(personalFields, true);
    if (personalTokenInput) personalTokenInput.value = personalVerification ? personalVerification.getToken() : '';
    updateVerificationFormVisibility();
  }

  function applyCorporatePrefill(prefill) {
    const map = {
      organization_name: prefill.name || '',
      organization_email: prefill.email || '',
      organization_website: prefill.website || '',
      organization_cac_number: prefill.cac_number || '',
      organization_tin: prefill.tin || '',
      organization_address_line1: prefill.address_line1 || '',
      organization_country: prefill.country || '',
      organization_state: prefill.state || ''
    };
    Object.keys(map).forEach((key) => {
      const field = form.querySelector('[name="' + key + '"]');
      if (field) field.value = map[key];
    });
    if (organizationPhoneHelper) {
      organizationPhoneHelper.setFull(prefill.phone || '');
    } else if (organizationPhoneHidden) {
      organizationPhoneHidden.value = prefill.phone || '';
    }
    setFieldLock(organizationIdentityFields, true);
    if (corporateTokenInput) corporateTokenInput.value = corporateVerification ? corporateVerification.getToken() : '';
    updateVerificationFormVisibility();
  }

  function updateVerificationFormVisibility() {
    if (!identityVerificationEnabled || !publicIdentityFormFields) return;
    const personalOk = !personalVerificationRequired() || (personalVerification && personalVerification.isVerified());
    const corporateOk = !corporateVerificationRequired() || (corporateVerification && corporateVerification.isVerified());
    const allow = personalOk && corporateOk;
    publicIdentityFormFields.classList.toggle('d-none', !allow);
    if (btnNext) {
      btnNext.disabled = step === 0 && !allow;
    }
  }

  function resetVerifiedStateAfterSubmit() {
    if (personalTokenInput) personalTokenInput.value = '';
    if (corporateTokenInput) corporateTokenInput.value = '';
    if (personalVerification) personalVerification.reset();
    if (corporateVerification) corporateVerification.reset();
    if (identityVerificationEnabled) {
      setFieldLock(personalFields, personalVerificationRequired());
      setFieldLock(organizationIdentityFields, corporateVerificationRequired());
    } else {
      setFieldLock(personalFields, false);
      setFieldLock(organizationIdentityFields, false);
    }
    if (publicCorporateVerificationCol) {
      publicCorporateVerificationCol.classList.toggle('d-none', !corporateVerificationRequired());
    }
    if (publicPersonalVerificationBlock) {
      publicPersonalVerificationBlock.classList.toggle('d-none', !personalVerificationRequired());
    }
    updateVerificationFormVisibility();
  }

  function enforceForcedMode() {
    if (forcedMode === 'organization') {
      if (organizationModeOrganization) {
        organizationModeOrganization.checked = true;
        organizationModeOrganization.disabled = false;
      }
      if (organizationModeIndividual) {
        organizationModeIndividual.checked = false;
        organizationModeIndividual.disabled = true;
      }
      if (organizationModeLabelIndividual) {
        organizationModeLabelIndividual.classList.add('disabled');
        organizationModeLabelIndividual.setAttribute('aria-disabled', 'true');
      }
      if (organizationModeLabelOrganization) {
        organizationModeLabelOrganization.classList.add('active');
      }
      toggleorganizationFields(true);
      return;
    }

    if (organizationModeIndividual) {
      organizationModeIndividual.disabled = false;
    }
    if (organizationModeLabelIndividual) {
      organizationModeLabelIndividual.classList.remove('disabled');
      organizationModeLabelIndividual.removeAttribute('aria-disabled');
    }
    if (forcedMode === 'personal') {
      toggleorganizationFields(false);
    }
  }

  function getPreviousStep(currentStep) {
    if (skipOrganizationStep && currentStep === 3) {
      return showModeStep ? 1 : 1;
    }
    const minStep = showModeStep ? 0 : 1;
    return Math.max(minStep, currentStep - 1);
  }

  function syncOrganizationStepVisibility() {
    const isPersonal = getComplaintMode() !== 'organization';
    skipOrganizationStep = isPersonal;
    if (stepIndicator2) stepIndicator2.classList.toggle('d-none', isPersonal);
    if (stepIndicator3) {
      const baseCount = showModeStep ? 4 : 3;
      stepIndicator3.textContent = isPersonal ? String(baseCount - 1) : String(baseCount);
    }
  }

  function updateHeading() {
    if (!headingEl) return;
    const mode = getComplaintMode();
    const forced = forcedMode === 'personal' || forcedMode === 'organization';
    if (!forced && step === 1) {
      headingEl.textContent = 'Submit a Case';
      if (subheadingEl) subheadingEl.textContent = 'Provide your contact details and case information.';
      return;
    }
    headingEl.textContent = mode === 'organization' ? 'Submit a Business Case' : 'Submit a Personal Case';
    if (subheadingEl) {
      subheadingEl.textContent = mode === 'organization'
        ? 'Provide your details first, then continue with the organization and case information.'
        : 'Provide your personal details and case information.';
    }
  }

  function showAlert(msg) {
    if (!msg) {
      alertBox.classList.add('d-none');
      alertBox.textContent = '';
      return;
    }
    alertBox.textContent = msg;
    alertBox.classList.remove('d-none');
  }
  function showSuccess(msg) {
    if (!msg) {
      successBox.classList.add('d-none');
      successBox.textContent = '';
      return;
    }
    successBox.textContent = msg;
    successBox.classList.remove('d-none');
  }

  function renderCategoriesForComplaintType() {
    if (!domainSelect || !domainSourceList.length) {
      renderSubDomainsForDomain();
      return;
    }
    const complaintType = String(complaintTypeSelect?.value || '').trim();
    const mapped = (complaintType && Array.isArray(serviceDomainRoutes[complaintType]))
      ? serviceDomainRoutes[complaintType].map(String)
      : [];
    const hasMapping = mapped.length > 0;
    const allowedSet = new Set(mapped);
    const filtered = hasMapping
      ? domainSourceList.filter(c => allowedSet.has(c.id))
      : domainSourceList.slice();
    const currentValue = domainSelect.value;

    domainSelect.innerHTML = '<option value="">Select a domain</option>';
    filtered.forEach(c => {
      const opt = document.createElement('option');
      opt.value = c.id;
      opt.textContent = c.name;
      domainSelect.appendChild(opt);
    });

    if (currentValue && filtered.some(c => c.id === currentValue)) {
      domainSelect.value = currentValue;
    } else if (filtered.length === 1) {
      domainSelect.value = filtered[0].id;
    } else {
      domainSelect.value = '';
    }
    renderSubDomainsForDomain();
  }

  function getSelectedDomain() {
    const domainId = String(domainSelect?.value || '').trim();
    if (!domainId) return null;
    return domainSourceList.find((item) => String(item.id) === domainId) || null;
  }

  function renderSubDomainsForDomain() {
    if (!subDomainSelect || !subDomainWrap) return;
    if (!serviceDomainSettings.enable_sub_domains) {
      subDomainWrap.classList.add('d-none');
      subDomainSelect.disabled = true;
      subDomainSelect.required = false;
      subDomainSelect.value = '';
      return;
    }
    const selectedDomain = getSelectedDomain();
    const options = Array.isArray(selectedDomain?.sub_domains)
      ? selectedDomain.sub_domains.filter((item) => Number(item?.is_active ?? 1) === 1)
      : [];
    const shouldShow = !!selectedDomain;
    const currentValue = String(subDomainSelect.value || '').trim();
    subDomainWrap.classList.toggle('d-none', !shouldShow);
    subDomainSelect.disabled = !options.length;
    subDomainSelect.required = options.length > 0;
    subDomainSelect.classList.toggle('d-none', options.length === 0);
    subDomainSelect.innerHTML = '<option value="">Select sub service domain</option>';

    if (!shouldShow) {
      subDomainSelect.value = '';
      return;
    }

    options.forEach((item) => {
      const option = document.createElement('option');
      option.value = String(item.id || '');
      option.textContent = item.name || `Sub Service Domain #${item.id}`;
      subDomainSelect.appendChild(option);
    });

    if (currentValue && options.some((item) => String(item.id) === currentValue)) {
      subDomainSelect.value = currentValue;
    } else {
      subDomainSelect.value = '';
    }
  }

  function updateSubComplaintTypes(selectedValue) {
    if (!subComplaintTypeWrap || !subComplaintTypeSelect) return;
    const complaintType = String(complaintTypeSelect?.value || '');
    const meta = complaintType ? complaintTypeMap[complaintType] : null;
    const options = meta && Array.isArray(meta.sub_types) ? meta.sub_types : [];
    const enabled = Number(complaintTypeSettings.enable_sub_types || 0) === 1;
    const shouldShow = enabled && options.length > 0;

    subComplaintTypeWrap.classList.toggle('d-none', !shouldShow);
    subComplaintTypeSelect.disabled = !shouldShow;
    subComplaintTypeSelect.required = shouldShow;
    subComplaintTypeSelect.innerHTML = '<option value="">Select sub complaint type</option>';

    if (!shouldShow) {
      subComplaintTypeSelect.value = '';
      return;
    }

    options.forEach((item) => {
      const code = String(item?.code || item?.label || '').trim();
      if (!code) return;
      const option = document.createElement('option');
      option.value = code;
      option.textContent = item?.label || code;
      subComplaintTypeSelect.appendChild(option);
    });

    if (selectedValue) {
      subComplaintTypeSelect.value = String(selectedValue);
      if (subComplaintTypeSelect.value !== String(selectedValue)) {
        subComplaintTypeSelect.value = '';
      }
    } else {
      subComplaintTypeSelect.value = '';
    }
  }

  function setStep(next) {
    step = next;
    document.querySelectorAll('.wizard-step').forEach(s => {
      s.classList.toggle('active', parseInt(s.dataset.step, 10) === step);
    });
    document.querySelectorAll('.step-item').forEach(s => {
      const n = parseInt(s.dataset.step, 10);
      s.classList.toggle('step-active', n === step);
      s.classList.toggle('step-done', n < step);
    });
    const minStep = showModeStep ? 0 : 1;
    if (btnPrev) btnPrev.disabled = step <= minStep;
    if (btnNext) btnNext.classList.toggle('d-none', step === 3);
    if (btnNext && !identityVerificationEnabled) btnNext.disabled = false;
    if (btnSubmit) btnSubmit.classList.toggle('d-none', step !== 3);
    document.querySelectorAll('.wizard-step').forEach(stepEl => {
      const isActive = stepEl.classList.contains('active');
      stepEl.querySelectorAll('[data-was-required]').forEach(input => {
        input.required = isActive;
      });
    });
    if (step === 2 && !skipOrganizationStep) {
      if (forcedMode === 'organization') {
        enforceForcedMode();
      } else {
        const mode = form.querySelector('input[name="organization_mode"]:checked')?.value || 'individual';
        toggleorganizationFields(mode === 'organization');
      }
    }
    updateHeading();
    updateVerificationFormVisibility();
  }

  const phoneHelper = window.PhoneHelper ? window.PhoneHelper.bind({
    countriesUrl: countriesJson,
    codeSelect: phoneCode,
    localInput: phoneInput,
    fullInput: phoneHidden,
    countrySelect: contactCountry,
    defaultDial: '+234',
    defaultCountry: 'NG'
  }) : null;

  const organizationPhoneHelper = window.PhoneHelper ? window.PhoneHelper.bind({
    countriesUrl: countriesJson,
    codeSelect: organizationPhoneCode,
    localInput: organizationPhoneInput,
    fullInput: organizationPhoneHidden,
    countrySelect: organizationCountry,
    defaultDial: '+234',
    defaultCountry: 'NG'
  }) : null;

  function toggleorganizationFields(enabled) {
    if (!organizationFields) return;
    organizationFields.classList.toggle('d-none', !enabled);
    organizationInputs.forEach(input => {
      input.disabled = !enabled;
      if (input.name === 'organization_name' || input.name === 'organization_email' || input.id === 'organization_phone_local') {
        input.required = enabled;
      }
    });
    if (organizationCustomWrap) {
      organizationCustomWrap.classList.toggle('d-none', !enabled || !organizationDefs.length);
    }
    if (caseForValue) caseForValue.value = enabled ? 'organization' : 'personal';
    if (modeNoticeEl) {
      modeNoticeEl.classList.toggle('d-none', !(enabled && forcedMode === 'organization'));
    }
    if (!enabled && organizationPhoneHidden) organizationPhoneHidden.value = '';
    if (!enabled && organizationPhoneInput) organizationPhoneInput.value = '';
    if (!enabled && organizationSameAddress) {
      organizationSameAddress.checked = false;
    }
  }

  function setupCustomFields() {
    if (typeof CustomFieldRenderer === 'undefined') return;
    CustomFieldRenderer.render(contactCustomContainer, contactDefs, null, {
      idPrefix: 'contact_cf_',
      labelClass: 'form-label',
      inputClass: 'form-control',
      selectClass: 'form-select',
      textareaRows: 3,
      emptyHtml: '',
      hideWhenEmpty: true,
      wrapEl: contactCustomWrap
    });
    CustomFieldRenderer.render(organizationCustomContainer, organizationDefs, null, {
      idPrefix: 'organization_cf_',
      labelClass: 'form-label',
      inputClass: 'form-control',
      selectClass: 'form-select',
      textareaRows: 3,
      emptyHtml: '',
      hideWhenEmpty: true,
      wrapEl: organizationCustomWrap
    });
    CustomFieldRenderer.render(caseCustomContainer, caseDefs, null, {
      idPrefix: 'case_cf_',
      labelClass: 'form-label',
      inputClass: 'form-control',
      selectClass: 'form-select',
      textareaRows: 3,
      emptyHtml: '',
      hideWhenEmpty: true,
      wrapEl: caseCustomWrap
    });
  }

  function populateCountrySelect(select, countries) {
    if (!select || !Array.isArray(countries)) return;
    const current = select.value || '';
    select.innerHTML = '<option value="">Select country</option>';
    countries.forEach(c => {
      const id = parseInt(c.id || 0, 10);
      const code = (c.code || c.iso2 || c.iso || '').toString();
      const name = (c.name || '').toString();
      if (!code || !name) return;
      const opt = document.createElement('option');
      opt.value = code.toUpperCase();
      opt.textContent = name;
      if (id) opt.dataset.id = String(id);
      if (code) opt.dataset.code = code.toUpperCase();
      select.appendChild(opt);
    });
    if (current) {
      select.value = current;
    } else {
      const ng = Array.from(select.options).find(o => (o.dataset.code || '').toUpperCase() === 'NG');
      if (ng) select.value = ng.value;
    }
  }

  function getCountryId(select) {
    if (!select) return 0;
    const opt = select.options[select.selectedIndex];
    const id = opt && opt.dataset ? parseInt(opt.dataset.id || '0', 10) : 0;
    return Number.isFinite(id) ? id : 0;
  }

  function populateStateSelect(select, states) {
    if (!select) return;
    select.innerHTML = '<option value="">Select state</option>';
    if (!Array.isArray(states)) return;
    states.forEach(s => {
      const name = (s.name || '').toString();
      if (!name) return;
      const opt = document.createElement('option');
      opt.value = name;
      opt.textContent = name;
      if (s.id) opt.dataset.id = String(s.id);
      select.appendChild(opt);
    });
  }

  function loadStatesFor(countrySelect, stateSelect) {
    if (!countrySelect || !stateSelect) return Promise.resolve();
    const countryId = getCountryId(countrySelect);
    if (!countryId) {
      populateStateSelect(stateSelect, []);
      return Promise.resolve();
    }
    return fetch(statesJson + '&country_id=' + encodeURIComponent(countryId))
      .then(res => res.ok ? res.json() : {})
      .then(payload => {
        const rows = Array.isArray(payload.data) ? payload.data : [];
        populateStateSelect(stateSelect, rows);
      })
      .catch(() => populateStateSelect(stateSelect, []));
  }

  function loadCountries() {
    return fetch(countriesJson)
      .then(res => res.ok ? res.json() : {})
      .then(payload => {
        const rows = Array.isArray(payload.data) ? payload.data : [];
        populateCountrySelect(contactCountry, rows);
        populateCountrySelect(organizationCountry, rows);
        return Promise.all([
          loadStatesFor(contactCountry, contactState),
          loadStatesFor(organizationCountry, organizationState)
        ]);
      })
      .catch(() => {});
  }

  function applySameAddress() {
    if (!organizationSameAddress || !organizationSameAddress.checked) return;
    if (contactAddress && organizationAddress) organizationAddress.value = contactAddress.value.trim();
    if (contactCountry && organizationCountry) organizationCountry.value = contactCountry.value;
    if (contactState && organizationState) organizationState.value = contactState.value;
  }

  function initEditor() {
    const editorEl = document.getElementById('public_case_description_editor');
    const hidden = document.getElementById('public_case_description');
    if (!editorEl || quillEditor || !window.Quill) return;
    quillEditor = new Quill(editorEl, {
      theme: 'snow',
      placeholder: 'Describe the issue...',
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
    if (hidden && hidden.value) {
      quillEditor.root.innerHTML = hidden.value;
    }
  }

  function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  function validateStep0() {
    const consentCheck = document.getElementById('ndpr_consent_check');
    const consentError = document.getElementById('ndpr_consent_error');
    if (consentCheck && !consentCheck.checked) {
      if (consentError) consentError.classList.remove('d-none');
      consentCheck.focus();
      return false;
    }
    if (consentError) consentError.classList.add('d-none');
    if (!identityVerificationEnabled) return true;
    if (personalVerificationRequired() && (!personalVerification || !personalVerification.isVerified())) {
      showAlert('Complete personal verification before continuing.');
      return false;
    }
    if (corporateVerificationRequired() && (!corporateVerification || !corporateVerification.isVerified())) {
      showAlert('Complete corporate verification before continuing.');
      return false;
    }
    return true;
  }

  function validateStep1() {
    const _val = (name) => { const el = form.querySelector('[name="' + name + '"]'); return el ? el.value.trim() : ''; };
    const firstName = _val('first_name');
    const lastName = _val('last_name');
    const email = _val('email');
    const gender = form.querySelector('[name="gender"]')?.value || '';
    if (phoneHelper && phoneHidden) phoneHidden.value = phoneHelper.getFull();
    const phone = _val('phone');

    // Collect ALL missing required fields in one pass so the error lists everything at once.
    const missing = [];
    if (!firstName)  missing.push('First name');
    if (!lastName)   missing.push('Last name');
    if (!email)      missing.push('Email');
    if (!gender)     missing.push('Gender');
    if (!phone)      missing.push('Phone');

    // Configurable-required fields (driven by contact_field_config setting)
    const configurableFields = [
      { name: 'tin_number',    label: 'Tax ID/TIN' },
      { name: 'nin_number',    label: 'NIN' },
      { name: 'bvn_number',    label: 'BVN' },
      { name: 'address_line1', label: 'Address' },
      { name: 'country',       label: 'Country' },
      { name: 'state',         label: 'State' },
    ];
    configurableFields.forEach(function (f) {
      const el = form.querySelector('[name="' + f.name + '"]');
      const isRequired = !!(el && (el.dataset.wasRequired || el.required));
      if (isRequired && !el.value.trim()) {
        missing.push(f.label);
      }
    });

    if (missing.length) {
      showAlert(missing.join(', ') + (missing.length === 1 ? ' is required.' : ' are required.'));
      return false;
    }

    if (!isValidEmail(email)) {
      showAlert('Please enter a valid email address.');
      return false;
    }

    return true;
  }

  function validateStep2() {
    const mode = form.querySelector('input[name="organization_mode"]:checked')?.value || 'individual';
    if (mode === 'organization') {
      if (corporateVerificationRequired() && (!corporateVerification || !corporateVerification.isVerified())) {
        showAlert('Corporate verification is required.');
        return false;
      }
      if (organizationPhoneHelper && organizationPhoneHidden) organizationPhoneHidden.value = organizationPhoneHelper.getFull();
      const _oval = (name) => { const el = form.querySelector('[name="' + name + '"]'); return el ? el.value.trim() : ''; };
      const organizationName = _oval('organization_name');
      const organizationEmail = _oval('organization_email');
      const organizationPhone = _oval('organization_phone');
      if (!organizationName) {
        showAlert('Organization Name is required.');
        return false;
      }
      if (!organizationEmail) {
        showAlert('organization email is required.');
        return false;
      }
      if (!organizationPhone) {
        showAlert('organization phone is required.');
        return false;
      }
    }
    return true;
  }

  btnPrev?.addEventListener('click', () => {
    showAlert('');
    setStep(getPreviousStep(step));
  });
  btnNext?.addEventListener('click', () => {
    showAlert('');
    if (step === 0 && !validateStep0()) return;
    if (step === 0) {
      setStep(1);
      return;
    }
    if (step === 1 && !validateStep1()) return;
    if (step === 1 && skipOrganizationStep) {
      setStep(3);
      return;
    }
    if (step === 2 && !validateStep2()) return;
    setStep(step + 1);
  });

  document.querySelectorAll('input[name="organization_mode"]').forEach(radio => {
    radio.addEventListener('change', () => {
      if (forcedMode === 'organization') {
        enforceForcedMode();
        updateHeading();
        return;
      }
      toggleorganizationFields(radio.value === 'organization');
      if (publicCorporateVerificationCol) {
        publicCorporateVerificationCol.classList.toggle('d-none', !(radio.value === 'organization' && identityVerificationOrganizationsEnabled));
      }
      if (publicPersonalVerificationBlock) {
        publicPersonalVerificationBlock.classList.toggle('d-none', !identityVerificationContactsEnabled);
      }
      if (radio.value !== 'organization') {
        if (corporateTokenInput) corporateTokenInput.value = '';
        if (corporateVerification) corporateVerification.reset();
        setFieldLock(organizationIdentityFields, false);
      }
      syncOrganizationStepVisibility();
      updateVerificationFormVisibility();
      updateHeading();
    });
  });

  if (attachmentsInput && attachmentsList) {
    attachmentsInput.addEventListener('change', () => {
      if (!attachmentsInput.files || !attachmentsInput.files.length) {
        attachmentsList.textContent = 'No files selected.';
        return;
      }
      attachmentsList.textContent = Array.from(attachmentsInput.files).map(f => f.name).join(', ');
    });
  }
  if (complaintTypeSelect) {
    complaintTypeSelect.addEventListener('change', () => {
      renderCategoriesForComplaintType();
      updateSubComplaintTypes('');
    });
  }
  if (domainSelect) {
    domainSelect.addEventListener('change', () => {
      renderSubDomainsForDomain();
    });
  }
  if (subDomainSelect) {
    subDomainSelect.addEventListener('change', renderSubDomainsForDomain);
  }

  form.addEventListener('submit', (e) => {
    e.preventDefault();
    showAlert('');
    showSuccess('');
    if (!validateStep1() || !validateStep2()) return;
    const payload = new FormData(form);
    if (personalTokenInput && personalTokenInput.value) payload.set('identity_personal_token', personalTokenInput.value);
    if (corporateTokenInput && corporateTokenInput.value) payload.set('identity_corporate_token', corporateTokenInput.value);
    if (quillEditor) {
      payload.set('description', quillEditor.root.innerHTML.trim());
    }
    const descriptionText = quillEditor ? quillEditor.getText().trim() : (payload.get('description') || '').trim();
    const complaintType = (payload.get('complaint_type') || '').trim();
    const subDomainId = subDomainSelect && !subDomainSelect.disabled
      ? String(payload.get('sub_domain_id') || '').trim()
      : '';
    if (subDomainSelect && !subDomainSelect.disabled && !subDomainId) {
      showAlert('Please select a sub service domain.');
      return;
    }
    const subComplaintType = subComplaintTypeSelect && !subComplaintTypeSelect.disabled
      ? String(payload.get('sub_complaint_type') || '').trim()
      : '';
    if (!payload.get('subject') || !payload.get('domain_id') || !payload.get('priority') || !descriptionText || !complaintType) {
      showAlert('Subject, service domain, complaint type, priority, and description are required.');
      return;
    }
    if (subComplaintTypeSelect && !subComplaintTypeSelect.disabled && !subComplaintType) {
      showAlert('Please select a sub complaint type.');
      return;
    }
    if (typeof CustomFieldRenderer !== 'undefined') {
      const contactCustom = CustomFieldRenderer.collect(contactCustomContainer);
      const organizationCustom = CustomFieldRenderer.collect(organizationCustomContainer);
      const caseCustom = CustomFieldRenderer.collect(caseCustomContainer);
      payload.set('contact_custom_fields', JSON.stringify(contactCustom || {}));
      payload.set('organization_custom_fields', JSON.stringify(organizationCustom || {}));
      payload.set('case_custom_fields', JSON.stringify(caseCustom || {}));
    }
    if (subDomainId) {
      payload.set('sub_domain_id', subDomainId);
    }
    if (spinner) spinner.classList.remove('d-none');
    if (submitLabel) submitLabel.textContent = 'Submitting...';
    btnSubmit.disabled = true;
    fetch('api/public/case', {
      method: 'POST',
      body: payload
    })
      .then(r => r.json().then(d => ({ ok: r.ok, data: d })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error(data?.error || 'Unable to submit case');
        showSuccess('Case submitted successfully. Case No: ' + (data.case_number || ''));
        if (data && data.redirect_url) {
          setTimeout(() => { window.location.href = data.redirect_url; }, 1200);
          return;
        }
        form.reset();
        resetVerifiedStateAfterSubmit();
        if (attachmentsList) attachmentsList.textContent = 'No files selected.';
        updateSubComplaintTypes('');
        renderCategoriesForComplaintType();
        renderSubDomainsForDomain();
        enforceForcedMode();
        toggleorganizationFields(getComplaintMode() === 'organization');
        updateHeading();
        setStep(1);
      })
      .catch(err => showAlert(err.message || 'Unable to submit case'))
      .finally(() => {
        btnSubmit.disabled = false;
        if (spinner) spinner.classList.add('d-none');
        if (submitLabel) submitLabel.textContent = submitDefaultLabel;
      });
  });

  setupCustomFields();
  initEditor();
  loadCountries();
  if (!organizationsEnabled && stepIndicator2) {
    stepIndicator2.classList.add('d-none');
  }
  syncOrganizationStepVisibility();
  enforceForcedMode();
  toggleorganizationFields(getComplaintMode() === 'organization');
  if (organizationSameAddress) {
    organizationSameAddress.addEventListener('change', () => applySameAddress());
  }
  if (contactAddress) contactAddress.addEventListener('input', applySameAddress);
  if (contactCountry) {
    contactCountry.addEventListener('change', () => {
      applySameAddress();
      loadStatesFor(contactCountry, contactState);
    });
  }
  if (organizationCountry) {
    organizationCountry.addEventListener('change', () => {
      loadStatesFor(organizationCountry, organizationState);
    });
  }
  updateHeading();
  setStep(showModeStep ? 0 : 1);
  renderCategoriesForComplaintType();
  renderSubDomainsForDomain();
  updateSubComplaintTypes('');

  if (identityVerificationEnabled && window.IdentityVerification) {
    setFieldLock(personalFields, personalVerificationRequired());
    setFieldLock(organizationIdentityFields, corporateVerificationRequired());
    if (publicCorporateVerificationCol) {
      publicCorporateVerificationCol.classList.toggle('d-none', !corporateVerificationRequired());
    }
    if (publicPersonalVerificationBlock) {
      publicPersonalVerificationBlock.classList.toggle('d-none', !personalVerificationRequired());
    }
    updateVerificationFormVisibility();
    if (publicPersonalVerificationBlock && identityVerificationContactsEnabled) {
      personalVerification = window.IdentityVerification.bindBlock({
        root: publicPersonalVerificationBlock,
        mode: 'public',
        endpoint: verificationApi,
        sectionType: 'personal',
        entityMode: function () { return getComplaintMode() === 'organization' ? 'corporate' : 'personal'; },
        availableMethods: Array.isArray((window.selfComplaintConfig || {}).identityVerificationPersonalMethods)
          ? window.selfComplaintConfig.identityVerificationPersonalMethods
          : null,
        onPrefill: function (prefill) {
          applyPersonalPrefill(prefill || {});
        },
        onReset: function () {
          if (personalTokenInput) personalTokenInput.value = '';
          updateVerificationFormVisibility();
        }
      });
    }
    if (publicCorporateVerificationBlock && identityVerificationOrganizationsEnabled) {
      corporateVerification = window.IdentityVerification.bindBlock({
        root: publicCorporateVerificationBlock,
        mode: 'public',
        endpoint: verificationApi,
        sectionType: 'corporate',
        entityMode: function () { return 'corporate'; },
        availableMethods: Array.isArray((window.selfComplaintConfig || {}).identityVerificationCorporateMethods)
          ? window.selfComplaintConfig.identityVerificationCorporateMethods
          : null,
        onPrefill: function (prefill) {
          applyCorporatePrefill(prefill || {});
        },
        onReset: function () {
          if (corporateTokenInput) corporateTokenInput.value = '';
          updateVerificationFormVisibility();
        }
      });
    }
  }
})();
