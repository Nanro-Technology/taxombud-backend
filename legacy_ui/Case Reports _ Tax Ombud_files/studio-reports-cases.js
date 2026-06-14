(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiReports = apiMap.casesReports || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/cases/reports');

  const dateInput = document.getElementById('reportDateRange');
  const chartTypeDefault = document.getElementById('reportChartType');
  const btnApply = document.getElementById('reportApply');
  const btnReset = document.getElementById('reportReset');
  const rangeLabel = document.getElementById('reportRangeLabel');
  const chartTypeSelectors = Array.from(document.querySelectorAll('.chart-type'));
  const stuckBody = document.getElementById('stuckCasesBody');
  let stuckTable = null;
  const categorySelect = document.getElementById('reportCategory');
  const csatAvgScore = document.getElementById('csatAvgScore');
  const csatAvgLabel = document.getElementById('csatAvgLabel');
  const csatResponseRate = document.getElementById('csatResponseRate');
  const csatResponseLabel = document.getElementById('csatResponseLabel');

  const chartRefs = {};
  let picker = null;
  let cached = {};
  let requestSeq = 0;

  function syncChartTypesFromDefault() {
    if (!chartTypeDefault) return;
    const def = chartTypeDefault.value;
    chartTypeSelectors.forEach((sel) => {
      if (!sel) return;
      const hasOption = Array.from(sel.options || []).some((opt) => opt.value === def);
      if (hasOption) {
        sel.value = def;
      }
    });
  }

  function formatDate(d) {
    if (!(d instanceof Date)) return '';
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  function getDefaultRange() {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 6); // last 7 days including today
    return [start, end];
  }

  function getRangeParams() {
    if (!picker || !picker.selectedDates.length) {
        const [s, e] = getDefaultRange();
        return { start: formatDate(s), end: formatDate(e) };
    }
    const [start, end] = picker.selectedDates;
    const s = formatDate(start);
    const e = formatDate(end || start);
    return { start: s, end: e };
  }

  function getChartColorsArray(id) {
    const el = document.getElementById(id);
    if (!el) return null;
    const colorsAttr = el.getAttribute('data-colors');
    if (!colorsAttr) return null;
    try {
        const colors = JSON.parse(colorsAttr);
        return colors.map((value) => {
            const color = (value || '').replace(' ', '');
            if (color.indexOf(',') === -1) {
                return getComputedStyle(document.documentElement).getPropertyValue(color) || color;
            }
            const parts = value.split(',');
            if (parts.length === 2) {
                return `rgba(${getComputedStyle(document.documentElement).getPropertyValue(parts[0])},${parts[1]})`;
            }
            return color;
        });
    } catch (err) {
        return null;
    }
  }

  function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str || '';
    return div.innerHTML;
  }

  function destroyChart(id) {
    if (chartRefs[id]) {
      chartRefs[id].destroy();
      delete chartRefs[id];
    }
  }

  function renderChart(id, options) {
    destroyChart(id);
    const el = document.getElementById(id);
    if (!el) return;
    el.innerHTML = '';
    const fallbackColors = ['#0d6efd', '#198754', '#ffc107', '#dc3545', '#0dcaf0', '#6c757d'];
    const safeOptions = options || {};
    if (!safeOptions.chart) {
      safeOptions.chart = { type: 'bar', height: 320, toolbar: { show: false } };
    } else if (!safeOptions.chart.type) {
      safeOptions.chart.type = 'bar';
    }
    if (!Array.isArray(safeOptions.colors) || !safeOptions.colors.length) {
      safeOptions.colors = getChartColorsArray(id) || fallbackColors;
    }
    safeOptions.bar = safeOptions.bar || {};
    safeOptions.line = safeOptions.line || {};
    safeOptions.area = safeOptions.area || {};
    safeOptions.pie = safeOptions.pie || {};
    safeOptions.donut = safeOptions.donut || {};
    const typeKey = safeOptions.chart && safeOptions.chart.type ? safeOptions.chart.type : 'bar';
    safeOptions[typeKey] = safeOptions[typeKey] || {};
    chartRefs[id] = new ApexCharts(el, safeOptions);
    chartRefs[id].render();
  }

  function setChartLoading(id) {
    destroyChart(id);
    const el = document.getElementById(id);
    if (el) {
      el.innerHTML = '<div class="text-center text-muted py-5">Computing values...</div>';
    }
  }

  function setChartError(id, message) {
    destroyChart(id);
    const el = document.getElementById(id);
    if (el) {
      el.innerHTML = `<div class="text-center text-danger py-5">${message || 'Failed to load'}</div>`;
    }
  }

  function baseOptions(type, labels, series, colors, extra = {}) {
    return {
      series,
      chart: { type, height: extra.height || 320, stacked: type === 'bar', toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      dataLabels: { enabled: false },
      xaxis: { categories: labels || [] },
      colors: colors || undefined,
      legend: { position: 'top' },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      ...extra
    };
  }

  function renderSeriesChart(chartId, labels, series, type) {
    const colors = getChartColorsArray(chartId);
    const safeSeries = (series && series.length) ? series : [{ name: 'No data', data: labels.map(() => 0) }];
    const opts = baseOptions(type, labels, safeSeries, colors, { height: 340 });
    renderChart(chartId, opts);
  }

  function renderDonut(chartId, totals, type = 'donut') {
    const colors = getChartColorsArray(chartId);
    const labels = totals.map((t) => t.name || 'Unknown');
    const series = totals.map((t) => Number(t.cnt || 0));
    const data = series.length ? series : [0];
    const opts = {
      series: data,
      labels: labels.length ? labels : ['No data'],
      chart: { type, height: 340 },
      colors: colors || undefined,
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
    };
    renderChart(chartId, opts);
  }

  function renderTotalsDonut(chartId, totals, type = 'donut') {
    const colors = getChartColorsArray(chartId);
    const labels = (totals || []).map((t) => t.name || 'Unknown');
    const series = (totals || []).map((t) => Number(t.cnt || 0));
    const data = series.length ? series : [0];
    const opts = {
      series: data,
      labels: labels.length ? labels : ['No data'],
      chart: { type: type === 'pie' ? 'pie' : 'donut', height: 340 },
      colors: colors || undefined,
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
    };
    renderChart(chartId, opts);
  }


  function renderAging(chartId, aging) {
    const colors = getChartColorsArray(chartId);
    const labels = aging.map((a) => a.bucket || 'Unknown');
    const series = aging.map((a) => Number(a.cnt || 0));
    const opts = {
      series: [{ name: 'Open cases', data: series.length ? series : [0] }],
      chart: { type: 'bar', height: 320, toolbar: { show: false } },
      plotOptions: { bar: { horizontal: false, distributed: true } },
      colors: colors || undefined,
      dataLabels: { enabled: false },
      xaxis: { categories: labels.length ? labels : ['No data'] },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
    };
    renderChart(chartId, opts);
  }


  function renderStuckTable(payload) {
    if (!stuckBody) return;
    if (window.jQuery && stuckTable) {
      stuckTable.clear().destroy();
      stuckTable = null;
    }
    const daysLabel = document.getElementById('stuckDaysLabel');
    if (daysLabel && payload && typeof payload.days_threshold !== 'undefined') {
      daysLabel.textContent = payload.days_threshold;
    }
    const rows = payload.items || [];
    if (!rows.length) {
      stuckBody.innerHTML = `<tr><td colspan="4" class="text-center text-muted">No cases stuck in status.</td></tr>`;
      return;
    }
    const frag = document.createDocumentFragment();
    rows.forEach((r) => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td><a href="studio/cases/view.kml?id=${encodeURIComponent(r.id)}">${escapeHtml(r.case_number || r.id)}</a></td>
        <td>${escapeHtml((r.subject || 'Case').substring(0, 90))}</td>
        <td>${escapeHtml(r.status || '')}</td>
        <td>${r.days_stuck || 0} days</td>
      `;
      frag.appendChild(tr);
    });
    stuckBody.innerHTML = '';
    stuckBody.appendChild(frag);
    if (window.jQuery && jQuery.fn.DataTable) {
      stuckTable = jQuery('#stuckCasesTable').DataTable({
        pageLength: 10,
        lengthMenu: [10, 25, 50],
        order: [[3, 'desc']],
        language: { emptyTable: 'No cases stuck in status.' }
      });
    }
  }

  function renderCsat(summary) {
    if (!csatAvgScore && !csatResponseRate) return;
    const sent = Number(summary?.sent || 0);
    const responses = Number(summary?.responses || 0);
    const avgScore = Number(summary?.avg_score || 0);
    const rate = Number(summary?.response_rate || 0);
    if (csatAvgScore) csatAvgScore.textContent = avgScore ? avgScore.toFixed(2) : '0';
    if (csatAvgLabel) csatAvgLabel.textContent = responses ? `${responses} responses` : 'No responses yet';
    if (csatResponseRate) csatResponseRate.textContent = sent ? `${rate}%` : '0%';
    if (csatResponseLabel) csatResponseLabel.textContent = `${responses}/${sent} responses`;
  }

  function applyDefaultChartTypes() {
    const def = chartTypeDefault ? chartTypeDefault.value : 'bar';
    chartTypeSelectors.forEach((sel) => {
      if (!sel) return;
      const hasOption = Array.from(sel.options || []).some((opt) => opt.value === def);
      if (hasOption) {
        sel.value = def;
      }
    });
  }

  function renderAll() {
    const statusType = (document.querySelector('[data-chart="statusChart"]') || {}).value || 'bar';
    const categoryType = (document.querySelector('[data-chart="categoryChart"]') || {}).value || 'bar';
    const priorityType = (document.querySelector('[data-chart="priorityChart"]') || {}).value || 'bar';
    const donutType = (document.querySelector('[data-chart="categoryDonut"]') || {}).value || 'donut';
    const statusTotalsType = (document.querySelector('[data-chart="statusTotalsChart"]') || {}).value || 'donut';

    if (Object.prototype.hasOwnProperty.call(cached, 'status_over_time')) {
      const labels = cached.labels || [];
      renderSeriesChart('statusChart', labels, cached.status_over_time || [], statusType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'category_over_time')) {
      const labels = cached.labels || [];
      renderSeriesChart('categoryChart', labels, cached.category_over_time || [], categoryType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'priority_over_time')) {
      const labels = cached.labels || [];
      renderSeriesChart('priorityChart', labels, cached.priority_over_time || [], priorityType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'status_totals')) {
      // Resolve status codes → labels and colors from the JSON source of truth
      const getStatusCfg = (typeof window.getCaseStatusConfig === 'function') ? window.getCaseStatusConfig : null;
      const statusTotals = (cached.status_totals || []).map(t => {
        const cfg = getStatusCfg ? getStatusCfg(String(t.name || '').toLowerCase()) : null;
        return { name: (cfg && cfg.label) ? cfg.label : t.name, cnt: t.cnt, color: cfg && cfg.color ? cfg.color : null };
      });
      const statusColors = statusTotals.map(t => t.color).filter(Boolean);
      if (statusColors.length === statusTotals.length) {
        // All slices have JSON colors — pass them directly, bypassing data-colors
        const labels = statusTotals.map(t => t.name || 'Unknown');
        const series = statusTotals.map(t => Number(t.cnt || 0));
        const opts = {
          series: series.length ? series : [0],
          labels: labels.length ? labels : ['No data'],
          chart: { type: statusTotalsType === 'pie' ? 'pie' : 'donut', height: 340 },
          colors: statusColors,
          legend: { position: 'bottom' },
          dataLabels: { enabled: true },
        };
        renderChart('statusTotalsChart', opts);
      } else {
        renderTotalsDonut('statusTotalsChart', statusTotals, statusTotalsType);
      }
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'category_totals')) {
      renderDonut('categoryDonut', cached.category_totals || [], donutType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'aging_open')) {
      renderAging('agingChart', cached.aging_open || []);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'stuck_cases')) {
      renderStuckTable(cached.stuck_cases || {});
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'csat_summary')) {
      renderCsat(cached.csat_summary || {});
    }
  }

  function updateRangeLabel(start, end) {
    if (rangeLabel) {
      rangeLabel.textContent = start && end ? `Showing ${start} to ${end}` : '';
    }
  }

  function fetchReports(e) {
    if (e) e.preventDefault();
    const mySeq = ++requestSeq;
    const { start, end } = getRangeParams();
    const baseParams = new URLSearchParams({ start_date: start, end_date: end });
    if (categorySelect) {
      const cat = (categorySelect.value || '').trim();
      if (cat !== '') {
        baseParams.set('domain_id', cat);
      }
    }
    updateRangeLabel(start, end);
    cached = {};
    setChartLoading('statusChart');
    setChartLoading('categoryChart');
    setChartLoading('priorityChart');
    setChartLoading('categoryDonut');
    setChartLoading('agingChart');
    setChartLoading('statusTotalsChart');
    if (window.jQuery && stuckTable) {
      stuckTable.clear().destroy();
      stuckTable = null;
    }
    if (stuckBody) {
      stuckBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Computing values...</td></tr>';
    }
    if (csatAvgScore) csatAvgScore.textContent = '--';
    if (csatAvgLabel) csatAvgLabel.textContent = 'Computing values...';
    if (csatResponseRate) csatResponseRate.textContent = '--';
    if (csatResponseLabel) csatResponseLabel.textContent = 'Computing values...';
    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Computing values...';
    }

    const sections = ['status', 'category', 'priority', 'totals', 'aging', 'stuck', 'csat'];
    const sectionErrors = {
      status: () => setChartError('statusChart', 'Failed to load status trend.'),
      category: () => setChartError('categoryChart', 'Failed to load category trend.'),
      priority: () => setChartError('priorityChart', 'Failed to load priority trend.'),
      totals: () => {
        setChartError('statusTotalsChart', 'Failed to load status totals.');
        setChartError('categoryDonut', 'Failed to load category totals.');
      },
      aging: () => setChartError('agingChart', 'Failed to load aging.'),
      stuck: () => {
        if (window.jQuery && stuckTable) {
          stuckTable.clear().destroy();
          stuckTable = null;
        }
        if (stuckBody) {
          stuckBody.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Failed to load stuck cases.</td></tr>';
        }
      },
      csat: () => {
        if (csatAvgScore) csatAvgScore.textContent = '--';
        if (csatAvgLabel) csatAvgLabel.textContent = 'Failed to load';
        if (csatResponseRate) csatResponseRate.textContent = '--';
        if (csatResponseLabel) csatResponseLabel.textContent = 'Failed to load';
      }
    };

    const runSection = (section) => {
      const params = new URLSearchParams(baseParams.toString());
      params.set('section', section);
      return fetch(`${apiReports}?${params.toString()}`)
        .then((res) => res.json().then((body) => ({ ok: res.ok, body })))
        .then(({ ok, body }) => {
          if (mySeq !== requestSeq) return;
          if (!ok) throw new Error(body?.error || `Unable to load ${section}`);
          cached = Object.assign(cached || {}, body || {});
          renderAll();
        })
        .catch(() => {
          if (mySeq !== requestSeq) return;
          if (sectionErrors[section]) sectionErrors[section]();
        });
    };

    const maxConcurrency = 2;
    let active = 0;
    let idx = 0;
    let completed = 0;
    const total = sections.length;

    return new Promise((resolve) => {
      const launchNext = () => {
        if (completed >= total) {
          resolve();
          return;
        }
        while (active < maxConcurrency && idx < total) {
          const section = sections[idx++];
          active += 1;
          runSection(section).finally(() => {
            active -= 1;
            completed += 1;
            launchNext();
          });
        }
      };
      launchNext();
    }).finally(() => {
        if (mySeq !== requestSeq) return;
        if (btnApply && btnApply.dataset.originalHtml) {
          btnApply.innerHTML = btnApply.dataset.originalHtml;
          btnApply.disabled = false;
          delete btnApply.dataset.originalHtml;
        }
      });
  }

  function init() {
    const defaultRange = getDefaultRange();
    if (dateInput && typeof flatpickr !== 'undefined') {
      picker = flatpickr(dateInput, {
        mode: 'range',
        dateFormat: 'Y-m-d',
        defaultDate: [formatDate(defaultRange[0]), formatDate(defaultRange[1])],
      });
    }
    loadCategories();
    applyDefaultChartTypes();
    fetchReports();

    if (btnApply) btnApply.addEventListener('click', fetchReports);
    if (categorySelect) {
      categorySelect.addEventListener('change', fetchReports);
    }
    if (chartTypeDefault) {
      chartTypeDefault.addEventListener('change', () => {
        syncChartTypesFromDefault();
        renderAll();
      });
    }
    if (btnReset) {
      btnReset.addEventListener('click', () => {
        if (picker) {
          const [s, e] = getDefaultRange();
          picker.setDate([s, e], true);
        }
        if (chartTypeDefault) chartTypeDefault.value = 'bar';
        applyDefaultChartTypes();
        if (categorySelect) categorySelect.value = '';
        fetchReports();
      });
    }
    chartTypeSelectors.forEach((sel) => {
      sel.addEventListener('change', renderAll);
    });
  }

  function loadCategories() {
    if (!categorySelect) return;
    // If server already rendered options, skip fetch
    if (categorySelect.options && categorySelect.options.length > 1) return;
    const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
    const url = apiMap.caseCategories || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/case-categories/index');
    fetch(url)
      .then((r) => r.json())
      .then((data) => {
        const list = Array.isArray(data?.data) ? data.data : Array.isArray(data) ? data : [];
        list.forEach((cat) => {
          const opt = document.createElement('option');
          opt.value = cat.id || '';
          opt.textContent = cat.name || 'Category';
          categorySelect.appendChild(opt);
        });
      })
      .catch(() => {
        // ignore load failures
      });
  }

  document.addEventListener('DOMContentLoaded', init);
})();
