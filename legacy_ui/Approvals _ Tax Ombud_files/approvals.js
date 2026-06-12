/* eslint-disable */
(function () {
  const cfg = window.approvalsConfig || {};
  const agentId = Number(cfg.agentId || 0);
  const canApproveAll = !!cfg.canApproveAll;
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const root = (typeof url_root !== 'undefined' ? url_root : '/');
  const apiApprovals = apiMap.approvalsIndex || (root + 'api/modules/approvals/index');
  const apiApprovalDetail = apiMap.approvalsDetail || (root + 'api/modules/approvals/detail');
  const pendingCountBadge = document.getElementById('approvalPendingCountBadge');

  const statusFilter = document.getElementById('approvalStatusFilter');
  const viewFilter = document.getElementById('approvalViewFilter');
  const btnFilter = document.getElementById('approvalFilterBtn');
  const alertBox = document.getElementById('approvalAlert');

  let dt = null;

  function showAlert(msg, type = 'danger') {
    if (!alertBox) return;
    if (!msg) { alertBox.className = 'alert alert-danger d-none'; alertBox.textContent = ''; return; }
    alertBox.className = 'alert alert-' + type;
    alertBox.textContent = msg;
    alertBox.classList.remove('d-none');
  }

  function statusBadge(status) {
    const map = {
      pending: 'bg-warning-subtle text-warning',
      approved: 'bg-success-subtle text-success',
      rejected: 'bg-danger-subtle text-danger',
      cancelled: 'bg-secondary-subtle text-secondary'
    };
    const key = (status || '').toLowerCase();
    return `<span class="badge ${map[key] || 'bg-light text-muted'}">${key || '-'}</span>`;
  }

  function requestStatusBadge(status) {
    const map = {
      resolved: 'bg-dark text-white',
      closed: 'bg-dark text-white',
      approved: 'bg-success-subtle text-success',
      pending: 'bg-warning-subtle text-warning',
      rejected: 'bg-danger-subtle text-danger',
      cancelled: 'bg-secondary-subtle text-secondary'
    };
    const key = (status || '').toLowerCase();
    return `<span class="badge ${map[key] || 'bg-light text-muted'} text-uppercase">${key || '-'}</span>`;
  }

  function fmtDate(val) {
    if (!val) return '-';
    return String(val).replace('T', ' ').slice(0, 16);
  }

  function setPendingCount(count) {
    if (!pendingCountBadge) return;
    pendingCountBadge.textContent = 'Approvals (' + Math.max(0, Number(count || 0)) + ')';
  }

  function buildQueryString() {
    const params = new URLSearchParams();
    if (statusFilter && statusFilter.value) params.set('status', statusFilter.value);
    if (viewFilter && viewFilter.value === 'approver') params.set('approver', '1');
    if (viewFilter && viewFilter.value === 'mine') params.set('mine', '1');
    params.set('limit', '2000');
    return params.toString();
  }

  function initTable(rows) {
    if ($.fn.DataTable.isDataTable('#approvalTable')) {
      $('#approvalTable').DataTable().clear().destroy();
    }
    $('#approvalTable tbody').empty();

    dt = $('#approvalTable').DataTable({
      destroy: true,
      data: rows,
      pageLength: 25,
      lengthMenu: [10, 25, 50, 100],
      order: [[6, 'desc']],
      language: { emptyTable: 'No approvals found.' },
      columns: [
        {
          className: 'approval-case-col',
          data: null,
          render: function (r) {
            const label = r.case_number || ('Case #' + (r.entity_id || ''));
            const href = r.entity_id_salt ? `studio/cases/view.kml?id=${encodeURIComponent(r.entity_id_salt)}` : '#';
            return `<a href="${href}" class="text-decoration-underline">${label}</a>`;
          }
        },
        {
          data: null,
          render: function (r) {
            const href = r.entity_id_salt ? `studio/cases/view.kml?id=${encodeURIComponent(r.entity_id_salt)}` : '#';
            return `<a href="${href}" class="text-decoration-underline">${r.subject || '-'}</a>`;
          }
        },
        {
          data: null,
          render: function (r) {
            const reqStatus = (r.requested_status || r.action || '').toString();
            const note = r.requested_note ? String(r.requested_note) : '';
            const top = reqStatus ? `<div class="mb-1">${requestStatusBadge(reqStatus)}</div>` : '';
            const bot = note ? `<div class="small text-muted">${note}</div>` : '<div class="small text-muted">-</div>';
            return top + bot;
          }
        },
        {
          data: 'status',
          render: function (val) { return statusBadge(val); }
        },
        {
          data: null,
          render: function (r) { return r.approver_name || ('Agent #' + (r.approver_id || '')); }
        },
        {
          data: null,
          render: function (r) { return r.requester_name || ('Agent #' + (r.requested_by || '')); }
        },
        {
          data: 'created_at',
          render: function (val) { return fmtDate(val); }
        },
        {
          className: 'approval-actions-col',
          data: null,
          orderable: false,
          render: function (r) {
            const href = r.entity_id_salt ? `studio/cases/view.kml?id=${encodeURIComponent(r.entity_id_salt)}` : '#';
            const btns = [];
            if (r.status === 'pending') {
              const isApprover = agentId && Number(r.approver_id) === agentId;
              const isRequester = agentId && Number(r.requested_by) === agentId;
              if (isApprover || canApproveAll) {
                btns.push(`<button class="btn btn-sm btn-primary approval-action flex-shrink-0 text-nowrap" data-id="${r.id}" data-action="approved"><i class="ri-check-line me-1"></i>Approve</button>`);
                btns.push(`<a href="${href}" class="btn btn-sm btn-outline-primary flex-shrink-0" title="View case" aria-label="View case"><i class="ri-eye-line"></i></a>`);
                btns.push(`<button class="btn btn-sm btn-outline-primary approval-action flex-shrink-0" data-id="${r.id}" data-action="rejected" title="Reject" aria-label="Reject"><i class="ri-close-line"></i></button>`);
              } else {
                btns.push(`<a href="${href}" class="btn btn-sm btn-outline-primary flex-shrink-0" title="View case" aria-label="View case"><i class="ri-eye-line"></i></a>`);
              }
              if (isRequester) {
                btns.push(`<button class="btn btn-sm btn-light approval-action flex-shrink-0" data-id="${r.id}" data-action="cancelled" title="Cancel request" aria-label="Cancel request"><i class="ri-close-circle-line"></i></button>`);
              }
            } else {
              btns.push(`<a href="${href}" class="btn btn-sm btn-outline-primary flex-shrink-0" title="View case" aria-label="View case"><i class="ri-eye-line"></i></a>`);
            }
            return `<div class="d-inline-flex align-items-center gap-1 flex-nowrap approval-actions-toolbar">${btns.join('')}</div>`;
          }
        }
      ]
    });
  }

  async function loadApprovals() {
    showAlert('');
    try {
      const res = await fetch(apiApprovals + '?' + buildQueryString());
      const data = await res.json().catch(() => ({}));
      if (!res.ok) {
        showAlert(data.error || data.message || 'Unable to load approvals');
        initTable([]);
        return;
      }
      initTable(data.data || []);
    } catch (e) {
      showAlert('Unable to load approvals');
      initTable([]);
    }
  }

  async function loadPendingCount() {
    if (!pendingCountBadge) return;
    try {
      const res = await fetch(apiApprovals + '?status=pending&approver=1&limit=1');
      const data = await res.json().catch(() => ({}));
      if (res.ok) setPendingCount(data.total || 0);
    } catch (e) { /* keep last */ }
  }

  async function updateApproval(id, status, trigger) {
    if (!id || !status) return;
    if (trigger && trigger.dataset.busy === '1') return;
    showAlert('');
    if (trigger) {
      trigger.dataset.busy = '1';
      if (window.toggleButtonLoading) window.toggleButtonLoading(trigger, true, '...');
    }
    try {
      const res = await fetch(apiApprovalDetail, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id, status })
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) { showAlert(data.error || data.message || 'Unable to update approval'); return; }
      const modalTitle = status === 'approved' ? 'Approved' : status === 'rejected' ? 'Rejected' : 'Updated';
      const modalMsg   = status === 'approved' ? 'The case has been approved and closed successfully.'
                       : status === 'rejected'  ? 'The approval request has been rejected.'
                       : 'Approval request cancelled.';
      if (typeof window.showSavedModal === 'function') window.showSavedModal(modalTitle, modalMsg);
      await Promise.all([loadApprovals(), loadPendingCount()]);
    } finally {
      if (trigger) {
        trigger.dataset.busy = '0';
        if (window.toggleButtonLoading) window.toggleButtonLoading(trigger, false);
      }
    }
  }

  document.addEventListener('click', (e) => {
    const btn = e.target.closest('.approval-action');
    if (!btn) return;
    updateApproval(btn.getAttribute('data-id'), btn.getAttribute('data-action'), btn);
  });

  if (btnFilter) {
    btnFilter.addEventListener('click', async () => {
      if (window.toggleButtonLoading) window.toggleButtonLoading(btnFilter, true, 'Filtering...');
      try { await loadApprovals(); }
      finally { if (window.toggleButtonLoading) window.toggleButtonLoading(btnFilter, false); }
    });
  }

  loadPendingCount();
  loadApprovals();
})();
