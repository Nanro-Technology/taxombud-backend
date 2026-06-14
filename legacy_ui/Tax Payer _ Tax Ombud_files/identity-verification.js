/* eslint-disable */
(function () {
  function text(el, value) {
    if (el) el.textContent = value || '';
  }

  function setStatus(el, value, tone) {
    if (!el) return;
    el.textContent = value || '';
    el.classList.remove('text-danger', 'text-success', 'text-muted', 'text-warning');
    if (!value) return;
    if (tone === 'danger') {
      el.classList.add('text-danger');
      return;
    }
    if (tone === 'success') {
      el.classList.add('text-success');
      return;
    }
    if (tone === 'warning') {
      el.classList.add('text-warning');
      return;
    }
    el.classList.add('text-muted');
  }

  function setBusy(btn, busy, label) {
    if (!btn) return;
    if (window.toggleButtonLoading) {
      window.toggleButtonLoading(btn, !!busy, label || 'Working...');
      return;
    }
    btn.disabled = !!busy;
  }

  function buildDispatchStatus(baseMessage, data) {
    return baseMessage;
  }

  function postJson(url, payload) {
    return fetch(url, {
      method: 'POST',
      credentials: 'same-origin',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload || {})
    }).then(function (res) {
      return res.json().catch(function () { return {}; }).then(function (data) {
        if (!res.ok) {
          throw new Error(data.error || data.message || 'Request failed');
        }
        return data;
      });
    });
  }

  function hasPrefillData(prefill) {
    if (!prefill || typeof prefill !== 'object') return false;
    return Object.keys(prefill).some(function (key) {
      const value = prefill[key];
      return value !== null && value !== undefined && String(value).trim() !== '';
    });
  }

  function bindBlock(opts) {
    const root = opts && opts.root ? opts.root : null;
    if (!root) return null;
    const mode = (opts.mode || 'public') === 'agent' ? 'agent' : 'public';
    const endpoint = opts.endpoint || '';
    const availableMethods = Array.isArray(opts.availableMethods)
      ? opts.availableMethods.map(function (item) { return String(item || '').toLowerCase(); }).filter(Boolean)
      : null;
    const entityModeGetter = typeof opts.entityMode === 'function'
      ? opts.entityMode
      : function () { return String(opts.entityMode || 'personal'); };
    const sectionType = String(opts.sectionType || root.dataset.sectionType || 'personal');
    const methodSelect = root.querySelector('[data-role="method"]');
    const identifierInput = root.querySelector('[data-role="identifier"]');
    const fetchBtn = root.querySelector('[data-role="fetch"]');
    const fetchWrap = root.querySelector('[data-role="fetch-wrap"]') || (fetchBtn ? fetchBtn.closest('[data-role="fetch-wrap"]') : null);
    const statusEl = root.querySelector('[data-role="status"]');
    const pinWrap = root.querySelector('[data-role="pin-wrap"]');
    const pinInput = root.querySelector('[data-role="pin"]');
    const verifyBtn = root.querySelector('[data-role="verify"]');
    const tokenInput = root.querySelector('[data-role="token"]');
    const badgeEl = root.querySelector('[data-role="verified-badge"]');
    const channelEl = root.querySelector('[data-role="channel"]');
    const resendBtn = root.querySelector('[data-role="resend"]');
    const retryCountdownEl = root.querySelector('[data-role="retry-countdown"]');
    const state = {
      token: '',
      verified: false,
      prefill: null,
      lastIdentifierType: '',
      lastIdentifierValue: '',
      retryTimer: null,
      retryRemaining: 0
    };

    function syncMethodAvailability() {
      if (!methodSelect || !availableMethods) return;
      const options = Array.from(methodSelect.options || []);
      let firstEnabled = '';
      options.forEach(function (opt) {
        if (!opt || !opt.value) return;
        const enabled = availableMethods.indexOf(String(opt.value).toLowerCase()) >= 0;
        opt.disabled = !enabled;
        if (enabled && !firstEnabled) {
          firstEnabled = opt.value;
        }
      });
      if (methodSelect.value && availableMethods.indexOf(String(methodSelect.value).toLowerCase()) < 0) {
        methodSelect.value = firstEnabled || '';
      }
      if (!methodSelect.value && firstEnabled) {
        methodSelect.value = firstEnabled;
      }
      const hasEnabled = !!firstEnabled;
      if (fetchBtn) fetchBtn.disabled = !hasEnabled;
      if (methodSelect) methodSelect.disabled = !hasEnabled;
      if (!hasEnabled) {
        setStatus(statusEl, 'No verification methods are currently enabled for this section.', 'warning');
      }
    }

    function clearRetryTimer() {
      if (state.retryTimer) {
        window.clearInterval(state.retryTimer);
        state.retryTimer = null;
      }
      state.retryRemaining = 0;
    }

    function updateRetryUi() {
      if (!resendBtn) return;
      if (mode !== 'public' || !state.token || state.verified) {
        resendBtn.classList.add('d-none');
        resendBtn.disabled = true;
        text(retryCountdownEl, '');
        return;
      }
      resendBtn.classList.remove('d-none');
      resendBtn.disabled = state.retryRemaining > 0;
      text(retryCountdownEl, state.retryRemaining > 0 ? ('Resend available in ' + state.retryRemaining + 's') : 'You can resend the OTP now.');
    }

    function startRetryTimer(seconds) {
      clearRetryTimer();
      state.retryRemaining = Math.max(0, Number(seconds || 0));
      updateRetryUi();
      if (state.retryRemaining <= 0) {
        return;
      }
      state.retryTimer = window.setInterval(function () {
        state.retryRemaining -= 1;
        if (state.retryRemaining <= 0) {
          clearRetryTimer();
        }
        updateRetryUi();
      }, 1000);
    }

    function reset(opts2) {
      state.token = '';
      state.verified = false;
      state.prefill = null;
      state.lastIdentifierType = '';
      state.lastIdentifierValue = '';
      clearRetryTimer();
      if (tokenInput) tokenInput.value = '';
      if (pinInput) pinInput.value = '';
      if (!opts2 || !opts2.keepStatus) {
        setStatus(statusEl, '', '');
      }
      if (pinWrap) pinWrap.classList.add('d-none');
      if (fetchWrap) fetchWrap.classList.remove('d-none');
      if (badgeEl) badgeEl.classList.add('d-none');
      if (channelEl) text(channelEl, '');
      updateRetryUi();
      if (typeof opts.onReset === 'function') {
        opts.onReset();
      }
    }

    async function fetchPrefill(token) {
      const payload = mode === 'public'
        ? { action: 'prefill', token: token, section_type: sectionType }
        : { action: 'lookup', token: token, section_type: sectionType };
      const data = await postJson(endpoint, payload);
      return data.prefill || {};
    }

    async function start() {
      const identifierType = methodSelect ? String(methodSelect.value || '') : '';
      const identifierValue = identifierInput ? String(identifierInput.value || '').trim() : '';
      if (!identifierType || !identifierValue) {
        setStatus(statusEl, 'Select a verification method and enter an identifier.', 'danger');
        return;
      }
      reset({ keepStatus: true });
      setBusy(fetchBtn, true, mode === 'public' ? 'Fetching...' : 'Fetching...');
      try {
        if (mode === 'public') {
          const data = await postJson(endpoint, {
            action: 'start',
            entity_mode: entityModeGetter(),
            section_type: sectionType,
            identifier_type: identifierType,
            identifier_value: identifierValue
          });
          state.token = String(data.token || '');
          state.lastIdentifierType = identifierType;
          state.lastIdentifierValue = identifierValue;
          if (tokenInput) tokenInput.value = state.token;

          // OTP sent — whether this is a new or returning person, they must verify.
          if (pinWrap) pinWrap.classList.remove('d-none');
          if (fetchWrap) fetchWrap.classList.add('d-none');
          const pieces = [];
          if (data.masked_email) pieces.push('Email: ' + data.masked_email);
          if (data.masked_phone) pieces.push('Phone: ' + data.masked_phone);
          text(channelEl, pieces.join(' | '));
          setStatus(statusEl, buildDispatchStatus('Verification code sent. Enter the PIN to continue.', data), 'success');
          startRetryTimer(30);
          if (typeof opts.onDispatch === 'function') {
            opts.onDispatch(data);
          }
          return;
        }
        const data = await postJson(endpoint, {
          action: 'lookup',
          entity_mode: entityModeGetter(),
          section_type: sectionType,
          identifier_type: identifierType,
          identifier_value: identifierValue
        });
        if (!data || typeof data !== 'object' || !data.token) {
          throw new Error('Verification lookup did not return a valid session.');
        }
        if (!hasPrefillData(data.prefill || {})) {
          throw new Error('No verification record was returned for that identifier.');
        }
        state.token = String(data.token || '');
        state.verified = true;
        state.prefill = data.prefill || {};
        if (tokenInput) tokenInput.value = state.token;
        if (badgeEl) badgeEl.classList.remove('d-none');
        setStatus(statusEl, 'Verification data fetched successfully.', 'success');
        if (typeof opts.onPrefill === 'function') {
          opts.onPrefill(state.prefill, data.summary || {});
        }
      } catch (err) {
        setStatus(statusEl, err.message || 'Unable to fetch verification data.', 'danger');
      } finally {
        setBusy(fetchBtn, false);
      }
    }

    async function verify() {
      const token = tokenInput ? String(tokenInput.value || '') : state.token;
      const code = pinInput ? String(pinInput.value || '').trim() : '';
      if (!token || !code) {
        setStatus(statusEl, 'Enter the verification PIN.', 'danger');
        return;
      }
      setBusy(verifyBtn, true, 'Verifying...');
      try {
        await postJson(endpoint, {
          action: 'verify',
          token: token,
          code: code
        });
        const prefillData = await postJson(endpoint, {
          action: 'prefill',
          token: token,
          section_type: sectionType
        });
        state.token = token;
        state.verified = true;
        state.prefill = prefillData.prefill || {};
        clearRetryTimer();
        if (badgeEl) badgeEl.classList.remove('d-none');
        updateRetryUi();
        setStatus(statusEl, 'Verification successful.', 'success');
        if (typeof opts.onPrefill === 'function') {
          try {
            opts.onPrefill(state.prefill, prefillData.summary || {});
          } catch (prefillErr) {
            console.error('onPrefill error:', prefillErr);
          }
        }
      } catch (err) {
        setStatus(statusEl, err.message || 'Unable to verify PIN.', 'danger');
      } finally {
        setBusy(verifyBtn, false);
      }
    }

    async function resend() {
      if (mode !== 'public' || !state.lastIdentifierType || !state.lastIdentifierValue) {
        setStatus(statusEl, 'Start verification first before retrying the OTP.', 'danger');
        return;
      }
      if (state.retryRemaining > 0) {
        updateRetryUi();
        return;
      }
      setBusy(resendBtn, true, 'Resending...');
      try {
        const data = await postJson(endpoint, {
          action: 'start',
          entity_mode: entityModeGetter(),
          section_type: sectionType,
          identifier_type: state.lastIdentifierType,
          identifier_value: state.lastIdentifierValue
        });
        state.token = String(data.token || '');
        if (tokenInput) tokenInput.value = state.token;
        if (pinInput) pinInput.value = '';
        const pieces = [];
        if (data.masked_email) pieces.push('Email: ' + data.masked_email);
        if (data.masked_phone) pieces.push('Phone: ' + data.masked_phone);
        text(channelEl, pieces.join(' | '));
        setStatus(statusEl, buildDispatchStatus('A new verification code has been sent.', data), 'success');
        startRetryTimer(30);
        if (typeof opts.onDispatch === 'function') {
          opts.onDispatch(data);
        }
      } catch (err) {
        setStatus(statusEl, err.message || 'Unable to resend verification code.', 'danger');
      } finally {
        setBusy(resendBtn, false);
        updateRetryUi();
      }
    }

    if (fetchBtn) {
      fetchBtn.addEventListener('click', function (e) {
        e.preventDefault();
        start();
      });
    }
    if (verifyBtn) {
      verifyBtn.addEventListener('click', function (e) {
        e.preventDefault();
        verify();
      });
    }
    if (resendBtn) {
      resendBtn.addEventListener('click', function (e) {
        e.preventDefault();
        resend();
      });
    }

    updateRetryUi();
    syncMethodAvailability();

    return {
      reset: reset,
      getToken: function () { return state.token; },
      isVerified: function () { return !!state.verified; },
      getPrefill: function () { return state.prefill || {}; }
    };
  }

  window.IdentityVerification = {
    bindBlock: bindBlock
  };
})();
