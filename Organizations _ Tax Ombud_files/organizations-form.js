function setuporganizationForm(options) {
  const opts = options || {};
  const escapeHtml = (val) => String(val ?? '').replace(/[&<>"']/g, (ch) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
  }[ch]));
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
  const base = (typeof url_root !== 'undefined' ? url_root : '../');
  const apiorganizationsIndex = opts.apiorganizationsIndex
    || apiMap.portalorganizationsIndex
    || apiMap.organizationsIndex
    || (base + 'api/portal/organizations/index');
  const apiorganizationDetail = opts.apiorganizationDetail
    || apiMap.portalorganizationDetail
    || apiMap.organizationDetail
    || (base + 'api/portal/organizations/detail');
  const apiContacts = opts.apiContacts
    || apiMap.portalContactsIndex
    || apiMap.contactsIndex
    || (base + 'api/portal/contacts/index');
  const countriesJson = apiMap.countries || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/geo/index?action=countries');
  const defaultContactId = opts.defaultContactId || null;
  const defaultContactLabel = opts.defaultContactLabel || '';

  const apiorganizationContacts =
    apiMap.organizationContacts
    || apiMap.portalOrganizationContacts
    || (base + 'api/modules/organizations/contacts');

  const modalEl = document.getElementById('organizationModal');
  if (!modalEl) return null;
  const modal = modalEl.classList.contains('offcanvas')
    ? new bootstrap.Offcanvas(modalEl)
    : new bootstrap.Modal(modalEl);
  const formAlert = document.getElementById('organizationFormAlert');
  const titleEl = document.getElementById('organizationModalLabel');
  const fields = {
    id: document.getElementById('organization_id'),
    name: document.getElementById('co_name'),
    tin: document.getElementById('co_tin'),
    cac: document.getElementById('co_cac'),
    phone: document.getElementById('co_phone'),
    phoneFull: document.getElementById('co_phone_full'),
    phoneCode: document.getElementById('co_phone_code'),
    email: document.getElementById('co_email'),
    website: document.getElementById('co_website'),
    address: document.getElementById('co_address'),
    city: document.getElementById('co_city'),
    cityName: document.getElementById('co_city_name'),
    state: document.getElementById('co_state'),
    stateName: document.getElementById('co_state_name'),
    country: document.getElementById('co_country'),
    countryName: document.getElementById('co_country_name'),
    contactId: document.getElementById('co_contact'),
    contactSearch: document.getElementById('co_contact_search'),
    contactResults: document.getElementById('co_contact_results'),
    contactSelected: document.getElementById('co_contact_selected'),
    contactStatus: document.getElementById('co_contact_search_status'),
    primary: document.getElementById('co_primary')
  };
  const verificationConfig = window.organizationIdentityVerificationConfig || {};
  const verificationWrap = document.getElementById('organizationIdentityVerificationWrap');
  const corporateTokenInput = document.getElementById('organization_identity_corporate_token');
  const phoneHelper = window.PhoneHelper ? window.PhoneHelper.bind({
    countriesUrl: countriesJson,
    codeSelect: fields.phoneCode,
    localInput: fields.phone,
    fullInput: fields.phoneFull,
    countrySelect: null,
    defaultDial: '+234',
    defaultCountry: 'NG'
  }) : null;
  const geoHelper = window.GeoHelper ? window.GeoHelper.bind({
    countrySelect: fields.country,
    stateSelect: fields.state,
    citySelect: fields.city,
    countryNameInput: fields.countryName,
    stateNameInput: fields.stateName,
    cityNameInput: fields.cityName,
    apiCountries: apiMap.geoCountries || (base + 'api/modules/geo/index?action=countries'),
    apiStates: apiMap.geoStates || (base + 'api/modules/geo/index?action=states'),
    apiCities: apiMap.geoCities || (base + 'api/modules/geo/index?action=cities'),
    defaultCountryCode: 'NG',
    onCountryMeta: (meta) => {
      if (!meta || !fields.phoneCode) return;
      const phonecode = String(meta.phonecode || '').trim();
      if (!phonecode) return;
      const dial = '+' + phonecode;
      if (!Array.from(fields.phoneCode.options || []).some((opt) => opt.value === dial)) {
        const opt = document.createElement('option');
        opt.value = dial;
        opt.textContent = dial;
        fields.phoneCode.appendChild(opt);
      }
      fields.phoneCode.value = dial;
      fields.phoneCode.dispatchEvent(new Event('change', { bubbles: true }));
    }
  }) : null;
  let editingId = null;
  let corporateVerificationBlock = null;

  function clearForm() {
    if (formAlert) formAlert.classList.add('d-none');
    if (formAlert) formAlert.textContent = '';
    Object.values(fields).forEach(f => { if (f && f.tagName === 'INPUT') f.value = ''; });
    if (fields.country) fields.country.value = '';
    if (phoneHelper) {
      phoneHelper.setFull('');
    } else if (fields.phoneCode && !fields.phoneCode.value) {
      fields.phoneCode.value = '+234';
    }
    if (fields.countryName) fields.countryName.value = '';
    if (fields.stateName) fields.stateName.value = '';
    if (fields.cityName) fields.cityName.value = '';
    if (fields.country) fields.country.value = '';
    if (fields.state) fields.state.value = '';
    if (fields.city) fields.city.value = '';
    if (fields.primary) fields.primary.checked = true;
    if (fields.contactSelected) fields.contactSelected.textContent = '';
    if (corporateTokenInput) corporateTokenInput.value = '';
    if (corporateVerificationBlock) corporateVerificationBlock.reset();
    if (defaultContactId && fields.contactId) {
      fields.contactId.value = defaultContactId;
      fields.contactSearch.value = defaultContactLabel || '';
      fields.contactSelected.textContent = defaultContactLabel ? ('Selected: ' + defaultContactLabel) : '';
    }
  }

  function showError(msg) {
    if (!formAlert) return;
    formAlert.textContent = msg;
    formAlert.classList.remove('d-none');
  }

  async function loadCountries() {
    const waits = [];
    if (phoneHelper) waits.push(phoneHelper.ready);
    if (geoHelper) waits.push(geoHelper.init({ country: 'Nigeria' }));
    if (waits.length) {
      await Promise.all(waits);
    }
  }

  function renderContacts(list) {
    if (!fields.contactResults) return;
    fields.contactResults.innerHTML = '';
    (list || []).forEach(c => {
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = 'list-group-item list-group-item-action';
      const label = ((c.first_name || '') + ' ' + (c.last_name || '')).trim();
      const main = label || c.phone || c.email || (moduleLabel('contact', 'singular') + ' #' + c.id);
      const meta = [c.phone, c.email, c.tin_number].filter(Boolean).join(' • ');
      btn.innerHTML = `<div class=\"fw-semibold\">${escapeHtml(main)}</div><div class=\"small text-muted\">${escapeHtml(meta)}</div>`;
      btn.addEventListener('click', () => {
        fields.contactId.value = c.id;
        fields.contactSearch.value = main;
        fields.contactSelected.textContent = 'Selected: ' + main;
        if (fields.contactStatus) {
          fields.contactStatus.textContent = '';
          fields.contactStatus.classList.remove('text-danger');
          fields.contactStatus.classList.add('text-muted');
        }
        fields.contactResults.classList.add('d-none');
        fields.contactResults.innerHTML = '';
      });
      fields.contactResults.appendChild(btn);
    });
    fields.contactResults.classList.toggle('d-none', !fields.contactResults.innerHTML);
  }

  function searchContacts(term) {
    if (!term || term.length < 4) {
      if (fields.contactResults) {
        fields.contactResults.classList.add('d-none');
        fields.contactResults.innerHTML = '';
      }
      if (fields.contactStatus) {
        fields.contactStatus.textContent = 'Enter 4 or more characters to search.';
        fields.contactStatus.classList.remove('text-danger');
        fields.contactStatus.classList.add('text-muted');
      }
      return;
    }
    if (fields.contactStatus) {
      fields.contactStatus.textContent = 'Searching...';
      fields.contactStatus.classList.remove('text-danger');
      fields.contactStatus.classList.add('text-muted');
    }
    fetch(apiContacts + '?q=' + encodeURIComponent(term) + '&limit=5')
      .then(r => r.json())
      .then(data => {
        const rows = data.data || data || [];
        if (!rows.length) {
          if (fields.contactResults) {
            fields.contactResults.classList.add('d-none');
            fields.contactResults.innerHTML = '';
          }
          if (fields.contactStatus) {
            fields.contactStatus.textContent = 'No result found.';
            fields.contactStatus.classList.add('text-danger');
            fields.contactStatus.classList.remove('text-muted');
          }
          return;
        }
        renderContacts(rows);
        if (fields.contactStatus) {
          fields.contactStatus.textContent = '';
          fields.contactStatus.classList.remove('text-danger');
          fields.contactStatus.classList.add('text-muted');
        }
      })
      .catch(() => {
        if (fields.contactStatus) {
          fields.contactStatus.textContent = 'No result found.';
          fields.contactStatus.classList.add('text-danger');
          fields.contactStatus.classList.remove('text-muted');
        }
      });
  }

  function fillForm(data) {
    editingId = data.id || null;
    if (fields.id) fields.id.value = data.id || '';
    if (fields.name) fields.name.value = data.name || '';
    if (fields.tin) fields.tin.value = data.tin || '';
    if (fields.cac) fields.cac.value = data.cac_number || '';
    if (phoneHelper) {
      phoneHelper.setFull(data.phone || '');
    } else if (fields.phone && fields.phoneCode && data.phone) {
      const opts = Array.from(fields.phoneCode.options || []).sort((a, b) => b.value.length - a.value.length);
      const match = opts.find(o => data.phone.startsWith(o.value));
      if (match) {
        fields.phoneCode.value = match.value;
        fields.phone.value = data.phone.slice(match.value.length).replace(/\D+/g, '');
      } else {
        fields.phone.value = (data.phone || '').replace(/\D+/g, '');
      }
    } else if (fields.phone) {
      fields.phone.value = (data.phone || '').replace(/\D+/g, '');
    }
    if (fields.email) fields.email.value = data.email || '';
    if (fields.website) fields.website.value = data.website || '';
    if (fields.address) fields.address.value = data.address_line1 || '';
    if (fields.cityName) fields.cityName.value = data.city || '';
    if (fields.stateName) fields.stateName.value = data.state || '';
    if (fields.countryName) fields.countryName.value = data.country || '';
    const owner = (data.contacts || []).find(c => c.is_primary) || (data.contacts || [])[0];
    if (owner) {
      const name = ((owner.first_name || '') + ' ' + (owner.last_name || '')).trim() || owner.phone || owner.email || (moduleLabel('contact', 'singular') + ' #' + owner.id);
      fields.contactId.value = owner.id;
      fields.contactSearch.value = name;
      fields.contactSelected.textContent = 'Selected: ' + name;
      if (fields.primary) fields.primary.checked = !!owner.is_primary;
    } else if (defaultContactId) {
      fields.contactId.value = defaultContactId;
      fields.contactSearch.value = defaultContactLabel || '';
      fields.contactSelected.textContent =
        defaultContactLabel ? ('Selected: ' + defaultContactLabel) : '';
      if (fields.primary) fields.primary.checked = true;
    } else {
      fields.contactId.value = '';
      fields.contactSearch.value = '';
      fields.contactSelected.textContent = '';
      if (fields.primary) fields.primary.checked = true;
    }
  }

  function applyCorporatePrefill(prefill) {
    if (!prefill) return;
    if (fields.name) fields.name.value = prefill.name || '';
    if (fields.tin) fields.tin.value = prefill.tin || '';
    if (fields.cac) fields.cac.value = prefill.cac_number || '';
    if (phoneHelper) {
      phoneHelper.setFull(prefill.phone || '');
    } else if (fields.phone) {
      fields.phone.value = (prefill.phone || '').replace(/\D+/g, '');
    }
    if (fields.email) fields.email.value = prefill.email || '';
    if (fields.website) fields.website.value = prefill.website || '';
    if (fields.address) fields.address.value = prefill.address_line1 || '';
    if (fields.cityName) fields.cityName.value = prefill.city || '';
    if (fields.stateName) fields.stateName.value = prefill.state || '';
    if (fields.countryName) fields.countryName.value = prefill.country || '';
  }

  async function openCreate(defaults) {
    editingId = null;
    if (titleEl) titleEl.textContent = 'New ' + moduleLabel('organization', 'singular');
    clearForm();
    if (verificationWrap) verificationWrap.classList.toggle('d-none', !(verificationConfig && verificationConfig.enabled));
    await loadCountries();
    if (geoHelper) {
      await geoHelper.init({
        country: defaults?.country || 'Nigeria',
        state: defaults?.state || '',
        city: defaults?.city || ''
      });
    }
    if (defaults?.phone && phoneHelper) {
      phoneHelper.setFull(defaults.phone);
    }
    const cid = defaults?.contactId || defaultContactId;
    const clabel = defaults?.contactLabel || defaultContactLabel;
    if (cid && fields.contactId) {
      fields.contactId.value = cid;
      fields.contactSelected.textContent = clabel || '';
      fields.contactSearch.value = clabel || '';
    }
    modal.show();
  }

  async function openEdit(id) {
    if (!id) return;
    clearForm();
    if (verificationWrap) verificationWrap.classList.add('d-none');
    if (titleEl) titleEl.textContent = 'Edit organization';
    await loadCountries();
    try {
      const res = await fetch(apiorganizationDetail + '?id=' + encodeURIComponent(id));
      const data = await res.json();
      if (!res.ok) throw new Error(data?.error || 'Unable to load organization');
      if (geoHelper) {
        await geoHelper.init({
          country: data.country || '',
          state: data.state || '',
          city: data.city || ''
        });
      }
      fillForm(data);
      modal.show();
    } catch (err) {
      showError(err.message || 'Unable to load organization');
    }
  }

  async function save() {
    if (formAlert) { formAlert.classList.add('d-none'); formAlert.textContent = ''; }
    const saveBtn = document.getElementById('saveorganizationBtn');
    const original = saveBtn ? saveBtn.innerHTML : '';
    if (saveBtn) { saveBtn.disabled = true; saveBtn.innerHTML = '<span class=\"spinner-border spinner-border-sm me-1\"></span> Saving...'; }

    const phoneRaw = (fields.phone?.value || '').replace(/\D+/g, '');
    const phoneHidden = fields.phoneFull ? fields.phoneFull.value.trim() : '';
    const phoneFull = phoneHidden || (phoneHelper ? phoneHelper.getFull() : (phoneRaw && fields.phoneCode?.value ? (fields.phoneCode.value + phoneRaw) : phoneRaw));
    const payload = {
      name: fields.name?.value.trim() || '',
      tin: fields.tin?.value.trim() || '',
      cac_number: fields.cac?.value.trim() || '',
      phone: phoneFull || '',
      email: fields.email?.value.trim() || '',
      website: fields.website?.value.trim() || '',
      address_line1: fields.address?.value.trim() || '',
      city: fields.cityName?.value.trim() || '',
      state: fields.stateName?.value.trim() || '',
      country: fields.countryName?.value.trim() || '',
      contact_id: parseInt(fields.contactId?.value || defaultContactId || '0', 10) || null
    };
    if (personalTokenInput && personalTokenInput.value) {
      payload.identity_personal_token = personalTokenInput.value;
      if (!payload.contact_id) {
        payload.contact_id = null;
      }
    }
    if (corporateTokenInput && corporateTokenInput.value) {
      payload.identity_corporate_token = corporateTokenInput.value;
    }
    if (!payload.name) {
      showError(moduleLabel('organization', 'singular') + ' Name is required.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = original; }
      return;
    }
    if (!payload.contact_id) {
      showError('Please select an owner contact.');
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = original; }
      return;
    }

    const url = editingId ? (apiorganizationDetail + '?id=' + encodeURIComponent(editingId)) : apiorganizationsIndex;
    const method = editingId ? 'PATCH' : 'POST';
    try {
      const res = await fetch(url, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data?.error || 'Unable to save organization');
      if (!editingId) {
        const newId = data?.id_s || data?.id || (data?.data && (data.data.id_s || data.data.id));
        if (newId) {
          const viewUrl = opts.redirectView
            ? opts.redirectView
            : (window.location.pathname.indexOf('/studio/') !== -1 ? 'studio/organizations/view.kml' : 'portal/organization/view.kml');
          window.location.assign(viewUrl + '?id=' + encodeURIComponent(newId));
          return;
        }
      }

      const orgId =
        editingId
        || data?.id_s
        || data?.id
        || (data?.data && (data.data.id_s || data.data.id));

      if (orgId && payload.contact_id) {
        const fd = new FormData();
        fd.append('organization_id', orgId);
        fd.append('contact_id', payload.contact_id);
        fd.append('is_primary', fields.primary?.checked ? '1' : '0');

        fetch(apiorganizationContacts, {
          method: 'POST',
          body: fd
        }).catch(() => {
          // fail silently to avoid blocking org save
        });
      }

      if (typeof opts.onSaved === 'function') opts.onSaved(data);
      modal.hide();
    } catch (err) {
      showError(err.message || 'Unable to save organization');
    } finally {
      if (saveBtn) { saveBtn.disabled = false; saveBtn.innerHTML = original; }
    }
  }

  // Contact search wiring
  if (fields.contactSearch) {
    let timer = null;
    fields.contactSearch.addEventListener('input', () => {
      clearTimeout(timer);
      const term = fields.contactSearch.value.trim();
      timer = setTimeout(() => searchContacts(term), 250);
    });
    fields.contactSearch.addEventListener('focus', () => {
      const term = fields.contactSearch.value.trim();
      if (term.length >= 4) searchContacts(term);
    });
  }

  const saveBtn = document.getElementById('saveorganizationBtn');
  if (saveBtn) saveBtn.addEventListener('click', save);

  if (verificationWrap) {
    verificationWrap.classList.toggle('d-none', !(verificationConfig && verificationConfig.enabled));
  }
  if (window.IdentityVerification && verificationConfig && verificationConfig.enabled) {
    if (verificationConfig.organizationsEnabled !== false) {
      corporateVerificationBlock = window.IdentityVerification.bindBlock({
        root: document.getElementById('organizationCorporateVerificationBlock'),
        mode: 'agent',
        endpoint: verificationConfig.endpoint || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/identity_verification'),
        sectionType: 'corporate',
        entityMode: 'corporate',
        availableMethods: Array.isArray(verificationConfig.corporateMethods) ? verificationConfig.corporateMethods : null,
        onPrefill: function (prefill) {
          applyCorporatePrefill(prefill || {});
          if (corporateTokenInput) corporateTokenInput.value = corporateVerificationBlock ? corporateVerificationBlock.getToken() : '';
        },
        onReset: function () {
          if (corporateTokenInput) corporateTokenInput.value = '';
        }
      });
    }
  }

  return { openCreate, openEdit, clearForm };
}
