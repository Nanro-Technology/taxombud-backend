/* eslint-disable */
(function () {
  "use strict";

  const accountHealthEnabled = !!window.accountHealthEnabled;
  const moduleLabel =
    typeof window.moduleLabel === "function"
      ? window.moduleLabel
      : (key, form) => {
          const defaults = {
            account: { singular: "Account", plural: "Accounts" },
            contact: { singular: "Contact", plural: "Contacts" },
            organization: { singular: "Organization", plural: "Organizations" },
            case: { singular: "Case", plural: "Cases" },
          };
          const k = String(key || "").toLowerCase();
          const f = String(form || "plural").toLowerCase() === "singular" ? "singular" : "plural";
          return (defaults[k] && defaults[k][f]) || k;
        };
  let accountTable = null;

  const offcanvasEl = document.getElementById("accountOffcanvas");
  const offcanvas = offcanvasEl ? new bootstrap.Offcanvas(offcanvasEl) : null;
  const form = document.getElementById("accountForm");
  const formAlert = document.getElementById("accountFormAlert");
  const pageAlert = document.getElementById("accountAlert");
  const apiBaseList =
    (typeof url_root !== "undefined" ? url_root : "../") + "api/modules/accounts/index";
  const apiBaseDetail =
    (typeof url_root !== "undefined" ? url_root : "../") + "api/modules/accounts/details";
  const phoneCodeSelect = document.getElementById("account_phone_code");
  const phoneLocalInput = document.getElementById("account_phone");
  const phoneFullInput = document.getElementById("account_phone_full");
  const phoneAltCodeSelect = document.getElementById("account_phone_alt_code");
  const phoneAltLocalInput = document.getElementById("account_phone_alt");
  const phoneAltFullInput = document.getElementById("account_phone_alt_full");
  const countrySelect = document.getElementById("account_country");
  const countryNameInput = document.getElementById("account_country_name");
  const stateSelect = document.getElementById("account_state");
  const stateNameInput = document.getElementById("account_state_name");
  const citySelect = document.getElementById("account_city");
  const cityNameInput = document.getElementById("account_city_name");
  const searchInput = document.getElementById("accountSearch");
  const searchBtn = document.getElementById("accountSearchBtn");
  const dateRangeInput = document.getElementById("accountDateRange");
  const datePicker =
    dateRangeInput && typeof window.flatpickr === "function"
      ? window.flatpickr(dateRangeInput, { mode: "range", dateFormat: "Y-m-d", allowInput: true })
      : null;

  const countriesJson = "api/modules/geo/index?action=countries";
  const apiIndustries =
    (typeof url_root !== "undefined" ? url_root : "../") + "api/modules/config/industries";
  const phoneHelper = window.PhoneHelper
    ? window.PhoneHelper.bind({
        countriesUrl: countriesJson,
        codeSelect: phoneCodeSelect,
        localInput: phoneLocalInput,
        fullInput: phoneFullInput,
        countrySelect: null,
        defaultDial: "+234",
        defaultCountry: "NG",
      })
    : null;
  const phoneAltHelper = window.PhoneHelper
    ? window.PhoneHelper.bind({
        countriesUrl: countriesJson,
        codeSelect: phoneAltCodeSelect,
        localInput: phoneAltLocalInput,
        fullInput: phoneAltFullInput,
        countrySelect: null,
        defaultDial: "+234",
        defaultCountry: "NG",
      })
    : null;
  const geoHelper = window.GeoHelper
    ? window.GeoHelper.bind({
        countrySelect: countrySelect,
        stateSelect: stateSelect,
        citySelect: citySelect,
        countryNameInput: countryNameInput,
        stateNameInput: stateNameInput,
        cityNameInput: cityNameInput,
        apiCountries: (typeof apiEndpoints === "function" ? apiEndpoints().geoCountries : ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/geo/index?action=countries")),
        apiStates: (typeof apiEndpoints === "function" ? apiEndpoints().geoStates : ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/geo/index?action=states")),
        apiCities: (typeof apiEndpoints === "function" ? apiEndpoints().geoCities : ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/geo/index?action=cities")),
        defaultCountryCode: "NG",
        onCountryMeta: (meta) => {
          if (!meta || !phoneCodeSelect) return;
          const phonecode = String(meta.phonecode || "").trim();
          if (!phonecode) return;
          const dial = "+" + phonecode;
          const ensureDialOption = (selectEl) => {
            if (!selectEl) return;
            if (!Array.from(selectEl.options || []).some((opt) => opt.value === dial)) {
              const opt = document.createElement('option');
              opt.value = dial;
              opt.textContent = dial;
              selectEl.appendChild(opt);
            }
            selectEl.value = dial;
            selectEl.dispatchEvent(new Event('change', { bubbles: true }));
          };
          ensureDialOption(phoneCodeSelect);
          ensureDialOption(phoneAltCodeSelect);
        },
      })
    : null;

  function populateCountries() {
    const waits = [];
    if (phoneHelper) waits.push(phoneHelper.ready);
    if (phoneAltHelper) waits.push(phoneAltHelper.ready);
    if (geoHelper) waits.push(geoHelper.init({ country: "Nigeria" }));
    return waits.length ? Promise.all(waits) : Promise.resolve();
  }

  function populateIndustries(selected) {
    fetch(apiIndustries)
      .then((r) => {
        if (!r.ok) throw new Error();
        return r.json();
      })
      .then((data) => {
        const select = document.getElementById("account_industry");
        if (!select) return;
        select.innerHTML = '<option value="">Select industry</option>';
        (data.data || []).forEach((ind) => {
          const opt = document.createElement("option");
          opt.value = ind.code || ind.label || "";
          opt.textContent = ind.label || ind.code || "";
          if (selected && selected === opt.value) opt.selected = true;
          select.appendChild(opt);
        });
      })
      .catch(() => {});
  }

  function escapeHtml(str) {
    return String(str || "").replace(/[&<>"']/g, (m) => ({
      "&": "&amp;",
      "<": "&lt;",
      ">": "&gt;",
      '"': "&quot;",
      "'": "&#39;",
    }[m]));
  }

  const AVATAR_COLORS = ['#4f46e5','#0891b2','#059669','#d97706','#dc2626','#7c3aed','#db2777','#0284c7','#16a34a','#ca8a04'];
  function avatarColor(name) {
    let h = 0;
    for (let i = 0; i < (name||'').length; i++) h = ((h << 5) - h) + name.charCodeAt(i);
    return AVATAR_COLORS[Math.abs(h) % AVATAR_COLORS.length];
  }
  function avatarInitials(name) {
    const parts = (name||'').trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return '?';
    return (parts[0][0] + (parts[1] ? parts[1][0] : '')).toUpperCase();
  }
  function avatarHtml(name) {
    return `<div class="entity-avatar" style="background:${avatarColor(name)}">${avatarInitials(name)}</div>`;
  }

  function buildCallHtml(phone, opts = {}) {
    if (!phone) return "N/A";
    const href =
      window.crmComms && window.crmComms.outboundCallHref
        ? window.crmComms.outboundCallHref(phone)
        : "../studio/outbound.kml?call=1&phone=" + encodeURIComponent(phone);
    return `<a href="${href}" class="confirm-call" data-phone="${escapeHtml(phone)}">${escapeHtml(phone)}</a>`;
  }

  function buildEmailHtml(email, opts = {}) {
    if (!email) return "N/A";
    const href =
      window.crmComms && window.crmComms.outboundEmailHref
        ? window.crmComms.outboundEmailHref(email)
        : "../studio/outbound.kml?email=1&to=" + encodeURIComponent(email);
    return `<a href="${href}">${escapeHtml(email)}</a>`;
  }

  function renderAccountHealth(id) {
    if (!accountHealthEnabled) return "";
    return `<span class="badge bg-light text-muted" data-account-health="${id}">0</span>`;
  }

  function initTable() {
    const columns = [];
    columns.push({
      data: null,
      render: (data, type, row, meta) => meta.row + 1 + meta.settings._iDisplayStart,
    });
    columns.push({
      data: null,
      render: (data, type, row) => {
        const id = row.id;
        const idSalt = row.id_s || id;
        const name = row.name || '';
        const subtitle = escapeHtml(row.country || row.email || '');
        return `
          <div class="entity-name-cell">
            ${avatarHtml(name)}
            <div>
              <div class="fw-semibold">
                <a href="studio/accounts/view.kml?id=${encodeURIComponent(idSalt)}" class="text-decoration-underline">${escapeHtml(name)}</a>
              </div>
              ${subtitle ? `<div class="small text-muted">${subtitle}</div>` : ''}
              <div class="account-inline-actions small mt-1">
                <a href="studio/accounts/view.kml?id=${encodeURIComponent(idSalt)}">View</a>
                <span class="text-muted">|</span>
                <a href="javascript:void(0);" class="btn-edit-account" data-account-id="${id}">Edit ${escapeHtml(moduleLabel("account", "singular"))}</a>
              </div>
            </div>
          </div>
        `;
      },
    });
    if (accountHealthEnabled) {
      columns.push({
        data: null,
        orderable: false,
        render: (data, type, row) => renderAccountHealth(row.id),
      });
    }
    columns.push({
      data: "phone",
      render: (data, type, row) => buildCallHtml(data || "", { phone: data || "", name: row.name || "", entityType: "account", entityId: row.id || "", entityLabel: row.name || "" }),
    });
    columns.push({
      data: "email",
      render: (data, type, row) => buildEmailHtml(data || "", { email: data || "", name: row.name || "", entityType: "account", entityId: row.id || "", entityLabel: row.name || "" }),
    });
    columns.push({
      data: "country",
      render: (data) => escapeHtml(data || ""),
    });
    columns.push({
      data: "status",
      render: (data) => {
        const status = String(data || "").toLowerCase();
        if (status === "active") {
          return '<span class="badge bg-success-subtle text-success">Active</span>';
        }
        return '<span class="badge bg-warning-subtle text-warning">Inactive</span>';
      },
    });
    columns.push({
      data: "description",
      render: (data) => escapeHtml(data || ""),
    });
    columns.push({
      data: null,
      orderable: false,
      render: (data, type, row) => {
        const id = row.id;
        const idSalt = row.id_s || id;
        return `
          <div class="d-flex justify-content-end gap-1">
            <a href="studio/accounts/view.kml?id=${encodeURIComponent(idSalt)}" class="btn btn-sm btn-light">
              <i class="ri-eye-line me-1"></i>View
            </a>
            <div class="btn-group btn-group-sm" role="group">
              <button type="button" class="btn btn-sm btn-soft-primary dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
                <i class="ri-more-2-fill"></i>
              </button>
              <div class="dropdown-menu dropdown-menu-end">
                <a class="dropdown-item btn-edit-account" href="javascript:void(0);" data-account-id="${id}">
                  <i class="ri-edit-2-line me-1"></i>Edit
                </a>
                <a class="dropdown-item text-danger btn-delete-account" href="javascript:void(0);" data-account-id="${id}">
                  <i class="ri-delete-bin-6-line me-1"></i>Delete
                </a>
              </div>
            </div>
          </div>
        `;
      },
      className: "text-end",
    });

    accountTable = window.jQuery("#accountTable").DataTable({
      processing: true,
      serverSide: true,
      searching: false,
      pageLength: (window.appConfig && window.appConfig.dataTablePageSize) ? window.appConfig.dataTablePageSize : 250,
      lengthMenu: [[250], [250]],
      ajax: {
        url: apiBaseList,
        type: "GET",
        data: function (d) {
          const term = searchInput ? searchInput.value.trim() : "";
          if (term) d.q = term;
          if (datePicker && datePicker.selectedDates.length) {
            const start = datePicker.selectedDates[0];
            const end = datePicker.selectedDates[1] || start;
            if (start) d.start_date = window.flatpickr.formatDate(start, "Y-m-d");
            if (end) d.end_date = window.flatpickr.formatDate(end, "Y-m-d");
          }
        },
        dataSrc: "data",
      },
      columns,
    });

    accountTable.on("draw", () => {
      if (window.refreshAccountHealth) window.refreshAccountHealth();
    });
  }

  function clearForm() {
    form.reset();
    document.getElementById("account_id").value = "";
    formAlert.classList.add("d-none");
    formAlert.innerHTML = "";
    document.getElementById("account_status").value = "active";
    if (phoneHelper) phoneHelper.setFull("");
    if (phoneAltHelper) phoneAltHelper.setFull("");
    if (phoneCodeSelect && !phoneCodeSelect.value) phoneCodeSelect.value = "+234";
    if (phoneAltCodeSelect && !phoneAltCodeSelect.value) phoneAltCodeSelect.value = "+234";
    if (countryNameInput) countryNameInput.value = "";
    if (stateNameInput) stateNameInput.value = "";
    if (cityNameInput) cityNameInput.value = "";
    if (geoHelper) {
      geoHelper.init({ country: "Nigeria" });
    }
  }

  function showFormError(msg) {
    formAlert.innerHTML = msg;
    formAlert.classList.remove("d-none");
  }

  function showPageError(msg) {
    if (pageAlert) {
      pageAlert.textContent = msg;
    } else {
      window.crmUiAlert(msg);
    }
  }

  initTable();

  function applySearch() {
    if (accountTable) accountTable.ajax.reload();
  }

  if (searchBtn) {
    searchBtn.addEventListener("click", function () {
      applySearch();
    });
  }
  if (searchInput) {
    searchInput.addEventListener("keypress", function (e) {
      if (e.key === "Enter") {
        e.preventDefault();
        applySearch();
      }
    });
  }

  document.getElementById("btnNewAccount").addEventListener("click", function () {
    clearForm();
    document.getElementById("accountOffcanvasLabel").textContent = "New " + moduleLabel("account", "singular");
    offcanvas?.show();
  });

  document.addEventListener("click", function (e) {
    const btn = e.target.closest(".btn-edit-account");
    if (!btn) return;
      const accountId = btn.getAttribute("data-account-id");
      const originalHtml = btn.innerHTML;
      btn.disabled = true;
      btn.innerHTML =
        '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>';
      clearForm();
      document.getElementById("accountOffcanvasLabel").textContent = "Edit " + moduleLabel("account", "singular");
      fetch(apiBaseDetail + "?id=" + encodeURIComponent(accountId))
        .then((r) => r.json())
        .then((data) => {
          if (!data || data.error) {
            showPageError(data && data.error ? data.error : "Unable to load " + moduleLabel("account", "singular").toLowerCase() + ".");
            return;
          }
          document.getElementById("account_id").value = data.id;
          document.getElementById("account_name").value = data.name || "";
          document.getElementById("account_description").value = data.description || "";
          document.getElementById("account_website").value = data.website || "";
          if (phoneHelper) {
            phoneHelper.setFull(data.phone || "");
          } else {
            document.getElementById("account_phone").value = (data.phone || "").replace(/\D+/g, "");
          }
          if (phoneAltHelper) {
            phoneAltHelper.setFull(data.phone_alt || "");
          } else if (phoneAltLocalInput) {
            phoneAltLocalInput.value = (data.phone_alt || "").replace(/\D+/g, "");
          }
          document.getElementById("account_email").value = data.email || "";
          document.getElementById("account_address").value = data.address_line1 || "";
          if (cityNameInput) cityNameInput.value = data.city || "";
          if (stateNameInput) stateNameInput.value = data.state || "";
          if (countryNameInput) countryNameInput.value = data.country || "";
          document.getElementById("account_postal").value = data.postal_code || "";
          if (geoHelper) {
            geoHelper.init({
              country: data.country || "",
              state: data.state || "",
              city: data.city || "",
            });
          }
          populateIndustries(data.industry || "");
          document.getElementById("account_industry").value = data.industry || "";
          document.getElementById("account_status").value = data.status || "active";
          offcanvas?.show();
        })
        .catch(() => showPageError("Unable to load " + moduleLabel("account", "singular").toLowerCase() + "."))
        .finally(() => {
          btn.disabled = false;
          btn.innerHTML = originalHtml;
        });
  });

  const confirmDeleteModalEl = document.getElementById("confirmDeleteAccount");
  const confirmDeleteModal = confirmDeleteModalEl ? new bootstrap.Modal(confirmDeleteModalEl) : null;
  const confirmDeleteBtn = document.getElementById("confirmDeleteAccountBtn");
  let pendingDeleteId = null;

  document.addEventListener("click", function (e) {
    const delBtn = e.target.closest(".btn-delete-account");
    if (!delBtn) return;
    pendingDeleteId = delBtn.getAttribute("data-account-id");
    confirmDeleteModal?.show();
  });

  confirmDeleteBtn?.addEventListener("click", function () {
    if (!pendingDeleteId) return;
    const spinner = this.querySelector(".spinner-border");
    const text = this.querySelector(".btn-text");
    this.disabled = true;
    if (spinner) spinner.classList.remove("d-none");
    if (text) text.textContent = "";

    window.jQuery.ajax({
      url: apiBaseDetail + "?id=" + encodeURIComponent(pendingDeleteId),
      type: "DELETE",
      success: function () {
        if (accountTable) {
          accountTable.ajax.reload(null, false);
        }
        confirmDeleteModal?.hide();
      },
      error: function () {
        showPageError("Unable to delete " + moduleLabel("account", "singular").toLowerCase() + ".");
        confirmDeleteModal?.hide();
      },
      complete: function () {
        confirmDeleteBtn.disabled = false;
        if (spinner) spinner.classList.add("d-none");
        if (text) text.textContent = "Delete";
        pendingDeleteId = null;
      },
    });
  });

  form.addEventListener("submit", function (e) {
    e.preventDefault();
    formAlert.classList.add("d-none");
    const saveBtn = document.getElementById("saveAccountBtn");
    const originalSaveHtml = saveBtn ? saveBtn.innerHTML : "";
    if (saveBtn) {
      saveBtn.disabled = true;
      saveBtn.innerHTML =
        '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
    }

    const accountId = document.getElementById("account_id").value;
    const phoneRaw = document.getElementById("account_phone").value.replace(/\D+/g, "");
    const phoneHidden = phoneFullInput ? phoneFullInput.value.trim() : "";
    const phoneFull =
      phoneHidden ||
      (phoneHelper
        ? phoneHelper.getFull()
        : phoneRaw && phoneCodeSelect && phoneCodeSelect.value && !phoneRaw.startsWith("+")
        ? phoneCodeSelect.value + phoneRaw
        : phoneRaw);
    const phoneAltRaw = phoneAltLocalInput ? phoneAltLocalInput.value.replace(/\D+/g, "") : "";
    const phoneAltHidden = phoneAltFullInput ? phoneAltFullInput.value.trim() : "";
    const phoneAltFull =
      phoneAltHidden ||
      (phoneAltHelper
        ? phoneAltHelper.getFull()
        : phoneAltRaw && phoneAltCodeSelect && phoneAltCodeSelect.value && !phoneAltRaw.startsWith("+")
        ? phoneAltCodeSelect.value + phoneAltRaw
        : phoneAltRaw);
    const payload = {
      name: document.getElementById("account_name").value.trim(),
      description: document.getElementById("account_description").value.trim(),
      status: document.getElementById("account_status").value,
      website: document.getElementById("account_website").value.trim(),
      phone: phoneFull,
      phone_alt: phoneAltFull,
      email: document.getElementById("account_email").value.trim(),
      industry: document.getElementById("account_industry").value.trim(),
      address_line1: document.getElementById("account_address").value.trim(),
      city: (cityNameInput ? cityNameInput.value : document.getElementById("account_city").value).trim(),
      state: (stateNameInput ? stateNameInput.value : document.getElementById("account_state").value).trim(),
      postal_code: document.getElementById("account_postal").value.trim(),
      country: (countryNameInput ? countryNameInput.value : document.getElementById("account_country").value).trim(),
    };

    window.jQuery.ajax({
      url: accountId ? apiBaseDetail + "?id=" + encodeURIComponent(accountId) : apiBaseList,
      type: accountId ? "PATCH" : "POST",
      contentType: "application/json",
      data: JSON.stringify(payload),
      success: function () {
        window.location.reload();
      },
      error: function (xhr) {
        let msg = "Unable to save account.";
        if (xhr.responseJSON && xhr.responseJSON.error) {
          msg = xhr.responseJSON.error;
        } else if (xhr.responseText) {
          try {
            const parsed = JSON.parse(xhr.responseText);
            if (parsed.error) msg = parsed.error;
          } catch (e) {
            msg = xhr.responseText;
          }
        } else if (xhr.statusText) {
          msg = xhr.statusText;
        }
        showFormError(msg);
      },
      complete: function () {
        if (saveBtn) {
          saveBtn.disabled = false;
          saveBtn.innerHTML = originalSaveHtml || "Save";
        }
      },
    });
  });

  populateCountries();
  populateIndustries();
})();
