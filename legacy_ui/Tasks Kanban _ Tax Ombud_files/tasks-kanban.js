/* eslint-disable */
(function() {
      const appTaskConfig = window.appTaskConfig || {};
      const apiTasks = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/tasks/index';
      const apiTaskDetailBase = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/tasks/detail';
      const apiCases = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/cases/index';
      const timeTrackingEnabled = Boolean(appTaskConfig.timeTrackingEnabled);
      const timeTrackingCanUse = Boolean(appTaskConfig.timeTrackingCanUse || appTaskConfig.timeTrackingCanManage);
      const timeTrackingCanManage = Boolean(appTaskConfig.timeTrackingCanManage);
      const columnsWrap = document.getElementById('kanbanColumns');
      const alertBox = document.getElementById('kanbanAlert');
      const searchBox = document.getElementById('kanbanSearch');
      const taskStatusSrc = (typeof url_root !== 'undefined' ? url_root : '../') + 'assets/json/task_statuses.json';
      let statusOrder = [
        { key: 'open', label: 'Open' },
        { key: 'in_progress', label: 'In Progress' },
        { key: 'blocked', label: 'Blocked' },
        { key: 'cancelled', label: 'Cancelled' }
      ];
      let allStatuses = [...statusOrder, { key: 'done', label: 'Done' }];
      let currentTasks = [];
      const viewCanvasEl = document.getElementById('taskViewCanvas');
      const viewCanvas = viewCanvasEl ? new bootstrap.Offcanvas(viewCanvasEl) : null;
      const viewTitle = document.getElementById('taskViewTitle');
      const viewStatus = document.getElementById('taskViewStatus');
      const viewPriority = document.getElementById('taskViewPriority');
      const viewAssignee = document.getElementById('taskViewAssignee');
      const viewDueDate = document.getElementById('taskViewDueDate');
      const viewDueBadge = document.getElementById('taskViewDueBadge');
      const viewDescription = document.getElementById('taskViewDescription');
      const viewCase = document.getElementById('taskViewCase');
      const viewMeta = document.getElementById('taskViewMeta');
      const viewAlert = document.getElementById('taskViewAlert');
      const viewEditBtn = document.getElementById('taskViewEditBtn');
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
      const createForm = document.getElementById('taskCreateForm');
      const createModalEl = document.getElementById('taskCreateModal');
      const createModal = createModalEl ? new bootstrap.Modal(createModalEl) : null;
      const createAlert = document.getElementById('taskCreateAlert');
      const createStatus = document.getElementById('taskCreateStatus');
      const createTitleEl = createModalEl ? createModalEl.querySelector('.modal-title') : null;
      const createSubmitBtn = document.getElementById('taskCreateSubmit');
      const createEditId = document.getElementById('taskEditId');
      const caseSearch = document.getElementById('taskCaseSearch');
      const caseResults = document.getElementById('taskCaseResults');
      const caseSelected = document.getElementById('taskCaseSelected');
      const caseIdInput = document.getElementById('taskCaseId');

      const formatPriority = (p) => {
        const key = (p || '').toLowerCase();
        const map = {
          urgent: 'badge bg-danger',
          high: 'badge bg-warning',
          medium: 'badge bg-info',
          low: 'badge bg-secondary'
        };
        return `<span class="${map[key] || 'badge bg-light text-muted'} text-uppercase">${p || 'N/A'}</span>`;
      };

      const formatDue = (due, status) => {
        if (!due) return '<span class="text-muted">No due date</span>';
        const dt = new Date(due.replace(' ', 'T'));
        if (Number.isNaN(dt.getTime())) return due;
        const keyStatus = (status || '').toLowerCase();
        const base = `${dt.toLocaleDateString()} ${dt.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
        if (keyStatus === 'done') return `${base} <span class="badge bg-primary-subtle text-primary ms-1">Done</span>`;
        if (keyStatus === 'cancelled') return `${base} <span class="badge bg-secondary-subtle text-secondary ms-1">Cancelled</span>`;
        const diffDays = Math.round((dt.getTime() - Date.now()) / (1000 * 60 * 60 * 24));
        if (diffDays > 1) return `${base} <span class="badge bg-success-subtle text-success ms-1">${diffDays} days left</span>`;
        if (diffDays === 1) return `${base} <span class="badge bg-success-subtle text-success ms-1">Tomorrow</span>`;
        if (diffDays === 0) return `${base} <span class="badge bg-warning-subtle text-warning ms-1">Today</span>`;
        return `${base} <span class="badge bg-danger-subtle text-danger ms-1">${Math.abs(diffDays)} day${Math.abs(diffDays) === 1 ? '' : 's'} overdue</span>`;
      };

      const formatStatus = (status) => {
        const key = (status || '').toLowerCase();
        const map = {
          open: 'badge bg-success-subtle text-success',
          in_progress: 'badge bg-info-subtle text-info',
          blocked: 'badge bg-warning-subtle text-warning',
          done: 'badge bg-primary-subtle text-primary',
          cancelled: 'badge bg-secondary-subtle text-secondary'
        };
        return `<span class="${map[key] || 'badge bg-light text-muted'} text-uppercase">${status || 'N/A'}</span>`;
      };

      const formatDueParts = (due, status) => {
        if (!due) return { date: 'N/A', badge: '<span class="badge bg-secondary-subtle text-secondary">--</span>' };
        const dt = new Date(due.replace(' ', 'T'));
        if (Number.isNaN(dt.getTime())) return { date: due, badge: '<span class="badge bg-secondary-subtle text-secondary">--</span>' };
        const keyStatus = (status || '').toLowerCase();
        const base = `${dt.toLocaleDateString()} ${dt.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
        if (keyStatus === 'done') return { date: base, badge: '<span class="badge bg-primary-subtle text-primary">Done</span>' };
        if (keyStatus === 'cancelled') return { date: base, badge: '<span class="badge bg-secondary-subtle text-secondary">Cancelled</span>' };
        const diffDays = Math.round((dt.getTime() - Date.now()) / (1000 * 60 * 60 * 24));
        if (diffDays > 1) return { date: base, badge: `<span class="badge bg-success-subtle text-success">${diffDays} days left</span>` };
        if (diffDays === 1) return { date: base, badge: '<span class="badge bg-success-subtle text-success">Tomorrow</span>' };
        if (diffDays === 0) return { date: base, badge: '<span class="badge bg-warning-subtle text-warning">Today</span>' };
        return { date: base, badge: `<span class="badge bg-danger-subtle text-danger">${Math.abs(diffDays)} day${Math.abs(diffDays) === 1 ? '' : 's'} overdue</span>` };
      };

      const cardTemplate = (task) => {
        const caseLabel = task.case_number ? `Case ${task.case_number}${task.case_subject ? ' — ' + task.case_subject : ''}` : (task.case_subject || '');
        return `<div class="card tasks-box mb-2 draggable-task" draggable="true" data-task-id="${task.id}" data-task-status="${task.status}">
          <div class="card-body">
            <div class="d-flex mb-1 align-items-start">
              <h6 class="fs-15 mb-0 flex-grow-1 text-truncate task-title">${task.title || 'Task'}</h6>
              <div class="dropdown">
                <button class="btn btn-sm btn-ghost-secondary" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                  <i class="ri-more-2-fill"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end">
                  <li><button class="dropdown-item kanban-view-task" data-task-id="${task.id}" type="button"><i class="ri-eye-line me-1"></i>View</button></li>
                  <li><button class="dropdown-item kanban-done-task" data-task-id="${task.id}" type="button"><i class="ri-check-line me-1"></i>Mark as Done</button></li>
                </ul>
              </div>
            </div>
            <div class="text-muted small mb-2">${task.description || ''}</div>
            <div class="d-flex align-items-center mb-2">
              <div class="flex-grow-1">${formatPriority(task.priority)}</div>
              <div class="flex-shrink-0 small text-muted">${task.assignee_name || 'Unassigned'}</div>
            </div>
            <div class="small text-muted mb-1">${formatDue(task.due_at, task.status)}</div>
            ${caseLabel ? `<div class="small text-primary">${caseLabel}</div>` : ''}
          </div>
        </div>`;
      };

      const renderColumns = (tasks) => {
        if (!columnsWrap) return;
        const term = (searchBox?.value || '').trim().toLowerCase();
        columnsWrap.innerHTML = '';
        const maxPerColumn = 10;
        statusOrder.forEach((st) => {
          const colTasks = (tasks || []).filter(t => (t.status || '').toLowerCase() === st.key && t.status !== 'closed');
          const filtered = term ? colTasks.filter(t => (t.title || '').toLowerCase().includes(term) || (t.description || '').toLowerCase().includes(term)) : colTasks;
          const badge = `<small class="badge bg-light text-muted align-bottom ms-1">${filtered.length}</small>`;
          const subset = filtered.slice(0, maxPerColumn);
          const list = subset.length ? subset.map(cardTemplate).join('') : '<div class="text-muted small">No tasks</div>';
          const col = document.createElement('div');
          col.className = 'col-12 col-md-6 col-xl-4 col-xxl-3';
          col.innerHTML = `<div class="tasks-list card h-100 dropzone" data-status="${st.key}">
              <div class="card-header d-flex align-items-center justify-content-between">
                <h6 class="fs-14 text-uppercase fw-semibold mb-0">${st.label} ${badge}</h6>
              </div>
              <div class="card-body" data-simplebar style="max-height: 70vh">
                ${list}
              </div>
            </div>`;
          columnsWrap.appendChild(col);
        });
      };

      const fetchTasks = () => {
        if (alertBox) alertBox.textContent = '';
        const qs = new URLSearchParams({ limit: '500' });
        fetch(apiTasks + '?' + qs.toString())
          .then(r => r.json().then(data => ({ ok: r.ok, data })))
          .then(({ ok, data }) => {
            if (!ok) throw new Error(data?.error || 'Unable to load tasks');
            currentTasks = data.data || [];
            renderColumns(currentTasks);
            bindDragHandlers();
          })
          .catch(err => { if (alertBox) alertBox.textContent = err.message || 'Unable to load tasks'; });
      };

      if (searchBox) {
        searchBox.addEventListener('input', () => fetchTasks());
      }

      function bindDragHandlers() {
        const cards = document.querySelectorAll('.draggable-task');
        const zones = document.querySelectorAll('.dropzone');
        cards.forEach(card => {
          card.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', card.dataset.taskId);
            e.dataTransfer.effectAllowed = 'move';
            card.classList.add('opacity-50');
          });
          card.addEventListener('dragend', () => card.classList.remove('opacity-50'));
        });
        zones.forEach(zone => {
          zone.addEventListener('dragover', (e) => {
            e.preventDefault();
            zone.classList.add('border', 'border-primary', 'border-dashed');
          });
          zone.addEventListener('dragleave', () => zone.classList.remove('border', 'border-primary', 'border-dashed'));
          zone.addEventListener('drop', (e) => {
            e.preventDefault();
            zone.classList.remove('border', 'border-primary', 'border-dashed');
            const id = e.dataTransfer.getData('text/plain');
            const newStatus = zone.dataset.status;
            if (!id || !newStatus) return;
            updateTaskStatus(id, newStatus);
          });
        });
      }

      function updateTaskStatus(id, status) {
        const allowed = new Set(allStatuses.map(s => s.key));
        if (!allowed.has((status || '').toLowerCase())) {
          if (alertBox) alertBox.textContent = 'Invalid status';
          return;
        }
        if (alertBox) alertBox.textContent = '';
        const url = `${(typeof url_root !== 'undefined' ? url_root : '../')}api/modules/tasks/detail?id=${encodeURIComponent(id)}`;
        const trigger = document.querySelector(`.kanban-done-task[data-task-id="${CSS.escape(String(id))}"]`);
        if (trigger && trigger.dataset.busy === '1') return;
        if (trigger) {
          trigger.dataset.busy = '1';
          if (window.toggleButtonLoading) window.toggleButtonLoading(trigger, true, '...');
        }
        fetch(url, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ status })
        })
          .then(async (resp) => {
            const data = await resp.json().catch(() => ({}));
            if (!resp.ok) throw new Error(data.error || 'Unable to update task');
            // update local cache and re-render
            currentTasks = currentTasks.map(t => t.id == id ? Object.assign({}, t, { status }) : t);
            renderColumns(currentTasks);
            bindDragHandlers();
          })
          .catch(err => { if (alertBox) alertBox.textContent = err.message || 'Unable to update task'; })
          .finally(() => {
            if (trigger) {
              trigger.dataset.busy = '0';
              if (window.toggleButtonLoading) window.toggleButtonLoading(trigger, false);
            }
          });
      }

      document.addEventListener('click', (e) => {
        const doneBtn = e.target.closest('.kanban-done-task');
        if (doneBtn) {
          const tid = doneBtn.dataset.taskId;
          if (tid) updateTaskStatus(tid, 'done');
        }
        const viewBtn = e.target.closest('.kanban-view-task');
        if (viewBtn) {
          const tid = viewBtn.dataset.taskId;
          if (tid) openViewOffcanvas(tid);
        }
      });

      function openViewOffcanvas(taskId) {
        if (!taskId) return;
        if (viewAlert) viewAlert.textContent = '';
        if (columnsWrap) {
          columnsWrap.setAttribute('aria-busy', 'true');
        }
        fetch(apiTaskDetailBase + '?id=' + encodeURIComponent(taskId))
          .then(r => r.json().then(data => ({ ok: r.ok, data })))
          .then(({ ok, data }) => {
            if (!ok) throw new Error(data?.error || 'Unable to load task');
            const task = data || {};
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
            if (viewEditBtn) {
              viewEditBtn.dataset.taskId = task.id;
              viewEditBtn.classList.remove('d-none');
            }
            if (taskTimeTracking && typeof taskTimeTracking.setEntityId === 'function') {
              taskTimeTracking.setEntityId(task.id || null);
            }
            if (viewCanvas) viewCanvas.show();
          })
          .catch(err => { if (viewAlert) viewAlert.textContent = err.message || 'Unable to load task'; })
          .finally(() => {
            if (columnsWrap) {
              columnsWrap.removeAttribute('aria-busy');
            }
          });
      }

      if (viewEditBtn) {
        viewEditBtn.addEventListener('click', () => {
          const tid = viewEditBtn.dataset.taskId;
          if (!tid) return;
          if (viewEditBtn.dataset.busy === '1') return;
          if (viewCanvas) viewCanvas.hide();
          viewEditBtn.dataset.busy = '1';
          if (window.toggleButtonLoading) window.toggleButtonLoading(viewEditBtn, true, 'Loading...');
          fetch(apiTaskDetailBase + '?id=' + encodeURIComponent(tid))
            .then(r => r.json().then(data => ({ ok: r.ok, data })))
            .then(({ ok, data }) => {
              if (!ok) throw new Error(data?.error || 'Unable to load task');
              openEditInModal(data);
            })
            .catch(err => { if (viewAlert) viewAlert.textContent = err.message || 'Unable to load task for edit'; })
            .finally(() => {
              viewEditBtn.dataset.busy = '0';
              if (window.toggleButtonLoading) window.toggleButtonLoading(viewEditBtn, false);
            });
        });
      }

      function loadStatusesAndTasks() {
        fetch(taskStatusSrc)
          .then(r => r.json().catch(() => ([])))
          .then(list => {
            const mapped = Array.isArray(list)
              ? list.map(st => ({
                  key: (st.code || '').toLowerCase(),
                  label: st.label || st.code || ''
                }))
              : [];
            if (mapped.length) {
              allStatuses = mapped;
              statusOrder = mapped.filter(st => st.key !== 'done');
            }
            // ensure done exists in allowed even if not present
            if (!allStatuses.some(s => s.key === 'done')) {
              allStatuses.push({ key: 'done', label: 'Done' });
            }
            if (createStatus) {
              createStatus.innerHTML = '';
              statusOrder.forEach(st => {
                const opt = document.createElement('option');
                opt.value = st.key;
                opt.textContent = st.label;
                createStatus.appendChild(opt);
              });
              createStatus.value = 'open';
            }
          })
          .catch(() => {})
          .finally(() => fetchTasks());
      }

      function resetCreateForm() {
        if (!createForm) return;
        createForm.reset();
        if (createStatus && createStatus.options.length) createStatus.value = 'open';
        if (caseResults) { caseResults.innerHTML = ''; caseResults.classList.add('d-none'); }
        if (caseSelected) caseSelected.textContent = '';
        if (caseIdInput) caseIdInput.value = '';
        if (createAlert) createAlert.textContent = '';
        if (createEditId) createEditId.value = '';
        if (createTitleEl) createTitleEl.textContent = 'Create Task';
        if (createSubmitBtn) createSubmitBtn.innerHTML = '<i class="ri-save-line me-1"></i>Save Task';
      }

      function renderCaseResults(list) {
        if (!caseResults) return;
        caseResults.innerHTML = '';
        if (!list || !list.length) {
          caseResults.classList.add('d-none');
          return;
        }
        list.slice(0, 5).forEach(cs => {
          const btn = document.createElement('button');
          btn.type = 'button';
          btn.className = 'list-group-item list-group-item-action';
          const label = cs.case_number ? `Case ${cs.case_number}${cs.subject ? ' — ' + cs.subject : ''}` : (cs.subject || '');
          btn.textContent = label || 'Case';
          btn.addEventListener('click', () => {
            if (caseSelected) caseSelected.textContent = label;
            if (caseIdInput) caseIdInput.value = cs.id || '';
            caseResults.classList.add('d-none');
          });
          caseResults.appendChild(btn);
        });
        caseResults.classList.remove('d-none');
      }

      let caseSearchTimer = null;
      if (caseSearch) {
        caseSearch.addEventListener('input', () => {
          const term = caseSearch.value.trim();
          if (caseSearchTimer) clearTimeout(caseSearchTimer);
          if (term.length < 4) {
            if (caseResults) caseResults.classList.add('d-none');
            return;
          }
          caseSearchTimer = setTimeout(() => {
            const qs = new URLSearchParams({ q: term, limit: '5' });
            fetch(apiCases + '?' + qs.toString())
              .then(r => r.json())
              .then(data => renderCaseResults(data.data || []))
              .catch(() => { if (caseResults) caseResults.classList.add('d-none'); });
          }, 300);
        });
      }

      if (createForm) {
        createForm.addEventListener('submit', (e) => {
          e.preventDefault();
          if (createAlert) createAlert.textContent = '';
          const formData = new FormData(createForm);
          const payload = {};
          formData.forEach((val, key) => { payload[key] = val; });
          if (!payload.title || !payload.status) {
            if (createAlert) createAlert.textContent = 'Title and status are required.';
            return;
          }
          const isEdit = createEditId && createEditId.value;
          const url = isEdit ? `${apiTaskDetailBase}?id=${encodeURIComponent(createEditId.value)}` : apiTasks;
          const method = isEdit ? 'PATCH' : 'POST';
          fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
          })
            .then(r => r.json().then(data => ({ ok: r.ok, data })))
            .then(({ ok, data }) => {
              if (!ok) throw new Error(data?.error || 'Unable to create task');
              if (createModal) createModal.hide();
              resetCreateForm();
              fetchTasks();
            })
            .catch(err => { if (createAlert) createAlert.textContent = err.message || 'Unable to save task'; });
        });
      }

      function openEditInModal(task) {
        if (!task || !createForm || !createModal) return;
        resetCreateForm();
        if (createTitleEl) createTitleEl.textContent = 'Edit Task';
        if (createSubmitBtn) createSubmitBtn.innerHTML = '<i class="ri-save-line me-1"></i>Update Task';
        if (createEditId) createEditId.value = task.id || '';
        createForm.querySelector('[name=\"title\"]').value = task.title || '';
        createForm.querySelector('[name=\"description\"]').value = task.description || '';
        createForm.querySelector('[name=\"priority\"]').value = (task.priority || 'medium');
        if (createStatus) createStatus.value = task.status || 'open';
        createForm.querySelector('[name=\"due_at\"]').value = task.due_at ? task.due_at.replace(' ', 'T').slice(0,16) : '';
        createForm.querySelector('[name=\"agent_id\"]').value = task.agent_id || '';
        if (caseIdInput) caseIdInput.value = task.case_id || '';
        if (task.case_subject || task.case_number) {
          const label = task.case_number ? `Case ${task.case_number}${task.case_subject ? ' — ' + task.case_subject : ''}` : (task.case_subject || '');
          if (caseSelected) caseSelected.textContent = label;
          if (caseSearch) caseSearch.value = label;
        }
        createModal.show();
      }

      loadStatusesAndTasks();
    })();
