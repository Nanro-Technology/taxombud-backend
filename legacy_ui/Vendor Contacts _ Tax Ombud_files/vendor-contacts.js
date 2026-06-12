/* eslint-disable */
(function () {
  const cfg = window.vendorContactsConfig || {};
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiIndex = apiMap.vendorContactsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/vendor-contacts/index');
  const apiDetail = apiMap.vendorContactsDetail || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/vendor-contacts/detail');

  const canCreate = !!cfg.canCreate;
  const canUpdate = !!cfg.canUpdate;
  const canDelete = !!cfg.canDelete;

  const grid = document.getElementById('vcGrid');
  const alertEl = document.getElementById('vcAlert');
  const searchInput = document.getElementById('vcSearch');
  const designationFilter = document.getElementById('vcDesignation');
  const scopeFilter = document.getElementById('vcScope');
  const addBtn = document.getElementById('vcAddBtn');
  const paginationWrap = document.getElementById('vcPaginationWrap');
  const paginationEl = document.getElementById('vcPagination');

  const modalEl = document.getElementById('vcModal');
  const modal = modalEl && typeof bootstrap !== 'undefined' ? new bootstrap.Modal(modalEl) : null;
  const modalTitle = document.getElementById('vcModalTitle');
  const modalAlert = document.getElementById('vcModalAlert');
  const saveBtn = document.getElementById('vcSaveBtn');
  const nameInput = document.getElementById('vcName');
  const phoneInput = document.getElementById('vcPhone');
  const emailInput = document.getElementById('vcEmail');
  const designationSelect = document.getElementById('vcDesignationSelect');
  const designationOther = document.getElementById('vcDesignationOther');
  const scopeType = document.getElementById('vcScopeType');
  const scopeId = document.getElementById('vcScopeId');
  const notesInput = document.getElementById('vcNotes');

  let currentId = null;
  let metaDomains = [];
  let metaDepts = [];
  let currentPage = 1;
  let totalPages = 1;

  function setAlert(msg) {
    if (!alertEl) return;
    alertEl.textContent = msg || '';
    alertEl.classList.toggle('d-none', !msg);
  }

  function setModalAlert(msg) {
    if (!modalAlert) return;
    modalAlert.textContent = msg || '';
    modalAlert.classList.toggle('d-none', !msg);
  }

  function initials(name) {
    const parts = (name || '').trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return 'VC';
    const first = parts[0][0] || '';
    const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
    return (first + last).toUpperCase();
  }

  function hashCode(str) {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      hash = ((hash << 5) - hash) + str.charCodeAt(i);
      hash |= 0;
    }
    return Math.abs(hash);
  }

  function hslColor(seed, offset) {
    const h = (seed + offset) % 360;
    return `hsl(${h}, 70%, 60%)`;
  }

  function bannerDataUri(seedStr) {
    const seed = hashCode(seedStr || 'vendor');
    const c1 = hslColor(seed, 0);
    const c2 = hslColor(seed, 120);
    const c3 = hslColor(seed, 240);
    const svg = `
      <svg xmlns="http://www.w3.org/2000/svg" width="600" height="200" viewBox="0 0 600 200">
        <defs>
          <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
            <stop offset="0%" stop-color="${c1}"/>
            <stop offset="55%" stop-color="${c2}"/>
            <stop offset="100%" stop-color="${c3}"/>
          </linearGradient>
        </defs>
        <rect width="600" height="200" fill="url(#g)"/>
      </svg>
    `;
    return 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(svg);
  }

  function esc(s) {
    return String(s || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function scopeLabel(row) {
    if (!row) return 'Open';
    if (row.scope_type === 'domain') {
      const match = metaDomains.find(d => String(d.id) === String(row.scope_id));
      return `Domain: ${match ? match.name : 'N/A'}`;
    }
    if (row.scope_type === 'department') {
      const match = metaDepts.find(d => String(d.id) === String(row.scope_id));
      return `Dept: ${match ? match.name : 'N/A'}`;
    }
    return 'Open';
  }

  function renderCard(row) {
    const phone = row.phone ? esc(row.phone) : '';
    const email = row.email ? esc(row.email) : '';
    const scope = scopeLabel(row);
    const menuActions = [];
    if (canUpdate) {
      menuActions.push(`<li><button type="button" class="dropdown-item vc-edit" data-id="${row.id}"><i class="ri-edit-line me-1"></i>Edit</button></li>`);
    }
    if (canDelete) {
      menuActions.push(`<li><button type="button" class="dropdown-item text-danger vc-del" data-id="${row.id}"><i class="ri-delete-bin-6-line me-1"></i>Delete</button></li>`);
    }
    const actionMenu = menuActions.length ? `
      <div class="dropstart position-absolute top-0 end-0">
        <button type="button" class="btn btn-sm btn-icon rounded-circle border-0 shadow-sm" style="background: rgba(255,255,255,0.96); color: #0f172a;" data-bs-toggle="dropdown" aria-expanded="false" aria-label="More actions">
          <i class="ri-more-2-fill fs-16"></i>
        </button>
        <ul class="dropdown-menu dropdown-menu-end">
          ${menuActions.join('')}
        </ul>
      </div>
    ` : '';
    const contactButtons = [];
    if (phone) {
      contactButtons.push(`<a class="btn btn-outline-primary w-100" href="tel:${phone}"><i class="ri-phone-line me-1"></i>Call</a>`);
    }
    if (email) {
      contactButtons.push(`<a class="btn btn-soft-primary w-100" href="mailto:${email}"><i class="ri-mail-line me-1"></i>Email</a>`);
    }
    const buttons = contactButtons.length
      ? `<div class="d-flex gap-2 justify-content-center">${contactButtons.join('')}</div>`
      : '';
    return `
      <div class="col-sm-4 col-lg-3">
        <div class="card">
          <div class="position-relative">
            <div class="vc-banner"></div>
            <div class="position-absolute start-0 w-100 px-3 py-2 text-white vc-name-banner text-center">
              ${esc(row.name)}
            </div>
          </div>
          <div class="card-body">
            <div class="text-center position-relative">
              ${actionMenu}
              <div class="avatar-md rounded-circle mt-n5 img-thumbnail border-light mx-auto d-block bg-light text-primary d-flex align-items-center justify-content-center fw-semibold">
                ${initials(row.name)}
              </div>
              <h5 class="mt-2 mb-1">${esc(row.name)}</h5>
              <p class="text-muted mb-2">${esc(row.designation || '')}</p>
              <div class="small text-muted">
                <div><i class="ri-phone-line me-1"></i>${phone || 'N/A'}</div>
                <div><i class="ri-mail-line me-1"></i>${email || 'N/A'}</div>
              </div>
              ${row.notes ? `<p class="text-muted">${esc(row.notes)}</p>` : `<p class="text-muted">${phone || email ? 'Available for support' : 'Vendor contact'}</p>`}
              ${buttons}
              <div class="mt-2">
                <span class="badge bg-light text-muted vc-badge">${esc(scope)}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  function renderList(rows) {
    if (!grid) return;
    if (!rows || !rows.length) {
      grid.innerHTML = '<div class="col"><div class="text-muted">No vendor contacts yet.</div></div>';
      return;
    }
    grid.innerHTML = rows.map(renderCard).join('');
  }

  function fillDesignationFilter(list) {
    if (!designationFilter) return;
    const current = designationFilter.value || '';
    designationFilter.innerHTML = '<option value="">All</option>';
    (list || []).forEach(d => {
      const opt = document.createElement('option');
      opt.value = d;
      opt.textContent = d;
      designationFilter.appendChild(opt);
    });
    if (current) designationFilter.value = current;
  }

  function fillDesignationSelect(list, current) {
    if (!designationSelect) return;
    designationSelect.innerHTML = '';
    (list || []).forEach(d => {
      const opt = document.createElement('option');
      opt.value = d;
      opt.textContent = d;
      designationSelect.appendChild(opt);
    });
    const other = document.createElement('option');
    other.value = '__other__';
    other.textContent = 'Add New';
    designationSelect.appendChild(other);
    if (designationOther) {
      designationOther.disabled = true;
    }
    if (current && list.includes(current)) {
      designationSelect.value = current;
      if (designationOther) designationOther.value = '';
    } else if (current) {
      designationSelect.value = '__other__';
      if (designationOther) designationOther.value = current;
      if (designationOther) designationOther.disabled = false;
    } else {
      designationSelect.value = '__other__';
      if (designationOther) {
        designationOther.value = '';
        designationOther.disabled = false;
      }
    }
  }

  function fillScopeTargets(type, currentId) {
    if (!scopeId) return;
    scopeId.innerHTML = '<option value="">Select</option>';
    let list = [];
    if (type === 'domain') list = metaDomains;
    if (type === 'department') list = metaDepts;
    list.forEach(item => {
      const opt = document.createElement('option');
      opt.value = item.id;
      opt.textContent = item.name;
      scopeId.appendChild(opt);
    });
    if (currentId) scopeId.value = String(currentId);
    scopeId.disabled = type === 'open';
  }

  function getDesignationValue() {
    if (!designationSelect) return '';
    if (designationSelect.value === '__other__') {
      return (designationOther && designationOther.value || '').trim();
    }
    return designationSelect.value;
  }

  function resetModal() {
    currentId = null;
    setModalAlert('');
    if (nameInput) nameInput.value = '';
    if (phoneInput) phoneInput.value = '';
    if (emailInput) emailInput.value = '';
    if (notesInput) notesInput.value = '';
    if (scopeType) scopeType.value = 'open';
    fillScopeTargets('open');
  }

  function renderPagination() {
    if (!paginationEl || !paginationWrap) return;
    if (!totalPages || totalPages <= 1) {
      paginationEl.innerHTML = '';
      paginationWrap.classList.add('d-none');
      return;
    }
    paginationWrap.classList.remove('d-none');
    const items = [];
    const addItem = (label, page, disabled, active) => {
      items.push(`
        <li class="page-item${disabled ? ' disabled' : ''}${active ? ' active' : ''}">
          <a class="page-link" href="javascript:void(0);" data-page="${page}">${label}</a>
        </li>
      `);
    };
    addItem('&laquo;', Math.max(1, currentPage - 1), currentPage === 1, false);
    const windowSize = 2;
    const start = Math.max(1, currentPage - windowSize);
    const end = Math.min(totalPages, currentPage + windowSize);
    if (start > 1) {
      addItem('1', 1, false, currentPage === 1);
      if (start > 2) {
        items.push('<li class="page-item disabled"><span class="page-link">…</span></li>');
      }
    }
    for (let p = start; p <= end; p++) {
      addItem(String(p), p, false, p === currentPage);
    }
    if (end < totalPages) {
      if (end < totalPages - 1) {
        items.push('<li class="page-item disabled"><span class="page-link">…</span></li>');
      }
      addItem(String(totalPages), totalPages, false, currentPage === totalPages);
    }
    addItem('&raquo;', Math.min(totalPages, currentPage + 1), currentPage === totalPages, false);
    paginationEl.innerHTML = items.join('');
  }

  function loadList(page = 1) {
    const params = new URLSearchParams();
    currentPage = Math.max(1, parseInt(page || 1, 10) || 1);
    params.set('limit', '500');
    params.set('page', String(currentPage));
    if (searchInput && searchInput.value.trim()) params.set('q', searchInput.value.trim());
    if (designationFilter && designationFilter.value) params.set('designation', designationFilter.value);
    if (scopeFilter && scopeFilter.value) params.set('scope_type', scopeFilter.value);
    setAlert('');
    return fetch(apiIndex + '?' + params.toString())
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error(data.error || 'Unable to load contacts');
        metaDomains = data.domains || [];
        metaDepts = data.departments || [];
        fillDesignationFilter(data.designations || []);
        renderList(data.data || []);
        totalPages = parseInt(data.pages || 1, 10) || 1;
        if (currentPage > totalPages) currentPage = totalPages;
        renderPagination();
      })
      .catch(err => setAlert(err.message || 'Unable to load contacts'));
  }

  function openModal(row) {
    if (!modal) return;
    resetModal();
    if (row) {
      currentId = row.id;
      if (modalTitle) modalTitle.textContent = 'Edit Vendor Contact';
      if (nameInput) nameInput.value = row.name || '';
      if (phoneInput) phoneInput.value = row.phone || '';
      if (emailInput) emailInput.value = row.email || '';
      if (notesInput) notesInput.value = row.notes || '';
      if (scopeType) scopeType.value = row.scope_type || 'open';
      fillScopeTargets(scopeType.value, row.scope_id);
      fillDesignationSelect((designationFilter ? Array.from(designationFilter.options).map(o => o.value).filter(Boolean) : []), row.designation || '');
    } else {
      if (modalTitle) modalTitle.textContent = 'Add Vendor Contact';
      fillDesignationSelect((designationFilter ? Array.from(designationFilter.options).map(o => o.value).filter(Boolean) : []), '');
      fillScopeTargets('open');
    }
    modal.show();
  }

  function saveContact() {
    setModalAlert('');
    const payload = {
      name: (nameInput && nameInput.value || '').trim(),
      phone: (phoneInput && phoneInput.value || '').trim(),
      email: (emailInput && emailInput.value || '').trim(),
      notes: (notesInput && notesInput.value || '').trim(),
      designation: getDesignationValue(),
      scope_type: (scopeType && scopeType.value) || 'open',
      scope_id: (scopeId && scopeId.value) || ''
    };
    if (!payload.name) return setModalAlert('Name is required');
    if (!payload.designation) return setModalAlert('Designation is required');
    if (payload.scope_type !== 'open' && !payload.scope_id) return setModalAlert('Select a scope target');

    const url = currentId ? (apiDetail + '?id=' + encodeURIComponent(currentId)) : apiIndex;
    const method = currentId ? 'PATCH' : 'POST';
    if (saveBtn) {
      saveBtn.disabled = true;
      const spinner = saveBtn.querySelector('.spinner-border');
      const btnText = saveBtn.querySelector('.btn-text');
      if (spinner) spinner.classList.remove('d-none');
      if (btnText) btnText.textContent = 'Saving...';
    }
    return fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error(data.error || 'Unable to save');
        modal.hide();
        return loadList(currentPage);
      })
      .catch(err => setModalAlert(err.message || 'Unable to save'))
      .finally(() => {
        if (saveBtn) {
          saveBtn.disabled = false;
          const spinner = saveBtn.querySelector('.spinner-border');
          const btnText = saveBtn.querySelector('.btn-text');
          if (spinner) spinner.classList.add('d-none');
          if (btnText) btnText.textContent = 'Save';
        }
      });
  }

  async function deleteContact(id) {
    if (!id) return;
    const ok = await window.crmUiConfirm('Delete this vendor contact?', 'Delete Vendor Contact', {
      okText: 'Delete',
      cancelText: 'Cancel',
      variant: 'danger',
      icon: 'warning'
    });
    if (!ok) return;
    fetch(apiDetail + '?id=' + encodeURIComponent(id), { method: 'DELETE' })
      .then(r => r.json().then(data => ({ ok: r.ok, data })))
      .then(({ ok, data }) => {
        if (!ok) throw new Error(data.error || 'Unable to delete');
        loadList(currentPage);
      })
      .catch(err => setAlert(err.message || 'Unable to delete'));
  }

  if (scopeType) {
    scopeType.addEventListener('change', () => fillScopeTargets(scopeType.value));
  }
  if (designationSelect) {
    designationSelect.addEventListener('change', () => {
      if (!designationOther) return;
      if (designationSelect.value === '__other__') {
        designationOther.disabled = false;
      } else {
        designationOther.disabled = true;
        designationOther.value = '';
      }
    });
  }
  if (addBtn) addBtn.addEventListener('click', () => openModal(null));
  if (saveBtn) saveBtn.addEventListener('click', saveContact);
  if (searchInput) searchInput.addEventListener('input', () => loadList(1));
  if (designationFilter) designationFilter.addEventListener('change', () => loadList(1));
  if (scopeFilter) scopeFilter.addEventListener('change', () => loadList(1));
  if (paginationEl) {
    paginationEl.addEventListener('click', (e) => {
      const link = e.target.closest('[data-page]');
      if (!link) return;
      const page = parseInt(link.getAttribute('data-page'), 10) || 1;
      if (page === currentPage) return;
      loadList(page);
    });
  }

  if (grid) {
    grid.addEventListener('click', (e) => {
      const editBtn = e.target.closest('.vc-edit');
      const delBtn = e.target.closest('.vc-del');
      if (editBtn) {
        const id = editBtn.getAttribute('data-id');
        fetch(apiDetail + '?id=' + encodeURIComponent(id))
          .then(r => r.json().then(data => ({ ok: r.ok, data })))
          .then(({ ok, data }) => {
            if (!ok) throw new Error(data.error || 'Unable to load contact');
            openModal(data.data);
          })
          .catch(err => setAlert(err.message || 'Unable to load contact'));
      }
      if (delBtn) {
        deleteContact(delBtn.getAttribute('data-id'));
      }
    });
  }

  loadList(1);
})();
