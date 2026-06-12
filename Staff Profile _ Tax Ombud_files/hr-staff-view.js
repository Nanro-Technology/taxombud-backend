/* eslint-disable */
  const hrStaffViewConfig = window.hrStaffViewConfig || {};
  const canEditFile = !!hrStaffViewConfig.canEditFile;
  const canDeleteFile = !!hrStaffViewConfig.canDeleteFile;
  const canHrManage = !!hrStaffViewConfig.canHrManage;
  const featureHrSuite = !!hrStaffViewConfig.featureHrSuite;
  const apiSupportedBanks = 'api/modules/config/supported_banks';
  const staffAlert = document.getElementById('staffAlert');
  const staffFilesList = document.getElementById('sp_files_list');
  const staffFilesAddBtn = document.getElementById('sp_files_add_btn');
  const staffFilesModalEl = document.getElementById('staffFilesModal');
  const staffFilesModal = staffFilesModalEl ? new bootstrap.Modal(staffFilesModalEl) : null;
  const staffFilesInput = document.getElementById('staff_files_input');
  const staffFilesPreview = document.getElementById('staff_files_list_preview');
  const staffFilesSaveBtn = document.getElementById('staff_files_save_btn');
  const staffFilesAlert = document.getElementById('staffFilesAlert');
  const staffDisplayName = document.getElementById('staffDisplayName');
  const staffEmail = document.getElementById('staffEmail');
  const staffProfileAvatar = document.getElementById('staffProfileAvatar');
  const staffProfilePhone = document.getElementById('staffProfilePhone');
  const staffProfileDepartment = document.getElementById('staffProfileDepartment');
  const staffProfileEmploymentType = document.getElementById('staffProfileEmploymentType');
  const staffProfileStatus = document.getElementById('staffProfileStatus');
  const staffProfilePayGrade = document.getElementById('staffProfilePayGrade');
  const bankListEl = document.getElementById('sp_bank_list');
  const bankAddBtn = document.getElementById('sp_bank_add_btn');
  const bankModalEl = document.getElementById('bankDetailModal');
  const bankModal = bankModalEl ? new bootstrap.Modal(bankModalEl) : null;
  const bankDeleteModalEl = document.getElementById('bankDeleteModal');
  const bankDeleteModal = bankDeleteModalEl ? new bootstrap.Modal(bankDeleteModalEl) : null;
  const bdModalTitle = document.getElementById('bdModalTitle');
  const bdModalAlert = document.getElementById('bdModalAlert');
  const bdIdInput = document.getElementById('bd_id');
  const bdBankName = document.getElementById('bd_bank_name');
  const bdBankCode = document.getElementById('bd_bank_code');
  const bdAccountNumber = document.getElementById('bd_account_number');
  const bdAccountName = document.getElementById('bd_account_name');
  const bdIsPrimary = document.getElementById('bd_is_primary');
  const bdSaveBtn = document.getElementById('bdSaveBtn');
  const bdDeleteName = document.getElementById('bdDeleteName');
  const bdDeleteAlert = document.getElementById('bdDeleteAlert');
  const bdDeleteConfirmBtn = document.getElementById('bdDeleteConfirmBtn');
  let supportedBanks = [];
  let bankDeleteId = 0;
  let staffProfileLoadSeq = 0;
  let staffProfileLoaded = false;
  let staffProfileSaving = false;
  const staffFields = {
    user_id: document.getElementById('sp_user_id'),
    staff_status: document.getElementById('sp_staff_status'),
    employee_code: document.getElementById('sp_employee_code'),
    title: document.getElementById('sp_title'),
    job_title: document.getElementById('sp_job_title'),
    department_id: document.getElementById('sp_department_id'),
    employment_type: document.getElementById('sp_employment_type'),
    hire_date: document.getElementById('sp_hire_date'),
    birth_date: document.getElementById('sp_birth_date'),
    education_level: document.getElementById('sp_education_level'),
    education_details: document.getElementById('sp_education_details'),
    phone_alt: document.getElementById('sp_phone_alt'),
    phone_alt_code: document.getElementById('sp_phone_alt_code'),
    phone_alt_local: document.getElementById('sp_phone_alt_local'),
    emergency_name: document.getElementById('sp_emergency_name'),
    emergency_phone: document.getElementById('sp_emergency_phone'),
    emergency_phone_code: document.getElementById('sp_emergency_phone_code'),
    emergency_phone_local: document.getElementById('sp_emergency_phone_local'),
    address_line1: document.getElementById('sp_address_line1'),
    address_line2: document.getElementById('sp_address_line2'),
    city: document.getElementById('sp_city'),
    state: document.getElementById('sp_state'),
    country: document.getElementById('sp_country'),
    next_of_kin_name: document.getElementById('sp_next_of_kin_name'),
    next_of_kin_relationship: document.getElementById('sp_next_of_kin_relationship'),
    next_of_kin_phone: document.getElementById('sp_next_of_kin_phone'),
    next_of_kin_phone_code: document.getElementById('sp_next_of_kin_phone_code'),
    next_of_kin_phone_local: document.getElementById('sp_next_of_kin_phone_local'),
    next_of_kin_address: document.getElementById('sp_next_of_kin_address')
  };

  function setPageBusy(isBusy) {
    if (typeof window.setGlobalUiLoading === 'function') {
      window.setGlobalUiLoading('hr-staff-view', !!isBusy, 30000);
      return;
    }
    const loaderEl = document.getElementById('loader');
    if (!loaderEl) return;
    loaderEl.style.display = isBusy ? 'flex' : 'none';
    loaderEl.style.pointerEvents = 'none';
  }

  function setProfileFormBusy(isBusy) {
    const form = document.getElementById('staffProfileForm');
    if (!form) return;
    form.querySelectorAll('input, select, textarea, button').forEach((el) => {
      if (el.id === 'sp_note_btn') return; // notes handled separately
      if (el.id === 'staffSaveBtn') return; // save button managed in submit flow
      el.disabled = !!isBusy;
    });
    const saveBtn = document.getElementById('staffSaveBtn');
    if (saveBtn && !staffProfileSaving) saveBtn.disabled = !!isBusy;
  }

  function digitsOnly(val) {
    return String(val || '').replace(/\D+/g, '');
  }

  function normalizeDateInput(val) {
    const raw = String(val || '').trim();
    if (!raw) return '';
    if (/^\d{4}-\d{2}-\d{2}$/.test(raw)) return raw;
    if (/^\d{4}-\d{2}-\d{2}[ T]/.test(raw)) return raw.slice(0, 10);
    const parsed = new Date(raw);
    if (!Number.isNaN(parsed.getTime())) {
      return parsed.toISOString().slice(0, 10);
    }
    return raw.slice(0, 10);
  }

  function setPhoneGroupFromFull(codeSelect, localInput, hiddenInput, full) {
    const raw = String(full || '');
    if (!raw) {
      codeSelect.value = codeSelect.value || '+234';
      localInput.value = '';
      hiddenInput.value = '';
      return;
    }
    const clean = raw.startsWith('+') ? raw : '+' + raw;
    const digits = digitsOnly(clean);
    let match = '';
    Array.from(codeSelect.options).forEach((opt) => {
      const dial = opt.value.replace('+', '');
      if (dial && digits.startsWith(dial) && dial.length > match.length) {
        match = dial;
      }
    });
    if (match) {
      codeSelect.value = '+' + match;
      localInput.value = digits.slice(match.length);
    } else {
      codeSelect.value = codeSelect.value || '+234';
      localInput.value = digits;
    }
    hiddenInput.value = codeSelect.value + localInput.value;
  }

  function applyDialConstraints(codeSelect, localInput) {
    if (!codeSelect || !localInput) return;
    const option = codeSelect.options[codeSelect.selectedIndex];
    if (!option) return;
    const minLen = option.getAttribute('data-min-length');
    const maxLen = option.getAttribute('data-max-length');
    if (maxLen) localInput.setAttribute('maxlength', maxLen);
    if (minLen) localInput.setAttribute('minlength', minLen);
    if (minLen) localInput.dataset.minLength = minLen;
    if (maxLen) localInput.dataset.maxLength = maxLen;
  }

  function bindPhoneGroup(codeSelect, localInput, hiddenInput) {
    const sync = () => {
      const code = codeSelect.value || '+234';
      applyDialConstraints(codeSelect, localInput);
      const maxLen = Number(localInput.dataset.maxLength || localInput.getAttribute('maxlength') || 0);
      let local = digitsOnly(localInput.value);
      if (maxLen > 0 && local.length > maxLen) {
        local = local.slice(0, maxLen);
      }
      localInput.value = local;
      hiddenInput.value = local ? code + local : '';
    };
    codeSelect.addEventListener('change', sync);
    localInput.addEventListener('input', sync);
    sync();
  }

  bindPhoneGroup(staffFields.phone_alt_code, staffFields.phone_alt_local, staffFields.phone_alt);
  bindPhoneGroup(staffFields.emergency_phone_code, staffFields.emergency_phone_local, staffFields.emergency_phone);
  bindPhoneGroup(staffFields.next_of_kin_phone_code, staffFields.next_of_kin_phone_local, staffFields.next_of_kin_phone);
  [staffFields.department_id, staffFields.employment_type, staffFields.staff_status, staffFields.phone_alt_code, staffFields.phone_alt_local, document.getElementById('sp_pay_grade_id')].forEach((el) => {
    if (!el) return;
    el.addEventListener('change', refreshProfileCardMeta);
    el.addEventListener('input', refreshProfileCardMeta);
  });

  function buildDepartmentMap() {
    const map = {};
    const select = document.getElementById('sp_department_id');
    if (!select) return map;
    Array.from(select.options).forEach((opt) => {
      if (!opt.value) return;
      map[String(opt.value)] = opt.textContent || opt.value;
    });
    return map;
  }

  function formatMovementValue(type, value, deptMap) {
    if (!value) return '—';
    const raw = String(value);
    if ((type === 'department' || type === 'department_id') && deptMap[raw]) {
      return deptMap[raw];
    }
    return raw;
  }

  function renderMovements(list) {
    const box = document.getElementById('sp_movements_list');
    if (!list || !list.length) {
      box.innerHTML = '<div class="text-muted">No movements yet.</div>';
      return;
    }
    const deptMap = buildDepartmentMap();
    box.innerHTML = list.map((m) => `
      <div class="mb-2">
        <div class="fw-semibold">${m.change_type}</div>
        <div class="text-muted">${formatMovementValue(m.change_type, m.old_value, deptMap)} → ${formatMovementValue(m.change_type, m.new_value, deptMap)}</div>
        <div class="text-muted">${m.created_at || ''}</div>
      </div>
    `).join('');
  }

  function renderNotes(list) {
    const box = document.getElementById('sp_notes_list');
    if (!list || !list.length) {
      box.innerHTML = '<div class="text-muted">No notes yet.</div>';
      return;
    }
    box.innerHTML = list.map((n) => `
      <div class="mb-2">
        <div>${n.note || ''}</div>
        <div class="text-muted">${n.created_at || ''}</div>
      </div>
    `).join('');
  }

  function renderProfileHeader(name, email) {
    const displayName = String(name || '').trim() || '-';
    const emailVal = String(email || '').trim() || '-';
    if (staffDisplayName) staffDisplayName.textContent = displayName;
    if (staffEmail) staffEmail.textContent = emailVal;
    if (staffProfileAvatar) {
      const parts = displayName === '-' ? [] : displayName.split(/\s+/).filter(Boolean);
      const initials = parts.length
        ? (parts[0].slice(0, 1) + (parts[1] ? parts[1].slice(0, 1) : '')).toUpperCase()
        : '-';
      staffProfileAvatar.textContent = initials || '-';
    }
  }

  function hideElAlert(el) {
    if (!el) return;
    el.classList.add('d-none');
    el.textContent = '';
  }

  function showElAlert(el, msg) {
    if (!el) return;
    el.textContent = msg || 'Unable to complete request';
    el.classList.remove('d-none');
  }

  function setBtnBusy(btn, busy) {
    if (!btn) return;
    const sp = btn.querySelector('.spinner-border');
    const lbl = btn.querySelector('.label');
    btn.disabled = !!busy;
    if (sp) sp.classList.toggle('d-none', !busy);
    if (lbl) lbl.style.display = busy ? 'none' : '';
  }

  function initBankSelect() {
    if (!bdBankName || typeof window.jQuery === 'undefined' || !window.jQuery.fn || !window.jQuery.fn.select2) return;
    const $bank = window.jQuery(bdBankName);
    if (!$bank.data('select2')) {
      $bank.select2({
        width: '100%',
        dropdownParent: window.jQuery('#bankDetailModal'),
        placeholder: bdBankName.getAttribute('data-placeholder') || 'Select bank',
        allowClear: true,
      });
    }
  }

  function fillBankSelectOptions(selectedValue) {
    if (!bdBankName) return;
    const selected = String(selectedValue || '').trim();
    let html = '<option value="">Select bank</option>';
    const seen = new Set();
    (supportedBanks || []).forEach((b) => {
      const label = String((b && (b.label || b.name || b.code)) || '').trim();
      if (!label) return;
      const key = label.toLowerCase();
      if (seen.has(key)) return;
      seen.add(key);
      const isSel = selected !== '' && label === selected ? ' selected' : '';
      html += '<option value="' + label.replace(/"/g, '&quot;') + '"' + isSel + '>' + label + '</option>';
    });
    if (selected && !seen.has(selected.toLowerCase())) {
      html += '<option value="' + selected.replace(/"/g, '&quot;') + '" selected>' + selected + '</option>';
    }
    bdBankName.innerHTML = html;
    if (typeof window.jQuery !== 'undefined' && window.jQuery.fn && window.jQuery.fn.select2) {
      window.jQuery(bdBankName).trigger('change.select2');
    }
  }

  async function loadSupportedBanks() {
    if (!featureHrSuite || !bdBankName) return [];
    try {
      const res = await fetch(apiSupportedBanks, { credentials: 'same-origin' });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || data.message || 'Unable to load supported banks');
      supportedBanks = Array.isArray(data.data) ? data.data : [];
    } catch (err) {
      supportedBanks = [];
    }
    fillBankSelectOptions((bdBankName && bdBankName.value) || '');
    return supportedBanks;
  }

  function resetBankForm() {
    if (bdIdInput) bdIdInput.value = '';
    fillBankSelectOptions('');
    if (bdBankName) bdBankName.value = '';
    if (typeof window.jQuery !== 'undefined' && window.jQuery.fn && window.jQuery.fn.select2 && bdBankName) {
      window.jQuery(bdBankName).val('').trigger('change');
    }
    if (bdBankCode) bdBankCode.value = '';
    if (bdAccountNumber) bdAccountNumber.value = '';
    if (bdAccountName) bdAccountName.value = '';
    if (bdIsPrimary) bdIsPrimary.checked = false;
    if (bdModalTitle) bdModalTitle.textContent = 'Add Bank Detail';
    hideElAlert(bdModalAlert);
  }

  function renderBankDetails(list) {
    if (!bankListEl) return;
    if (!Array.isArray(list) || !list.length) {
      bankListEl.innerHTML = '<div class="text-muted">No bank details.</div>';
      return;
    }
    bankListEl.innerHTML = list.map((b) => {
      const name = String(b.bank_name || '').trim() || 'Bank';
      const acctName = String(b.account_name || '').trim() || '-';
      const acctNo = String(b.account_number || '').trim();
      const acctMasked = acctNo ? ('***' + acctNo.slice(-4)) : '-';
      const code = String(b.bank_code || '').trim();
      const primaryBadge = Number(b.is_primary || 0) === 1 ? '<span class="badge bg-success-subtle text-success">Primary</span>' : '';
      const actions = canHrManage
        ? `<div class="d-flex gap-1">
            <button type="button" class="btn btn-sm btn-soft-primary js-bank-edit" data-id="${Number(b.id || 0)}" title="Edit"><i class="ri-pencil-line"></i></button>
            <button type="button" class="btn btn-sm btn-soft-danger js-bank-delete" data-id="${Number(b.id || 0)}" data-name="${name.replace(/"/g, '&quot;')}" title="Delete"><i class="ri-delete-bin-line"></i></button>
          </div>`
        : '';
      return `<div class="border rounded p-2 d-flex justify-content-between align-items-start gap-2">
        <div>
          <div class="fw-semibold">${name} ${primaryBadge}</div>
          <div class="text-muted">${acctName}</div>
          <div class="text-muted">${acctMasked}${code ? ' • ' + code : ''}</div>
        </div>
        ${actions}
      </div>`;
    }).join('');
  }

  async function loadBankDetails(userId) {
    if (!featureHrSuite || !bankListEl || !userId) return;
    bankListEl.innerHTML = '<div class="text-muted">Loading bank details…</div>';
    try {
      const res = await fetch(`api/modules/hr/bank_details?user_id=${encodeURIComponent(userId)}`, { credentials: 'same-origin' });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || data.message || 'Unable to load bank details');
      renderBankDetails(data.data || []);
    } catch (err) {
      bankListEl.innerHTML = `<div class="text-danger small">${String(err.message || 'Unable to load bank details.')}</div>`;
    }
  }

  async function saveBankDetail() {
    const userId = Number(staffFields.user_id && staffFields.user_id.value || 0);
    if (!userId) return;
    hideElAlert(bdModalAlert);
    const payload = {
      user_id: userId,
      bank_name: String((bdBankName && bdBankName.value) || '').trim(),
      bank_code: String((bdBankCode && bdBankCode.value) || '').trim(),
      account_number: String((bdAccountNumber && bdAccountNumber.value) || '').trim(),
      account_name: String((bdAccountName && bdAccountName.value) || '').trim(),
      is_primary: !!(bdIsPrimary && bdIsPrimary.checked),
    };
    if (!payload.bank_name || !payload.account_number || !payload.account_name) {
      showElAlert(bdModalAlert, 'Bank name, account number and account name are required.');
      return;
    }
    const editId = Number((bdIdInput && bdIdInput.value) || 0);
    const method = editId > 0 ? 'PATCH' : 'POST';
    const url = editId > 0 ? `api/modules/hr/bank_details?id=${encodeURIComponent(editId)}` : 'api/modules/hr/bank_details';
    setBtnBusy(bdSaveBtn, true);
    try {
      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin',
        body: JSON.stringify(payload),
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || data.message || 'Unable to save bank detail');
      if (bankModal) bankModal.hide();
      await loadBankDetails(userId);
    } catch (err) {
      showElAlert(bdModalAlert, String(err.message || 'Unable to save bank detail'));
    } finally {
      setBtnBusy(bdSaveBtn, false);
    }
  }

  async function deleteBankDetail() {
    const userId = Number(staffFields.user_id && staffFields.user_id.value || 0);
    if (!bankDeleteId || !userId) return;
    setBtnBusy(bdDeleteConfirmBtn, true);
    hideElAlert(bdDeleteAlert);
    try {
      const res = await fetch(`api/modules/hr/bank_details?id=${encodeURIComponent(bankDeleteId)}`, {
        method: 'DELETE',
        credentials: 'same-origin',
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || data.message || 'Unable to delete bank detail');
      if (bankDeleteModal) bankDeleteModal.hide();
      bankDeleteId = 0;
      await loadBankDetails(userId);
    } catch (err) {
      showElAlert(bdDeleteAlert, String(err.message || 'Unable to delete bank detail'));
    } finally {
      setBtnBusy(bdDeleteConfirmBtn, false);
    }
  }

  function _prettyLabel(v) {
    const s = String(v || '').trim();
    if (!s) return '-';
    return s.replace(/_/g, ' ').replace(/\b\w/g, (m) => m.toUpperCase());
  }

  function _selectedText(selectEl) {
    if (!selectEl) return '-';
    const idx = selectEl.selectedIndex;
    const opt = (idx >= 0 && selectEl.options) ? selectEl.options[idx] : null;
    const txt = opt ? String(opt.textContent || '').trim() : '';
    return txt || '-';
  }

  function refreshProfileCardMeta() {
    if (staffProfilePhone) {
      staffProfilePhone.textContent = String((staffFields.phone_alt && staffFields.phone_alt.value) || '').trim() || '-';
    }
    if (staffProfileDepartment) {
      staffProfileDepartment.textContent = _selectedText(staffFields.department_id);
    }
    if (staffProfileEmploymentType) {
      staffProfileEmploymentType.textContent = _prettyLabel(staffFields.employment_type && staffFields.employment_type.value);
    }
    if (staffProfileStatus) {
      staffProfileStatus.textContent = _prettyLabel(staffFields.staff_status && staffFields.staff_status.value);
    }
    if (staffProfilePayGrade) {
      const pgSel = document.getElementById('sp_pay_grade_id');
      staffProfilePayGrade.textContent = pgSel ? _selectedText(pgSel) : '-';
    }
  }

  function buildFileUrl(path) {
    if (!path) return '';
    const base = (typeof url_root !== 'undefined') ? url_root : '/';
    return base.replace(/\/+$/, '/') + 'storage/' + path.replace(/^\/+/, '');
  }

  function renderStaffFiles(list) {
    if (!staffFilesList) return;
    if (!list || !list.length) {
      staffFilesList.innerHTML = '<div class="text-muted">No files attached.</div>';
      return;
    }
    staffFilesList.innerHTML = list.map((f) => {
      const name = f.file_name || 'File';
      const size = f.size_bytes ? `${(f.size_bytes / (1024 * 1024)).toFixed(1)}MB` : '';
      const iconClass = f.mime_type && f.mime_type.includes('pdf') ? 'ri-file-pdf-line' : 'ri-folder-zip-line';
      const downloadHref = `api/modules/files/download?id=${encodeURIComponent(f.id)}`;
      const publicUrl = f.storage_path ? buildFileUrl(f.storage_path) : '';
      const viewInfo = (typeof resolveFileViewUrl === 'function')
        ? resolveFileViewUrl(f.file_name || '', publicUrl, downloadHref)
        : { url: publicUrl || downloadHref, type: 'other' };
      const openHref = viewInfo.url || downloadHref;
      const extSource = f.file_name || f.storage_path || '';
      const ext = (typeof fileExtensionFromName === 'function')
        ? fileExtensionFromName(extSource)
        : (extSource.split('.').pop() || '').toLowerCase();
      const isOffice = ['doc','docx','xls','xlsx','ppt','pptx'].includes(ext);
      const editItem = (canEditFile && isOffice)
        ? `<li><a class="dropdown-item js-file-edit" href="javascript:void(0);" data-id="${f.id}"><i class="ri-pencil-line align-bottom me-2 text-muted"></i>Edit</a></li>`
        : '';
      const deleteItem = canDeleteFile
        ? `<li><a class="dropdown-item text-danger js-staff-file-delete" href="javascript:void(0);" data-id="${f.id}"><i class="ri-delete-bin-line align-bottom me-2 text-muted"></i>Delete</a></li>`
        : '';
      const downloadItem = `<li><a class="dropdown-item" href="${downloadHref}" target="_blank" rel="noopener"><i class="ri-download-2-line align-bottom me-2 text-muted"></i>Download</a></li>`;
      return `
        <div class="border rounded border-dashed p-2">
          <div class="d-flex align-items-center">
            <div class="flex-shrink-0 me-3">
              <div class="avatar-sm">
                <div class="avatar-title bg-light text-secondary rounded fs-24">
                  <i class="${iconClass}"></i>
                </div>
              </div>
            </div>
            <div class="flex-grow-1 overflow-hidden">
              <h5 class="fs-13 mb-1">
                <a href="${openHref}" class="text-body text-truncate d-block" target="_blank" rel="noopener">${name}</a>
              </h5>
              <div>${size || '-'}</div>
            </div>
            <div class="flex-shrink-0 ms-2">
              <div class="d-flex gap-1">
                <a class="btn btn-icon text-muted btn-sm fs-18 material-shadow-none js-file-view" href="${openHref}" data-view-url="${openHref}" data-view-type="${viewInfo.type}" target="_blank" rel="noopener" title="View">
                  <i class="ri-eye-line"></i>
                </a>
                <div class="dropdown">
                  <button class="btn btn-icon text-muted btn-sm fs-18 dropdown material-shadow-none" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="ri-more-fill"></i>
                  </button>
                  <ul class="dropdown-menu dropdown-menu-end">
                    ${editItem}
                    ${downloadItem}
                    ${deleteItem}
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>`;
    }).join('');
  }

  async function loadStaffProfile(userId) {
    const reqSeq = ++staffProfileLoadSeq;
    setPageBusy(true);
    setProfileFormBusy(true);
    try {
      const res = await fetch(`api/modules/hr/staff?id=${userId}`, {credentials:'same-origin'});
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || 'Unable to load staff profile');
      if (reqSeq !== staffProfileLoadSeq) return;
      staffAlert.classList.add('d-none');
      renderProfileHeader(data.display_name, data.email);
      staffFields.user_id.value = data.id || '';
      staffFields.staff_status.value = data.staff_status || 'active';
      if (staffFields.employee_code) staffFields.employee_code.value = data.employee_code || '';
      if (staffFields.title) staffFields.title.value = data.title || '';
      staffFields.job_title.value = data.job_title || '';
      staffFields.department_id.value = data.department_id || '';
      staffFields.employment_type.value = data.employment_type || '';
      const pgSel = document.getElementById('sp_pay_grade_id');
      if (pgSel) pgSel.value = data.pay_grade_id || '';
      const supIdEl = document.getElementById('sp_supervisor_user_id');
      const supNameEl = document.getElementById('sp_supervisor_name');
      const supHintEl = document.getElementById('sp_supervisor_hint');
      if (supIdEl) supIdEl.value = data.supervisor_user_id || '';
      if (supNameEl) supNameEl.value = data.supervisor_name || '';
      if (supHintEl) supHintEl.textContent = data.supervisor_name ? 'Supervisor set from saved profile' : 'Auto-set when you pick a department';
      staffFields.hire_date.value = normalizeDateInput(data.hire_date);
      staffFields.birth_date.value = normalizeDateInput(data.birth_date);
      staffFields.education_level.value = data.education_level || '';
      staffFields.education_details.value = data.education_details || '';
      setPhoneGroupFromFull(staffFields.phone_alt_code, staffFields.phone_alt_local, staffFields.phone_alt, data.phone_alt || '');
      staffFields.address_line1.value = data.address_line1 || '';
      staffFields.address_line2.value = data.address_line2 || '';
      staffFields.city.value = data.city || '';
      staffFields.state.value = data.state || '';
      staffFields.country.value = data.country || '';
      staffFields.emergency_name.value = data.emergency_contact_name || '';
      setPhoneGroupFromFull(staffFields.emergency_phone_code, staffFields.emergency_phone_local, staffFields.emergency_phone, data.emergency_contact_phone || '');
      staffFields.next_of_kin_name.value = data.next_of_kin_name || '';
      staffFields.next_of_kin_relationship.value = (data.next_of_kin_relationship || '').toLowerCase();
      setPhoneGroupFromFull(staffFields.next_of_kin_phone_code, staffFields.next_of_kin_phone_local, staffFields.next_of_kin_phone, data.next_of_kin_phone || '');
      staffFields.next_of_kin_address.value = data.next_of_kin_address || '';
      renderMovements(data.movements || []);
      renderNotes(data.notes || []);
      renderStaffFiles(data.staff_files || []);
      await loadBankDetails(data.id || staffFields.user_id.value);
      refreshProfileCardMeta();
      staffProfileLoaded = true;
    } catch (err) {
      if (reqSeq !== staffProfileLoadSeq) return;
      staffAlert.textContent = err.message || 'Unable to load staff profile';
      staffAlert.classList.remove('d-none');
    } finally {
      if (reqSeq === staffProfileLoadSeq) {
        setPageBusy(false);
        setProfileFormBusy(false);
      }
    }
  }

  document.getElementById('staffProfileForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!staffProfileLoaded) {
      staffAlert.textContent = 'Please wait for the staff profile to finish loading, then save again.';
      staffAlert.classList.remove('d-none');
      return;
    }
    if (staffProfileSaving) return;
    staffProfileSaving = true;
    setPageBusy(true);
    const saveBtn = document.getElementById('staffSaveBtn');
    const successEl = document.getElementById('staffSuccess');
    staffAlert.classList.add('d-none');
    successEl.classList.add('d-none');
    if (saveBtn) {
      saveBtn.disabled = true;
      saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Saving...';
    }
    const payload = {
      user_id: staffFields.user_id.value,
      staff_status: staffFields.staff_status.value,
      employee_code: staffFields.employee_code ? staffFields.employee_code.value.trim() : '',
      title: staffFields.title ? staffFields.title.value.trim() : undefined,
      job_title: staffFields.job_title.value.trim(),
      department_id: staffFields.department_id.value,
      employment_type: staffFields.employment_type.value,
      hire_date: staffFields.hire_date.value,
      birth_date: staffFields.birth_date.value,
      education_level: staffFields.education_level.value,
      education_details: staffFields.education_details.value.trim(),
      phone_alt: staffFields.phone_alt.value.trim(),
      address_line1: staffFields.address_line1.value.trim(),
      address_line2: staffFields.address_line2.value.trim(),
      city: staffFields.city.value.trim(),
      state: staffFields.state.value.trim(),
      country: staffFields.country.value.trim(),
      emergency_contact_name: staffFields.emergency_name.value.trim(),
      emergency_contact_phone: staffFields.emergency_phone.value.trim(),
      next_of_kin_name: staffFields.next_of_kin_name.value.trim(),
      next_of_kin_relationship: staffFields.next_of_kin_relationship.value.trim(),
      next_of_kin_phone: staffFields.next_of_kin_phone.value.trim(),
      next_of_kin_address: staffFields.next_of_kin_address.value.trim(),
      pay_grade_id: (document.getElementById('sp_pay_grade_id') || {}).value || null,
      supervisor_user_id: (document.getElementById('sp_supervisor_user_id') || {}).value || null
    };
    try {
      const res = await fetch('api/modules/hr/staff', {
        method: 'PATCH',
        headers: {'Content-Type': 'application/json'},
        credentials: 'same-origin',
        body: JSON.stringify(payload)
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.message || 'Unable to save');
      await loadStaffProfile(staffFields.user_id.value);
      successEl.classList.remove('d-none');
      setTimeout(() => successEl.classList.add('d-none'), 3500);
    } catch (err) {
      staffAlert.textContent = err.message || 'Unable to save';
      staffAlert.classList.remove('d-none');
    } finally {
      staffProfileSaving = false;
      setPageBusy(false);
      if (saveBtn) {
        saveBtn.disabled = false;
        saveBtn.innerHTML = '<i class="ri-save-3-line me-1"></i>Save Profile';
      }
    }
  });

  document.getElementById('sp_note_btn').addEventListener('click', async () => {
    const noteInput = document.getElementById('sp_note');
    const note = noteInput.value.trim();
    if (!note) return;
    setPageBusy(true);
    staffAlert.classList.add('d-none');
    try {
      const res = await fetch('api/modules/hr/staff', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        credentials: 'same-origin',
        body: JSON.stringify({
          user_id: staffFields.user_id.value,
          note
        })
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.message || 'Unable to save note');
      noteInput.value = '';
      await loadStaffProfile(staffFields.user_id.value);
    } catch (err) {
      staffAlert.textContent = err.message || 'Unable to save note';
      staffAlert.classList.remove('d-none');
    } finally {
      setPageBusy(false);
    }
  });

  function renderStaffFilePreview(files) {
    if (!staffFilesPreview) return;
    if (!files.length) {
      staffFilesPreview.textContent = 'No files selected';
      return;
    }
    staffFilesPreview.innerHTML = '';
    files.forEach((f) => {
      const div = document.createElement('div');
      div.textContent = `${f.name} (${Math.round(f.size / 1024)} KB)`;
      staffFilesPreview.appendChild(div);
    });
  }

  async function uploadStaffFiles(files) {
    if (!files.length) return [];
    const newIds = [];
    for (const f of files) {
      const fd = new FormData();
      fd.append('file', f);
      const res = await fetch('api/modules/files/index', { method: 'POST', body: fd });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || data.message || 'Upload failed');
      if (data.id) newIds.push(data.id);
    }
    return newIds;
  }

  async function attachStaffFiles() {
    const files = Array.from(staffFilesInput?.files || []);
    if (!files.length) {
      staffFilesAlert.textContent = 'Please select files to upload.';
      staffFilesAlert.classList.remove('d-none');
      return;
    }
    staffFilesAlert.classList.add('d-none');
    staffFilesSaveBtn.querySelector('.spinner-border').classList.remove('d-none');
    staffFilesSaveBtn.querySelector('.label').classList.add('d-none');
    try {
      const ids = await uploadStaffFiles(files);
      const res = await fetch('api/modules/hr/staff', {
        method: 'PATCH',
        headers: {'Content-Type': 'application/json'},
        credentials: 'same-origin',
        body: JSON.stringify({ user_id: staffFields.user_id.value, file_ids: ids })
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.message || 'Unable to attach files');
      staffFilesInput.value = '';
      renderStaffFilePreview([]);
      if (staffFilesModal) staffFilesModal.hide();
      await loadStaffProfile(staffFields.user_id.value);
    } catch (err) {
      staffFilesAlert.textContent = err.message || 'Unable to attach files';
      staffFilesAlert.classList.remove('d-none');
    } finally {
      staffFilesSaveBtn.querySelector('.spinner-border').classList.add('d-none');
      staffFilesSaveBtn.querySelector('.label').classList.remove('d-none');
    }
  }

  if (staffFilesAddBtn) {
    staffFilesAddBtn.addEventListener('click', () => {
      if (staffFilesModal) staffFilesModal.show();
    });
  }
  if (staffFilesInput) {
    staffFilesInput.addEventListener('change', () => renderStaffFilePreview(Array.from(staffFilesInput.files || [])));
  }
  if (staffFilesSaveBtn) {
    staffFilesSaveBtn.addEventListener('click', attachStaffFiles);
  }
  if (staffFilesList) {
    staffFilesList.addEventListener('click', async (e) => {
      const btn = e.target.closest('.js-staff-file-delete');
      if (!btn) return;
      const fid = parseInt(btn.getAttribute('data-id'), 10);
      if (!fid || !staffFields.user_id.value) return;
      const ok = await window.crmUiConfirm('Remove this file from the staff profile?', 'Remove Staff File', {
        okText: 'Remove',
        cancelText: 'Cancel',
        variant: 'danger',
        icon: 'warning'
      });
      if (!ok) return;
      try {
        const res = await fetch('api/modules/hr/staff', {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          credentials: 'same-origin',
          body: JSON.stringify({
            user_id: staffFields.user_id.value,
            remove_file_ids: [fid]
          })
        });
        const data = await res.json().catch(() => ({}));
        if (!res.ok) throw new Error(data.message || 'Unable to remove file');
        await loadStaffProfile(staffFields.user_id.value);
      } catch (err) {
        staffFilesAlert.textContent = err.message || 'Unable to remove file';
        staffFilesAlert.classList.remove('d-none');
      }
    });
  }

  if (bankAddBtn) {
    bankAddBtn.addEventListener('click', () => {
      resetBankForm();
      if (bankModal) bankModal.show();
    });
  }

  if (bdSaveBtn) {
    bdSaveBtn.addEventListener('click', saveBankDetail);
  }

  if (bankListEl) {
    bankListEl.addEventListener('click', async (e) => {
      const editBtn = e.target.closest('.js-bank-edit');
      const deleteBtn = e.target.closest('.js-bank-delete');
      const userId = Number(staffFields.user_id && staffFields.user_id.value || 0);
      if (editBtn) {
        const id = Number(editBtn.getAttribute('data-id') || 0);
        if (!id || !userId) return;
        resetBankForm();
        if (bdModalTitle) bdModalTitle.textContent = 'Edit Bank Detail';
        try {
          const res = await fetch(`api/modules/hr/bank_details?user_id=${encodeURIComponent(userId)}`, { credentials: 'same-origin' });
          const data = await res.json().catch(() => ({}));
          if (!res.ok) throw new Error(data.error || data.message || 'Unable to load bank details');
          const row = (data.data || []).find((x) => Number(x.id || 0) === id);
          if (!row) throw new Error('Bank detail not found');
          if (bdIdInput) bdIdInput.value = String(row.id || '');
          fillBankSelectOptions(row.bank_name || '');
          if (bdBankName) bdBankName.value = row.bank_name || '';
          if (typeof window.jQuery !== 'undefined' && window.jQuery.fn && window.jQuery.fn.select2 && bdBankName) {
            window.jQuery(bdBankName).val(row.bank_name || '').trigger('change');
          }
          if (bdBankCode) bdBankCode.value = row.bank_code || '';
          if (bdAccountNumber) bdAccountNumber.value = row.account_number || '';
          if (bdAccountName) bdAccountName.value = row.account_name || '';
          if (bdIsPrimary) bdIsPrimary.checked = Number(row.is_primary || 0) === 1;
          if (bankModal) bankModal.show();
        } catch (err) {
          showElAlert(staffAlert, String(err.message || 'Unable to load bank detail'));
        }
      }
      if (deleteBtn) {
        const id = Number(deleteBtn.getAttribute('data-id') || 0);
        if (!id) return;
        bankDeleteId = id;
        if (bdDeleteName) bdDeleteName.textContent = deleteBtn.getAttribute('data-name') || 'bank detail';
        hideElAlert(bdDeleteAlert);
        if (bankDeleteModal) bankDeleteModal.show();
      }
    });
  }

  if (bdDeleteConfirmBtn) {
    bdDeleteConfirmBtn.addEventListener('click', deleteBankDetail);
  }

  initBankSelect();
  loadSupportedBanks();

  // Department change → auto-set supervisor from dept head
  if (staffFields.department_id) {
    staffFields.department_id.addEventListener('change', async () => {
      const deptId = staffFields.department_id.value;
      const supIdEl = document.getElementById('sp_supervisor_user_id');
      const supNameEl = document.getElementById('sp_supervisor_name');
      const supHintEl = document.getElementById('sp_supervisor_hint');
      if (!deptId) {
        if (supIdEl) supIdEl.value = '';
        if (supNameEl) supNameEl.value = '';
        if (supHintEl) supHintEl.textContent = '';
        return;
      }
      try {
        const res = await fetch(`api/modules/departments/detail?id=${deptId}`, {credentials: 'same-origin'});
        const dept = await res.json();
        if (res.ok && dept.head_user_id && String(dept.head_user_id) !== String(hrStaffViewConfig.userId)) {
          if (supIdEl) supIdEl.value = dept.head_user_id;
          if (supNameEl) supNameEl.value = dept.head_name || '';
          if (supHintEl) supHintEl.textContent = 'Auto-set from department head';
        } else {
          if (supIdEl) supIdEl.value = '';
          if (supNameEl) supNameEl.value = '';
          if (supHintEl) supHintEl.textContent = 'No department head assigned';
        }
      } catch (e) { /* ignore */ }
    });
  }

  // Supervisor clear button
  const supClearBtn = document.getElementById('sp_supervisor_clear');
  if (supClearBtn) {
    supClearBtn.addEventListener('click', () => {
      const supIdEl = document.getElementById('sp_supervisor_user_id');
      const supNameEl = document.getElementById('sp_supervisor_name');
      const supHintEl = document.getElementById('sp_supervisor_hint');
      if (supIdEl) supIdEl.value = '';
      if (supNameEl) supNameEl.value = '';
      if (supHintEl) supHintEl.textContent = '';
    });
  }

  const staffUserId = hrStaffViewConfig.userId || null;
  if (!staffUserId) {
    staffAlert.textContent = 'Missing user_id';
    staffAlert.classList.remove('d-none');
  } else {
    refreshProfileCardMeta();
    loadStaffProfile(staffUserId);
  }
