(function () {
  'use strict';

  var config       = window.DASH_BUILDER_CONFIG  || {};
  var tmplCfg      = window.DASH_TEMPLATE_CONFIG || {};
  var canPersonalise = !!(config.canPersonalise || config.canManage);
  var canManage      = !!config.canManage;

  /* ── Shared state ─────────────────────────────────────────────────────────── */
  var catalog   = [];   // widget catalog (loaded once)
  var sortable  = null; // SortableJS instance

  /* ── Helpers ──────────────────────────────────────────────────────────────── */
  function esc(s) {
    return String(s == null ? '' : s)
      .replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
  }

  function api(path, opts) {
    return fetch(path, opts || {})
      .then(function(r){ return r.json().then(function(d){ return { ok: r.ok, status: r.status, data: d }; }); });
  }

  function colClass(size) {
    if (size === 'sm') return 'col-6 col-md-3';
    if (size === 'lg') return 'col-12 col-md-6';
    return 'col-6 col-md-4'; // md default
  }

  function toneIcon(tone) {
    var map = { primary:'#405189', success:'#0ab39c', warning:'#f7b84b', danger:'#f06548', info:'#299cdb', secondary:'#74788d' };
    return map[tone] || '#405189';
  }

  function trendHtml(pct) {
    if (pct === null || pct === undefined) return '';
    var cls = pct > 0 ? 'dash-trend-up' : (pct < 0 ? 'dash-trend-down' : 'dash-trend-flat');
    var arrow = pct > 0 ? '▲' : (pct < 0 ? '▼' : '→');
    return '<span class="dash-trend ' + cls + '">' + arrow + ' ' + Math.abs(pct) + '%</span>';
  }

  function nextSize(s) {
    return s === 'sm' ? 'md' : (s === 'md' ? 'lg' : 'sm');
  }

  var WAVE_STYLES = ['dash-wave-style-a', 'dash-wave-style-b', 'dash-wave-style-c'];

  /* ── Widget card HTML ─────────────────────────────────────────────────────── */
  function buildWidgetCard(w, editMode, idx) {
    var size      = w.size || 'md';
    var tone      = w.tone || 'secondary';
    var waveStyle = WAVE_STYLES[(idx || 0) % 3];
    var valueStr  = (w.value !== null && w.value !== undefined) ? Number(w.value).toLocaleString() : '—';
    var trend     = trendHtml(w.trend_pct);

    var removeBtn = editMode
      ? '<button class="dash-remove-btn" data-key="' + esc(w.key) + '" title="Remove"><i class="ri-close-line"></i></button>'
      : '';

    var editBar = editMode
      ? '<div class="d-flex align-items-center gap-1 mb-2">'
          + '<span class="dash-drag-handle me-1"><i class="ri-drag-move-2-line"></i></span>'
          + '<div class="dash-size-chips">'
          + ['sm','md','lg'].map(function(s){
              return '<span class="dash-size-chip' + (size === s ? ' active' : '') + '" data-key="' + esc(w.key) + '" data-size="' + s + '">' + s.toUpperCase() + '</span>';
            }).join('')
          + '</div>'
          + '</div>'
      : '';

    return '<div class="' + colClass(size) + ' dash-widget-item" data-key="' + esc(w.key) + '" data-size="' + esc(size) + '">'
      + '<div class="card card-animate dash-wave-card dash-tone-' + esc(tone) + ' ' + waveStyle + ' mb-0" style="position:relative;">'
      + removeBtn
      + '<div class="card-body">'
      + editBar
      + '<p class="text-uppercase fw-medium text-muted text-truncate mb-2">' + esc(w.label) + '</p>'
      + '<div class="d-flex align-items-center gap-3 mb-1">'
      + '<div class="avatar-sm flex-shrink-0"><span class="avatar-title bg-' + esc(tone) + '-subtle text-' + esc(tone) + ' rounded-2 fs-2"><i class="' + esc(w.icon || 'ri-bar-chart-line') + '"></i></span></div>'
      + '<div class="flex-grow-1"><h4 class="fs-4 mb-0">' + esc(valueStr) + (trend ? ' ' + trend : '') + '</h4></div>'
      + '</div>'
      + (w.description ? '<p class="text-muted small mb-0 mt-1 text-truncate">' + esc(w.description) + '</p>' : '')
      + '</div></div></div>';
  }

  /* ── Catalog tile HTML ────────────────────────────────────────────────────── */
  function buildCatalogTile(w, isAdded) {
    return '<div class="col-6 col-md-4">'
      + '<div class="dash-catalog-tile' + (isAdded ? ' is-added' : '') + '" data-add-key="' + esc(w.key) + '">'
      + '<div class="d-flex align-items-start gap-3">'
      + '<div class="dash-catalog-icon"><i class="' + esc(w.icon) + '"></i></div>'
      + '<div class="flex-grow-1 min-w-0">'
      + '<div class="fw-semibold small">' + esc(w.label) + (isAdded ? ' <span class="badge bg-success-subtle text-success">Added</span>' : '') + '</div>'
      + '<div class="text-muted" style="font-size:.75rem;">' + esc(w.description) + '</div>'
      + '</div>'
      + '</div>'
      + '</div>'
      + '</div>';
  }

  /* ── Load catalog (cached in memory) ─────────────────────────────────────── */
  function loadCatalog(cb) {
    if (catalog.length) { cb(catalog); return; }
    api('api/modules/dashboard/?action=catalog').then(function(res) {
      if (res.ok && Array.isArray(res.data.data)) catalog = res.data.data;
      cb(catalog);
    });
  }

  /* ── Widget picker modal ──────────────────────────────────────────────────── */
  function openWidgetPicker(currentKeys, onAdd) {
    var modalEl = document.getElementById('widgetPickerModal');
    var grid    = document.getElementById('widgetPickerGrid');
    if (!modalEl || !grid) return;

    loadCatalog(function(cat) {
      grid.innerHTML = cat.map(function(w) {
        return buildCatalogTile(w, currentKeys.indexOf(w.key) >= 0);
      }).join('');

      grid.querySelectorAll('[data-add-key]').forEach(function(tile) {
        tile.addEventListener('click', function() {
          var key = tile.getAttribute('data-add-key');
          if (tile.classList.contains('is-added')) return;
          tile.classList.add('is-added');
          var w = cat.find(function(c){ return c.key === key; });
          if (w) onAdd(Object.assign({}, w, { size: 'md', position: 0, value: null, trend_pct: null }));
        });
      });

      bootstrap.Modal.getOrCreateInstance(modalEl).show();
    });
  }

  /* ══════════════════════════════════════════════════════════════════════════ */
  /*  PERSONAL DASHBOARD                                                        */
  /* ══════════════════════════════════════════════════════════════════════════ */
  function initPersonalDashboard() {
    var root         = document.getElementById('dashBuilderRoot');
    var notice       = document.getElementById('dashInheritedNotice');
    var btnEdit      = document.getElementById('btnEditLayout');
    var editActions  = document.getElementById('dashEditActions');
    var btnSave      = document.getElementById('btnSaveLayout');
    var btnCancel    = document.getElementById('btnCancelEdit');
    var btnReset     = document.getElementById('btnResetTemplate');
    if (!root) return;

    var widgets     = [];
    var editMode    = false;
    var dataSource  = 'empty';

    /* ── Load ── */
    function load() {
      root.innerHTML = '<div class="col-12 text-center py-5 text-muted"><div class="spinner-border spinner-border-sm me-2"></div>Loading…</div>';
      api('api/modules/dashboard/').then(function(res) {
        if (!res.ok) { root.innerHTML = '<div class="col-12"><div class="alert alert-danger">' + esc((res.data && res.data.error) || 'Unable to load dashboard.') + '</div></div>'; return; }
        widgets    = res.data.widgets || [];
        dataSource = res.data.source  || 'empty';
        if (notice) notice.classList.toggle('d-none', dataSource !== 'role_template');
        if (btnReset) btnReset.style.display = dataSource === 'role_template' ? '' : 'none';
        render();
      });
    }

    /* ── Render ── */
    function render() {
      if (!widgets.length) {
        root.innerHTML = '<div class="col-12 text-center py-5 text-muted">'
          + '<i class="ri-layout-grid-line fs-1 d-block mb-3"></i>'
          + '<p class="mb-0">No widgets yet.</p>'
          + (canPersonalise ? '<button class="btn btn-primary btn-md mt-3" id="btnAddFirst"><i class="ri-add-line me-1"></i>Add your first widget</button>' : '<p class="small">Contact your administrator to configure a dashboard.</p>')
          + '</div>';
        var bf = document.getElementById('btnAddFirst');
        if (bf) bf.addEventListener('click', function(){ openPicker(); });
        return;
      }
      root.innerHTML = widgets.map(function(w, i){ return buildWidgetCard(w, editMode, i); }).join('');
      if (editMode) initSortable();
      bindCardEvents();
    }

    /* ── SortableJS ── */
    function initSortable() {
      if (sortable) { sortable.destroy(); sortable = null; }
      if (typeof Sortable === 'undefined') return;
      sortable = Sortable.create(root, {
        animation: 150,
        handle: '.dash-drag-handle',
        draggable: '.dash-widget-item',
        ghostClass: 'sortable-ghost',
        onEnd: function() {
          var keys = [];
          root.querySelectorAll('.dash-widget-item').forEach(function(el){ keys.push(el.getAttribute('data-key')); });
          widgets.sort(function(a, b){ return keys.indexOf(a.key) - keys.indexOf(b.key); });
        }
      });
    }

    /* ── Card click events ── */
    function bindCardEvents() {
      // Remove buttons
      root.querySelectorAll('.dash-remove-btn').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
          e.stopPropagation();
          var key = btn.getAttribute('data-key');
          widgets = widgets.filter(function(w){ return w.key !== key; });
          render();
        });
      });
      // Size chips
      root.querySelectorAll('.dash-size-chip').forEach(function(chip) {
        chip.addEventListener('click', function(e) {
          e.stopPropagation();
          var key  = chip.getAttribute('data-key');
          var size = chip.getAttribute('data-size');
          widgets = widgets.map(function(w){ return w.key === key ? Object.assign({}, w, { size: size }) : w; });
          render();
        });
      });
    }

    /* ── Open picker ── */
    function openPicker() {
      var currentKeys = widgets.map(function(w){ return w.key; });
      openWidgetPicker(currentKeys, function(w) {
        widgets.push(w);
        render();
      });
    }

    /* ── Edit mode toggle ── */
    function enterEditMode() {
      editMode = true;
      document.body.classList.add('dash-edit-mode');
      render();
      // Add "+ Add Widget" button at bottom
      var addRow = document.createElement('div');
      addRow.className = 'col-12';
      addRow.id = 'dashAddWidgetRow';
      addRow.innerHTML = '<button class="btn btn-outline-primary btn-md w-100" id="btnAddWidget"><i class="ri-add-line me-1"></i>Add Widget</button>';
      root.appendChild(addRow);
      addRow.querySelector('#btnAddWidget').addEventListener('click', openPicker);
    }

    function exitEditMode(reloadFromServer) {
      editMode = false;
      document.body.classList.remove('dash-edit-mode');
      if (sortable) { sortable.destroy(); sortable = null; }
      if (reloadFromServer) { load(); } else { render(); }
    }

    /* ── Save ── */
    function save() {
      var layout = widgets.map(function(w, i){
        var el = root.querySelector('[data-key="' + w.key + '"]');
        var size = el ? (el.getAttribute('data-size') || w.size) : w.size;
        return { key: w.key, size: size, position: i };
      });
      if (btnSave) btnSave.disabled = true;
      api('api/modules/dashboard/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ widgets: layout })
      }).then(function(res) {
        if (btnSave) btnSave.disabled = false;
        if (!res.ok) { if (window.crmUiAlert) window.crmUiAlert(res.data.error || 'Save failed.', 'Dashboard'); return; }
        if (window.showSavedModal) window.showSavedModal('Saved', 'Dashboard layout saved successfully.');
        exitEditMode(true);
      });
    }

    /* ── Reset to role template ── */
    function resetToTemplate() {
      if (window.crmUiConfirm) {
        window.crmUiConfirm('Reset to your role\'s default layout? Your personal customisation will be removed.', 'Reset Dashboard', function() {
          api('api/modules/dashboard/?action=reset_personal', { method: 'DELETE' }).then(function(){ load(); });
        });
      } else if (confirm('Reset to role default?')) {
        api('api/modules/dashboard/?action=reset_personal', { method: 'DELETE' }).then(function(){ load(); });
      }
    }

    /* ── Event bindings ── */
    if (btnEdit)   btnEdit.addEventListener('click', enterEditMode);
    if (btnSave)   btnSave.addEventListener('click', save);
    if (btnCancel) btnCancel.addEventListener('click', function(){ exitEditMode(false); widgets = []; load(); });
    if (btnReset)  btnReset.addEventListener('click', resetToTemplate);

    load();
  }

  /* ══════════════════════════════════════════════════════════════════════════ */
  /*  ROLE TEMPLATE DESIGNER                                                    */
  /* ══════════════════════════════════════════════════════════════════════════ */
  function initRoleTemplateDashboard() {
    var root      = document.getElementById('dashTemplateRoot');
    var hint      = document.getElementById('templateEmptyHint');
    var select    = document.getElementById('templateRoleSelect');
    var btnSave   = document.getElementById('btnSaveTemplate');
    var statusMsg = document.getElementById('templateStatusMsg');
    if (!root || !select) return;

    var widgets   = [];
    var currentRoleId   = 0;
    var currentRoleName = '';

    /* ── Load role template ── */
    function loadTemplate(roleId, roleName) {
      currentRoleId   = roleId;
      currentRoleName = roleName;
      if (hint) hint.style.display = 'none';
      root.innerHTML = '<div class="col-12 text-center py-4 text-muted"><div class="spinner-border spinner-border-sm me-2"></div>Loading template…</div>';
      if (btnSave) { btnSave.disabled = false; btnSave.textContent = 'Save Template'; }
      api('api/modules/dashboard/?action=role_template&role_id=' + roleId).then(function(res) {
        if (!res.ok) { root.innerHTML = '<div class="col-12"><div class="alert alert-danger">' + esc((res.data && res.data.error) || 'Load failed.') + '</div></div>'; return; }
        widgets = res.data.widgets || [];
        if (statusMsg) statusMsg.textContent = widgets.length ? widgets.length + ' widget(s) configured' : 'No widgets yet';
        render();
      });
    }

    /* ── Render ── */
    function render() {
      if (!widgets.length) {
        root.innerHTML = '<div class="col-12 text-center py-4 text-muted">'
          + '<i class="ri-layout-grid-line fs-1 d-block mb-2"></i>'
          + '<p class="mb-0">No widgets in this template yet.</p>'
          + '<button class="btn btn-outline-primary btn-md mt-3" id="btnAddFirst"><i class="ri-add-line me-1"></i>Add Widget</button>'
          + '</div>';
        var bf = document.getElementById('btnAddFirst');
        if (bf) bf.addEventListener('click', openPicker);
        return;
      }
      root.innerHTML = widgets.map(function(w, i){ return buildWidgetCard(w, true, i); }).join('');
      // Add widget button at bottom
      var addRow = document.createElement('div');
      addRow.className = 'col-12';
      addRow.innerHTML = '<button class="btn btn-outline-primary btn-md w-100" id="btnAddWidget"><i class="ri-add-line me-1"></i>Add Widget</button>';
      root.appendChild(addRow);
      addRow.querySelector('#btnAddWidget').addEventListener('click', openPicker);
      initTemplateSortable();
      bindTemplateCardEvents();
    }

    /* ── SortableJS ── */
    function initTemplateSortable() {
      if (sortable) { sortable.destroy(); sortable = null; }
      if (typeof Sortable === 'undefined') return;
      sortable = Sortable.create(root, {
        animation: 150,
        handle: '.dash-drag-handle',
        draggable: '.dash-widget-item',
        ghostClass: 'sortable-ghost',
        onEnd: function() {
          var keys = [];
          root.querySelectorAll('.dash-widget-item').forEach(function(el){ keys.push(el.getAttribute('data-key')); });
          widgets.sort(function(a, b){ return keys.indexOf(a.key) - keys.indexOf(b.key); });
        }
      });
    }

    /* ── Card events ── */
    function bindTemplateCardEvents() {
      root.querySelectorAll('.dash-remove-btn').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
          e.stopPropagation();
          var key = btn.getAttribute('data-key');
          widgets = widgets.filter(function(w){ return w.key !== key; });
          if (statusMsg) statusMsg.textContent = widgets.length + ' widget(s) configured';
          render();
        });
      });
      root.querySelectorAll('.dash-size-chip').forEach(function(chip) {
        chip.addEventListener('click', function(e) {
          e.stopPropagation();
          var key  = chip.getAttribute('data-key');
          var size = chip.getAttribute('data-size');
          widgets = widgets.map(function(w){ return w.key === key ? Object.assign({}, w, { size: size }) : w; });
          render();
        });
      });
    }

    /* ── Open picker ── */
    function openPicker() {
      var currentKeys = widgets.map(function(w){ return w.key; });
      openWidgetPicker(currentKeys, function(w) {
        widgets.push(w);
        if (statusMsg) statusMsg.textContent = widgets.length + ' widget(s) configured';
        render();
      });
    }

    /* ── Save ── */
    function save() {
      if (!currentRoleId) return;
      var layout = widgets.map(function(w, i){
        var el = root.querySelector('[data-key="' + w.key + '"]');
        var size = el ? (el.getAttribute('data-size') || w.size) : w.size;
        return { key: w.key, size: size, position: i };
      });
      if (btnSave) { btnSave.disabled = true; btnSave.textContent = 'Saving…'; }
      api('api/modules/dashboard/?action=role_template&role_id=' + currentRoleId, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ widgets: layout })
      }).then(function(res) {
        if (btnSave) { btnSave.disabled = false; btnSave.textContent = 'Save Template'; }
        if (!res.ok) { if (window.crmUiAlert) window.crmUiAlert(res.data.error || 'Save failed.', 'Templates'); return; }
        if (window.showSavedModal) window.showSavedModal('Template Saved', 'Dashboard template for "' + currentRoleName + '" saved successfully.');
      });
    }

    /* ── Event bindings ── */
    select.addEventListener('change', function() {
      var opt = select.options[select.selectedIndex];
      if (!select.value) { root.innerHTML = ''; if (hint) hint.style.display = ''; if (btnSave) btnSave.disabled = true; return; }
      loadTemplate(parseInt(select.value, 10), opt.text);
    });
    if (btnSave) btnSave.addEventListener('click', save);
  }

  /* ── Init ────────────────────────────────────────────────────────────────── */
  if (document.getElementById('dashBuilderRoot'))  initPersonalDashboard();
  if (document.getElementById('dashTemplateRoot')) initRoleTemplateDashboard();
})();
