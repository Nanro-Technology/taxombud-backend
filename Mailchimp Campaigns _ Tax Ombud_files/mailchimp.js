(() => {
  const cfg = window.mailchimpCampaignConfig || {};
  const apiUrl = cfg.apiUrl || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/mailchimp/campaigns');

  const pageAlert = document.getElementById('mailchimpPageAlert');
  const tableBody = document.getElementById('mailchimpTableBody');
  const emptyState = document.getElementById('mailchimpEmptyState');
  const audienceSelect = document.getElementById('mailchimpAudienceSelect');
  const audienceModalSelect = document.getElementById('mailchimpCampaignAudience');
  const searchInput = document.getElementById('mailchimpSearch');
  const statusFilter = document.getElementById('mailchimpStatusFilter');
  const filterBtn = document.getElementById('mailchimpFilterBtn');
  const resetBtn = document.getElementById('mailchimpResetBtn');
  const newBtn = document.getElementById('mailchimpNewBtn');
  const syncBtn = document.getElementById('mailchimpSyncBtn');

  const modalEl = document.getElementById('mailchimpCampaignModal');
  const modal = modalEl && typeof bootstrap !== 'undefined' ? bootstrap.Modal.getOrCreateInstance(modalEl) : null;
  const modalTitle = document.getElementById('mailchimpCampaignModalTitle');
  const formAlert = document.getElementById('mailchimpFormAlert');
  const campaignIdInput = document.getElementById('mailchimpCampaignId');
  const campaignNameInput = document.getElementById('mailchimpCampaignName');
  const campaignSubjectInput = document.getElementById('mailchimpCampaignSubject');
  const campaignPreviewInput = document.getElementById('mailchimpCampaignPreview');
  const campaignMeta = document.getElementById('mailchimpCampaignMeta');
  const saveBtn = document.getElementById('mailchimpSaveDraftBtn');
  const sendBtn = document.getElementById('mailchimpSendBtn');

  const audienceSeed = Array.isArray(cfg.audiences) ? cfg.audiences.slice() : [];
  let audiences = audienceSeed.slice();
  let campaigns = [];
  let quill = null;
  let readOnlyMode = false;

  function esc(value) {
    return String(value == null ? '' : value).replace(/[&<>"']/g, (ch) => ({
      '&': '&amp;',
      '<': '&lt;',
      '>': '&gt;',
      '"': '&quot;',
      "'": '&#039;'
    }[ch]));
  }

  function stripHtml(value) {
    const div = document.createElement('div');
    div.innerHTML = String(value == null ? '' : value);
    return (div.textContent || div.innerText || '').replace(/\s+/g, ' ').trim();
  }

  function fmtDate(value) {
    if (!value) return '—';
    const dt = new Date(value);
    if (Number.isNaN(dt.getTime())) return value;
    return dt.toLocaleString();
  }

  function setPageAlert(message, variant = 'info') {
    if (!pageAlert) return;
    if (!message) {
      pageAlert.className = 'alert alert-info d-none mb-0 mt-3';
      pageAlert.textContent = '';
      return;
    }
    pageAlert.className = `alert alert-${variant} mb-0 mt-3`;
    pageAlert.textContent = message;
  }

  function setFormAlert(message) {
    if (!formAlert) return;
    if (!message) {
      formAlert.classList.add('d-none');
      formAlert.textContent = '';
      return;
    }
    formAlert.classList.remove('d-none');
    formAlert.textContent = message;
  }

  function setBusy(button, busy) {
    if (!button) return;
    button.disabled = !!busy;
    const label = button.querySelector('.btn-label');
    const spinner = button.querySelector('.spinner-border');
    if (label) label.classList.toggle('d-none', !!busy);
    if (spinner) spinner.classList.toggle('d-none', !busy);
  }

  function apiFetchDetailed(url, options) {
    return fetch(url, Object.assign({ credentials: 'same-origin' }, options || {}))
      .then(async (resp) => {
        const text = await resp.text();
        let data = null;
        if (text) {
          try {
            data = JSON.parse(text);
          } catch (e) {
            data = { raw: text };
          }
        }
        return {
          ok: resp.ok,
          status: resp.status,
          statusText: resp.statusText,
          data: data,
          raw: text
        };
      });
  }

  function confirmAction(message) {
    return window.confirm(String(message || 'Are you sure?'));
  }

  function getAudienceLabel(id) {
    const key = String(id || '');
    const row = audiences.find((item) => String(item.id || '') === key);
    if (!row) {
      return key || '—';
    }
    const count = Number(row.member_count || 0);
    return row.name ? `${row.name}${count > 0 ? ` (${count})` : ''}` : key;
  }

  function setAudienceSelectValue(selectEl, id) {
    if (!selectEl) return;
    const value = String(id || '');
    if (value && Array.from(selectEl.options).some((opt) => String(opt.value || '') === value)) {
      selectEl.value = value;
      return;
    }
    if (selectEl.options.length > 0) {
      selectEl.selectedIndex = 0;
    }
  }

  function renderAudienceOptions() {
    const targetIds = [audienceSelect, audienceModalSelect];
    targetIds.forEach((selectEl) => {
      if (!selectEl) return;
      const current = String(selectEl.value || cfg.defaultAudienceId || '');
      selectEl.innerHTML = '';
      if (!audiences.length) {
        const opt = document.createElement('option');
        opt.value = '';
        opt.textContent = cfg.configured ? 'No audiences found' : 'Configure Mailchimp first';
        selectEl.appendChild(opt);
        selectEl.disabled = true;
        return;
      }
      selectEl.disabled = false;
      audiences.forEach((aud) => {
        const opt = document.createElement('option');
        opt.value = String(aud.id || '');
        const count = Number(aud.member_count || 0);
        opt.textContent = aud.name ? `${aud.name}${count > 0 ? ` (${count})` : ''}` : String(aud.id || '');
        selectEl.appendChild(opt);
      });
      setAudienceSelectValue(selectEl, current || cfg.defaultAudienceId || audiences[0].id || '');
    });
  }

  function statusBadge(status) {
    const val = String(status || 'draft').toLowerCase();
    const map = {
      draft: 'bg-secondary-subtle text-secondary',
      failed: 'bg-danger-subtle text-danger',
      sent: 'bg-success-subtle text-success'
    };
    return `<span class="badge ${map[val] || 'bg-light text-muted'}">${esc(val)}</span>`;
  }

  function resetForm() {
    if (campaignIdInput) campaignIdInput.value = '';
    if (campaignNameInput) campaignNameInput.value = '';
    if (campaignSubjectInput) campaignSubjectInput.value = '';
    if (campaignPreviewInput) campaignPreviewInput.value = '';
    if (campaignMeta) campaignMeta.textContent = '';
    setFormAlert('');
    if (quill) {
      quill.setText('');
      quill.enable(true);
    }
    readOnlyMode = false;
    if (saveBtn) saveBtn.classList.remove('d-none');
    if (sendBtn) sendBtn.classList.remove('d-none');
    if (saveBtn) saveBtn.disabled = false;
    if (sendBtn) sendBtn.disabled = !cfg.configured;
  }

  function setReadOnly(flag) {
    readOnlyMode = !!flag;
    const disabled = !!flag;
    if (campaignNameInput) campaignNameInput.disabled = disabled;
    if (campaignSubjectInput) campaignSubjectInput.disabled = disabled;
    if (campaignPreviewInput) campaignPreviewInput.disabled = disabled;
    if (audienceModalSelect) audienceModalSelect.disabled = disabled;
    if (quill) quill.enable(!disabled);
    if (saveBtn) saveBtn.classList.toggle('d-none', disabled);
    if (sendBtn) sendBtn.classList.toggle('d-none', disabled);
    if (modalTitle) modalTitle.textContent = disabled ? 'View Campaign' : (campaignIdInput && campaignIdInput.value ? 'Edit Campaign' : 'New Campaign');
  }

  function openCampaignModal(rowId, forceReadOnly = false) {
    const row = campaigns.find((item) => String(item.id) === String(rowId));
    resetForm();
    if (!row) {
      setAudienceSelectValue(audienceModalSelect, audienceSelect ? audienceSelect.value : (cfg.defaultAudienceId || ''));
      if (modalTitle) modalTitle.textContent = 'New Campaign';
      if (modal) modal.show();
      return;
    }
    if (campaignIdInput) campaignIdInput.value = String(row.id || '');
    if (campaignNameInput) campaignNameInput.value = row.name || '';
    if (campaignSubjectInput) campaignSubjectInput.value = row.subject_line || '';
    if (campaignPreviewInput) campaignPreviewInput.value = row.preview_text || '';
    setAudienceSelectValue(audienceModalSelect, row.audience_id || cfg.defaultAudienceId || '');
    if (campaignMeta) {
      const parts = [];
      if (row.mailchimp_campaign_id) {
        parts.push(`Remote ID: ${row.mailchimp_campaign_id}`);
      }
      if (row.updated_at) {
        parts.push(`Updated: ${fmtDate(row.updated_at)}`);
      }
      campaignMeta.textContent = parts.join(' · ');
    }
    if (quill) {
      quill.root.innerHTML = row.html_body || '<p><br></p>';
    }
    const isReadOnly = forceReadOnly || String(row.status || '').toLowerCase() === 'sent';
    setReadOnly(isReadOnly);
    if (modal) modal.show();
  }

  function renderRows(rows) {
    if (!tableBody) return;
    tableBody.innerHTML = '';
    if (!rows.length) {
      if (emptyState) emptyState.classList.remove('d-none');
      return;
    }
    if (emptyState) emptyState.classList.add('d-none');
    rows.forEach((row) => {
      const tr = document.createElement('tr');
      const snippet = stripHtml(row.html_body || '');
      const lastError = row.last_error ? `<div class="small text-danger mt-1">${esc(row.last_error)}</div>` : '';
      const actions = [];
      if (String(row.status || '').toLowerCase() === 'sent') {
        actions.push(`<button type="button" class="btn btn-sm btn-outline-primary mc-view" data-id="${esc(row.id)}"><i class="ri-eye-line me-1"></i>View</button>`);
      } else {
        actions.push(`<button type="button" class="btn btn-sm btn-outline-primary mc-edit" data-id="${esc(row.id)}"><i class="ri-pencil-line me-1"></i>Edit</button>`);
        actions.push(`<button type="button" class="btn btn-sm btn-soft-danger mc-delete" data-id="${esc(row.id)}"><i class="ri-delete-bin-line me-1"></i>Delete</button>`);
      }
      tr.innerHTML = `
        <td>
          <div class="fw-semibold">${esc(row.name || '-')}</div>
          <div class="mailchimp-snippet">${esc(snippet || 'No body preview available yet.')}</div>
          ${lastError}
        </td>
        <td>
          <div class="fw-semibold">${esc(row.audience_name || row.audience_id || '-')}</div>
          <div class="small text-muted">${esc(row.audience_id || '')}</div>
        </td>
        <td>${esc(row.subject_line || '-')}</td>
        <td>${statusBadge(row.status)}</td>
        <td>
          <div class="fw-semibold">${esc(fmtDate(row.updated_at))}</div>
          <div class="mailchimp-meta text-muted">${row.remote_status ? esc(row.remote_status) : '&nbsp;'}</div>
        </td>
        <td>
          <div class="d-flex flex-wrap gap-1 mailchimp-actions">${actions.join('')}</div>
        </td>
      `;
      tableBody.appendChild(tr);
    });
  }

  function loadCampaigns() {
    if (!tableBody) return Promise.resolve();
    const qs = new URLSearchParams();
    if (searchInput && searchInput.value.trim()) qs.set('q', searchInput.value.trim());
    if (statusFilter && statusFilter.value) qs.set('status', statusFilter.value);
    qs.set('limit', '200');
    return apiFetchDetailed(apiUrl + '?' + qs.toString(), { method: 'GET' })
      .then((resp) => {
        if (!resp.ok) {
          throw new Error((resp.data && (resp.data.error || resp.data.message)) || resp.statusText || 'Unable to load campaigns');
        }
        campaigns = Array.isArray(resp.data?.data) ? resp.data.data : [];
        renderRows(campaigns);
        if (resp.data?.mailchimp) {
          cfg.configured = !!resp.data.mailchimp.configured;
        }
        if (sendBtn) sendBtn.disabled = !cfg.configured || readOnlyMode;
        if (syncBtn) syncBtn.disabled = !cfg.configured;
        return resp;
      })
      .catch((err) => {
        campaigns = [];
        renderRows([]);
        setPageAlert(err.message || 'Unable to load campaigns', 'danger');
      });
  }

  function loadAudiences() {
    const seed = audiences.length ? audiences.slice() : audienceSeed.slice();
    audiences = seed;
    renderAudienceOptions();
    const selected = audienceSelect && audienceSelect.value ? audienceSelect.value : (cfg.defaultAudienceId || '');
    setAudienceSelectValue(audienceSelect, selected);
    setAudienceSelectValue(audienceModalSelect, selected);

    if (!cfg.configured) {
      if (syncBtn) syncBtn.disabled = true;
      if (sendBtn) sendBtn.disabled = true;
      return Promise.resolve(audiences);
    }

    return apiFetchDetailed(apiUrl + '?action=audiences', { method: 'GET' })
      .then((resp) => {
        if (!resp.ok) {
          throw new Error((resp.data && (resp.data.error || resp.data.message)) || resp.statusText || 'Unable to load audiences');
        }
        audiences = Array.isArray(resp.data?.data) ? resp.data.data : [];
        if (!audiences.length && audienceSeed.length) {
          audiences = audienceSeed.slice();
        }
        renderAudienceOptions();
        const defaultAudience = String(resp.data?.mailchimp?.default_audience_id || cfg.defaultAudienceId || '');
        setAudienceSelectValue(audienceSelect, defaultAudience);
        setAudienceSelectValue(audienceModalSelect, defaultAudience);
        if (sendBtn) sendBtn.disabled = !cfg.configured || readOnlyMode;
        if (syncBtn) syncBtn.disabled = !cfg.configured;
        return audiences;
      })
      .catch((err) => {
        if (!audiences.length && audienceSeed.length) {
          audiences = audienceSeed.slice();
          renderAudienceOptions();
        }
        setPageAlert(err.message || 'Unable to load Mailchimp audiences. Using saved audience only.', 'warning');
        if (syncBtn) syncBtn.disabled = true;
        if (sendBtn) sendBtn.disabled = true;
        return audiences;
      });
  }

  function collectPayload(sendNow) {
    const html = quill ? quill.root.innerHTML : '';
    const text = stripHtml(html);
    const audienceId = audienceModalSelect ? String(audienceModalSelect.value || '') : '';
    const audienceName = audienceModalSelect && audienceModalSelect.selectedOptions && audienceModalSelect.selectedOptions[0]
      ? audienceModalSelect.selectedOptions[0].textContent || ''
      : audienceId;
    return {
      id: campaignIdInput ? campaignIdInput.value : '',
      name: campaignNameInput ? campaignNameInput.value.trim() : '',
      subject_line: campaignSubjectInput ? campaignSubjectInput.value.trim() : '',
      preview_text: campaignPreviewInput ? campaignPreviewInput.value.trim() : '',
      audience_id: audienceId,
      audience_name: audienceName,
      html_body: html,
      send_now: sendNow ? '1' : '0',
      body_text: text
    };
  }

  function saveCampaign(sendNow) {
    if (!cfg.canManage) return;
    if (readOnlyMode && !sendNow) return;
    const payload = collectPayload(sendNow);
    if (!payload.name) {
      setFormAlert('Campaign name is required.');
      return;
    }
    if (!payload.subject_line) {
      setFormAlert('Subject line is required.');
      return;
    }
    if (!payload.audience_id) {
      setFormAlert('Audience/List ID is required.');
      return;
    }
    if (!payload.body_text) {
      setFormAlert('Campaign body is required.');
      return;
    }

    if (sendNow && !cfg.configured) {
      setFormAlert('Mailchimp is not enabled or incomplete in System Integrations.');
      return;
    }

    const method = payload.id ? 'PATCH' : 'POST';
    const spinnerButton = sendNow ? sendBtn : saveBtn;
    setBusy(spinnerButton, true);
    setFormAlert('');
    setPageAlert('');

    apiFetchDetailed(apiUrl, {
      method: method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
      .then((resp) => {
        if (!resp.ok) {
          const msg = (resp.data && (resp.data.error || resp.data.message)) || resp.statusText || 'Unable to save campaign';
          if (resp.data && resp.data.saved_local) {
            setFormAlert(msg + ' The draft was saved locally and marked failed until Mailchimp is available.');
            loadCampaigns();
          } else {
            setFormAlert(msg);
          }
          throw new Error(msg);
        }
        if (modal) modal.hide();
        setPageAlert(resp.data?.message || (sendNow ? 'Campaign sent.' : 'Campaign saved.'), 'success');
        resetForm();
        return loadCampaigns();
      })
      .catch((err) => {
        if (!String(err && err.message ? err.message : '').length) {
          setFormAlert('Unable to save campaign.');
        }
      })
      .finally(() => {
        setBusy(spinnerButton, false);
      });
  }

  function syncContacts() {
    if (!cfg.canManage) return;
    if (!cfg.configured) {
      setPageAlert('Mailchimp is not enabled or incomplete in System Integrations.', 'warning');
      return;
    }
    const audienceId = audienceSelect ? String(audienceSelect.value || '') : '';
    if (!audienceId) {
      setPageAlert('Select an audience before syncing contacts.', 'warning');
      return;
    }
    if (!confirmAction(`Sync all contacts into "${getAudienceLabel(audienceId)}"?`)) {
      return;
    }
    setBusy(syncBtn, true);
    setPageAlert('');
    apiFetchDetailed(apiUrl + '?action=sync_contacts', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ audience_id: audienceId })
    })
      .then((resp) => {
        if (!resp.ok) {
          throw new Error((resp.data && (resp.data.error || resp.data.message)) || resp.statusText || 'Unable to sync contacts');
        }
        const data = resp.data?.data || {};
        const count = Number(data.synced || 0);
        const failed = Number(data.failed || 0);
        const skipped = Number(data.skipped || 0);
        const message = `Synced ${count} contact${count === 1 ? '' : 's'}${failed > 0 ? `, ${failed} failed` : ''}${skipped > 0 ? `, ${skipped} duplicate(s) skipped` : ''}.`;
        setPageAlert(message, failed > 0 ? 'warning' : 'success');
        return loadCampaigns();
      })
      .catch((err) => {
        setPageAlert(err.message || 'Unable to sync contacts.', 'danger');
      })
      .finally(() => {
        setBusy(syncBtn, false);
      });
  }

  function deleteCampaign(id) {
    if (!cfg.canManage) return;
    const row = campaigns.find((item) => String(item.id) === String(id));
    if (!row) return;
    if (String(row.status || '').toLowerCase() === 'sent') {
      setPageAlert('Sent campaigns cannot be deleted.', 'warning');
      return;
    }
    if (!confirmAction(`Delete "${row.name || 'this campaign'}"?`)) {
      return;
    }
    setPageAlert('');
    apiFetchDetailed(apiUrl + '?id=' + encodeURIComponent(String(id)), {
      method: 'DELETE'
    })
      .then((resp) => {
        if (!resp.ok) {
          throw new Error((resp.data && (resp.data.error || resp.data.message)) || resp.statusText || 'Unable to delete campaign');
        }
        setPageAlert(resp.data?.message || 'Campaign deleted.', 'success');
        return loadCampaigns();
      })
      .catch((err) => {
        setPageAlert(err.message || 'Unable to delete campaign', 'danger');
      });
  }

  function initQuill() {
    if (quill || !document.getElementById('mailchimpEditor') || typeof Quill === 'undefined') {
      return;
    }
    quill = new Quill('#mailchimpEditor', {
      theme: 'snow',
      modules: {
        toolbar: [
          [{ header: [1, 2, false] }],
          ['bold', 'italic', 'underline'],
          [{ list: 'ordered' }, { list: 'bullet' }],
          ['link'],
          ['clean']
        ]
      }
    });
    if (!cfg.configured) {
      quill.enable(false);
    }
  }

  function wireEvents() {
    if (newBtn) {
      newBtn.addEventListener('click', () => {
        if (!cfg.canManage) return;
        resetForm();
        if (campaignMeta) campaignMeta.textContent = cfg.configured ? `From ${cfg.defaultFromName || ''} <${cfg.defaultFromEmail || ''}> · Reply-to ${cfg.replyToEmail || ''}`.trim() : 'Mailchimp is not fully configured yet.';
        setAudienceSelectValue(audienceModalSelect, audienceSelect ? audienceSelect.value : (cfg.defaultAudienceId || ''));
        setReadOnly(false);
        if (modalTitle) modalTitle.textContent = 'New Campaign';
        if (modal) modal.show();
      });
    }
    if (filterBtn) {
      filterBtn.addEventListener('click', () => loadCampaigns());
    }
    if (resetBtn) {
      resetBtn.addEventListener('click', () => {
        if (searchInput) searchInput.value = '';
        if (statusFilter) statusFilter.value = '';
        if (audienceSelect) setAudienceSelectValue(audienceSelect, cfg.defaultAudienceId || '');
        if (audienceModalSelect) setAudienceSelectValue(audienceModalSelect, cfg.defaultAudienceId || '');
        loadCampaigns();
      });
    }
    if (syncBtn) {
      syncBtn.addEventListener('click', () => syncContacts());
    }
    if (saveBtn) {
      saveBtn.addEventListener('click', () => saveCampaign(false));
    }
    if (sendBtn) {
      sendBtn.addEventListener('click', () => saveCampaign(true));
    }
    if (audienceSelect) {
      audienceSelect.addEventListener('change', () => {
        setAudienceSelectValue(audienceModalSelect, audienceSelect.value);
      });
    }
    if (audienceModalSelect) {
      audienceModalSelect.addEventListener('change', () => {
        setAudienceSelectValue(audienceSelect, audienceModalSelect.value);
      });
    }
    if (modalEl) {
      modalEl.addEventListener('shown.bs.modal', () => {
        initQuill();
        if (quill && campaignIdInput && !campaignIdInput.value) {
          quill.setText('');
        }
      });
      modalEl.addEventListener('hidden.bs.modal', () => {
        resetForm();
      });
    }
    if (tableBody) {
      tableBody.addEventListener('click', (event) => {
        const editBtn = event.target.closest('.mc-edit');
        const viewBtn = event.target.closest('.mc-view');
        const deleteBtn = event.target.closest('.mc-delete');
        if (editBtn) {
          event.preventDefault();
          openCampaignModal(editBtn.getAttribute('data-id') || '', false);
        } else if (viewBtn) {
          event.preventDefault();
          openCampaignModal(viewBtn.getAttribute('data-id') || '', true);
        } else if (deleteBtn) {
          event.preventDefault();
          deleteCampaign(deleteBtn.getAttribute('data-id') || '');
        }
      });
    }
  }

  document.addEventListener('DOMContentLoaded', () => {
    if (!tableBody) return;
    initQuill();
    wireEvents();
    renderAudienceOptions();
    setAudienceSelectValue(audienceSelect, cfg.defaultAudienceId || '');
    setAudienceSelectValue(audienceModalSelect, cfg.defaultAudienceId || '');
    if (saveBtn) saveBtn.disabled = false;
    if (sendBtn) sendBtn.disabled = !cfg.configured;
    if (syncBtn) syncBtn.disabled = !cfg.configured;
    if (!cfg.configured) {
      setPageAlert('Mailchimp is not fully enabled yet. Save the API key and audience details in System Integrations first.', 'warning');
    }
    Promise.resolve(loadAudiences()).finally(() => loadCampaigns());
  });
})();
