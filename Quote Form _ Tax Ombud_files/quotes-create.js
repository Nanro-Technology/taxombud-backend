/* eslint-disable */
(function () {
  'use strict';

  const cfg = window.quoteFormConfig || {};
  const canCreate = !!cfg.canCreate;
  const canUpdate = !!cfg.canUpdate;

  const api = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiIndex = api.quotesIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/quotes/index');
  const apiDetail = api.quotesDetail || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/quotes/detail');
  const apiFiles = api.filesIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/files/index');

  const alertBox = document.getElementById('quoteFormAlert');
  const heading = document.getElementById('quoteFormHeading');
  const quoteIdInput = document.getElementById('quoteId');

  const titleInput = document.getElementById('quoteTitle');
  const statusInput = document.getElementById('quoteStatus');
  const currencyInput = document.getElementById('quoteCurrency');
  const parentTypeSelect = document.getElementById('quoteParentType');
  const parentSelectLabel = document.getElementById('quoteParentSelectLabel');
  const parentSelect = document.getElementById('quoteParentId');
  const issuedDateInput = document.getElementById('quoteIssuedDate');
  const expiryDateInput = document.getElementById('quoteExpiryDate');
  const taxInput = document.getElementById('quoteTaxAmount');
  const discountInput = document.getElementById('quoteDiscountAmount');
  const notesEditorEl = document.getElementById('quoteNotesEditor');
  const notesInput = document.getElementById('quoteNotes');
  const filesInput = document.getElementById('quoteFilesInput');
  const filesList = document.getElementById('quoteFilesList');

  const addItemBtn = document.getElementById('quoteAddItemBtn');
  const itemsTbody = document.querySelector('#quoteItemsTable tbody');
  const subtotalEl = document.getElementById('quoteSubtotal');
  const taxEl = document.getElementById('quoteTax');
  const discountEl = document.getElementById('quoteDiscount');
  const totalEl = document.getElementById('quoteTotal');
  const saveBtn = document.getElementById('quoteSaveBtn');

  let linkedFileIds = [];
  let linkedFilesById = {};
  let lookupSeq = 0;
  let parentChoices = null;
  let notesEditor = null;
  const organizationsEnabledByModule = !(
    window.__mmkModulesEnabled &&
    Object.prototype.hasOwnProperty.call(window.__mmkModulesEnabled, 'organizations') &&
    !window.__mmkModulesEnabled.organizations
  );
  const organizationsEnabled = (cfg.organizationsEnabled !== false) && organizationsEnabledByModule;

  if (currencyInput && typeof populateCurrencySelect === 'function') {
    populateCurrencySelect(currencyInput, { defaultCode: 'NGN' });
  }

  const parentMeta = {
    account: {
      lookup: 'accounts',
      label: 'Account',
      selectPlaceholder: 'Select account',
      searchPlaceholder: 'Search account'
    },
    contact: {
      lookup: 'contacts',
      label: 'Contact',
      selectPlaceholder: 'Select contact',
      searchPlaceholder: 'Search contact'
    },
    organization: {
      lookup: 'organizations',
      label: 'Organization',
      selectPlaceholder: 'Select organization',
      searchPlaceholder: 'Search organization'
    }
  };

  if (!organizationsEnabled) {
    delete parentMeta.organization;
    if (parentTypeSelect) {
      const opt = parentTypeSelect.querySelector('option[value="organization"]');
      if (opt) opt.remove();
    }
  }

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
    alertBox.classList.toggle('d-none', !msg);
  }

  function setSaving(saving) {
    if (!saveBtn) return;
    const spinner = saveBtn.querySelector('.spinner-border');
    const text = saveBtn.querySelector('.btn-text');
    saveBtn.disabled = !!saving;
    if (spinner) spinner.classList.toggle('d-none', !saving);
    if (text) text.textContent = saving ? 'Saving...' : 'Save Quote';
  }

  function debounce(fn, wait) {
    let t = null;
    return function (...args) {
      if (t) clearTimeout(t);
      t = setTimeout(() => fn.apply(this, args), wait);
    };
  }

  function getParentType() {
    const fallback = Object.keys(parentMeta)[0] || 'account';
    const type = String((parentTypeSelect && parentTypeSelect.value) || fallback).toLowerCase();
    return parentMeta[type] ? type : fallback;
  }

  function getSelectedParentId() {
    // Choices can show a selected item while the native select value is stale.
    if (parentChoices && typeof parentChoices.getValue === 'function') {
      const selected = parentChoices.getValue(true);
      if (Array.isArray(selected)) {
        const first = selected.length ? Number(selected[0] || 0) : 0;
        if (first > 0) return first;
      } else {
        const val = Number(selected || 0);
        if (val > 0) return val;
      }
    }
    return Number((parentSelect && parentSelect.value) || 0);
  }

  function setParentUi(type) {
    const meta = parentMeta[type] || parentMeta.account;
    if (parentTypeSelect) parentTypeSelect.value = type;
    if (parentSelectLabel) parentSelectLabel.textContent = 'Select ' + meta.label;
  }

  function ensureParentChoices() {
    if (!parentSelect || typeof Choices === 'undefined' || parentChoices) return;
    parentChoices = new Choices(parentSelect, {
      searchEnabled: true,
      shouldSort: false,
      itemSelectText: '',
      allowHTML: false,
      placeholder: true,
      placeholderValue: 'Select parent'
    });
  }

  function upsertParentOption(value, label) {
    const strVal = String(value || '');
    if (!strVal || !parentSelect) return;
    let opt = parentSelect.querySelector(`option[value="${strVal}"]`);
    if (!opt) {
      opt = document.createElement('option');
      opt.value = strVal;
      parentSelect.appendChild(opt);
    }
    opt.textContent = label || ('#' + strVal);
  }

  function refreshParentChoices(rows, selectedValue, selectedLabel, emptyText) {
    if (!parentSelect) return;
    const selected = selectedValue ? String(selectedValue) : '';
    const list = Array.isArray(rows) ? rows : [];
    parentSelect.innerHTML = `<option value="">${emptyText}</option>`;
    list.forEach((row) => {
      const val = String(row.id || '').trim();
      if (!val) return;
      const opt = document.createElement('option');
      opt.value = val;
      opt.textContent = row.label || ('#' + row.id);
      parentSelect.appendChild(opt);
    });
    if (selected) {
      upsertParentOption(selected, selectedLabel);
      parentSelect.value = selected;
    } else {
      parentSelect.value = '';
    }
    if (typeof window.normalizeSelectOptions === 'function') {
      window.normalizeSelectOptions(parentSelect);
    }
    ensureParentChoices();
    if (parentChoices) {
      const selectedValue = String(parentSelect.value || '');
      const seen = new Set();
      const choices = [];
      Array.from(parentSelect.options).forEach((opt) => {
        const value = String(opt.value || '');
        if (value === '') return; // Let Choices render one placeholder, not a duplicate empty option
        const key = value === '' ? '__empty__' : value;
        if (seen.has(key)) return;
        seen.add(key);
        choices.push({
          value,
          label: opt.textContent || '',
          selected: value === selectedValue,
          disabled: !!opt.disabled
        });
      });
      if (typeof parentChoices.clearStore === 'function') {
        parentChoices.clearStore();
      } else {
        parentChoices.clearChoices();
      }
      parentChoices.setChoices(choices, 'value', 'label', true);
      if (parentSelect.value) {
        parentChoices.setChoiceByValue(String(parentSelect.value));
      }
    }
  }

  function setNotesValue(rawValue) {
    const value = String(rawValue || '');
    if (notesEditor) {
      const asHtml = value.indexOf('<') !== -1 && value.indexOf('>') !== -1;
      notesEditor.root.innerHTML = asHtml ? value : esc(value).replace(/\n/g, '<br>');
    }
    if (notesInput) notesInput.value = value;
  }

  function getNotesValue() {
    if (notesEditor) {
      const text = (notesEditor.getText ? notesEditor.getText() : notesEditor.root.innerText || '')
        .replace(/\s+$/g, '');
      if (notesInput) notesInput.value = text;
      return text;
    }
    return (notesInput && notesInput.value ? notesInput.value : '').trim();
  }

  function initNotesEditor() {
    if (!notesEditorEl || typeof Quill === 'undefined') return;
    notesEditor = new Quill(notesEditorEl, {
      theme: 'snow',
      modules: {
        toolbar: [
          ['bold', 'italic', 'underline'],
          [{ list: 'ordered' }, { list: 'bullet' }],
          ['link'],
          ['clean']
        ]
      }
    });
    notesEditor.on('text-change', function () {
      if (notesInput) {
        notesInput.value = getNotesValue();
      }
    });
    if (notesInput && notesInput.value) {
      setNotesValue(notesInput.value);
    }
  }

  function setParentSelection(type, id, label) {
    const parentType = parentMeta[type] ? type : 'account';
    setParentUi(parentType);
    const parentId = id ? String(id) : '';
    const optionLabel = label || (parentMeta[parentType].label + ' #' + parentId);
    const emptyText = parentMeta[parentType].selectPlaceholder;
    refreshParentChoices([], parentId, optionLabel, emptyText);
  }

  async function loadParentLookup(type, q) {
    if (!parentSelect) return;
    const meta = parentMeta[type] || parentMeta.account;
    const thisSeq = ++lookupSeq;
    const previousValue = String(parentSelect.value || '');
    const previousLabel = previousValue && parentSelect.selectedOptions && parentSelect.selectedOptions.length
      ? parentSelect.selectedOptions[0].textContent
      : '';

    const params = new URLSearchParams();
    params.set('lookup', meta.lookup);
    params.set('limit', '10');
    if (q) params.set('q', q);
    try {
      const res = await fetch(apiIndex + '?' + params.toString());
      const data = await res.json().catch(() => ({}));
      if (thisSeq !== lookupSeq) return;
      if (!res.ok) return;

      const rows = Array.isArray(data.data) ? data.data : [];
      const emptyText = meta.selectPlaceholder;
      refreshParentChoices(rows, previousValue, previousLabel || (meta.label + ' #' + previousValue), emptyText);
    } catch (_e) {
      // Lookup hydration is best-effort.
    }
  }

  function addItemRow(item) {
    if (!itemsTbody) return;
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><input type="text" class="form-control form-control-sm qi-name" placeholder="Item" value="${esc(item && item.item_name || '')}"></td>
      <td><input type="text" class="form-control form-control-sm qi-desc" placeholder="Description" value="${esc(item && item.description || '')}"></td>
      <td><input type="number" step="0.01" class="form-control form-control-sm qi-qty" value="${item && item.qty != null ? Number(item.qty) : 1}"></td>
      <td><input type="number" step="0.01" class="form-control form-control-sm qi-unit money-input" value="${item && item.unit_price != null ? Number(item.unit_price) : 0}"></td>
      <td><input type="text" class="form-control form-control-sm qi-line" value="0.00" readonly></td>
      <td class="text-center"><button type="button" class="btn btn-sm btn-soft-danger qi-remove"><i class="ri-close-line"></i></button></td>
    `;
    itemsTbody.appendChild(tr);
    recalcRow(tr);
  }

  function recalcRow(tr) {
    const qty = Number((tr.querySelector('.qi-qty') || {}).value || 0);
    const unit = Number(window.moneyVal(tr.querySelector('.qi-unit')) || 0);
    const line = Math.max(0, qty) * Math.max(0, unit);
    const lineInput = tr.querySelector('.qi-line');
    if (lineInput) lineInput.value = line.toFixed(2);
    recalcTotals();
  }

  function recalcTotals() {
    let subtotal = 0;
    Array.from(itemsTbody.querySelectorAll('tr')).forEach((tr) => {
      subtotal += Number((tr.querySelector('.qi-line') || {}).value || 0);
    });
    const tax = Number(window.moneyVal(taxInput) || 0);
    const discount = Number(window.moneyVal(discountInput) || 0);
    const total = Math.max(0, subtotal + Math.max(0, tax) - Math.max(0, discount));
    subtotalEl.textContent = subtotal.toFixed(2);
    taxEl.textContent = Math.max(0, tax).toFixed(2);
    discountEl.textContent = Math.max(0, discount).toFixed(2);
    totalEl.textContent = total.toFixed(2);
  }

  function collectItems() {
    const out = [];
    Array.from(itemsTbody.querySelectorAll('tr')).forEach((tr) => {
      const item_name = (tr.querySelector('.qi-name') || {}).value || '';
      const description = (tr.querySelector('.qi-desc') || {}).value || '';
      const qty = Number((tr.querySelector('.qi-qty') || {}).value || 0);
      const unit_price = Number(window.moneyVal(tr.querySelector('.qi-unit')) || 0);
      if (!item_name.trim() && !description.trim()) return;
      out.push({ item_name: item_name.trim(), description: description.trim(), qty, unit_price });
    });
    return out;
  }

  function renderFiles() {
    if (!filesList) return;
    if (!linkedFileIds.length) {
      filesList.innerHTML = 'No files linked.';
      return;
    }
    filesList.innerHTML = linkedFileIds.map((id) => {
      const f = linkedFilesById[id] || {};
      const name = f.file_name || ('File #' + id);
      return `<div class="d-flex justify-content-between align-items-center mb-1"><span>${esc(name)}</span><button type="button" class="btn btn-sm btn-light text-danger qf-remove" data-id="${id}"><i class="ri-close-line"></i></button></div>`;
    }).join('');
  }

  function resolveParentFromRow(row) {
    if (row.parent_type && row.parent_id) {
      return {
        type: String(row.parent_type).toLowerCase(),
        id: row.parent_id,
        label: row.parent_label || null
      };
    }
    if (row.account_id) return { type: 'account', id: row.account_id, label: row.account_name || null };
    if (row.contact_id) return { type: 'contact', id: row.contact_id, label: row.contact_name || null };
    if (organizationsEnabled && row.organization_id) return { type: 'organization', id: row.organization_id, label: row.organization_name || null };
    return { type: 'account', id: null, label: null };
  }

  async function uploadFiles() {
    if (!filesInput || !filesInput.files || !filesInput.files.length) return [];
    const ids = [];
    for (const file of Array.from(filesInput.files)) {
      const fd = new FormData();
      fd.append('file', file);
      const res = await fetch(apiFiles, { method: 'POST', body: fd });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || 'Unable to upload file');
      if (data.id) ids.push(Number(data.id));
    }
    return ids;
  }

  async function loadExisting(id) {
    const res = await fetch(apiDetail + '?id=' + encodeURIComponent(id));
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.error || 'Unable to load quote');
    const row = data.data || {};

    if (heading) heading.textContent = 'Edit Quote';
    titleInput.value = row.title || '';
    statusInput.value = row.status || 'draft';
    currencyInput.value = row.currency || 'NGN';
    issuedDateInput.value = row.issued_date || '';
    expiryDateInput.value = row.expiry_date || '';
    window.moneySet(taxInput, row.tax_amount || 0);
    window.moneySet(discountInput, row.discount_amount || 0);
    setNotesValue(row.notes || '');

    const resolvedParent = resolveParentFromRow(row);
    setParentSelection(
      resolvedParent.type,
      resolvedParent.id,
      resolvedParent.label || parentMeta[resolvedParent.type].label + ' #' + resolvedParent.id
    );
    loadParentLookup(resolvedParent.type, '').catch(() => {});

    itemsTbody.innerHTML = '';
    (row.items || []).forEach((it) => addItemRow(it));
    if (!(row.items || []).length) addItemRow({});

    linkedFileIds = [];
    linkedFilesById = {};
    (row.files || []).forEach((f) => {
      const fid = Number(f.file_id || f.id || 0);
      if (!fid) return;
      linkedFileIds.push(fid);
      linkedFilesById[fid] = f;
    });
    linkedFileIds = Array.from(new Set(linkedFileIds));
    renderFiles();
    recalcTotals();
  }

  async function saveQuote() {
    showAlert('');
    const id = (quoteIdInput.value || '').trim();
    if (!id && !canCreate) {
      showAlert('No permission to create quote.');
      return;
    }
    if (id && !canUpdate) {
      showAlert('No permission to update quote.');
      return;
    }

    const items = collectItems();
    if (!items.length) {
      showAlert('Add at least one item.');
      return;
    }

    const parentType = getParentType();
    const parentId = getSelectedParentId();
    if (!parentId) {
      showAlert('Select one parent (Account, Contact or Organization).');
      return;
    }

    const payload = {
      title: (titleInput.value || '').trim(),
      status: statusInput.value,
      currency: (currencyInput.value || 'NGN').trim().toUpperCase(),
      account_id: parentType === 'account' ? parentId : null,
      contact_id: parentType === 'contact' ? parentId : null,
      organization_id: parentType === 'organization' ? parentId : null,
      issued_date: issuedDateInput.value || null,
      expiry_date: expiryDateInput.value || null,
      tax_amount: Number(window.moneyVal(taxInput) || 0),
      discount_amount: Number(window.moneyVal(discountInput) || 0),
      notes: getNotesValue(),
      items
    };

    setSaving(true);
    try {
      const uploaded = await uploadFiles();
      payload.file_ids = Array.from(new Set([...(linkedFileIds || []), ...uploaded]));

      const method = id ? 'PATCH' : 'POST';
      const url = id ? (apiDetail + '?id=' + encodeURIComponent(id)) : apiIndex;
      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || 'Unable to save quote');

      const nextId = id || data.id;
      if (!nextId) {
        window.location.href = '/studio/quotes/index.kml';
        return;
      }
      window.location.href = '/studio/quotes/view.kml?id=' + encodeURIComponent(nextId);
    } catch (e) {
      showAlert(e.message || 'Unable to save quote');
    } finally {
      setSaving(false);
    }
  }

  document.addEventListener('click', function (e) {
    const rm = e.target.closest('.qf-remove');
    if (rm) {
      e.preventDefault();
      const id = Number(rm.getAttribute('data-id'));
      linkedFileIds = linkedFileIds.filter((x) => x !== id);
      delete linkedFilesById[id];
      renderFiles();
      return;
    }
    const delRow = e.target.closest('.qi-remove');
    if (delRow) {
      e.preventDefault();
      const tr = delRow.closest('tr');
      if (tr) tr.remove();
      if (!itemsTbody.querySelector('tr')) addItemRow({});
      recalcTotals();
    }
  });

  itemsTbody.addEventListener('input', function (e) {
    const tr = e.target.closest('tr');
    if (!tr) return;
    recalcRow(tr);
  });
  taxInput.addEventListener('input', recalcTotals);
  discountInput.addEventListener('input', recalcTotals);
  addItemBtn.addEventListener('click', function () { addItemRow({}); });
  saveBtn.addEventListener('click', saveQuote);

  parentTypeSelect.addEventListener('change', function () {
    const type = getParentType();
    setParentSelection(type, null, null);
    loadParentLookup(type, '').catch(() => {});
  });

  if (parentSelect) {
    parentSelect.addEventListener('search', debounce((evt) => {
      const type = getParentType();
      const q = evt && evt.detail && typeof evt.detail.value === 'string' ? evt.detail.value.trim() : '';
      loadParentLookup(type, q).catch(() => {});
    }, 250));
  }

  initNotesEditor();
  ensureParentChoices();
  const id = (quoteIdInput.value || '').trim();
  if (id) {
    loadExisting(id).catch((e) => showAlert(e.message || 'Unable to load quote'));
  } else {
    setParentSelection('account', null, null);
    loadParentLookup('account', '').catch(() => {});
    addItemRow({ qty: 1, unit_price: 0 });
    recalcTotals();
  }
})();
