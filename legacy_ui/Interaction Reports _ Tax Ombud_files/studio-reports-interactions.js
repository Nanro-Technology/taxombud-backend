(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiReports = apiMap.interactionsReports
    || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/interactions/reports');

  const dateInput        = document.getElementById('intReportDateRange');
  const chartTypeDefault = document.getElementById('intReportChartType');
  const btnApply         = document.getElementById('intReportApply');
  const btnReset         = document.getElementById('intReportReset');
  const rangeLabel       = document.getElementById('intReportRangeLabel');
  const typeFilter       = document.getElementById('intReportType');
  const channelFilter    = document.getElementById('intReportChannel');
  const chartTypeSelectors = Array.from(document.querySelectorAll('.int-chart-type'));

  const kpiEls = {
    total:    document.getElementById('intKpiTotal'),
    inbound:  document.getElementById('intKpiInbound'),
    outbound: document.getElementById('intKpiOutbound'),
    followup: document.getElementById('intKpiFollowup'),
  };

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
    return { start: formatDate(start), end: formatDate(end || start) };
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
    } catch (err) { return null; }
  }

  function destroyChart(id) {
    if (chartRefs[id]) { chartRefs[id].destroy(); delete chartRefs[id]; }
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
      chart: { type, height: extra.height || 320, stacked: type === 'bar' && extra.stacked === true, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      dataLabels: { enabled: false },
      xaxis: { categories: labels || [] },
      colors: colors || undefined,
      legend: { position: 'top' },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      ...extra
    };
  }
  function renderSeriesChart(chartId, labels, series, type, opts = {}) {
    const colors = getChartColorsArray(chartId);
    const safeSeries = (series && series.length) ? series : [{ name: 'No data', data: labels.map(() => 0) }];
    renderChart(chartId, baseOptions(type, labels, safeSeries, colors, { height: 340, ...opts }));
  }
  function renderTotalsDonut(chartId, totals, type = 'donut') {
    const colors = getChartColorsArray(chartId);
    const labels = (totals || []).map((t) => t.name || 'Unknown');
    const series = (totals || []).map((t) => Number(t.cnt || 0));
    const data = series.length ? series : [0];
    renderChart(chartId, {
      series: data,
      labels: labels.length ? labels : ['No data'],
      chart: { type: type === 'pie' ? 'pie' : 'donut', height: 320 },
      colors: colors || undefined,
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
    });
  }
  function renderBar(chartId, totals, type = 'bar') {
    const colors = getChartColorsArray(chartId);
    const labels = (totals || []).map((t) => t.name || 'Unassigned');
    const series = (totals || []).map((t) => Number(t.cnt || 0));
    renderChart(chartId, {
      series: [{ name: 'Interactions', data: series.length ? series : [0] }],
      chart: { type: 'bar', height: 320, toolbar: { show: false } },
      plotOptions: { bar: { horizontal: type === 'horizontal', columnWidth: '55%' } },
      colors: colors || undefined,
      dataLabels: { enabled: false },
      xaxis: { categories: labels.length ? labels : ['No data'] },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      legend: { show: false },
    });
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
    const dirType      = (document.querySelector('[data-chart=\"intDirectionChart\"]') || {}).value || 'area';
    const typeType     = (document.querySelector('[data-chart=\"intTypeChart\"]')      || {}).value || 'donut';
    const channelType  = (document.querySelector('[data-chart=\"intChannelChart\"]')   || {}).value || 'donut';
    const agentType    = (document.querySelector('[data-chart=\"intAgentChart\"]')     || {}).value || 'bar';
    const outcomeType  = (document.querySelector('[data-chart=\"intOutcomeChart\"]')   || {}).value || 'donut';
    const entityType   = (document.querySelector('[data-chart=\"intEntityChart\"]')    || {}).value || 'bar';
    const followupType = (document.querySelector('[data-chart=\"intFollowupChart\"]')  || {}).value || 'bar';

    if (Object.prototype.hasOwnProperty.call(cached, 'totals')) {
      const t = cached.totals || {};
      if (kpiEls.total)    kpiEls.total.textContent    = String(t.total || 0);
      if (kpiEls.inbound)  kpiEls.inbound.textContent  = String(t.inbound || 0);
      if (kpiEls.outbound) kpiEls.outbound.textContent = String(t.outbound || 0);
      if (kpiEls.followup) kpiEls.followup.textContent = String(t.follow_up_pending || 0);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'direction_over_time')) {
      renderSeriesChart('intDirectionChart', cached.labels || [], cached.direction_over_time || [], dirType, { stacked: dirType === 'bar' });
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'type_totals')) {
      renderTotalsDonut('intTypeChart', cached.type_totals || [], typeType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'channel_totals')) {
      renderTotalsDonut('intChannelChart', cached.channel_totals || [], channelType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'agent_totals')) {
      renderBar('intAgentChart', cached.agent_totals || [], agentType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'outcome_totals')) {
      renderTotalsDonut('intOutcomeChart', cached.outcome_totals || [], outcomeType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'entity_totals')) {
      renderBar('intEntityChart', cached.entity_totals || [], entityType);
    }
    if (Object.prototype.hasOwnProperty.call(cached, 'followup_by_agent')) {
      renderBar('intFollowupChart', cached.followup_by_agent || [], followupType);
    }
  }

  function updateRangeLabel(start, end) {
    if (rangeLabel) rangeLabel.textContent = start && end ? `Showing ${start} to ${end}` : '';
  }

  function fetchReports(e) {
    if (e) e.preventDefault();
    const mySeq = ++requestSeq;
    const { start, end } = getRangeParams();
    const baseParams = new URLSearchParams({ start_date: start, end_date: end });
    if (typeFilter && typeFilter.value)       baseParams.set('interaction_type', typeFilter.value);
    if (channelFilter && channelFilter.value) baseParams.set('channel', channelFilter.value);
    updateRangeLabel(start, end);
    cached = {};
    setChartLoading('intDirectionChart');
    setChartLoading('intTypeChart');
    setChartLoading('intChannelChart');
    setChartLoading('intAgentChart');
    setChartLoading('intOutcomeChart');
    setChartLoading('intEntityChart');
    setChartLoading('intFollowupChart');
    Object.values(kpiEls).forEach((el) => { if (el) el.textContent = '...'; });

    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Computing values...';
    }

    const sections = ['totals', 'type', 'channel', 'direction', 'agents', 'outcome', 'entity', 'followup'];
    const sectionErrors = {
      totals:    () => Object.values(kpiEls).forEach((el) => { if (el) el.textContent = '0'; }),
      type:      () => setChartError('intTypeChart', 'Failed to load type breakdown.'),
      channel:   () => setChartError('intChannelChart', 'Failed to load channel breakdown.'),
      direction: () => setChartError('intDirectionChart', 'Failed to load direction trend.'),
      agents:    () => setChartError('intAgentChart', 'Failed to load agents.'),
      outcome:   () => setChartError('intOutcomeChart', 'Failed to load outcomes.'),
      entity:    () => setChartError('intEntityChart', 'Failed to load entity breakdown.'),
      followup:  () => setChartError('intFollowupChart', 'Failed to load follow-up backlog.'),
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
    let active = 0, idx = 0, completed = 0;
    const total = sections.length;
    return new Promise((resolve) => {
      const launchNext = () => {
        if (completed >= total) { resolve(); return; }
        while (active < maxConcurrency && idx < total) {
          const section = sections[idx++];
          active += 1;
          runSection(section).finally(() => { active -= 1; completed += 1; launchNext(); });
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
    fetchReports();
    if (btnApply)     btnApply.addEventListener('click', fetchReports);
    if (typeFilter)   typeFilter.addEventListener('change', fetchReports);
    if (channelFilter) channelFilter.addEventListener('change', fetchReports);
    if (chartTypeDefault) chartTypeDefault.addEventListener('change', () => { syncChartTypesFromDefault(); renderAll(); });
    if (btnReset) {
      btnReset.addEventListener('click', () => {
        if (picker) { const [s, e] = getDefaultRange(); picker.setDate([s, e], true); }
        if (chartTypeDefault) chartTypeDefault.value = 'bar';
        if (typeFilter) typeFilter.value = '';
        if (channelFilter) channelFilter.value = '';
        syncChartTypesFromDefault();
        fetchReports();
      });
    }
    chartTypeSelectors.forEach((sel) => sel.addEventListener('change', renderAll));
  }

  document.addEventListener('DOMContentLoaded', init);
})();
