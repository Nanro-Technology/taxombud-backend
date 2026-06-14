/* eslint-disable */
(function () {
  "use strict";

  const cfg = window.contractsConfig || {};
  const canCreate = !!cfg.canCreate;
  const canUpdate = !!cfg.canUpdate;
  const canDelete = !!cfg.canDelete;

  const api = (typeof apiEndpoints === "function") ? apiEndpoints() : {};
  const apiIndex = api.contractsIndex || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/contracts/index");
  const apiDetail = api.contractsDetail || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/contracts/detail");
  const apiFiles = api.filesIndex || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/files/index");
  const apiQuotesDetail = api.quotesDetail || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/quotes/detail");

  const pageSize = (window.appConfig && window.appConfig.dataTablePageSize) ? Number(window.appConfig.dataTablePageSize) : 250;

  const searchInput = document.getElementById("contractsSearch");
  const statusFilter = document.getElementById("contractsStatusFilter");
  const parentTypeFilter = document.getElementById("contractsParentTypeFilter");
  const parentFilterSearch = document.getElementById("contractsParentSearch");
  const parentFilter = document.getElementById("contractsParentIdFilter");
  const applyFilterBtn = document.getElementById("contractsApplyFilterBtn");
  const newBtn = document.getElementById("contractsNewBtn");
  const alertBox = document.getElementById("contractsAlert");

  const modalEl = document.getElementById("contractModal");
  const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
  const deleteModalEl = document.getElementById("contractDeleteModal");
  const deleteModal = deleteModalEl ? new bootstrap.Modal(deleteModalEl) : null;
  const modalTitle = document.getElementById("contractModalTitle");
  const modalAlert = document.getElementById("contractModalAlert");
  const saveBtn = document.getElementById("contractSaveBtn");
  const deleteBtn = document.getElementById("contractDeleteConfirmBtn");

  const contractIdInput = document.getElementById("contractId");
  const titleInput = document.getElementById("contractTitle");
  const statusInput = document.getElementById("contractStatus");
  const startDateInput = document.getElementById("contractStartDate");
  const endDateInput = document.getElementById("contractEndDate");
  const renewalDateInput = document.getElementById("contractRenewalDate");
  const reminderDaysSelect = document.getElementById("contractReminderDays");
  const notesInput = document.getElementById("contractNotes");
  const filesInput = document.getElementById("contractFilesInput");
  const filesExisting = document.getElementById("contractFilesExisting");

  const parentTypeInput = document.getElementById("contractParentType");
  const parentSearchLabel = document.getElementById("contractParentSearchLabel");
  const parentSelectLabel = document.getElementById("contractParentSelectLabel");
  const parentSearchInput = document.getElementById("contractParentSearch");
  const parentSelect = document.getElementById("contractParentId");
  const agentSelect = document.getElementById("contractAgentId");
  const quoteIdInput = document.getElementById("contractQuoteId");
  const quoteSearchInput = document.getElementById("contractQuoteSearch");
  const quoteSelect = document.getElementById("contractQuoteIdSelect");
  const clearQuoteBtn = document.getElementById("contractClearQuoteBtn");

  const query = new URLSearchParams(window.location.search || "");
  const openFromQuoteId = (query.get("from_quote") || "").trim();
  const openViewContractId = (query.get("view_contract") || "").trim();
  const organizationsEnabledByModule = !(
    window.__mmkModulesEnabled &&
    Object.prototype.hasOwnProperty.call(window.__mmkModulesEnabled, 'organizations') &&
    !window.__mmkModulesEnabled.organizations
  );
  const organizationsEnabled = (cfg.organizationsEnabled !== false) && organizationsEnabledByModule;

  let table = null;
  let pendingDeleteId = null;
  let currentFileIds = [];
  let currentFilesById = {};
  let filterLookupSeq = 0;
  let modalLookupSeq = 0;
  let agentChoices = null;
  let reminderChoices = null;
  let quoteLookupSeq = 0;
  const defaultReminderDays = ["30", "14", "7"];

  const parentMeta = {
    organization: { lookup: "organizations", allLabel: "All organizations", searchPlaceholder: "Search organization" },
    account: { lookup: "accounts", allLabel: "All accounts", searchPlaceholder: "Search account" },
    contact: { lookup: "contacts", allLabel: "All contacts", searchPlaceholder: "Search contact" }
  };

  const modalParentMeta = {
    organization: { lookup: "organizations", label: "Organization", selectPlaceholder: "Select organization", searchPlaceholder: "Search organization" },
    account: { lookup: "accounts", label: "Account", selectPlaceholder: "Select account", searchPlaceholder: "Search account" },
    contact: { lookup: "contacts", label: "Contact", selectPlaceholder: "Select contact", searchPlaceholder: "Search contact" }
  };

  if (!organizationsEnabled) {
    delete parentMeta.organization;
    delete modalParentMeta.organization;
    if (parentTypeFilter) {
      const o = parentTypeFilter.querySelector('option[value="organization"]');
      if (o) o.remove();
    }
    if (parentTypeInput) {
      const o2 = parentTypeInput.querySelector('option[value="organization"]');
      if (o2) o2.remove();
    }
  }

  function esc(val) {
    return String(val || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function showAlert(msg) {
    if (!alertBox) return;
    alertBox.textContent = msg || "";
  }

  function showModalAlert(msg) {
    if (!modalAlert) return;
    modalAlert.textContent = msg || "";
    modalAlert.classList.toggle("d-none", !msg);
  }

  function statusBadge(status) {
    const key = String(status || "").toLowerCase();
    const cls = {
      draft: "bg-secondary-subtle text-secondary",
      active: "bg-success-subtle text-success",
      expired: "bg-warning-subtle text-warning",
      cancelled: "bg-danger-subtle text-danger"
    }[key] || "bg-light text-muted";
    return `<span class="badge ${cls}">${esc(status || "-")}</span>`;
  }

  function fmtDate(v) {
    if (!v) return "-";
    return String(v).slice(0, 10);
  }

  function setSaving(btn, saving, textWhenIdle, textWhenBusy) {
    if (!btn) return;
    const spinner = btn.querySelector(".spinner-border");
    const txt = btn.querySelector(".btn-text");
    btn.disabled = !!saving;
    if (spinner) spinner.classList.toggle("d-none", !saving);
    if (txt) txt.textContent = saving ? textWhenBusy : textWhenIdle;
  }

  async function withModalLoading(taskFn) {
    const canShow = typeof window.showGlobalLoading === "function" && typeof window.hideGlobalLoading === "function";
    if (canShow) window.showGlobalLoading("Loading contract form...");
    try {
      await taskFn();
    } finally {
      if (canShow) window.hideGlobalLoading();
    }
  }

  function ensureAgentChoices() {
    if (!agentSelect || typeof Choices === "undefined" || agentChoices) return;
    agentChoices = new Choices(agentSelect, {
      searchEnabled: true,
      shouldSort: false,
      itemSelectText: "",
      allowHTML: false,
      placeholder: false
    });
  }

  function ensureReminderChoices() {
    if (!reminderDaysSelect || typeof Choices === "undefined" || reminderChoices) return;
    reminderChoices = new Choices(reminderDaysSelect, {
      searchEnabled: false,
      shouldSort: false,
      removeItemButton: true,
      itemSelectText: "",
      allowHTML: false,
      placeholder: true,
      placeholderValue: "Select reminder cycle"
    });
  }

  function setReminderDays(values) {
    if (!reminderDaysSelect) return;
    const allowed = new Set(["30", "14", "7", "3", "1"]);
    const next = Array.from(new Set((values || []).map((v) => String(v)).filter((v) => allowed.has(v))));
    const finalValues = next.length ? next : defaultReminderDays.slice();

    if (reminderChoices) {
      if (typeof reminderChoices.removeActiveItems === "function") {
        reminderChoices.removeActiveItems();
      }
      finalValues.forEach((v) => reminderChoices.setChoiceByValue(v));
      return;
    }

    Array.from(reminderDaysSelect.options || []).forEach((opt) => {
      opt.selected = finalValues.includes(String(opt.value));
    });
  }

  function getReminderDays() {
    if (!reminderDaysSelect) return defaultReminderDays.map((v) => parseInt(v, 10));
    const selected = Array.from(reminderDaysSelect.selectedOptions || [])
      .map((opt) => parseInt(String(opt.value || ""), 10))
      .filter((v) => Number.isFinite(v) && v > 0);
    if (!selected.length) return defaultReminderDays.map((v) => parseInt(v, 10));
    return Array.from(new Set(selected));
  }

  function refreshAgentChoices(selectedValue) {
    if (!agentSelect) return;
    const rebuilt = [];
    const seen = new Set();
    let emptyAdded = false;
    Array.from(agentSelect.options || []).forEach((opt) => {
      const value = String(opt.value || "");
      if (value === "") {
        if (emptyAdded) return;
        emptyAdded = true;
        rebuilt.push({ value: "", label: "Unassigned", disabled: !!opt.disabled });
        return;
      }
      if (seen.has(value)) return;
      seen.add(value);
      rebuilt.push({ value, label: opt.textContent || ("Agent #" + value), disabled: !!opt.disabled });
    });
    if (!emptyAdded) {
      rebuilt.unshift({ value: "", label: "Unassigned", disabled: false });
    }
    agentSelect.innerHTML = "";
    rebuilt.forEach((row) => {
      const opt = document.createElement("option");
      opt.value = row.value;
      opt.textContent = row.label;
      opt.disabled = !!row.disabled;
      agentSelect.appendChild(opt);
    });
    ensureAgentChoices();
    if (!agentChoices) return;
    const selected = String(selectedValue != null ? selectedValue : (agentSelect.value || ""));
    const options = [];
    const seenChoices = new Set();
    Array.from(agentSelect.options || []).forEach((opt) => {
      const value = String(opt.value || "");
      const key = value === "" ? "__empty__" : value;
      if (seenChoices.has(key)) return;
      seenChoices.add(key);
      options.push({
        value,
        label: opt.textContent || "",
        selected: value === selected,
        disabled: !!opt.disabled
      });
    });
    if (typeof agentChoices.clearChoices === "function") {
      agentChoices.clearChoices();
    }
    agentChoices.setChoices(options, "value", "label", true);
    if (typeof agentChoices.removeActiveItems === "function") {
      agentChoices.removeActiveItems();
    }
    if (selected !== "") {
      agentChoices.setChoiceByValue(selected);
    } else {
      agentChoices.setChoiceByValue("");
    }
  }

  function setSelectValue(selectEl, value, label, emptyLabel) {
    if (!selectEl) return;
    const val = value ? String(value) : "";
    if (!val) {
      selectEl.value = "";
      return;
    }
    let found = false;
    Array.from(selectEl.options || []).forEach((opt) => {
      if (opt.value === val) found = true;
    });
    if (!found) {
      const opt = document.createElement("option");
      opt.value = val;
      opt.textContent = label || (`#${val}`);
      selectEl.appendChild(opt);
    }
    if (emptyLabel && !selectEl.querySelector('option[value=""]')) {
      const emptyOpt = document.createElement("option");
      emptyOpt.value = "";
      emptyOpt.textContent = emptyLabel;
      selectEl.prepend(emptyOpt);
    }
    if (typeof window.normalizeSelectOptions === "function") {
      window.normalizeSelectOptions(selectEl);
    }
    selectEl.value = val;
    if (selectEl === agentSelect) {
      refreshAgentChoices(selectEl.value || "");
    }
  }

  function renderFiles() {
    if (!filesExisting) return;
    if (!currentFileIds.length) {
      filesExisting.innerHTML = "No attached files.";
      return;
    }
    filesExisting.innerHTML = currentFileIds.map((id) => {
      const file = currentFilesById[id] || {};
      const name = file.file_name || ("File #" + id);
      const href = "api/modules/files/download?id=" + encodeURIComponent(id);
      return `<div class="d-flex align-items-center justify-content-between mb-1">
        <a href="${href}" target="_blank"><i class="ri-attachment-line me-1"></i>${esc(name)}</a>
        <button type="button" class="btn btn-sm btn-light text-danger contract-file-remove" data-id="${id}"><i class="ri-close-line"></i></button>
      </div>`;
    }).join("");
  }

  function setQuoteLink(id, text) {
    const value = id ? String(id) : "";
    if (quoteIdInput) quoteIdInput.value = value;
    if (!quoteSelect) return;
    if (!value) {
      quoteSelect.value = "";
      return;
    }
    let opt = null;
    Array.from(quoteSelect.options || []).forEach((o) => {
      if (!opt && String(o.value || "") === value) opt = o;
    });
    if (!opt) {
      opt = document.createElement("option");
      opt.value = value;
      opt.textContent = text || ("Quote #" + value);
      quoteSelect.appendChild(opt);
    } else if (text) {
      opt.textContent = text;
    }
    quoteSelect.value = value;
  }

  async function loadQuoteLookup(q) {
    if (!quoteSelect) return;
    const thisSeq = ++quoteLookupSeq;
    const params = new URLSearchParams();
    params.set("lookup", "quotes");
    params.set("limit", "10");
    if (q) params.set("q", q);
    const selected = String((quoteIdInput && quoteIdInput.value) || quoteSelect.value || "");
    const selectedLabel = selected && quoteSelect.selectedOptions && quoteSelect.selectedOptions.length
      ? quoteSelect.selectedOptions[0].textContent
      : "";
    const res = await fetch(apiIndex + "?" + params.toString());
    const data = await res.json().catch(() => ({}));
    if (thisSeq !== quoteLookupSeq || !res.ok) return;
    quoteSelect.innerHTML = '<option value="">No source quote selected</option>';
    (data.data || []).forEach((row) => {
      const opt = document.createElement("option");
      opt.value = row.id;
      opt.textContent = row.label || ("Quote #" + row.id);
      quoteSelect.appendChild(opt);
    });
    if (selected) {
      let keep = null;
      Array.from(quoteSelect.options || []).forEach((o) => {
        if (!keep && String(o.value || "") === selected) keep = o;
      });
      if (!keep) {
        keep = document.createElement("option");
        keep.value = selected;
        keep.textContent = selectedLabel || ("Quote #" + selected);
        quoteSelect.appendChild(keep);
      }
      quoteSelect.value = selected;
    }
  }

  function uniqIds(list) {
    return Array.from(new Set((list || []).map((x) => parseInt(x, 10)).filter((n) => Number.isFinite(n) && n > 0)));
  }

  async function loadLookup(type, q, targetSelect, emptyText) {
    if (!targetSelect) return;
    const params = new URLSearchParams();
    params.set("lookup", type);
    params.set("limit", "10");
    if (q) params.set("q", q);
    const res = await fetch(apiIndex + "?" + params.toString());
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.error || "Lookup failed");
    const oldVal = targetSelect.value;
    targetSelect.innerHTML = `<option value="">${emptyText}</option>`;
    (data.data || []).forEach((row) => {
      const opt = document.createElement("option");
      opt.value = row.id;
      opt.textContent = row.label || ("#" + row.id);
      targetSelect.appendChild(opt);
    });
    if (oldVal && targetSelect.querySelector(`option[value="${oldVal}"]`)) {
      targetSelect.value = oldVal;
    }
    if (typeof window.normalizeSelectOptions === "function") {
      window.normalizeSelectOptions(targetSelect);
    }
    if (targetSelect === agentSelect) {
      refreshAgentChoices(targetSelect.value || "");
    }
  }

  function getParentType() {
    const fallback = Object.keys(parentMeta)[0] || "account";
    const type = String((parentTypeFilter && parentTypeFilter.value) || fallback).toLowerCase();
    return parentMeta[type] ? type : fallback;
  }

  function buildParentFilterParams() {
    const selectedId = Number((parentFilter && parentFilter.value) || 0);
    if (!selectedId) {
      return { account_id: "", contact_id: "", organization_id: "" };
    }
    const type = getParentType();
    return {
      account_id: type === "account" ? String(selectedId) : "",
      contact_id: type === "contact" ? String(selectedId) : "",
      organization_id: type === "organization" ? String(selectedId) : ""
    };
  }

  function setParentFilterUi(type, preserveValue) {
    const meta = parentMeta[type] || parentMeta.organization;
    const oldVal = preserveValue && parentFilter ? String(parentFilter.value || "") : "";
    if (parentFilterSearch) parentFilterSearch.placeholder = meta.searchPlaceholder;
    if (!parentFilter) return;
    parentFilter.innerHTML = `<option value="">${meta.allLabel}</option>`;
    if (oldVal) {
      const opt = document.createElement("option");
      opt.value = oldVal;
      opt.textContent = "#" + oldVal;
      parentFilter.appendChild(opt);
      parentFilter.value = oldVal;
    }
  }

  function getModalParentType() {
    const fallback = Object.keys(modalParentMeta)[0] || "account";
    const type = String((parentTypeInput && parentTypeInput.value) || fallback).toLowerCase();
    return modalParentMeta[type] ? type : fallback;
  }

  function setModalParentUi(type) {
    const meta = modalParentMeta[type] || modalParentMeta.organization;
    if (parentTypeInput) parentTypeInput.value = type;
    if (parentSearchLabel) parentSearchLabel.textContent = "Search " + meta.label;
    if (parentSelectLabel) parentSelectLabel.textContent = "Select " + meta.label;
    if (parentSearchInput) parentSearchInput.placeholder = meta.searchPlaceholder;
    if (parentSelect && !parentSelect.value) {
      parentSelect.innerHTML = `<option value="">${meta.selectPlaceholder}</option>`;
    } else if (parentSelect && parentSelect.options.length && parentSelect.options[0].value === "") {
      parentSelect.options[0].textContent = meta.selectPlaceholder;
    }
  }

  function setModalParentSelection(type, id, label) {
    const fallback = Object.keys(modalParentMeta)[0] || "account";
    const parentType = modalParentMeta[type] ? type : fallback;
    setModalParentUi(parentType);
    if (!parentSelect) return;
    const parentId = id ? String(id) : "";
    const optionLabel = label || (modalParentMeta[parentType].label + " #" + parentId);
    parentSelect.innerHTML = `<option value="">${modalParentMeta[parentType].selectPlaceholder}</option>`;
    if (parentId) {
      const opt = document.createElement("option");
      opt.value = parentId;
      opt.textContent = optionLabel;
      parentSelect.appendChild(opt);
      parentSelect.value = parentId;
    } else {
      parentSelect.value = "";
    }
  }

  async function loadModalParentLookup(type, q) {
    if (!parentSelect) return;
    const meta = modalParentMeta[type] || modalParentMeta.organization;
    const thisSeq = ++modalLookupSeq;
    const previousValue = String(parentSelect.value || "");
    const previousLabel = previousValue && parentSelect.selectedOptions && parentSelect.selectedOptions.length
      ? parentSelect.selectedOptions[0].textContent
      : "";
    try {
      await loadLookup(meta.lookup, q, parentSelect, meta.selectPlaceholder);
      if (thisSeq !== modalLookupSeq) return;
      if (previousValue) {
        if (!parentSelect.querySelector(`option[value="${previousValue}"]`)) {
          const opt = document.createElement("option");
          opt.value = previousValue;
          opt.textContent = previousLabel || ("#" + previousValue);
          parentSelect.appendChild(opt);
        }
        parentSelect.value = previousValue;
      }
    } catch (_e) {
      // best effort
    }
  }

  async function loadParentFilterLookup(q) {
    if (!parentFilter) return;
    const type = getParentType();
    const meta = parentMeta[type] || parentMeta.organization;
    const thisSeq = ++filterLookupSeq;
    const previousValue = String(parentFilter.value || "");
    const previousLabel = previousValue && parentFilter.selectedOptions && parentFilter.selectedOptions.length
      ? parentFilter.selectedOptions[0].textContent
      : "";
    try {
      await loadLookup(meta.lookup, q, parentFilter, meta.allLabel);
      if (thisSeq !== filterLookupSeq) return;
      if (previousValue) {
        if (!parentFilter.querySelector(`option[value="${previousValue}"]`)) {
          const opt = document.createElement("option");
          opt.value = previousValue;
          opt.textContent = previousLabel || ("#" + previousValue);
          parentFilter.appendChild(opt);
        }
        parentFilter.value = previousValue;
      }
    } catch (_e) {
      // best effort
    }
  }

  function debounce(fn, wait) {
    let timer = null;
    return function (...args) {
      if (timer) clearTimeout(timer);
      timer = setTimeout(() => fn.apply(this, args), wait);
    };
  }

  function initTable() {
    table = window.jQuery("#contractsTable").DataTable({
      processing: true,
      serverSide: true,
      searching: false,
      pageLength: pageSize,
      lengthMenu: [[pageSize], [pageSize]],
      ajax: {
        url: apiIndex,
        type: "GET",
        dataSrc: "data",
        data: function (d) {
          d.q = (searchInput && searchInput.value || "").trim();
          d.status = statusFilter ? statusFilter.value : "";
          const parentParams = buildParentFilterParams();
          d.account_id = parentParams.account_id;
          d.contact_id = parentParams.contact_id;
          d.organization_id = parentParams.organization_id;
          d.limit = d.length || pageSize;
        }
      },
      columns: [
        {
          data: null,
          render: (row, type, full, meta) => meta.row + 1 + meta.settings._iDisplayStart
        },
        {
          data: null,
          render: (row) => {
            const id = row.id;
            const title = row.title || ("Contract #" + id);
            const subtitle = row.notes ? `<div class="small text-muted text-truncate" style="max-width: 280px;">${esc(row.notes)}</div>` : "";
            return `<div class="fw-semibold">${esc(title)}</div>${subtitle}`;
          }
        },
        {
          data: null,
          render: (row) => {
            if (row.quote_id && row.quote_id_s) {
              const label = row.quote_number || row.quote_title || ("Quote #" + row.quote_id);
              return `<a href="studio/quotes/view.kml?id=${encodeURIComponent(row.quote_id_s)}">${esc(label)}</a>`;
            }
            return '<span class="text-muted">-</span>';
          }
        },
        { data: "account_name", render: (v) => esc(v || "-") },
        { data: "contact_name", render: (v) => esc((v || "").trim() || "-") },
        { data: "status", render: (v) => statusBadge(v) },
        {
          data: null,
          render: (row) => {
            const renewal = fmtDate(row.renewal_date);
            const start = fmtDate(row.start_date);
            const end = fmtDate(row.end_date);
            return `<div>${renewal}</div><div class="small text-muted">${start} → ${end}</div>`;
          }
        },
        { data: "assigned_agent_name", render: (v) => esc(v || "-") },
        {
          data: null,
          orderable: false,
          className: "text-end",
          render: (row) => {
            const editBtn = canUpdate ? `<button class="btn btn-sm btn-outline-primary contract-edit me-1" data-id="${row.id}"><i class="ri-edit-line me-1"></i>Edit</button>` : "";
            const invoiceBtn = row.id_s ? `<a class="btn btn-sm btn-outline-primary me-1" href="studio/invoices/invoices-create.kml?contract_id=${encodeURIComponent(row.id_s)}"><i class="ri-file-list-3-line me-1"></i>Invoice</a>` : "";
            const delBtn = canDelete ? `<button class="btn btn-sm btn-soft-danger contract-delete" data-id="${row.id}"><i class="ri-delete-bin-6-line me-1"></i>Delete</button>` : "";
            return editBtn + invoiceBtn + delBtn;
          }
        }
      ]
    });
  }

  function resetModal() {
    showModalAlert("");
    if (contractIdInput) contractIdInput.value = "";
    if (titleInput) titleInput.value = "";
    if (statusInput) statusInput.value = "draft";
    if (startDateInput) startDateInput.value = "";
    if (endDateInput) endDateInput.value = "";
    if (renewalDateInput) renewalDateInput.value = "";
    setReminderDays(defaultReminderDays);
    if (notesInput) notesInput.value = "";
    if (filesInput) filesInput.value = "";
    if (parentSearchInput) parentSearchInput.value = "";
    if (parentSelect) parentSelect.value = "";
    if (agentSelect) {
      agentSelect.value = "";
      refreshAgentChoices("");
    }
    setQuoteLink("", "");
    if (quoteSearchInput) quoteSearchInput.value = "";
    currentFileIds = [];
    currentFilesById = {};
    renderFiles();
  }

  async function applyQuotePrefill(quoteId) {
    if (!quoteId) return;
    const res = await fetch(apiQuotesDetail + "?id=" + encodeURIComponent(quoteId));
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.error || "Unable to load source quote");
    const row = data.data || {};
    if (String(row.status || "").toLowerCase() !== "approved") {
      throw new Error("Quote must be approved before contract creation");
    }
    setQuoteLink(row.id || "", (row.quote_number || ("Quote #" + row.id)) + (row.title ? (" - " + row.title) : ""));
    if (titleInput && !titleInput.value) titleInput.value = row.title || "";
    if (startDateInput && !startDateInput.value) startDateInput.value = row.issued_date || "";
    if (renewalDateInput && !renewalDateInput.value) renewalDateInput.value = row.expiry_date || "";
    if (notesInput && !notesInput.value) notesInput.value = row.notes || "";

    let parentType = organizationsEnabled ? "organization" : "account";
    let parentId = organizationsEnabled ? (row.organization_id || "") : "";
    let parentLabel = organizationsEnabled ? (row.organization_name || "") : "";
    if (!parentId && row.account_id) {
      parentType = "account";
      parentId = row.account_id;
      parentLabel = row.account_name || "";
    }
    if (!parentId && row.contact_id) {
      parentType = "contact";
      parentId = row.contact_id;
      parentLabel = (row.contact_name || "").trim();
    }
    setModalParentSelection(parentType, parentId, parentLabel);
    await loadModalParentLookup(parentType, "");
    if (parentId) {
      setSelectValue(parentSelect, parentId, parentLabel || ("#" + parentId), modalParentMeta[parentType].selectPlaceholder);
    }
  }

  async function applyQuoteSelectionFromDropdown(quoteId) {
    if (!quoteId) return;
    const res = await fetch(apiQuotesDetail + "?id=" + encodeURIComponent(quoteId));
    const data = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(data.error || "Unable to load selected quote");
    const row = data.data || {};
    if (String(row.status || "").toLowerCase() !== "approved") {
      throw new Error("Quote must be approved before contract creation");
    }

    setQuoteLink(row.id || quoteId, (row.quote_number || ("Quote #" + row.id)) + (row.title ? (" - " + row.title) : ""));

    // Auto-link contract parent to quote parent.
    let parentType = organizationsEnabled ? "organization" : "account";
    let parentId = organizationsEnabled ? (row.organization_id || "") : "";
    let parentLabel = organizationsEnabled ? (row.organization_name || "") : "";
    if (!parentId && row.account_id) {
      parentType = "account";
      parentId = row.account_id;
      parentLabel = row.account_name || "";
    }
    if (!parentId && row.contact_id) {
      parentType = "contact";
      parentId = row.contact_id;
      parentLabel = (row.contact_name || "").trim();
    }
    setModalParentSelection(parentType, parentId, parentLabel);
    await loadModalParentLookup(parentType, "");
    if (parentId) {
      setSelectValue(parentSelect, parentId, parentLabel || ("#" + parentId), modalParentMeta[parentType].selectPlaceholder);
    }

    // Helpful prefill where empty.
    if (titleInput && !titleInput.value) titleInput.value = row.title || "";
    if (startDateInput && !startDateInput.value) startDateInput.value = row.issued_date || "";
    if (renewalDateInput && !renewalDateInput.value) renewalDateInput.value = row.expiry_date || "";
    if (notesInput && !notesInput.value) notesInput.value = row.notes || "";
  }

  async function openNew(fromQuoteId) {
    if (!modal) return;
    await withModalLoading(async () => {
      resetModal();
      if (modalTitle) modalTitle.textContent = "New Contract";
      const type = getModalParentType();
      setModalParentSelection(type, null, null);
      await Promise.all([
        loadModalParentLookup(type, ""),
        loadLookup("agents", "", agentSelect, "Unassigned"),
        loadQuoteLookup("")
      ]).catch(() => {});
      if (fromQuoteId) {
        await applyQuotePrefill(fromQuoteId);
      }
      modal.show();
    });
  }

  async function openEdit(id) {
    if (!modal) return;
    await withModalLoading(async () => {
      resetModal();
      if (modalTitle) modalTitle.textContent = "Edit Contract";
      const res = await fetch(apiDetail + "?id=" + encodeURIComponent(id));
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || "Unable to load contract");
      const row = data.data || {};

      if (contractIdInput) contractIdInput.value = row.id || "";
      if (titleInput) titleInput.value = row.title || "";
      if (statusInput) statusInput.value = row.status || "draft";
      if (startDateInput) startDateInput.value = row.start_date || "";
      if (endDateInput) endDateInput.value = row.end_date || "";
      if (renewalDateInput) renewalDateInput.value = row.renewal_date || "";
      setReminderDays(Array.isArray(row.reminder_days) ? row.reminder_days : defaultReminderDays);
      if (notesInput) notesInput.value = row.notes || "";
      setQuoteLink(
        row.quote_id || "",
        row.quote_id ? ((row.quote_number || ("Quote #" + row.quote_id)) + (row.quote_title ? (" - " + row.quote_title) : "")) : ""
      );

      const parentType = (organizationsEnabled && row.organization_id) ? "organization" : (row.account_id ? "account" : "contact");
      const parentId = (organizationsEnabled ? row.organization_id : null) || row.account_id || row.contact_id || "";
      const parentLabel = (organizationsEnabled ? row.organization_name : null) || row.account_name || (row.contact_name || "").trim();
      setModalParentSelection(parentType, parentId, parentLabel || (parentId ? ("#" + parentId) : ""));

      await Promise.all([
        loadModalParentLookup(parentType, ""),
        loadLookup("agents", "", agentSelect, "Unassigned"),
        loadQuoteLookup("")
      ]).catch(() => {});

      setSelectValue(agentSelect, row.assigned_agent_id, row.assigned_agent_name || ("Agent #" + row.assigned_agent_id), "Unassigned");
      if (quoteSelect) {
        setQuoteLink(
          row.quote_id || "",
          row.quote_id ? ((row.quote_number || ("Quote #" + row.quote_id)) + (row.quote_title ? (" - " + row.quote_title) : "")) : ""
        );
      }

      currentFileIds = [];
      currentFilesById = {};
      (row.files || []).forEach((f) => {
        const fid = parseInt(f.file_id || f.id, 10);
        if (!fid) return;
        currentFileIds.push(fid);
        currentFilesById[fid] = f;
      });
      currentFileIds = uniqIds(currentFileIds);
      renderFiles();
      const reviewRoot = document.querySelector('[data-commercial-review-root][data-mode="document"][data-entity-type="contract"]');
      if (reviewRoot && reviewRoot.__commercialReviewWidget) {
        reviewRoot.__commercialReviewWidget.refresh();
      }

      modal.show();
    });
  }

  async function uploadNewFiles() {
    if (!filesInput || !filesInput.files || !filesInput.files.length) return [];
    const uploaded = [];
    for (const f of Array.from(filesInput.files)) {
      const fd = new FormData();
      fd.append("file", f);
      const res = await fetch(apiFiles, { method: "POST", body: fd });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) {
        throw new Error(data.error || data.message || "Unable to upload file");
      }
      if (data.id) uploaded.push(parseInt(data.id, 10));
    }
    return uniqIds(uploaded);
  }

  async function saveContract() {
    showModalAlert("");
    const id = contractIdInput ? contractIdInput.value : "";
    const parentType = getModalParentType();
    const parentId = parentSelect && parentSelect.value ? String(parentSelect.value) : "";
    const payload = {
      quote_id: quoteIdInput && quoteIdInput.value ? quoteIdInput.value : null,
      title: (titleInput && titleInput.value || "").trim(),
      status: (statusInput && statusInput.value || "draft").trim(),
      account_id: parentType === "account" && parentId ? parentId : null,
      contact_id: parentType === "contact" && parentId ? parentId : null,
      organization_id: parentType === "organization" && parentId ? parentId : null,
      start_date: (startDateInput && startDateInput.value || "").trim(),
      end_date: (endDateInput && endDateInput.value || "").trim(),
      renewal_date: (renewalDateInput && renewalDateInput.value || "").trim(),
      reminder_days: getReminderDays(),
      assigned_agent_id: agentSelect && agentSelect.value ? agentSelect.value : null,
      notes: (notesInput && notesInput.value || "").trim()
    };
    if (!payload.account_id && !payload.contact_id && !payload.organization_id) {
      showModalAlert("Select organization, account, or contact.");
      return;
    }

    setSaving(saveBtn, true, "Save Contract", "Saving...");
    try {
      const uploadedIds = await uploadNewFiles();
      payload.file_ids = uniqIds([...(currentFileIds || []), ...uploadedIds]);

      const method = id ? "PATCH" : "POST";
      const url = id ? (apiDetail + "?id=" + encodeURIComponent(id)) : apiIndex;
      const res = await fetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || "Unable to save contract");
      if (data && contractIdInput) {
        const savedId = Number(data.id_num || data.id || 0);
        if (savedId > 0) {
          contractIdInput.value = String(savedId);
        }
      }
      const reviewRoot = document.querySelector('[data-commercial-review-root][data-mode="document"][data-entity-type="contract"]');
      if (reviewRoot && reviewRoot.__commercialReviewWidget) {
        reviewRoot.__commercialReviewWidget.refresh();
      }
      if (modal) modal.hide();
      if (table) table.ajax.reload(null, false);
    } catch (err) {
      showModalAlert(err.message || "Unable to save contract");
    } finally {
      setSaving(saveBtn, false, "Save Contract", "Saving...");
    }
  }

  async function doDelete() {
    if (!pendingDeleteId) return;
    setSaving(deleteBtn, true, "Delete", "Deleting...");
    try {
      const res = await fetch(apiDetail + "?id=" + encodeURIComponent(pendingDeleteId), { method: "DELETE" });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(data.error || "Unable to delete contract");
      if (deleteModal) deleteModal.hide();
      pendingDeleteId = null;
      if (table) table.ajax.reload(null, false);
    } catch (err) {
      showAlert(err.message || "Unable to delete contract");
    } finally {
      setSaving(deleteBtn, false, "Delete", "Deleting...");
    }
  }

  function wireLookups() {
    const debouncedFilterParent = debounce(() => {
      loadParentFilterLookup((parentFilterSearch && parentFilterSearch.value || "").trim()).catch(() => {});
    }, 300);
    const debouncedModalParent = debounce(() => {
      const type = getModalParentType();
      loadModalParentLookup(type, (parentSearchInput && parentSearchInput.value || "").trim()).catch(() => {});
    }, 300);
    const debouncedModalAgent = debounce((q) => {
      loadLookup("agents", (q || "").trim(), agentSelect, "Unassigned").catch(() => {});
    }, 300);
    const debouncedQuote = debounce(() => {
      loadQuoteLookup((quoteSearchInput && quoteSearchInput.value || "").trim()).catch(() => {});
    }, 300);

    if (parentTypeFilter) {
      parentTypeFilter.addEventListener("change", () => {
        const type = getParentType();
        setParentFilterUi(type, false);
        if (parentFilterSearch) parentFilterSearch.value = "";
        loadParentFilterLookup("").catch(() => {});
      });
    }
    if (parentTypeInput) {
      parentTypeInput.addEventListener("change", () => {
        const type = getModalParentType();
        setModalParentSelection(type, null, null);
        if (parentSearchInput) parentSearchInput.value = "";
        loadModalParentLookup(type, "").catch(() => {});
      });
    }
    if (parentFilterSearch) parentFilterSearch.addEventListener("input", debouncedFilterParent);
    if (parentSearchInput) parentSearchInput.addEventListener("input", debouncedModalParent);
    if (quoteSearchInput) quoteSearchInput.addEventListener("input", debouncedQuote);
    if (quoteSelect) {
      quoteSelect.addEventListener("change", async () => {
        const selected = quoteSelect.value || "";
        if (quoteIdInput) quoteIdInput.value = selected;
        if (!selected) return;
        try {
          await applyQuoteSelectionFromDropdown(selected);
        } catch (err) {
          showModalAlert(err.message || "Unable to apply quote details");
        }
      });
    }
    if (agentSelect) {
      agentSelect.addEventListener("search", (evt) => {
        const q = evt && evt.detail && typeof evt.detail.value === "string" ? evt.detail.value : "";
        debouncedModalAgent(q);
      });
    }

    setParentFilterUi(getParentType(), false);
    loadParentFilterLookup("").catch(() => {});
    setModalParentUi(getModalParentType());
    ensureAgentChoices();
    ensureReminderChoices();
    setReminderDays(defaultReminderDays);
    refreshAgentChoices(agentSelect ? agentSelect.value : "");
  }

  function wireEvents() {
    if (applyFilterBtn) {
      applyFilterBtn.addEventListener("click", () => table && table.ajax.reload());
    }
    if (searchInput) {
      searchInput.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
          e.preventDefault();
          table && table.ajax.reload();
        }
      });
    }
    if (newBtn && canCreate) {
      newBtn.addEventListener("click", () => openNew().catch((err) => showAlert(err.message || "Unable to open contract form")));
    }
    if (saveBtn) {
      saveBtn.addEventListener("click", saveContract);
    }
    if (deleteBtn) {
      deleteBtn.addEventListener("click", doDelete);
    }
    if (clearQuoteBtn) {
      clearQuoteBtn.addEventListener("click", () => {
        setQuoteLink("", "");
        if (quoteSearchInput) quoteSearchInput.value = "";
        loadQuoteLookup("").catch(() => {});
      });
    }
    document.addEventListener("click", (e) => {
      const edit = e.target.closest(".contract-edit");
      if (edit) {
        const id = edit.getAttribute("data-id");
        if (!id) return;
        openEdit(id).catch((err) => showAlert(err.message || "Unable to load contract"));
        return;
      }
      const del = e.target.closest(".contract-delete");
      if (del) {
        pendingDeleteId = del.getAttribute("data-id");
        if (deleteModal) deleteModal.show();
        return;
      }
      const rm = e.target.closest(".contract-file-remove");
      if (rm) {
        const id = parseInt(rm.getAttribute("data-id"), 10);
        if (!id) return;
        currentFileIds = currentFileIds.filter((x) => x !== id);
        renderFiles();
      }
    });
  }

  initTable();
  wireLookups();
  wireEvents();
  if (openFromQuoteId && canCreate) {
    openNew(openFromQuoteId).catch((err) => showAlert(err.message || "Unable to open contract form"));
  } else if (openViewContractId && canUpdate) {
    openEdit(openViewContractId).catch((err) => showAlert(err.message || "Unable to load contract"));
  }
})();
