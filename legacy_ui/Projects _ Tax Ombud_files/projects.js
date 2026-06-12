/* eslint-disable */
(function () {
  "use strict";

  const cfg = window.projectsConfig || {};
  const canUpdate = !!cfg.canUpdate;
  const canDelete = !!cfg.canDelete;

  const apiMap = (typeof apiEndpoints === "function") ? apiEndpoints() : {};
  const apiIndex = apiMap.projectsIndex || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/projects/index");
  const apiDetail = apiMap.projectsDetail || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/projects/detail");

  const pageSize = (window.appConfig && window.appConfig.dataTablePageSize)
    ? Number(window.appConfig.dataTablePageSize)
    : 150;

  const searchInput = document.getElementById("projectsSearch");
  const statusFilter = document.getElementById("projectsStatusFilter");
  const ownerFilterSearch = document.getElementById("projectsOwnerSearch");
  const ownerFilter = document.getElementById("projectsOwnerFilter");
  const applyFilterBtn = document.getElementById("projectsApplyFilterBtn");
  const resetFilterBtn = document.getElementById("projectsResetFilterBtn");
  const alertBox = document.getElementById("projectsAlert");
  const gridEl = document.getElementById("projectsGrid");
  const pagerWrap = document.getElementById("projectsPagerWrap");
  const pagerText = document.getElementById("projectsPagerText");
  const paginationEl = document.getElementById("projectsPagination");

  const deleteModalEl = document.getElementById("projectDeleteModal");
  const deleteModal = deleteModalEl ? new bootstrap.Modal(deleteModalEl) : null;
  const deleteBtn = document.getElementById("projectDeleteConfirmBtn");

  const state = {
    q: "",
    status: "",
    ownerUserId: "",
    offset: 0,
    limit: pageSize,
    total: 0,
    filtered: 0,
    requestToken: 0
  };

  let pendingDeleteId = null;

  function esc(v) {
    return String(v || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/\"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function showAlert(msg) {
    if (!alertBox) return;
    alertBox.textContent = msg || "";
  }

  function setSaving(btn, saving, idleLabel, busyLabel) {
    if (!btn) return;
    const spinner = btn.querySelector(".spinner-border");
    const txt = btn.querySelector(".btn-text");
    btn.disabled = !!saving;
    if (spinner) spinner.classList.toggle("d-none", !saving);
    if (txt) txt.textContent = saving ? busyLabel : idleLabel;
  }

  function statusMeta(status) {
    const key = String(status || "").toLowerCase();
    const map = {
      planning: { badge: "bg-info-subtle text-info", top: "bg-info-subtle", progress: 20 },
      active: { badge: "bg-success-subtle text-success", top: "bg-success-subtle", progress: 55 },
      on_hold: { badge: "bg-warning-subtle text-warning", top: "bg-warning-subtle", progress: 40 },
      completed: { badge: "bg-primary-subtle text-primary", top: "bg-primary-subtle", progress: 100 },
      cancelled: { badge: "bg-danger-subtle text-danger", top: "bg-danger-subtle", progress: 0 }
    };
    const meta = map[key] || { badge: "bg-light text-muted", top: "bg-light", progress: 0 };
    return {
      key,
      label: key ? key.replace(/_/g, " ") : "unknown",
      badgeClass: meta.badge,
      topClass: meta.top,
      progress: meta.progress
    };
  }

  function initials(name) {
    const parts = String(name || "").trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return "?";
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }

  function formatDateShort(value) {
    if (!value) return "-";
    const d = new Date(String(value).replace(" ", "T"));
    if (Number.isNaN(d.getTime())) return esc(value);
    return d.toLocaleDateString(undefined, { day: "2-digit", month: "short", year: "numeric" });
  }

  function applyCurrentFilters() {
    state.q = searchInput ? searchInput.value.trim() : "";
    state.status = statusFilter ? statusFilter.value : "";
    state.ownerUserId = ownerFilter ? ownerFilter.value : "";
    state.offset = 0;
  }

  function buildListParams() {
    const p = new URLSearchParams();
    p.set("limit", String(state.limit));
    p.set("offset", String(state.offset));
    if (state.q) p.set("q", state.q);
    if (state.status) p.set("status", state.status);
    if (state.ownerUserId) p.set("owner_user_id", state.ownerUserId);
    return p;
  }

  function renderLoadingCards() {
    if (!gridEl) return;
    gridEl.innerHTML = "";
    for (let i = 0; i < 4; i += 1) {
      gridEl.insertAdjacentHTML("beforeend", `
        <div class="col-xxl-3 col-sm-6 project-card">
          <div class="card card-height-100">
            <div class="card-body d-flex align-items-center justify-content-center" style="min-height:180px;">
              <div class="text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>Loading projects...
              </div>
            </div>
          </div>
        </div>
      `);
    }
  }

  function cardActions(row) {
    const idForUrl = encodeURIComponent(row.id_s || row.id);
    const view = `<a class="dropdown-item" href="studio/projects/view.kml?id=${idForUrl}"><i class="ri-eye-fill align-bottom me-2 text-muted"></i> View</a>`;
    const edit = canUpdate ? `<a class="dropdown-item" href="studio/projects/projects-create.kml?id=${idForUrl}"><i class="ri-pencil-fill align-bottom me-2 text-muted"></i> Edit</a>` : "";
    const remove = canDelete ? `<a class="dropdown-item project-delete" href="#" data-id="${row.id}"><i class="ri-delete-bin-fill align-bottom me-2 text-muted"></i> Remove</a>` : "";
    return `${view}${edit}${remove ? '<div class="dropdown-divider"></div>' + remove : ""}`;
  }

  function renderCard(row) {
    const meta = statusMeta(row.status);
    const title = esc(row.name || ("Project #" + row.id));
    const desc = esc(row.description_short || row.description || "No description");
    const ownerName = esc(row.owner_name || "Unassigned");
    const ownerInitials = initials(row.owner_name || "U");
    const updated = formatDateShort(row.updated_at || row.created_at);
    const deadline = formatDateShort(row.end_date);
    const taskTotal = Math.max(0, parseInt(row.task_total || 0, 10) || 0);
    const taskDone = Math.max(0, parseInt(row.task_done || 0, 10) || 0);
    const timelineCount = Math.max(0, parseInt(row.timeline_count || 0, 10) || 0);
    const timelineDone = Math.max(0, parseInt(row.timeline_done_count || 0, 10) || 0);
    let progress = Math.max(0, Math.min(100, Number(row.completion_percent)));
    if (!Number.isFinite(progress)) {
      progress = Math.max(0, Math.min(100, Number(meta.progress || 0)));
    }
    const progressHint = timelineCount > 0
      ? `${timelineDone}/${timelineCount} milestones completed`
      : `${taskDone}/${taskTotal} tasks completed`;

    return `
      <div class="col-xxl-3 col-sm-6 project-card mb-4">
        <div class="card card-height-100">
          <div class="card-body">
            <div class="p-3 mt-n3 mx-n3 ${meta.topClass} rounded-top">
              <div class="d-flex align-items-center">
                <div class="flex-grow-1">
                  <p class="text-muted mb-0">Updated ${esc(updated)}</p>
                </div>
                <div class="flex-shrink-0">
                  <div class="dropdown">
                    <button class="btn btn-link text-muted p-1 py-0 text-decoration-none fs-15 shadow-none" data-bs-toggle="dropdown" aria-expanded="false">
                      <i class="ri-more-2-fill"></i>
                    </button>
                    <div class="dropdown-menu dropdown-menu-end">
                      ${cardActions(row)}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="py-3">
              <h5 class="mb-2 fs-14">
                <a href="studio/projects/view.kml?id=${encodeURIComponent(row.id_s || row.id)}" class="text-body">${title}</a>
              </h5>
              <p class="text-muted text-truncate-two-lines mb-3">${desc}</p>
              <div class="row gy-3">
                <div class="col-6">
                  <p class="text-muted mb-1">Status</p>
                  <div class="badge ${meta.badgeClass} fs-12 text-capitalize">${esc(meta.label)}</div>
                </div>
                <div class="col-6">
                  <p class="text-muted mb-1">Deadline</p>
                  <h5 class="fs-14 mb-0">${esc(deadline)}</h5>
                </div>
              </div>
              <div class="d-flex align-items-center mt-3">
                <p class="text-muted mb-0 me-2">Owner:</p>
                <div class="avatar-xxs me-2">
                  <span class="avatar-title rounded-circle bg-primary-subtle text-primary fw-semibold">${esc(ownerInitials)}</span>
                </div>
                <span class="text-body fw-medium">${ownerName}</span>
              </div>
            </div>

            <div>
              <div class="d-flex mb-2">
                <div class="flex-grow-1"><div>Progress</div></div>
                <div class="flex-shrink-0"><div>${progress}%</div></div>
              </div>
              <div class="small text-muted mb-1">${esc(progressHint)}</div>
              <div class="progress progress-sm animated-progress bg-success-subtle">
                <div class="progress-bar bg-success" role="progressbar" style="width:${progress}%;"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  function renderEmptyState() {
    if (!gridEl) return;
    gridEl.innerHTML = `
      <div class="col-12">
        <div class="card">
          <div class="card-body text-center text-muted py-5">
            <i class="ri-folder-open-line fs-24 d-block mb-2"></i>
            No projects found for this filter.
          </div>
        </div>
      </div>
    `;
  }

  function renderGrid(rows) {
    if (!gridEl) return;
    if (!rows || !rows.length) {
      renderEmptyState();
      return;
    }
    gridEl.innerHTML = rows.map(renderCard).join("");
  }

  function createPageItem(label, page, disabled, active) {
    const cls = ["page-item"];
    if (disabled) cls.push("disabled");
    if (active) cls.push("active");
    const safePage = Number(page) > 0 ? String(page) : "";
    return `<li class="${cls.join(" ")}"><a class="page-link project-page-link" href="#" data-page="${safePage}">${label}</a></li>`;
  }

  function renderPagination() {
    if (!pagerWrap || !pagerText || !paginationEl) return;
    const total = Number(state.filtered || 0);
    const from = total > 0 ? (state.offset + 1) : 0;
    const to = Math.min(state.offset + state.limit, total);
    pagerText.innerHTML = `Showing <span class="fw-semibold">${from}</span> to <span class="fw-semibold">${to}</span> of <span class="fw-semibold text-decoration-underline">${total}</span> entries`;

    const pageCount = Math.max(1, Math.ceil(total / state.limit));
    const currentPage = Math.floor(state.offset / state.limit) + 1;
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(pageCount, currentPage + 2);

    let html = "";
    html += createPageItem("Previous", currentPage - 1, currentPage <= 1, false);
    for (let p = startPage; p <= endPage; p += 1) {
      html += createPageItem(String(p), p, false, p === currentPage);
    }
    html += createPageItem("Next", currentPage + 1, currentPage >= pageCount, false);
    paginationEl.innerHTML = html;
    pagerWrap.style.display = "flex";
  }

  async function fetchProjects() {
    const token = ++state.requestToken;
    showAlert("");
    renderLoadingCards();
    try {
      const res = await fetch(apiIndex + "?" + buildListParams().toString(), { method: "GET" });
      const payload = await res.json().catch(() => ({}));
      if (token !== state.requestToken) return;
      if (!res.ok) throw new Error(payload.error || "Unable to load projects");
      const rows = Array.isArray(payload.data) ? payload.data : [];
      state.total = Number(payload.total || 0);
      state.filtered = Number(payload.filtered || payload.total || rows.length || 0);
      renderGrid(rows);
      renderPagination();
    } catch (err) {
      if (token !== state.requestToken) return;
      showAlert(err.message || "Unable to load projects");
      renderEmptyState();
      if (pagerWrap) pagerWrap.style.display = "none";
    }
  }

  async function loadOwners(target, q) {
    if (!target) return;
    const params = new URLSearchParams();
    params.set("lookup", "staff");
    params.set("limit", "30");
    if (q) params.set("q", q);

    const current = target.value;
    const res = await fetch(apiIndex + "?" + params.toString(), { method: "GET" });
    const payload = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(payload.error || "Unable to load owners");

    target.innerHTML = '<option value="">All Owners</option>';
    (payload.data || []).forEach((row) => {
      const opt = document.createElement("option");
      opt.value = String(row.id || "");
      opt.textContent = row.label || ("#" + row.id);
      target.appendChild(opt);
    });

    if (current && target.querySelector(`option[value="${current}"]`)) {
      target.value = current;
    }
  }

  function debounce(fn, wait) {
    let timer = null;
    return function debounced(...args) {
      if (timer) clearTimeout(timer);
      timer = setTimeout(() => fn.apply(this, args), wait);
    };
  }

  async function deleteProject() {
    if (!pendingDeleteId) return;
    setSaving(deleteBtn, true, "Delete", "Deleting...");
    try {
      const res = await fetch(apiDetail + "?id=" + encodeURIComponent(pendingDeleteId), { method: "DELETE" });
      const payload = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(payload.error || "Unable to delete project");
      if (deleteModal) deleteModal.hide();
      pendingDeleteId = null;
      await fetchProjects();
    } catch (err) {
      showAlert(err.message || "Unable to delete project");
    } finally {
      setSaving(deleteBtn, false, "Delete", "Deleting...");
    }
  }

  function bindEvents() {
    if (applyFilterBtn) {
      applyFilterBtn.addEventListener("click", function () {
        applyCurrentFilters();
        fetchProjects();
      });
    }

    if (resetFilterBtn) {
      resetFilterBtn.addEventListener("click", function () {
        if (searchInput) searchInput.value = "";
        if (statusFilter) statusFilter.value = "";
        if (ownerFilterSearch) ownerFilterSearch.value = "";
        if (ownerFilter) ownerFilter.value = "";
        applyCurrentFilters();
        fetchProjects();
      });
    }

    if (searchInput) {
      searchInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
          e.preventDefault();
          applyCurrentFilters();
          fetchProjects();
        }
      });
    }

    if (statusFilter) {
      statusFilter.addEventListener("change", function () {
        applyCurrentFilters();
        fetchProjects();
      });
    }

    if (ownerFilter) {
      ownerFilter.addEventListener("change", function () {
        applyCurrentFilters();
        fetchProjects();
      });
    }

    if (ownerFilterSearch) {
      ownerFilterSearch.addEventListener("input", debounce(function () {
        loadOwners(ownerFilter, ownerFilterSearch.value.trim()).catch(() => {});
      }, 300));
    }

    if (deleteBtn) {
      deleteBtn.addEventListener("click", deleteProject);
    }

    document.addEventListener("click", function (e) {
      const del = e.target.closest(".project-delete");
      if (del) {
        e.preventDefault();
        pendingDeleteId = parseInt(del.getAttribute("data-id"), 10) || null;
        if (pendingDeleteId && deleteModal) deleteModal.show();
        return;
      }

      const pageLink = e.target.closest(".project-page-link");
      if (!pageLink) return;
      e.preventDefault();
      const page = parseInt(pageLink.getAttribute("data-page"), 10);
      if (!page || page < 1) return;
      const totalPages = Math.max(1, Math.ceil((state.filtered || 0) / state.limit));
      if (page > totalPages) return;
      state.offset = (page - 1) * state.limit;
      fetchProjects();
    });
  }

  function init() {
    loadOwners(ownerFilter, "").catch(() => {});
    bindEvents();
    applyCurrentFilters();
    fetchProjects();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
