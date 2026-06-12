/* eslint-disable */
(function () {
  "use strict";

  const cfg = window.organizationListConfig || {};
  const moduleLabel = (typeof window.moduleLabel === "function")
    ? window.moduleLabel
    : (key, form) => {
      const defaults = {
        account: { singular: "Account", plural: "Accounts" },
        contact: { singular: "Contact", plural: "Contacts" },
        organization: { singular: "Organization", plural: "Organizations" },
        case: { singular: "Case", plural: "Cases" }
      };
      const k = String(key || "").toLowerCase();
      const f = String(form || "plural").toLowerCase() === "singular" ? "singular" : "plural";
      return (defaults[k] && defaults[k][f]) || k;
    };
  const apiBase = typeof url_root !== "undefined" ? url_root : "../";
  const apiOrganizationDetail = apiBase + "api/modules/organizations/detail";
  const apiExport = apiBase + "api/modules/organizations/export";
  const comms = window.crmComms || null;
  const prefillContactId = cfg.prefillContactId ?? null;
  const prefillContactLabel = cfg.prefillContactLabel ?? "";
  const autoOpen = !!cfg.autoOpen;
  const canEdit = !!cfg.canEdit;
  const canDelete = !!cfg.canDelete;

  let organizationTable = null;
  const searchInput = document.getElementById("organizationSearch");
  const searchBtn = document.getElementById("organizationSearchBtn");
  const resetBtn = document.getElementById("organizationResetBtn");
  const dateRange = document.getElementById("organizationDateRange");
  const datePicker = dateRange
    ? flatpickr(dateRange, { mode: "range", dateFormat: "Y-m-d", allowInput: true })
    : null;
  function formatLocalDate(d) {
    if (!d) return "";
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${y}-${m}-${day}`;
  }

  const organizationQuickOffcanvas = new bootstrap.Offcanvas("#organizationQuickView");
  const organizationForm = setuporganizationForm({
    apiorganizationsIndex: apiBase + "api/modules/organizations/index",
    apiorganizationDetail: apiBase + "api/modules/organizations/detail",
    apiContacts: apiBase + "api/modules/contacts/index",
    onSaved: () => window.location.reload(),
  });

  function clearForm() {
    if (!organizationForm) return;
    organizationForm.clearForm();
    document.getElementById("co_contact").value = prefillContactId || "";
    document.getElementById("co_contact_search").value = prefillContactLabel || "";
    document.getElementById("co_contact_selected").textContent = prefillContactId
      ? "Selected: " + prefillContactLabel
      : "";
  }

  function contactDisplay(c) {
    if (!c) return "";
    const name = ((c.first_name || "") + " " + (c.last_name || "")).trim();
    return name || c.phone || c.email || (moduleLabel("contact", "singular") + " #" + (c.id_s || c.id || ""));
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

  function buildPhoneHtml(phone, opts = {}) {
    if (!phone) return "-";
    const includeActionButton = opts && opts.includeActionButton === false ? false : true;
    const callBtn = includeActionButton && comms ? comms.buildActionButton("call", opts) : "";
    const href = (window.crmComms && window.crmComms.outboundCallHref)
      ? window.crmComms.outboundCallHref(phone)
      : "../studio/outbound.kml?call=1&phone=" + encodeURIComponent(phone);
    return `<a href="${href}" class="confirm-call" data-phone="${escapeHtml(phone)}">${escapeHtml(phone)}</a>${callBtn}`;
  }

  function buildEmailHtml(email, opts = {}) {
    if (!email) return "-";
    const includeActionButton = opts && opts.includeActionButton === false ? false : true;
    const emailBtn = includeActionButton && comms ? comms.buildActionButton("email", opts) : "";
    const href = (window.crmComms && window.crmComms.outboundEmailHref)
      ? window.crmComms.outboundEmailHref(email)
      : "../studio/outbound.kml?email=1&to=" + encodeURIComponent(email);
    return `<a href="${href}">${escapeHtml(email)}</a>${emailBtn}`;
  }

  function renderPrimaryContact(row) {
    const name = (row.primary_contact_name || "").trim();
    if (!name) return "-";
    const id = row.primary_contact_id_s || row.primary_contact_id || "";
    const nameHtml = id
      ? `<a href="studio/contacts/view.kml?id=${encodeURIComponent(id)}" class="text-decoration-underline">${escapeHtml(name)}</a>`
      : escapeHtml(name);
    const metaParts = [];
    if (row.primary_contact_phone) metaParts.push(buildPhoneHtml(row.primary_contact_phone, { includeActionButton: false }));
    if (row.primary_contact_email) metaParts.push(buildEmailHtml(row.primary_contact_email, { includeActionButton: false }));
    const meta = metaParts.length ? `<div class="text-muted small mt-1">${metaParts.join(" • ")}</div>` : "";
    return `${nameHtml}${meta}`;
  }

  function initTable() {
    const cols = [
      {
        data: null,
        render: (data, type, row, meta) => meta.row + 1 + meta.settings._iDisplayStart,
      },
      {
        data: null,
        render: (data, type, row) => {
          const name = row.name || '';
          const idSalt = row.id_s || row.id;
          const deleted = String(row.status || "") === "9";
          const subtitle = escapeHtml(row.email || row.phone || '');
          const nameLine = deleted
            ? `<span class="text-danger fw-semibold">${escapeHtml(name)}</span> <span class="badge bg-danger ms-1">Deleted</span>`
            : `<a href="studio/organizations/view.kml?id=${encodeURIComponent(idSalt)}" class="text-decoration-underline fw-semibold">${escapeHtml(name)}</a>`;
          const actions = `
            <div class="organization-inline-actions small mt-1">
              <a href="javascript:void(0);" class="organization-qv-link" data-organization-id="${row.id}">Quick View</a>
              <span class="text-muted">|</span>
              <a href="studio/organizations/view.kml?id=${encodeURIComponent(idSalt)}">View</a>
              ${canEdit ? '<span class="text-muted">|</span><a href="javascript:void(0);" class="btn-edit-organization" data-organization-id="' + row.id + '">Edit</a>' : ''}
            </div>`;
          return `
            <div class="entity-name-cell">
              ${avatarHtml(name)}
              <div>
                <div>${nameLine}</div>
                ${subtitle ? `<div class="small text-muted">${subtitle}</div>` : ''}
                ${actions}
              </div>
            </div>`;
        },
      },
      {
        data: "phone",
        render: (data, type, row) =>
          buildPhoneHtml(data, { phone: data || "", name: row.name || "", entityType: "organization", entityId: row.id || "", entityLabel: row.name || "", includeActionButton: false }),
      },
      {
        data: "email",
        render: (data, type, row) =>
          buildEmailHtml(data, { email: data || "", name: row.name || "", entityType: "organization", entityId: row.id || "", entityLabel: row.name || "", includeActionButton: false }),
      },
      {
        data: null,
        render: (data, type, row) => renderPrimaryContact(row),
      },
      {
        data: "created_at",
        render: (data) => escapeHtml(data || ""),
      },
      {
        data: null,
        orderable: false,
        className: "text-end",
        render: (data, type, row) => {
          const idSalt = row.id_s || row.id;
          return `
            <div class="btn-group btn-group-sm">
              <a class="btn btn-primary" href="studio/organizations/view.kml?id=${encodeURIComponent(idSalt)}"><i class="ri-eye-line me-1"></i>View</a>
              <button class="btn btn-soft-primary dropdown-toggle" data-bs-toggle="dropdown" aria-expanded="false">
                <i class="ri-more-fill"></i> More
              </button>
              <div class="dropdown-menu dropdown-menu-end">
                ${canEdit ? '<button class="dropdown-item btn-edit-organization" data-organization-id="' + row.id + '"><i class="ri-edit-line me-1"></i>Edit</button>' : ''}
                ${canDelete ? '<button class="dropdown-item text-danger btn-delete-organization" data-organization-id="' + row.id + '"><i class="ri-delete-bin-6-line me-1"></i>Delete</button>' : ''}
              </div>
            </div>`;
        },
      },
    ];

    organizationTable = window.jQuery("#organizationTable").DataTable({
      processing: true,
      serverSide: true,
      searching: false,
      pageLength: (window.appConfig && window.appConfig.dataTablePageSize) ? window.appConfig.dataTablePageSize : 250,
      lengthMenu: [[250], [250]],
      ajax: {
        url: apiBase + "api/modules/organizations/index",
        type: "GET",
        data: function (d) {
          const term = searchInput ? searchInput.value.trim() : "";
          if (d.search && typeof d.search === "object") {
            d.search.value = term;
          } else if (term) {
            d.search = { value: term };
          }
          if (datePicker && datePicker.selectedDates.length) {
            const [start, end] = datePicker.selectedDates;
            const s = formatLocalDate(start);
            const e = formatLocalDate(end || start);
            if (s) d.start_date = s;
            if (e) d.end_date = e;
          }
        },
        dataSrc: "data",
      },
      columns: cols,
      order: [[5, "desc"]],
    });
  }

  initTable();

  async function loadOrganizationQuickView(organizationId) {
    if (!organizationId) return;
    const alert = document.getElementById("cqv_alert");
    const nameEl = document.getElementById("cqv_name");
    const createdEl = document.getElementById("cqv_created");
    const phoneEl = document.getElementById("cqv_phone");
    const emailEl = document.getElementById("cqv_email");
    const webEl = document.getElementById("cqv_website");
    const tinEl = document.getElementById("cqv_tin");
    const addrEl = document.getElementById("cqv_address");
    const contactsEl = document.getElementById("cqv_contacts");
    const fullLink = document.getElementById("cqv_full_link");
    if (alert) {
      alert.classList.add("d-none");
      alert.textContent = "";
    }
    if (contactsEl) contactsEl.innerHTML = '<div class="text-muted">Loading...</div>';
    if (nameEl) nameEl.textContent = "Loading...";
    if (createdEl) createdEl.textContent = "";
    if (phoneEl) phoneEl.innerHTML = "";
    if (emailEl) emailEl.innerHTML = "";
    if (webEl) webEl.innerHTML = "";
    if (tinEl) tinEl.textContent = "";
    if (addrEl) addrEl.textContent = "";
    try {
      const res = await fetch(apiOrganizationDetail + "?id=" + encodeURIComponent(organizationId));
      const data = await res.json();
      if (!res.ok) throw new Error(data?.error || "Unable to load organization");
      if (nameEl) nameEl.textContent = data.name || "-";
      if (createdEl) createdEl.textContent = data.created_at || "";
      const orgCommsOpts = {
        phone: data.phone || "",
        email: data.email || "",
        name: data.name || "",
        entityType: "organization",
        entityId: data.id || "",
        entityLabel: data.name || "",
      };
      if (phoneEl) phoneEl.innerHTML = buildPhoneHtml(data.phone, orgCommsOpts);
      if (emailEl) emailEl.innerHTML = buildEmailHtml(data.email, orgCommsOpts);
      if (webEl) webEl.innerHTML = data.website ? `<a href="${data.website}" target="_blank">${data.website}</a>` : "-";
      if (tinEl) tinEl.textContent = data.tin || "-";
      if (addrEl)
        addrEl.textContent =
          [data.address_line1, data.city, data.country].filter(Boolean).join(", ") || "-";
      if (fullLink) fullLink.href = "studio/organizations/view.kml?id=" + encodeURIComponent(organizationId);
      if (contactsEl) {
        contactsEl.innerHTML = "";
        (data.contacts || []).forEach((ct) => {
          const item = document.createElement("div");
          item.className = "list-group-item";
          const name = contactDisplay(ct);
          const meta = [ct.phone, ct.email].filter(Boolean).join(" • ");
          const commsOpts = {
            phone: ct.phone || "",
            email: ct.email || "",
            name,
            entityType: "contact",
            entityId: ct.id || "",
            entityLabel: name,
          };
          const commsHtml = comms ? comms.buildInlineButtons(commsOpts) : "";
          item.innerHTML = `<div class="fw-semibold">${escapeHtml(name)}${
            ct.is_primary ? ' <span class="badge bg-light text-primary ms-1">Primary</span>' : ""
          }</div><div class="text-muted">${escapeHtml(meta) || "-"} ${commsHtml}</div>`;
          contactsEl.appendChild(item);
        });
        if (!contactsEl.innerHTML) {
          contactsEl.innerHTML = '<div class="text-muted">No contacts</div>';
        }
      }
      organizationQuickOffcanvas.show();
    } catch (err) {
      if (alert) {
        alert.textContent = err.message || "Unable to load organization";
        alert.classList.remove("d-none");
      }
    }
  }

  document.addEventListener("click", async (e) => {
    const btn = e.target.closest(".organization-qv-link");
    if (!btn || !btn.dataset.organizationId) return;
    e.preventDefault();
    await loadOrganizationQuickView(btn.dataset.organizationId);
  });

  document.getElementById("btnNeworganization").addEventListener("click", function () {
    clearForm();
    organizationForm.openCreate({
      contactId: prefillContactId,
      contactLabel: prefillContactLabel,
    });
  });

  function applyFilters() {
    if (organizationTable) organizationTable.ajax.reload();
  }

  const exportBtn = document.getElementById("organizationExportBtn");
  if (exportBtn) {
    exportBtn.addEventListener("click", () => {
      const params = new URLSearchParams();
      if (searchInput && searchInput.value.trim()) params.set("q", searchInput.value.trim());
      if (datePicker && datePicker.selectedDates.length) {
        const [start, end] = datePicker.selectedDates;
        const s = start ? start.toISOString().slice(0, 10) : "";
        const e = end ? end.toISOString().slice(0, 10) : s;
        if (s) params.set("start_date", s);
        if (e) params.set("end_date", e);
      }
      if (prefillContactId) params.set("contact_id", prefillContactId);
      const url = apiExport + (params.toString() ? "?" + params.toString() : "");
      window.location.href = url;
    });
  }

  if (autoOpen && organizationForm) {
    clearForm();
    organizationForm.openCreate({
      contactId: prefillContactId,
      contactLabel: prefillContactLabel,
    });
  }

  if (searchBtn) searchBtn.addEventListener("click", applyFilters);
  if (resetBtn) {
    resetBtn.addEventListener("click", () => {
      if (searchInput) searchInput.value = "";
      if (datePicker) datePicker.clear();
      if (organizationTable) organizationTable.ajax.reload();
    });
  }

  const deleteOrganizationModal = new bootstrap.Modal(
    document.getElementById("deleteorganizationModal")
  );
  const confirmDeleteBtn = document.getElementById("confirmDeleteorganization");
  let pendingDeleteId = null;

  document.addEventListener("click", (e) => {
    const editBtn = e.target.closest(".btn-edit-organization");
    if (editBtn && organizationForm) {
      e.preventDefault();
      organizationForm.openEdit(editBtn.dataset.organizationId);
      return;
    }
    const delBtn = e.target.closest(".btn-delete-organization");
    if (delBtn) {
      e.preventDefault();
      pendingDeleteId = delBtn.dataset.organizationId || null;
      deleteOrganizationModal.show();
    }
  });

  if (confirmDeleteBtn) {
    confirmDeleteBtn.addEventListener("click", () => {
      if (!pendingDeleteId) return;
      const spinner = confirmDeleteBtn.querySelector(".spinner-border");
      const text = confirmDeleteBtn.querySelector(".btn-text");
      confirmDeleteBtn.disabled = true;
      if (spinner) spinner.classList.remove("d-none");
      if (text) text.textContent = "Delete";
      fetch(apiBase + "api/modules/organizations/detail?id=" + encodeURIComponent(pendingDeleteId), {
        method: "DELETE",
      })
        .then((r) => r.json().then((data) => ({ ok: r.ok, data })))
        .then(({ ok, data }) => {
          if (!ok) throw new Error(data?.error || "Unable to delete organization");
          window.location.reload();
        })
        .catch(() => window.crmUiAlert("Unable to delete organization"))
        .finally(() => {
          confirmDeleteBtn.disabled = false;
          if (spinner) spinner.classList.add("d-none");
          deleteOrganizationModal.hide();
          pendingDeleteId = null;
        });
    });
  }
})();
