/* eslint-disable */
(function() {
      const appCallConfig = window.appCallConfig || {};
      const apiCalls = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/calls/index';
      const apiCallDetail = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/calls/detail';
      const apiContacts = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/contacts/index';
      const orgEnabled = !!(window.__mmkModulesEnabled && window.__mmkModulesEnabled.organizations);
      const leadsEnabled = !!(window.__mmkModulesEnabled && window.__mmkModulesEnabled.leads);
      const apiorganizations = orgEnabled ? ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/organizations/index') : null;
      const apiCases = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/cases/index';
      const apiLeads = leadsEnabled ? ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/leads/index') : null;
      const apiExport = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/calls/export';
      const canDeleteCall = Boolean(appCallConfig.canDeleteCall);
      const callTable = $('#callTable').DataTable({ ordering: false });
      const searchInput = document.getElementById('callSearch');
      const dirSel = document.getElementById('callDirection');
      const statusSel = document.getElementById('callStatus');
      const dateRangeSel = document.getElementById('callDateRange');
      const agentSel = document.getElementById('callAgent');
      const exportBtn = document.getElementById('callExportBtn');
      const alertBox = document.getElementById('callAlert');
      const btnSearch = document.getElementById('btnCallSearch');
      const btnReset = document.getElementById('btnCallReset');
      const btnNew = document.getElementById('btnNewCall');
      const modalEl = document.getElementById('callModal');
      const modal = modalEl ? new bootstrap.Modal(modalEl) : null;
      const confirmDelEl = document.getElementById('confirmDeleteCallModal');
      const confirmDelModal = confirmDelEl ? new bootstrap.Modal(confirmDelEl) : null;
      const confirmDelBtn = document.getElementById('confirmDeleteCallBtn');
      let pendingDeleteId = null;
      const form = document.getElementById('callForm');

      // Link selector in modal.
      const linkTypeSel = document.getElementById('call_link_type');
      // Optional module guard: remove unified-search types that depend on missing modules.
      if (linkTypeSel && linkTypeSel.options) {
        const toRemove = [];
        for (let i = 0; i < linkTypeSel.options.length; i++) {
          const opt = linkTypeSel.options[i];
          const val = (opt && opt.value) ? String(opt.value).toLowerCase() : '';
          if (val === 'organization' && !orgEnabled) toRemove.push(opt);
          if (val === 'lead' && !leadsEnabled) toRemove.push(opt);
        }
        toRemove.forEach((opt) => {
          try { linkTypeSel.removeChild(opt); } catch (e) {}
        });
        const cur = String(linkTypeSel.value || '').toLowerCase();
        if ((cur === 'organization' && !orgEnabled) || (cur === 'lead' && !leadsEnabled)) {
          linkTypeSel.value = 'contact';
        }
      }
      const formAlert = document.getElementById('callFormAlert');
      const saveBtn = document.getElementById('callSaveBtn');
      let editingId = null;
      const identityVerificationEnabled = !!appCallConfig.identityVerificationEnabled;
      const identityVerificationContactsEnabled = appCallConfig.identityVerificationContactsEnabled !== false;
      const identityVerificationOrganizationsEnabled = appCallConfig.identityVerificationOrganizationsEnabled !== false;
      const identityVerificationApi = appCallConfig.identityVerificationApi || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/identity_verification');
      const identityWrap = document.getElementById('callIdentityVerificationWrap');
      const identityModeInput = document.getElementById('callIdentityMode');
      const personalTokenInput = document.getElementById('call_identity_personal_token');
      const corporateTokenInput = document.getElementById('call_identity_corporate_token');
      const personalBlockRoot = document.getElementById('callPersonalVerificationBlock');
      const corporateBlockRoot = document.getElementById('callCorporateVerificationBlock');
      let personalVerificationBlock = null;
      let corporateVerificationBlock = null;
      const linkSearchWrap = document.getElementById('call_link_search_wrap');
      const linkSearch = document.getElementById('call_link_search');
      const linkResults = document.getElementById('call_link_results');
      const linkSelected = document.getElementById('call_link_selected');
      const linkStatus = document.getElementById('call_link_search_status');
      const contactIdInput = document.getElementById('call_contact_id');
      const organizationIdInput = document.getElementById('call_organization_id');
      const caseIdInput = document.getElementById('call_case_id');
      const leadIdInput = document.getElementById('call_lead_id');
      const phoneCodeInput = document.getElementById('call_phone_code');
      const phoneLocalInput = document.getElementById('call_phone');
      const phoneFullInput = document.getElementById('call_phone_full');
      const countriesJson = (typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/geo/index?action=countries';
      const phoneHelper = window.PhoneHelper ? window.PhoneHelper.bind({
        countriesUrl: countriesJson,
        codeSelect: phoneCodeInput,
        localInput: phoneLocalInput,
        fullInput: phoneFullInput,
        defaultDial: '+234'
      }) : null;

      function initSelect2() {
        if (!(window.jQuery && jQuery.fn && jQuery.fn.select2)) return;
        const $agentFilter = jQuery('#callAgent');
        if ($agentFilter.length) {
          if ($agentFilter.data('select2')) $agentFilter.select2('destroy');
          $agentFilter.select2({
            width: '100%',
            placeholder: 'All',
            allowClear: true
          });
        }
        const $agentInput = jQuery('#call_agent');
        if ($agentInput.length) {
          if ($agentInput.data('select2')) $agentInput.select2('destroy');
          $agentInput.select2({
            width: '100%',
            dropdownParent: jQuery('#callModal'),
            placeholder: '-- Unassigned --',
            allowClear: true
          });
        }
      }

      function setAlert(msg) { if (alertBox) alertBox.textContent = msg || ''; }
      function setFormAlert(msg) {
        if (!formAlert) return;
        if (!msg) { formAlert.classList.add('d-none'); formAlert.textContent=''; return; }
        formAlert.classList.remove('d-none'); formAlert.textContent = msg;
      }

      function resolveDateRange() {
        const raw = dateRangeSel && dateRangeSel.value ? String(dateRangeSel.value).trim() : '';
        if (!raw) return { startAt: '', endAt: '' };
        const matches = raw.match(/\d{4}-\d{2}-\d{2}/g) || [];
        if (matches.length >= 2) return { startAt: matches[0], endAt: matches[1] };
        if (matches.length === 1) return { startAt: matches[0], endAt: matches[0] };
        return { startAt: '', endAt: '' };
      }

      function buildQuery() {
        const p = new URLSearchParams();
        if (searchInput && searchInput.value) p.set('q', searchInput.value.trim());
        if (dirSel && dirSel.value) p.set('direction', dirSel.value);
        if (statusSel && statusSel.value) p.set('status', statusSel.value);
        const range = resolveDateRange();
        if (range.startAt) p.set('start_at', range.startAt);
        if (range.endAt) p.set('end_at', range.endAt);
        if (agentSel && agentSel.value) p.set('agent_id', agentSel.value);
        p.set('limit', '200');
        return '?' + p.toString();
      }

      function buildExportParams() {
        const p = new URLSearchParams();
        if (searchInput && searchInput.value) p.set('q', searchInput.value.trim());
        if (dirSel && dirSel.value) p.set('direction', dirSel.value);
        if (statusSel && statusSel.value) p.set('status', statusSel.value);
        const range = resolveDateRange();
        if (range.startAt) p.set('start_at', range.startAt);
        if (range.endAt) p.set('end_at', range.endAt);
        if (agentSel && agentSel.value) p.set('agent_id', agentSel.value);
        return p;
      }

      function formatStatus(st) {
        const key = (st || '').toLowerCase();
        const map = {
          planned: 'badge bg-info-subtle text-info',
          ringing: 'badge bg-warning-subtle text-warning',
          answered: 'badge bg-success-subtle text-success',
          missed: 'badge bg-danger-subtle text-danger',
          completed: 'badge bg-primary-subtle text-primary',
          cancelled: 'badge bg-secondary-subtle text-secondary'
        };
        return `<span class="${map[key] || 'badge bg-light text-muted'} text-uppercase">${st || 'N/A'}</span>`;
      }

      function formatDirection(dir) {
        const key = (dir || '').toLowerCase();
        if (key === 'inbound') {
          return `<span class="badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-2">
            <i class="mdi mdi-phone-incoming-outline me-1" style="font-size:15px"></i>Inbound
          </span>`;
        }
        if (key === 'outbound') {
          return `<span class="badge rounded-pill bg-primary-subtle text-primary border border-primary-subtle px-3 py-2">
            <i class="mdi mdi-phone-outgoing-outline me-1" style="font-size:15px"></i>Outbound
          </span>`;
        }
        return dir || '-';
      }

      function renderTable(rows) {
        callTable.clear();
        (rows || []).forEach((r, idx) => {
          let rel = '-';
          // Prefer case when present, else contact, organization, lead.
          if (r.case_id) {
            const label = r.case_number ? `Case ${r.case_number}${r.case_subject ? ' — ' + r.case_subject : ''}` : ('Case #' + r.case_id);
            rel = `<a href="studio/cases/view.kml?id=${r.case_id}" class="text-decoration-underline">${label}</a>`;
          } else if (r.contact_id) {
            const label = ((r.contact_first || '') + ' ' + (r.contact_last || '')).trim() || r.phone || ('Contact #' + r.contact_id);
            rel = `<a href="studio/contacts/view.kml?id=${r.contact_id}" class="text-decoration-underline">${label}</a>`;
          } else if (r.organization_id) {
            const label = r.organization_name || ('organization #' + r.organization_id);
            const orgEnabled = !!(window.__mmkModulesEnabled && window.__mmkModulesEnabled.organizations);
            rel = orgEnabled
              ? `<a href="studio/organizations/view.kml?id=${r.organization_id_s || r.organization_id}" class="text-decoration-underline">${label}</a>`
              : `${label}`;
          } else if (r.lead_id) {
            const label = r.lead_title || ('Lead #' + r.lead_id);
            const leadsEnabled = !!(window.__mmkModulesEnabled && window.__mmkModulesEnabled.leads);
            rel = leadsEnabled
              ? `<a href="studio/leads/view.kml?id=${r.lead_id}" class="text-decoration-underline">${label}</a>`
              : `${label}`;
          }
          callTable.row.add([
            idx + 1,
            r.subject || '',
            formatDirection(r.direction),
            formatStatus(r.status),
            r.phone || '',
            rel,
            r.agent_name || '',
            r.start_at || '',
            `<div class="btn-group btn-group-sm">
              <button class="btn btn-light btn-view-call" data-call-id="${r.id}"><i class="ri-eye-line me-1"></i>View</button>
              ${canDeleteCall ? `<button class="btn btn-soft-danger btn-delete-call" data-call-id="${r.id}"><i class="ri-delete-bin-line me-1"></i>Delete</button>` : ''}
            </div>`
          ]);
        });
        callTable.draw();
      }

      function loadCalls() {
        setAlert('');
        fetch(apiCalls + buildQuery())
          .then(r => r.json().then(data => ({ ok: r.ok, data })))
          .then(({ ok, data }) => {
            if (!ok) throw new Error(data?.error || 'Unable to load calls');
            renderTable(data.data || []);
          })
          .catch(err => setAlert(err.message || 'Unable to load calls'));
      }

      async function loadPhoneCodes() {
        if (phoneHelper) {
          await phoneHelper.ready;
        }
      }

      function setPhoneFromValue(value) {
        if (phoneHelper) {
          phoneHelper.setFull(value || '');
          return;
        }
        if (phoneLocalInput) {
          phoneLocalInput.value = String(value || '').replace(/\D+/g, '');
        }
        if (phoneFullInput) {
          phoneFullInput.value = value || '';
        }
      }

      function syncIdentityMode() {
        if (!identityWrap) return;
        const mode = identityModeInput ? String(identityModeInput.value || 'personal').toLowerCase() : 'personal';
        const personalCol = personalBlockRoot ? personalBlockRoot.closest('.col-md-6') : null;
        const corporateCol = corporateBlockRoot ? corporateBlockRoot.closest('.col-md-6') : null;
        if (personalCol) {
          personalCol.classList.toggle('d-none', !identityVerificationContactsEnabled);
        }
        if (corporateCol) {
          const showCorporate = mode === 'corporate' && identityVerificationOrganizationsEnabled;
          corporateCol.classList.toggle('d-none', !showCorporate);
          if (!showCorporate) {
            if (corporateTokenInput) corporateTokenInput.value = '';
            if (corporateVerificationBlock) corporateVerificationBlock.reset();
          }
        }
      }

      function applyPersonalPrefill(prefill) {
        if (!prefill) return;
        const fullName = [prefill.first_name || '', prefill.last_name || ''].join(' ').trim();
        const currentPhone = phoneHelper ? phoneHelper.getFull() : (phoneFullInput ? phoneFullInput.value : '');
        if ((prefill.phone || '') && !currentPhone) {
          setPhoneFromValue(prefill.phone || '');
        }
        const subjectInput = document.getElementById('call_subject');
        if (subjectInput && !subjectInput.value.trim()) {
          subjectInput.value = fullName ? ('Call with ' + fullName) : 'Call';
        }
        if (linkTypeSel) linkTypeSel.value = 'contact';
        if (linkSearchWrap) linkSearchWrap.classList.remove('d-none');
        if (linkSearch) linkSearch.value = fullName || prefill.phone || prefill.email || 'Verified Contact';
        if (linkSelected) linkSelected.textContent = 'Verified contact will be created on save';
      }

      function applyCorporatePrefill(prefill) {
        if (!prefill) return;
        const companyName = prefill.name || 'Verified Organization';
        const currentPhone = phoneHelper ? phoneHelper.getFull() : (phoneFullInput ? phoneFullInput.value : '');
        if ((prefill.phone || '') && !currentPhone) {
          setPhoneFromValue(prefill.phone || '');
        }
        const subjectInput = document.getElementById('call_subject');
        if (subjectInput && !subjectInput.value.trim()) {
          subjectInput.value = 'Call with ' + companyName;
        }
        if (linkTypeSel) linkTypeSel.value = 'organization';
        if (linkSearchWrap) linkSearchWrap.classList.remove('d-none');
        if (linkSearch) linkSearch.value = companyName;
        if (linkSelected) linkSelected.textContent = 'Verified organization will be created on save';
      }

      function resetForm() {
        if (!form) return;
        form.reset();
        editingId = null;
        document.getElementById('call_id').value = '';
        setFormAlert('');
        document.getElementById('callModalTitle').textContent = 'New Call';
        // defaults for placeholders
        if (document.getElementById('call_direction')) document.getElementById('call_direction').value = 'inbound';
        if (document.getElementById('call_status')) document.getElementById('call_status').value = 'planned';
        if (document.getElementById('call_agent')) {
          document.getElementById('call_agent').value = '';
          if (window.jQuery) jQuery('#call_agent').trigger('change.select2');
        }
        if (phoneHelper) {
          phoneHelper.setFull('');
        } else if (document.getElementById('call_phone_code') && !document.getElementById('call_phone_code').value) {
          document.getElementById('call_phone_code').value = '+234';
        }
        if (contactIdInput) contactIdInput.value = '';
        if (organizationIdInput) organizationIdInput.value = '';
        if (caseIdInput) caseIdInput.value = '';
        if (leadIdInput) leadIdInput.value = '';
        if (identityModeInput) identityModeInput.value = 'personal';
        if (personalTokenInput) personalTokenInput.value = '';
        if (corporateTokenInput) corporateTokenInput.value = '';
        if (personalVerificationBlock) personalVerificationBlock.reset();
        if (corporateVerificationBlock) corporateVerificationBlock.reset();
        if (linkSelected) linkSelected.textContent = '';
        if (linkResults) { linkResults.classList.add('d-none'); linkResults.innerHTML=''; }
        if (linkTypeSel) { linkTypeSel.value = ''; if (linkSearchWrap) linkSearchWrap.classList.add('d-none'); }
        if (linkSearch) linkSearch.value = '';
        syncIdentityMode();
      }

      function openEdit(id) {
        if (!id) return;
        loadPhoneCodes();
        setFormAlert('');
        if (identityWrap) identityWrap.classList.add('d-none');
        fetch(apiCallDetail + '?id=' + encodeURIComponent(id))
          .then(r => r.json().then(data => ({ ok: r.ok, data })))
          .then(({ ok, data }) => {
            if (!ok) throw new Error(data?.error || 'Unable to load call');
            editingId = data.id;
            document.getElementById('call_id').value = data.id;
            document.getElementById('call_subject').value = data.subject || '';
            document.getElementById('call_direction').value = data.direction || 'inbound';
            document.getElementById('call_status').value = data.status || 'planned';
            if (phoneHelper) {
              phoneHelper.setFull(data.phone || '');
            } else if (phoneLocalInput) {
              phoneLocalInput.value = (data.phone || '').replace(/\D+/g, '');
            }
            if (document.getElementById('call_agent')) {
              document.getElementById('call_agent').value = data.agent_id || '';
              if (window.jQuery) jQuery('#call_agent').trigger('change.select2');
            }
            document.getElementById('call_start_at').value = data.start_at ? data.start_at.replace(' ', 'T') : '';
            document.getElementById('call_end_at').value = data.end_at ? data.end_at.replace(' ', 'T') : '';
            document.getElementById('call_notes').value = data.notes || '';
            if (contactIdInput) contactIdInput.value = data.contact_id || '';
            if (organizationIdInput) organizationIdInput.value = data.organization_id || '';
            if (caseIdInput) caseIdInput.value = data.case_id || '';
            if (leadIdInput) leadIdInput.value = data.lead_id || '';
            // Set link type/label
            let linkLabel = '';
            if (data.contact_id) { if (linkTypeSel) linkTypeSel.value = 'contact'; linkLabel = ((data.contact_first || '') + ' ' + (data.contact_last || '')).trim(); }
            else if (data.organization_id) { if (linkTypeSel) linkTypeSel.value = 'organization'; linkLabel = data.organization_name || ''; }
            else if (data.case_id) { if (linkTypeSel) linkTypeSel.value = 'case'; linkLabel = data.case_number ? `Case ${data.case_number}${data.case_subject ? ' — ' + data.case_subject : ''}` : ''; }
            else if (data.lead_id) { if (linkTypeSel) linkTypeSel.value = 'lead'; linkLabel = data.lead_id ? `Lead #${data.lead_id}` : ''; }
            if (linkLabel) {
              if (linkSelected) linkSelected.textContent = linkLabel;
              if (linkSearch) linkSearch.value = linkLabel;
            }
            if (linkTypeSel && linkTypeSel.value) {
              if (linkSearchWrap) linkSearchWrap.classList.remove('d-none');
            }
            document.getElementById('callModalTitle').textContent = 'Edit Call';
            if (modal) modal.show();
          })
          .catch(err => setAlert(err.message || 'Unable to load call'));
      }

      function saveCall(e) {
        e.preventDefault();
        setFormAlert('');
        const payload = {
          subject: document.getElementById('call_subject').value.trim(),
          direction: document.getElementById('call_direction').value,
          status: document.getElementById('call_status').value,
          phone: phoneHelper ? phoneHelper.getFull() : (phoneFullInput ? phoneFullInput.value : (phoneLocalInput ? phoneLocalInput.value : '')),
          agent_id: document.getElementById('call_agent') ? document.getElementById('call_agent').value : '',
          contact_id: contactIdInput ? contactIdInput.value : '',
          organization_id: organizationIdInput ? organizationIdInput.value : '',
          case_id: caseIdInput ? caseIdInput.value : '',
          lead_id: leadIdInput ? leadIdInput.value : '',
          start_at: document.getElementById('call_start_at').value.replace('T', ' ').trim(),
          end_at: document.getElementById('call_end_at').value.replace('T', ' ').trim(),
          notes: document.getElementById('call_notes').value.trim()
        };
        if (personalTokenInput && personalTokenInput.value) {
          payload.identity_personal_token = personalTokenInput.value;
        }
        if (identityModeInput && identityModeInput.value === 'corporate' && corporateTokenInput && corporateTokenInput.value) {
          payload.identity_corporate_token = corporateTokenInput.value;
        }
        if (!payload.subject) {
          setFormAlert('Subject is required.');
          return;
        }
        const phoneDigits = (phoneLocalInput ? phoneLocalInput.value : '').replace(/\\D+/g, '');
        const minLen = phoneLocalInput ? Number(phoneLocalInput.dataset.minLength || phoneLocalInput.getAttribute('minlength') || 0) : 0;
        const maxLen = phoneLocalInput ? Number(phoneLocalInput.dataset.maxLength || phoneLocalInput.getAttribute('maxlength') || 0) : 0;
        if (!phoneDigits || (minLen && phoneDigits.length < minLen) || (maxLen && phoneDigits.length > maxLen)) {
          const hint = (minLen && maxLen && minLen === maxLen) ? `${minLen} digits` : `${minLen || 1}-${maxLen || 12} digits`;
          setFormAlert(`Enter a valid phone number (${hint}).`);
          return;
        }
        saveBtn.disabled = true;
        const url = editingId ? (apiCallDetail + '?id=' + encodeURIComponent(editingId)) : apiCalls;
        const method = editingId ? 'PATCH' : 'POST';
        fetch(url, {
          method,
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        })
          .then(r => r.json().then(data => ({ ok: r.ok, data })))
          .then(({ ok, data }) => {
            if (!ok) throw new Error(data?.error || 'Unable to save call');
            if (modal) modal.hide();
            resetForm();
            loadCalls();
          })
          .catch(err => setFormAlert(err.message || 'Unable to save call'))
          .finally(() => { saveBtn.disabled = false; });
      }

      loadPhoneCodes();
      initSelect2();

      if (btnSearch) btnSearch.addEventListener('click', loadCalls);
      if (btnReset) btnReset.addEventListener('click', () => {
        if (searchInput) searchInput.value = '';
        if (dirSel) dirSel.value = '';
        if (statusSel) statusSel.value = '';
        if (dateRangeSel) dateRangeSel.value = '';
        if (agentSel) {
          agentSel.value = '';
          if (window.jQuery) jQuery('#callAgent').trigger('change.select2');
        }
        loadCalls();
      });
      if (exportBtn) {
        exportBtn.addEventListener('click', () => {
          const params = buildExportParams();
          const url = apiExport + (params.toString() ? ('?' + params.toString()) : '');
          window.location.href = url;
        });
      }
      if (btnNew) btnNew.addEventListener('click', () => { resetForm(); modal && modal.show(); });
      if (form) form.addEventListener('submit', saveCall);
      if (identityWrap) identityWrap.classList.toggle('d-none', !identityVerificationEnabled);
      if (identityModeInput) identityModeInput.addEventListener('change', syncIdentityMode);
      if (window.IdentityVerification && identityVerificationEnabled) {
        if (personalBlockRoot && identityVerificationContactsEnabled) {
          personalVerificationBlock = window.IdentityVerification.bindBlock({
            root: personalBlockRoot,
            mode: 'agent',
            endpoint: identityVerificationApi,
            sectionType: 'personal',
            entityMode: 'corporate',
            availableMethods: Array.isArray((window.appCallConfig || {}).identityVerificationPersonalMethods)
              ? window.appCallConfig.identityVerificationPersonalMethods
              : null,
            onPrefill: function(prefill) {
              applyPersonalPrefill(prefill || {});
              if (personalTokenInput) {
                personalTokenInput.value = personalVerificationBlock ? personalVerificationBlock.getToken() : '';
              }
            },
            onReset: function() {
              if (personalTokenInput) personalTokenInput.value = '';
            }
          });
        }
        if (corporateBlockRoot && identityVerificationOrganizationsEnabled) {
          corporateVerificationBlock = window.IdentityVerification.bindBlock({
            root: corporateBlockRoot,
            mode: 'agent',
            endpoint: identityVerificationApi,
            sectionType: 'corporate',
            entityMode: 'corporate',
            availableMethods: Array.isArray((window.appCallConfig || {}).identityVerificationCorporateMethods)
              ? window.appCallConfig.identityVerificationCorporateMethods
              : null,
            onPrefill: function(prefill) {
              applyCorporatePrefill(prefill || {});
              if (corporateTokenInput) {
                corporateTokenInput.value = corporateVerificationBlock ? corporateVerificationBlock.getToken() : '';
              }
            },
            onReset: function() {
              if (corporateTokenInput) corporateTokenInput.value = '';
            }
          });
        }
      }
      syncIdentityMode();

      function applyPrefillFromUrl() {
        const params = new URLSearchParams(window.location.search);
        if (!params.get('call')) return;
        const phone = params.get('phone') || '';
        const name = params.get('name') || '';
        const entityType = params.get('entity_type') || 'contact';
        const entityId = params.get('entity_id') || '';
        const entityLabel = params.get('entity_label') || name;
        if (!phone && !entityId) return;

        resetForm();
        if (phoneHelper) {
          phoneHelper.setFull(phone);
        } else if (phoneLocalInput) {
          phoneLocalInput.value = (phone || '').replace(/\D+/g, '');
          if (phoneFullInput) phoneFullInput.value = phone || '';
        }
        const subj = document.getElementById('call_subject');
        if (subj && !subj.value) {
          subj.value = name ? `Call with ${name}` : 'Outbound call';
        }
        const dir = document.getElementById('call_direction');
        if (dir && !dir.value) dir.value = 'outbound';
        if (linkTypeSel) linkTypeSel.value = entityType || 'contact';
        if (linkSearchWrap) linkSearchWrap.classList.remove('d-none');
        clearLinks();
        if (entityType === 'contact' && contactIdInput) contactIdInput.value = entityId || '';
        if (entityType === 'organization' && organizationIdInput) organizationIdInput.value = entityId || '';
        if (entityType === 'case' && caseIdInput) caseIdInput.value = entityId || '';
        if (entityType === 'lead' && leadIdInput) leadIdInput.value = entityId || '';
        if (linkSelected) linkSelected.textContent = entityLabel || '';
        if (linkSearch) linkSearch.value = entityLabel || '';
        if (modal) modal.show();
      }

      $('#callTable').on('click', '.btn-view-call', function() {
        const id = this.getAttribute('data-call-id');
        openEdit(id);
      });

      $('#callTable').on('click', '.btn-delete-call', function() {
        const id = this.getAttribute('data-call-id');
        if (!id) return;
        pendingDeleteId = id;
        confirmDelModal && confirmDelModal.show();
      });

      if (confirmDelBtn) {
        confirmDelBtn.addEventListener('click', () => {
          if (!pendingDeleteId) return;
          const spinner = confirmDelBtn.querySelector('.spinner-border');
          const text = confirmDelBtn.querySelector('.btn-text');
          confirmDelBtn.disabled = true;
          if (spinner) spinner.classList.remove('d-none');
          if (text) text.textContent = '';
          fetch(apiCallDetail + '?id=' + encodeURIComponent(pendingDeleteId), { method: 'DELETE' })
            .then(r => r.json().then(data => ({ ok: r.ok, data })))
            .then(({ ok, data }) => {
              if (!ok) throw new Error(data?.error || 'Unable to delete call');
              loadCalls();
              confirmDelModal && confirmDelModal.hide();
            })
            .catch(err => setAlert(err.message || 'Unable to delete call'))
            .finally(() => {
              confirmDelBtn.disabled = false;
              if (spinner) spinner.classList.add('d-none');
              if (text) text.textContent = 'Delete';
              pendingDeleteId = null;
            });
        });
      }

      function clearLinks() {
        if (contactIdInput) contactIdInput.value = '';
        if (organizationIdInput) organizationIdInput.value = '';
        if (caseIdInput) caseIdInput.value = '';
        if (leadIdInput) leadIdInput.value = '';
        if (linkSelected) linkSelected.textContent = '';
        if (linkResults) { linkResults.classList.add('d-none'); linkResults.innerHTML = ''; }
      }

      function bindUnifiedSearch() {
        if (!linkSearch || !linkResults || !linkTypeSel) return;
        let timer = null;
        linkSearch.addEventListener('input', () => {
          const term = linkSearch.value.trim();
          if (timer) clearTimeout(timer);
          if (!linkTypeSel.value || term.length < 4) {
            linkResults.classList.add('d-none');
            if (linkStatus) {
              linkStatus.textContent = 'Enter 4 or more characters to search.';
              linkStatus.classList.remove('text-danger');
              linkStatus.classList.add('text-muted');
            }
            return;
          }
          if (linkStatus) {
            linkStatus.textContent = 'Searching...';
            linkStatus.classList.remove('text-danger');
            linkStatus.classList.add('text-muted');
          }
          const { endpoint, labelFn } = (() => {
            switch (linkTypeSel.value) {
              case 'contact': return { endpoint: apiContacts, labelFn: (i) => ((i.first_name || '') + ' ' + (i.last_name || '')).trim() || i.phone || i.email || ('Contact #' + i.id) };
              case 'organization': return { endpoint: apiorganizations, labelFn: (i) => i.name || ('organization #' + i.id) };
              case 'case': return { endpoint: apiCases, labelFn: (i) => (i.case_number ? 'Case ' + i.case_number : 'Case #' + i.id) + (i.subject ? ' — ' + i.subject : '') };
              case 'lead': return { endpoint: apiLeads, labelFn: (i) => i.title || ('Lead #' + i.id) };
              default: return { endpoint: null, labelFn: () => '' };
            }
          })();
          if (!endpoint) return;
          timer = setTimeout(() => {
            const qs = new URLSearchParams({ q: term, limit: '5' });
            fetch(endpoint + '?' + qs.toString())
              .then(r => r.json())
              .then(data => {
                const list = data.data || [];
                linkResults.innerHTML = '';
                if (!list.length) {
                  linkResults.classList.add('d-none');
                  if (linkStatus) {
                    linkStatus.textContent = 'No result found.';
                    linkStatus.classList.add('text-danger');
                    linkStatus.classList.remove('text-muted');
                  }
                  return;
                }
                list.slice(0,5).forEach(item => {
                  const btn = document.createElement('button');
                  btn.type = 'button';
                  btn.className = 'list-group-item list-group-item-action';
                  const label = labelFn(item);
                  btn.textContent = label;
                  btn.addEventListener('click', () => {
                    clearLinks();
                    if (linkTypeSel.value === 'contact' && contactIdInput) contactIdInput.value = item.id || '';
                    if (linkTypeSel.value === 'organization' && organizationIdInput) organizationIdInput.value = item.id || '';
                    if (linkTypeSel.value === 'case' && caseIdInput) caseIdInput.value = item.id || '';
                    if (linkTypeSel.value === 'lead' && leadIdInput) leadIdInput.value = item.id || '';
                    if (linkSelected) linkSelected.textContent = label;
                    if (linkStatus) {
                      linkStatus.textContent = '';
                      linkStatus.classList.remove('text-danger');
                      linkStatus.classList.add('text-muted');
                    }
                    linkResults.classList.add('d-none');
                  });
                  linkResults.appendChild(btn);
                });
                linkResults.classList.remove('d-none');
              })
              .catch(() => {
                linkResults.classList.add('d-none');
                if (linkStatus) {
                  linkStatus.textContent = 'No result found.';
                  linkStatus.classList.add('text-danger');
                  linkStatus.classList.remove('text-muted');
                }
              });
          }, 300);
        });
      }

      if (linkTypeSel) {
        linkTypeSel.addEventListener('change', () => {
          clearLinks();
          if (linkSearch) linkSearch.value = '';
          if (linkTypeSel.value) {
            if (linkSearchWrap) linkSearchWrap.classList.remove('d-none');
            if (linkStatus) {
              linkStatus.textContent = 'Enter 4 or more characters to search.';
              linkStatus.classList.remove('text-danger');
              linkStatus.classList.add('text-muted');
            }
          } else {
            if (linkSearchWrap) linkSearchWrap.classList.add('d-none');
          }
        });
      }

      bindUnifiedSearch();

      loadCalls();
      applyPrefillFromUrl();
    })();
