(function (window) {
  const allowedCols = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

  function getColSize(def, opts) {
    const optObj = (def.options && typeof def.options === 'object' && !Array.isArray(def.options)) ? def.options : {};
    const raw = parseInt(def.col || optObj.col || opts.defaultCol || 6, 10);
    return allowedCols.includes(raw) ? raw : (opts.defaultCol || 6);
  }

  function normalizeOptions(def) {
    if (!def || typeof def !== 'object') return [];
    if (Array.isArray(def.options)) return def.options;
    const optObj = (def.options && typeof def.options === 'object') ? def.options : {};
    if (Array.isArray(optObj.options)) return optObj.options;
    if (Array.isArray(optObj.values)) return optObj.values;
    if (optObj.values && typeof optObj.values === 'object' && !Array.isArray(optObj.values)) {
      return Object.entries(optObj.values).map(([value, label]) => ({ value, label }));
    }
    const ignoredKeys = [
      'input', 'col', 'col_size', 'colspan', 'width',
      'placeholder', 'help_text', 'rows',
      'dependency', 'depends_on', 'dependency_map', 'parent_key', 'map'
    ];
    const keys = Object.keys(optObj || {}).filter(k => !ignoredKeys.includes(k));
    if (keys.length && keys.every(k => typeof optObj[k] === 'string' || typeof optObj[k] === 'number')) {
      return keys.map(k => ({ value: k, label: optObj[k] }));
    }
    return [];
  }

  function mapOptionLabel(def, value) {
    const opts = normalizeOptions(def);
    const match = opts.find(o => {
      const val = (typeof o === 'object') ? (o.value || o.code || o.label || o.name || o.text || '') : o;
      return String(val) === String(value);
    });
    if (match) {
      if (typeof match === 'object') {
        return match.label || match.name || match.title || match.text || match.value || match.code || String(value);
      }
      return String(match);
    }
    return String(value);
  }

  function formatDisplayValue(def, rawVal) {
    if (rawVal === null || rawVal === undefined || rawVal === '') return '';
    const type = (def.type || '').toLowerCase();
    if (Array.isArray(rawVal)) {
      const mapped = rawVal.map(v => mapOptionLabel(def, v)).filter(Boolean);
      return mapped.join(', ');
    }
    if (type === 'bool' || type === 'boolean') {
      return (rawVal === true || rawVal === '1' || rawVal === 1 || String(rawVal).toLowerCase() === 'yes') ? 'Yes' : 'No';
    }
    if (type === 'select' || type === 'multiselect') {
      return mapOptionLabel(def, rawVal);
    }
    if (type === 'datetime') {
      const str = String(rawVal).replace('T', ' ').trim();
      const d = new Date(str);
      if (!isNaN(d.getTime())) {
        const pad = (n) => n.toString().padStart(2, '0');
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} @ ${pad(d.getHours())}:${pad(d.getMinutes())}`;
      }
    }
    return String(rawVal);
  }

  function parseDependency(def) {
    if (!def || typeof def !== 'object') return null;
    const optObj = (def.options && typeof def.options === 'object' && !Array.isArray(def.options)) ? def.options : {};
    const depObj = (optObj.dependency && typeof optObj.dependency === 'object') ? optObj.dependency : null;
    const parentKey = String(
      (depObj && depObj.parent_key) || optObj.depends_on || optObj.parent_key || ''
    ).trim();
    const mapRaw = (depObj && depObj.map && typeof depObj.map === 'object')
      ? depObj.map
      : (optObj.dependency_map && typeof optObj.dependency_map === 'object' ? optObj.dependency_map : {});
    if (!parentKey) return null;
    const map = {};
    Object.keys(mapRaw || {}).forEach(k => {
      const key = String(k || '').trim();
      if (!key) return;
      const arr = Array.isArray(mapRaw[k]) ? mapRaw[k] : [];
      map[key] = arr.map(v => String(v || '').trim()).filter(Boolean);
    });
    return { parentKey: parentKey, map: map };
  }

  function optionEntry(opt) {
    if (typeof opt === 'object') {
      const value = String(opt.value || opt.code || opt.label || opt.name || opt.text || '').trim();
      const label = String(opt.label || opt.name || opt.title || opt.text || opt.value || opt.code || '').trim();
      return { value, label: label || value };
    }
    const value = String(opt || '').trim();
    return { value, label: value };
  }

  function renderSelectOptions(selectEl, optionsArr, currentVal, isMulti, placeholder) {
    if (!selectEl) return;
    const prevMulti = Array.isArray(currentVal) ? currentVal.map(v => String(v)) : [];
    const prevSingle = currentVal == null ? '' : String(currentVal);
    selectEl.innerHTML = '';
    if (!isMulti) {
      selectEl.appendChild(new Option(placeholder || 'Select...', ''));
    }
    const allowedValues = [];
    (Array.isArray(optionsArr) ? optionsArr : []).forEach(raw => {
      const entry = optionEntry(raw);
      if (!entry.value && !entry.label) return;
      const opt = new Option(entry.label, entry.value);
      selectEl.appendChild(opt);
      if (entry.value) allowedValues.push(entry.value);
    });

    if (isMulti) {
      Array.from(selectEl.options).forEach(o => { o.selected = prevMulti.includes(o.value); });
    } else {
      selectEl.value = allowedValues.includes(prevSingle) ? prevSingle : '';
    }
  }

  function applyDependencies(container, defs, options = {}) {
    const list = Array.isArray(defs) ? defs : [];
    if (!container || !list.length) return;
    const savedValues = options._savedValues || {};

    const byKey = {};
    list.forEach(def => {
      const key = String(def.field_key || def.key || '').trim();
      if (key) byKey[key] = def;
    });

    list.forEach(def => {
      const key = String(def.field_key || def.key || '').trim();
      if (!key) return;
      const dep = parseDependency(def);
      if (!dep || !dep.parentKey) return;

      const childEl = container.querySelector('[data-cf-key="' + key.replace(/"/g, '\\"') + '"]');
      const parentEl = container.querySelector('[data-cf-key="' + dep.parentKey.replace(/"/g, '\\"') + '"]');
      if (!childEl || !parentEl) return;

      const childType = (childEl.dataset.cfType || '').toLowerCase();
      if (!['select', 'multiselect'].includes(childType)) return;
      const isMulti = childType === 'multiselect';
      let allOptions = normalizeOptions(def).map(optionEntry).filter(o => o.value);
      if (!allOptions.length) {
        const fromMap = new Set();
        Object.keys(dep.map || {}).forEach((parentVal) => {
          const arr = Array.isArray(dep.map[parentVal]) ? dep.map[parentVal] : [];
          arr.forEach((v) => {
            const vv = String(v || '').trim();
            if (vv) fromMap.add(vv);
          });
        });
        allOptions = Array.from(fromMap).map((v) => ({ value: v, label: v }));
      }
      const allSet = new Set(allOptions.map(o => o.value));
      const normalizeText = (v) => String(v || '').trim().toLowerCase();
      const optionValueByLabelNorm = new Map();
      const optionValueByValueNorm = new Map();
      allOptions.forEach((o) => {
        const val = String(o.value || '').trim();
        const lab = String(o.label || o.value || '').trim();
        if (val) {
          optionValueByValueNorm.set(normalizeText(val), val);
        }
        if (lab) {
          optionValueByLabelNorm.set(normalizeText(lab), val);
        }
      });

      const depMapEntries = Object.entries(dep.map || {});
      const getDepValuesForParent = (parentValue, parentLabel) => {
        const candidates = [String(parentValue || '').trim(), String(parentLabel || '').trim()].filter(Boolean);
        for (let i = 0; i < candidates.length; i += 1) {
          const c = candidates[i];
          if (Array.isArray(dep.map[c])) return dep.map[c];
          const hit = depMapEntries.find(([k]) => normalizeText(k) === normalizeText(c));
          if (hit && Array.isArray(hit[1])) return hit[1];
        }
        return [];
      };

      const savedVal = Object.prototype.hasOwnProperty.call(savedValues, key) ? savedValues[key] : undefined;
      let isFirstRun = true;

      const updateChild = function () {
        const parentSelections = parentEl.multiple
          ? Array.from(parentEl.selectedOptions || []).map((o) => ({
            value: String(o.value || '').trim(),
            label: String(o.text || o.label || '').trim()
          })).filter((o) => o.value || o.label)
          : (() => {
            const v = String(parentEl.value || '').trim();
            const idx = parentEl.selectedIndex;
            const lbl = (idx >= 0 && parentEl.options && parentEl.options[idx])
              ? String(parentEl.options[idx].text || parentEl.options[idx].label || '').trim()
              : '';
            return (v || lbl) ? [{ value: v, label: lbl }] : [];
          })();
        const currentVal = isFirstRun
          ? (isMulti
              ? (Array.isArray(savedVal) ? savedVal.map(v => String(v)) : [])
              : (savedVal !== undefined && savedVal !== null ? String(savedVal) : ''))
          : (isMulti
              ? Array.from(childEl.selectedOptions || []).map(o => String(o.value || ''))
              : String(childEl.value || ''));
        isFirstRun = false;

        if (!parentSelections.length) {
          const before = isMulti
            ? Array.from(childEl.selectedOptions || []).map(o => String(o.value || '')).join('|')
            : String(childEl.value || '');
          renderSelectOptions(childEl, [], currentVal, isMulti, options.selectPlaceholder || 'Select...');
          childEl.disabled = true;
          const after = isMulti
            ? Array.from(childEl.selectedOptions || []).map(o => String(o.value || '')).join('|')
            : String(childEl.value || '');
          if (after !== before) {
            childEl.dispatchEvent(new Event('change', { bubbles: true }));
          }
          return;
        }

        const allowed = new Set();
        parentSelections.forEach((sel) => {
          const arr = getDepValuesForParent(sel.value, sel.label);
          arr.forEach(v => {
            const vvRaw = String(v || '').trim();
            if (!vvRaw) return;
            if (allSet.has(vvRaw)) {
              allowed.add(vvRaw);
              return;
            }
            const byValue = optionValueByValueNorm.get(normalizeText(vvRaw));
            if (byValue && allSet.has(byValue)) {
              allowed.add(byValue);
              return;
            }
            const byLabel = optionValueByLabelNorm.get(normalizeText(vvRaw));
            if (byLabel && allSet.has(byLabel)) {
              allowed.add(byLabel);
            }
          });
        });

        const filtered = allOptions.filter(o => allowed.has(o.value));
        const before = isMulti
          ? Array.from(childEl.selectedOptions || []).map(o => String(o.value || '')).join('|')
          : String(childEl.value || '');
        renderSelectOptions(childEl, filtered, currentVal, isMulti, options.selectPlaceholder || 'Select...');
        childEl.disabled = filtered.length === 0;
        const after = isMulti
          ? Array.from(childEl.selectedOptions || []).map(o => String(o.value || '')).join('|')
          : String(childEl.value || '');
        if (after !== before) {
          childEl.dispatchEvent(new Event('change', { bubbles: true }));
        }
      };

      parentEl.addEventListener('change', updateChild);
      updateChild();
    });
  }

  function createInput(def, val, opts, id) {
    const type = (def.type || 'text').toLowerCase();
    const optObj = (def.options && typeof def.options === 'object' && !Array.isArray(def.options)) ? def.options : {};
    const optionsArr = normalizeOptions(def);
    const inputHint = (optObj.input || '').toString().toLowerCase();
    const placeholder = String(optObj.placeholder || def.placeholder || def.label || def.key || def.field_key || '').trim();

    if (type === 'select' || type === 'multiselect') {
      const sel = document.createElement('select');
      sel.className = opts.selectClass || 'form-select';
      sel.id = id;
      if (type === 'multiselect') sel.multiple = true;
      sel.appendChild(new Option(opts.selectPlaceholder || 'Select...', ''));
      optionsArr.forEach(opt => {
        const v = (typeof opt === 'object') ? (opt.value || opt.code || opt.label || opt.name || opt.text || '') : String(opt);
        const l = (typeof opt === 'object') ? (opt.label || opt.name || opt.title || opt.text || opt.value || opt.code || '') : String(opt);
        if (!v && !l) return;
        sel.appendChild(new Option(l, v));
      });
      if (type === 'multiselect' && Array.isArray(val)) {
        Array.from(sel.options).forEach(o => { if (val.includes(o.value)) o.selected = true; });
      } else if (val !== undefined && val !== null) {
        sel.value = val;
      }
      return { el: sel, inputType: type };
    }

    if (type === 'bool' || type === 'boolean') {
      const sel = document.createElement('select');
      sel.className = opts.selectClass || 'form-select';
      sel.id = id;
      sel.appendChild(new Option(opts.selectPlaceholder || 'Select...', ''));
      sel.appendChild(new Option('Yes', '1'));
      sel.appendChild(new Option('No', '0'));
      if (val === true || val === '1' || val === 1) sel.value = '1';
      else if (val === false || val === '0' || val === 0) sel.value = '0';
      return { el: sel, inputType: type };
    }

    if (type === 'textarea' || inputHint === 'textarea') {
      const ta = document.createElement('textarea');
      ta.className = opts.textareaClass || opts.inputClass || 'form-control';
      ta.id = id;
      const configuredRows = parseInt(optObj.rows || opts.textareaRows || 3, 10);
      ta.rows = (configuredRows >= 2 && configuredRows <= 12) ? configuredRows : 3;
      ta.placeholder = placeholder;
      if (val !== undefined && val !== null) ta.value = val;
      return { el: ta, inputType: type };
    }

    const input = document.createElement('input');
    input.className = opts.inputClass || 'form-control';
    input.id = id;
    const htmlType = (type === 'number') ? 'number'
      : (type === 'date') ? 'date'
      : (type === 'datetime') ? 'datetime-local'
      : (['email', 'tel', 'url'].includes(inputHint) ? inputHint : 'text');
    input.type = htmlType;
    input.placeholder = placeholder;
    if (val !== undefined && val !== null) input.value = val;
    return { el: input, inputType: type };
  }

  function getHelpText(def) {
    const optObj = (def && def.options && typeof def.options === 'object' && !Array.isArray(def.options)) ? def.options : {};
    return String(optObj.help_text || def.help_text || '').trim();
  }

  function render(container, defs, values, options = {}) {
    if (!container) return;
    const wrapEl = options.wrapEl;
    const list = Array.isArray(defs) ? defs : [];
    container.innerHTML = '';
    container.classList.add('row', 'g-2');
    if (!list.length) {
      if (options.emptyHtml) container.innerHTML = options.emptyHtml;
      if (wrapEl && options.hideWhenEmpty !== false) wrapEl.classList.add('d-none');
      return;
    }
    if (wrapEl) wrapEl.classList.remove('d-none');

    list.forEach(def => {
      const key = def.field_key || def.key;
      if (!key) return;
      const label = def.label || key;
      const required = !!def.required;
      const val = (values && Object.prototype.hasOwnProperty.call(values, key)) ? values[key] : '';
      const colSize = getColSize(def, options);
      const wrap = document.createElement('div');
      wrap.className = `col-md-${colSize}` + (options.cellClass ? ` ${options.cellClass}` : '');
      const id = (options.idPrefix || 'cf_') + key;
      const labelEl = document.createElement('label');
      labelEl.className = options.labelClass || 'form-label small text-muted';
      labelEl.setAttribute('for', id);
      labelEl.textContent = label + (required ? ' *' : '');
      wrap.appendChild(labelEl);

      const { el: input, inputType } = createInput(def, val, options, id);
      input.dataset.cfKey = key;
      input.dataset.cfType = inputType || def.type || 'text';
      if (required) input.required = true;
      wrap.appendChild(input);
      const helpText = getHelpText(def);
      if (helpText) {
        const helpEl = document.createElement('div');
        helpEl.className = options.helpClass || 'form-text';
        helpEl.textContent = helpText;
        wrap.appendChild(helpEl);
      }
      container.appendChild(wrap);
    });

    applyDependencies(container, list, Object.assign({}, options, { _savedValues: values || {} }));
  }

  function collect(container) {
    if (!container) return null;
    const out = {};
    container.querySelectorAll('[data-cf-key]').forEach(input => {
      const key = input.dataset.cfKey;
      const type = (input.dataset.cfType || '').toLowerCase();
      let val;
      if (type === 'multiselect') {
        val = Array.from(input.selectedOptions || []).map(o => o.value).filter(Boolean);
        if (!val.length) val = null;
      } else if (type === 'bool' || type === 'boolean') {
        const v = input.value;
        if (v === '') val = null; else val = (v === '1' || v === 'true' || v === 'yes');
      } else {
        val = input.value;
        if (val === '') val = null;
      }
      if (val !== null && val !== undefined) {
        out[key] = val;
      }
    });
    return Object.keys(out).length ? out : null;
  }

  function display(container, defs, values, options = {}) {
    if (!container) return 0;
    const wrapEl = options.wrapEl;
    const dividerEl = options.dividerEl;
    const list = Array.isArray(defs) ? defs : [];
    container.innerHTML = '';
    const addClasses = (el, cls) => {
      if (!el || !cls) return;
      cls.toString().split(/\s+/).filter(Boolean).forEach(c => el.classList.add(c));
    };
    addClasses(container, options.containerRowClass || 'row');
    container.classList.add('g-2');
    let count = 0;
    list.forEach(def => {
      const key = def.field_key || def.key;
      if (!key) return;
      const rawVal = (values && Object.prototype.hasOwnProperty.call(values, key)) ? values[key] : '';
      if (rawVal === null || rawVal === undefined || rawVal === '') return;
      const label = def.label || key;
      const wrap = document.createElement('div');
      wrap.className = '';
      addClasses(wrap, options.rowClass || 'col-12 row g-1');
      const labelEl = document.createElement('div');
      labelEl.className = '';
      addClasses(labelEl, options.labelClass || 'col-5 text-muted');
      labelEl.textContent = label;
      const valEl = document.createElement('div');
      valEl.className = '';
      addClasses(valEl, options.valueClass || 'col-7 fw-semibold text-dark');
      valEl.textContent = formatDisplayValue(def, rawVal);
      wrap.appendChild(labelEl);
      wrap.appendChild(valEl);
      container.appendChild(wrap);
      count++;
    });
    const hideEmpty = options.hideWhenEmpty !== false;
    if (count === 0) {
      if (hideEmpty) container.classList.add('d-none');
      if (wrapEl && hideEmpty) wrapEl.classList.add('d-none');
      if (dividerEl && hideEmpty) dividerEl.classList.add('d-none');
    } else {
      container.classList.remove('d-none');
      if (wrapEl) wrapEl.classList.remove('d-none');
      if (dividerEl) dividerEl.classList.remove('d-none');
    }
    return count;
  }

  window.CustomFieldRenderer = { render, collect, display, formatDisplayValue };
})(window);
