/**
 * Interactions listing page — filter, paginate, summary stats, view offcanvas.
 */
(function () {
  'use strict';

  const PAGE_SIZE = 50;

  const TYPE_LABELS = {
    enquiry: 'Enquiry', information: 'Information', notice: 'Notice',
    followup: 'Follow-up', complaint_lead: 'Complaint Lead', other: 'Other',
  };
  const TYPE_COLORS = {
    enquiry: 'primary', information: 'info', notice: 'warning',
    followup: 'secondary', complaint_lead: 'danger', other: 'dark',
  };
  const CHANNEL_LABELS = {
    phone: 'Phone', email: 'Email', in_person: 'In-Person',
    whatsapp: 'WhatsApp', sms: 'SMS', other: 'Other',
  };
  const DIRECTION_LABELS = { inbound: 'Inbound', outbound: 'Outbound' };
  const OUTCOME_LABELS = {
    resolved: 'Resolved', awaiting_reply: 'Awaiting Reply',
    needs_followup: 'Needs Follow-up', escalated_to_case: 'Escalated to Case',
    other: 'Other',
  };
  const ENTITY_PATHS = {
    contact:      'studio/contacts/view.kml?id=',
    organization: 'studio/organizations/view.kml?id=',
    lead:         'studio/leads/view.kml?id=',
    account:      'studio/accounts/view.kml?id=',
  };
  const ENTITY_ICONS = {
    contact: 'ri-user-3-line',
    organization: 'ri-building-4-line',
    lead: 'ri-funnel-line',
    account: 'ri-briefcase-line',
  };
  const ENTITY_LABELS = {
    contact: 'Contact', organization: 'Organization',
    lead: 'Lead', account: 'Account',
  };

  const root = (typeof url_root !== 'undefined' ? url_root : '');

  const state = { offset: 0, total: 0, lastRows: [], rowsById: {} };

  // DOM refs
  const body = document.getElementById('intTableBody');
  const pagerInfo = document.getElementById('intPagerInfo');
  const pagerPrev = document.getElementById('intPagerPrev');
  const pagerNext = document.getElementById('intPagerNext');
  const refreshBtn = document.getElementById('intRefreshBtn');
  const applyBtn = document.getElementById('intFilterApplyBtn');
  const resetBtn = document.getElementById('intFilterResetBtn');
  const filters = {
    search:    document.getElementById('intFilterSearch'),
    type:      document.getElementById('intFilterType'),
    channel:   document.getElementById('intFilterChannel'),
    direction: document.getElementById('intFilterDirection'),
    outcome:   document.getElementById('intFilterOutcome'),
    date:      document.getElementById('intFilterDateRange'),
  };
  const stat = {
    total:    document.getElementById('intStatTotal'),
    inbound:  document.getElementById('intStatInbound'),
    outbound: document.getElementById('intStatOutbound'),
    followup: document.getElementById('intStatFollowup'),
  };
  const offcanvasEl = document.getElementById('intViewOffcanvas');
  const offcanvasBody = document.getElementById('intViewBody');
  const offcanvasSubtitle = document.getElementById('intViewSubtitle');
  let bsOffcanvas = null;

  // Helpers
  function esc(s) {
    return String(s == null ? '' : s)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }
  function fmtDate(iso) {
    if (!iso) return '';
    const d = new Date((iso || '').replace(' ', 'T'));
    if (isNaN(d.getTime())) return iso;
    return d.toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' });
  }

  function buildQuery() {
    const params = new URLSearchParams();
    params.set('limit', String(PAGE_SIZE));
    params.set('offset', String(state.offset));
    if (filters.search.value.trim())    params.set('q', filters.search.value.trim());
    if (filters.type.value)              params.set('type', filters.type.value);
    if (filters.channel.value)           params.set('channel', filters.channel.value);
    if (filters.direction.value)         params.set('direction', filters.direction.value);
    if (filters.outcome.value)           params.set('outcome', filters.outcome.value);
    const dr = filters.date.value.trim();
    if (dr) {
      const m = dr.split(/\s+to\s+/i);
      if (m[0]) params.set('date_from', m[0].trim());
      if (m[1]) params.set('date_to', m[1].trim());
    }
    return params.toString();
  }

  function renderRow(row) {
    const typeLabel = TYPE_LABELS[row.interaction_type] || row.interaction_type;
    const typeColor = TYPE_COLORS[row.interaction_type] || 'secondary';
    const channel = CHANNEL_LABELS[row.channel] || row.channel;
    const outcome = row.outcome ? (OUTCOME_LABELS[row.outcome] || row.outcome) : '<span class="text-muted">—</span>';
    const dirIcon = row.direction === 'outbound'
      ? '<i class="ri-arrow-right-up-line text-success int-dir-icon" title="Outbound"></i>'
      : '<i class="ri-arrow-left-down-line text-primary int-dir-icon" title="Inbound"></i>';

    const entityPath = ENTITY_PATHS[row.entity_type] || '';
    const entityIcon = ENTITY_ICONS[row.entity_type] || 'ri-question-line';
    const entityLink = entityPath
      ? `<a href="${entityPath}${encodeURIComponent(row.entity_id)}" class="text-decoration-none" onclick="event.stopPropagation();"><i class="${entityIcon} me-1"></i>${esc(ENTITY_LABELS[row.entity_type] || row.entity_type)} #${esc(row.entity_id)}</a>`
      : `<span class="text-muted"><i class="${entityIcon} me-1"></i>${esc(row.entity_type)} #${esc(row.entity_id)}</span>`;

    return `<tr data-int-id="${esc(row.id)}">
      <td>${dirIcon}</td>
      <td>
        <a class="int-subject-link" data-int-view="${esc(row.id)}">${esc(row.subject || '(no subject)')}</a>
        ${row.description ? `<div class="small text-muted text-truncate" style="max-width:460px;">${esc(row.description)}</div>` : ''}
        ${row.follow_up_at ? `<div class="small text-warning mt-1"><i class="ri-alarm-line me-1"></i>${esc(fmtDate(row.follow_up_at))}</div>` : ''}
      </td>
      <td>
        <div><span class="badge bg-${typeColor}-subtle text-${typeColor} int-type-badge">${esc(typeLabel)}</span></div>
        <div class="small text-muted mt-1">${esc(channel)}</div>
      </td>
      <td>${outcome}</td>
      <td>${entityLink}</td>
      <td><small>${row.logged_by_name ? esc(row.logged_by_name) : '<span class="text-muted">—</span>'}</small></td>
      <td><small class="text-muted">${esc(fmtDate(row.occurred_at))}</small></td>
      <td class="text-end">
        <button type="button" class="btn btn-sm btn-soft-primary" data-int-view="${esc(row.id)}" title="View details">
          <i class="ri-eye-line"></i> View
        </button>
      </td>
    </tr>`;
  }

  function renderSummary(rows, total) {
    if (stat.total) stat.total.textContent = String(total || 0);
    let inbound = 0, outbound = 0, followup = 0;
    rows.forEach((r) => {
      if (r.direction === 'inbound')  inbound++;
      if (r.direction === 'outbound') outbound++;
      if (r.outcome === 'needs_followup' || r.follow_up_at) followup++;
    });
    if (stat.inbound)  stat.inbound.textContent  = String(inbound);
    if (stat.outbound) stat.outbound.textContent = String(outbound);
    if (stat.followup) stat.followup.textContent = String(followup);
  }

  function load() {
    body.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading…</td></tr>';
    const qs = buildQuery();
    fetch(root + 'api/modules/interactions/index?' + qs, { credentials: 'same-origin' })
      .then((r) => r.json())
      .then((d) => {
        if (!d || !d.ok) throw new Error((d && d.error) || 'Failed to load');
        const rows = d.data || [];
        state.lastRows = rows;
        state.total = parseInt(d.total, 10) || 0;
        state.rowsById = {};
        rows.forEach((r) => { state.rowsById[String(r.id)] = r; });

        if (!rows.length) {
          body.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-4"><i class="ri-chat-voice-line me-1"></i>No interactions match the current filter.</td></tr>';
        } else {
          body.innerHTML = rows.map(renderRow).join('');
        }
        renderSummary(rows, state.total);
        const from = state.total === 0 ? 0 : (state.offset + 1);
        const to   = Math.min(state.offset + rows.length, state.total);
        pagerInfo.textContent = state.total === 0
          ? 'No results'
          : ('Showing ' + from + ' to ' + to + ' of ' + state.total);
        pagerPrev.disabled = state.offset <= 0;
        pagerNext.disabled = (state.offset + rows.length) >= state.total;
      })
      .catch((err) => {
        body.innerHTML = '<tr><td colspan="8" class="text-center text-danger py-4">' + esc(err.message || 'Load failed') + '</td></tr>';
      });
  }

  // ── Detail offcanvas ──────────────────────────────────────────────────────
  function openInteraction(id) {
    if (!offcanvasEl) return;
    if (!bsOffcanvas && typeof bootstrap !== 'undefined' && bootstrap.Offcanvas) {
      bsOffcanvas = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
    }
    // Show immediately with cached row, then fetch fresh detail
    const cached = state.rowsById[String(id)];
    if (cached) renderDetail(cached, /* freshLoading */ true);
    else        offcanvasBody.innerHTML = '<div class="text-center text-muted py-5"><span class="spinner-border spinner-border-sm me-2"></span>Loading…</div>';
    if (bsOffcanvas) bsOffcanvas.show();

    fetch(root + 'api/modules/interactions/detail?id=' + encodeURIComponent(id), { credentials: 'same-origin' })
      .then((r) => r.json())
      .then((d) => {
        if (!d || !d.ok) throw new Error((d && d.error) || 'Failed to load');
        renderDetail(d.data, false);
      })
      .catch((err) => {
        offcanvasBody.innerHTML = '<div class="alert alert-danger">' + esc(err.message || 'Failed to load interaction') + '</div>';
      });
  }

  function renderDetail(row, isStale) {
    const typeLabel = TYPE_LABELS[row.interaction_type] || row.interaction_type;
    const typeColor = TYPE_COLORS[row.interaction_type] || 'secondary';
    const channel = CHANNEL_LABELS[row.channel] || row.channel;
    const direction = DIRECTION_LABELS[row.direction] || row.direction;
    const outcome = row.outcome ? (OUTCOME_LABELS[row.outcome] || row.outcome) : '—';
    const entityPath = ENTITY_PATHS[row.entity_type] || '';
    const entityIcon = ENTITY_ICONS[row.entity_type] || 'ri-question-line';
    const entityLabel = ENTITY_LABELS[row.entity_type] || row.entity_type;
    const entityLink = entityPath
      ? `<a href="${entityPath}${encodeURIComponent(row.entity_id)}" class="text-decoration-none"><i class="${entityIcon} me-1"></i>${esc(entityLabel)} #${esc(row.entity_id)} <i class="ri-external-link-line ms-1"></i></a>`
      : `<span class="text-muted"><i class="${entityIcon} me-1"></i>${esc(entityLabel)} #${esc(row.entity_id)}</span>`;

    offcanvasSubtitle.textContent = (entityLabel + ' #' + row.entity_id) + '  ·  ' + fmtDate(row.occurred_at);

    // Attachments (if any) — uses the shared frontend helper if available
    let attachmentsHtml = '';
    if (typeof window.crmRenderActivityAttachments === 'function') {
      const inner = window.crmRenderActivityAttachments(row.metadata);
      if (inner) attachmentsHtml = `<div class="iv-field"><div class="iv-label">Attachments</div>${inner}</div>`;
    }

    const followLine = row.follow_up_at
      ? `<div class="iv-field"><div class="iv-label">Follow-up due</div><div class="iv-value text-warning"><i class="ri-alarm-line me-1"></i>${esc(fmtDate(row.follow_up_at))}</div></div>`
      : '';

    offcanvasBody.innerHTML = `
      ${isStale ? '<div class="text-end small text-muted mb-2"><span class="spinner-border spinner-border-sm me-1"></span>Refreshing…</div>' : ''}
      <div class="d-flex align-items-center gap-2 mb-3 flex-wrap">
        <span class="badge bg-${typeColor}-subtle text-${typeColor} int-type-badge">${esc(typeLabel)}</span>
        <span class="badge bg-light text-muted">${esc(channel)}</span>
        <span class="badge bg-secondary-subtle text-secondary">${esc(direction)}</span>
        ${row.outcome ? `<span class="badge bg-info-subtle text-info">${esc(outcome)}</span>` : ''}
      </div>

      <div class="iv-field">
        <div class="iv-label">Subject</div>
        <div class="iv-value fw-semibold">${esc(row.subject || '(no subject)')}</div>
      </div>

      ${row.description ? `<div class="iv-field"><div class="iv-label">Description</div><div class="iv-desc">${esc(row.description)}</div></div>` : ''}

      ${attachmentsHtml}

      ${followLine}

      <hr>

      <div class="row g-2">
        <div class="col-6">
          <div class="iv-label">Related to</div>
          <div class="iv-value">${entityLink}</div>
        </div>
        <div class="col-6">
          <div class="iv-label">Logged by</div>
          <div class="iv-value">${row.logged_by_name ? esc(row.logged_by_name) : '<span class="text-muted">—</span>'}</div>
        </div>
        <div class="col-6">
          <div class="iv-label">When it happened</div>
          <div class="iv-value">${esc(fmtDate(row.occurred_at))}</div>
        </div>
        <div class="col-6">
          <div class="iv-label">Created</div>
          <div class="iv-value">${esc(fmtDate(row.created_at))}</div>
        </div>
      </div>
    `;
  }

  // Wire events
  applyBtn.addEventListener('click', () => { state.offset = 0; load(); });
  resetBtn.addEventListener('click', () => {
    Object.values(filters).forEach((f) => { if (f) f.value = ''; });
    state.offset = 0;
    load();
  });
  refreshBtn.addEventListener('click', load);
  pagerPrev.addEventListener('click', () => { if (state.offset > 0) { state.offset = Math.max(0, state.offset - PAGE_SIZE); load(); } });
  pagerNext.addEventListener('click', () => { state.offset += PAGE_SIZE; load(); });
  filters.search.addEventListener('keydown', (e) => { if (e.key === 'Enter') { state.offset = 0; load(); } });

  // Open detail offcanvas only when the Subject link or the View button is clicked
  body.addEventListener('click', (e) => {
    const trigger = e.target.closest('[data-int-view]');
    if (!trigger) return;
    e.preventDefault();
    const id = trigger.dataset.intView;
    if (id) openInteraction(id);
  });

  document.addEventListener('DOMContentLoaded', load);
})();
