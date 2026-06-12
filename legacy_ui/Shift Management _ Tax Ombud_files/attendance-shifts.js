(function () {
  const cfg = window.hrAttendanceShiftsConfig || {};
  const csrf = cfg.csrf || '';
  const departments = Array.isArray(cfg.departments) ? cfg.departments : [];
  const staff = Array.isArray(cfg.staff) ? cfg.staff : [];
  const canManage = cfg.canManage === true;
  let shifts = [];
  let assignments = [];

  function esc(v) {
    return String(v == null ? '' : v)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function showAlert(type, message) {
    const host = document.getElementById('attendanceShiftsAlert');
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

  function fillStaticSelects() {
    const userSelect = document.getElementById('attendanceAssignmentUser');
    const deptSelect = document.getElementById('attendanceAssignmentDepartment');
    if (userSelect) {
      userSelect.innerHTML = '<option value="">Select staff</option>' + staff.map((row) => '<option value="' + esc(row.id) + '">' + esc(row.display_name || '') + '</option>').join('');
    }
    if (deptSelect) {
      deptSelect.innerHTML = '<option value="">Select department</option>' + departments.map((row) => '<option value="' + esc(row.id) + '">' + esc(row.name || '') + '</option>').join('');
    }
  }

  function fillShiftOptions(selectedValue) {
    const select = document.getElementById('attendanceAssignmentShift');
    if (!select) return;
    select.innerHTML = '<option value="">Select shift</option>' + shifts.map((row) => '<option value="' + esc(row.id) + '">' + esc(row.name || '') + '</option>').join('');
    if (selectedValue) select.value = String(selectedValue);
  }

  function renderShiftsTable() {
    const tableId = '#attendanceShiftsTable';
    if ($.fn.DataTable.isDataTable(tableId)) {
      $(tableId).DataTable().clear().destroy();
    }
    const tbody = document.querySelector(tableId + ' tbody');
    tbody.innerHTML = shifts.map((row) => '<tr>'
      + '<td>' + esc(row.name || '-') + '</td>'
      + '<td>' + esc(row.start_time || '-') + '</td>'
      + '<td>' + esc(row.end_time || '-') + '</td>'
      + '<td>' + esc(row.grace_minutes || 0) + '</td>'
      + '<td>' + esc(row.break_minutes || 0) + '</td>'
      + '<td>' + esc(row.overtime_threshold_minutes || 0) + '</td>'
      + '<td>' + (Number(row.is_active || 0) === 1 ? '<span class="badge bg-success-subtle text-success">Active</span>' : '<span class="badge bg-secondary-subtle text-secondary">Inactive</span>') + '</td>'
      + (canManage ? '<td><div class="d-flex gap-2"><button type="button" class="btn btn-soft-primary btn-sm js-edit-shift" data-id="' + esc(row.id) + '"><i class="ri-edit-line me-1"></i>Edit</button><button type="button" class="btn btn-soft-danger btn-sm js-delete-shift" data-id="' + esc(row.id) + '"><i class="ri-delete-bin-line me-1"></i>Delete</button></div></td>' : '')
      + '</tr>').join('');
    $(tableId).DataTable({
      destroy: true,
      pageLength: 10,
      order: [[0, 'asc']],
      language: { emptyTable: 'No shifts configured yet.' }
    });
  }

  function renderAssignmentsTable() {
    const tableId = '#attendanceAssignmentsTable';
    if ($.fn.DataTable.isDataTable(tableId)) {
      $(tableId).DataTable().clear().destroy();
    }
    const tbody = document.querySelector(tableId + ' tbody');
    tbody.innerHTML = assignments.map((row) => {
      const target = row.user_name || row.department_name || '-';
      return '<tr>'
        + '<td>' + esc(target) + '</td>'
        + '<td>' + esc(row.shift_name || '-') + '</td>'
        + '<td>' + esc(row.effective_from || '-') + '</td>'
        + '<td>' + esc(row.effective_to || '-') + '</td>'
        + (canManage ? '<td><div class="d-flex gap-2"><button type="button" class="btn btn-soft-primary btn-sm js-edit-assignment" data-id="' + esc(row.id) + '"><i class="ri-edit-line me-1"></i>Edit</button><button type="button" class="btn btn-soft-danger btn-sm js-delete-assignment" data-id="' + esc(row.id) + '"><i class="ri-delete-bin-line me-1"></i>Remove</button></div></td>' : '')
        + '</tr>';
    }).join('');
    $(tableId).DataTable({
      destroy: true,
      pageLength: 10,
      order: [[2, 'desc']],
      language: { emptyTable: 'No assignments found.' }
    });
  }

  async function loadData() {
    showAlert('', '');
    try {
      const data = await fetchJson('api/modules/hr/attendance_shifts');
      shifts = Array.isArray(data.shifts) ? data.shifts : [];
      assignments = Array.isArray(data.assignments) ? data.assignments : [];
      fillShiftOptions();
      renderShiftsTable();
      renderAssignmentsTable();
    } catch (e) {
      showAlert('danger', e.message || 'Unable to load shift data.');
    }
  }

  function resetShiftForm() {
    document.getElementById('attendanceShiftId').value = '';
    document.getElementById('attendanceShiftName').value = '';
    document.getElementById('attendanceShiftStart').value = '';
    document.getElementById('attendanceShiftEnd').value = '';
    document.getElementById('attendanceShiftGrace').value = '10';
    document.getElementById('attendanceShiftBreak').value = '60';
    document.getElementById('attendanceShiftOtThreshold').value = '0';
    document.getElementById('attendanceShiftActive').checked = true;
  }

  function resetAssignmentForm() {
    document.getElementById('attendanceAssignmentId').value = '';
    document.getElementById('attendanceAssignmentType').value = 'user';
    document.getElementById('attendanceAssignmentUser').value = '';
    document.getElementById('attendanceAssignmentDepartment').value = '';
    document.getElementById('attendanceAssignmentShift').value = '';
    document.getElementById('attendanceAssignmentFrom').value = '';
    document.getElementById('attendanceAssignmentTo').value = '';
    toggleAssignmentType();
  }

  function toggleAssignmentType() {
    const type = document.getElementById('attendanceAssignmentType').value || 'user';
    document.getElementById('attendanceAssignmentUserWrap').classList.toggle('d-none', type !== 'user');
    document.getElementById('attendanceAssignmentDepartmentWrap').classList.toggle('d-none', type !== 'department');
  }

  async function saveShift() {
    const id = document.getElementById('attendanceShiftId').value || '';
    const payload = {
      name: document.getElementById('attendanceShiftName').value || '',
      start_time: document.getElementById('attendanceShiftStart').value || '',
      end_time: document.getElementById('attendanceShiftEnd').value || '',
      grace_minutes: document.getElementById('attendanceShiftGrace').value || 0,
      break_minutes: document.getElementById('attendanceShiftBreak').value || 0,
      overtime_threshold_minutes: document.getElementById('attendanceShiftOtThreshold').value || 0,
      is_active: document.getElementById('attendanceShiftActive').checked ? 1 : 0
    };
    if (!payload.name || !payload.start_time || !payload.end_time) {
      showAlert('danger', 'Shift name, start time, and end time are required.');
      return;
    }
    try {
      const method = id ? 'PATCH' : 'POST';
      if (id) payload.id = id;
      await fetchJson('api/modules/hr/attendance_shifts', {
        method: method,
        headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrf },
        body: JSON.stringify(payload)
      });
      bootstrap.Modal.getInstance(document.getElementById('attendanceShiftModal')).hide();
      showAlert('success', 'Shift saved successfully.');
      await loadData();
    } catch (e) {
      showAlert('danger', e.message || 'Unable to save shift.');
    }
  }

  async function saveAssignment() {
    const id = document.getElementById('attendanceAssignmentId').value || '';
    const type = document.getElementById('attendanceAssignmentType').value || 'user';
    const payload = {
      type: 'assignment',
      shift_id: document.getElementById('attendanceAssignmentShift').value || '',
      effective_from: document.getElementById('attendanceAssignmentFrom').value || '',
      effective_to: document.getElementById('attendanceAssignmentTo').value || ''
    };
    if (type === 'department') payload.department_id = document.getElementById('attendanceAssignmentDepartment').value || '';
    else payload.user_id = document.getElementById('attendanceAssignmentUser').value || '';
    if (!payload.shift_id || !payload.effective_from || (!payload.user_id && !payload.department_id)) {
      showAlert('danger', 'Assignment target, shift, and effective from date are required.');
      return;
    }
    try {
      const method = id ? 'PATCH' : 'POST';
      if (id) payload.id = id;
      await fetchJson('api/modules/hr/attendance_shifts', {
        method: method,
        headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrf },
        body: JSON.stringify(payload)
      });
      bootstrap.Modal.getInstance(document.getElementById('attendanceAssignmentModal')).hide();
      showAlert('success', 'Assignment saved successfully.');
      await loadData();
    } catch (e) {
      showAlert('danger', e.message || 'Unable to save assignment.');
    }
  }

  async function deleteResource(kind, id) {
    if (!window.crmUiConfirm) return;
    const confirmed = await window.crmUiConfirm({
      title: 'Delete ' + (kind === 'shift' ? 'Shift' : 'Assignment'),
      text: 'This action cannot be undone.',
      confirmButtonText: 'Delete',
      confirmButtonClass: 'btn btn-danger btn-md'
    });
    if (!confirmed) return;
    try {
      await fetchJson('api/modules/hr/attendance_shifts?' + new URLSearchParams({ id: id, type: kind }).toString(), {
        method: 'DELETE',
        headers: { 'X-CSRF-Token': csrf }
      });
      showAlert('success', (kind === 'shift' ? 'Shift' : 'Assignment') + ' removed.');
      await loadData();
    } catch (e) {
      showAlert('danger', e.message || 'Unable to delete resource.');
    }
  }

  function bindEvents() {
    const elAssignType = document.getElementById('attendanceAssignmentType');
    if (elAssignType) elAssignType.addEventListener('change', toggleAssignmentType);
    const elOpenShift = document.getElementById('btnOpenShiftModal');
    if (elOpenShift) elOpenShift.addEventListener('click', () => {
      resetShiftForm();
      bootstrap.Modal.getOrCreateInstance(document.getElementById('attendanceShiftModal')).show();
    });
    const elOpenAssign = document.getElementById('btnOpenAssignmentModal');
    if (elOpenAssign) elOpenAssign.addEventListener('click', () => {
      resetAssignmentForm();
      fillShiftOptions();
      bootstrap.Modal.getOrCreateInstance(document.getElementById('attendanceAssignmentModal')).show();
    });
    const elSaveShift = document.getElementById('btnSaveAttendanceShift');
    if (elSaveShift) elSaveShift.addEventListener('click', saveShift);
    const elSaveAssign = document.getElementById('btnSaveAttendanceAssignment');
    if (elSaveAssign) elSaveAssign.addEventListener('click', saveAssignment);
    document.addEventListener('click', (e) => {
      const editShiftBtn = e.target.closest('.js-edit-shift');
      if (editShiftBtn) {
        const row = shifts.find((item) => String(item.id) === String(editShiftBtn.getAttribute('data-id')));
        if (!row) return;
        document.getElementById('attendanceShiftId').value = row.id || '';
        document.getElementById('attendanceShiftName').value = row.name || '';
        document.getElementById('attendanceShiftStart').value = row.start_time || '';
        document.getElementById('attendanceShiftEnd').value = row.end_time || '';
        document.getElementById('attendanceShiftGrace').value = row.grace_minutes || 0;
        document.getElementById('attendanceShiftBreak').value = row.break_minutes || 0;
        document.getElementById('attendanceShiftOtThreshold').value = row.overtime_threshold_minutes || 0;
        document.getElementById('attendanceShiftActive').checked = Number(row.is_active || 0) === 1;
        bootstrap.Modal.getOrCreateInstance(document.getElementById('attendanceShiftModal')).show();
        return;
      }
      const deleteShiftBtn = e.target.closest('.js-delete-shift');
      if (deleteShiftBtn) {
        deleteResource('shift', deleteShiftBtn.getAttribute('data-id'));
        return;
      }
      const editAssignmentBtn = e.target.closest('.js-edit-assignment');
      if (editAssignmentBtn) {
        const row = assignments.find((item) => String(item.id) === String(editAssignmentBtn.getAttribute('data-id')));
        if (!row) return;
        document.getElementById('attendanceAssignmentId').value = row.id || '';
        document.getElementById('attendanceAssignmentType').value = row.user_id ? 'user' : 'department';
        document.getElementById('attendanceAssignmentUser').value = row.user_id || '';
        document.getElementById('attendanceAssignmentDepartment').value = row.department_id || '';
        document.getElementById('attendanceAssignmentShift').value = row.shift_id || '';
        document.getElementById('attendanceAssignmentFrom').value = row.effective_from || '';
        document.getElementById('attendanceAssignmentTo').value = row.effective_to || '';
        toggleAssignmentType();
        bootstrap.Modal.getOrCreateInstance(document.getElementById('attendanceAssignmentModal')).show();
        return;
      }
      const deleteAssignmentBtn = e.target.closest('.js-delete-assignment');
      if (deleteAssignmentBtn) {
        deleteResource('assignment', deleteAssignmentBtn.getAttribute('data-id'));
      }
    });
  }

  fillStaticSelects();
  fillShiftOptions();
  if (!canManage) {
    const addShiftBtn = document.getElementById('btnOpenShiftModal');
    if (addShiftBtn) addShiftBtn.remove();
    const addAssignmentBtn = document.getElementById('btnOpenAssignmentModal');
    if (addAssignmentBtn) addAssignmentBtn.remove();
  }
  bindEvents();
  loadData();
})();
