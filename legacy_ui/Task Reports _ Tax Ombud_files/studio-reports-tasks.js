(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiReports = apiMap.tasksReports || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/tasks/reports');

  const dateInput = document.getElementById('taskReportDateRange');
  const chartTypeDefault = document.getElementById('taskReportChartType');
  const btnApply = document.getElementById('taskReportApply');
  const btnReset = document.getElementById('taskReportReset');
  const rangeLabel = document.getElementById('taskReportRangeLabel');
  const chartTypeSelectors = Array.from(document.querySelectorAll('.task-chart-type'));
  const categorySelect = document.getElementById('taskReportCategory');

  const chartRefs = {};
  let picker = null;
  let cached = {};
  let requestSeq = 0;

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
    chartRefs[id] = new ApexCharts(el, options);
    chartRefs[id].render();
  }

  function setChartLoading(id) {
    destroyChart(id);
    const el = document.getElementById(id);
    if (el) el.innerHTML = '<div class="text-center text-muted py-5">Computing values...</div>';
  }

  function setChartError(id, message) {
    destroyChart(id);
    const el = document.getElementById(id);
    if (el) el.innerHTML = `<div class="text-center text-danger py-5">${message || 'Failed to load'}</div>`;
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

  function renderTotalsDonut(chartId, totals, type = 'donut') {
    const colors = getChartColorsArray(chartId);
    const labels = (totals || []).map((t) => t.name || 'Unknown');
    const series = (totals || []).map((t) => Number(t.cnt || 0));
    const data = series.length ? series : [0];
    const opts = {
      series: data,
      labels: labels.length ? labels : ['No data'],
      chart: { type: type === 'pie' ? 'pie' : 'donut', height: 320 },
      colors: colors || undefined,
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
    };
    renderChart(chartId, opts);
  }

  function renderAssignee(chartId, totals, type = 'bar') {
    const colors = getChartColorsArray(chartId);
    const labels = (totals || []).map((t) => t.name || 'Unassigned');
    const series = (totals || []).map((t) => Number(t.cnt || 0));
    const opts = {
      series: [{ name: 'Tasks', data: series.length ? series : [0] }],
      chart: { type: 'bar', height: 320, toolbar: { show: false } },
      plotOptions: { bar: { horizontal: type === 'horizontal', columnWidth: '55%' } },
      colors: colors || undefined,
      dataLabels: { enabled: false },
      xaxis: { categories: labels.length ? labels : ['No data'] },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      legend: { show: false },
    };
    renderChart(chartId, opts);
  }

  function renderAging(chartId, aging) {
    const colors = getChartColorsArray(chartId);
    const labels = aging.map((a) => a.bucket || 'Unknown');
    const series = aging.map((a) => Number(a.cnt || 0));
    const opts = {
      series: [{ name: 'Open tasks', data: series.length ? series : [0] }],
      chart: { type: 'bar', height: 300, toolbar: { show: false } },
      plotOptions: { bar: { distributed: true } },
      colors: colors || undefined,
      dataLabels: { enabled: false },
      xaxis: { categories: labels.length ? labels : ['No data'] },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
    };
    renderChart(chartId, opts);
  }

  function syncChartTypesFromDefault() {
    if (!chartTypeDefault) return;
    const def = chartTypeDefault.value;
    chartTypeSelectors.forEach((sel) => {
      if (!sel) return;
      const hasOption = Array.from(sel.options || []).some((opt) => opt.value === def);
      if (hasOption) sel.value = def;
    });
  }

  function renderAll() {
    const statusType = (document.querySelector('[data-chart=\"taskStatusChart\"]') || {}).value || 'bar';
    const priorityType = (document.querySelector('[data-chart=\"taskPriorityChart\"]') || {}).value || 'bar';
    const statusTotalsType = (document.querySelector('[data-chart=\"taskStatusTotalsChart\"]') || {}).value || 'donut';
    const priorityTotalsType = (document.querySelector('[data-chart=\"taskPriorityTotalsChart\"]') || {}).value || 'donut';
    const assigneeType = (document.querySelector('[data-chart=\"taskAssigneeChart\"]') || {}).value || 'bar';

    if (Object.prototype.hasOwnProperty.call(cached, 'status_over_time')) {
      const labels = cached.labels || [];
      renderSeriesChart('taskStatusChart', labels, cached.status_over_time || [], statusType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'priority_over_time')) {
      const labels = cached.labels || [];
      renderSeriesChart('taskPriorityChart', labels, cached.priority_over_time || [], priorityType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'status_totals')) {
      renderTotalsDonut('taskStatusTotalsChart', cached.status_totals || [], statusTotalsType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'priority_totals')) {
      renderTotalsDonut('taskPriorityTotalsChart', cached.priority_totals || [], priorityTotalsType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'assignee_totals')) {
      renderAssignee('taskAssigneeChart', cached.assignee_totals || [], assigneeType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'aging_open')) {
      renderAging('taskAgingChart', cached.aging_open || []);
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
    setChartLoading('taskStatusChart');
    setChartLoading('taskPriorityChart');
    setChartLoading('taskStatusTotalsChart');
    setChartLoading('taskPriorityTotalsChart');
    setChartLoading('taskAssigneeChart');
    setChartLoading('taskAgingChart');
    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Computing values...';
    }

    const sections = ['status', 'priority', 'assignee', 'aging'];
    const sectionErrors = {
      status: () => {
        setChartError('taskStatusChart', 'Failed to load status trend.');
        setChartError('taskStatusTotalsChart', 'Failed to load status totals.');
      },
      priority: () => {
        setChartError('taskPriorityChart', 'Failed to load priority trend.');
        setChartError('taskPriorityTotalsChart', 'Failed to load priority totals.');
      },
      assignee: () => setChartError('taskAssigneeChart', 'Failed to load assignees.'),
      aging: () => setChartError('taskAgingChart', 'Failed to load aging.'),
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
    syncChartTypesFromDefault();
    loadCategories();
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
        syncChartTypesFromDefault();
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
    // If server rendered options, skip fetch
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
