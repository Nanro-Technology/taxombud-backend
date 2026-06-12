/* Shared geo helper: country -> state -> city dependent selects. */
(function () {
  function safeText(v) {
    return String(v == null ? "" : v).trim();
  }

  function createOption(value, label, selected) {
    const opt = document.createElement("option");
    opt.value = String(value == null ? "" : value);
    opt.textContent = safeText(label) || "-";
    if (selected) opt.selected = true;
    return opt;
  }

  function setSelectLoading(selectEl, loadingLabel) {
    if (!selectEl) return;
    selectEl.disabled = true;
    selectEl.innerHTML = "";
    selectEl.appendChild(createOption("", loadingLabel || "Loading...", true));
  }

  function setSelectEmpty(selectEl, label) {
    if (!selectEl) return;
    selectEl.disabled = false;
    selectEl.innerHTML = "";
    selectEl.appendChild(createOption("", label || "Select", true));
  }

  function selectByText(selectEl, valueText) {
    if (!selectEl) return false;
    const target = safeText(valueText).toLowerCase();
    if (!target) return false;
    const opts = Array.from(selectEl.options || []);
    const found = opts.find((o) => safeText(o.textContent).toLowerCase() === target);
    if (!found) return false;
    selectEl.value = found.value;
    return true;
  }

  function ensureLegacyOption(selectEl, valueText, labelPrefix) {
    if (!selectEl) return false;
    const txt = safeText(valueText);
    if (!txt) return false;
    const exists = Array.from(selectEl.options || []).some((o) => safeText(o.textContent).toLowerCase() === txt.toLowerCase());
    if (exists) return false;
    const legacyValue = "__legacy__:" + txt;
    const opt = createOption(legacyValue, (labelPrefix || "Current: ") + txt, true);
    selectEl.appendChild(opt);
    selectEl.value = legacyValue;
    return true;
  }

  function GeoBind(opts) {
    const cfg = Object.assign(
      {
        apiCountries: (typeof apiEndpoints === "function" ? apiEndpoints().geoCountries : ""),
        apiStates: (typeof apiEndpoints === "function" ? apiEndpoints().geoStates : ""),
        apiCities: (typeof apiEndpoints === "function" ? apiEndpoints().geoCities : ""),
        apiCreateCity: (typeof apiEndpoints === "function" ? apiEndpoints().geoCreateCity : ""),
        defaultCountryCode: "NG",
        countryPlaceholder: "Select country",
        statePlaceholder: "Select state",
        cityPlaceholder: "Select city",
        emptyStatesLabel: "No states found",
        emptyCitiesLabel: "No cities found",
        cityNotListedLabel: "Not listed? Add city",
        allowCreateCity: true,
        onCountryMeta: null,
      },
      opts || {}
    );

    const countrySelect = cfg.countrySelect || null;
    const stateSelect = cfg.stateSelect || null;
    const citySelect = cfg.citySelect || null;
    const countryNameInput = cfg.countryNameInput || null;
    const stateNameInput = cfg.stateNameInput || null;
    const cityNameInput = cfg.cityNameInput || null;

    let countries = [];
    let states = [];
    let cities = [];
    let countriesById = new Map();
    let cityInlineWrap = null;
    let cityInlineInput = null;
    let cityInlineSaveBtn = null;
    let cityInlineCancelBtn = null;
    let cityInlineMsg = null;

    function syncHiddenNames() {
      if (countryNameInput && countrySelect) {
        const cOpt = countrySelect.options[countrySelect.selectedIndex];
        countryNameInput.value = cOpt && countrySelect.value ? safeText(cOpt.textContent) : "";
      }
      if (stateNameInput && stateSelect) {
        const sOpt = stateSelect.options[stateSelect.selectedIndex];
        stateNameInput.value = sOpt && stateSelect.value ? safeText(sOpt.textContent) : "";
      }
      if (cityNameInput && citySelect) {
        const cityOpt = citySelect.options[citySelect.selectedIndex];
        cityNameInput.value = cityOpt && citySelect.value ? safeText(cityOpt.textContent) : "";
      }
    }

    function countryMeta() {
      if (!countrySelect) return null;
      const id = parseInt(countrySelect.value || "", 10);
      if (!id) return null;
      return countriesById.get(id) || null;
    }

    async function fetchJson(url) {
      const res = await fetch(url, { credentials: "same-origin" });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error((data && data.error) || "Unable to load geo data");
      return Array.isArray(data.data) ? data.data : [];
    }

    async function postJson(url, payload) {
      const res = await fetch(url, {
        method: "POST",
        credentials: "same-origin",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload || {}),
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) throw new Error((data && data.error) || "Unable to save");
      return data;
    }

    function ensureCityInlineUi() {
      if (!citySelect || cityInlineWrap) return;
      cityInlineWrap = document.createElement("div");
      cityInlineWrap.className = "border rounded p-2 mt-2 d-none";
      cityInlineWrap.innerHTML = [
        '<label class="form-label mb-1">Add new city</label>',
        '<div class="input-group input-group-sm">',
        '  <input type="text" class="form-control" placeholder="Enter city name">',
        '  <button type="button" class="btn btn-primary">Add</button>',
        '  <button type="button" class="btn btn-light">Cancel</button>',
        '</div>',
        '<div class="small text-muted mt-1"></div>'
      ].join("");
      cityInlineInput = cityInlineWrap.querySelector("input");
      const btns = cityInlineWrap.querySelectorAll("button");
      cityInlineSaveBtn = btns[0];
      cityInlineCancelBtn = btns[1];
      cityInlineMsg = cityInlineWrap.querySelector(".small");
      citySelect.parentElement && citySelect.parentElement.appendChild(cityInlineWrap);

      cityInlineCancelBtn.addEventListener("click", () => {
        cityInlineWrap.classList.add("d-none");
        citySelect.value = "";
        if (cityInlineInput) cityInlineInput.value = "";
        if (cityInlineMsg) cityInlineMsg.textContent = "";
        syncHiddenNames();
      });
      cityInlineSaveBtn.addEventListener("click", async () => {
        const stateId = parseInt(stateSelect && stateSelect.value ? stateSelect.value : "", 10) || 0;
        const cityName = safeText(cityInlineInput ? cityInlineInput.value : "");
        if (!stateId) {
          if (cityInlineMsg) cityInlineMsg.textContent = "Select a state first.";
          return;
        }
        if (!cityName) {
          if (cityInlineMsg) cityInlineMsg.textContent = "Enter city name.";
          return;
        }
        if (!cfg.apiCreateCity) {
          if (cityInlineMsg) cityInlineMsg.textContent = "City creation endpoint is not configured.";
          return;
        }
        cityInlineSaveBtn.disabled = true;
        if (cityInlineMsg) cityInlineMsg.textContent = "Saving...";
        try {
          const data = await postJson(cfg.apiCreateCity, { action: "create_city", state_id: stateId, name: cityName });
          const row = data && data.data ? data.data : null;
          await loadCities(stateId, row && row.name ? row.name : cityName);
          cityInlineWrap.classList.add("d-none");
          if (cityInlineInput) cityInlineInput.value = "";
          if (cityInlineMsg) cityInlineMsg.textContent = "";
        } catch (e) {
          if (cityInlineMsg) cityInlineMsg.textContent = e && e.message ? e.message : "Unable to save city.";
        } finally {
          cityInlineSaveBtn.disabled = false;
        }
      });
    }

    async function loadCountries(selectedCountryName) {
      if (!countrySelect || !cfg.apiCountries) return;
      setSelectLoading(countrySelect, "Loading countries...");
      countries = await fetchJson(cfg.apiCountries);
      countriesById = new Map(countries.map((c) => [parseInt(c.id, 10), c]));

      countrySelect.disabled = false;
      countrySelect.innerHTML = "";
      countrySelect.appendChild(createOption("", cfg.countryPlaceholder, false));
      countries.forEach((row) => {
        const opt = createOption(row.id, row.name, false);
        opt.dataset.code = safeText(row.code).toUpperCase();
        opt.dataset.phonecode = safeText(row.phonecode);
        countrySelect.appendChild(opt);
      });

      let selected = false;
      if (selectedCountryName) {
        selected = selectByText(countrySelect, selectedCountryName);
      }
      if (!selected) {
        const ng = countries.find((c) => safeText(c.code).toUpperCase() === safeText(cfg.defaultCountryCode).toUpperCase());
        if (ng) {
          countrySelect.value = String(ng.id);
          selected = true;
        }
      }
      if (!selected) {
        countrySelect.value = "";
      }
      syncHiddenNames();
      if (typeof cfg.onCountryMeta === "function") cfg.onCountryMeta(countryMeta());
    }

    async function loadStates(countryId, selectedStateName) {
      if (!stateSelect) return;
      if (!countryId) {
        setSelectEmpty(stateSelect, cfg.statePlaceholder);
        if (citySelect) setSelectEmpty(citySelect, cfg.cityPlaceholder);
        states = [];
        cities = [];
        syncHiddenNames();
        return;
      }
      setSelectLoading(stateSelect, "Loading states...");
      if (citySelect) setSelectLoading(citySelect, "Loading cities...");
      const url = cfg.apiStates + "&country_id=" + encodeURIComponent(countryId);
      states = await fetchJson(url);
      stateSelect.disabled = false;
      stateSelect.innerHTML = "";
      stateSelect.appendChild(createOption("", states.length ? cfg.statePlaceholder : cfg.emptyStatesLabel, false));
      states.forEach((row) => stateSelect.appendChild(createOption(row.id, row.name, false)));
      if (selectedStateName) {
        selectByText(stateSelect, selectedStateName);
      } else {
        stateSelect.value = "";
      }
      syncHiddenNames();
    }

    async function loadCities(stateId, selectedCityName) {
      if (!citySelect) return;
      if (!stateId) {
        setSelectEmpty(citySelect, cfg.cityPlaceholder);
        cities = [];
        syncHiddenNames();
        return;
      }
      setSelectLoading(citySelect, "Loading cities...");
      const url = cfg.apiCities + "&state_id=" + encodeURIComponent(stateId);
      cities = await fetchJson(url);
      citySelect.disabled = false;
      citySelect.innerHTML = "";
      citySelect.appendChild(createOption("", cities.length ? cfg.cityPlaceholder : cfg.emptyCitiesLabel, false));
      cities.forEach((row) => citySelect.appendChild(createOption(row.id, row.name, false)));
      if (cfg.allowCreateCity && cfg.apiCreateCity) {
        citySelect.appendChild(createOption("__new__", cfg.cityNotListedLabel, false));
      }
      if (selectedCityName) {
        selectByText(citySelect, selectedCityName);
      } else {
        citySelect.value = "";
      }
      syncHiddenNames();
    }

    async function init(prefill) {
      const pf = prefill || {};
      await loadCountries(pf.country || "");
      const countryId = parseInt(countrySelect && countrySelect.value ? countrySelect.value : "", 10) || 0;
      await loadStates(countryId, pf.state || "");
      const stateId = parseInt(stateSelect && stateSelect.value ? stateSelect.value : "", 10) || 0;
      await loadCities(stateId, pf.city || "");

      // Preserve legacy values if not found in geo list.
      if (stateNameInput && pf.state && !safeText(stateNameInput.value)) {
        ensureLegacyOption(stateSelect, pf.state, "Current: ");
        stateNameInput.value = safeText(pf.state);
      }
      if (cityNameInput && pf.city && !safeText(cityNameInput.value)) {
        ensureLegacyOption(citySelect, pf.city, "Current: ");
        cityNameInput.value = safeText(pf.city);
      }
      if (countryNameInput && pf.country && !safeText(countryNameInput.value)) {
        ensureLegacyOption(countrySelect, pf.country, "Current: ");
        countryNameInput.value = safeText(pf.country);
      }
    }

    if (countrySelect) {
      countrySelect.addEventListener("change", async function () {
        syncHiddenNames();
        if (typeof cfg.onCountryMeta === "function") cfg.onCountryMeta(countryMeta());
        const countryId = parseInt(countrySelect.value || "", 10) || 0;
        try {
          await loadStates(countryId, "");
          const stateId = parseInt(stateSelect && stateSelect.value ? stateSelect.value : "", 10) || 0;
          await loadCities(stateId, "");
        } catch (_) {
          setSelectEmpty(stateSelect, cfg.emptyStatesLabel);
          setSelectEmpty(citySelect, cfg.emptyCitiesLabel);
        }
      });
    }
    if (stateSelect) {
      stateSelect.addEventListener("change", async function () {
        syncHiddenNames();
        const stateId = parseInt(stateSelect.value || "", 10) || 0;
        try {
          await loadCities(stateId, "");
        } catch (_) {
          setSelectEmpty(citySelect, cfg.emptyCitiesLabel);
        }
      });
    }
    if (citySelect) {
      ensureCityInlineUi();
      citySelect.addEventListener("change", function () {
        if (citySelect.value === "__new__") {
          if (cityInlineWrap) cityInlineWrap.classList.remove("d-none");
          if (cityInlineInput) {
            cityInlineInput.value = "";
            cityInlineInput.focus();
          }
          if (cityInlineMsg) cityInlineMsg.textContent = "";
          if (cityNameInput) cityNameInput.value = "";
          return;
        }
        if (cityInlineWrap) cityInlineWrap.classList.add("d-none");
        syncHiddenNames();
      });
    }

    return {
      init,
      syncHiddenNames,
      getNames: function () {
        syncHiddenNames();
        return {
          country: countryNameInput ? countryNameInput.value : "",
          state: stateNameInput ? stateNameInput.value : "",
          city: cityNameInput ? cityNameInput.value : "",
        };
      },
      getCountryMeta: countryMeta,
    };
  }

  window.GeoHelper = {
    bind: GeoBind,
  };
})();
