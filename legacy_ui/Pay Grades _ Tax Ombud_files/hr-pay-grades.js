/* eslint-disable */
(function () {
  'use strict';

  const cfg = window.hrPayGradesConfig || {};
  const canManage = !!cfg.canManage;
  const alertBox = document.getElementById('pgAlert');
  const modalEl = document.getElementById('payGradeModal');
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const deleteModalEl = document.getElementById('payGradeDeleteModal');
  const deleteModal = deleteModalEl ? new bootstrap.Modal(deleteModalEl) : null;

  const fields = {
    id: document.getElementById('pg_id'),
    name: document.getElementById('pg_name'),
    level: document.getElementById('pg_level'),
    currency: document.getElementById('pg_currency'),
    min_salary: document.getElementById('pg_min_salary'),
    max_salary: document.getElementById('pg_max_salary'),
    description: document.getElementById('pg_description'),
  };

  const modalTitle = document.getElementById('pgModalTitle');
  const modalAlert = document.getElementById('pgModalAlert');
  const saveBtn = document.getElementById('pgSaveBtn');
  const deleteNameEl = document.getElementById('pgDeleteName');
  const deleteAlert = document.getElementById('pgDeleteAlert');
  const deleteConfirmBtn = document.getElementById('pgDeleteConfirmBtn');
  let deleteTarget = null;

  function showPageAlert(message) {
    if (!alertBox) return;
    alertBox.textContent = message || 'Something went wrong.';
    alertBox.classList.remove('d-none');
  }

  function hidePageAlert() {
    if (!alertBox) return;
    alertBox.classList.add('d-none');
    alertBox.textContent = '';
  }

  function showModalAlert(message, target) {
    if (!target) return;
    target.textContent = message || 'Something went wrong.';
    target.classList.remove('d-none');
  }

  function hideModalAlert(target) {
    if (!target) return;
    target.classList.add('d-none');
    target.textContent = '';
  }

  function setBtnBusy(btn, busy, busyLabel) {
    if (!btn) return;
    const label = btn.querySelector('.label');
    const spinner = btn.querySelector('.spinner-border');
    btn.disabled = !!busy;
    if (label && busyLabel) label.textContent = busy ? busyLabel : (btn.dataset.label || label.textContent);
    if (label && !btn.dataset.label) btn.dataset.label = label.textContent;
    if (spinner) spinner.classList.toggle('d-none', !busy);
  }

  function toMoney(val) {
    if (val === null || val === undefined || val === '') return '-';
    const n = Number(val);
    if (!Number.isFinite(n)) return String(val);
    return n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  function esc(str) {
    return String(str == null ? '' : str)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  async function apiFetch(url, options) {
    const res = await fetch(url, Object.assign({ credentials: 'same-origin' }, options || {}));
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.message || data.error || 'Request failed');
    return data;
  }

  function resetForm() {
    if (!fields.id) return;
    fields.id.value = '';
    fields.name.value = '';
    fields.level.value = '1';
    fields.currency.value = 'NGN';
    fields.min_salary.value = '';
    fields.max_salary.value = '';
    fields.description.value = '';
    hideModalAlert(modalAlert);
  }

  function fillForm(row) {
    fields.id.value = row.id || '';
    fields.name.value = row.name || '';
    fields.level.value = row.level || 1;
    fields.currency.value = (row.currency || 'NGN').toUpperCase();
    fields.min_salary.value = row.min_salary == null ? '' : row.min_salary;
    fields.max_salary.value = row.max_salary == null ? '' : row.max_salary;
    fields.description.value = row.description || '';
    hideModalAlert(modalAlert);
  }

  async function loadCurrencies() {
    if (!fields.currency) return;
    const current = (fields.currency.value || 'NGN').toUpperCase();
    try {
      if (typeof window.loadManagedCurrencies === 'function' && typeof window.populateCurrencySelect === 'function') {
        const rows = await window.loadManagedCurrencies(true);
        const preferred = current || 'NGN';
        window.populateCurrencySelect(fields.currency, rows, preferred, { includeInactive: false });
        if (!fields.currency.value) fields.currency.value = preferred;
        return;
      }
      const data = await apiFetch('api/modules/config/currencies');
      const rows = Array.isArray(data.data) ? data.data.filter((r) => Number(r.active || 0) === 1) : [];
      if (!rows.length) return;
      fields.currency.innerHTML = rows.map((r) => `<option value="${esc(String(r.code || '').toUpperCase())}">${esc(String(r.code || '').toUpperCase())}</option>`).join('');
      fields.currency.value = rows.some((r) => String(r.code).toUpperCase() === current) ? current : (rows.some((r) => String(r.code).toUpperCase() === 'NGN') ? 'NGN' : String(rows[0].code).toUpperCase());
    } catch (_) {
      // Keep fallback static options in markup if currency config is unavailable.
    }
  }

  const dt = $('#payGradesTable').DataTable({
    processing: true,
    searching: true,
    ordering: true,
    autoWidth: false,
    ajax: async function (_data, callback) {
      hidePageAlert();
      try {
        const resp = await apiFetch('api/modules/hr/pay_grades');
        callback({ data: Array.isArray(resp.data) ? resp.data : [] });
      } catch (err) {
        showPageAlert(err.message || 'Unable to load pay grades');
        callback({ data: [] });
      }
    },
    language: {
      emptyTable: 'No data in the table...'
    },
    columns: [
      { data: 'name', defaultContent: '-' },
      { data: 'level', defaultContent: '-' },
      { data: 'currency', defaultContent: 'NGN' },
      {
        data: 'min_salary',
        render: (d) => toMoney(d)
      },
      {
        data: 'max_salary',
        render: (d) => toMoney(d)
      },
      { data: 'description', defaultContent: '-', render: (d) => esc(d || '-') },
      ...(canManage ? [{
        data: null,
        orderable: false,
        searchable: false,
        render: function (_d, _t, row) {
          return `
            <div class="d-flex gap-1">
              <button type="button" class="btn btn-sm btn-soft-primary js-pg-edit" data-id="${row.id}"><i class="ri-pencil-line"></i></button>
              <button type="button" class="btn btn-sm btn-soft-danger js-pg-delete" data-id="${row.id}" data-name="${esc(row.name || '')}"><i class="ri-delete-bin-line"></i></button>
            </div>`;
        }
      }] : [])
    ]
  });

  function reloadTable(keepPaging) {
    hidePageAlert();
    apiFetch('api/modules/hr/pay_grades').then(function (resp) {
      const rows = Array.isArray(resp.data) ? resp.data : [];
      dt.clear().rows.add(rows).draw(!keepPaging);
    }).catch(function (err) {
      showPageAlert(err.message || 'Unable to load pay grades');
    });
  }

  document.getElementById('createPayGradeBtn')?.addEventListener('click', async function () {
    resetForm();
    if (modalTitle) modalTitle.textContent = 'New Pay Grade';
    await loadCurrencies();
    if (fields.currency && !fields.currency.value) fields.currency.value = 'NGN';
  });

  $('#payGradesTable tbody').on('click', '.js-pg-edit', async function () {
    const row = dt.row($(this).closest('tr')).data();
    if (!row) return;
    resetForm();
    await loadCurrencies();
    fillForm(row);
    if (modalTitle) modalTitle.textContent = 'Edit Pay Grade';
    modal?.show();
  });

  $('#payGradesTable tbody').on('click', '.js-pg-delete', function () {
    const row = dt.row($(this).closest('tr')).data();
    if (!row) return;
    deleteTarget = row;
    hideModalAlert(deleteAlert);
    if (deleteNameEl) deleteNameEl.textContent = row.name || '';
    deleteModal?.show();
  });

  saveBtn?.addEventListener('click', async function () {
    hideModalAlert(modalAlert);
    const payload = {
      name: (fields.name?.value || '').trim(),
      level: Number(fields.level?.value || 1),
      currency: String(fields.currency?.value || 'NGN').trim().toUpperCase(),
      min_salary: window.moneyVal(fields.min_salary) || '',
      max_salary: window.moneyVal(fields.max_salary) || '',
      description: (fields.description?.value || '').trim()
    };
    if (!payload.name) {
      showModalAlert('Name is required.', modalAlert);
      fields.name?.focus();
      return;
    }
    if (!payload.currency) payload.currency = 'NGN';
    setBtnBusy(saveBtn, true, 'Saving...');
    try {
      if (fields.id && String(fields.id.value || '').trim() !== '') {
        await apiFetch(`api/modules/hr/pay_grades?id=${encodeURIComponent(fields.id.value)}`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
      } else {
        await apiFetch('api/modules/hr/pay_grades', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
      }
      modalEl?.addEventListener('hidden.bs.modal', function () {
        reloadTable(true);
        if (typeof window.showSavedModal === 'function') {
          window.showSavedModal('Saved', 'Pay grade saved successfully.');
        }
      }, { once: true });
      modal?.hide();
    } catch (err) {
      showModalAlert(err.message || 'Unable to save pay grade', modalAlert);
    } finally {
      setBtnBusy(saveBtn, false);
    }
  });

  deleteConfirmBtn?.addEventListener('click', async function () {
    hideModalAlert(deleteAlert);
    if (!deleteTarget || !deleteTarget.id) return;
    setBtnBusy(deleteConfirmBtn, true, 'Deleting...');
    try {
      await apiFetch(`api/modules/hr/pay_grades?id=${encodeURIComponent(deleteTarget.id)}`, { method: 'DELETE' });
      deleteModalEl?.addEventListener('hidden.bs.modal', function () {
        reloadTable(true);
        if (typeof window.showSavedModal === 'function') {
          window.showSavedModal('Deleted', 'Pay grade deleted successfully.');
        }
      }, { once: true });
      deleteModal?.hide();
      deleteTarget = null;
    } catch (err) {
      showModalAlert(err.message || 'Unable to delete pay grade', deleteAlert);
    } finally {
      setBtnBusy(deleteConfirmBtn, false);
    }
  });

  modalEl?.addEventListener('show.bs.modal', loadCurrencies);
})();
