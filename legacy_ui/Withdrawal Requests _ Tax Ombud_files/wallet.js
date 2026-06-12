/**
 * Wallet Module JS
 * Handles: WalletAdmin (index), WalletDetail (detail), WalletWithdrawals (withdrawals)
 */

(function () {
  'use strict';

  /* ------------------------------------------------------------------ helpers */
  const fmt = (v, cur) => {
    const n = v != null ? Number(v).toLocaleString('en-NG', {minimumFractionDigits: 2}) : '-';
    return cur ? `${cur} ${n}` : n;
  };
  const fmtDate = (v) => v ? String(v).slice(0, 19).replace('T', ' ') : '-';

  function statusBadge(status) {
    const map = {
      active:    'bg-success',
      frozen:    'bg-warning text-dark',
      closed:    'bg-dark',
      pending:   'bg-secondary',
      approved:  'bg-info text-dark',
      paid:      'bg-success',
      rejected:  'bg-danger',
      cancelled: 'bg-dark',
      credit:    'bg-success',
      debit:     'bg-danger',
    };
    return `<span class="badge ${map[status] || 'bg-secondary'}">${status || '-'}</span>`;
  }

  function spinBtn(btn, on) {
    const lbl = btn.querySelector('.label');
    const sp  = btn.querySelector('.spinner-border');
    if (on) { btn.disabled = true;  lbl && (lbl.style.display = 'none'); sp && sp.classList.remove('d-none'); }
    else    { btn.disabled = false; lbl && (lbl.style.display = '');     sp && sp.classList.add('d-none'); }
  }

  function showAlert(el, msg, type = 'danger') {
    if (!el) return;
    el.className = `alert alert-${type}`;
    el.textContent = msg;
    el.classList.remove('d-none');
  }

  function hideAlert(el) {
    if (!el) return;
    el.classList.add('d-none');
    el.textContent = '';
  }

  async function apiFetch(url, opts = {}) {
    const res = await fetch(url, { credentials: 'same-origin', ...opts });
    const data = await res.json().catch(() => ({}));
    return { ok: res.ok, status: res.status, data };
  }

  /* ================================================================ WALLET ADMIN */
  function initWalletAdmin() {
    const cfg     = window.walletAdminConfig || {};
    const alertEl = document.getElementById('waAlert');
    let dt;

    async function loadWallets() {
      const r = await apiFetch('api/modules/wallet/index');
      if (!r.ok) { showAlert(alertEl, r.data.error || 'Failed to load wallets'); return; }
      const wallets = r.data.data   || [];
      const totals  = r.data.totals || {};

      const el = (id) => document.getElementById(id);
      if (el('waSummaryCount'))   el('waSummaryCount').textContent   = wallets.length;
      if (el('waSummaryBalance')) el('waSummaryBalance').textContent = fmt(totals.total_balance || 0, 'NGN');
      if (el('waSummaryActive'))  el('waSummaryActive').textContent  = wallets.filter(w => w.status === 'active').length;
      if (el('waSummaryPending')) el('waSummaryPending').textContent = wallets.reduce((s, w) => s + (parseInt(w.pending_withdrawals, 10) || 0), 0);

      if (window.$ && $.fn.DataTable && $.fn.DataTable.isDataTable('#walletsTable')) {
        $('#walletsTable').DataTable().destroy();
      }
      dt = null;
      const tbody = document.querySelector('#walletsTable tbody');
      tbody.innerHTML = wallets.map(w => {
        const userLink = w.user_id_s
          ? `<a href="studio/hr/staff-view.kml?user_id=${encodeURIComponent(w.user_id_s)}" class="fw-semibold text-primary">${w.employee_name || ('User #' + w.user_id)}</a>`
          : `<span class="fw-semibold">${w.employee_name || w.user_id || '-'}</span>`;
        return `
          <tr>
            <td>${userLink}</td>
            <td class="fw-bold">${fmt(w.balance, w.currency || 'NGN')}</td>
            <td>${w.currency || 'NGN'}</td>
            <td>${statusBadge(w.status)}</td>
            <td><span class="badge bg-warning-subtle text-warning">${w.pending_withdrawals || 0}</span></td>
            <td>${fmtDate(w.updated_at)}</td>
            <td class="text-nowrap">
              <a href="studio/wallet/detail.kml?user_id=${w.user_id}" class="btn btn-sm btn-soft-primary me-1" title="View">
                <i class="ri-eye-line me-1"></i>View
              </a>
              ${cfg.canManage ? `
                <button class="btn btn-sm btn-soft-success wa-credit me-1" data-uid="${w.user_id}" data-name="${w.employee_name || w.user_id}" title="Credit Wallet">
                  <i class="ri-add-circle-line me-1"></i>Credit
                </button>
                <button class="btn btn-sm btn-soft-secondary wa-status" data-wid="${w.id}" data-name="${w.employee_name || w.user_id}" data-status="${w.status}" title="Change Status">
                  <i class="ri-settings-3-line me-1"></i>Status
                </button>
              ` : ''}
            </td>
          </tr>
        `;
      }).join('');
      dt = window.$ && $.fn.DataTable ? $('#walletsTable').DataTable({ order: [[5, 'desc']] }) : null;
    }

    loadWallets();

    // Credit modal
    const creditModalEl  = document.getElementById('creditModal');
    const bsCreditModal  = creditModalEl ? new bootstrap.Modal(creditModalEl) : null;
    const creditAlert    = document.getElementById('creditModalAlert');
    const creditSaveBtn  = document.getElementById('creditSaveBtn');

    const table = document.getElementById('walletsTable');
    if (table) table.addEventListener('click', (e) => {
      const creditBtn = e.target.closest('.wa-credit');
      const statusBtn = e.target.closest('.wa-status');

      if (creditBtn) {
        document.getElementById('credit_user_id').value   = creditBtn.dataset.uid;
        document.getElementById('credit_user_name').value = creditBtn.dataset.name;
        document.getElementById('credit_amount').value    = '';
        document.getElementById('credit_note').value      = '';
        document.getElementById('credit_reference').value = '';
        hideAlert(creditAlert);
        bsCreditModal && bsCreditModal.show();
      }

      if (statusBtn) {
        document.getElementById('ws_wallet_id').value       = statusBtn.dataset.wid;
        document.getElementById('ws_user_name').textContent = statusBtn.dataset.name;
        document.getElementById('ws_status').value          = statusBtn.dataset.status;
        hideAlert(document.getElementById('wsAlert'));
        new bootstrap.Modal(document.getElementById('walletStatusModal')).show();
      }
    });

    if (creditSaveBtn) creditSaveBtn.addEventListener('click', async () => {
      const uid    = document.getElementById('credit_user_id').value;
      const amount = parseFloat(window.moneyVal(document.getElementById('credit_amount')));
      const note   = document.getElementById('credit_note').value.trim();
      const ref    = document.getElementById('credit_reference').value.trim();
      if (!amount || amount <= 0) { showAlert(creditAlert, 'Enter a valid amount'); return; }
      if (!note) { showAlert(creditAlert, 'Note is required'); return; }
      spinBtn(creditSaveBtn, true);
      const payload = { user_id: parseInt(uid, 10), amount, note };
      if (ref) payload.reference = ref;
      const r = await apiFetch('api/modules/wallet/credit', {
        method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(payload)
      });
      spinBtn(creditSaveBtn, false);
      if (!r.ok) { showAlert(creditAlert, r.data.error || 'Credit failed'); return; }
      bsCreditModal && bsCreditModal.hide();
      showAlert(alertEl, 'Wallet credited successfully.', 'success');
      loadWallets();
    });

    const wsSaveBtn = document.getElementById('wsSaveBtn');
    if (wsSaveBtn) wsSaveBtn.addEventListener('click', async () => {
      const wid    = document.getElementById('ws_wallet_id').value;
      const status = document.getElementById('ws_status').value;
      spinBtn(wsSaveBtn, true);
      const r = await apiFetch(`api/modules/wallet/detail?id=${wid}`, {
        method: 'PATCH', headers: {'Content-Type':'application/json'}, body: JSON.stringify({ status })
      });
      spinBtn(wsSaveBtn, false);
      if (!r.ok) { showAlert(document.getElementById('wsAlert'), r.data.error || 'Update failed'); return; }
      bootstrap.Modal.getInstance(document.getElementById('walletStatusModal'))?.hide();
      loadWallets();
    });
  }

  /* ================================================================ WALLET DETAIL */
  function initWalletDetail() {
    const cfg = window.walletDetailConfig || {};
    if (!cfg.userId) return;

    async function loadDetail() {
      const r = await apiFetch(`api/modules/wallet/detail?user_id=${cfg.userId}`);
      if (!r.ok) return;
      const wallet = r.data;

      const el = (id) => document.getElementById(id);
      if (el('wdEmployeeName')) el('wdEmployeeName').textContent = wallet.employee_name || 'Wallet Detail';
      if (el('wdWalletStatus')) el('wdWalletStatus').textContent = `Status: ${wallet.status || '-'}`;
      if (el('wdBalance'))      el('wdBalance').textContent      = fmt(wallet.balance);
      if (el('wdCurrency'))     el('wdCurrency').textContent     = wallet.currency || 'NGN';
      const withdrawalRows = Array.isArray(wallet.withdrawal_requests) ? wallet.withdrawal_requests : [];
      const paidWithdrawnFallback = withdrawalRows
        .filter(w => String(w.status || '').toLowerCase() === 'paid')
        .reduce((sum, w) => sum + (parseFloat(w.amount) || 0), 0);
      const totalWithdrawn = (wallet.total_withdrawn != null) ? Number(wallet.total_withdrawn) : paidWithdrawnFallback;
      if (el('wdTotalWithdrawn')) el('wdTotalWithdrawn').textContent = fmt(totalWithdrawn, wallet.currency || 'NGN');
      const totalAmount = (parseFloat(wallet.balance) || 0) + totalWithdrawn;
      if (el('wdTotalAmount')) el('wdTotalAmount').textContent = fmt(totalAmount, wallet.currency || 'NGN');

      // Transactions
      const txBody = el('txBody');
      if (txBody) {
        const txs = wallet.transactions || [];
        txBody.innerHTML = txs.length ? txs.map(tx => `
          <tr>
            <td>${statusBadge(tx.type)}</td>
            <td class="${tx.type === 'credit' ? 'text-success' : 'text-danger'} fw-bold">${fmt(tx.amount)}</td>
            <td>${fmt(tx.balance_after)}</td>
            <td><small class="font-monospace">${tx.reference || '-'}</small></td>
            <td>${tx.note || '-'}</td>
            <td>${fmtDate(tx.created_at)}</td>
          </tr>
        `).join('') : '<tr><td colspan="6" class="text-center text-muted">No transactions.</td></tr>';

        if (window.$ && $.fn.DataTable) {
          if ($.fn.DataTable.isDataTable('#txTable')) $('#txTable').DataTable().destroy();
          $('#txTable').DataTable({ order: [[5, 'desc']] });
        }
      }

      // Withdrawals
      const wdBody = el('withdrawalDetailBody');
      if (wdBody) {
        const wds = withdrawalRows;
        wdBody.innerHTML = wds.length ? wds.map(w => `
          <tr>
            <td class="fw-bold">${fmt(w.amount)}</td>
            <td>${w.bank_name ? `${w.bank_name} ···${String(w.account_number||'').slice(-4)}` : '-'}</td>
            <td>${statusBadge(w.status)}</td>
            <td>${fmtDate(w.created_at)}</td>
            <td>${fmtDate(w.approved_at)}</td>
            <td>${fmtDate(w.paid_at)}</td>
            <td>
              ${w.status === 'pending' ? `<button class="btn btn-sm btn-outline-danger wd-cancel-btn" data-id="${w.id}">Cancel</button>` : '-'}
            </td>
          </tr>
        `).join('') : '<tr><td colspan="7" class="text-center text-muted">No withdrawal requests.</td></tr>';
      }
    }

    loadDetail();

    // Own cancel button
    const wdTabPanel = document.getElementById('wdTab');
    if (wdTabPanel) wdTabPanel.addEventListener('click', async (e) => {
      const cancelBtn = e.target.closest('.wd-cancel-btn');
      if (!cancelBtn) return;
      if (!(await crmUiConfirm('Cancel this withdrawal request?', 'Cancel Withdrawal', { okText: 'Yes, Cancel', variant: 'danger' }))) return;
      const r = await apiFetch(`api/modules/wallet/withdrawals?id=${cancelBtn.dataset.id}`, {
        method: 'PATCH', headers: {'Content-Type':'application/json'}, body: JSON.stringify({ status: 'cancelled' })
      });
      if (!r.ok) { crmUiAlert(r.data.error || 'Cancel failed', 'Wallet Withdrawal'); return; }
      loadDetail();
    });
  }

  /* ================================================================ WALLET WITHDRAWALS */
  function initWalletWithdrawals() {
    const cfg       = window.walletWithdrawalsConfig || {};
    const alertEl   = document.getElementById('wrAlert');
    let activeWorkflow = 'pending';
    const canSeeFullBank = !!(cfg.canPay || cfg.canManage);
    const quickPendingBtn = document.getElementById('wrQuickPending');
    const quickApprovedBtn = document.getElementById('wrQuickApproved');
    const quickPaidBtn = document.getElementById('wrQuickPaid');
    const quickAllBtn = document.getElementById('wrQuickAll');
    let dt;

    function setQuickActive(targetBtn) {
      [quickPendingBtn, quickApprovedBtn, quickPaidBtn, quickAllBtn].forEach((btn) => {
        if (!btn) return;
        if (btn === targetBtn) btn.classList.add('active');
        else btn.classList.remove('active');
      });
      if (quickPendingBtn) {
        quickPendingBtn.classList.toggle('btn-primary', quickPendingBtn.classList.contains('active'));
        quickPendingBtn.classList.toggle('btn-outline-primary', !quickPendingBtn.classList.contains('active'));
      }
      if (quickApprovedBtn) {
        quickApprovedBtn.classList.toggle('btn-primary', quickApprovedBtn.classList.contains('active'));
        quickApprovedBtn.classList.toggle('btn-outline-primary', !quickApprovedBtn.classList.contains('active'));
      }
      if (quickPaidBtn) {
        quickPaidBtn.classList.toggle('btn-primary', quickPaidBtn.classList.contains('active'));
        quickPaidBtn.classList.toggle('btn-outline-primary', !quickPaidBtn.classList.contains('active'));
      }
      if (quickAllBtn) {
        quickAllBtn.classList.toggle('btn-primary', quickAllBtn.classList.contains('active'));
        quickAllBtn.classList.toggle('btn-outline-primary', !quickAllBtn.classList.contains('active'));
      }
    }

    async function loadWorkflowCounts() {
      const r = await apiFetch('api/modules/wallet/withdrawals');
      if (!r.ok) return;
      const rows = Array.isArray(r.data.data) ? r.data.data : [];
      const counts = { pending: 0, approved: 0, paid: 0 };
      rows.forEach((row) => {
        const s = String(row.status || '').toLowerCase();
        if (Object.prototype.hasOwnProperty.call(counts, s)) counts[s] += 1;
      });
      const elPending = document.getElementById('wrCountPending');
      const elApproved = document.getElementById('wrCountApproved');
      const elPaid = document.getElementById('wrCountPaid');
      if (elPending) elPending.textContent = String(counts.pending);
      if (elApproved) elApproved.textContent = String(counts.approved);
      if (elPaid) elPaid.textContent = String(counts.paid);
    }

    async function loadWithdrawals() {
      const params = activeWorkflow ? `?status=${encodeURIComponent(activeWorkflow)}` : '';
      const r = await apiFetch(`api/modules/wallet/withdrawals${params}`);
      if (!r.ok) { showAlert(alertEl, r.data.error || 'Failed to load withdrawals'); return; }
      const rows = r.data.data || [];

      if (window.$ && $.fn.DataTable && $.fn.DataTable.isDataTable('#withdrawalsTable')) {
        $('#withdrawalsTable').DataTable().clear().destroy();
      }
      dt = null;
      const tbody = document.querySelector('#withdrawalsTable tbody');
      const actionMenuHtml = (w) => {
        const actions = [];
        if (cfg.canApprove && w.status === 'pending') {
          actions.push(`<li><button type="button" class="dropdown-item wr-action-item" data-action="approve" data-id="${w.id}"><i class="ri-check-line me-2 text-success"></i>Approve</button></li>`);
          actions.push(`<li><button type="button" class="dropdown-item wr-action-item" data-action="reject" data-id="${w.id}"><i class="ri-close-line me-2 text-danger"></i>Reject</button></li>`);
        }
        if (cfg.canPay && w.status === 'approved') {
          actions.push(`<li><button type="button" class="dropdown-item wr-action-item" data-action="pay" data-id="${w.id}"><i class="ri-bank-card-line me-2 text-primary"></i>Mark Paid</button></li>`);
        }
        if (cfg.canManage && !['paid', 'cancelled'].includes(w.status)) {
          actions.push(`<li><button type="button" class="dropdown-item wr-action-item" data-action="cancel" data-id="${w.id}"><i class="ri-delete-bin-line me-2 text-muted"></i>Cancel</button></li>`);
        }
        if (!actions.length) return '<span class="text-muted">-</span>';
        return `
          <div class="dropdown dropstart">
            <button class="btn btn-outline-primary btn-sm dropdown-toggle wr-action-btn" type="button" data-bs-toggle="dropdown" data-bs-boundary="viewport" aria-expanded="false">
              <i class="ri-settings-3-line me-1"></i>Actions
            </button>
            <ul class="dropdown-menu">
              ${actions.join('')}
            </ul>
          </div>
        `;
      };
      tbody.innerHTML = rows.length ? rows.map(w => `
        <tr>
          <td>${w.employee_name || w.user_id}</td>
          <td class="fw-bold">${fmt(w.amount)} <span class="text-muted small fw-normal">${w.currency || 'NGN'}</span></td>
          <td>${w.bank_name
            ? (canSeeFullBank
              ? `${w.bank_name} · ${w.account_name || '-'} · ${w.account_number || '-'}`
              : `${w.bank_name} ···${String(w.account_number||'').slice(-4)}`)
            : '-'}</td>
          <td>${statusBadge(w.status)}</td>
          <td>${fmtDate(w.created_at)}</td>
          <td>${fmtDate(w.approved_at)}</td>
          <td>${fmtDate(w.paid_at)}</td>
          <td>${actionMenuHtml(w)}</td>
        </tr>
      `).join('') : '';
      if (window.$ && $.fn.DataTable) {
        dt = $('#withdrawalsTable').DataTable({
          destroy: true,
          order: [[4, 'desc']],
          language: { emptyTable: 'No withdrawals found.' }
        });
      }
    }

    loadWithdrawals();
    loadWorkflowCounts();
    if (quickPendingBtn) quickPendingBtn.addEventListener('click', () => { activeWorkflow = 'pending'; setQuickActive(quickPendingBtn); loadWithdrawals(); });
    if (quickApprovedBtn) quickApprovedBtn.addEventListener('click', () => { activeWorkflow = 'approved'; setQuickActive(quickApprovedBtn); loadWithdrawals(); });
    if (quickPaidBtn) quickPaidBtn.addEventListener('click', () => { activeWorkflow = 'paid'; setQuickActive(quickPaidBtn); loadWithdrawals(); });
    if (quickAllBtn) quickAllBtn.addEventListener('click', () => { activeWorkflow = ''; setQuickActive(quickAllBtn); loadWithdrawals(); });

    // Action modal
    const actionModalEl = document.getElementById('wrActionModal');
    const bsActionModal = actionModalEl ? new bootstrap.Modal(actionModalEl) : null;
    const actionTitle   = document.getElementById('wrActionTitle');
    const actionBody    = document.getElementById('wrActionBody');
    const actionAlert   = document.getElementById('wrActionAlert');
    const actionConfirm = document.getElementById('wrActionConfirmBtn');
    const payoutRefRow  = document.getElementById('wrPayoutRefRow');
    const approveNoteRow = document.getElementById('wrApproveNoteRow');
    let pendingAction   = null;

    function openAction(title, body, action, showPayoutRef, showNote) {
      if (actionTitle) actionTitle.textContent = title;
      if (actionBody)  actionBody.textContent  = body;
      hideAlert(actionAlert);
      payoutRefRow  && (showPayoutRef ? payoutRefRow.classList.remove('d-none')  : payoutRefRow.classList.add('d-none'));
      approveNoteRow && (showNote     ? approveNoteRow.classList.remove('d-none') : approveNoteRow.classList.add('d-none'));
      const pr = document.getElementById('wrPayoutReference'); if (pr) pr.value = '';
      const an = document.getElementById('wrApproveNote');     if (an) an.value = '';
      pendingAction = action;
      bsActionModal && bsActionModal.show();
    }

    if (actionConfirm) actionConfirm.addEventListener('click', async () => {
      if (!pendingAction) return;
      spinBtn(actionConfirm, true);
      const r = await pendingAction();
      spinBtn(actionConfirm, false);
      if (r === false || !r) return; // validation blocked
      if (!r.ok) { showAlert(actionAlert, r.data.error || 'Action failed'); return; }
      bsActionModal && bsActionModal.hide();
      loadWithdrawals();
      loadWorkflowCounts();
    });

    const table = document.getElementById('withdrawalsTable');
    if (table) table.addEventListener('click', (e) => {
      const actionBtn = e.target.closest('.wr-action-item');
      if (!actionBtn) return;
      const action = String(actionBtn.dataset.action || '');
      const id = String(actionBtn.dataset.id || '');
      if (!id) return;

      if (action === 'approve') openAction('Approve Withdrawal', 'Approve and reserve this amount from the user wallet now?',
        () => apiFetch(`api/modules/wallet/withdrawals?id=${id}`, {
          method:'PATCH', headers:{'Content-Type':'application/json'},
          body: JSON.stringify({ status:'approved', approved_note: (document.getElementById('wrApproveNote')||{}).value || null })
        }), false, true);

      if (action === 'reject') openAction('Reject Withdrawal', 'Reject this withdrawal request?',
        () => apiFetch(`api/modules/wallet/withdrawals?id=${id}`, {
          method:'PATCH', headers:{'Content-Type':'application/json'},
          body: JSON.stringify({ status:'rejected', approved_note: (document.getElementById('wrApproveNote')||{}).value || null })
        }), false, true);

      if (action === 'pay') openAction('Mark as Paid', 'Enter the bank transfer reference to confirm payment.',
        () => {
          const payoutRef = (document.getElementById('wrPayoutReference')||{}).value.trim();
          if (!payoutRef) { showAlert(actionAlert, 'Payout reference is required'); return false; }
          return apiFetch(`api/modules/wallet/withdrawals?id=${id}`, {
            method:'PATCH', headers:{'Content-Type':'application/json'},
            body: JSON.stringify({ status:'paid', payout_reference: payoutRef })
          });
        }, true, false);

      if (action === 'cancel') openAction('Cancel Withdrawal', 'Cancel this withdrawal request?',
        () => apiFetch(`api/modules/wallet/withdrawals?id=${id}`, {
          method:'PATCH', headers:{'Content-Type':'application/json'},
          body: JSON.stringify({ status:'cancelled' })
        }), false, false);
    });
  }

  /* ================================================================= INIT */
  document.addEventListener('DOMContentLoaded', () => {
    if (window.walletAdminConfig)       initWalletAdmin();
    if (window.walletDetailConfig)      initWalletDetail();
    if (window.walletWithdrawalsConfig) initWalletWithdrawals();
  });

})();
