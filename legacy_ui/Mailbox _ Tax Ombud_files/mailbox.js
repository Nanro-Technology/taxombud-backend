/* eslint-disable */
(function() {
      const mailboxConfig = window.mailboxConfig || {};
      const mailboxEmail = mailboxConfig.mailboxEmail || '';
      const mailboxOptions = Array.isArray(mailboxConfig.mailboxOptions) ? mailboxConfig.mailboxOptions : [];
      const defaultMailboxId = mailboxConfig.defaultMailboxId || null;
      const apiMessages = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/mail/messages';
      const apiMessage = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/mail/message';
      const apiAttachment = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/mail/attachment';
      const listEl = document.getElementById('mail-list');
      const socialListEl = document.getElementById('social-mail-list');
      const promoListEl = document.getElementById('promotions-mail-list');
      const loader = document.getElementById('elmLoader');
      const errorBox = document.getElementById('unreadConversations');
      const mailError = document.getElementById('mail-error');
      const rangeEl = document.getElementById('mail-range');
      const masterCheck = document.getElementById('checkall');
      const detailSubject = document.querySelector('.email-subject-title');
      const detailFrom = document.getElementById('mail-detail-from');
      const detailTo = document.getElementById('mail-detail-to');
      const detailBody = document.getElementById('mail-detail-body');
      const detailDate = document.getElementById('mail-detail-date');
      const detailAttachments = document.getElementById('mail-detail-attachments');
      const composeForm = document.getElementById('compose-form');
      const composeAlert = document.getElementById('compose-alert');
      const composeBody = document.getElementById('compose-body');
      const composeSubmitBtn = document.getElementById('compose-submit');
      const composeAttachments = document.getElementById('compose-attachments');
      const mailboxLabel = document.getElementById('mailbox-label');
      const normalizeEmailAddress = (raw) => {
        const val = String(raw || '').trim();
        const m = val.match(/[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}/i);
        return m ? m[0].toLowerCase() : val;
      };
      const escapeHtml = (value) => String(value == null ? '' : value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
      let currentMessageId = null;
      let currentFolder = 'Inbox';
      let currentOffset = 0;
      let totalCount = 0;
      const pageSize = 50;
      let messages = [];
      let ckEditorInstance = null;
      let activeMailboxId = null;
      let lastSmtpFailure = null;
      let lastImapStatus = null; // null=unknown | true=ok | object=error

      function syncSelectionState() {
        const checkboxes = Array.from(document.querySelectorAll('.message-list .form-check-input'));
        const checked = checkboxes.filter(cb => cb.checked);
        if (masterCheck) {
          masterCheck.checked = checkboxes.length > 0 && checked.length === checkboxes.length;
          masterCheck.indeterminate = checked.length > 0 && checked.length < checkboxes.length;
        }
      }

      function clearDetailState() {
        currentMessageId = null;
        document.body.classList.remove('email-detail-show');
      }

      function getMailboxLabelById(id) {
        const match = mailboxOptions.find(opt => String(opt.id) === String(id));
        return match ? match.label : '';
      }

      function getMailboxOptionById(id) {
        return mailboxOptions.find(opt => String(opt.id) === String(id)) || null;
      }

      function getActiveMailboxDetails() {
        const opt = getMailboxOptionById(activeMailboxId);
        return opt && opt.details ? opt.details : null;
      }

      function setActiveMailbox(id) {
        if (!id) return;
        activeMailboxId = String(id);
        localStorage.setItem('mailbox_active_id', activeMailboxId);
        const label = getMailboxLabelById(activeMailboxId) || mailboxEmail || 'Not assigned';
        if (mailboxLabel) {
          mailboxLabel.textContent = 'Active Mailbox: ' + label;
        }
        const switchLabel = document.getElementById('mailbox-switch-label');
        const switchInfo  = document.getElementById('mailbox-switch-info');
        if (switchLabel) {
          const atIdx = label.indexOf('@');
          const shortLabel = atIdx > 0 ? label.substring(0, atIdx) : label;
          switchLabel.textContent = shortLabel;
          switchLabel.title = '';
        }
        if (switchInfo) {
          switchInfo.classList.toggle('d-none', !label.includes('@'));
          switchInfo.setAttribute('title', label);
          switchInfo.setAttribute('data-bs-original-title', label);
          const existingTip = bootstrap.Tooltip.getInstance(switchInfo);
          if (existingTip) existingTip.dispose();
          new bootstrap.Tooltip(switchInfo);
        }
      }

      function setText(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value || '-';
      }

      function openComposeModalWithData(payload = {}) {
        const toInput = document.querySelector('#compose-form [name="to"]');
        const ccInput = document.querySelector('#compose-form [name="cc"]');
        const bccInput = document.querySelector('#compose-form [name="bcc"]');
        const subjInput = document.querySelector('#compose-form [name="subject"]');
        const bodyInput = document.querySelector('#compose-form [name="body"]');
        const composeAlertLocal = document.getElementById('compose-alert');
        const attachmentPreview = document.getElementById('compose-attachments-preview');

        if (toInput) toInput.value = payload.to || '';
        if (ccInput) ccInput.value = payload.cc || '';
        if (bccInput) bccInput.value = payload.bcc || '';
        if (subjInput) subjInput.value = payload.subject || '';

        const bodyHtml = payload.bodyHtml || '';
        if (ckEditorInstance && typeof ckEditorInstance.setData === 'function') {
          ckEditorInstance.setData(bodyHtml);
        } else if (bodyInput) {
          bodyInput.value = bodyHtml;
        }

        if (composeAlertLocal) {
          composeAlertLocal.classList.add('d-none');
          composeAlertLocal.textContent = '';
        }
        
        if (composeAttachments) {
          composeAttachments.value = '';
          if (attachmentPreview) attachmentPreview.innerHTML = '';
        }

        const composeModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('composemodal'));
        composeModal.show();
      }

      function renderSmtpDetails() {
        const details = getActiveMailboxDetails();
        const emptyBox = document.getElementById('smtp-details-empty');
        const lastErrWrap = document.getElementById('smtp-send-error-wrap');
        const lastErrText = document.getElementById('smtp-send-error-text');
        const label = getMailboxLabelById(activeMailboxId) || mailboxEmail || 'Not assigned';

        setText('smtp-detail-label', label);

        if (!details) {
          if (emptyBox) emptyBox.classList.remove('d-none');
          ['smtp-detail-from','smtp-detail-reply','smtp-detail-host','smtp-detail-port','smtp-detail-imap-port','smtp-detail-user','smtp-detail-pass','smtp-detail-ssl']
            .forEach((id) => setText(id, '-'));
        } else {
          if (emptyBox) emptyBox.classList.add('d-none');
          setText('smtp-detail-from', details.from_email || '');
          setText('smtp-detail-reply', details.reply_to || '');
          setText('smtp-detail-host', details.host || '');
          setText('smtp-detail-port', details.outgoing_port || '');
          setText('smtp-detail-imap-port', details.incoming_port || '');
          setText('smtp-detail-user', details.username || '');
          setText('smtp-detail-pass', details.has_password ? 'Configured' : 'Not set');
          setText('smtp-detail-ssl', details.use_ssl ? 'Enabled' : 'Disabled');
        }

        if (lastSmtpFailure && String(lastSmtpFailure.mailbox_id || '') === String(activeMailboxId || '')) {
          if (lastErrText) lastErrText.textContent = lastSmtpFailure.error || 'SMTP send failed.';
          if (lastErrWrap) lastErrWrap.classList.remove('d-none');
        } else if (lastErrWrap) {
          lastErrWrap.classList.add('d-none');
        }
      }

      function updateImapStatusBadge(status) {
        const badge = document.getElementById('imap-status-badge');
        const cell  = document.getElementById('smtp-detail-imap-status');
        if (!activeMailboxId) {
          if (badge) {
            badge.className = 'badge bg-secondary';
            badge.title = 'No mailbox assigned';
            badge.innerHTML = '<i class="ri-mail-close-line me-1"></i>No mailbox';
          }
          if (cell) cell.innerHTML = '<span class="text-muted">No mailbox assigned</span>';
          return;
        }
        if (status === 'checking') {
          if (badge) {
            badge.className = 'badge bg-secondary';
            badge.title = 'Checking IMAP connection…';
            badge.innerHTML = '<span class="spinner-border me-1" style="width:.55rem;height:.55rem;border-width:.1em" role="status" aria-hidden="true"></span>Checking';
          }
          if (cell) cell.innerHTML = '<span class="text-muted">Checking…</span>';
        } else if (status === true) {
          if (badge) {
            badge.className = 'badge bg-success';
            badge.title = 'IMAP connection is active';
            badge.innerHTML = '<i class="ri-checkbox-circle-fill me-1"></i>Connected';
          }
          if (cell) cell.innerHTML = '<span class="text-success fw-semibold">Connected</span>';
        } else if (status && typeof status === 'object') {
          const msg    = status.warning || 'IMAP connection failed';
          const errors = Array.isArray(status.imap_errors) ? [...new Set(status.imap_errors)] : [];
          if (badge) {
            badge.className = 'badge bg-danger';
            badge.title = msg;
            badge.innerHTML = '<i class="ri-error-warning-fill me-1"></i>IMAP Error';
          }
          if (cell) {
            cell.innerHTML = `<span class="text-danger fw-semibold">${msg}</span>` +
              (errors.length ? `<div class="text-danger small mt-1">${errors.join('<br>')}</div>` : '');
          }
        }
      }

      function initMailboxSwitcher() {
        if (!Array.isArray(mailboxOptions) || mailboxOptions.length === 0) return;
        const stored = localStorage.getItem('mailbox_active_id');
        const initial = mailboxOptions.some(opt => String(opt.id) === String(stored)) ? stored : (defaultMailboxId || mailboxOptions[0]?.id);
        setActiveMailbox(initial);
        const menu = document.getElementById('mailbox-switch-menu');
        if (!menu) return;
        menu.innerHTML = '';
        mailboxOptions.forEach((opt) => {
          const li = document.createElement('li');
          const a = document.createElement('a');
          a.className = 'dropdown-item d-flex align-items-center justify-content-between';
          a.href = 'javascript:void(0);';
          a.dataset.mailboxId = opt.id;
          a.title = opt.label || 'Mailbox';
          a.innerHTML = `<span>${opt.label}</span>${String(opt.id) === String(activeMailboxId) ? '<i class="ri-check-line text-success ms-2"></i>' : ''}`;
          a.addEventListener('click', () => {
            setActiveMailbox(opt.id);
            currentOffset = 0;
            menu.querySelectorAll('a').forEach((item) => {
              const id = item.dataset.mailboxId;
              item.title = getMailboxLabelById(id) || 'Mailbox';
              item.innerHTML = `<span>${getMailboxLabelById(id) || 'Mailbox'}</span>${String(id) === String(activeMailboxId) ? '<i class="ri-check-line text-success ms-2"></i>' : ''}`;
            });
            renderSmtpDetails();
            loadMessages(currentFolder);
          });
          li.appendChild(a);
          menu.appendChild(li);
        });
      }

      function setActiveFolder(folder) {
        currentFolder = folder || 'Inbox';
    }

      function normalizeHtml(html) {
        let raw = String(html || '').trim();
        if (!raw) return '';
        if (/&lt;|&gt;|&amp;lt;|&amp;gt;/.test(raw)) {
          const txt = document.createElement('textarea');
          txt.innerHTML = raw;
          raw = txt.value;
        }
        let doc;
        try {
          doc = new DOMParser().parseFromString(raw, 'text/html');
        } catch (e) {
          return raw;
        }
        if (!doc || !doc.body) return raw;
        doc.querySelectorAll('script, style, iframe, object, embed, link, meta, title').forEach((el) => el.remove());
        const walker = document.createTreeWalker(doc.body, NodeFilter.SHOW_ELEMENT, null);
        while (walker.nextNode()) {
          const el = walker.currentNode;
          Array.from(el.attributes).forEach((attr) => {
            const name = attr.name.toLowerCase();
            const value = String(attr.value || '');
            if (name.startsWith('on') || name === 'style') {
              el.removeAttribute(attr.name);
              return;
            }
            if (name === 'href' || name === 'src') {
              const v = value.trim().toLowerCase();
              if (v.startsWith('javascript:') || v.startsWith('data:text/html')) {
                el.removeAttribute(attr.name);
                return;
              }
            }
          });
        }
        return doc.body.innerHTML || raw;
      }

      document.querySelectorAll('.folder-link').forEach(link => {
        link.addEventListener('click', (e) => {
          e.preventDefault();

          if (typeof window.showGlobalLoading === 'function') {
              window.showGlobalLoading('Loading mailbox...');
          }
          document.body.classList.add('mailbox-loading');

          document.querySelectorAll('.folder-link').forEach(a => a.classList.remove('active'));
          link.classList.add('active');

          const folder = link.getAttribute('data-folder') || 'Inbox';
          setActiveFolder(folder);
          if (mailboxLabel) {
              const label = getMailboxLabelById(activeMailboxId) || mailboxEmail || 'Not assigned';
              mailboxLabel.textContent = 'Active Mailbox: ' + label + ' — ' + folder;
          }
          currentOffset = 0;
          loadMessages(folder);
        });
      });

      function snippet(text, len = 80) {
        const clean = (text || '').replace(/<[^>]+>/g, '').replace(/\s+/g, ' ').trim();
        if (clean.length <= len) return clean;
        return clean.slice(0, len - 1) + '…';
      }

      function parseJsonSafe(resp) {
        return resp.text().then((txt) => {
          try {
            return { ok: resp.ok, json: JSON.parse(txt), raw: txt };
          } catch (e) {
            return { ok: resp.ok, json: null, raw: txt };
          }
        });
      }

      function showError(msg) {
        if (mailError) {
          mailError.textContent = msg || 'Unable to retrieve mails.';
          mailError.classList.remove('d-none');
        }
        if (!errorBox) return;
        errorBox.textContent = msg || '';
        errorBox.classList.remove('alert-warning');
        errorBox.classList.add('alert-danger');
        errorBox.classList.add('show');
      }
      function clearError() {
        if (mailError) {
          mailError.classList.add('d-none');
          mailError.textContent = '';
        }
        if (!errorBox) return;
        errorBox.classList.remove('alert-danger');
        errorBox.classList.add('alert-warning');
        errorBox.textContent = 'No Unread Conversations';
      }

      const btnSmtpDetails = document.getElementById('btnSmtpDetails');
      if (btnSmtpDetails) {
        btnSmtpDetails.addEventListener('click', () => {
          renderSmtpDetails();
        });
      }

      function fetchWithTimeout(url, options = {}, timeoutMs = 30000) {
        const controller = new AbortController();
        const id = setTimeout(() => controller.abort(), timeoutMs);
        return fetch(url, { ...options, signal: controller.signal }).finally(() => clearTimeout(id));
      }

      function withMailboxParam(url) {
        if (!activeMailboxId) return url;
        const sep = url.includes('?') ? '&' : '?';
        return url + sep + 'mailbox_id=' + encodeURIComponent(activeMailboxId);
      }

      function messageUrl(id) {
        return withMailboxParam(apiMessage + '?id=' + encodeURIComponent(id));
      }

      function attachmentUrl(messageId, attachment) {
        const params = new URLSearchParams({
          message_id: String(messageId)
        });
        if (attachment && attachment.storage_path) {
          params.set('path', attachment.storage_path);
        }
        if (activeMailboxId) {
          params.set('mailbox_id', activeMailboxId);
        }
        return apiAttachment + '?' + params.toString();
      }

      function messagesUrl(params) {
        const qs = new URLSearchParams(params || {});
        if (activeMailboxId) qs.set('mailbox_id', activeMailboxId);
        return apiMessages + '?' + qs.toString();
      }
      

      function renderList() {
        if (!listEl) return;
        if (masterCheck) { masterCheck.checked = false; masterCheck.indeterminate = false; }
        listEl.innerHTML = '';
        if (socialListEl) socialListEl.innerHTML = '<li class="text-muted py-4 text-center">All messages appear in Primary.</li>';
        if (promoListEl) promoListEl.innerHTML = '<li class="text-muted py-4 text-center">All messages appear in Primary.</li>';
        if (!messages.length) {
          listEl.innerHTML = '<li class="text-muted py-4 text-center">No messages found.</li>';
          if (rangeEl) rangeEl.textContent = '0 of 0';
          return;
        }
        messages.forEach(msg => {
          const li = document.createElement('li');
          const isUnread = !(msg.read_at || msg.is_read || (currentFolder && currentFolder.toLowerCase() === 'sent'));
          li.className = isUnread ? 'unread' : '';
          const subject = msg.subject || '(No Subject)';
          const fromLine = (currentFolder && currentFolder.toLowerCase() === 'sent')
            ? (msg.to_name || msg.to_email || 'Recipient')
            : (msg.from_name || msg.from_email || 'Sender');
          const dateText = (msg.sent_at || msg.created_at || '').split(' ')[0] || '';
          const teaserSource = msg.body_text || msg.body_html || '';
          const teaser = snippet(teaserSource) || '(No preview)';
          const attachmentCount = Array.isArray(msg.attachments) ? msg.attachments.length : 0;
          li.innerHTML = `
            <div class="col-mail col-mail-1">
              <div class="form-check checkbox-wrapper-mail fs-14">
                <input class="form-check-input" type="checkbox" value="${msg.id}" id="checkbox-${msg.id}">
                <label class="form-check-label" for="checkbox-${msg.id}"></label>
              </div>
              <input type="hidden" value="${msg.userImg || ''}" class="mail-userimg" />
              <button type="button" class="btn avatar-xs p-0 material-shadow-none favourite-btn fs-15">
                <i class="ri-star-fill"></i>
              </button>
              <a href="javascript:void(0);" class="title item-subject" data-id="${msg.id}"><span class="title-name">${fromLine}</span></a>
            </div>
            <div class="col-mail col-mail-2">
              <a href="javascript:void(0);" class="subject item-subject" data-id="${msg.id}">
                <span class="subject-title">${subject}</span> – <span class="teaser">${teaser}</span>
              </a>
              <div class="date d-flex align-items-center justify-content-end gap-2">
                ${attachmentCount ? `<span class="badge bg-info-subtle text-info"><i class="ri-attachment-2 me-1"></i>${attachmentCount}</span>` : ''}
                <span>${dateText}</span>
              </div>
            </div>
          `;
          const cb = li.querySelector('.form-check-input');
          if (cb) {
            cb.addEventListener('change', (e) => {
              li.classList.toggle('active', e.target.checked);
              syncSelectionState();
            });
          }
          li.querySelectorAll('.item-subject').forEach(a => a.addEventListener('click', () => loadDetail(msg.id)));
          listEl.appendChild(li);
        });
        if (rangeEl) {
          const start = messages.length ? currentOffset + 1 : 0;
          const end = messages.length ? currentOffset + messages.length : 0;
          rangeEl.textContent = `${start}-${end} of ${end || 0}`;
        }
      }

      function afterTrashAction(targets) {
        if (targets.includes(String(currentMessageId)) || targets.includes(currentMessageId)) {
          clearDetailState();
        }
        document.querySelectorAll('.message-list .form-check-input:checked').forEach((cb) => {
          cb.checked = false;
          cb.closest('li')?.classList.remove('active');
        });
        syncSelectionState();
        loadMessages(currentFolder);
      }

      function moveToTrash(ids = []) {
        const targets = ids.length ? ids : (currentMessageId ? [currentMessageId] : []);
        if (!targets.length) return;
        const permanentDelete = String(currentFolder || '').toLowerCase() === 'trash';
        const targetSet = new Set(targets.map((mid) => String(mid)));
        if (targetSet.size > 1 || (currentMessageId && targetSet.has(String(currentMessageId)))) {
          clearDetailState();
        }

        // Permanent delete: send ONE bulk request instead of one DELETE per
        // message. Firing hundreds of parallel requests overwhelmed the server
        // (Apache 503). The server deletes them all in a single request.
        if (permanentDelete) {
          const numericIds = targets.map((mid) => parseInt(mid, 10)).filter((n) => n > 0);
          const mbParam = (typeof activeMailboxId !== 'undefined' && activeMailboxId) ? activeMailboxId : null;
          fetchWithTimeout(apiMessage, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ action: 'bulk_delete', ids: numericIds, mailbox_id: mbParam })
          }, 120000)
            .then(parseJsonSafe)
            .then(({ ok, json, raw }) => {
              if (!ok) throw new Error((json && json.error) || raw || 'Unable to delete messages');
              afterTrashAction(targets);
              if (json && json.failed) {
                showError(json.failed + ' message(s) could not be deleted.');
              }
            })
            .catch(err => showError(err.message || 'Unable to delete messages'));
          return;
        }

        // Move to Trash (non-permanent): per-message PATCH (kept as-is).
        Promise.allSettled(targets.map((mid) => {
          return fetchWithTimeout(messageUrl(mid), {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ folder: 'Trash' })
          }, 30000).then(parseJsonSafe).then(({ ok, json, raw }) => {
            if (!ok) {
              throw new Error((json && json.error) || raw || 'Unable to move message to Trash');
            }
            return json || {};
          });
        }))
          .then((results) => {
            const failures = results.filter((r) => r.status === 'rejected');
            afterTrashAction(targets);
            if (failures.length) {
              const first = failures[0].reason;
              showError((first && first.message) || 'Some messages could not be moved to Trash.');
            }
          })
          .catch(err => showError(err.message || 'Unable to move to Trash'));
      }

      function runBulkMessageUpdate(ids, payload) {
        if (!ids.length) return Promise.resolve();
        return Promise.all(ids.map((mid) =>
          fetch(messageUrl(mid), {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
          }).then(parseJsonSafe).then(({ ok, json, raw }) => {
            if (!ok) {
              throw new Error((json && json.error) || raw || 'Unable to update message');
            }
            return json || {};
          })
        ));
      }

      function fetchFolder(folderName, offset = 0, limit = pageSize) {
        const qs = new URLSearchParams({ folder: folderName || 'Inbox', offset: String(offset), limit: String(limit) });
        return fetchWithTimeout(messagesUrl(Object.fromEntries(qs.entries())), {}, 30000)
          .then(parseJsonSafe)
          .then(({ ok, json, raw }) => {
            if (!ok) throw new Error((json && json.error) || raw || 'Unable to fetch ' + folderName);
            return {
              rows: (json && json.data) || [],
              total: (json && (json.total ?? json.count)) || 0,
              sync_warning: (json && json.sync_warning) || null
            };
          });
      }

      function updatePagerButtons() {
        const prev = document.getElementById('prevPageBtn');
        const next = document.getElementById('nextPageBtn');
        if (prev) prev.disabled = currentOffset === 0;
        if (next) next.disabled = (currentOffset + pageSize) >= totalCount;
      }

      function loadMessages(folder) {
        currentFolder = folder || 'Inbox';
        if (loader) loader.classList.remove('d-none');
        clearError();
        if (!activeMailboxId) {
          messages = [];
          totalCount = 0;
          updateImapStatusBadge(null);
          renderList();
          if (rangeEl) rangeEl.textContent = '0 of 0';
          updatePagerButtons();
          if (loader) loader.classList.add('d-none');
          if (typeof window.hideGlobalLoading === 'function') {
              window.hideGlobalLoading();
          }
          document.body.classList.remove('mailbox-loading');
          return;
        }
        if (['inbox', 'all'].includes((currentFolder || '').toLowerCase())) {
          updateImapStatusBadge('checking');
        }
        const isAll = (currentFolder && currentFolder.toLowerCase() === 'all');
        const folderParam = isAll ? 'Inbox' : currentFolder;

        const loadPromise = isAll
          ? Promise.all([fetchFolder('Inbox', 0, pageSize * 2), fetchFolder('Sent', 0, pageSize * 2)]).then(([inbox, sent]) => {
              const inboxRows = inbox?.rows || [];
              const sentRows = sent?.rows || [];
              const merged = [...inboxRows, ...sentRows];
              merged.sort((a, b) => {
                const da = new Date(a.sent_at || a.created_at || 0).getTime();
                const db = new Date(b.sent_at || b.created_at || 0).getTime();
                return db - da;
              });
              const total = (inbox?.total || inboxRows.length) + (sent?.total || sentRows.length);
              // propagate sync_warning if either inbox or sent has it
              let sync_warning = inbox?.sync_warning || sent?.sync_warning || null;
              return { rows: merged.slice(currentOffset, currentOffset + pageSize), total, sync_warning };
            })
          : fetchFolder(folderParam, currentOffset, pageSize);

        loadPromise
          .then((resp) => {
            // Surface soft warning if present
            if (resp && resp.sync_warning && resp.sync_warning.ok === false) {
                const warning = resp.sync_warning || {};
                const detailParts = [];
                if (warning.mailbox_host) detailParts.push(`Host: ${warning.mailbox_host}`);
                if (warning.mailbox_port) detailParts.push(`Port: ${warning.mailbox_port}`);
                if (Array.isArray(warning.imap_errors) && warning.imap_errors.length) {
                  detailParts.push(`IMAP: ${warning.imap_errors.join(' | ')}`);
                }
                const detail = detailParts.length ? ` (${detailParts.join(', ')})` : '';
                showError((warning.warning || 'Mailbox sync issue') + detail);
                lastImapStatus = warning;
                updateImapStatusBadge(warning);
            } else if (['inbox', 'all'].includes((currentFolder || '').toLowerCase())) {
                lastImapStatus = true;
                updateImapStatusBadge(true);
            }
            const rows = resp?.rows || [];
            const total = resp?.total || rows.length;
            messages = rows;
            totalCount = total;
            renderList();
            if (rangeEl) {
              const start = rows.length ? (currentOffset + 1) : 0;
              const end = rows.length ? (currentOffset + rows.length) : 0;
              rangeEl.textContent = `${start}-${end} of ${total}`;
            }
            updatePagerButtons();
          })
          .catch(err => {
            messages = [];
            totalCount = 0;
            renderList();
            showError(err.message || 'Unable to fetch inbox');
            if (rangeEl) rangeEl.textContent = '0 of 0';
            updatePagerButtons();
          })
          .finally(() => {
            if (loader) loader.classList.add('d-none');
            if (typeof window.hideGlobalLoading === 'function') {
                window.hideGlobalLoading();
            }
            document.body.classList.remove('mailbox-loading');
        });
      }

      function loadDetail(id) {
        if (!id) return;
        currentMessageId = id;
        if (detailSubject) detailSubject.textContent = 'Loading...';
        if (detailBody) detailBody.innerHTML = '<div class="text-muted small">Loading message...</div>';
        fetchWithTimeout(messageUrl(id), {}, 30000)
          .then(parseJsonSafe)
          .then(({ ok, json, raw }) => {
            if (!ok) throw new Error((json && json.error) || raw || 'Unable to load message');
            const data = json || {};
            const isDraft = String(data.folder || currentFolder || '').toLowerCase() === 'draft';
            if (isDraft) {
              openComposeModalWithData({
                to: normalizeEmailAddress(data.to_email || ''),
                cc: data.cc_email || '',
                bcc: data.bcc_email || '',
                subject: data.subject || '',
                bodyHtml: data.body_html || data.body_text || ''
              });
              return;
            }
            if (detailSubject) detailSubject.textContent = data.subject || '(No Subject)';
            if (detailFrom) detailFrom.textContent = data.from_email || '';
            if (detailTo) detailTo.textContent = data.to_email || '';
            if (detailDate) detailDate.textContent = data.sent_at || data.created_at || '-';
            if (detailBody) {
              if (data.body_html) {
                detailBody.innerHTML = normalizeHtml(data.body_html);
              } else if (data.body_text) {
                detailBody.innerHTML = normalizeHtml(data.body_text);
              } else {
                detailBody.innerHTML = '';
              }
            }
            if (detailAttachments) {
              const atts = Array.isArray(data.attachments) ? data.attachments : [];
              if (!atts.length) {
                detailAttachments.innerHTML = '';
              } else {
                detailAttachments.innerHTML = `<hr><h6 class="fw-semibold mb-2 mt-4">Attachments</h6>` + atts.map(att => {
                  const url = attachmentUrl(id, att);
                  const size = att.size_bytes ? ` (${Math.round(att.size_bytes/1024)} KB)` : '';
                  const available = att.available !== false;
                  return `<div class="d-flex align-items-center gap-2 mb-1">
                    <i class="ri-attachment-line text-muted"></i>
                    ${available
                      ? `<a href="${url}" target="_blank" rel="noopener" class="text-decoration-underline">${escapeHtml(att.filename || 'Attachment')}</a>`
                      : `<span class="text-muted">${escapeHtml(att.filename || 'Attachment')} <span class="badge bg-danger-subtle text-danger ms-1">Unavailable</span></span>`}
                    <span class="text-muted small">${size}</span>
                  </div>`;
                }).join('');
              }
            }
            // highlight selected row
            document.querySelectorAll('.message-list li').forEach(li => li.classList.remove('active-detail'));
            const activeLi = document.querySelector(`.message-list .form-check-input[value="${id}"]`)?.closest('li');
            if (activeLi) activeLi.classList.add('active-detail');
            document.body.classList.add('email-detail-show');
            const replyBtn = document.getElementById('replyBtn');
            if (replyBtn) {
              replyBtn.classList.remove('d-none');
              replyBtn.onclick = () => {
                openComposeModalWithData({
                  to: normalizeEmailAddress(data.from_email || ''),
                  subject: data.subject ? 'Re: ' + data.subject : '',
                  bodyHtml: ''
                });
              };
            }
            // mark as read
            fetchWithTimeout(messageUrl(id), {
              method: 'PATCH',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ mark_read: true })
            }).then(() => {
              loadMessages(currentFolder);
            }).catch(() => {});
          })
          .catch(err => showError(err.message || 'Unable to load message'));
      }

      const refreshBtn = document.getElementById('refresh-mails');
      if (refreshBtn) refreshBtn.addEventListener('click', () => {
          currentOffset = 0;
          loadMessages(currentFolder);
      });

      if (composeAttachments) {
        const attachmentPreview = document.getElementById('compose-attachments-preview');
        composeAttachments.addEventListener('change', (e) => {
          if (!attachmentPreview) return;
          if (!composeAttachments.files || composeAttachments.files.length === 0) {
            attachmentPreview.innerHTML = '';
            return;
          }
          const fileList = Array.from(composeAttachments.files).map((f, i) => {
            const sizeKB = Math.round(f.size / 1024);
            return `<span class="badge bg-light text-dark me-2 mb-2"><i class="ri-attachment-2 me-1"></i>${f.name} (${sizeKB}KB)</span>`;
          }).join('');
          attachmentPreview.innerHTML = `<div class="d-flex flex-wrap gap-1">${fileList}</div>`;
        });
      }

      if (composeForm) {
        composeForm.addEventListener('submit', (e) => {
          e.preventDefault();
          if (composeAlert) { composeAlert.classList.add('d-none'); composeAlert.textContent = ''; }
          if (composeSubmitBtn) {
            composeSubmitBtn.disabled = true;
            const sp = composeSubmitBtn.querySelector('.spinner-border');
            if (sp) sp.classList.remove('d-none');
          }
          const to = composeForm.querySelector('[name="to"]')?.value.trim() || '';
          const toClean = normalizeEmailAddress(to);
          const subject = composeForm.querySelector('[name="subject"]')?.value.trim() || '';
          const bodyEl = composeForm.querySelector('[name="body"]');
          const body = ckEditorInstance ? ckEditorInstance.getData().trim() : (bodyEl ? bodyEl.value.trim() : (document.getElementById('compose-body')?.innerHTML || '').trim());
          if (!to || !subject || !body) {
            if (composeAlert) { composeAlert.textContent = 'To, subject, and body are required.'; composeAlert.classList.remove('d-none'); }
            if (composeSubmitBtn) {
              composeSubmitBtn.disabled = false;
              const sp = composeSubmitBtn.querySelector('.spinner-border');
              if (sp) sp.classList.add('d-none');
            }
            return;
          }
          
          let attachmentInfo = '';
          if (composeAttachments && composeAttachments.files && composeAttachments.files.length) {
            const attachmentList = Array.from(composeAttachments.files).map(f => f.name);
            attachmentInfo = ' with ' + attachmentList.length + ' attachment' + (attachmentList.length === 1 ? '' : 's') + ' (' + attachmentList.join(', ') + ')';
          }
          
          const fd = new FormData();
          fd.append('to', toClean || to);
          fd.append('subject', subject);
          fd.append('body_html', body);
          const ccVal = composeForm.querySelector('[name="cc"]')?.value.trim() || '';
          const bccVal = composeForm.querySelector('[name="bcc"]')?.value.trim() || '';
          if (ccVal) fd.append('cc', ccVal);
          if (bccVal) fd.append('bcc', bccVal);
          if (composeAttachments && composeAttachments.files && composeAttachments.files.length) {
            Array.from(composeAttachments.files).forEach(f => fd.append('attachments[]', f));
          }
          if (activeMailboxId) {
            fd.append('mailbox_id', activeMailboxId);
          }
          
          if (composeAlert) { 
            composeAlert.textContent = 'Sending email' + attachmentInfo + '...'; 
            composeAlert.classList.remove('d-none');
            composeAlert.classList.remove('alert-danger');
            composeAlert.classList.add('alert-info');
          }
          
          fetch(apiMessages + (activeMailboxId ? ('?mailbox_id=' + encodeURIComponent(activeMailboxId)) : ''), {
            method: 'POST',
            body: fd
          })
            .then(parseJsonSafe)
            .then(({ ok, json, raw }) => {
              if (!ok) throw new Error((json && json.error) || raw || 'Unable to send message');
              if (json && json.status === 'draft') {
                lastSmtpFailure = {
                  mailbox_id: activeMailboxId,
                  error: (json && json.send_error) || 'SMTP send failed',
                  debug: (json && json.smtp_debug) || null
                };
                renderSmtpDetails();
                throw new Error('Message moved to Draft. SMTP failed: ' + ((json && json.send_error) || 'Unable to deliver using current SMTP settings.'));
              }
              lastSmtpFailure = null;
              renderSmtpDetails();
              if (composeAlert) { 
                composeAlert.textContent = 'Email sent successfully' + attachmentInfo + '.'; 
                composeAlert.classList.remove('alert-danger', 'alert-info');
                composeAlert.classList.add('alert-success');
              }
              // close modal
              setTimeout(() => {
                const composeModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('composemodal'));
                if (composeModal) composeModal.hide();
                setActiveFolder('Sent');
                loadMessages('Sent');
              }, 1500);
            })
            .catch(err => {
              if (composeAlert) { 
                composeAlert.textContent = err.message || 'Unable to send message'; 
                composeAlert.classList.remove('d-none', 'alert-info', 'alert-success');
                composeAlert.classList.add('alert-danger');
              }
              if (lastSmtpFailure) {
                setActiveFolder('Draft');
                loadMessages('Draft');
              }
            })
            .finally(() => {
              if (composeSubmitBtn) {
                composeSubmitBtn.disabled = false;
                const sp = composeSubmitBtn.querySelector('.spinner-border');
                if (sp) sp.classList.add('d-none');
              }
            });
        });
      }

      document.querySelectorAll('.remove-mail').forEach(btn => {
        btn.addEventListener('click', (e) => {
          e.preventDefault();
          moveToTrash();
        });
      });

      if (masterCheck) {
        masterCheck.addEventListener('change', (e) => {
          const checked = e.target.checked;
          document.querySelectorAll('.message-list .form-check-input').forEach(cb => {
            cb.checked = checked;
            cb.closest('li')?.classList.toggle('active', checked);
          });
          syncSelectionState();
        });
      }

      function getSelectedIds() {
        return Array.from(document.querySelectorAll('.message-list .form-check-input:checked')).map(cb => cb.value).filter(Boolean);
      }

      const btnMarkRead = document.getElementById('btnMarkRead');
      if (btnMarkRead) {
        btnMarkRead.addEventListener('click', () => {
          const ids = getSelectedIds();
          if (!ids.length) return;
          runBulkMessageUpdate(ids, { mark_read: true })
            .then(() => loadMessages(currentFolder))
            .catch(err => showError(err.message || 'Unable to mark messages as read'));
        });
      }

      const btnDelete = document.getElementById('btnDelete');
      if (btnDelete) {
        btnDelete.addEventListener('click', () => {
          const ids = getSelectedIds();
          moveToTrash(ids);
        });
      }

      const bulkMarkRead = document.getElementById('bulkMarkRead');
      if (bulkMarkRead) {
        bulkMarkRead.addEventListener('click', () => {
          const ids = getSelectedIds();
          if (!ids.length) return;
          runBulkMessageUpdate(ids, { mark_read: true })
            .then(() => loadMessages(currentFolder))
            .catch(err => showError(err.message || 'Unable to mark messages as read'));
        });
      }

      const bulkMarkUnread = document.getElementById('bulkMarkUnread');
      if (bulkMarkUnread) {
        bulkMarkUnread.addEventListener('click', () => {
          const ids = getSelectedIds();
          if (!ids.length) return;
          runBulkMessageUpdate(ids, { mark_read: false })
            .then(() => loadMessages(currentFolder))
            .catch(err => showError(err.message || 'Unable to mark messages as unread'));
        });
      }

      const bulkDelete = document.getElementById('bulkDelete');
      if (bulkDelete) {
        bulkDelete.addEventListener('click', () => {
          const ids = getSelectedIds();
          moveToTrash(ids);
        });
      }

      const detailMarkRead = document.getElementById('detailMarkRead');
      if (detailMarkRead) {
        detailMarkRead.addEventListener('click', () => {
          if (!currentMessageId) return;
          fetch(messageUrl(currentMessageId), {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ mark_read: true })
          }).finally(() => loadMessages(currentFolder));
        });
      }

      const detailMarkUnread = document.getElementById('detailMarkUnread');
      if (detailMarkUnread) {
        detailMarkUnread.addEventListener('click', () => {
          if (!currentMessageId) return;
          fetch(messageUrl(currentMessageId), {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ mark_read: false })
          }).finally(() => loadMessages(currentFolder));
        });
      }

      const detailDelete = document.getElementById('detailDelete');
      if (detailDelete) {
        detailDelete.addEventListener('click', () => {
          if (!currentMessageId) return;
          moveToTrash([currentMessageId]);
        });
      }

      const prevMailBtn = document.getElementById('prevMailBtn');
      const nextMailBtn = document.getElementById('nextMailBtn');
      function openSibling(offset) {
        if (!currentMessageId || !messages.length) return;
        const idx = messages.findIndex(m => String(m.id) === String(currentMessageId));
        if (idx === -1) return;
        const targetIdx = idx + offset;
        if (targetIdx < 0 || targetIdx >= messages.length) return;
        const target = messages[targetIdx];
        loadDetail(target.id);
      }
      if (prevMailBtn) prevMailBtn.addEventListener('click', () => openSibling(-1));
      if (nextMailBtn) nextMailBtn.addEventListener('click', () => openSibling(1));

      let refreshTimer = null;
      function scheduleRefresh() {
        const interval = window.appConfig?.mailboxRefreshMs ?? 180000;
        refreshTimer = setInterval(() => {
          if (document.hidden) return;
          if (loader && !loader.classList.contains('d-none')) return;
          loadMessages(currentFolder);
        }, interval);
      }
      scheduleRefresh();

      const closeBtnEmail = document.getElementById('close-btn-email');
      if (closeBtnEmail) {
        closeBtnEmail.addEventListener('click', () => {
          document.body.classList.remove('email-detail-show');
        });
      }

      const prevPageBtn = document.getElementById('prevPageBtn');
      const nextPageBtn = document.getElementById('nextPageBtn');
      if (prevPageBtn) {
        prevPageBtn.addEventListener('click', () => {
          if (currentOffset === 0) return;
          currentOffset = Math.max(0, currentOffset - pageSize);
          loadMessages(currentFolder);
        });
      }
      if (nextPageBtn) {
        nextPageBtn.addEventListener('click', () => {
          if ((currentOffset + pageSize) >= totalCount) return;
          currentOffset += pageSize;
          loadMessages(currentFolder);
        });
      }

      initMailboxSwitcher();
      renderSmtpDetails();
      loadMessages('Inbox');

      function applyComposeFromUrl() {
        const params = new URLSearchParams(window.location.search);
        if (!params.get('compose') && !params.get('to')) return;
        const toParam = params.get('to') || '';
        const nameParam = params.get('name') || '';
        const subjectParam = params.get('subject') || '';
        const toInput = document.querySelector('#compose-form [name="to"]');
        const subjInput = document.querySelector('#compose-form [name="subject"]');
        openComposeModalWithData({
          to: toParam ? normalizeEmailAddress(toParam) : '',
          subject: subjectParam || (nameParam ? `Hello ${nameParam}` : ''),
          bodyHtml: ''
        });
        const modalEl = document.getElementById('composemodal');
        if (!modalEl) return;
        // Fallback: trigger the compose button if modal didn't render
        setTimeout(() => {
          if (!modalEl.classList.contains('show')) {
            const btn = document.querySelector('[data-bs-target="#composemodal"]');
            if (btn) btn.click();
          }
        }, 200);
      }

      if (composeBody && window.ClassicEditor) {
        ClassicEditor.create(composeBody).then(editor => {
          ckEditorInstance = editor;
        }).catch(() => {});
      }

      setTimeout(applyComposeFromUrl, 150);
    })();
