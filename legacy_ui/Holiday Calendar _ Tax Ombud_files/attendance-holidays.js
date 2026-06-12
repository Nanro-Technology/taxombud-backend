(function () {
  const cfg = window.hrAttendanceHolidaysConfig || {};
  const csrf = cfg.csrf || '';
  const departments = Array.isArray(cfg.departments) ? cfg.departments : [];
  const canManage = cfg.canManage === true;
  let holidays = [];

  function esc(v) {
    return String(v == null ? '' : v)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function showAlert(type, message) {
    const host = document.getElementById('attendanceHolidaysAlert');
    if (!host) return;
    host.innerHTML = message ? '<div class="alert alert-' + esc(type) + ' py-2 mb-3">' + esc(message) + '</div>' : '';
  }

  async function fetchJson(url, options) {
    const res = await fetch(url, Object.assign({ credentials: 'same-origin' }, options || {}));
    let data = null;
    try { data = await res.json(); } catch (e) { data = null; }
    if (!res.ok) throw new Error((data && (data.error || data.message)) || ('Request failed (' + res.status + ')'));
    return data;
  }

  function fillDepartmentSelect() {
    const select = document.getElementById('attendanceHolidayDepartment');
    if (!select) return;
    select.innerHTML = '<option value="">Select department</option>' + departments.map((row) => '<option value="' + esc(row.id) + '">' + esc(row.name || '') + '</option>').join('');
  }

  function getHolidayMode() {
    const sel = document.getElementById('attendanceHolidayMode');
    return (sel && sel.value === 'date_range') ? 'date_range' : 'full_day';
  }

  function toggleHolidayModeFields() {
    const mode = getHolidayMode();
    const endWrap = document.getElementById('attendanceHolidayEndDateWrap');
    const endInput = document.getElementById('attendanceHolidayEndDate');
    if (endInput) {
      endInput.required = mode === 'date_range';
      endInput.readOnly = mode !== 'date_range';
      endInput.classList.toggle('bg-light', mode !== 'date_range');
      if (mode !== 'date_range') {
        endInput.value = document.getElementById('attendanceHolidayStartDate').value || endInput.value || '';
      }
    }
    if (endWrap) {
      endWrap.classList.remove('d-none');
    }
  }

  function toggleDepartmentField() {
    const appliesTo = document.getElementById('attendanceHolidayAppliesTo').value || 'all';
    document.getElementById('attendanceHolidayDepartmentWrap').classList.toggle('d-none', appliesTo !== 'department');
  }

  function renderTable() {
    const tableId = '#attendanceHolidaysTable';
    if ($.fn.DataTable.isDataTable(tableId)) {
      $(tableId).DataTable().clear().destroy();
    }
    const tbody = document.querySelector(tableId + ' tbody');
    tbody.innerHTML = holidays.map((row, idx) => '<tr>'
      + '<td>' + esc(idx + 1) + '</td>'
      + '<td>' + esc(row.holiday_start_date || row.holiday_date || '-') + '</td>'
      + '<td>' + esc(row.holiday_end_date || row.holiday_start_date || row.holiday_date || '-') + '</td>'
      + '<td><span class="badge ' + esc((row.holiday_mode || 'full_day') === 'date_range' ? 'bg-warning-subtle text-warning' : 'bg-success-subtle text-success') + '">' + esc((row.holiday_mode || 'full_day') === 'date_range' ? 'Date Range' : 'Full Day') + '</span></td>'
      + '<td>' + esc(row.name || '-') + '</td>'
      + '<td>' + esc(row.holiday_type || '-') + '</td>'
      + '<td>' + esc(row.applies_to || '-') + '</td>'
      + '<td>' + esc(row.department_name || '-') + '</td>'
      + (canManage ? '<td><div class="d-flex gap-2"><button type="button" class="btn btn-soft-primary btn-sm js-edit-holiday" data-id="' + esc(row.id) + '"><i class="ri-edit-line me-1"></i>Edit</button><button type="button" class="btn btn-soft-danger btn-sm js-delete-holiday" data-id="' + esc(row.id) + '"><i class="ri-delete-bin-line me-1"></i>Delete</button></div></td>' : '')
      + '</tr>').join('');
    $(tableId).DataTable({
      destroy: true,
      pageLength: 15,
      order: [[1, 'asc']],
      language: { emptyTable: 'No holidays found for this year.' }
    });
  }

  async function loadHolidays() {
    showAlert('', '');
    try {
      const year = document.getElementById('attendanceHolidayYear').value || '';
      const data = await fetchJson('api/modules/hr/attendance_holidays?year=' + encodeURIComponent(year));
      holidays = Array.isArray(data.data) ? data.data : [];
      renderTable();
    } catch (e) {
      showAlert('danger', e.message || 'Unable to load holidays.');
    }
  }

  function resetForm() {
    document.getElementById('attendanceHolidayId').value = '';
    document.getElementById('attendanceHolidayName').value = '';
    document.getElementById('attendanceHolidayMode').value = 'full_day';
    document.getElementById('attendanceHolidayStartDate').value = '';
    document.getElementById('attendanceHolidayEndDate').value = '';
    document.getElementById('attendanceHolidayType').value = 'public';
    document.getElementById('attendanceHolidayAppliesTo').value = 'all';
    document.getElementById('attendanceHolidayDepartment').value = '';
    toggleHolidayModeFields();
    toggleDepartmentField();
  }

  async function saveHoliday() {
    const id = document.getElementById('attendanceHolidayId').value || '';
    const mode = getHolidayMode();
    const startDate = document.getElementById('attendanceHolidayStartDate').value || '';
    const endDate = document.getElementById('attendanceHolidayEndDate').value || '';
    const payload = {
      name: document.getElementById('attendanceHolidayName').value || '',
      holiday_mode: mode,
      holiday_start_date: startDate,
      holiday_end_date: mode === 'date_range' ? endDate : startDate,
      holiday_date: startDate,
      holiday_type: document.getElementById('attendanceHolidayType').value || 'public',
      applies_to: document.getElementById('attendanceHolidayAppliesTo').value || 'all',
      department_id: document.getElementById('attendanceHolidayDepartment').value || ''
    };
    if (!payload.name || !payload.holiday_start_date) {
      showAlert('danger', 'Holiday name and start date are required.');
      return;
    }
    if (mode === 'date_range' && !payload.holiday_end_date) {
      showAlert('danger', 'Holiday end date is required for date ranges.');
      return;
    }
    if (payload.applies_to !== 'department') {
      payload.department_id = '';
    }
    if (id) payload.id = id;
    try {
      await fetchJson('api/modules/hr/attendance_holidays', {
        method: id ? 'PATCH' : 'POST',
        headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrf },
        body: JSON.stringify(payload)
      });
      bootstrap.Modal.getInstance(document.getElementById('attendanceHolidayModal')).hide();
      showAlert('success', 'Holiday saved successfully.');
      await loadHolidays();
    } catch (e) {
      showAlert('danger', e.message || 'Unable to save holiday.');
    }
  }

  async function deleteHoliday(id) {
    if (!window.crmUiConfirm) return;
    const confirmed = await window.crmUiConfirm({
      title: 'Delete Holiday',
      text: 'This holiday will be removed from the calendar.',
      confirmButtonText: 'Delete',
      confirmButtonClass: 'btn btn-danger btn-md'
    });
    if (!confirmed) return;
    try {
      await fetchJson('api/modules/hr/attendance_holidays?id=' + encodeURIComponent(id), {
        method: 'DELETE',
        headers: { 'X-CSRF-Token': csrf }
      });
      showAlert('success', 'Holiday removed.');
      await loadHolidays();
    } catch (e) {
      showAlert('danger', e.message || 'Unable to delete holiday.');
    }
  }

  function bindEvents() {
    const elMode = document.getElementById('attendanceHolidayMode');
    if (elMode) elMode.addEventListener('change', toggleHolidayModeFields);
    const elStartDate = document.getElementById('attendanceHolidayStartDate');
    if (elStartDate) elStartDate.addEventListener('change', () => {
      if (getHolidayMode() !== 'date_range') {
        const endDate = document.getElementById('attendanceHolidayEndDate');
        if (endDate) endDate.value = elStartDate.value || '';
      }
    });
    const elAppliesTo = document.getElementById('attendanceHolidayAppliesTo');
    if (elAppliesTo) elAppliesTo.addEventListener('change', toggleDepartmentField);
    const elAddBtn = document.getElementById('btnOpenAttendanceHolidayModal');
    if (elAddBtn) elAddBtn.addEventListener('click', () => {
      resetForm();
      bootstrap.Modal.getOrCreateInstance(document.getElementById('attendanceHolidayModal')).show();
    });
    const elRefresh = document.getElementById('btnRefreshAttendanceHolidays');
    if (elRefresh) elRefresh.addEventListener('click', loadHolidays);
    const elSave = document.getElementById('btnSaveAttendanceHoliday');
    if (elSave) elSave.addEventListener('click', saveHoliday);
    document.addEventListener('click', (e) => {
      const editBtn = e.target.closest('.js-edit-holiday');
      if (editBtn) {
        const row = holidays.find((item) => String(item.id) === String(editBtn.getAttribute('data-id')));
        if (!row) return;
        document.getElementById('attendanceHolidayId').value = row.id || '';
        document.getElementById('attendanceHolidayName').value = row.name || '';
        document.getElementById('attendanceHolidayMode').value = row.holiday_mode || ((row.holiday_end_date && row.holiday_end_date !== row.holiday_start_date) ? 'date_range' : 'full_day');
        document.getElementById('attendanceHolidayStartDate').value = row.holiday_start_date || row.holiday_date || '';
        document.getElementById('attendanceHolidayEndDate').value = row.holiday_end_date || row.holiday_start_date || row.holiday_date || '';
        document.getElementById('attendanceHolidayType').value = row.holiday_type || 'public';
        document.getElementById('attendanceHolidayAppliesTo').value = row.applies_to || 'all';
        document.getElementById('attendanceHolidayDepartment').value = row.department_id || '';
        if (getHolidayMode() !== 'date_range') {
          document.getElementById('attendanceHolidayEndDate').value = document.getElementById('attendanceHolidayStartDate').value || '';
        }
        toggleHolidayModeFields();
        toggleDepartmentField();
        bootstrap.Modal.getOrCreateInstance(document.getElementById('attendanceHolidayModal')).show();
        return;
      }
      const deleteBtn = e.target.closest('.js-delete-holiday');
      if (deleteBtn) {
        deleteHoliday(deleteBtn.getAttribute('data-id'));
      }
    });
  }

  fillDepartmentSelect();
  toggleHolidayModeFields();
  toggleDepartmentField();
  if (!canManage) {
    const addBtn = document.getElementById('btnOpenAttendanceHolidayModal');
    if (addBtn) addBtn.remove();
  }
  bindEvents();
  loadHolidays();
})();
