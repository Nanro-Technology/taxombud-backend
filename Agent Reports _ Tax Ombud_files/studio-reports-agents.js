(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiReports = apiMap.agentsReports || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/agents/reports');
  const apiAgents = apiMap.agentsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/agents/index');

  const dateInput = document.getElementById('agentReportDateRange');
  const btnApply = document.getElementById('agentReportApply');
  const btnReset = document.getElementById('agentReportReset');
  const rangeLabel = document.getElementById('agentReportRangeLabel');
  const categorySelect = document.getElementById('agentReportCategory');
  const agentsSelect = document.getElementById('agentReportAgents');
  const dailyBody = document.getElementById('agentDailyCaseBody');

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
    start.setDate(end.getDate() - 6);
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
    // Clear any manual loading/error placeholders before mounting chart.
    el.innerHTML = '';
    chartRefs[id] = new ApexCharts(el, options);
    chartRefs[id].render();
  }

  function renderStackedBar(chartId, labels, openSeries, closedSeries, colors, opts = {}) {
    const safeLabels = labels.length ? labels : ['No data'];
    const series = [
      { name: openSeries.name, data: openSeries.data.length ? openSeries.data : [0] },
      { name: closedSeries.name, data: closedSeries.data.length ? closedSeries.data : [0] },
    ];
    renderChart(chartId, {
      series,
      chart: { type: 'bar', height: opts.height || 320, stacked: true, toolbar: { show: false } },
      plotOptions: { bar: { horizontal: opts.horizontal || false, columnWidth: '55%' } },
      dataLabels: { enabled: false },
      colors: colors || undefined,
      xaxis: { categories: safeLabels },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      legend: { position: 'top' },
      tooltip: { shared: true, intersect: false },
    });
  }

  function renderDirectionStack(chartId, labels, inbound, outbound, colors) {
    const safeLabels = labels.length ? labels : ['No data'];
    const series = [
      { name: 'Inbound', data: inbound.length ? inbound : [0] },
      { name: 'Outbound', data: outbound.length ? outbound : [0] },
    ];
    renderChart(chartId, {
      series,
      chart: { type: 'bar', height: 320, stacked: true, toolbar: { show: false } },
      plotOptions: { bar: { horizontal: false, columnWidth: '55%' } },
      dataLabels: { enabled: false },
      colors: colors || undefined,
      xaxis: { categories: safeLabels },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      legend: { position: 'top' },
      tooltip: { shared: true, intersect: false },
    });
  }

  function renderAging(chartId, payload) {
    const colors = getChartColorsArray(chartId);
    const labels = [];
    const seriesBuckets = ['0-2 days', '3-7 days', '8-14 days', '15+ days'];
    const dataPerBucket = seriesBuckets.map(() => []);
    (payload || []).forEach((row) => {
      labels.push(row.name || 'Agent');
      seriesBuckets.forEach((bucket, idx) => {
        dataPerBucket[idx].push(Number(row.buckets?.[bucket] || 0));
      });
    });
    const safeLabels = labels.length ? labels : ['No data'];
    const series = seriesBuckets.map((bucket, idx) => ({
      name: bucket,
      data: dataPerBucket[idx].length ? dataPerBucket[idx] : [0],
    }));
    renderChart(chartId, {
      series,
      chart: { type: 'bar', height: 320, stacked: true, toolbar: { show: false } },
      plotOptions: { bar: { horizontal: true, barHeight: '60%' } },
      dataLabels: { enabled: false },
      colors: colors || undefined,
      xaxis: { categories: safeLabels },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() } },
      legend: { position: 'top' },
      tooltip: { shared: true, intersect: false },
    });
  }

  function renderCasesSection() {
    const casesData = cached.cases_per_agent || [];
    const caseLabels = casesData.map((r) => r.name || 'Agent');
    renderStackedBar(
      'agentCasesChart',
      caseLabels,
      { name: 'Open', data: casesData.map((r) => Number(r.open || 0)) },
      { name: 'Closed', data: casesData.map((r) => Number(r.closed || 0)) },
      getChartColorsArray('agentCasesChart'),
    );
  }

  function renderTasksSection() {
    const tasksData = cached.tasks_per_agent || [];
    const taskLabels = tasksData.map((r) => r.name || 'Agent');
    renderStackedBar(
      'agentTasksChart',
      taskLabels,
      { name: 'Open', data: tasksData.map((r) => Number(r.open || 0)) },
      { name: 'Closed', data: tasksData.map((r) => Number(r.closed || 0)) },
      getChartColorsArray('agentTasksChart'),
    );
  }

  function renderCallsSection() {
    const callsData = cached.calls_per_agent || [];
    const callLabels = callsData.map((r) => r.name || 'Agent');
    renderDirectionStack(
      'agentCallsChart',
      callLabels,
      callsData.map((r) => Number(r.inbound || 0)),
      callsData.map((r) => Number(r.outbound || 0)),
      getChartColorsArray('agentCallsChart'),
    );
  }

  function renderAgingSection() {
    const agingData = cached.cases_aging_per_agent || [];
    renderAging('agentAgingChart', agingData);
  }

  function renderDailySection() {
    const dailyData = cached.daily_case_performance || [];
    if (dailyBody) {
      if (!dailyData.length) {
        dailyBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No data.</td></tr>';
      } else {
        const frag = document.createDocumentFragment();
        dailyData.forEach((row) => {
          const tr = document.createElement('tr');
          tr.innerHTML = `
            <td class="fw-medium">${row.name || 'Agent'}</td>
            <td>${Number(row.assigned || 0)}</td>
            <td>${Number(row.closed || 0)}</td>
            <td>${Number(row.pending || 0)}</td>
          `;
          frag.appendChild(tr);
        });
        dailyBody.innerHTML = '';
        dailyBody.appendChild(frag);
      }
    }
  }

  function setChartLoading(chartId) {
    destroyChart(chartId);
    const el = document.getElementById(chartId);
    if (el) {
      el.innerHTML = '<div class="text-center text-muted py-5">Computing values...</div>';
    }
  }

  function setChartError(chartId, message) {
    destroyChart(chartId);
    const el = document.getElementById(chartId);
    if (el) {
      el.innerHTML = `<div class="text-center text-danger py-5">${message || 'Failed to load'}</div>`;
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
    if (categorySelect && categorySelect.value) {
      const cat = (categorySelect.value || '').trim();
      if (cat !== '') baseParams.set('domain_id', cat);
    }
    if (agentsSelect) {
      const selected = Array.from(agentsSelect.selectedOptions || []).map((opt) => opt.value).filter((v) => v);
      if (selected.length) {
        baseParams.set('agent_ids', selected.join(','));
      }
    }
    updateRangeLabel(start, end);
    cached = {};
    if (dailyBody) {
      dailyBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Computing values...</td></tr>';
    }
    setChartLoading('agentCasesChart');
    setChartLoading('agentTasksChart');
    setChartLoading('agentCallsChart');
    setChartLoading('agentAgingChart');
    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Computing values...';
    }
    const sections = ['daily', 'cases', 'tasks', 'calls', 'aging'];
    const sectionRenderers = {
      daily: renderDailySection,
      cases: renderCasesSection,
      tasks: renderTasksSection,
      calls: renderCallsSection,
      aging: renderAgingSection,
    };
    const sectionErrors = {
      daily: () => {
        if (dailyBody) {
          dailyBody.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Failed to load daily data.</td></tr>';
        }
      },
      cases: () => setChartError('agentCasesChart', 'Failed to load cases.'),
      tasks: () => setChartError('agentTasksChart', 'Failed to load tasks.'),
      calls: () => setChartError('agentCallsChart', 'Failed to load calls.'),
      aging: () => setChartError('agentAgingChart', 'Failed to load aging.'),
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
          if (sectionRenderers[section]) sectionRenderers[section]();
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
    })
      .finally(() => {
        if (mySeq !== requestSeq) return;
        if (btnApply && btnApply.dataset.originalHtml) {
          btnApply.innerHTML = btnApply.dataset.originalHtml;
          btnApply.disabled = false;
          delete btnApply.dataset.originalHtml;
        }
      });
  }

  function loadAgents() {
    if (!agentsSelect) return;
    const url = apiAgents;
    if (typeof window.initEnhancedSelect === 'function') {
      window.initEnhancedSelect(agentsSelect);
    }
    fetch(url)
      .then((r) => r.json())
      .then((data) => {
        const list = Array.isArray(data?.data) ? data.data : Array.isArray(data) ? data : [];
        list.forEach((ag) => {
          const opt = document.createElement('option');
          opt.value = ag.id || '';
          opt.textContent = ag.display_name || ag.name || `Agent #${ag.id || ''}`;
          agentsSelect.appendChild(opt);
        });
        if (typeof window.refreshEnhancedSelect === 'function') {
          window.refreshEnhancedSelect(agentsSelect);
        }
      })
      .catch(() => {
        // ignore
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
    loadAgents();
    fetchReports();

    if (btnApply) btnApply.addEventListener('click', fetchReports);
    if (btnReset) {
      btnReset.addEventListener('click', () => {
        if (picker) {
          const [s, e] = getDefaultRange();
          picker.setDate([s, e], true);
        }
        if (categorySelect) categorySelect.value = '';
        if (agentsSelect) {
          Array.from(agentsSelect.options || []).forEach((opt) => opt.selected = false);
        }
        fetchReports();
      });
    }
    if (categorySelect) categorySelect.addEventListener('change', fetchReports);
    if (agentsSelect) agentsSelect.addEventListener('change', fetchReports);
  }

  document.addEventListener('DOMContentLoaded', init);
})();
