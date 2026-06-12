/* Shared phone helper: keep dial code separate, enforce per-country local length. */
(function () {
  'use strict';

  function normalizeDial(value) {
    if (!value) return '';
    const raw = String(value).trim();
    if (!raw) return '';
    if (raw.startsWith('+')) return raw.replace(/\s+/g, '');
    return '+' + raw.replace(/\s+/g, '');
  }

  function digitsOnly(value) {
    return String(value || '').replace(/\D+/g, '');
  }

  function pickLocalLength(country) {
    const len = country && country.local_length;
    const min = Number(len && len.min);
    const max = Number(len && len.max);
    return {
      min: Number.isFinite(min) && min > 0 ? min : 6,
      max: Number.isFinite(max) && max > 0 ? max : 12
    };
  }

  function getCountriesUrl(explicitUrl) {
    if (explicitUrl) return explicitUrl;
    const base = (typeof url_root !== 'undefined' ? url_root : '../');
    return base + 'api/modules/geo/index?action=countries';
  }

  function normalizeCountry(raw) {
    if (!raw || typeof raw !== 'object') return null;
    const id = Number(raw.id || 0);
    const code = String(raw.code || raw.sortname || '').trim().toUpperCase();
    const name = String(raw.name || '').trim();
    const phone = String(raw.dial_code || raw.phonecode || '').trim();
    const dial = normalizeDial(phone);
    if (!code || !name || !dial) return null;
    const local = raw.local_length || {};
    const min = Number(local.min || raw.local_length_min || 0);
    const max = Number(local.max || raw.local_length_max || 0);
    const minLen = Number.isFinite(min) && min > 0 ? min : 6;
    const maxLen = Number.isFinite(max) && max > 0 ? max : 12;
    return {
      id: Number.isFinite(id) && id > 0 ? id : 0,
      code: code,
      name: name,
      dial_code: dial,
      local_length: { min: minLen, max: Math.max(maxLen, minLen) }
    };
  }

  function loadCountries(url) {
    return fetch(url, { cache: 'no-store' })
      .then(r => r.ok ? r.json() : [])
      .then(json => {
        const rows = Array.isArray(json) ? json : (Array.isArray(json && json.data) ? json.data : []);
        return rows.map(normalizeCountry).filter(Boolean);
      })
      .catch(() => []);
  }

  function buildDialMap(list) {
    const map = new Map();
    (list || []).forEach(c => {
      if (!c || !c.dial_code) return;
      const dial = normalizeDial(c.dial_code);
      if (!dial) return;
      map.set(dial, c);
    });
    return map;
  }

  function findDialMatch(list, fullNumber) {
    const val = String(fullNumber || '').trim();
    if (!val) return null;
    const sorted = (list || []).slice().sort((a, b) => {
      const da = normalizeDial(a.dial_code || '');
      const db = normalizeDial(b.dial_code || '');
      return db.length - da.length;
    });
    for (const c of sorted) {
      const dial = normalizeDial(c.dial_code || '');
      if (dial && val.startsWith(dial)) return { country: c, dial };
    }
    return null;
  }

  function PhoneHelperBind(opts) {
    const config = opts || {};
    const codeSelect = config.codeSelect || null;
    const countrySelect = config.countrySelect || null;
    const localInput = config.localInput || null;
    const fullInput = config.fullInput || null;
    const countriesUrl = getCountriesUrl(config.countriesUrl);
    const defaultDial = normalizeDial(config.defaultDial || '+234');
    const defaultCountry = (config.defaultCountry || '').toUpperCase();
    let countries = [];
    let dialMap = new Map();

    function setLocalConstraints(country) {
      if (!localInput) return;
      const len = pickLocalLength(country);
      localInput.setAttribute('maxlength', String(len.max));
      localInput.setAttribute('minlength', String(len.min));
      localInput.dataset.minLength = String(len.min);
      localInput.dataset.maxLength = String(len.max);
    }

    function updateCountryByDial(dial) {
      if (!countrySelect) return;
      if (!dial) return;
      const country = dialMap.get(dial);
      if (!country) return;
      const code = String(country.code || '').toUpperCase();
      if (!code) return;
      if (countrySelect.value !== code) countrySelect.value = code;
      setLocalConstraints(country);
    }

    function updateDialByCountry(code) {
      if (!code || !codeSelect) return;
      const match = (countries || []).find(c => String(c.code || '').toUpperCase() === code.toUpperCase());
      if (!match) return;
      const dial = normalizeDial(match.dial_code || '');
      if (!dial) return;
      if (codeSelect.value !== dial) codeSelect.value = dial;
      setLocalConstraints(match);
    }

    function syncFullValue() {
      if (!fullInput) return;
      const dial = codeSelect ? normalizeDial(codeSelect.value) : '';
      const local = digitsOnly(localInput ? localInput.value : '');
      if (localInput) localInput.value = local;
      if (!local) {
        fullInput.value = '';
        return;
      }
      fullInput.value = dial ? (dial + local) : local;
    }

    function handleLocalInput() {
      if (!localInput) return;
      const max = Number(localInput.dataset.maxLength || localInput.getAttribute('maxlength') || 0);
      let cleaned = digitsOnly(localInput.value);
      if (max > 0 && cleaned.length > max) cleaned = cleaned.slice(0, max);
      localInput.value = cleaned;
      syncFullValue();
    }

    function initSelects() {
      if (codeSelect) {
        const codes = (countries || []).filter(c => c && c.dial_code);
        codeSelect.innerHTML = '';
        if (!codes.length) {
          // Fallback: keep at least the default dial so the select isn't empty
          const opt = document.createElement('option');
          opt.value = defaultDial || '+234';
          opt.textContent = defaultDial || '+234';
          codeSelect.appendChild(opt);
          codeSelect.value = defaultDial || '+234';
        } else {
          codes.sort((a, b) => normalizeDial(a.dial_code).length - normalizeDial(b.dial_code).length);
          codes.forEach(c => {
            const dial = normalizeDial(c.dial_code);
            if (!dial) return;
            const opt = document.createElement('option');
            opt.value = dial;
            opt.textContent = dial;
            codeSelect.appendChild(opt);
          });
          if (defaultDial && codeSelect.querySelector('option[value="' + defaultDial + '"]')) {
            codeSelect.value = defaultDial;
          } else if (codeSelect.options.length) {
            codeSelect.selectedIndex = 0;
          }
        }
      }

      if (countrySelect) {
        countrySelect.innerHTML = '<option value="">Select country</option>';
        (countries || []).forEach(c => {
          if (!c || !c.code || !c.name) return;
          const opt = document.createElement('option');
          opt.value = String(c.code).toUpperCase();
          opt.textContent = c.name;
          if (c.id) opt.dataset.id = String(c.id);
          opt.dataset.code = String(c.code).toUpperCase();
          countrySelect.appendChild(opt);
        });
          if (defaultCountry && countrySelect.querySelector('option[value="' + defaultCountry + '"]')) {
            countrySelect.value = defaultCountry;
          }
          // If a default country was provided prefer setting the dial by country
          // to avoid cases where defaultDial isn't matched due to list variations.
          if (defaultCountry) {
            updateDialByCountry(defaultCountry);
          }
      }

      if (codeSelect && codeSelect.value) updateCountryByDial(codeSelect.value);
      else if (countrySelect && countrySelect.value) updateDialByCountry(countrySelect.value);
      // Ensure any change handlers respond to the initial selection
      try {
        if (codeSelect) codeSelect.dispatchEvent(new Event('change', { bubbles: true }));
        if (countrySelect) countrySelect.dispatchEvent(new Event('change', { bubbles: true }));
      } catch (err) {
        // ignore
      }
    }

    const ready = loadCountries(countriesUrl).then(list => {
      countries = Array.isArray(list) ? list : [];
      dialMap = buildDialMap(countries);
      initSelects();
      handleLocalInput();
    });

    if (codeSelect) {
      codeSelect.addEventListener('change', () => {
        updateCountryByDial(codeSelect.value);
        handleLocalInput();
      });
    }
    if (countrySelect) {
      countrySelect.addEventListener('change', () => {
        updateDialByCountry(countrySelect.value);
        handleLocalInput();
      });
    }
    if (localInput) {
      localInput.addEventListener('input', handleLocalInput);
    }

    function setFull(fullNumber) {
      const val = String(fullNumber || '').trim();
      if (!val) {
        if (localInput) localInput.value = '';
        if (fullInput) fullInput.value = '';
        if (codeSelect && defaultDial) {
          if (!codeSelect.querySelector('option[value="' + defaultDial + '"]')) {
            const opt = document.createElement('option');
            opt.value = defaultDial;
            opt.textContent = defaultDial;
            codeSelect.appendChild(opt);
          }
          codeSelect.value = defaultDial;
          updateCountryByDial(defaultDial);
        } else if (countrySelect && defaultCountry) {
          countrySelect.value = defaultCountry;
          updateDialByCountry(defaultCountry);
        }
        return;
      }
      const match = findDialMatch(countries, val);
      if (match && codeSelect) {
        if (!codeSelect.querySelector('option[value="' + match.dial + '"]')) {
          const opt = document.createElement('option');
          opt.value = match.dial;
          opt.textContent = match.dial;
          codeSelect.appendChild(opt);
        }
        codeSelect.value = match.dial;
        updateCountryByDial(match.dial);
        if (localInput) localInput.value = digitsOnly(val.slice(match.dial.length));
      } else if (localInput) {
        localInput.value = digitsOnly(val);
      }
      handleLocalInput();
    }

    function getFull() {
      if (fullInput && fullInput.value) return fullInput.value;
      const dial = codeSelect ? normalizeDial(codeSelect.value) : '';
      const local = digitsOnly(localInput ? localInput.value : '');
      return local ? (dial ? dial + local : local) : '';
    }

    return {
      ready,
      setFull,
      getFull
    };
  }

  window.PhoneHelper = {
    bind: PhoneHelperBind
  };
})();
