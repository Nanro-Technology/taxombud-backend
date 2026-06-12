/* eslint-disable */
(function () {
  "use strict";

  const cfg = window.projectFormConfig || {};
  const canCreate = !!cfg.canCreate;
  const canUpdate = !!cfg.canUpdate;

  const api = (typeof apiEndpoints === "function") ? apiEndpoints() : {};
  const apiIndex = api.projectsIndex || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/projects/index");
  const apiDetail = api.projectsDetail || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/projects/detail");
  const apiFiles = api.filesIndex || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/files/index");

  const alertBox = document.getElementById("projectFormAlert");
  const heading = document.getElementById("projectFormHeading");

  const idInput = document.getElementById("projectId");
  const nameInput = document.getElementById("projectName");
  const statusInput = document.getElementById("projectStatus");
  const ownerInput = document.getElementById("projectOwnerId");
  const memberSelect = document.getElementById("projectMemberIds");
  const startDateInput = document.getElementById("projectStartDate");
  const endDateInput = document.getElementById("projectEndDate");
  const descriptionInput = document.getElementById("projectDescription");
  const descriptionEditorEl = document.getElementById("projectDescriptionEditor");
  const filesInput = document.getElementById("projectFilesInput");
  const filesList = document.getElementById("projectFilesList");
  const saveBtn = document.getElementById("projectSaveBtn");

  let linkedFileIds = [];
  let linkedFilesById = {};
  let quill = null;

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
    alertBox.classList.toggle("d-none", !msg);
  }

  function setSaving(saving) {
    if (!saveBtn) return;
    const spinner = saveBtn.querySelector(".spinner-border");
    const text = saveBtn.querySelector(".btn-text");
    saveBtn.disabled = !!saving;
    if (spinner) spinner.classList.toggle("d-none", !saving);
    if (text) text.textContent = saving ? "Saving..." : "Save Project";
  }

  function debounce(fn, wait) {
    let t = null;
    return function (...args) {
      if (t) clearTimeout(t);
      t = setTimeout(() => fn.apply(this, args), wait);
    };
  }

  function setOwnerValue(id, label) {
    if (!ownerInput || !id) return;
    const v = String(id);
    if (!ownerInput.querySelector(`option[value="${v}"]`)) {
      const opt = document.createElement("option");
      opt.value = v;
      opt.textContent = label || ("#" + v);
      ownerInput.appendChild(opt);
    }
    ownerInput.value = v;
    if (window.jQuery && jQuery.fn && jQuery.fn.select2) {
      jQuery(ownerInput).trigger('change');
    }
  }

  function renderFiles() {
    if (!filesList) return;
    if (!linkedFileIds.length) {
      filesList.innerHTML = "No files linked.";
      return;
    }
    filesList.innerHTML = linkedFileIds.map((id) => {
      const f = linkedFilesById[id] || {};
      const name = f.file_name || ("File #" + id);
      return `<div class="d-flex justify-content-between align-items-center mb-1"><span>${esc(name)}</span><button type="button" class="btn btn-sm btn-light text-danger pf-remove" data-id="${id}"><i class="ri-close-line"></i></button></div>`;
    }).join("");
  }

  async function uploadFiles() {
    if (!filesInput || !filesInput.files || !filesInput.files.length) return [];
    const ids = [];
    for (const file of Array.from(filesInput.files)) {
      const fd = new FormData();
      fd.append("file", file);
      const res = await fetch(apiFiles, { method: "POST", body: fd });
      const payload = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(payload.error || ("Upload failed for " + file.name));
      const fileId = payload.id ? Number(payload.id) : (payload.data && payload.data.id ? Number(payload.data.id) : 0);
      if (fileId > 0) {
        ids.push(fileId);
        linkedFilesById[fileId] = linkedFilesById[fileId] || { file_id: fileId, file_name: file.name };
      }
    }
    return ids;
  }

  async function loadExisting(id) {
    const res = await fetch(apiDetail + "?id=" + encodeURIComponent(id));
    const payload = await res.json().catch(() => ({}));
    if (!res.ok) throw new Error(payload.error || "Unable to load project");
    const data = payload.data || {};
    const project = data.project || {};

    if (heading) heading.textContent = "Edit Project";
    if (nameInput) nameInput.value = project.name || "";
    if (statusInput) statusInput.value = project.status || "planning";
    if (startDateInput) startDateInput.value = project.start_date || "";
    if (endDateInput) endDateInput.value = project.end_date || "";
    if (descriptionInput) descriptionInput.value = project.description || "";
    if (quill) {
      quill.setText("");
      const html = (project.description || "").trim();
      if (html) {
        quill.clipboard.dangerouslyPasteHTML(html);
      }
    }

    setOwnerValue(project.owner_user_id, project.owner_name || "");

    // Prefill members (staff)
    if (memberSelect) {
      const members = Array.isArray(data.members) ? data.members : [];
      memberSelect.innerHTML = "";
      members.forEach((m) => {
        const uid = String(m.user_id || "");
        if (!uid) return;
        const opt = document.createElement("option");
        opt.value = uid;
        opt.textContent = m.user_name || ("#" + uid);
        opt.selected = true;
        memberSelect.appendChild(opt);
      });
      if (window.jQuery && jQuery.fn && jQuery.fn.select2) {
        jQuery(memberSelect).trigger('change');
      }
    }

    linkedFileIds = [];
    linkedFilesById = {};
    (data.files || []).forEach((f) => {
      const fid = Number(f.file_id || f.id || 0);
      if (!fid) return;
      linkedFileIds.push(fid);
      linkedFilesById[fid] = f;
    });
    linkedFileIds = Array.from(new Set(linkedFileIds));
    renderFiles();
  }

  async function saveProject() {
    showAlert("");
    const id = (idInput && idInput.value || "").trim();
    if (!id && !canCreate) {
      showAlert("No permission to create project.");
      return;
    }
    if (id && !canUpdate) {
      showAlert("No permission to update project.");
      return;
    }

    const name = (nameInput && nameInput.value || "").trim();
    if (!name) {
      showAlert("Project name is required.");
      return;
    }

    const payload = {
      name,
      status: statusInput ? statusInput.value : "planning",
      owner_user_id: ownerInput && ownerInput.value ? Number(ownerInput.value) : null,
      start_date: startDateInput ? startDateInput.value : "",
      end_date: endDateInput ? endDateInput.value : "",
      description: (descriptionInput && descriptionInput.value || "").trim()
    };
    if (memberSelect) {
      payload.member_user_ids = Array.from(memberSelect.selectedOptions || [])
        .map((o) => parseInt(o.value, 10))
        .filter((v) => Number.isFinite(v) && v > 0);
    }

    setSaving(true);
    try {
      const uploaded = await uploadFiles();
      payload.file_ids = Array.from(new Set([...(linkedFileIds || []), ...uploaded]));

      const method = id ? "PATCH" : "POST";
      const url = id ? (apiDetail + "?id=" + encodeURIComponent(id)) : apiIndex;
      const res = await fetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });
      const out = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error(out.error || "Unable to save project");

      const nextId = id || out.id;
      if (!nextId) {
        window.location.href = "/studio/projects/index.kml";
        return;
      }
      window.location.href = "/studio/projects/view.kml?id=" + encodeURIComponent(out.id_s || nextId);
    } catch (e) {
      showAlert(e.message || "Unable to save project");
    } finally {
      setSaving(false);
    }
  }

  function wireEvents() {

    document.addEventListener("click", function (e) {
      const rm = e.target.closest(".pf-remove");
      if (!rm) return;
      e.preventDefault();
      const id = Number(rm.getAttribute("data-id"));
      linkedFileIds = linkedFileIds.filter((x) => x !== id);
      delete linkedFilesById[id];
      renderFiles();
    });

    if (saveBtn) saveBtn.addEventListener("click", saveProject);
  }

  function initEditor() {
    if (!descriptionEditorEl || !window.Quill) return;
    quill = new Quill(descriptionEditorEl, {
      theme: "snow",
      modules: {
        toolbar: [
          ["bold", "italic", "underline"],
          [{ list: "ordered" }, { list: "bullet" }],
          ["link"],
          ["clean"]
        ]
      }
    });
    quill.on("text-change", function () {
      if (!descriptionInput) return;
      descriptionInput.value = descriptionEditorEl.querySelector(".ql-editor") ? descriptionEditorEl.querySelector(".ql-editor").innerHTML : "";
    });
  }
  function initSelect2() {
    if (!ownerInput && !memberSelect) return;
    if (!window.jQuery || !jQuery.fn || !jQuery.fn.select2) {
      showAlert('Select2 is not loaded.');
      return;
    }

    const ajaxCfg = {
      transport: function (params, success, failure) {
        let url = params.url || '';
        try {
          const qp = new URLSearchParams();
          const data = (params && params.data && typeof params.data === 'object') ? params.data : {};
          Object.keys(data).forEach((k) => {
            const v = data[k];
            if (v === undefined || v === null || v === '') return;
            qp.append(k, String(v));
          });
          const qs = qp.toString();
          if (qs) url += (url.indexOf('?') >= 0 ? '&' : '?') + qs;
        } catch (e) {}

        fetch(url)
          .then((r) => r.json().then((j) => ({ ok: r.ok, json: j })))
          .then(({ ok, json }) => {
            if (!ok) throw new Error((json && json.error) || 'Unable to load staff');
            success(json);
          })
          .catch(failure);
      },
      delay: 250,
      data: function (params) {
        return {
          lookup: 'staff',
          limit: 20,
          q: (params.term || '').trim()
        };
      },
      processResults: function (data) {
        const rows = (data && data.data) ? data.data : [];
        return {
          results: rows.map((row) => ({
            id: String(row.id),
            text: row.label || ('#' + row.id)
          }))
        };
      }
    };

    if (ownerInput) {
      jQuery(ownerInput).select2({
        width: '100%',
        placeholder: 'Unassigned',
        allowClear: true,
        ajax: Object.assign({ url: apiIndex }, ajaxCfg)
      });
    }

    if (memberSelect) {
      jQuery(memberSelect).select2({
        width: '100%',
        placeholder: 'Select members',
        closeOnSelect: false,
        ajax: Object.assign({ url: apiIndex }, ajaxCfg)
      });
    }
  }

  async function init() {
    initEditor();
    initSelect2();

    const id = (idInput && idInput.value || "").trim();
    if (id) {
      try {
        await loadExisting(id);
      } catch (e) {
        showAlert(e.message || "Unable to load project");
      }
    }

    wireEvents();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
