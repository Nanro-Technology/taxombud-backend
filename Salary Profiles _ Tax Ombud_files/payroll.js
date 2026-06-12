/**
 * Payroll Module JS
 * Handles: PayrollPeriods, PayrollSalaryProfiles, PayrollRuns, PayrollDetail
 */

(function () {
  'use strict';

  /* ------------------------------------------------------------------ helpers */
  const fmt = (v) => v != null ? Number(v).toLocaleString('en-NG', {minimumFractionDigits: 2}) : '-';
  const fmtDate = (v) => v ? String(v).slice(0, 10) : '-';

  function statusBadge(status) {
    const map = {
      draft:    'bg-secondary',
      open:     'bg-info text-dark',
      closed:   'bg-dark',
      review:   'bg-warning text-dark',
      approved: 'bg-success',
      posted:   'bg-primary',
      cancelled:'bg-danger',
      active:   'bg-success',
      inactive: 'bg-secondary',
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

  /* ================================================================== PERIODS */
  function initPayrollPeriods() {
    const cfg = window.payrollPeriodsConfig || {};
    const alertEl = document.getElementById('ppAlert');
    const currencyEl = document.getElementById('pp_currency');
    let dt;

    async function ensurePeriodCurrencies(preferredCode) {
      if (!currencyEl) return;
      const preferred = String(preferredCode || '').toUpperCase() || 'NGN';
      if (typeof window.populateCurrencySelect === 'function') {
        await window.populateCurrencySelect(currencyEl, { defaultCode: preferred, activeOnly: true });
        if (!currencyEl.options.length) {
          await window.populateCurrencySelect(currencyEl, { defaultCode: preferred, activeOnly: false });
        }
      }
      if (!currencyEl.options.length) {
        currencyEl.innerHTML = '<option value="NGN">NGN</option>';
      }
      if (preferred && Array.from(currencyEl.options).some(o => String(o.value).toUpperCase() === preferred)) {
        currencyEl.value = preferred;
      } else if (currencyEl.options.length) {
        currencyEl.value = currencyEl.options[0].value;
      }
    }

    async function loadPeriods() {
      const r = await apiFetch('api/modules/payroll/periods');
      if (!r.ok) { showAlert(alertEl, r.data.error || 'Failed to load periods'); return; }
      const rows = r.data.data || [];

      if (dt) { dt.destroy(); document.querySelector('#periodsTable tbody').innerHTML = ''; }
      const tbody = document.querySelector('#periodsTable tbody');
      tbody.innerHTML = rows.map(p => `
        <tr>
          <td>${p.name}</td>
          <td class="text-nowrap small">${p.period_month}/${p.period_year}</td>
          <td class="text-nowrap small">${fmtDate(p.start_date)}</td>
          <td class="text-nowrap small">${fmtDate(p.end_date)}</td>
          <td class="text-nowrap small">${p.currency || 'NGN'}</td>
          <td>${statusBadge(p.status)}</td>
          <td>${p.run_count || 0}</td>
          ${cfg.canManage ? `<td>
            <div class="btn-group btn-group-sm flex-wrap" role="group" aria-label="Period actions">
              ${p.status === 'draft' ? `<button class="btn btn-soft-success pp-open" data-id="${p.id}" title="Open"><i class="ri-lock-unlock-line me-1"></i>Open</button>` : ''}
              ${p.status === 'open'  ? `<button class="btn btn-soft-secondary pp-close" data-id="${p.id}" title="Close"><i class="ri-lock-line me-1"></i>Close</button>` : ''}
              ${p.status === 'draft' ? `<button class="btn btn-soft-primary pp-edit" data-id="${p.id}" title="Edit"><i class="ri-pencil-line"></i></button>` : ''}
              ${p.status === 'draft' ? `<button class="btn btn-soft-danger pp-delete" data-id="${p.id}" data-name="${p.name}" title="Delete"><i class="ri-delete-bin-line"></i></button>` : ''}
            </div>
          </td>` : '<td>-</td>'}
        </tr>
      `).join('');
      dt = window.$ && $.fn.DataTable ? $('#periodsTable').DataTable({ order: [[1, 'desc']] }) : null;
    }

    loadPeriods();
    ensurePeriodCurrencies('NGN');

    const modal      = document.getElementById('periodModal');
    const bsModal    = modal ? new bootstrap.Modal(modal) : null;
    const createBtn  = document.getElementById('createPeriodBtn');
    const saveBtn    = document.getElementById('ppSaveBtn');
    const modalAlert = document.getElementById('ppModalAlert');

    async function resetForm() {
      ['pp_id','pp_name','pp_start_date','pp_end_date'].forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
      const m = document.getElementById('pp_month');
      const y = document.getElementById('pp_year');
      if (m) m.value = new Date().getMonth() + 1;
      if (y) y.value = new Date().getFullYear();
      await ensurePeriodCurrencies('NGN');
      const title = document.getElementById('ppModalTitle');
      if (title) title.textContent = 'New Payroll Period';
      hideAlert(modalAlert);
    }

    if (createBtn) createBtn.addEventListener('click', async () => { await resetForm(); bsModal && bsModal.show(); });

    if (saveBtn) saveBtn.addEventListener('click', async () => {
      const id = document.getElementById('pp_id').value;
      const payload = {
        name:         document.getElementById('pp_name').value.trim(),
        period_month: parseInt(document.getElementById('pp_month').value, 10),
        period_year:  parseInt(document.getElementById('pp_year').value, 10),
        start_date:   document.getElementById('pp_start_date').value,
        end_date:     document.getElementById('pp_end_date').value,
        currency:     (document.getElementById('pp_currency').value || 'NGN').trim().toUpperCase(),
      };
      if (!payload.name || !payload.start_date || !payload.end_date) {
        showAlert(modalAlert, 'Name, start date, and end date are required'); return;
      }
      spinBtn(saveBtn, true);
      const r = await apiFetch(id ? `api/modules/payroll/periods?id=${id}` : 'api/modules/payroll/periods', {
        method: id ? 'PATCH' : 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(payload)
      });
      spinBtn(saveBtn, false);
      if (!r.ok) { showAlert(modalAlert, r.data.error || 'Save failed'); return; }
      bsModal && bsModal.hide();
      loadPeriods();
    });

    const table = document.getElementById('periodsTable');
    if (table) table.addEventListener('click', async (e) => {
      const editBtn   = e.target.closest('.pp-edit');
      const deleteBtn = e.target.closest('.pp-delete');
      const openBtn   = e.target.closest('.pp-open');
      const closeBtn  = e.target.closest('.pp-close');

      if (editBtn) {
        const r = await apiFetch(`api/modules/payroll/periods?id=${editBtn.dataset.id}`);
        if (!r.ok) return;
        const p = r.data;
        await resetForm();
        document.getElementById('pp_id').value         = p.id;
        document.getElementById('pp_name').value        = p.name;
        document.getElementById('pp_month').value       = p.period_month;
        document.getElementById('pp_year').value        = p.period_year;
        document.getElementById('pp_start_date').value  = fmtDate(p.start_date);
        document.getElementById('pp_end_date').value    = fmtDate(p.end_date);
        await ensurePeriodCurrencies((p.currency || 'NGN'));
        const title = document.getElementById('ppModalTitle');
        if (title) title.textContent = 'Edit Payroll Period';
        bsModal && bsModal.show();
      }

      if (deleteBtn) {
        const nameEl = document.getElementById('ppDeleteName');
        if (nameEl) nameEl.textContent = deleteBtn.dataset.name;
        const delModalEl = document.getElementById('periodDeleteModal');
        if (!delModalEl) return;
        const delModal = new bootstrap.Modal(delModalEl);
        hideAlert(document.getElementById('ppDeleteAlert'));
        const confirmBtn = document.getElementById('ppDeleteConfirmBtn');
        const newBtn = confirmBtn.cloneNode(true);
        confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);
        newBtn.addEventListener('click', async () => {
          spinBtn(newBtn, true);
          const r = await apiFetch(`api/modules/payroll/periods?id=${deleteBtn.dataset.id}`, { method: 'DELETE' });
          spinBtn(newBtn, false);
          if (!r.ok) { showAlert(document.getElementById('ppDeleteAlert'), r.data.error || 'Delete failed'); return; }
          delModal.hide(); loadPeriods();
        });
        delModal.show();
      }

      if (openBtn || closeBtn) {
        const btn = openBtn || closeBtn;
        const newStatus = openBtn ? 'open' : 'closed';
        const confirmOpts = { okText: 'Yes, Update' };
        if (closeBtn) {
          confirmOpts.bodyHtml = '<p class="mb-0" style="font-size:0.925rem;">Close this payroll period now? You can reopen it later if needed.</p>';
        }
        if (!(await crmUiConfirm(`Set period to "${newStatus}"?`, 'Update Payroll Period', confirmOpts))) return;
        const r = await apiFetch(`api/modules/payroll/periods?id=${btn.dataset.id}`, {
          method: 'PATCH', headers: {'Content-Type': 'application/json'},
          body: JSON.stringify({ status: newStatus })
        });
        if (!r.ok) { crmUiAlert(r.data.error || 'Action failed'); return; }
        loadPeriods();
      }
    });
  }

  /* ============================================================== SALARY PROFILES */
  function initPayrollSalaryProfiles() {
    const cfg = window.payrollSalaryProfilesConfig || {};
    const alertEl = document.getElementById('spAlert');
    let dt, componentRows = [];

    async function loadProfiles() {
      const r = await apiFetch('api/modules/payroll/salary_profiles');
      if (!r.ok) { showAlert(alertEl, r.data.error || 'Failed to load profiles'); return; }
      const rows = r.data.data || [];

      if (dt) { dt.destroy(); document.querySelector('#salaryProfilesTable tbody').innerHTML = ''; }
      const tbody = document.querySelector('#salaryProfilesTable tbody');
      tbody.innerHTML = rows.map(p => `
        <tr>
          <td>${p.employee_name || p.user_id}</td>
          <td>${p.pay_grade_name || '-'}</td>
          <td>${fmt(p.base_salary)}</td>
          <td>${p.currency || 'NGN'}</td>
          <td>${fmtDate(p.effective_date)}</td>
          <td>${statusBadge(p.status)}</td>
          <td>${p.component_count || 0}</td>
          ${cfg.canManage ? `<td>
            <div class="btn-group btn-group-sm" role="group" aria-label="Salary profile actions">
              <button class="btn btn-soft-primary sp-edit" data-id="${p.id}" title="Edit"><i class="ri-pencil-line me-1"></i>Edit</button>
              <button class="btn btn-soft-danger sp-delete" data-id="${p.id}" data-name="${p.employee_name || p.user_id}" title="Delete"><i class="ri-delete-bin-line me-1"></i>Delete</button>
            </div>
          </td>` : '<td>-</td>'}
        </tr>
      `).join('');
      dt = window.$ && $.fn.DataTable ? $('#salaryProfilesTable').DataTable() : null;
    }

    loadProfiles();

    const modal      = document.getElementById('salaryProfileModal');
    const bsModal    = modal ? new bootstrap.Modal(modal) : null;
    const createBtn  = document.getElementById('createSalaryProfileBtn');
    const saveBtn    = document.getElementById('spSaveBtn');
    const modalAlert = document.getElementById('spModalAlert');
    const compList   = document.getElementById('sp_components_list');
    const addCompBtn = document.getElementById('sp_add_component_btn');
    const payGradeEl = document.getElementById('sp_pay_grade_id');
    const baseSalaryEl = document.getElementById('sp_base_salary');
    const currencyEl = document.getElementById('sp_currency');
    const payGradeRangeEl = document.getElementById('sp_pay_grade_range');

    function formatMoney(v) {
      const n = Number(v);
      if (!Number.isFinite(n)) return null;
      return n.toLocaleString('en-NG', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function updatePayGradeRangeHint() {
      if (!payGradeEl || !payGradeRangeEl) return;
      const selected = payGradeEl.options[payGradeEl.selectedIndex];
      if (!selected || !selected.value) {
        payGradeRangeEl.textContent = '';
        payGradeRangeEl.classList.add('d-none');
        return;
      }
      const minRaw = selected.getAttribute('data-min-salary');
      const maxRaw = selected.getAttribute('data-max-salary');
      const minFmt = formatMoney(minRaw);
      const maxFmt = formatMoney(maxRaw);
      const currency = (currencyEl && currencyEl.value ? String(currencyEl.value).trim().toUpperCase() : 'NGN');
      if (!minFmt && !maxFmt) {
        payGradeRangeEl.textContent = '';
        payGradeRangeEl.classList.add('d-none');
        return;
      }
      if (minFmt && maxFmt) {
        payGradeRangeEl.textContent = `Pay grade range: ${currency} ${minFmt} - ${currency} ${maxFmt}`;
      } else if (minFmt) {
        payGradeRangeEl.textContent = `Pay grade minimum: ${currency} ${minFmt}`;
      } else {
        payGradeRangeEl.textContent = `Pay grade maximum: ${currency} ${maxFmt}`;
      }
      payGradeRangeEl.classList.remove('d-none');
    }

    function renderComponents() {
      if (!compList) return;
      compList.innerHTML = componentRows.map((c, i) => `
        <div class="row g-2 align-items-center mb-2 comp-row" data-idx="${i}">
          <div class="col-md-4">
            <input type="text" class="form-control form-control-sm comp-name" value="${c.name || ''}" placeholder="Component name">
          </div>
          <div class="col-md-2">
            <select class="form-select form-select-sm comp-type">
              <option value="allowance" ${c.component_type === 'allowance' ? 'selected' : ''}>Allowance</option>
              <option value="deduction" ${c.component_type === 'deduction' ? 'selected' : ''}>Deduction</option>
            </select>
          </div>
          <div class="col-md-2">
            <input type="number" class="form-control form-control-sm comp-amount" value="${c.amount || 0}" min="0" step="0.01">
          </div>
          <div class="col-md-2">
            <div class="form-check mt-1">
              <input class="form-check-input comp-pct" type="checkbox" ${c.is_percentage ? 'checked' : ''}>
              <label class="form-check-label small">% of base</label>
            </div>
          </div>
          <div class="col-md-2">
            <button class="btn btn-sm btn-outline-danger comp-remove" data-idx="${i}"><i class="ri-delete-bin-line"></i></button>
          </div>
        </div>
      `).join('');
    }

    function syncComponentRowsFromDom() {
      if (!compList) return;
      componentRows = gatherComponents();
    }

    if (addCompBtn) addCompBtn.addEventListener('click', () => {
      syncComponentRowsFromDom();
      componentRows.push({ name: '', component_type: 'allowance', amount: 0, is_percentage: 0 });
      renderComponents();
    });

    if (compList) compList.addEventListener('click', (e) => {
      const rmBtn = e.target.closest('.comp-remove');
      if (rmBtn) {
        syncComponentRowsFromDom();
        componentRows.splice(parseInt(rmBtn.dataset.idx, 10), 1);
        renderComponents();
      }
    });

    function gatherComponents() {
      const rows = compList ? compList.querySelectorAll('.comp-row') : [];
      return Array.from(rows).map(row => ({
        name:           row.querySelector('.comp-name').value.trim(),
        component_type: row.querySelector('.comp-type').value,
        amount:         parseFloat(row.querySelector('.comp-amount').value) || 0,
        is_percentage:  row.querySelector('.comp-pct').checked ? 1 : 0,
      }));
    }

    function resetForm() {
      ['sp_profile_id','sp_base_salary','sp_effective_date','sp_notes'].forEach(id => {
        const el = document.getElementById(id); if (el) el.value = '';
      });
      const userSel = document.getElementById('sp_user_id');
      if (userSel) {
        userSel.value = '';
        if (window.$ && $(userSel).data('select2')) {
          $(userSel).val(null).trigger('change');
        }
      }
      const cur = document.getElementById('sp_currency'); if (cur) cur.value = 'NGN';
      const st  = document.getElementById('sp_status');   if (st)  st.value  = 'active';
      const pg  = document.getElementById('sp_pay_grade_id'); if (pg) pg.value = '';
      updatePayGradeRangeHint();
      const t   = document.getElementById('spModalTitle'); if (t) t.textContent = 'New Salary Profile';
      if (compList) compList.innerHTML = '';
      componentRows = [];
      hideAlert(modalAlert);
    }

    if (createBtn) createBtn.addEventListener('click', () => { resetForm(); bsModal && bsModal.show(); });

    if (payGradeEl && baseSalaryEl) {
      payGradeEl.addEventListener('change', () => {
        const selected = payGradeEl.options[payGradeEl.selectedIndex];
        updatePayGradeRangeHint();
        if (!selected) return;
        const minSalaryRaw = selected.getAttribute('data-min-salary');
        const minSalary = Number(minSalaryRaw);
        if (!Number.isFinite(minSalary) || minSalary < 0) return;
        if (typeof window.moneySet === 'function') {
          window.moneySet(baseSalaryEl, minSalary);
        } else {
          baseSalaryEl.value = minSalary.toFixed(2);
        }
      });
    }
    if (currencyEl) {
      currencyEl.addEventListener('input', updatePayGradeRangeHint);
      currencyEl.addEventListener('change', updatePayGradeRangeHint);
    }

    // Employee Select2 search
    const userSelect = document.getElementById('sp_user_id');
    if (userSelect && window.$ && $.fn.select2) {
      $(userSelect).select2({
        dropdownParent: $('#salaryProfileModal'),
        width: '100%',
        placeholder: userSelect.dataset.placeholder || 'Search employee...',
        allowClear: true,
        ajax: {
          delay: 300,
          transport: function(params, success, failure) {
            const q = String((params.data && params.data.term) || '').trim();
            apiFetch(`api/modules/hr/staff?search=${encodeURIComponent(q)}`).then((r) => {
              if (!r.ok) {
                failure(r.data || {});
                return;
              }
              success(r.data);
            }).catch(failure);
          },
          processResults: function(data) {
            const items = (data && data.data) ? data.data : [];
            return {
              results: items.map((u) => ({
                id: u.id || u.user_id,
                text: u.display_name || u.email || ('User #' + (u.id || u.user_id || '')),
              }))
            };
          }
        }
      });
    }

    if (saveBtn) saveBtn.addEventListener('click', async () => {
      const id     = document.getElementById('sp_profile_id').value;
      const userId = document.getElementById('sp_user_id').value;
      if (!userId) { showAlert(modalAlert, 'Please select an employee'); return; }
      const effDate = document.getElementById('sp_effective_date').value;
      if (!effDate)  { showAlert(modalAlert, 'Effective date is required'); return; }
      const payload = {
        action:         id ? 'update_profile' : 'create_profile',
        user_id:        parseInt(userId, 10),
        pay_grade_id:   document.getElementById('sp_pay_grade_id').value || null,
        base_salary:    parseFloat(window.moneyVal(document.getElementById('sp_base_salary'))) || 0,
        currency:       (document.getElementById('sp_currency').value || 'NGN').trim(),
        effective_date: effDate,
        status:         document.getElementById('sp_status').value,
        notes:          document.getElementById('sp_notes').value.trim() || null,
        components:     gatherComponents(),
      };
      spinBtn(saveBtn, true);
      const r = await apiFetch(
        id ? `api/modules/payroll/salary_profiles?id=${id}` : 'api/modules/payroll/salary_profiles',
        { method: id ? 'PATCH' : 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(payload) }
      );
      spinBtn(saveBtn, false);
      if (!r.ok) { showAlert(modalAlert, r.data.error || 'Save failed'); return; }
      bsModal && bsModal.hide();
      loadProfiles();
    });

    const table = document.getElementById('salaryProfilesTable');
    if (table) table.addEventListener('click', async (e) => {
      const editBtn   = e.target.closest('.sp-edit');
      const deleteBtn = e.target.closest('.sp-delete');

      if (editBtn) {
        const r = await apiFetch(`api/modules/payroll/salary_profiles?id=${editBtn.dataset.id}`);
        if (!r.ok) return;
        const p = r.data;
        resetForm();
        document.getElementById('sp_profile_id').value   = p.id;
        const userSel = document.getElementById('sp_user_id');
        if (userSel) {
          const selectedText = p.employee_name || ('User #' + p.user_id);
          const existing = Array.from(userSel.options || []).find(o => String(o.value) === String(p.user_id));
          if (!existing) {
            const opt = new Option(selectedText, String(p.user_id), true, true);
            userSel.add(opt);
          }
          userSel.value = String(p.user_id);
          if (window.$ && $(userSel).data('select2')) {
            $(userSel).val(String(p.user_id)).trigger('change');
          }
        }
        const pg = document.getElementById('sp_pay_grade_id'); if (pg) pg.value = p.pay_grade_id || '';
        window.moneySet(document.getElementById('sp_base_salary'), p.base_salary);
        document.getElementById('sp_currency').value     = p.currency || 'NGN';
        updatePayGradeRangeHint();
        document.getElementById('sp_effective_date').value = fmtDate(p.effective_date);
        document.getElementById('sp_status').value       = p.status;
        document.getElementById('sp_notes').value        = p.notes || '';
        const t = document.getElementById('spModalTitle'); if (t) t.textContent = 'Edit Salary Profile';
        componentRows = (p.components || []).map(c => ({...c}));
        renderComponents();
        bsModal && bsModal.show();
      }

      if (deleteBtn) {
        const nameEl = document.getElementById('spDeleteName'); if (nameEl) nameEl.textContent = deleteBtn.dataset.name;
        const delModalEl = document.getElementById('salaryProfileDeleteModal'); if (!delModalEl) return;
        const delModal = new bootstrap.Modal(delModalEl);
        hideAlert(document.getElementById('spDeleteAlert'));
        const confirmBtn = document.getElementById('spDeleteConfirmBtn');
        const newBtn = confirmBtn.cloneNode(true);
        confirmBtn.parentNode.replaceChild(newBtn, confirmBtn);
        newBtn.addEventListener('click', async () => {
          spinBtn(newBtn, true);
          const r = await apiFetch(`api/modules/payroll/salary_profiles?id=${deleteBtn.dataset.id}`, { method: 'DELETE' });
          spinBtn(newBtn, false);
          if (!r.ok) { showAlert(document.getElementById('spDeleteAlert'), r.data.error || 'Delete failed'); return; }
          delModal.hide(); loadProfiles();
        });
        delModal.show();
      }
    });
  }

  /* ================================================================= PAYROLL RUNS */
  function initPayrollRuns() {
    const cfg = window.payrollRunsConfig || {};
    const alertEl = document.getElementById('prAlert');
    let dt;

    async function loadRuns() {
      const r = await apiFetch('api/modules/payroll/index');
      if (!r.ok) { showAlert(alertEl, r.data.error || 'Failed to load runs'); return; }
      const rows = r.data.data || [];

      if (dt) { dt.destroy(); document.querySelector('#payrollRunsTable tbody').innerHTML = ''; }
      const tbody = document.querySelector('#payrollRunsTable tbody');
      tbody.innerHTML = rows.map(run => `
        <tr>
          <td>${run.period_name || run.period_id}</td>
          <td>${statusBadge(run.status)}</td>
          <td class="text-nowrap small">${run.currency || 'NGN'}</td>
          <td>${fmt(run.total_gross)}</td>
          <td>${fmt(run.total_deductions)}</td>
          <td>${fmt(run.total_net)}</td>
          <td class="text-nowrap small text-end">${run.item_count || 0}</td>
          <td>${fmtDate(run.created_at)}</td>
          <td class="text-nowrap">
            <div class="btn-group btn-group-sm" role="group" aria-label="Payroll run actions">
              <a href="studio/payroll/detail.kml?id=${run.id}" class="btn btn-soft-primary" title="View"><i class="ri-eye-line me-1"></i>View</a>
              ${cfg.canManage  && run.status === 'draft'    ? `<button class="btn btn-soft-warning run-review"  data-id="${run.id}" title="Submit for Review"><i class="ri-send-plane-line me-1"></i>Review</button>` : ''}
              ${cfg.canApprove && run.status === 'review'   ? `<button class="btn btn-soft-success run-approve" data-id="${run.id}" title="Approve"><i class="ri-check-double-line me-1"></i>Approve</button>` : ''}
              ${cfg.canPost    && run.status === 'approved' ? `<button class="btn btn-soft-info run-post"       data-id="${run.id}" title="Post to Wallet"><i class="ri-send-to-back me-1"></i>Post to Wallet</button>` : ''}
              ${cfg.canManage  && ['draft','review'].includes(run.status) ? `<button class="btn btn-soft-danger run-cancel" data-id="${run.id}" title="Cancel"><i class="ri-close-circle-line me-1"></i>Cancel</button>` : ''}
            </div>
          </td>
        </tr>
      `).join('');
      dt = window.$ && $.fn.DataTable ? $('#payrollRunsTable').DataTable({ order: [[7, 'desc']] }) : null;
    }

    loadRuns();

    // Create run
    const createModal   = document.getElementById('createRunModal');
    const bsCreateModal = createModal ? new bootstrap.Modal(createModal) : null;
    const createAlert   = document.getElementById('createRunAlert');
    const createSaveBtn = document.getElementById('createRunSaveBtn');

    if (createSaveBtn) createSaveBtn.addEventListener('click', async () => {
      const periodId = (document.getElementById('run_period_id') || {}).value;
      const notes    = (document.getElementById('run_notes') || {}).value.trim();
      if (!periodId) { showAlert(createAlert, 'Please select a period'); return; }
      spinBtn(createSaveBtn, true);
      const r = await apiFetch('api/modules/payroll/index', {
        method: 'POST', headers: {'Content-Type':'application/json'},
        body: JSON.stringify({ period_id: parseInt(periodId, 10), notes: notes || null })
      });
      spinBtn(createSaveBtn, false);
      if (!r.ok) { showAlert(createAlert, r.data.error || 'Failed to create run'); return; }
      bsCreateModal && bsCreateModal.hide();
      loadRuns();
    });

    // Run action modal
    const actionModal   = document.getElementById('runActionModal');
    const bsActionModal = actionModal ? new bootstrap.Modal(actionModal) : null;
    const actionTitle   = document.getElementById('runActionTitle');
    const actionBody    = document.getElementById('runActionBody');
    const actionAlert   = document.getElementById('runActionAlert');
    const actionConfirm = document.getElementById('runActionConfirmBtn');
    const actionNotesEl = document.getElementById('runActionNotes');
    let pendingAction   = null;

    function showRunAction(title, body, action) {
      if (actionTitle) actionTitle.textContent = title;
      if (actionBody)  actionBody.textContent  = body;
      hideAlert(actionAlert);
      if (actionNotesEl) actionNotesEl.value = '';
      pendingAction = action;
      bsActionModal && bsActionModal.show();
    }

    if (actionConfirm) actionConfirm.addEventListener('click', async () => {
      if (!pendingAction) return;
      spinBtn(actionConfirm, true);
      const r = await pendingAction();
      spinBtn(actionConfirm, false);
      if (!r.ok) { showAlert(actionAlert, r.data.error || 'Action failed'); return; }
      bsActionModal && bsActionModal.hide();
      loadRuns();
    });

    const table = document.getElementById('payrollRunsTable');
    if (table) table.addEventListener('click', (e) => {
      const reviewBtn  = e.target.closest('.run-review');
      const approveBtn = e.target.closest('.run-approve');
      const postBtn    = e.target.closest('.run-post');
      const cancelBtn  = e.target.closest('.run-cancel');

      if (reviewBtn)  showRunAction('Submit for Review', 'Submit this run for approval review?',
        () => {
          const payload = { status: 'review' };
          if (actionNotesEl && actionNotesEl.value.trim()) payload.notes = actionNotesEl.value.trim();
          return apiFetch(`api/modules/payroll/detail?id=${reviewBtn.dataset.id}`, { method:'PATCH', headers:{'Content-Type':'application/json'}, body:JSON.stringify(payload) });
        });
      if (approveBtn) showRunAction('Approve Run', 'Approve this payroll run?',
        () => {
          const payload = { status: 'approved' };
          if (actionNotesEl && actionNotesEl.value.trim()) payload.notes = actionNotesEl.value.trim();
          return apiFetch(`api/modules/payroll/detail?id=${approveBtn.dataset.id}`, { method:'PATCH', headers:{'Content-Type':'application/json'}, body:JSON.stringify(payload) });
        });
      if (postBtn)    showRunAction('Post to Wallet', 'Post this run? Employee wallets will be credited. This cannot be undone.',
        () => {
          const payload = { status: 'posted' };
          if (actionNotesEl && actionNotesEl.value.trim()) payload.notes = actionNotesEl.value.trim();
          return apiFetch(`api/modules/payroll/detail?id=${postBtn.dataset.id}`, { method:'PATCH', headers:{'Content-Type':'application/json'}, body:JSON.stringify(payload) });
        });
      if (cancelBtn)  showRunAction('Cancel Run', 'Cancel this payroll run?',
        () => {
          const payload = { status: 'cancelled' };
          if (actionNotesEl && actionNotesEl.value.trim()) payload.notes = actionNotesEl.value.trim();
          return apiFetch(`api/modules/payroll/detail?id=${cancelBtn.dataset.id}`, { method:'PATCH', headers:{'Content-Type':'application/json'}, body:JSON.stringify(payload) });
        });
    });
  }

  /* ================================================================= PAYROLL DETAIL */
  function initPayrollDetail() {
    const cfg = window.payrollDetailConfig || {};
    if (!cfg.runId) return;

    const alertEl     = document.getElementById('rdAlert');
    const actionBtns  = document.getElementById('rdActionBtns');
    const actionModal = document.getElementById('rdActionModal');
    const bsActionModal = actionModal ? new bootstrap.Modal(actionModal) : null;
    const actionTitle = document.getElementById('rdActionTitle');
    const actionBodyEl = document.getElementById('rdActionBody');
    const actionAlert = document.getElementById('rdActionAlert');
    const actionConfirm = document.getElementById('rdActionConfirmBtn');
    const notesRow = document.getElementById('rdNotesRow');
    let pendingAction = null;
    let pendingActionCode = '';

    async function loadRun() {
      const r = await apiFetch(`api/modules/payroll/detail?id=${cfg.runId}`);
      if (!r.ok) { showAlert(alertEl, r.data.error || 'Failed to load run'); return; }
      const run = r.data;

      const el = (id) => document.getElementById(id);
      if (el('runPeriodLabel'))    el('runPeriodLabel').textContent    = run.period_name ? `— ${run.period_name}` : '';
      if (el('runStatusLabel'))    el('runStatusLabel').textContent    = `Status: ${run.status || '-'}`;
      if (el('rdTotalGross'))      el('rdTotalGross').textContent      = `${run.currency || 'NGN'} ${fmt(run.total_gross)}`;
      if (el('rdTotalDeductions')) el('rdTotalDeductions').textContent = `${run.currency || 'NGN'} ${fmt(run.total_deductions)}`;
      if (el('rdTotalNet'))        el('rdTotalNet').textContent        = `${run.currency || 'NGN'} ${fmt(run.total_net)}`;
      if (el('rdEmployeeCount'))   el('rdEmployeeCount').textContent   = (run.items || []).length;

      // Action buttons
      if (actionBtns) {
        actionBtns.innerHTML = '';
        if (cfg.canManage  && run.status === 'draft')    addActionBtn(actionBtns, 'sync',      'Sync Staff',        'btn-info', { action: 'sync_draft_profiles' });
        if (cfg.canManage  && run.status === 'draft')    addActionBtn(actionBtns, 'review',    'Submit for Review', 'btn-warning');
        if (cfg.canApprove && run.status === 'review')   addActionBtn(actionBtns, 'approved',  'Approve',           'btn-success');
        if (cfg.canPost    && run.status === 'approved') addActionBtn(actionBtns, 'posted',    'Post to Wallet',    'btn-primary');
        if (cfg.canManage  && ['draft','review'].includes(run.status)) addActionBtn(actionBtns, 'cancelled', 'Cancel Run', 'btn-danger');
      }

      // Items table
      const tbody = el('runItemsBody');
      if (tbody) {
        const items = run.items || [];
        tbody.innerHTML = items.length ? items.map(item => `
          <tr>
            <td>${item.employee_name || item.user_id}</td>
            <td>${fmt(item.gross_pay)}</td>
            <td>${fmt(item.total_allowances)}</td>
            <td>${fmt(item.total_deductions)}</td>
            <td>${item.leave_deduction_days > 0 ? `${item.leave_deduction_days}d / ${fmt(item.leave_deduction_amount)}` : '-'}</td>
            <td class="fw-bold">${fmt(item.net_pay)}</td>
            <td>${item.currency || run.currency || 'NGN'}</td>
            <td>${item.bank_name ? `${item.bank_name} ···${String(item.account_number||'').slice(-4)}` : (item.wallet_credited ? '<span class="badge bg-primary">Wallet</span>' : '-')}</td>
            <td>${statusBadge(item.status)}</td>
            <td class="small text-muted">${item.notes || '-'}</td>
          </tr>
        `).join('') : '<tr><td colspan="10" class="text-center text-muted">No items found.</td></tr>';

        if (window.$ && $.fn.DataTable) {
          if ($.fn.DataTable.isDataTable('#runItemsTable')) $('#runItemsTable').DataTable().destroy();
          $('#runItemsTable').DataTable();
        }
      }
    }

    function addActionBtn(container, status, label, cls, opts = {}) {
      const iconMap = {
        review: 'ri-send-plane-line',
        approved: 'ri-check-double-line',
        posted: 'ri-bank-card-line',
        cancelled: 'ri-close-circle-line',
        sync: 'ri-user-add-line',
      };
      const btn = document.createElement('button');
      btn.className = `btn ${cls} btn-md`;
      btn.innerHTML = `<span class="label"><i class="${iconMap[status] || 'ri-check-line'} me-1"></i>${label}</span><span class="spinner-border spinner-border-sm d-none" role="status"></span>`;
      btn.dataset.status = status;
      if (opts.action) btn.dataset.action = opts.action;
      container.appendChild(btn);
    }

    const messages = {
      review:    'Submit this run for approval review?',
      approved:  'Approve this payroll run?',
      posted:    'Post this payroll run? Employee wallets will be credited and this cannot be undone.',
      cancelled: 'Cancel this payroll run?',
      sync:      'Sync this draft run to include newly added active salary profiles?',
    };

    if (actionBtns) actionBtns.addEventListener('click', (e) => {
      const btn = e.target.closest('button[data-status]'); if (!btn) return;
      const newStatus = btn.dataset.status;
      const action = btn.dataset.action || '';
      pendingActionCode = action || newStatus;
      if (actionTitle) actionTitle.textContent = btn.querySelector('.label').textContent;
      if (actionBodyEl) actionBodyEl.textContent = messages[newStatus] || 'Confirm this action?';
      hideAlert(actionAlert);
      const notesEl = document.getElementById('rdActionNotes');
      if (notesRow) notesRow.classList.toggle('d-none', action === 'sync_draft_profiles');
      pendingAction = async () => {
        const payload = action === 'sync_draft_profiles' ? { action: 'sync_draft_profiles' } : { status: newStatus };
        if (action !== 'sync_draft_profiles' && notesEl && notesEl.value.trim()) payload.notes = notesEl.value.trim();
        return apiFetch(`api/modules/payroll/detail?id=${cfg.runId}`, {
          method: 'PATCH', headers: {'Content-Type':'application/json'}, body: JSON.stringify(payload)
        });
      };
      bsActionModal && bsActionModal.show();
    });

    if (actionConfirm) actionConfirm.addEventListener('click', async () => {
      if (!pendingAction) return;
      spinBtn(actionConfirm, true);
      const r = await pendingAction();
      spinBtn(actionConfirm, false);
      if (!r.ok) { showAlert(actionAlert, r.data.error || 'Action failed'); return; }
      bsActionModal && bsActionModal.hide();
      if (pendingActionCode === 'sync_draft_profiles') {
        window.location.reload();
        return;
      }
      loadRun();
    });

    loadRun();
  }

  /* ================================================================= INIT */
  document.addEventListener('DOMContentLoaded', () => {
    if (window.payrollPeriodsConfig)        initPayrollPeriods();
    if (window.payrollSalaryProfilesConfig) initPayrollSalaryProfiles();
    if (window.payrollRunsConfig)           initPayrollRuns();
    if (window.payrollDetailConfig)         initPayrollDetail();
  });

})();
