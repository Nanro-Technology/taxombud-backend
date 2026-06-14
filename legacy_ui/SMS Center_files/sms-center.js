/* eslint-disable */
(function () {
  'use strict';

  // ─── Data & API ─────────────────────────────────────────────────────────────
  const smsRows    = Array.isArray(window.__smsData) ? window.__smsData : [];
  const rootUrl    = (typeof url_root !== 'undefined' && url_root) ? String(url_root) : '../';
  const baseUrl    = rootUrl + 'api/modules/';
  const apiContacts      = baseUrl + 'contacts/index';
  const apiOrganizations = baseUrl + 'organizations/index';
  const apiLeads         = baseUrl + 'leads/index';
  const apiSend          = rootUrl + 'api/modules/sms/send';
  const apiBalance       = rootUrl + 'api/modules/sms/balance';

  // ─── Helpers ────────────────────────────────────────────────────────────────
  function esc(s) {
    return String(s == null ? '' : s)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
  }

  function fmtDate(s) {
    if (!s) return '—';
    try {
      const d = new Date(String(s).replace(' ', 'T'));
      if (isNaN(d.getTime())) return s;
      return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
        + ' ' + d.toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });
    } catch (e) { return s; }
  }

  function fmtPhone(raw) {
    if (!raw) return '—';
    const parts = String(raw).split(',').map(s => s.trim()).filter(Boolean);
    if (parts.length === 0) return '—';
    if (parts.length === 1) return esc(parts[0]);
    if (parts.length === 2) return esc(parts[0]) + ', ' + esc(parts[1]);
    const extra = parts.length - 2;
    return esc(parts[0]) + ', ' + esc(parts[1])
      + ' <span class="badge bg-secondary ms-1" title="' + esc(parts.slice(2).join(', ')) + '">+' + extra + ' more</span>';
  }

  function renderDirection(val) {
    if (val === 'inbound')  return '<span class="badge bg-info-subtle text-info border border-info-subtle px-2"><i class="ri-arrow-left-down-line"></i> In</span>';
    if (val === 'outbound') return '<span class="badge bg-primary-subtle text-primary border border-primary-subtle px-2"><i class="ri-arrow-right-up-line"></i> Out</span>';
    return val ? esc(val) : '—';
  }

  function renderStatus(val) {
    const map = {
      sent:        ['bg-success-subtle text-success border-success-subtle',     'ri-check-line',           'Sent'],
      delivered:   ['bg-success-subtle text-success border-success-subtle',     'ri-checkbox-circle-line', 'Delivered'],
      failed:      ['bg-danger-subtle text-danger border-danger-subtle',        'ri-close-circle-line',    'Failed'],
      undelivered: ['bg-danger-subtle text-danger border-danger-subtle',        'ri-close-circle-line',    'Undelivered'],
      queued:      ['bg-warning-subtle text-warning border-warning-subtle',     'ri-time-line',            'Queued'],
      pending:     ['bg-secondary-subtle text-secondary border-secondary-subtle','ri-loader-line',         'Pending'],
      scheduled:   ['bg-info-subtle text-info border-info-subtle',              'ri-calendar-line',        'Scheduled'],
      sending:     ['bg-primary-subtle text-primary border-primary-subtle',     'ri-send-plane-line',      'Sending'],
    };
    const entry = map[String(val || '').toLowerCase()];
    if (!entry) return val ? '<span class="badge bg-light text-dark border">' + esc(val) + '</span>' : '—';
    const [cls, icon, label] = entry;
    return '<span class="badge ' + cls + ' border"><i class="' + icon + ' me-1"></i>' + label + '</span>';
  }

  function renderProvider(val) {
    const map = {
      twilio:         'bg-primary-subtle text-primary',
      africastalking: 'bg-success-subtle text-success',
      kull:           'bg-info-subtle text-info',
      termii:         'bg-warning-subtle text-warning',
      kudi:           'bg-secondary-subtle text-secondary',
      mobbow:         'bg-dark-subtle text-dark',
    };
    if (!val) return '—';
    const cls = map[String(val).toLowerCase()] || 'bg-light text-dark';
    return '<span class="badge ' + cls + '">' + esc(val) + '</span>';
  }

  // ─── DataTable ──────────────────────────────────────────────────────────────
  var dt = null;

  function renderPlainRows(rows) {
    var tableEl = document.getElementById('smsTable');
    if (!tableEl) return;
    var tbody = tableEl.querySelector('tbody');
    if (!tbody) return;
    tbody.innerHTML = '';

    if (!Array.isArray(rows) || rows.length === 0) {
      tbody.innerHTML = '<tr><td colspan="9" class="text-center text-muted py-4">No SMS messages yet.</td></tr>';
      return;
    }

    rows.forEach(function (row) {
      var tr = document.createElement('tr');
      tr.innerHTML =
        '<td class="text-end text-muted">' + esc(row.id) + '</td>' +
        '<td>' + (row.contact_name
          ? (row.contact_id
              ? '<a href="studio/contacts/view/' + row.contact_id + '.kml" class="text-body fw-semibold">' + esc(row.contact_name) + '</a>'
              : esc(row.contact_name))
          : '<span class="text-muted">—</span>') + '</td>' +
        '<td>' + fmtPhone(row.direction === 'inbound' ? row.from_number : row.to_number) + '</td>' +
        '<td>' + renderProvider(row.provider) + '</td>' +
        '<td>' + fmtDate(row.created_at) + '</td>' +
        '<td>' + renderDirection(row.direction) + '</td>' +
        '<td>' + renderStatus(row.status) + '</td>' +
        '<td>' + (row.sender_label ? esc(row.sender_label) : '<span class="text-muted">—</span>') + '</td>' +
        '<td><div class="d-flex gap-1">' +
          '<button class="btn btn-sm btn-primary sms-view-btn" data-id="' + row.id + '">' +
          '<i class="ri-eye-line me-1"></i>View</button>' +
          '<button class="btn btn-sm btn-outline-danger sms-delete-btn" data-id="' + row.id + '">' +
          '<i class="ri-delete-bin-line"></i></button>' +
        '</div></td>';
      tbody.appendChild(tr);
    });
  }

  function bindTableActions(rows) {
    $('#smsTable').off('click.smsview').on('click.smsview', '.sms-view-btn', function () {
      var id  = parseInt(this.dataset.id, 10);
      var row = rows.find(function (r) { return r.id === id; });
      if (row) openDetail(row);
    });

    $('#smsTable').off('click.smsdelete').on('click.smsdelete', '.sms-delete-btn', function () {
      triggerDelete(parseInt(this.dataset.id, 10));
    });
  }

  async function notifyDeleteResult(message, isError) {
    if (window.crmUiAlert) {
      await window.crmUiAlert(message, isError ? 'Error' : 'Success', {
        variant: isError ? 'danger' : 'success',
        okText: 'OK'
      });
      return;
    }
    window.alert(message);
  }

  function initTable(rows) {
    var tableEl = document.getElementById('smsTable');
    if (!tableEl) return;
    renderPlainRows(rows);

    if (!(window.jQuery && $.fn && $.fn.DataTable)) {
      return;
    }

    try {
      if ($.fn.DataTable.isDataTable('#smsTable')) {
        $('#smsTable').DataTable().destroy();
      }
      tableEl.querySelector('tbody').innerHTML = '';

      dt = $('#smsTable').DataTable({
        data:        rows,
        destroy:     true,
        order:       [[0, 'desc']],
        pageLength:  25,
        language:    { emptyTable: 'No SMS messages yet.' },
        columns: [
          {
            data: 'id',
            className: 'text-end text-muted',
          },
          {
            data: 'contact_name',
            render: function (v, t, row) {
              if (!v) return '<span class="text-muted">—</span>';
              if (row.contact_id) {
                return '<a href="studio/contacts/view/' + row.contact_id + '.kml" class="text-body fw-semibold">' + esc(v) + '</a>';
              }
              return esc(v);
            },
          },
          {
            data: 'to_number',
            render: function (v, t, row) {
              var phone = row.direction === 'inbound' ? row.from_number : row.to_number;
              return fmtPhone(phone);
            },
          },
          {
            data: 'provider',
            render: function (v) { return renderProvider(v); },
          },
          {
            data: 'created_at',
            render: function (v) { return fmtDate(v); },
          },
          {
            data: 'direction',
            render: function (v) { return renderDirection(v); },
          },
          {
            data: 'status',
            render: function (v) { return renderStatus(v); },
          },
          {
            data: 'sender_label',
            render: function (v) { return v ? esc(v) : '<span class="text-muted">—</span>'; },
          },
          {
            data:       null,
            orderable:  false,
            searchable: false,
            render: function (v, t, row) {
              return '<div class="d-flex gap-1">'
                + '<button class="btn btn-sm btn-primary sms-view-btn" data-id="' + row.id + '">'
                + '<i class="ri-eye-line me-1"></i>View</button>'
                + '<button class="btn btn-sm btn-outline-danger sms-delete-btn" data-id="' + row.id + '">'
                + '<i class="ri-delete-bin-line"></i></button>'
                + '</div>';
            },
          },
        ],
      });
    } catch (e) {
      console.error('sms-center table init failed:', e);
      renderPlainRows(rows);
    }

    bindTableActions(rows);
  }

  // ─── Delete ─────────────────────────────────────────────────────────────────
  async function triggerDelete(id) {
    var ok = await window.crmUiConfirm(
      'Are you sure you want to delete this SMS message? This cannot be undone.',
      'Delete Message',
      { okText: 'Delete', cancelText: 'Cancel', variant: 'danger' }
    );
    if (!ok) return;
    try {
      var r    = await fetch('api/modules/sms/message?id=' + id, {
        method: 'DELETE',
        credentials: 'same-origin',
        headers: { 'Accept': 'application/json', 'X-CSRF-Token': window.csrfToken || '' },
      });
      var data = await r.json();
      if (data && data.deleted) {
        if (detailModal) detailModal.hide();
        await notifyDeleteResult('Message deleted.', false);
        window.location.reload();
      } else {
        await notifyDeleteResult(data.error || 'Delete failed.', true);
      }
    } catch (e) {
      await notifyDeleteResult('Network error. Please try again.', true);
    }
  }

  // ─── Detail Modal ───────────────────────────────────────────────────────────
  var detailModal    = null;
  var detailCurrentId = 0;

  // Delete from detail modal
  var deleteBtnEl = document.getElementById('btnSmsDelete');
  if (deleteBtnEl) {
    deleteBtnEl.addEventListener('click', function () {
      if (!detailCurrentId) return;
      triggerDelete(detailCurrentId);
    });
  }

  function openDetail(row) {
    var modalEl = document.getElementById('smsDetailModal');
    if (!modalEl) return;
    if (!detailModal) detailModal = new bootstrap.Modal(modalEl);

    detailCurrentId = row.id || 0;
    // Reset delete button
    if (deleteBtnEl) {
      deleteBtnEl.disabled = false;
      deleteBtnEl.innerHTML = '<i class="ri-delete-bin-line me-1"></i>Delete';
    }

    function set(id, val) {
      var el = modalEl.querySelector('#' + id);
      if (el) el.textContent = (val === null || val === undefined || val === '') ? '—' : val;
    }
    function setHtml(id, val) {
      var el = modalEl.querySelector('#' + id);
      if (el) el.innerHTML = (val === null || val === undefined || val === '') ? '—' : val;
    }

    set('smsDetailId',          row.id);
    set('smsDetailDate',        fmtDate(row.created_at));
    setHtml('smsDetailDirection', renderDirection(row.direction));
    setHtml('smsDetailStatus',    renderStatus(row.status));
    setHtml('smsDetailProvider',  renderProvider(row.provider));
    set('smsDetailCampaign',    row.campaign_id ? '#' + row.campaign_id : '—');
    set('smsDetailFrom',        row.from_number);
    set('smsDetailTo',          row.to_number);
    set('smsDetailContact',     row.contact_name || (row.contact_id ? 'Contact #' + row.contact_id : ''));
    set('smsDetailSender',      row.sender_label);
    set('smsDetailSentAt',      fmtDate(row.sent_at));
    set('smsDetailDeliveredAt', fmtDate(row.delivered_at));
    set('smsDetailBody',        row.body);

    var errRow = modalEl.querySelector('#smsDetailErrorRow');
    var errEl  = modalEl.querySelector('#smsDetailError');
    if (errRow && errEl) {
      if (row.error) {
        errEl.textContent = row.error;
        errRow.style.display = '';
      } else {
        errRow.style.display = 'none';
      }
    }
    detailModal.show();
  }

  // ─── Search helpers ─────────────────────────────────────────────────────────
  var minSearchLen = 4;
  var searchLimit  = 5;
  var searchTimer  = null;

  function hideResults(container) {
    if (!container) return;
    container.classList.add('d-none');
    container.innerHTML = '';
  }

  function setSearchStatus(el, msg, isError) {
    if (!el) return;
    el.textContent = msg || '';
    el.classList.toggle('text-danger', !!isError);
    el.classList.toggle('text-muted',  !isError);
  }

  function renderSearchResults(container, rows, labelFn, pickFn) {
    if (!container) return;
    container.innerHTML = '';
    rows.forEach(function (item) {
      var btn = document.createElement('button');
      btn.type      = 'button';
      btn.className = 'list-group-item list-group-item-action py-2';
      btn.innerHTML = labelFn(item);
      btn.addEventListener('click', function () { pickFn(item); });
      container.appendChild(btn);
    });
    container.classList.toggle('d-none', rows.length === 0);
  }

  function bindSearch(inputEl, statusEl, resultsEl, url, labelFn, pickFn) {
    if (!inputEl || !resultsEl) return;
    inputEl.addEventListener('input', function () {
      clearTimeout(searchTimer);
      var term = inputEl.value.trim();
      searchTimer = setTimeout(function () {
        if (!term || term.length < minSearchLen) {
          hideResults(resultsEl);
          setSearchStatus(statusEl, 'Enter 4+ characters to search.', false);
          return;
        }
        setSearchStatus(statusEl, 'Searching…', false);
        fetch(url + '?q=' + encodeURIComponent(term) + '&limit=' + searchLimit)
          .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
          .then(function (data) {
            var rows = data.data || [];
            if (!rows.length) {
              hideResults(resultsEl);
              setSearchStatus(statusEl, 'No results found.', true);
              return;
            }
            renderSearchResults(resultsEl, rows, labelFn, function (item) {
              pickFn(item);
              hideResults(resultsEl);
              setSearchStatus(statusEl, '', false);
            });
            setSearchStatus(statusEl, '', false);
          })
          .catch(function () {
            hideResults(resultsEl);
            setSearchStatus(statusEl, 'Search failed.', true);
          });
      }, 300);
    });
    document.addEventListener('click', function (e) {
      if (!resultsEl.contains(e.target) && e.target !== inputEl) hideResults(resultsEl);
    });
  }

  // ─── Recipient type toggle ───────────────────────────────────────────────────
  var recipientType    = document.getElementById('smsRecipientType');
  var contactBlock     = document.getElementById('smsContactBlock');
  var organizationBlock = document.getElementById('smsOrganizationBlock');
  var leadBlock        = document.getElementById('smsLeadBlock');
  var customBlock      = document.getElementById('smsCustomBlock');

  // Remove org/lead options if modules not enabled
  var orgEnabled   = !!(window.__mmkModulesEnabled && window.__mmkModulesEnabled.organizations);
  var leadsEnabled = !!(window.__mmkModulesEnabled && window.__mmkModulesEnabled.leads);
  if (recipientType) {
    Array.from(recipientType.options).forEach(function (opt) {
      var val = String(opt.value || '').toLowerCase();
      if ((val === 'organization' && !orgEnabled) || (val === 'lead' && !leadsEnabled)) {
        opt.remove();
      }
    });
  }

  function showRecipient(type) {
    if (contactBlock)      contactBlock.classList.toggle('d-none',      type !== 'contact');
    if (organizationBlock) organizationBlock.classList.toggle('d-none', type !== 'organization');
    if (leadBlock)         leadBlock.classList.toggle('d-none',         type !== 'lead');
    if (customBlock)       customBlock.classList.toggle('d-none',       type !== 'custom');
  }

  if (recipientType) {
    showRecipient(recipientType.value);
    recipientType.addEventListener('change', function () { showRecipient(recipientType.value); });
  }

  // Contact search
  bindSearch(
    document.getElementById('sms_contact_search'),
    document.getElementById('sms_contact_search_status'),
    document.getElementById('sms_contact_results'),
    apiContacts,
    function (c) {
      var name = ((c.first_name || '') + ' ' + (c.last_name || '')).trim() || c.phone || c.email || ('Contact #' + c.id);
      var meta = [c.phone, c.email, c.tin_number].filter(Boolean).join(' · ');
      return '<div class="fw-semibold">' + esc(name) + '</div>'
           + (meta ? '<div class="small text-muted">' + esc(meta) + '</div>' : '');
    },
    function (c) {
      var hidden = document.getElementById('sms_contact_id');
      var input  = document.getElementById('sms_contact_search');
      if (hidden) hidden.value = c.id;
      if (input)  input.value  = ((c.first_name || '') + ' ' + (c.last_name || '')).trim() || c.phone || ('Contact #' + c.id);
    }
  );

  // Organization search
  bindSearch(
    document.getElementById('sms_organization_search'),
    document.getElementById('sms_organization_search_status'),
    document.getElementById('sms_organization_results'),
    apiOrganizations,
    function (org) {
      var meta = [org.phone, org.email, org.tin].filter(Boolean).join(' · ');
      return '<div class="fw-semibold">' + esc(org.name || ('Org #' + org.id)) + '</div>'
           + (meta ? '<div class="small text-muted">' + esc(meta) + '</div>' : '');
    },
    function (org) {
      var hidden = document.getElementById('sms_organization_id');
      var input  = document.getElementById('sms_organization_search');
      if (hidden) hidden.value = org.id;
      if (input)  input.value  = org.name || ('Org #' + org.id);
    }
  );

  // Lead search
  bindSearch(
    document.getElementById('sms_lead_search'),
    document.getElementById('sms_lead_search_status'),
    document.getElementById('sms_lead_results'),
    apiLeads,
    function (lead) {
      var title = lead.title || lead.subject || ('Lead #' + lead.id);
      var meta  = [lead.contact_name, lead.phone, lead.email].filter(Boolean).join(' · ');
      return '<div class="fw-semibold">' + esc(title) + '</div>'
           + (meta ? '<div class="small text-muted">' + esc(meta) + '</div>' : '');
    },
    function (lead) {
      var hidden = document.getElementById('sms_lead_id');
      var input  = document.getElementById('sms_lead_search');
      if (hidden) hidden.value = lead.id;
      if (input)  input.value  = lead.title || lead.subject || ('Lead #' + lead.id);
    }
  );

  // ─── Mode toggle ────────────────────────────────────────────────────────────
  var singleFields    = document.getElementById('smsSingleFields');
  var bulkFields      = document.getElementById('smsBulkFields');
  var campaignRow     = document.getElementById('smsCampaignRow');

  document.querySelectorAll('input[name="mode"]').forEach(function (r) {
    r.addEventListener('change', function () {
      var mode = document.querySelector('input[name="mode"]:checked').value;
      var isBulk = mode === 'bulk';
      if (bulkFields)   bulkFields.classList.toggle('d-none', !isBulk);
      if (singleFields) singleFields.classList.toggle('d-none', isBulk);
      if (campaignRow)  campaignRow.classList.toggle('d-none', !isBulk);
    });
  });

  // ─── Char counter ────────────────────────────────────────────────────────────
  var messageInput = document.querySelector('textarea[name="body"]');
  var charEl       = document.getElementById('smsCharCount');
  var pageEl       = document.getElementById('smsPageCount');

  function updateCount() {
    if (!messageInput || !charEl || !pageEl) return;
    var len   = messageInput.value.length;
    var pages = Math.max(1, Math.ceil(len / 160));
    charEl.textContent = len + ' chars';
    pageEl.textContent = pages + ' SMS · ' + (pages * 160) + ' chars/page';
  }

  if (messageInput) {
    messageInput.addEventListener('input', updateCount);
    updateCount();
  }

  // ─── Send form ───────────────────────────────────────────────────────────────
  var form       = document.getElementById('smsSendForm');
  var statusEl   = document.getElementById('smsSendStatus');
  var sendBtn    = document.getElementById('smsSendBtn');

  if (form) {
    form.addEventListener('submit', function (e) {
      e.preventDefault();
      if (sendBtn) {
        sendBtn.disabled = true;
        sendBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Sending…';
      }
      if (statusEl) { statusEl.textContent = ''; statusEl.className = 'small'; }

      var formData = new FormData(form);
      fetch(apiSend, {
        method:      'POST',
        credentials: 'same-origin',
        headers:     { 'X-CSRF-Token': window.csrfToken || '' },
        body:        formData,
      })
        .then(function (resp) {
          return resp.json().then(function (data) { return { ok: resp.ok, data: data }; });
        })
        .then(function (res) {
          if (!res.ok) throw new Error(res.data.error || 'Send failed');
          var d = res.data;
          var msg = d.status === 'scheduled' ? 'Scheduled!'
                  : d.status === 'queued'    ? 'Queued' + (d.count ? ' (' + d.count + ' recipients)' : '') + '!'
                  : d.sent != null           ? 'Sent (' + d.sent + (d.failed ? ', ' + d.failed + ' failed' : '') + ')!'
                  : 'Sent!';
          if (statusEl) { statusEl.textContent = msg; statusEl.className = 'small text-success'; }
          setTimeout(function () { window.location.reload(); }, 1400);
        })
        .catch(function (err) {
          if (statusEl) { statusEl.textContent = err.message || 'Send failed'; statusEl.className = 'small text-danger'; }
          if (sendBtn) {
            sendBtn.disabled = false;
            sendBtn.innerHTML = '<i class="ri-send-plane-line me-1"></i>Send SMS';
          }
        });
    });
  }

  // ─── Balance check ──────────────────────────────────────────────────────────
  var balanceBtn   = document.getElementById('btnSmsBalance');
  var balanceLabel = document.getElementById('smsBalanceLabel');

  if (balanceBtn && balanceLabel) {
    balanceBtn.addEventListener('click', function () {
      balanceBtn.disabled = true;
      balanceLabel.textContent = 'Checking…';
      fetch(apiBalance, { credentials: 'same-origin' })
        .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
        .then(function (data) {
          var val = data.balance
            ? String(data.balance) + (data.currency ? ' ' + data.currency : '')
            : (data.message || 'Unavailable');
          balanceLabel.textContent = 'Balance: ' + val;
        })
        .catch(function () { balanceLabel.textContent = 'Balance: unavailable'; })
        .finally(function () { balanceBtn.disabled = false; });
    });
  }

  // ─── Refresh button ──────────────────────────────────────────────────────────
  var refreshBtn = document.getElementById('btnRefreshSms');
  if (refreshBtn) {
    refreshBtn.addEventListener('click', function () { window.location.reload(); });
  }

  // ─── Init ────────────────────────────────────────────────────────────────────
  initTable(smsRows);

})();
