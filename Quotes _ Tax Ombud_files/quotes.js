/* eslint-disable */
(function () {
  "use strict";

  const cfg = window.quotesConfig || {};
  const canDelete = !!cfg.canDelete;
  const canUpdate = !!cfg.canUpdate;

  const api = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiIndex = api.quotesIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/quotes/index');
  const apiDetail = api.quotesDetail || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/quotes/detail');

  const pageSize = (window.appConfig && window.appConfig.dataTablePageSize) ? Number(window.appConfig.dataTablePageSize) : 250;

  const searchInput = document.getElementById('quotesSearch');
  const statusFilter = document.getElementById('quotesStatusFilter');
  const parentTypeFilter = document.getElementById('quotesParentTypeFilter');
  const parentFilter = document.getElementById('quotesParentIdFilter');
  const parentSearchInput = document.getElementById('quotesParentSearch');
  const applyBtn = document.getElementById('quotesApplyFilterBtn');
  const alertBox = document.getElementById('quotesAlert');

  const parentMeta = {
    account: { lookup: 'accounts', allLabel: 'All accounts', searchPlaceholder: 'Search account' },
    contact: { lookup: 'contacts', allLabel: 'All contacts', searchPlaceholder: 'Search contact' },
    organization: { lookup: 'organizations', allLabel: 'All organizations', searchPlaceholder: 'Search organization' }
  };

  let table = null;
  let quotesLoading = false;

  function esc(v) {
    return String(v || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function showAlert(msg) {
    if (!alertBox) return;
    alertBox.textContent = msg || '';
  }

  function badge(status) {
    const key = String(status || '').toLowerCase();
    const cls = {
      draft: 'bg-secondary-subtle text-secondary',
      sent: 'bg-info-subtle text-info',
      approved: 'bg-success-subtle text-success',
      rejected: 'bg-danger-subtle text-danger',
      expired: 'bg-warning-subtle text-warning'
    }[key] || 'bg-light text-muted';
    return `<span class="badge ${cls}">${esc(status || '-')}</span>`;
  }

  function debounce(fn, wait) {
    let t = null;
    return function (...args) {
      if (t) clearTimeout(t);
      t = setTimeout(() => fn.apply(this, args), wait);
    };
  }

  function getParentType() {
    const t = String((parentTypeFilter && parentTypeFilter.value) || 'account').toLowerCase();
    return parentMeta[t] ? t : 'account';
  }

  function buildParentFilterParams() {
    const selectedId = Number((parentFilter && parentFilter.value) || 0);
    if (!selectedId) {
      return { account_id: '', contact_id: '', organization_id: '' };
    }
    const type = getParentType();
    return {
      account_id: type === 'account' ? String(selectedId) : '',
      contact_id: type === 'contact' ? String(selectedId) : '',
      organization_id: type === 'organization' ? String(selectedId) : ''
    };
  }

  function setParentFilterUi(type, preserveValue) {
    const meta = parentMeta[type] || parentMeta.account;
    const oldValue = preserveValue && parentFilter ? String(parentFilter.value || '') : '';
    if (parentSearchInput) parentSearchInput.placeholder = meta.searchPlaceholder;
    if (parentFilter) {
      parentFilter.innerHTML = `<option value="">${meta.allLabel}</option>`;
      if (oldValue) {
        const opt = document.createElement('option');
        opt.value = oldValue;
        opt.textContent = '#' + oldValue;
        parentFilter.appendChild(opt);
        parentFilter.value = oldValue;
      }
    }
  }

  async function loadLookup(type, q, selectEl, emptyText) {
    if (!selectEl) return;
    const params = new URLSearchParams();
    params.set('lookup', type);
    params.set('limit', '10');
    if (q) params.set('q', q);
    const res = await fetch(apiIndex + '?' + params.toString());
    const data = await res.json().catch(() => ({}));
    if (!res.ok) return;
    const old = String(selectEl.value || '');
    const oldLabel = old && selectEl.selectedOptions && selectEl.selectedOptions.length ? selectEl.selectedOptions[0].textContent : ('#' + old);
    selectEl.innerHTML = `<option value="">${emptyText}</option>`;
    (data.data || []).forEach((row) => {
      const opt = document.createElement('option');
      opt.value = row.id;
      opt.textContent = row.label || ('#' + row.id);
      selectEl.appendChild(opt);
    });
    if (old) {
      if (!selectEl.querySelector(`option[value="${old}"]`)) {
        const opt = document.createElement('option');
        opt.value = old;
        opt.textContent = oldLabel;
        selectEl.appendChild(opt);
      }
      selectEl.value = old;
    }
  }

  async function loadParentLookup(q) {
    const type = getParentType();
    const meta = parentMeta[type] || parentMeta.account;
    return loadLookup(meta.lookup, q, parentFilter, meta.allLabel);
  }

  function initTable() {
    table = window.jQuery('#quotesTable').DataTable({
      processing: true,
      serverSide: true,
      searching: false,
      pageLength: pageSize,
      lengthMenu: [[pageSize], [pageSize]],
      ajax: {
        url: apiIndex,
        type: 'GET',
        dataSrc: 'data',
        data: function (d) {
          d.q = (searchInput && searchInput.value || '').trim();
          d.status = statusFilter ? statusFilter.value : '';
          const parentParams = buildParentFilterParams();
          d.account_id = parentParams.account_id;
          d.contact_id = parentParams.contact_id;
          d.organization_id = parentParams.organization_id;
          d.limit = d.length || pageSize;
        }
      },
      columns: [
        { data: null, render: (row, type, full, meta) => meta.row + 1 + meta.settings._iDisplayStart },
        {
          data: null,
          render: (row) => {
            const href = `studio/quotes/view.kml?id=${encodeURIComponent(row.id_s || row.id)}`;
            const title = row.title || ('Quote #' + row.id);
            return `<div class="fw-semibold"><a href="${href}" class="text-decoration-none">${esc(row.quote_number || '-')}</a></div><div class="small text-muted">${esc(title)}</div>`;
          }
        },
        {
          data: null,
          render: (row) => {
            const chunks = [];
            if (row.account_name) chunks.push('Account: ' + row.account_name);
            if (row.contact_name) chunks.push('Contact: ' + row.contact_name);
            if (row.organization_name) chunks.push('Org: ' + row.organization_name);
            return esc(chunks.join(' | ') || '-');
          }
        },
        { data: 'status', render: (v) => badge(v) },
        {
          data: null,
          render: (row) => `<div>${esc(row.issued_date || '-')}</div><div class="small text-muted">${esc(row.expiry_date || '-')}</div>`
        },
        {
          data: null,
          render: (row) => `${Number(row.total_amount || 0).toLocaleString()} ${esc(row.currency || 'NGN')}`
        },
        {
          data: null,
          orderable: false,
          className: 'text-end',
          render: (row) => {
            const viewBtn = `<a class="btn btn-sm btn-outline-primary me-1" href="studio/quotes/view.kml?id=${encodeURIComponent(row.id_s || row.id)}"><i class="ri-eye-line me-1"></i>View</a>`;
            const editBtn = canUpdate ? `<a class="btn btn-sm btn-soft-primary me-1" href="studio/quotes/quotes-create.kml?id=${encodeURIComponent(row.id_s || row.id)}"><i class="ri-edit-line me-1"></i>Edit</a>` : '';
            const delBtn = canDelete ? `<button class="btn btn-sm btn-soft-danger quote-del" data-id="${row.id}"><i class="ri-delete-bin-6-line me-1"></i>Delete</button>` : '';
            return viewBtn + editBtn + delBtn;
          }
        }
      ]
    });
    window.jQuery('#quotesTable').on('xhr.dt error.dt', function () {
      quotesLoading = false;
      if (window.toggleButtonLoading) window.toggleButtonLoading(applyBtn, false);
    });
  }

  async function deleteQuote(id, trigger) {
    const ok = await window.crmUiConfirm('Delete this quote?', 'Delete Quote', {
      okText: 'Delete',
      cancelText: 'Cancel',
      variant: 'danger',
      icon: 'warning'
    });
    if (!ok) return;
    if (trigger && trigger.dataset.busy === '1') return;
    try {
      if (trigger) {
        trigger.dataset.busy = '1';
        if (window.toggleButtonLoading) window.toggleButtonLoading(trigger, true, 'Deleting...');
      }
      const res = await fetch(apiDetail + '?id=' + encodeURIComponent(id), { method: 'DELETE' });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || 'Unable to delete quote');
      if (table) table.ajax.reload(null, false);
    } catch (e) {
      showAlert(e.message || 'Unable to delete quote');
    } finally {
      if (trigger) {
        trigger.dataset.busy = '0';
        if (window.toggleButtonLoading) window.toggleButtonLoading(trigger, false);
      }
    }
  }

  document.addEventListener('click', function (e) {
    const btn = e.target.closest('.quote-del');
    if (!btn) return;
    e.preventDefault();
    deleteQuote(btn.getAttribute('data-id'), btn);
  });

  applyBtn && applyBtn.addEventListener('click', () => {
    if (quotesLoading) return;
    quotesLoading = true;
    if (window.toggleButtonLoading) window.toggleButtonLoading(applyBtn, true, 'Applying...');
    if (table) table.ajax.reload();
    if (!table) {
      quotesLoading = false;
      if (window.toggleButtonLoading) window.toggleButtonLoading(applyBtn, false);
    }
  });
  searchInput && searchInput.addEventListener('keyup', function (e) {
    if (e.key === 'Enter' && table) table.ajax.reload();
  });

  parentTypeFilter && parentTypeFilter.addEventListener('change', () => {
    const type = getParentType();
    setParentFilterUi(type, false);
    if (parentSearchInput) parentSearchInput.value = '';
    loadParentLookup('').catch(() => {});
  });

  parentSearchInput && parentSearchInput.addEventListener('input', debounce(() => {
    loadParentLookup((parentSearchInput.value || '').trim()).catch(() => {});
  }, 250));

  setParentFilterUi(getParentType(), false);
  loadParentLookup('').finally(initTable);
})();
