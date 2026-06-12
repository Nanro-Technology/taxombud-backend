/* eslint-disable */
(function() {
      const appTaskConfig = window.appTaskConfig || {};
      const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
      const apiTasks = apiMap.tasksIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/tasks/index');
      const apiTaskDetailBase = apiMap.taskDetail || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/tasks/detail');
      const apiCases = apiMap.casesIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/cases/index');
      const apiExport = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/tasks/export';
      const canDelete = Boolean(appTaskConfig.canDelete);
      const canUpdate = Boolean(appTaskConfig.canUpdate);
      const timeTrackingEnabled = Boolean(appTaskConfig.timeTrackingEnabled);
      const timeTrackingCanUse = Boolean(appTaskConfig.timeTrackingCanUse || appTaskConfig.timeTrackingCanManage);
      const timeTrackingCanManage = Boolean(appTaskConfig.timeTrackingCanManage);
      const table = $('#taskTable').DataTable();
      const filterText = document.getElementById('filterText');
      const filterStatus = document.getElementById('filterStatus');
      const filterPriority = document.getElementById('filterPriority');
      const filterMine = document.getElementById('filterMine');
      const agentList = document.getElementById('agentList');
      const btnSearch = document.getElementById('btnSearch');
      const btnResetFilters = document.getElementById('btnResetFilters');
      const taskAlert = document.getElementById('taskAlert');
      const defaultAssignedToMe = Boolean(appTaskConfig.defaultAssignedToMe);
      const btnNewTask = document.getElementById('btnNewTask');
      const exportBtn = document.getElementById('taskExportBtn');
      const createModalEl = document.getElementById('taskCreateModal');
      const createForm = document.getElementById('taskCreateForm');
      const createAlert = document.getElementById('taskCreateAlert');
      const createSubmit = document.getElementById('taskCreateSubmit');
      const createCaseId = document.getElementById('taskCaseId');
      const createCaseStatus = document.getElementById('taskCaseSearchStatus');
      const createCaseSearch = document.getElementById('taskCaseSearch');
      const createCaseResults = document.getElementById('taskCaseResults');
      const createCaseSelected = document.getElementById('taskCaseSelected');
      const createStatus = createForm ? createForm.querySelector('#taskCreateStatus') : null;
      const modalInstance = createModalEl && typeof bootstrap !== 'undefined' ? new bootstrap.Modal(createModalEl) : null;
      const detailCanvasEl = document.getElementById('taskDetailCanvas');
      const detailCanvas = detailCanvasEl && typeof bootstrap !== 'undefined' ? new bootstrap.Offcanvas(detailCanvasEl) : null;
      const detailForm = document.getElementById('taskDetailForm');
      const detailAlert = document.getElementById('taskDetailAlert');
      const detailSave = document.getElementById('taskDetailSave');
      const detailCaseId = document.getElementById('detailCaseId');
      const detailCaseSearch = document.getElementById('detailCaseSearch');
      const detailCaseStatus = document.getElementById('detailCaseSearchStatus');
      const detailCaseResults = document.getElementById('detailCaseResults');
      const detailCaseSelected = document.getElementById('detailCaseSelected');
      const deleteModalEl = document.getElementById('taskDeleteModal');
      const deleteModal = deleteModalEl && typeof bootstrap !== 'undefined' ? new bootstrap.Modal(deleteModalEl) : null;
      const deleteTitle = document.getElementById('deleteTaskTitle');
      const confirmDeleteBtn = document.getElementById('confirmDeleteTask');
      const viewCanvasEl = document.getElementById('taskViewCanvas');
      const viewCanvas = viewCanvasEl && typeof bootstrap !== 'undefined' ? new bootstrap.Offcanvas(viewCanvasEl) : null;
      const viewTitle = document.getElementById('taskViewTitle');
      const viewStatus = document.getElementById('taskViewStatus');
      const viewPriority = document.getElementById('taskViewPriority');
      const viewAssignee = document.getElementById('taskViewAssignee');
      const viewDueDate = document.getElementById('taskViewDueDate');
      const viewDueBadge = document.getElementById('taskViewDueBadge');
      const viewDescription = document.getElementById('taskViewDescription');
      const viewMeta = document.getElementById('taskViewMeta');
      const viewEditBtn = document.getElementById('taskViewEditBtn');
      const viewAlert = document.getElementById('taskViewAlert');
      const viewCase = document.getElementById('taskViewCase');
      const minCaseSearchLen = 4;
      let deleteTaskId = null;
      const taskTimeTrackingRoot = document.getElementById('taskTimeTracking');
      let taskTimeTracking = null;

      if (timeTrackingEnabled && window.TimeTracking && taskTimeTrackingRoot) {
        taskTimeTracking = window.TimeTracking.init(taskTimeTrackingRoot, {
          entityType: 'task',
          entityId: null,
          canUse: timeTrackingCanUse,
          canManage: timeTrackingCanManage
        });
      }

      function showTaskAlert(msg) {
        if (!taskAlert) return;
        taskAlert.textContent = msg || '';
      }

      async function loadTaskStatuses() {
        const selects = [filterStatus, createStatus, document.getElementById('detailStatus')];
        const sourceEl = selects.find(el => el && el.dataset.statusSource);
        const src = sourceEl ? sourceEl.dataset.statusSource : 'assets/json/task_statuses.json';
        try {
          const res = await fetch(src);
          const list = await res.json();
          if (!Array.isArray(list)) return;
          selects.forEach((el) => {
            if (!el) return;
            const includeAll = el === filterStatus;
            el.innerHTML = includeAll ? '<option value=\"\">All status</option>' : '';
            list.forEach((st) => {
              const opt = document.createElement('option');
              opt.value = st.code || '';
              opt.textContent = st.label || st.code || '';
              el.appendChild(opt);
            });
            // keep current value if exists
            const desired = el.dataset.defaultValue || (includeAll ? '' : 'open');
            if (desired) el.value = desired;
          });
        } catch (e) {
          // fall back silently
        }
      }

      function formatStatus(status) {
        const key = (status || '').toLowerCase();
        const map = {
          open: 'badge bg-success-subtle text-success',
          in_progress: 'badge bg-info-subtle text-info',
          blocked: 'badge bg-warning-subtle text-warning',
          done: 'badge bg-primary-subtle text-primary',
          cancelled: 'badge bg-secondary-subtle text-secondary'
        };
        return `<span class="${map[key] || 'badge bg-light text-muted'} text-uppercase">${status || 'N/A'}</span>`;
      }

      function formatPriority(p) {
        const key = (p || '').toLowerCase();
        const map = {
          urgent: 'badge bg-danger',
          high: 'badge bg-warning',
          medium: 'badge bg-info',
          low: 'badge bg-secondary'
        };
        return `<span class="${map[key] || 'badge bg-light text-muted'} text-uppercase">${p || 'N/A'}</span>`;
      }

      function formatDue(due, status) {
        if (!due) return '<span class="text-muted">N/A</span>';
        const dateObj = new Date(due.replace(' ', 'T'));
        if (Number.isNaN(dateObj.getTime())) return due;
        const keyStatus = (status || '').toLowerCase();
        const now = new Date();
        const diffMs = dateObj.getTime() - now.getTime();
        const days = Math.round(diffMs / (1000 * 60 * 60 * 24));
        const isDone = keyStatus === 'done';
        const isCancelled = keyStatus === 'cancelled';
        if (isDone) {
          const friendlyDone = `${dateObj.toLocaleDateString()} ${dateObj.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'})}`;
          return `<div class="d-flex flex-column"><span>${friendlyDone}</span><small class="text-muted"><span class="badge bg-primary-subtle text-primary">Done</span></small></div>`;
        }
        if (isCancelled) {
          const friendlyCancelled = `${dateObj.toLocaleDateString()} ${dateObj.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'})}`;
          return `<div class="d-flex flex-column"><span>${friendlyCancelled}</span><small class="badge bg-secondary-subtle text-secondary">Cancelled</small></div>`;
        }
        let badge = '<span class="badge bg-secondary-subtle text-secondary">--</span>';
        if (days > 1) badge = `<span class="badge bg-success-subtle text-success">${days} days left</span>`;
        else if (days === 1) badge = '<span class="badge bg-success-subtle text-success">Tomorrow</span>';
        else if (days === 0) badge = '<span class="badge bg-warning-subtle text-warning">Today</span>';
        else badge = `<span class="badge bg-danger-subtle text-danger">${Math.abs(days)} day${Math.abs(days) === 1 ? '' : 's'} overdue</span>`;
        const friendly = `${dateObj.toLocaleDateString()} ${dateObj.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'})}`;
        return `<div class="d-flex flex-column"><span>${friendly}</span><small class="text-muted">${badge}</small></div>`;
      }

      function formatDueParts(due, status) {
        if (!due) return { date: 'N/A', badge: '<span class="badge bg-secondary-subtle text-secondary">--</span>' };
        const dateObj = new Date(due.replace(' ', 'T'));
        if (Number.isNaN(dateObj.getTime())) return { date: due, badge: '<span class="badge bg-secondary-subtle text-secondary">--</span>' };
        const keyStatus = (status || '').toLowerCase();
        const now = new Date();
        const diffMs = dateObj.getTime() - now.getTime();
        const days = Math.round(diffMs / (1000 * 60 * 60 * 24));
        if (keyStatus === 'done') {
          return { date: `${dateObj.toLocaleDateString()} ${dateObj.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'})}`, badge: '<span class="badge bg-primary-subtle text-primary">Done</span>' };
        }
        if (keyStatus === 'cancelled') {
          return { date: `${dateObj.toLocaleDateString()} ${dateObj.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'})}`, badge: '<span class="badge bg-secondary-subtle text-secondary">Cancelled</span>' };
        }
        let badge = '<span class="badge bg-secondary-subtle text-secondary">--</span>';
        if (days > 1) badge = `<span class="badge bg-success-subtle text-success">${days} days left</span>`;
        else if (days === 1) badge = '<span class="badge bg-success-subtle text-success">Tomorrow</span>';
        else if (days === 0) badge = '<span class="badge bg-warning-subtle text-warning">Today</span>';
        else badge = `<span class="badge bg-danger-subtle text-danger">${Math.abs(days)} day${Math.abs(days) === 1 ? '' : 's'} overdue</span>`;
        const friendly = `${dateObj.toLocaleDateString()} ${dateObj.toLocaleTimeString([], {hour: '2-digit', minute: '2-digit'})}`;
        return { date: friendly, badge };
      }

      function caseLabel(c) {
        if (!c) return '';
        if (c.case_number) {
          return c.case_number + (c.subject ? ' — ' + c.subject : '');
        }
        return c.subject || ('Case #' + (c.id || ''));
      }

      function setCaseSearchStatus(statusEl, state, message) {
        if (!statusEl) return;
        const msg = message || '';
        statusEl.textContent = msg;
        statusEl.classList.toggle('text-danger', state === 'empty');
        statusEl.classList.toggle('text-muted', state !== 'empty');
      }

      function renderCaseResults(list, inputEl, hiddenEl, selectedEl, resultsEl, statusEl) {
        if (!resultsEl || !inputEl || !hiddenEl) return;
        resultsEl.innerHTML = '';
        if (!list || !list.length) {
          resultsEl.classList.add('d-none');
          setCaseSearchStatus(statusEl, 'empty', 'No result found.');
          return;
        }
        setCaseSearchStatus(statusEl, 'ready', '');
        (list || []).forEach(cs => {
          const btn = document.createElement('button');
          btn.type = 'button';
          btn.className = 'list-group-item list-group-item-action';
          btn.innerHTML = `<div class="fw-semibold">${caseLabel(cs)}</div><div class="small text-muted">Status: ${cs.status || 'N/A'} • Priority: ${cs.priority || ''}</div>`;
          btn.addEventListener('click', () => {
            hiddenEl.value = cs.id || '';
            inputEl.value = caseLabel(cs);
            if (selectedEl) selectedEl.textContent = hiddenEl.value ? ('Selected: ' + caseLabel(cs)) : '';
            resultsEl.classList.add('d-none');
            setCaseSearchStatus(statusEl, 'ready', '');
          });
          resultsEl.appendChild(btn);
        });
        resultsEl.classList.toggle('d-none', !resultsEl.innerHTML);
      }

      function searchCases(term, inputEl, hiddenEl, selectedEl, resultsEl, statusEl) {
        if (!term || term.length < minCaseSearchLen) {
          if (resultsEl) resultsEl.classList.add('d-none');
          setCaseSearchStatus(statusEl, 'idle', 'Enter 4 or more characters to search.');
          return;
        }
        setCaseSearchStatus(statusEl, 'searching', 'Searching...');
        const qs = new URLSearchParams({ q: term, limit: '3' });
        fetch(apiCases + '?' + qs.toString())
          .then(async (r) => {
            if (r.status === 403) {
              const qsMine = new URLSearchParams({ q: term, limit: '3', assigned_to_me: '1' });
              return fetch(apiCases + '?' + qsMine.toString());
            }
            return r;
          })
          .then(r => { if (!r.ok) throw new Error(); return r.json(); })
          .then(data => renderCaseResults(data.data || [], inputEl, hiddenEl, selectedEl, resultsEl, statusEl))
          .catch(() => {
            if (resultsEl) resultsEl.classList.add('d-none');
            setCaseSearchStatus(statusEl, 'empty', 'No result found.');
          });
      }

      function buildQuery() {
        const params = new URLSearchParams();
        const q = filterText ? filterText.value.trim() : '';
        if (q) params.set('q', q);
        if (filterStatus && filterStatus.value) params.set('status', filterStatus.value);
        if (filterPriority && filterPriority.value) params.set('priority', filterPriority.value);
        if (agentList && agentList.value) params.set('agent_id', agentList.value);
        if (filterMine) {
          params.set('assigned_to_me', filterMine.checked ? '1' : '0');
        }
        params.set('limit', '200');
        return params.toString() ? ('?' + params.toString()) : '';
      }

      function buildExportParams() {
        const params = new URLSearchParams();
        const q = filterText ? filterText.value.trim() : '';
        if (q) params.set('q', q);
        if (filterStatus && filterStatus.value) params.set('status', filterStatus.value);
        if (filterPriority && filterPriority.value) params.set('priority', filterPriority.value);
        if (agentList && agentList.value) params.set('agent_id', agentList.value);
        if (filterMine) {
          params.set('assigned_to_me', filterMine.checked ? '1' : '0');
        }
        return params;
      }

      function renderTable(rows) {
        table.clear();
        (rows || []).forEach((r, idx) => {
          table.row.add([
            idx + 1,
            r.title || '',
            formatStatus(r.status),
            formatPriority(r.priority),
            r.assignee_name || '',
            formatDue(r.due_at, r.status),
            `<div class="btn-group">
              <button class="btn btn-soft-primary btn-sm btn-view-task" data-task-id="${r.id}" title="View"><i class="ri-eye-line me-1"></i>View</button>
              ${canUpdate ? `<button class="btn btn-soft-info btn-sm btn-edit-task" data-task-id="${r.id}" title="Edit"><i class="ri-edit-line me-1"></i>Edit</button>` : ''}
              ${canDelete ? `<button class="btn btn-soft-danger btn-sm btn-delete-task" data-task-id="${r.id}" title="Delete"><i class="ri-delete-bin-line"></i></button>` : ''}
            </div>`
          ]);
        });
        table.draw();
      }

      function openDeleteModal(taskId, title) {
        deleteTaskId = taskId;
        if (deleteTitle) deleteTitle.textContent = title || '';
        if (deleteModal) deleteModal.show();
      }

      function deleteTask(taskId) {
        if (!taskId) return;
        showTaskAlert('');
        const url = apiTaskDetailBase + '?id=' + encodeURIComponent(taskId);
        fetch(url, { method: 'DELETE' })
          .then(async (resp) => {
            const data = await resp.json().catch(() => ({}));
            if (!resp.ok) throw new Error(data.error || 'Unable to delete task');
            return data;
          })
          .then(() => {
            showTaskAlert('Task deleted.');
            loadTasks();
          })
          .catch((err) => showTaskAlert(err.message || 'Unable to delete task'))
          .finally(() => {
            if (deleteModal) deleteModal.hide();
            deleteTaskId = null;
          });
      }

      function loadTasks() {
        const params = buildQuery();
        fetch(apiTasks + params)
          .then(r => {
            if (!r.ok) throw new Error('Unable to load tasks');
            return r.json();
          })
          .then(data => {
            const rows = data.data || [];
            // Ensure tasks with status 'done' appear at the bottom of the table
            rows.sort((a, b) => {
              const aDone = ((a.status || '').toLowerCase() === 'done') ? 1 : 0;
              const bDone = ((b.status || '').toLowerCase() === 'done') ? 1 : 0;
              return aDone - bDone; // non-done (0) before done (1)
            });
            renderTable(rows);
          })
          .catch(() => showTaskAlert('Unable to load tasks'));
      }

      if (btnSearch) btnSearch.addEventListener('click', loadTasks);
      if (filterText) filterText.addEventListener('keypress', (e) => { if (e.key === 'Enter') { e.preventDefault(); loadTasks(); }});
      if (filterStatus) filterStatus.addEventListener('change', loadTasks);
      if (filterPriority) filterPriority.addEventListener('change', loadTasks);
      if (agentList) agentList.addEventListener('change', loadTasks);
      if (filterMine) filterMine.addEventListener('change', loadTasks);
      if (btnResetFilters) {
        btnResetFilters.addEventListener('click', () => {
          if (filterText) filterText.value = '';
          if (filterStatus) filterStatus.value = '';
          if (filterPriority) filterPriority.value = '';
          if (agentList) agentList.value = '';
          if (filterMine) filterMine.checked = defaultAssignedToMe;
          loadTasks();
        });
      }
      if (exportBtn) {
        exportBtn.addEventListener('click', () => {
          const params = buildExportParams();
          const url = apiExport + (params.toString() ? ('?' + params.toString()) : '');
          window.location.href = url;
        });
      }
      if (filterMine && defaultAssignedToMe) {
        filterMine.checked = true;
      }
      loadTaskStatuses().finally(() => loadTasks());

      if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', () => deleteTask(deleteTaskId));
      }

      $('#taskTable').on('click', '.btn-delete-task', function() {
        const taskId = this.getAttribute('data-task-id');
        const rowTitle = $(this).closest('tr').find('td:nth-child(2)').text().trim();
        openDeleteModal(taskId, rowTitle);
      });
      $('#taskTable').on('click', '.btn-view-task', function() {
        const taskId = this.getAttribute('data-task-id');
        const btn = this;
        const icon = btn.querySelector('i');
        const original = btn.innerHTML;
        btn.disabled = true;
        if (icon) icon.classList.add('d-none');
        const spinner = document.createElement('span');
        spinner.className = 'spinner-border spinner-border-sm me-1';
        spinner.setAttribute('role', 'status');
        spinner.setAttribute('aria-hidden', 'true');
        btn.prepend(spinner);
        openTaskView(taskId).finally(() => {
          btn.disabled = false;
          spinner.remove();
          if (icon) icon.classList.remove('d-none');
          btn.innerHTML = original;
        });
      });
      $('#taskTable').on('click', '.btn-edit-task', function() {
        const taskId = this.getAttribute('data-task-id');
        const btn = this;
        const icon = btn.querySelector('i');
        const original = btn.innerHTML;
        btn.disabled = true;
        if (icon) icon.classList.add('d-none');
        const spinner = document.createElement('span');
        spinner.className = 'spinner-border spinner-border-sm me-1';
        spinner.setAttribute('role', 'status');
        spinner.setAttribute('aria-hidden', 'true');
        btn.prepend(spinner);
        openTaskDetail(taskId).finally(() => {
          btn.disabled = false;
          spinner.remove();
          if (icon) icon.classList.remove('d-none');
          btn.innerHTML = original;
        });
      });

      function showCreateAlert(msg, isError = true) {
        if (!createAlert) return;
        createAlert.textContent = msg || '';
        createAlert.classList.toggle('text-danger', !!isError);
        createAlert.classList.toggle('text-success', !isError);
      }

      function resetCreateForm() {
        if (!createForm) return;
        createForm.reset();
        showCreateAlert('');
            const statusField = createForm.querySelector('[name="status"]');
            const priorityField = createForm.querySelector('[name="priority"]');
            if (statusField) statusField.value = 'open';
            if (priorityField) priorityField.value = 'medium';
        createForm.classList.remove('was-validated');
        if (createCaseId) createCaseId.value = '';
        if (createCaseSearch) createCaseSearch.value = '';
        if (createCaseSelected) createCaseSelected.textContent = '';
        if (createCaseResults) createCaseResults.classList.add('d-none');
        setCaseSearchStatus(createCaseStatus, 'idle', 'Enter 4 or more characters to search.');
      }

      if (btnNewTask && modalInstance) {
        btnNewTask.addEventListener('click', () => {
          resetCreateForm();
          modalInstance.show();
        });
      }

      if (createCaseSearch) {
        createCaseSearch.addEventListener('input', () => {
          const term = createCaseSearch.value.trim();
          if (!term) {
            if (createCaseId) createCaseId.value = '';
            if (createCaseSelected) createCaseSelected.textContent = '';
          }
          searchCases(term, createCaseSearch, createCaseId, createCaseSelected, createCaseResults, createCaseStatus);
        });
      }
      if (detailCaseSearch) {
        detailCaseSearch.addEventListener('input', () => {
          const term = detailCaseSearch.value.trim();
          if (!term) {
            if (detailCaseId) detailCaseId.value = '';
            if (detailCaseSelected) detailCaseSelected.textContent = '';
          }
          searchCases(term, detailCaseSearch, detailCaseId, detailCaseSelected, detailCaseResults, detailCaseStatus);
        });
      }

      function populateDetail(task) {
        if (!task) return;
        document.getElementById('detailTaskId').value = task.id || '';
        document.getElementById('detailTitle').value = task.title || '';
        document.getElementById('detailDescription').value = task.description || '';
        document.getElementById('detailStatus').value = task.status || 'open';
        document.getElementById('detailPriority').value = task.priority || 'medium';
        document.getElementById('detailAgent').value = task.agent_id || '';
        const dueInput = document.getElementById('detailDueAt');
        if (dueInput) {
          if (task.due_at) {
            const iso = task.due_at.replace(' ', 'T');
            dueInput.value = iso.length === 16 ? iso : iso.slice(0, 16);
          } else {
            dueInput.value = '';
          }
        }
        if (detailCaseId) detailCaseId.value = task.case_id || '';
        const caseLabelText = task.case_subject ? caseLabel(task) : (task.case_id ? ('Case #' + task.case_id) : '');
        if (detailCaseSearch) detailCaseSearch.value = caseLabelText;
        if (detailCaseSelected) detailCaseSelected.textContent = caseLabelText ? ('Selected: ' + caseLabelText) : '';
        if (detailCaseResults) detailCaseResults.classList.add('d-none');
        setCaseSearchStatus(detailCaseStatus, detailCaseId && detailCaseId.value ? 'ready' : 'idle', detailCaseId && detailCaseId.value ? '' : 'Enter 4 or more characters to search.');
      }

      function openTaskDetail(taskId) {
        if (!taskId) return Promise.resolve();
        detailAlert && (detailAlert.textContent = '');
        const url = apiTaskDetailBase + '?id=' + encodeURIComponent(taskId);
        return fetch(url)
          .then(async (resp) => {
            const data = await resp.json().catch(() => ({}));
            if (!resp.ok) throw new Error(data.error || 'Unable to load task');
            return data;
          })
          .then((task) => {
            populateDetail(task);
            if (detailCanvas) detailCanvas.show();
          })
          .catch((err) => {
            showTaskAlert(err.message || 'Unable to load task detail');
          });
      }

      function populateView(task) {
        if (!task) return;
        const dueParts = formatDueParts(task.due_at || '', task.status);
        if (viewTitle) viewTitle.textContent = task.title || 'Task';
        if (viewStatus) viewStatus.innerHTML = formatStatus(task.status);
        if (viewPriority) viewPriority.innerHTML = formatPriority(task.priority);
        if (viewAssignee) viewAssignee.textContent = task.assignee_name || 'Unassigned';
        if (viewDueDate) viewDueDate.textContent = dueParts.date;
        if (viewDueBadge) viewDueBadge.innerHTML = dueParts.badge;
        if (viewDescription) viewDescription.textContent = task.description || '—';
        if (viewCase) {
          const baseCase = task.case_number
            ? 'Case ' + task.case_number
            : (task.case_id ? ('Case #' + task.case_id) : '');
          const label = baseCase
            ? (task.case_subject ? baseCase + ' — ' + task.case_subject : baseCase)
            : '—';
          viewCase.textContent = '';
          if (task.case_id) {
            const link = document.createElement('a');
            link.href = 'studio/cases/view.kml?id=' + encodeURIComponent(String(task.case_id));
            link.className = 'link-primary text-decoration-underline';
            link.textContent = label || ('Case #' + task.case_id);
            viewCase.appendChild(link);
          } else {
            viewCase.textContent = label || '—';
          }
        }
        if (viewMeta) viewMeta.innerHTML = `
          <div>Created by: <span class="fw-semibold">${task.created_by_name || 'N/A'}</span></div>
          <div>Created at: ${task.created_at || 'N/A'}</div>
          <div>Updated at: ${task.updated_at || 'N/A'}</div>
        `;
      }

      function normalizeDue(value) {
        if (!value) return '';
        const replaced = value.replace('T', ' ').trim();
        if (replaced.length === 16) return replaced + ':00';
        return replaced;
      }

      function openTaskView(taskId) {
        if (!taskId) return Promise.resolve();
        if (viewAlert) viewAlert.textContent = '';
        const url = apiTaskDetailBase + '?id=' + encodeURIComponent(taskId);
        return fetch(url)
          .then(async (resp) => {
            const data = await resp.json().catch(() => ({}));
            if (!resp.ok) throw new Error(data.error || 'Unable to load task');
            return data;
          })
          .then((task) => {
            populateView(task);
            if (viewEditBtn) {
              viewEditBtn.dataset.taskId = task.id;
              viewEditBtn.style.display = 'block';
            }
            if (taskTimeTracking) {
              taskTimeTracking.setEntity('task', task.id);
            }
            if (viewCanvas) viewCanvas.show();
          })
          .catch((err) => {
            if (viewAlert) viewAlert.textContent = err.message || 'Unable to load task detail';
          });
      }

      if (viewEditBtn) {
        viewEditBtn.addEventListener('click', () => {
          const tid = viewEditBtn.dataset.taskId;
          if (tid) {
            openTaskDetail(tid);
            if (viewCanvas) viewCanvas.hide();
          }
        });
      }

      if (detailForm) {
        detailForm.addEventListener('submit', (e) => {
          e.preventDefault();
          const taskId = document.getElementById('detailTaskId').value;
          if (!taskId) return;
          const fd = new FormData(detailForm);
          const payload = {
            title: (fd.get('title') || '').toString().trim(),
            description: (fd.get('description') || '').toString().trim(),
            status: (fd.get('status') || '').toString(),
            priority: (fd.get('priority') || '').toString(),
            due_at: normalizeDue((fd.get('due_at') || '').toString()),
            agent_id: fd.get('agent_id') ? parseInt(fd.get('agent_id'), 10) : null,
            case_id: fd.get('case_id') ? parseInt(fd.get('case_id'), 10) : null
          };
          if (!payload.title) {
            detailForm.classList.add('was-validated');
            return;
          }
          detailAlert && (detailAlert.textContent = '');
          if (detailSave) detailSave.disabled = true;
          const url = apiTaskDetailBase + '?id=' + encodeURIComponent(taskId);
          fetch(url, {
            method: 'PATCH',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(payload)
          })
            .then(async (resp) => {
              const data = await resp.json().catch(() => ({}));
              if (!resp.ok) throw new Error(data.error || 'Unable to update task');
              return data;
            })
            .then(() => {
              if (detailCanvas) detailCanvas.hide();
              loadTasks();
            })
            .catch((err) => {
              if (detailAlert) detailAlert.textContent = err.message || 'Unable to update task';
            })
            .finally(() => {
              if (detailSave) detailSave.disabled = false;
            });
        });
      }

      if (createForm) {
        createForm.addEventListener('submit', (e) => {
          e.preventDefault();
          if (!createForm.checkValidity()) {
            createForm.classList.add('was-validated');
            return;
          }
          const fd = new FormData(createForm);
          const payload = {
            title: (fd.get('title') || '').toString().trim(),
            description: (fd.get('description') || '').toString().trim(),
            status: (fd.get('status') || 'open').toString(),
            priority: (fd.get('priority') || 'medium').toString(),
            due_at: normalizeDue((fd.get('due_at') || '').toString()),
            agent_id: fd.get('agent_id') ? parseInt(fd.get('agent_id'), 10) : null,
            case_id: fd.get('case_id') ? parseInt(fd.get('case_id'), 10) : null
          };
          showCreateAlert('');
          if (createSubmit) createSubmit.disabled = true;
          fetch(apiTasks, {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(payload)
          })
            .then(async (resp) => {
              const data = await resp.json().catch(() => ({}));
              if (!resp.ok) {
                throw new Error(data.error || 'Unable to create task');
              }
              return data;
            })
            .then(() => {
              showCreateAlert('Task created', false);
              if (modalInstance) modalInstance.hide();
              resetCreateForm();
              loadTasks();
            })
            .catch((err) => {
              showCreateAlert(err.message || 'Unable to create task');
            })
            .finally(() => {
              if (createSubmit) createSubmit.disabled = false;
            });
        });
      }

      function maybeOpenFromUrl() {
        const params = new URLSearchParams(window.location.search);
        const open = params.get('open');
        const id = params.get('id');
        if (open === 'task' && id) {
          openTaskView(id);
        }
      }

      if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', maybeOpenFromUrl);
      } else {
        maybeOpenFromUrl();
      }
    })();
