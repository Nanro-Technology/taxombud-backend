(function () {
  const cfg = window.inventoryConfig || {};
  const tableBody = document.getElementById('inventoryBody');
  const searchBtn = document.getElementById('invSearchBtn');
  const searchInput = document.getElementById('invSearch');
  const statusSel = document.getElementById('invStatus');
  const categorySel = document.getElementById('invCategory');
  const deptSel = document.getElementById('invDept');
  const locInput = document.getElementById('invLocation');
  const saveBtn = document.getElementById('saveInventoryBtn');
  const statusBtn = document.getElementById('saveStatusBtn');
  const checkDept = document.getElementById('checkDepartment');
  const loadCheckItemsBtn = document.getElementById('loadCheckItems');
  const checkItemsBody = document.getElementById('checkItemsBody');
  const submitCheckBtn = document.getElementById('submitInventoryCheck');
  const summaryTotal = document.getElementById('invSumTotal');
  const summaryActive = document.getElementById('invSumActive');
  const summaryDamaged = document.getElementById('invSumDamaged');
  const summaryRetired = document.getElementById('invSumRetired');
  const deptList = document.getElementById('invDeptList');
  const assignedUserSelect = document.getElementById('invAssignedUser');
  const staffUsers = Array.isArray(cfg.staffUsers) ? cfg.staffUsers : [];
  const inventoryModalEl = document.getElementById('inventoryModal');

  function esc(v) {
    return String(v || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function api(url, options = {}) {
    return fetch(url, {
      credentials: 'same-origin',
      headers: {'Content-Type': 'application/json'},
      ...options
    }).then(r => r.json().then(d => ({ok: r.ok, status: r.status, data: d})));
  }

  function uploadFile(file) {
    const form = new FormData();
    form.append('file', file);
    return fetch('api/modules/files/index', {
      method: 'POST',
      credentials: 'same-origin',
      body: form
    }).then(r => r.json().then(d => ({ ok: r.ok, data: d })));
  }

  function renderRows(items) {
    if (!tableBody) return;
    if (!items || !items.length) {
      tableBody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">No records</td></tr>';
      return;
    }
    tableBody.innerHTML = items.map((row, idx) => {
      const statusBadge = `<span class="badge bg-${row.status === 'active' ? 'success' : row.status === 'retired' ? 'secondary' : 'warning'}">${row.status}</span>`;
      const viewUrl = row.id_salt ? `studio/inventory/view.kml?id=${encodeURIComponent(row.id_salt)}` : '#';
      return `
        <tr>
          <td>${idx + 1}</td>
          <td>${row.inventory_uid || ''}</td>
          <td>${row.name || ''}</td>
          <td>${row.category_code || ''}</td>
          <td>${row.department_name || '-'}</td>
          <td>${row.location || '-'}</td>
          <td>${row.quantity || 1}</td>
          <td>${statusBadge}</td>
          <td>
            <a class="btn btn-sm btn-outline-primary me-1" href="${viewUrl}"><i class="ri-eye-line"></i></a>
            ${cfg.canUpdate ? `<button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${row.id}"><i class="ri-edit-line"></i></button>` : ''}
            ${cfg.canUpdate ? `<button class="btn btn-sm btn-outline-warning" data-action="status" data-id="${row.id}"><i class="ri-refresh-line"></i></button>` : ''}
          </td>
        </tr>
      `;
    }).join('');
    initInventoryTable();
  }

  function renderSummary(data) {
    if (!data) return;
    const status = data.inventory_status_counts || [];
    const byDept = data.inventory_by_department || [];
    const get = (key) => {
      const row = status.find((r) => (r.status || '').toLowerCase() === key);
      return Number(row ? row.total || 0 : 0);
    };
    const total = status.reduce((sum, r) => sum + Number(r.total || 0), 0);
    if (summaryTotal) summaryTotal.textContent = total;
    if (summaryActive) summaryActive.textContent = get('active');
    if (summaryDamaged) summaryDamaged.textContent = get('damaged');
    if (summaryRetired) summaryRetired.textContent = get('retired');
    if (deptList) {
      if (!byDept.length) {
        deptList.innerHTML = '<div class="text-muted">No department data</div>';
      } else {
        deptList.innerHTML = byDept.map((row) => `
          <a href="javascript:void(0);" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3">
            <span class="fw-semibold">${row.department_name || 'Unassigned'}</span>
            <span class="badge bg-primary rounded-pill">${row.total || 0}</span>
          </a>
        `).join('');
      }
    }
  }

  function loadSummary() {
    if (!cfg.apiReports) return;
    fetch(cfg.apiReports, { credentials: 'same-origin' })
      .then((r) => r.json().then((d) => ({ ok: r.ok, data: d })))
      .then(({ ok, data }) => {
        if (!ok) return;
        renderSummary(data);
      })
      .catch(() => {});
  }

  function loadInventory() {
    if (!tableBody) return;
    const tableEl = document.getElementById('inventoryTable');
    if (tableEl && typeof $ !== 'undefined' && $.fn.DataTable && $.fn.DataTable.isDataTable(tableEl)) {
      $(tableEl).DataTable().clear().destroy();
    }
    tableBody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">Loading...</td></tr>';
    const qs = new URLSearchParams();
    if (searchInput && searchInput.value.trim()) qs.set('q', searchInput.value.trim());
    if (statusSel && statusSel.value) qs.set('status', statusSel.value);
    if (categorySel && categorySel.value) qs.set('category', categorySel.value);
    if (deptSel && deptSel.value) qs.set('department_id', deptSel.value);
    if (locInput && locInput.value.trim()) qs.set('location', locInput.value.trim());

    api(`${cfg.apiList}?${qs.toString()}`).then(({ok, data}) => {
      if (!ok) {
        tableBody.innerHTML = '<tr><td colspan="9" class="text-center text-muted">Unable to load</td></tr>';
        return;
      }
      renderRows(data.data || []);
    });
  }

  function initInventoryTable() {
    const tableEl = document.getElementById('inventoryTable');
    if (!tableEl || typeof $ === 'undefined' || !$.fn.DataTable) return;
    if ($.fn.DataTable.isDataTable(tableEl)) {
      $(tableEl).DataTable().clear().destroy();
    }
    $(tableEl).DataTable({
      pageLength: 25,
      lengthChange: false,
      ordering: true,
      searching: false,
      info: true,
      autoWidth: false
    });
  }

  function openEdit(id) {
    api(`${cfg.apiDetail}?id=${encodeURIComponent(id)}`).then(({ok, data}) => {
      if (!ok) return;
      document.getElementById('invId').value = data.id;
      document.getElementById('invName').value = data.name || '';
      document.getElementById('invCategoryField').value = data.category_code || '';
      document.getElementById('invDescription').value = data.description || '';
      const deptVal = data.department_id || data.department_id_effective || '';
      const deptSelect = document.getElementById('invDepartment');
      if (deptSelect) {
        const hasDept = Array.from(deptSelect.options).some(opt => String(opt.value) === String(deptVal));
        if (deptVal && !hasDept) {
          const opt = document.createElement('option');
          opt.value = deptVal;
          opt.textContent = data.department_name || `Department #${deptVal}`;
          deptSelect.appendChild(opt);
        }
        deptSelect.value = deptVal;
      }
      updateAssignedUsers();
      if (assignedUserSelect) {
        const assignedId = data.assigned_user_id || '';
        if (assignedId) {
          const exists = Array.from(assignedUserSelect.options).some(opt => String(opt.value) === String(assignedId));
          if (!exists) {
            const opt = document.createElement('option');
            opt.value = assignedId;
            opt.textContent = data.assigned_user_name || ('User #' + assignedId);
            assignedUserSelect.appendChild(opt);
          }
          assignedUserSelect.value = assignedId;
        } else {
          assignedUserSelect.value = '';
        }
      }
      document.getElementById('invLocationField').value = data.location || '';
      document.getElementById('invMode').value = data.mode || 'single';
      document.getElementById('invQuantity').value = data.quantity || 1;
      document.getElementById('invSerial').value = data.serial_number || '';
      const imgInput = document.getElementById('invImage');
      if (imgInput) imgInput.value = '';
      const modal = new bootstrap.Modal(document.getElementById('inventoryModal'));
      modal.show();
    });
  }

  function resetInventoryForm() {
    document.getElementById('invId').value = '';
    document.getElementById('invName').value = '';
    document.getElementById('invCategoryField').value = '';
    document.getElementById('invDescription').value = '';
    document.getElementById('invDepartment').value = '';
    if (assignedUserSelect) assignedUserSelect.innerHTML = '<option value="">Unassigned</option>';
    document.getElementById('invLocationField').value = '';
    document.getElementById('invMode').value = 'single';
    document.getElementById('invQuantity').value = 1;
    document.getElementById('invSerial').value = '';
    const imgInput = document.getElementById('invImage');
    if (imgInput) imgInput.value = '';
  }

  function openStatus(id) {
    document.getElementById('invStatusId').value = id;
    document.getElementById('invStatusNote').value = '';
    const modal = new bootstrap.Modal(document.getElementById('invStatusModal'));
    modal.show();
  }

  if (tableBody) {
    tableBody.addEventListener('click', (e) => {
      const btn = e.target.closest('button');
      if (!btn) return;
      const action = btn.getAttribute('data-action');
      const id = btn.getAttribute('data-id');
      if (action === 'edit') openEdit(id);
      if (action === 'status') openStatus(id);
    });
  }

  if (searchBtn) searchBtn.addEventListener('click', loadInventory);

  function updateAssignedUsers() {
    if (!assignedUserSelect) return;
    const deptId = document.getElementById('invDepartment')?.value || '';
    assignedUserSelect.innerHTML = '<option value="">Unassigned</option>';
    if (!deptId) return;
    staffUsers
      .filter(u => String(u.department_id || '') === String(deptId))
      .forEach(u => {
        const label = u.display_name || ('User #' + u.id);
        const opt = document.createElement('option');
        opt.value = u.id;
        opt.textContent = label;
        assignedUserSelect.appendChild(opt);
      });
  }

  const deptField = document.getElementById('invDepartment');
  if (deptField) deptField.addEventListener('change', () => {
    updateAssignedUsers();
  });

  if (saveBtn) {
    saveBtn.addEventListener('click', () => {
      if (window.toggleButtonLoading) window.toggleButtonLoading(saveBtn, true, 'Saving...');
      const imgInput = document.getElementById('invImage');
      const payload = {
        name: document.getElementById('invName').value.trim(),
        category_code: document.getElementById('invCategoryField').value,
        description: document.getElementById('invDescription').value.trim(),
        department_id: document.getElementById('invDepartment').value,
        assigned_user_id: document.getElementById('invAssignedUser')?.value || '',
        location: document.getElementById('invLocationField').value.trim(),
        mode: document.getElementById('invMode').value,
        quantity: document.getElementById('invQuantity').value,
        serial_number: document.getElementById('invSerial').value.trim()
      };
      if (!payload.serial_number) {
        window.crmUiAlert('Serial number is required.');
        if (window.toggleButtonLoading) window.toggleButtonLoading(saveBtn, false);
        return;
      }
      const id = document.getElementById('invId').value;
      const url = id ? `${cfg.apiDetail}?id=${encodeURIComponent(id)}` : cfg.apiList;

      const proceedSave = (imageFileId) => {
        if (imageFileId) payload.image_file_id = imageFileId;
        const options = id
          ? {method: 'PATCH', body: JSON.stringify({...payload, action: 'update'})}
          : {method: 'POST', body: JSON.stringify(payload)};
        api(url, options)
          .then(({ok, data}) => {
            if (!ok) {
              window.crmUiAlert(data.error || 'Unable to save');
              return;
            }
            const modalEl = document.getElementById('inventoryModal');
            resetInventoryForm();
            bootstrap.Modal.getInstance(modalEl)?.hide();
            loadInventory();
          })
          .catch(() => {
            window.crmUiAlert('Unable to save');
          })
          .finally(() => {
            if (window.toggleButtonLoading) window.toggleButtonLoading(saveBtn, false);
          });
      };

      if (imgInput && imgInput.files && imgInput.files[0]) {
        uploadFile(imgInput.files[0])
          .then(({ok, data}) => {
            if (!ok) {
              window.crmUiAlert(data.error || 'Unable to upload image');
              if (window.toggleButtonLoading) window.toggleButtonLoading(saveBtn, false);
              return;
            }
            proceedSave(data.id);
          })
          .catch(() => {
            window.crmUiAlert('Unable to upload image');
            if (window.toggleButtonLoading) window.toggleButtonLoading(saveBtn, false);
          });
      } else {
        proceedSave(null);
      }
    });
  }

  if (statusBtn) {
    statusBtn.addEventListener('click', () => {
      if (window.toggleButtonLoading) window.toggleButtonLoading(statusBtn, true, 'Updating...');
      const id = document.getElementById('invStatusId').value;
      const status = document.getElementById('invStatusField').value;
      const note = document.getElementById('invStatusNote').value.trim();
      api(`${cfg.apiDetail}?id=${encodeURIComponent(id)}`, {
        method: 'PATCH',
        body: JSON.stringify({action: 'status', status, note})
      })
        .then(({ok, data}) => {
          if (!ok) {
            window.crmUiAlert(data.error || 'Unable to update status');
            return;
          }
          const modalEl = document.getElementById('invStatusModal');
          bootstrap.Modal.getInstance(modalEl)?.hide();
          loadInventory();
        })
        .catch(() => {
          window.crmUiAlert('Unable to update status');
        })
        .finally(() => {
          if (window.toggleButtonLoading) window.toggleButtonLoading(statusBtn, false);
        });
    });
  }

  function renderCheckItems(items) {
    if (!checkItemsBody) return;
    if (!items || !items.length) {
      checkItemsBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No items in this department.</td></tr>';
      return;
    }
    checkItemsBody.innerHTML = items.map(row => `
      <tr data-id="${row.id}">
        <td>${esc(row.name || '')} <div class="small text-muted">${esc(row.inventory_uid || '')}</div></td>
        <td class="text-muted small">
          <span class="inv-check-desc" title="${esc(row.description || '-')}">${esc(row.description || '-')}</span>
        </td>
        <td title="${esc(row.assigned_user_name || '-')}">${esc(row.assigned_user_name || '-')}</td>
        <td>
          <select class="form-select form-select-sm check-status">
            <option value="confirmed">Confirmed</option>
            <option value="missing">Missing</option>
            <option value="damaged">Damaged</option>
          </select>
        </td>
        <td><input type="number" class="form-control form-control-sm check-qty" min="0" value="${row.quantity || 0}"></td>
        <td><input type="text" class="form-control form-control-sm check-note" placeholder="Optional note"></td>
      </tr>
    `).join('');
  }

  if (loadCheckItemsBtn) {
    loadCheckItemsBtn.addEventListener('click', () => {
      if (window.toggleButtonLoading) window.toggleButtonLoading(loadCheckItemsBtn, true, 'Loading...');
      if (!checkDept || !checkDept.value) {
        window.crmUiAlert('Select a department');
        if (window.toggleButtonLoading) window.toggleButtonLoading(loadCheckItemsBtn, false);
        return;
      }
      if (checkItemsBody) {
        checkItemsBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Loading...</td></tr>';
      }
      api(`${cfg.apiList}?department_id=${encodeURIComponent(checkDept.value)}&include_retired=1`)
        .then(({ok, data}) => {
          if (!ok) {
            if (checkItemsBody) checkItemsBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Unable to load items</td></tr>';
            return;
          }
          renderCheckItems(data.data || []);
        })
        .catch(() => {
          if (checkItemsBody) checkItemsBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Unable to load items</td></tr>';
        })
        .finally(() => {
          if (window.toggleButtonLoading) window.toggleButtonLoading(loadCheckItemsBtn, false);
        });
    });
  }

  if (submitCheckBtn) {
    submitCheckBtn.addEventListener('click', () => {
      if (window.toggleButtonLoading) window.toggleButtonLoading(submitCheckBtn, true, 'Submitting...');
      if (!checkDept || !checkDept.value) {
        window.crmUiAlert('Select a department');
        if (window.toggleButtonLoading) window.toggleButtonLoading(submitCheckBtn, false);
        return;
      }
      const rows = Array.from(checkItemsBody?.querySelectorAll('tr') || []);
      const items = rows.map(tr => ({
        inventory_id: tr.getAttribute('data-id'),
        result_status: tr.querySelector('.check-status')?.value || 'confirmed',
        quantity: tr.querySelector('.check-qty')?.value || null,
        note: tr.querySelector('.check-note')?.value || ''
      })).filter(r => r.inventory_id);
      if (!items.length) {
        window.crmUiAlert('No items to submit');
        if (window.toggleButtonLoading) window.toggleButtonLoading(submitCheckBtn, false);
        return;
      }
      api(cfg.apiChecks, {
        method: 'POST',
        body: JSON.stringify({ department_id: checkDept.value, items })
      })
        .then(({ok, data}) => {
          if (!ok) {
            window.crmUiAlert(data.error || 'Unable to submit check');
            return;
          }
          const modalEl = document.getElementById('inventoryCheckModal');
          bootstrap.Modal.getInstance(modalEl)?.hide();
          loadInventory();
        })
        .catch(() => {
          window.crmUiAlert('Unable to submit check');
        })
        .finally(() => {
          if (window.toggleButtonLoading) window.toggleButtonLoading(submitCheckBtn, false);
        });
    });
  }

  if (inventoryModalEl) {
    inventoryModalEl.addEventListener('hidden.bs.modal', () => {
      resetInventoryForm();
    });
  }

  loadSummary();
  loadInventory();
})();
