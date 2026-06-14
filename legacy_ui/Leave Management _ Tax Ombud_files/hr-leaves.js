/* eslint-disable */
  const hrLeavesConfig = window.hrLeavesConfig || {};
  const myLeaveBody = document.getElementById('myLeaveBody');
  const allLeaveBody = document.getElementById('allLeaveBody');
  const canManageLeaves = Boolean(hrLeavesConfig.canManageLeaves);
  const canCreateLeave = Boolean(hrLeavesConfig.canCreateLeave);
  const currentUserId = hrLeavesConfig.currentUserId || null;

  // Lazy-init flags: allLeavesTable lives in a hidden tab. Initialising DataTables
  // on a hidden element causes "_DT_CellIndex undefined" because it can't measure columns.
  let allLeavesTabVisible = false;
  let allLeavesDataReady = false;

  function initAllLeavesTable() {
    if (!allLeavesTabVisible || !allLeavesDataReady) return;
    if (!window.$ || !$.fn.DataTable || !document.getElementById('allLeavesTable')) return;
    if (allLeavesDt) { allLeavesDt.destroy(); allLeavesDt = null; }
    allLeavesDt = $('#allLeavesTable').DataTable({
      destroy: true,
      language: { emptyTable: 'No data available' }
    });
  }

  function normalizeInputDate(val) {
    if (!val) return '';
    return String(val).replace(' ', 'T').slice(0, 16);
  }

  function formatDisplayDate(val) {
    if (!val) return '-';
    const norm = normalizeInputDate(val);
    return norm.replace('T', ' ');
  }

  function formatDuration(startVal, endVal) {
    if (!startVal || !endVal) return '-';
    const start = new Date(normalizeInputDate(startVal));
    const end = new Date(normalizeInputDate(endVal));
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return '-';
    let diffMs = end.getTime() - start.getTime();
    if (diffMs < 0) diffMs = 0;
    const totalMinutes = Math.floor(diffMs / (1000 * 60));
    const totalHours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    if (totalHours < 24) {
      if (totalHours <= 0) return `${minutes} min`;
      return minutes > 0 ? `${totalHours}h ${minutes}m` : `${totalHours}h`;
    }
    const days = Math.floor(totalHours / 24);
    const hours = totalHours % 24;
    if (hours <= 0) return `${days}d`;
    return `${days}d ${hours}h`;
  }

  const canEditAll = Boolean(hrLeavesConfig.canEditAll);
  let myLeavesDt = null;
  let allLeavesDt = null;

  function esc(value) {
    return String(value == null ? '' : value).replace(/[&<>"']/g, function (m) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m];
    });
  }

  function statusPriority(status) {
    const normalized = String(status || '').trim().toLowerCase();
    if (normalized === 'pending_confirmation') return 0;
    if (normalized === 'pending') return 1;
    if (normalized === 'approved') return 2;
    if (normalized === 'rejected') return 3;
    if (normalized === 'cancelled') return 4;
    return 5;
  }

  function windowStatus(startVal, endVal) {
    const today = new Date();
    today.setHours(0,0,0,0);
    const start = new Date(normalizeInputDate(startVal));
    const end = new Date(normalizeInputDate(endVal));
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return 'Unknown';
    if (end < today) return 'Past';
    if (start > today) return 'Upcoming';
    return 'Ongoing';
  }

  function setBtnLoading(btn, isLoading) {
    if (!btn) return;
    if (isLoading) {
      btn.dataset.originalHtml = btn.innerHTML;
      btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Loading...';
      btn.disabled = true;
      return;
    }
    if (btn.dataset.originalHtml) {
      btn.innerHTML = btn.dataset.originalHtml;
      delete btn.dataset.originalHtml;
    }
    btn.disabled = false;
  }

  function extractApiError(payload) {
    if (!payload || typeof payload !== 'object') return '';
    const msg = [
      payload.message,
      payload.error,
      payload.details,
      payload.reason
    ].find((v) => typeof v === 'string' && v.trim() !== '');
    return msg ? msg.trim() : '';
  }

  async function loadLeaves() {
    const myUrl = currentUserId ? `api/modules/hr/leaves?user_id=${encodeURIComponent(currentUserId)}` : 'api/modules/hr/leaves';
    const myRes = await fetch(myUrl, {credentials:'same-origin'});
    const myData = await myRes.json();
    if (myLeavesDt) { myLeavesDt.destroy(); myLeavesDt = null; }
    if (!myData.data || !myData.data.length) {
      myLeaveBody.innerHTML = '<tr><td colspan="10" class="text-center text-muted">No leave requests</td></tr>';
    } else {
      myLeaveBody.innerHTML = myData.data.map((row) => `
        <tr>
          <td>${row.id}</td>
          <td>${row.leave_type || '-'}</td>
          <td>${formatDisplayDate(row.start_date)}</td>
          <td>${formatDisplayDate(row.end_date)}</td>
          <td>${row.days_count ?? '-'}</td>
          <td>${formatDuration(row.start_date, row.end_date)}</td>
          <td><span class="badge ${windowStatus(row.start_date, row.end_date)==='Ongoing'?'bg-success-subtle text-success':windowStatus(row.start_date, row.end_date)==='Upcoming'?'bg-info-subtle text-info':'bg-secondary-subtle text-secondary'}">${windowStatus(row.start_date, row.end_date)}</span></td>
          <td>
            ${row.status}
          </td>
          <td>${row.reason || '-'}</td>
          <td>
            ${row.status === 'pending' ? `
              <div class="leave-actions">
                <button class="btn btn-sm btn-primary leave-edit"
                  data-id="${row.id}"
                  data-type="${row.leave_type || 'annual'}"
                  data-start="${row.start_date || ''}"
                  data-end="${row.end_date || ''}"
                  data-reason="${row.reason || ''}"
                  title="Edit leave request"><i class="ri-edit-2-line me-1"></i>Edit</button>
                <button class="btn btn-sm btn-outline-danger leave-cancel" data-id="${row.id}" title="Cancel leave request"><i class="ri-close-circle-line me-1"></i>Cancel</button>
              </div>
            ` : '<span class="text-muted small">—</span>'}
          </td>
        </tr>
      `).join('');
    }

    if (canEditAll && allLeaveBody) {
      const allUrl = canEditAll ? 'api/modules/hr/leaves?all=1' : 'api/modules/hr/leaves';
      const allRes = await fetch(allUrl, {credentials:'same-origin'});
      const allData = await allRes.json();
      const allRows = Array.isArray(allData.data) ? allData.data.slice() : [];
      const windowRank = (row) => {
        const w = windowStatus(row.start_date, row.end_date);
        if (w === 'Upcoming') return 0;
        if (w === 'Ongoing') return 1;
        if (w === 'Past') return 2;
        return 3;
      };
      const parseTs = (val) => {
        const t = new Date(normalizeInputDate(val)).getTime();
        return Number.isFinite(t) ? t : Number.MAX_SAFE_INTEGER;
      };
      allRows.sort((a, b) => {
        const statusDiff = statusPriority(a.status) - statusPriority(b.status);
        if (statusDiff !== 0) return statusDiff;
        const rankDiff = windowRank(a) - windowRank(b);
        if (rankDiff !== 0) return rankDiff;
        const timeDiff = parseTs(a.start_date) - parseTs(b.start_date);
        if (timeDiff !== 0) return timeDiff;
        return (Number(b.id) || 0) - (Number(a.id) || 0);
      });
      if (allLeavesDt) { allLeavesDt.destroy(); allLeavesDt = null; }
      if (!allRows.length) {
        allLeaveBody.innerHTML = '<tr><td colspan="12" class="text-center text-muted">No data available</td></tr>';
      } else {
        allLeaveBody.innerHTML = allRows.map((row) => `
          <tr>
            <td>${row.id}</td>
            <td>
              <div class="d-flex flex-column">
                <span>${esc(row.user_name || '-')}</span>
                ${row.department_name ? `<span class="badge bg-light text-body border align-self-start mt-1">${esc(row.department_name)}</span>` : ''}
              </div>
            </td>
            <td>${row.leave_type || '-'}</td>
            <td>${formatDisplayDate(row.start_date)}</td>
            <td>${formatDisplayDate(row.end_date)}</td>
            <td>${row.days_count ?? '-'}</td>
            <td>${formatDuration(row.start_date, row.end_date)}</td>
            <td><span class="badge ${windowStatus(row.start_date, row.end_date)==='Ongoing'?'bg-success-subtle text-success':windowStatus(row.start_date, row.end_date)==='Upcoming'?'bg-info-subtle text-info':'bg-secondary-subtle text-secondary'}">${windowStatus(row.start_date, row.end_date)}</span></td>
            <td>${row.status}</td>
            <td>${row.payroll_deduction_type || 'none'}</td>
            <td>${row.reason || '-'}</td>
            <td>
              ${row.status === 'pending' ? `
                <div class="leave-actions">
                  ${canEditAll ? `
                    <button class="btn btn-sm btn-outline-primary btn-icon leave-edit"
                      data-id="${row.id}"
                      data-type="${row.leave_type || 'annual'}"
                      data-start="${row.start_date || ''}"
                      data-end="${row.end_date || ''}"
                      data-reason="${row.reason || ''}"
                      aria-label="Edit request"
                      title="Edit request"><i class="ri-edit-2-line"></i></button>
                  ` : ''}
                  ${canManageLeaves ? `
                    <button class="btn btn-sm btn-success btn-icon leave-approve" data-id="${row.id}" aria-label="Approve request" title="Approve request"><i class="ri-check-line"></i></button>
                    <button class="btn btn-sm btn-outline-danger btn-icon leave-reject" data-id="${row.id}" aria-label="Reject request" title="Reject request"><i class="ri-close-line"></i></button>
                  ` : ''}
                </div>
              ` : '<span class="text-muted small">—</span>'}
            </td>
          </tr>
        `).join('');
      }
    }

    if (window.$ && $.fn.DataTable) {
      myLeavesDt = $('#myLeavesTable').DataTable({
        destroy: true,
        language: { emptyTable: 'No leave requests' }
      });
    }
    // allLeavesTable is in a hidden tab — init it lazily to avoid _DT_CellIndex crash
    allLeavesDataReady = true;
    initAllLeavesTable();
  }

  let currentEditId = null;

  function resetLeaveForm() {
    currentEditId = null;
    document.getElementById('leaveModalTitle').textContent = 'New Leave Request';
    const leaveTypeSelect = document.getElementById('leave_type');
    const leaveTypeIdSelect = document.getElementById('leave_type_id');
    if (leaveTypeSelect) leaveTypeSelect.value = 'annual';
    if (leaveTypeIdSelect) leaveTypeIdSelect.value = '';
    document.getElementById('leave_start').value = '';
    document.getElementById('leave_end').value = '';
    document.getElementById('leave_reason').value = '';
    document.getElementById('leaveAlert').classList.add('d-none');
  }

  document.getElementById('leaveRequestModal').addEventListener('hidden.bs.modal', resetLeaveForm);

  document.getElementById('leaveSubmitBtn').addEventListener('click', async () => {
    if (!currentEditId && !canCreateLeave) {
      if (window.crmUiAlert) {
        window.crmUiAlert('You do not have permission to create leave requests.', 'Permission Required');
      }
      return;
    }
    const leaveTypeSelect = document.getElementById('leave_type');
    const leaveTypeIdSelect = document.getElementById('leave_type_id');
    const payload = {
      start_date: document.getElementById('leave_start').value,
      end_date: document.getElementById('leave_end').value,
      reason: document.getElementById('leave_reason').value.trim()
    };
    if (leaveTypeIdSelect) {
      payload.leave_type_id = leaveTypeIdSelect.value || '';
      const selected = leaveTypeIdSelect.options[leaveTypeIdSelect.selectedIndex];
      if (selected && selected.text) {
        payload.leave_type = selected.text.replace(/\s*\(Unpaid\)\s*$/i, '').trim();
      }
    } else {
      payload.leave_type = leaveTypeSelect ? leaveTypeSelect.value : 'annual';
    }
    const alertEl = document.getElementById('leaveAlert');
    const startVal = payload.start_date || '';
    const endVal = payload.end_date || '';
    if (!startVal || !endVal) {
      alertEl.textContent = 'Start Date & Time and End Date & Time are required.';
      alertEl.classList.remove('d-none');
      return;
    }
    const startDate = new Date(startVal);
    const endDate = new Date(endVal);
    if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) {
      alertEl.textContent = 'Please enter valid Start and End date/time values.';
      alertEl.classList.remove('d-none');
      return;
    }
    if (endDate < startDate) {
      alertEl.textContent = 'End Date & Time must be after Start Date & Time.';
      alertEl.classList.remove('d-none');
      return;
    }
    let res;
    if (currentEditId) {
      payload.id = currentEditId;
      res = await fetch('api/modules/hr/leaves', {
        method: 'PATCH',
        headers: {'Content-Type': 'application/json'},
        credentials: 'same-origin',
        body: JSON.stringify(payload)
      });
    } else {
      res = await fetch('api/modules/hr/leaves', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        credentials: 'same-origin',
        body: JSON.stringify(payload)
      });
    }
    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      const msg = extractApiError(err) || 'Unable to submit request';
      alertEl.textContent = msg;
      alertEl.classList.remove('d-none');
      return;
    }
    alertEl.classList.add('d-none');
    bootstrap.Modal.getInstance(document.getElementById('leaveRequestModal')).hide();
    loadLeaves();
  });

  if (allLeaveBody) allLeaveBody.addEventListener('click', async (e) => {
    const approve = e.target.closest('.leave-approve');
    const reject = e.target.closest('.leave-reject');
    const edit = e.target.closest('.leave-edit');
    if (!approve && !reject && !edit) return;
    if (edit) {
      setBtnLoading(edit, true);
      const id = edit.getAttribute('data-id');
      const type = edit.getAttribute('data-type') || 'annual';
      const start = normalizeInputDate(edit.getAttribute('data-start') || '');
      const end = normalizeInputDate(edit.getAttribute('data-end') || '');
      const reason = edit.getAttribute('data-reason') || '';
      currentEditId = parseInt(id, 10);
      document.getElementById('leaveModalTitle').textContent = 'Edit Leave Request';
      const leaveTypeSelect = document.getElementById('leave_type');
      const leaveTypeIdSelect = document.getElementById('leave_type_id');
      if (leaveTypeIdSelect) {
        leaveTypeIdSelect.value = '';
        const options = Array.from(leaveTypeIdSelect.options || []);
        const match = options.find(opt => (opt.textContent || '').toLowerCase().replace(/\s*\(unpaid\)\s*$/i, '').trim() === String(type || '').toLowerCase().trim());
        if (match) leaveTypeIdSelect.value = match.value;
      } else if (leaveTypeSelect) {
        leaveTypeSelect.value = type || 'annual';
      }
      document.getElementById('leave_start').value = start;
      document.getElementById('leave_end').value = end;
      document.getElementById('leave_reason').value = reason === '-' ? '' : reason;
      const modal = new bootstrap.Modal(document.getElementById('leaveRequestModal'));
      modal.show();
      setTimeout(() => setBtnLoading(edit, false), 200);
      return;
    }
    const reviewOk = await window.crmUiConfirm(
      approve ? 'Approve this leave request?' : 'Deny this leave request?',
      approve ? 'Approve Leave Request' : 'Deny Leave Request',
      {
        okText: approve ? 'Approve' : 'Deny',
        cancelText: 'Cancel',
        variant: approve ? 'primary' : 'danger',
        icon: 'warning'
      }
    );
    if (!reviewOk) {
      return;
    }
    const id = (approve || reject).getAttribute('data-id');
    const status = approve ? 'approved' : 'rejected';
    await fetch('api/modules/hr/leaves', {
      method: 'PATCH',
      headers: {'Content-Type': 'application/json'},
      credentials: 'same-origin',
      body: JSON.stringify({id, status})
    });
    loadLeaves();
  });

  myLeaveBody.addEventListener('click', async (e) => {
    const edit = e.target.closest('.leave-edit');
    const cancel = e.target.closest('.leave-cancel');
    if (edit) {
      setBtnLoading(edit, true);
      const id = edit.getAttribute('data-id');
      const row = edit.closest('tr');
      if (!row) return;
      const type = edit.getAttribute('data-type') || 'annual';
      const start = normalizeInputDate(edit.getAttribute('data-start') || '');
      const end = normalizeInputDate(edit.getAttribute('data-end') || '');
      const reason = edit.getAttribute('data-reason') || '';
      currentEditId = parseInt(id, 10);
      document.getElementById('leaveModalTitle').textContent = 'Edit Leave Request';
      const leaveTypeSelect = document.getElementById('leave_type');
      const leaveTypeIdSelect = document.getElementById('leave_type_id');
      if (leaveTypeIdSelect) {
        leaveTypeIdSelect.value = '';
        const options = Array.from(leaveTypeIdSelect.options || []);
        const match = options.find(opt => (opt.textContent || '').toLowerCase().replace(/\s*\(unpaid\)\s*$/i, '').trim() === String(type || '').toLowerCase().trim());
        if (match) leaveTypeIdSelect.value = match.value;
      } else if (leaveTypeSelect) {
        leaveTypeSelect.value = type || 'annual';
      }
      document.getElementById('leave_start').value = start;
      document.getElementById('leave_end').value = end;
      document.getElementById('leave_reason').value = reason === '-' ? '' : reason;
      const modal = new bootstrap.Modal(document.getElementById('leaveRequestModal'));
      modal.show();
      setTimeout(() => setBtnLoading(edit, false), 200);
      return;
    }
    if (!cancel) return;
    const cancelOk = await window.crmUiConfirm('Cancel this leave request?', 'Cancel Leave Request', {
      okText: 'Cancel Request',
      cancelText: 'Keep',
      variant: 'danger',
      icon: 'warning'
    });
    if (!cancelOk) {
      return;
    }
    const id = cancel.getAttribute('data-id');
    await fetch('api/modules/hr/leaves', {
      method: 'PATCH',
      headers: {'Content-Type': 'application/json'},
      credentials: 'same-origin',
      body: JSON.stringify({id, status: 'cancelled'})
    });
    loadLeaves();
  });

  loadLeaves();

  // When the "All Requests" tab is shown, initialize its DataTable.
  // (Can't do it on page load — hidden elements cause _DT_CellIndex crash.)
  document.querySelector('a[href="#allLeaves"]')?.addEventListener('shown.bs.tab', function () {
    allLeavesTabVisible = true;
    initAllLeavesTable();
  });
