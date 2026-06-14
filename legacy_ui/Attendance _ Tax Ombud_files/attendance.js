(function () {
  const cfg = window.hrAttendanceConfig || {};
  const csrf = cfg.csrf || '';
  const canManage = cfg.canManage === true;
  const selfOnly = cfg.selfOnly === true;
  const currentUserId = cfg.currentUserId || 0;
  const selfClockMode = String(cfg.selfClockMode || 'allow');
  const selfClockGeoEnforced = cfg.selfClockGeoEnforced === true;
  const departments = Array.isArray(cfg.departments) ? cfg.departments : [];
  const staff = Array.isArray(cfg.staff) ? cfg.staff : [];
  let lastReportData = [];

  const dailyTableId = '#attendanceDailyTable';
  const monthlyTableId = '#attendanceMonthlyTable';
  const overtimeTableId = '#attendanceOvertimeTable';

  function esc(v) {
    return String(v == null ? '' : v)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function showAlert(targetId, type, message) {
    const host = document.getElementById(targetId);
    if (!host) return;
    if (!message) {
      host.innerHTML = '';
      return;
    }
    host.innerHTML = '<div class="alert alert-' + esc(type) + ' py-2 mb-3">' + esc(message) + '</div>';
  }

  async function fetchJson(url, options) {
    const res = await fetch(url, Object.assign({ credentials: 'same-origin' }, options || {}));
    let data = null;
    try { data = await res.json(); } catch (e) { data = null; }
    if (!res.ok) {
      throw new Error((data && (data.error || data.message)) || ('Request failed (' + res.status + ')'));
    }
    return data;
  }

  function hoursFromMinutes(minutes) {
    return (Number(minutes || 0) / 60).toFixed(2);
  }

  function isWeekendDate(value) {
    if (!value) return false;
    const date = new Date(String(value).slice(0, 10) + 'T00:00:00');
    if (Number.isNaN(date.getTime())) return false;
    const dow = date.getDay();
    return dow === 0 || dow === 6;
  }

  function statusBadge(status) {
    const map = {
      present: 'success',
      absent: 'danger',
      late: 'warning',
      half_day: 'info',
      on_leave: 'primary',
      holiday: 'secondary',
      weekend: 'light'
    };
    const label = String(status || 'absent').replace(/_/g, ' ');
    const cls = map[status] || 'secondary';
    return '<span class="badge bg-' + cls + '-subtle text-' + cls + '">' + esc(label) + '</span>';
  }

  function buildOptions(select, rows, includeAllLabel) {
    if (!select) return;
    const current = select.value;
    const opts = ['<option value="">' + esc(includeAllLabel || 'Select') + '</option>'];
    rows.forEach((row) => {
      opts.push('<option value="' + esc(row.id) + '">' + esc(row.name || row.display_name || '') + '</option>');
    });
    select.innerHTML = opts.join('');
    if (current) select.value = current;
  }

  function initStaticFilters() {
    if (selfOnly) {
      // Hide department and staff filters — user sees only their own records
      ['attendanceDepartmentFilter', 'attendanceMonthDepartmentFilter', 'attendanceOvertimeUserFilter'].forEach((id) => {
        const el = document.getElementById(id);
        if (el) el.closest('.col-12') && (el.closest('.col-12').style.display = 'none');
      });
      return;
    }
    buildOptions(document.getElementById('attendanceDepartmentFilter'), departments, 'All departments');
    buildOptions(document.getElementById('attendanceMonthDepartmentFilter'), departments, 'All departments');
    const otUserRows = staff.map((row) => ({ id: row.id, name: row.display_name || 'Staff #' + row.id }));
    buildOptions(document.getElementById('attendanceOvertimeUserFilter'), otUserRows, 'All staff');
    const userSelect = document.getElementById('attendanceRecordUser');
    if (userSelect) {
      userSelect.innerHTML = staff.map((row) => '<option value="' + esc(row.id) + '">' + esc(row.display_name || 'Staff #' + row.id) + '</option>').join('');
    }
  }

  function buildDailyTable(rows) {
    if ($.fn.DataTable.isDataTable(dailyTableId)) {
      $(dailyTableId).DataTable().clear().destroy();
    }
    const tbody = document.querySelector(dailyTableId + ' tbody');
    tbody.innerHTML = (rows || []).map((row) => {
      const sourceParts = [];
      if (row.source_site) sourceParts.push('<div class="fw-semibold">' + esc(row.source_site) + '</div>');
      if (row.source_device_id) sourceParts.push('<div class="text-muted small">Device: ' + esc(row.source_device_id) + '</div>');
      if (row.source_provider) sourceParts.push('<div class="text-muted small">Provider: ' + esc(row.source_provider) + '</div>');
      if (!sourceParts.length) sourceParts.push('<span class="text-muted">-</span>');
      const isOwnRow = selfOnly && String(row.user_id) === String(currentUserId);
      const actionBtn = canManage
        ? '<button type="button" class="btn btn-soft-primary btn-sm js-correct-record" data-record-id="' + esc(row.id || '') + '" data-user-id="' + esc(row.user_id) + '" data-record-date="' + esc(row.record_date) + '" data-status="' + esc(row.status) + '" data-clock-in="' + esc((row.clock_in || '').replace(' ', 'T').slice(0, 16)) + '" data-clock-out="' + esc((row.clock_out || '').replace(' ', 'T').slice(0, 16)) + '" data-notes="' + esc(row.notes || '') + '"><i class="ri-edit-line me-1"></i>Correct</button>'
        : isOwnRow
          ? '<button type="button" class="btn btn-success btn-md js-self-clock-in me-1"><i class="ri-time-line me-1"></i>Clock In</button>'
            + '<button type="button" class="btn btn-warning btn-md js-self-clock-out"><i class="ri-time-line me-1"></i>Clock Out</button>'
          : '<span class="text-muted small">View only</span>';
      return '<tr>'
        + '<td>' + esc(row.user_name || '-') + '</td>'
        + '<td>' + esc(row.department_name || '-') + '</td>'
        + '<td>' + esc(row.shift_name || '-') + '</td>'
        + '<td>' + sourceParts.join('') + '</td>'
        + '<td>' + esc(row.clock_in || '-') + '</td>'
        + '<td>' + esc(row.clock_out || '-') + '</td>'
        + '<td>' + statusBadge(row.status) + '</td>'
        + '<td>' + esc(hoursFromMinutes(row.worked_minutes)) + '</td>'
        + '<td>' + esc(hoursFromMinutes(row.overtime_minutes)) + '</td>'
        + '<td>' + esc(row.late_minutes || 0) + ' min</td>'
        + '<td>' + actionBtn + '</td>'
        + '</tr>';
    }).join('');
    $(dailyTableId).DataTable({
      destroy: true,
      pageLength: 25,
      order: [[0, 'asc']],
      language: { emptyTable: 'No attendance records found.' }
    });
  }

  function buildMonthlyTable(rows) {
    if ($.fn.DataTable.isDataTable(monthlyTableId)) {
      $(monthlyTableId).DataTable().clear().destroy();
    }
    const tbody = document.querySelector(monthlyTableId + ' tbody');
    tbody.innerHTML = (rows || []).map((row) => {
      const monthlySourceParts = [];
      if (row.source_sites_label) monthlySourceParts.push('<div class="fw-semibold">' + esc(row.source_sites_label) + '</div>');
      if (row.source_devices_label) monthlySourceParts.push('<div class="text-muted small">Devices: ' + esc(row.source_devices_label) + '</div>');
      if (row.source_providers_label) monthlySourceParts.push('<div class="text-muted small">Providers: ' + esc(row.source_providers_label) + '</div>');
      if (!monthlySourceParts.length) monthlySourceParts.push('<span class="text-muted">-</span>');
      return '<tr>'
      + '<td>' + esc(row.user_name || '-') + '</td>'
      + '<td>' + esc(row.department_name || '-') + '</td>'
      + '<td>' + monthlySourceParts.join('') + '</td>'
      + '<td>' + esc(row.present_days || 0) + '</td>'
      + '<td>' + esc(row.absent_days || 0) + '</td>'
      + '<td>' + esc(row.late_days || 0) + '</td>'
      + '<td>' + esc(row.half_days || 0) + '</td>'
      + '<td>' + esc(row.on_leave_days || 0) + '</td>'
      + '<td>' + esc(row.holiday_days || 0) + '</td>'
      + '<td>' + esc(Number(row.total_worked_hours || 0).toFixed(2)) + '</td>'
      + '<td>' + esc(Number(row.total_overtime_hours || 0).toFixed(2)) + '</td>'
      + '<td>' + esc(row.total_late_minutes || 0) + '</td>'
      + '</tr>';
    }).join('');
    $(monthlyTableId).DataTable({
      destroy: true,
      pageLength: 25,
      order: [[0, 'asc']],
      language: { emptyTable: 'No attendance summary available.' }
    });
  }

  function buildOvertimeTable(rows) {
    if (!document.querySelector(overtimeTableId)) return;
    if ($.fn.DataTable.isDataTable(overtimeTableId)) {
      $(overtimeTableId).DataTable().clear().destroy();
    }
    const tbody = document.querySelector(overtimeTableId + ' tbody');
    tbody.innerHTML = (rows || []).map((row) => {
      let actionHtml = '<span class="text-muted small">Reviewed</span>';
      if (String(row.status || '') === 'pending') {
        actionHtml = ''
          + '<div class="d-flex gap-2">'
          + '<button type="button" class="btn btn-soft-success btn-sm js-review-overtime" data-id="' + esc(row.id) + '" data-action="approve"><i class="ri-check-line me-1"></i>Approve</button>'
          + '<button type="button" class="btn btn-soft-danger btn-sm js-review-overtime" data-id="' + esc(row.id) + '" data-action="reject"><i class="ri-close-line me-1"></i>Reject</button>'
          + '</div>';
      }
      return '<tr>'
        + '<td>' + esc(row.user_name || '-') + '</td>'
        + '<td>' + esc(row.request_date || '-') + '</td>'
        + '<td>' + esc(row.requested_minutes || 0) + '</td>'
        + '<td>' + esc(row.recorded_overtime_minutes || 0) + '</td>'
        + '<td>' + statusBadge(row.status) + '</td>'
        + '<td>' + esc(row.reason || '-') + '</td>'
        + '<td>' + actionHtml + '</td>'
        + '</tr>';
    }).join('');
    $(overtimeTableId).DataTable({
      destroy: true,
      pageLength: 25,
      order: [[1, 'desc']],
      language: { emptyTable: 'No overtime requests found.' }
    });
  }

  function updateSummaryCards(rows) {
    const stats = { present: 0, late: 0, absent: 0, overtimeMinutes: 0 };
    (rows || []).forEach((row) => {
      if (row.status === 'present') stats.present++;
      if (row.status === 'late') { stats.present++; stats.late++; }
      if (row.status === 'absent') stats.absent++;
      if (row.status === 'half_day') stats.present++;
      if (row.status === 'on_leave') stats.present++;
      stats.overtimeMinutes += Number(row.overtime_minutes || 0);
    });
    const set = (id, value) => {
      const el = document.getElementById(id);
      if (el) el.textContent = value;
    };
    set('attStatPresent', stats.present);
    set('attStatLate', stats.late);
    set('attStatAbsent', stats.absent);
    set('attStatOtHours', (stats.overtimeMinutes / 60).toFixed(1));
  }

  function renderRollup(rollup) {
    const host = document.getElementById('attendanceRollupChips');
    if (!host) return;
    const rows = [
      ['Employees', rollup.employees || 0],
      ['Present', rollup.present_days || 0],
      ['Absent', rollup.absent_days || 0],
      ['Late', rollup.late_days || 0],
      ['Leave', rollup.on_leave_days || 0],
      ['Worked Hrs', Number(rollup.total_worked_hours || 0).toFixed(2)],
      ['OT Hours', Number(rollup.total_overtime_hours || 0).toFixed(2)]
    ];
    host.innerHTML = rows.map((row) => '<span class="att-summary-chip"><span>' + esc(row[0]) + ':</span><strong>' + esc(row[1]) + '</strong></span>').join('');
  }

  async function loadDailyView() {
    showAlert('attendanceAlert', '', '');
    try {
      const date = document.getElementById('attendanceDateFilter').value || '';
      const status = document.getElementById('attendanceStatusFilter').value || '';
      const params = new URLSearchParams({ mode: 'daily_grid', date: date });
      if (selfOnly) {
        params.set('user_id', currentUserId);
      } else {
        const departmentId = document.getElementById('attendanceDepartmentFilter').value || '';
        if (departmentId) params.set('department_id', departmentId);
      }
      if (status) params.set('status', status);
      const data = await fetchJson('api/modules/hr/attendance?' + params.toString());
      buildDailyTable(data.data || []);
      updateSummaryCards(data.data || []);
    } catch (e) {
      showAlert('attendanceAlert', 'danger', e.message || 'Unable to load attendance.');
    }
  }

  async function loadMonthlyReport() {
    showAlert('attendanceAlert', '', '');
    try {
      const month = document.getElementById('attendanceMonthFilter').value || '';
      const params = new URLSearchParams({ month: month });
      if (selfOnly) {
        params.set('user_id', currentUserId);
      } else {
        const departmentId = document.getElementById('attendanceMonthDepartmentFilter').value || '';
        if (departmentId) params.set('department_id', departmentId);
      }
      const data = await fetchJson('api/modules/hr/attendance_report?' + params.toString());
      lastReportData = data.data || [];
      buildMonthlyTable(lastReportData);
      renderRollup(data.rollup || {});
    } catch (e) {
      showAlert('attendanceAlert', 'danger', e.message || 'Unable to load report.');
    }
  }

  async function loadOvertimeRequests() {
    if (!document.querySelector(overtimeTableId)) return;
    try {
      const status = document.getElementById('attendanceOvertimeStatusFilter').value || '';
      const userId = document.getElementById('attendanceOvertimeUserFilter').value || '';
      const params = new URLSearchParams();
      if (status) params.set('status', status);
      if (userId) params.set('user_id', userId);
      const data = await fetchJson('api/modules/hr/overtime?' + params.toString());
      buildOvertimeTable(data.data || []);
    } catch (e) {
      showAlert('attendanceAlert', 'danger', e.message || 'Unable to load overtime requests.');
    }
  }

  async function loadEffectiveShiftPreview() {
    const userId = document.getElementById('attendanceRecordUser').value || '';
    const recordDate = document.getElementById('attendanceRecordDate').value || '';
    const clockIn = document.getElementById('attendanceRecordClockIn').value || '';
    const clockOut = document.getElementById('attendanceRecordClockOut').value || '';
    const host = document.getElementById('attendanceCalcPreview');
    const workingDayAlert = document.getElementById('attendanceWorkingDayAlert');
    const saveBtn = document.getElementById('btnSaveAttendanceRecord');
    if (!host) return;
    if (!userId || !recordDate) {
      host.textContent = 'Pick staff and date to preview calculations.';
      if (workingDayAlert) workingDayAlert.classList.add('d-none');
      if (saveBtn) saveBtn.disabled = false;
      return;
    }
    if (isWeekendDate(recordDate)) {
      host.textContent = 'Weekend selected. Attendance is not recorded on Saturdays or Sundays.';
      if (workingDayAlert) {
        workingDayAlert.textContent = 'This date is a weekend. Attendance can only be saved on working days (Monday to Friday).';
        workingDayAlert.classList.remove('d-none');
      }
      if (saveBtn) saveBtn.disabled = true;
      return;
    }
    if (workingDayAlert) workingDayAlert.classList.add('d-none');
    if (saveBtn) saveBtn.disabled = false;
    try {
      let shiftText = 'No shift assigned';
      let otText = '0';
      let lateText = '0';
      const shiftRes = await fetchJson('api/modules/hr/attendance_shifts?user_id=' + encodeURIComponent(userId) + '&date=' + encodeURIComponent(recordDate));
      const shift = shiftRes.effective_shift || null;
      if (shift) {
        shiftText = (shift.name || 'Shift') + ' (' + (shift.start_time || '') + ' - ' + (shift.end_time || '') + ')';
      }
      let worked = 0;
      if (clockIn && clockOut) {
        const start = new Date(clockIn);
        const end = new Date(clockOut);
        if (!Number.isNaN(start.getTime()) && !Number.isNaN(end.getTime()) && end > start) {
          worked = Math.floor((end.getTime() - start.getTime()) / 60000);
          if (shift && Number(shift.break_minutes || 0) > 0) worked = Math.max(0, worked - Number(shift.break_minutes || 0));
          if (shift) {
            const shiftStart = new Date(recordDate + 'T' + shift.start_time);
            const shiftEnd = new Date(recordDate + 'T' + shift.end_time);
            if (shiftEnd <= shiftStart) shiftEnd.setDate(shiftEnd.getDate() + 1);
            const graceEnd = new Date(shiftStart.getTime() + (Number(shift.grace_minutes || 0) * 60000));
            const startDt = new Date(clockIn);
            const endDt = new Date(clockOut);
            if (startDt > graceEnd) lateText = String(Math.floor((startDt.getTime() - graceEnd.getTime()) / 60000));
            const scheduled = Math.max(0, Math.floor((shiftEnd.getTime() - shiftStart.getTime()) / 60000) - Number(shift.break_minutes || 0));
            const extra = worked - scheduled;
            if (extra > Number(shift.overtime_threshold_minutes || 0)) {
              otText = String(extra);
            }
          }
        }
      }
      host.textContent = 'Shift: ' + shiftText + ' | Worked: ' + hoursFromMinutes(worked) + ' hrs | OT: ' + otText + ' min | Late: ' + lateText + ' min';
    } catch (e) {
      host.textContent = 'Preview unavailable right now. Final values will be computed on save.';
    }
  }

  async function saveAttendanceRecord() {
    showAlert('attendanceCorrectionAlert', '', '');
    const recordId = document.getElementById('attendanceRecordId').value || '';
    const recordDate = document.getElementById('attendanceRecordDate').value || '';
    if (isWeekendDate(recordDate)) {
      showAlert('attendanceCorrectionAlert', 'danger', 'Attendance cannot be recorded on weekends. Please select a working day.');
      return;
    }
    const payload = {
      user_id: document.getElementById('attendanceRecordUser').value || '',
      record_date: recordDate,
      status: document.getElementById('attendanceRecordStatus').value || '',
      clock_in: document.getElementById('attendanceRecordClockIn').value ? document.getElementById('attendanceRecordClockIn').value.replace('T', ' ') + ':00' : '',
      clock_out: document.getElementById('attendanceRecordClockOut').value ? document.getElementById('attendanceRecordClockOut').value.replace('T', ' ') + ':00' : '',
      notes: document.getElementById('attendanceRecordNotes').value || '',
      source: 'manual'
    };
    if (!payload.user_id || !payload.record_date) {
      showAlert('attendanceCorrectionAlert', 'danger', 'Staff and date are required.');
      return;
    }
    try {
      const isUpdate = recordId && Number(recordId) > 0;
      const url = isUpdate ? 'api/modules/hr/attendance?id=' + encodeURIComponent(recordId) : 'api/modules/hr/attendance';
      await fetchJson(url, {
        method: isUpdate ? 'PATCH' : 'POST',
        headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrf },
        body: JSON.stringify(payload)
      });
      showAlert('attendanceCorrectionAlert', 'success', 'Attendance record saved.');
      await loadDailyView();
    } catch (e) {
      showAlert('attendanceCorrectionAlert', 'danger', e.message || 'Unable to save record.');
    }
  }

  async function submitOvertimeReview() {
    showAlert('attendanceOvertimeReviewAlert', '', '');
    const id = document.getElementById('attendanceOvertimeReviewId').value || '';
    const action = document.getElementById('attendanceOvertimeReviewAction').value || '';
    const reviewNote = document.getElementById('attendanceOvertimeReviewNote').value || '';
    if (!id || !action) return;
    try {
      await fetchJson('api/modules/hr/overtime', {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrf },
        body: JSON.stringify({ id: id, action: action, review_note: reviewNote })
      });
      showAlert('attendanceOvertimeReviewAlert', 'success', 'Overtime request reviewed.');
      await loadOvertimeRequests();
      const modalEl = document.getElementById('attendanceOvertimeReviewModal');
      if (modalEl && window.bootstrap) {
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
      }
    } catch (e) {
      showAlert('attendanceOvertimeReviewAlert', 'danger', e.message || 'Unable to submit review.');
    }
  }

  function exportAttendanceCsv() {
    if (!lastReportData.length) {
      showAlert('attendanceAlert', 'warning', 'Load a report first before exporting.');
      return;
    }
    const header = ['Employee', 'Department', 'Present', 'Absent', 'Late', 'Half Days', 'Leave', 'Holidays', 'Total Hours', 'OT Hours', 'Late Minutes'];
    const lines = [header.join(',')];
    lastReportData.forEach((row) => {
      const cells = [
        row.user_name || '',
        row.department_name || '',
        row.present_days || 0,
        row.absent_days || 0,
        row.late_days || 0,
        row.half_days || 0,
        row.on_leave_days || 0,
        row.holiday_days || 0,
        Number(row.total_worked_hours || 0).toFixed(2),
        Number(row.total_overtime_hours || 0).toFixed(2),
        row.total_late_minutes || 0
      ].map((v) => '"' + String(v).replace(/"/g, '""') + '"');
      lines.push(cells.join(','));
    });
    const blob = new Blob([lines.join('\n')], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = 'attendance-report-' + (document.getElementById('attendanceMonthFilter').value || 'month') + '.csv';
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(link.href);
  }

  async function submitClockAction(action) {
    if (!currentUserId) return;
    if (selfClockMode !== 'allow') {
      if (window.showErrorModal) window.showErrorModal('Clocking Disabled', selfClockMode === 'device_only'
        ? 'Self clocking is disabled. Use an approved attendance device.'
        : 'Self clocking is disabled by system policy.');
      return;
    }
    const btn = document.getElementById(action === 'clock_in' ? 'btnAttendanceClockIn' : 'btnAttendanceClockOut');
    if (btn) btn.disabled = true;
    try {
      const payload = { action: action, user_id: currentUserId };
      if (selfClockGeoEnforced) {
        if (!navigator.geolocation || typeof navigator.geolocation.getCurrentPosition !== 'function') {
          throw new Error('Geolocation is required for self clocking on this system.');
        }
        const position = await new Promise((resolve, reject) => {
          navigator.geolocation.getCurrentPosition(resolve, reject, {
            enableHighAccuracy: true,
            timeout: 15000,
            maximumAge: 0,
          });
        }).catch((err) => {
          throw new Error(err && err.message ? err.message : 'Unable to get your location.');
        });
        payload.latitude = Number(position.coords && position.coords.latitude);
        payload.longitude = Number(position.coords && position.coords.longitude);
        payload.accuracy = Number(position.coords && position.coords.accuracy);
      }
      await fetchJson('api/modules/hr/attendance', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'X-CSRF-Token': csrf },
        body: JSON.stringify(payload),
      });
      if (window.showSavedModal) window.showSavedModal(
        action === 'clock_in' ? 'Clocked In' : 'Clocked Out',
        action === 'clock_in' ? 'Clock-in recorded successfully.' : 'Clock-out recorded successfully.'
      );
      loadDailyView();
    } catch (e) {
      if (window.showErrorModal) window.showErrorModal('Attendance Error', e.message || 'Unable to record attendance action.');
    } finally {
      if (btn) btn.disabled = false;
    }
  }

  function bindEvents() {
    const refreshDailyBtn = document.getElementById('btnRefreshDailyAttendance');
    if (refreshDailyBtn) refreshDailyBtn.addEventListener('click', loadDailyView);
    const refreshReportBtn = document.getElementById('btnRefreshAttendanceReport');
    if (refreshReportBtn) refreshReportBtn.addEventListener('click', loadMonthlyReport);
    const refreshOtBtn = document.getElementById('btnRefreshOvertimeRequests');
    if (refreshOtBtn) refreshOtBtn.addEventListener('click', loadOvertimeRequests);
    const exportBtn = document.getElementById('btnExportAttendanceCsv');
    if (exportBtn) exportBtn.addEventListener('click', exportAttendanceCsv);
    const saveBtn = document.getElementById('btnSaveAttendanceRecord');
    if (saveBtn) saveBtn.addEventListener('click', saveAttendanceRecord);
    const reviewBtn = document.getElementById('btnSubmitAttendanceOvertimeReview');
    if (reviewBtn) reviewBtn.addEventListener('click', submitOvertimeReview);
    ['attendanceRecordUser', 'attendanceRecordDate', 'attendanceRecordClockIn', 'attendanceRecordClockOut'].forEach((id) => {
      const el = document.getElementById(id);
      if (el) el.addEventListener('change', loadEffectiveShiftPreview);
    });
    const clockInBtn = document.getElementById('btnAttendanceClockIn');
    if (clockInBtn) clockInBtn.addEventListener('click', () => submitClockAction('clock_in'));
    const clockOutBtn = document.getElementById('btnAttendanceClockOut');
    if (clockOutBtn) clockOutBtn.addEventListener('click', () => submitClockAction('clock_out'));
    const openCorrectionBtn = document.getElementById('btnOpenAttendanceCorrection');
    if (openCorrectionBtn) {
      openCorrectionBtn.addEventListener('click', () => {
        document.getElementById('attendanceRecordId').value = '';
        const canvasEl = document.getElementById('attendanceCorrectionCanvas');
        if (canvasEl && window.bootstrap) {
          bootstrap.Offcanvas.getOrCreateInstance(canvasEl).show();
          loadEffectiveShiftPreview();
        }
      });
    }
    document.addEventListener('click', (e) => {
      if (e.target.closest('.js-self-clock-in')) { submitClockAction('clock_in'); return; }
      if (e.target.closest('.js-self-clock-out')) { submitClockAction('clock_out'); return; }
      const correctBtn = e.target.closest('.js-correct-record');
      if (correctBtn) {
        document.getElementById('attendanceRecordId').value = correctBtn.getAttribute('data-record-id') || '';
        document.getElementById('attendanceRecordUser').value = correctBtn.getAttribute('data-user-id') || '';
        document.getElementById('attendanceRecordDate').value = correctBtn.getAttribute('data-record-date') || '';
        document.getElementById('attendanceRecordStatus').value = correctBtn.getAttribute('data-status') || '';
        document.getElementById('attendanceRecordClockIn').value = correctBtn.getAttribute('data-clock-in') || '';
        document.getElementById('attendanceRecordClockOut').value = correctBtn.getAttribute('data-clock-out') || '';
        document.getElementById('attendanceRecordNotes').value = correctBtn.getAttribute('data-notes') || '';
        showAlert('attendanceCorrectionAlert', '', '');
        const canvasEl = document.getElementById('attendanceCorrectionCanvas');
        if (canvasEl && window.bootstrap) {
          bootstrap.Offcanvas.getOrCreateInstance(canvasEl).show();
        }
        loadEffectiveShiftPreview();
        return;
      }

      const reviewTrigger = e.target.closest('.js-review-overtime');
      if (reviewTrigger) {
        document.getElementById('attendanceOvertimeReviewId').value = reviewTrigger.getAttribute('data-id') || '';
        document.getElementById('attendanceOvertimeReviewAction').value = reviewTrigger.getAttribute('data-action') || '';
        document.getElementById('attendanceOvertimeReviewNote').value = '';
        showAlert('attendanceOvertimeReviewAlert', '', '');
        const modalEl = document.getElementById('attendanceOvertimeReviewModal');
        if (modalEl && window.bootstrap) {
          bootstrap.Modal.getOrCreateInstance(modalEl).show();
        }
      }
    });
    document.querySelectorAll('a[data-bs-toggle="tab"]').forEach((tabLink) => {
      tabLink.addEventListener('shown.bs.tab', (e) => {
        const href = e.target.getAttribute('href') || '';
        if (href === '#attendanceMonthlyTab') loadMonthlyReport();
        if (href === '#attendanceOvertimeTab') loadOvertimeRequests();
      });
    });
  }

  initStaticFilters();
  bindEvents();
  loadDailyView();
})();
