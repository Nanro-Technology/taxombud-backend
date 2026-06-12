/* eslint-disable */
(function () {
  'use strict';

  const api = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiCommercial = api.commercialReview || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/commercial-review');
  const apiTickets = api.ticketsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/tickets/index');

  const DOC_URLS = {
    quote:    (id) => 'api/modules/quotes/pdf?id='    + encodeURIComponent(id),
    invoice:  (id) => 'api/modules/invoices/pdf?id='  + encodeURIComponent(id),
    contract: (id) => 'api/modules/contracts/pdf?id=' + encodeURIComponent(id)
  };

  function esc(v) {
    return String(v || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function qs(root, sel) {
    return root ? root.querySelector(sel) : null;
  }

  function qsa(root, sel) {
    return root ? Array.from(root.querySelectorAll(sel)) : [];
  }

  function show(el, visible) {
    if (!el) return;
    el.classList.toggle('d-none', !visible);
  }

  function setLoading(btn, loading, text) {
    if (!btn) return;
    const label = btn.querySelector('.btn-text, .label');
    const spinner = btn.querySelector('.spinner-border');
    btn.disabled = !!loading;
    if (label && text) label.textContent = text;
    if (spinner) spinner.classList.toggle('d-none', !loading);
  }

  function readEntityId(root) {
    const hidden = qs(root, '[data-cr-entity-id-input]');
    const fromInput = hidden && document.getElementById(hidden.getAttribute('data-cr-entity-id-input') || '');
    const raw = String((fromInput && fromInput.value) || root.getAttribute('data-entity-id') || '').trim();
    const num = parseInt(raw, 10);
    return Number.isFinite(num) && num > 0 ? num : 0;
  }

  function readTicketId(root) {
    const raw = String(root.getAttribute('data-ticket-id') || '').trim();
    const num = parseInt(raw, 10);
    return Number.isFinite(num) && num > 0 ? num : 0;
  }

  function readEntityType(root) {
    return String(root.getAttribute('data-entity-type') || '').trim().toLowerCase();
  }

  function renderTicketLink(root, link) {
    const wrap = qs(root, '[data-cr-ticket-link-wrap]');
    const anchor = qs(root, '[data-cr-ticket-link]');
    if (!wrap || !anchor) return;
    if (!link || !link.ticket_id_s) {
      anchor.removeAttribute('href');
      anchor.textContent = 'No review ticket linked yet.';
      show(wrap, true);
      return;
    }
    anchor.href = 'studio/tickets/view.kml?id=' + encodeURIComponent(link.ticket_id_s);
    anchor.textContent = link.ticket_number ? ('Ticket #' + link.ticket_number) : ('Ticket #' + link.ticket_id);
    show(wrap, true);
  }

  function renderDocLink(root, entity) {
    const wrap = qs(root, '[data-cr-doc-link-wrap]');
    const anchor = qs(root, '[data-cr-doc-link]');
    if (!wrap || !anchor || !entity) return;
    const entityType = String(entity.entity_type || '').toLowerCase();
    const entityId = entity.entity_id_s || entity.entity_id;
    if (!entityType || !entityId || !DOC_URLS[entityType]) {
      show(wrap, false);
      return;
    }
    anchor.href = DOC_URLS[entityType](entityId);
    anchor.target = '_blank';
    anchor.rel = 'noopener noreferrer';
    anchor.textContent = entity.label || entityType;
    show(wrap, true);
  }

  function renderDepartments(root, departments, selectedDeptId) {
    const deptEl = qs(root, '[data-cr-department]');
    if (!deptEl) return;
    const current = String(selectedDeptId || deptEl.value || '');
    deptEl.innerHTML = '<option value="">Select department</option>';
    (Array.isArray(departments) ? departments : []).forEach((row) => {
      const id = String(row && row.id ? row.id : '');
      if (!id) return;
      const opt = document.createElement('option');
      opt.value = id;
      opt.textContent = row.name || ('Department #' + id);
      deptEl.appendChild(opt);
    });
    if (current) {
      deptEl.value = current;
    }
  }

  function setStatus(root, link, entity, settings) {
    const statusEl = qs(root, '[data-cr-status]');
    const noteEl = qs(root, '[data-cr-note]');
    const deptEl = qs(root, '[data-cr-department]');
    const ticketSearch = qs(root, '[data-cr-ticket-search]');
    const ticketSelect = qs(root, '[data-cr-ticket-select]');
    const startBtn = qs(root, '[data-cr-start]');
    const attachBtn = qs(root, '[data-cr-attach]');
    const openBtn = qs(root, '[data-cr-open]');
    const canWrite = String(root.getAttribute('data-cr-can-write') || '1') !== '0';
    const config = entity && entity.entity_type ? (settings[String(entity.entity_type).toLowerCase()] || {}) : {};
    const linked = !!(link && link.ticket_id);

    if (statusEl) {
      const status = linked ? String(link.review_status || 'in_review') : (config.required ? 'review_required' : 'not_linked');
      const badgeClass = {
        approved: 'bg-success-subtle text-success',
        returned: 'bg-warning-subtle text-warning',
        rejected: 'bg-danger-subtle text-danger',
        issued: 'bg-info-subtle text-info',
        closed: 'bg-secondary-subtle text-secondary',
        in_review: 'bg-primary-subtle text-primary',
        review_required: 'bg-warning-subtle text-warning',
        not_linked: 'bg-light text-muted'
      }[status] || 'bg-light text-muted';
      statusEl.className = 'badge ' + badgeClass;
      statusEl.textContent = status.replace(/_/g, ' ');
    }

    if (deptEl) {
      const deptId = String(config.department_id || '');
      if (deptId) {
        deptEl.value = deptId;
      }
      deptEl.disabled = linked || !canWrite;
    }

    if (noteEl && linked) {
      noteEl.value = '';
    }

    const searchSection = (ticketSearch ? ticketSearch.closest('.row') : null)
      || (ticketSelect ? ticketSelect.closest('.row') : null);
    show(searchSection, !linked);
    show(openBtn, linked);
    show(startBtn, !linked);
    show(attachBtn, !linked);

    if (startBtn) {
      const chosenDeptId = String((deptEl && deptEl.value) || config.department_id || '');
      startBtn.disabled = !canWrite || !entity || !entity.entity_id || !chosenDeptId;
      if (!chosenDeptId && !linked) {
        startBtn.title = 'Select a review department first.';
      } else {
        startBtn.removeAttribute('title');
      }
    }
    if (attachBtn) {
      attachBtn.disabled = !canWrite;
    }
    if (ticketSearch) ticketSearch.disabled = !canWrite;
    if (ticketSelect) ticketSelect.disabled = !canWrite;
    if (noteEl) noteEl.disabled = !canWrite || linked;
  }

  async function loadTicketLookup(root) {
    const searchInput = qs(root, '[data-cr-ticket-search]');
    const select = qs(root, '[data-cr-ticket-select]');
    const value = String(searchInput && searchInput.value || '').trim();
    if (!select) return;
    select.innerHTML = '<option value="">Select review ticket</option>';
    try {
      const params = new URLSearchParams();
      params.set('lookup', 'review_tickets');
      params.set('limit', '10');
      if (value) params.set('q', value);
      const res = await fetch(apiTickets + '?' + params.toString(), { credentials: 'same-origin' });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) {
        throw new Error(data.error || 'Unable to search review tickets');
      }
      (data.data || []).forEach((row) => {
        const opt = document.createElement('option');
        opt.value = row.id;
        opt.textContent = row.label || ('Ticket #' + row.id);
        select.appendChild(opt);
      });
    } catch (e) {
      select.innerHTML = '<option value="">No review tickets found</option>';
    }
  }

  function initSelect2(root) {
    if (typeof $ === 'undefined' || !$.fn || !$.fn.select2) return false;
    const ticketSelect = qs(root, '[data-cr-ticket-select]');
    if (!ticketSelect) return false;
    // Append dropdown inside the modal so z-index is correct behind the backdrop
    const modalEl = ticketSelect.closest('.modal') || document.body;
    $(ticketSelect).select2({
      placeholder: 'Search for a review ticket…',
      allowClear: true,
      minimumInputLength: 0,
      width: '100%',
      dropdownParent: $(modalEl),
      ajax: {
        url: apiTickets,
        dataType: 'json',
        delay: 300,
        xhrFields: { withCredentials: true },
        data: function (params) {
          return { lookup: 'review_tickets', q: params.term || '', limit: 20 };
        },
        processResults: function (data) {
          return {
            results: (data.data || []).map(function (r) {
              return { id: r.id, text: r.label || ('Ticket #' + r.id) };
            })
          };
        }
      }
    });
    $(ticketSelect).on('change', function () {
      const attach = qs(root, '[data-cr-attach]');
      if (attach) attach.disabled = !$(ticketSelect).val();
    });
    return true;
  }

  async function loadState(root) {
    const entityType = readEntityType(root);
    const entityId = readEntityId(root);
    const ticketId = readTicketId(root);
    if (!entityType && !ticketId) {
      return;
    }
    const params = new URLSearchParams();
    if (entityType && entityId > 0) {
      params.set('entity_type', entityType);
      params.set('entity_id', String(entityId));
    } else if (ticketId > 0) {
      params.set('ticket_id', String(ticketId));
    }
    try {
      const res = await fetch(apiCommercial + '?' + params.toString(), { credentials: 'same-origin' });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) {
        throw new Error(data.error || 'Unable to load commercial review');
      }
      const payload = data.data || {};
      root.__commercialReviewState = payload;
      renderDepartments(root, payload.departments || [], payload.link && payload.link.department_id ? payload.link.department_id : ((payload.settings && payload.settings[readEntityType(root)] && payload.settings[readEntityType(root)].department_id) || ''));
      setStatus(root, payload.link || null, payload.entity || null, payload.settings || {});
      renderTicketLink(root, payload.link || null);
      renderDocLink(root, payload.entity || null);
    } catch (e) {
      const alert = qs(root, '[data-cr-alert]');
      if (alert) {
        alert.textContent = e.message || 'Unable to load commercial review';
        show(alert, true);
      }
    }
  }

  async function submitAction(root, action) {
    const entityType = readEntityType(root);
    const entityId = readEntityId(root);
    const ticketId = readTicketId(root);
    const state = root.__commercialReviewState || {};
    const link = state.link || null;
    const settings = state.settings || {};
    const config = entityType ? (settings[entityType] || {}) : {};
    const noteEl = qs(root, '[data-cr-note]');
    const deptEl = qs(root, '[data-cr-department]');
    const ticketSelect = qs(root, '[data-cr-ticket-select]');
    const payload = {
      action: action,
      note: noteEl ? String(noteEl.value || '').trim() : '',
      entity_type: entityType || (link && link.entity_type) || '',
      entity_id: entityId || (link && link.entity_id) || 0,
      ticket_id: ticketId || (link && link.ticket_id) || 0
    };

    if (action === 'start') {
      payload.department_id = deptEl ? (deptEl.value || config.department_id || '') : (config.department_id || '');
      if (!payload.department_id) {
        throw new Error('Select a review department first.');
      }
    }
    if (action === 'attach') {
      const rawVal = ticketSelect
        ? (typeof $ !== 'undefined' && $.fn && $.fn.select2 ? ($(ticketSelect).val() || '') : ticketSelect.value)
        : '';
      const tid = parseInt(rawVal || '0', 10);
      if (!tid) {
        throw new Error('Select an existing review ticket.');
      }
      payload.ticket_id = tid;
    }

    const btn = qs(root, `[data-cr-${action}]`);
    setLoading(btn, true, btn && btn.getAttribute('data-loading-text') ? btn.getAttribute('data-loading-text') : null);
    try {
      const res = await fetch(apiCommercial, {
        method: 'POST',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) {
        throw new Error(data.error || data.message || 'Unable to update commercial review');
      }
      await loadState(root);
      try {
        window.dispatchEvent(new CustomEvent('commercial-review:updated', {
          detail: {
            entityType: entityType || (link && link.entity_type) || '',
            entityId: entityId || (link && link.entity_id) || 0,
            ticketId: ticketId || (link && link.ticket_id) || 0,
            action: action
          }
        }));
      } catch (_e) {
        // ignore
      }
      if (typeof window.showSavedModal === 'function') {
        window.showSavedModal('Saved', data.message || 'Commercial review updated.');
      }
    } finally {
      setLoading(btn, false);
    }
  }

  function bind(root) {
    const startBtn = qs(root, '[data-cr-start]');
    const attachBtn = qs(root, '[data-cr-attach]');
    const openBtn = qs(root, '[data-cr-open]');
    const refreshBtn = qs(root, '[data-cr-refresh]');
    const searchBtn = qs(root, '[data-cr-ticket-search-btn]');
    const searchInput = qs(root, '[data-cr-ticket-search]');
    const ticketSelect = qs(root, '[data-cr-ticket-select]');

    if (searchBtn && searchInput) {
      searchBtn.addEventListener('click', async (e) => {
        e.preventDefault();
        await loadTicketLookup(root);
      });
      searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
          e.preventDefault();
          loadTicketLookup(root);
        }
      });
    }
    if (startBtn) {
      startBtn.addEventListener('click', async (e) => {
        e.preventDefault();
        try {
          await submitAction(root, 'start');
        } catch (err) {
          const alert = qs(root, '[data-cr-alert]');
          if (alert) {
            alert.textContent = err.message || 'Unable to start review';
            show(alert, true);
          }
        }
      });
    }
    if (attachBtn) {
      attachBtn.addEventListener('click', async (e) => {
        e.preventDefault();
        try {
          await submitAction(root, 'attach');
        } catch (err) {
          const alert = qs(root, '[data-cr-alert]');
          if (alert) {
            alert.textContent = err.message || 'Unable to attach review ticket';
            show(alert, true);
          }
        }
      });
    }
    if (openBtn) {
      openBtn.addEventListener('click', (e) => {
        e.preventDefault();
        const link = root.__commercialReviewState && root.__commercialReviewState.link;
        if (!link || !link.ticket_id_s) return;
        window.location.href = 'studio/tickets/view.kml?id=' + encodeURIComponent(link.ticket_id_s);
      });
    }
    if (refreshBtn) {
      refreshBtn.addEventListener('click', async (e) => {
        e.preventDefault();
        await loadState(root);
      });
    }

    if (ticketSelect) {
      ticketSelect.addEventListener('change', () => {
        const attach = qs(root, '[data-cr-attach]');
        if (attach) attach.disabled = !ticketSelect.value;
      });
    }
  }

  function initRoot(root) {
    if (!root || root.__commercialReviewWidget) return;
    root.__commercialReviewWidget = {
      refresh: () => loadState(root),
      loadTickets: () => loadTicketLookup(root),
      startReview: () => submitAction(root, 'start'),
      attachReview: () => submitAction(root, 'attach')
    };
    bind(root);
    root.__crSelect2 = initSelect2(root);
    loadState(root);
  }

  function initAll() {
    qsa(document, '[data-commercial-review-root]').forEach(initRoot);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAll);
  } else {
    initAll();
  }

  window.CommercialReviewWidget = {
    init: initRoot
  };
})();
