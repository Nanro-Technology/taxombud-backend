(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiReports = apiMap.omniReports || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/omni/reports');

  const dateInput = document.getElementById('reportDateRange');
  const chartTypeDefault = document.getElementById('reportChartType');
  const btnApply = document.getElementById('reportApply');
  const btnReset = document.getElementById('reportReset');
  const rangeLabel = document.getElementById('reportRangeLabel');
  const chartTypeSelectors = Array.from(document.querySelectorAll('.chart-type'));

  const chartRefs = {};
  let picker = null;
  let cached = {};
  let requestSeq = 0;

  function syncChartTypesFromDefault() {
    if (!chartTypeDefault) return;
    const def = chartTypeDefault.value;
    chartTypeSelectors.forEach((sel) => {
      if (!sel) return;
      const has = Array.from(sel.options || []).some(o => o.value === def);
      if (has) sel.value = def;
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
    // Fallbacks for colors and series
    const palette = options.colors && options.colors.length ? options.colors : ['#405189','#0ab39c','#f7b84b','#f06548','#299cdb','#6f42c1'];
    options.colors = palette;
    options.fill = options.fill || { colors: palette };
    options.markers = options.markers || { colors: palette };
    if (!options.series || !options.series.length) {
      options.series = [{ name: 'No data', data: [0] }];
    }
    if (!options.chart) {
      options.chart = { type: 'bar', height: 320, toolbar: { show: false } };
    } else if (!options.chart.type) {
      options.chart.type = 'bar';
    }
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
    const safeSeries = Array.isArray(series) && series.length ? series : [{ name: 'No data', data: (labels && labels.length ? labels : ['No data']).map(() => 0) }];
    return {
      series: safeSeries,
      chart: { type, height: extra.height || 320, stacked: type === 'bar', toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      dataLabels: { enabled: false },
      xaxis: { categories: labels || [] },
      colors: (colors && colors.length) ? colors : ['#405189','#0ab39c','#f7b84b','#f06548','#299cdb','#6f42c1'],
      fill: { colors: (colors && colors.length) ? colors : ['#405189','#0ab39c','#f7b84b','#f06548','#299cdb','#6f42c1'] },
      markers: { colors: (colors && colors.length) ? colors : ['#405189','#0ab39c','#f7b84b','#f06548','#299cdb','#6f42c1'] },
      legend: { position: 'top' },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      ...extra
    };
  }

  function renderSeriesChart(chartId, labels, series, type) {
    const colors = getChartColorsArray(chartId);
    const safeLabels = (labels && labels.length) ? labels : ['No data'];
    const safeSeries = (series && series.length)
      ? series.map((s) => ({
          name: s.name || 'Series',
          data: (Array.isArray(s.data) && s.data.length) ? s.data : [0]
        }))
      : [{ name: 'No data', data: [0] }];
    const opts = baseOptions(type, safeLabels, safeSeries, colors, { height: 340 });
    renderChart(chartId, opts);
  }

  function renderDonut(chartId, rows, type = 'donut') {
    const colors = getChartColorsArray(chartId);
    const labels = rows.map((r) => r.name || r.channel || 'Unknown');
    const series = rows.map((r) => Number(r.cnt || 0));
    const data = series.length ? series : [0];
    const opts = {
      series: data,
      labels: labels.length ? labels : ['No data'],
      chart: { type: type === 'pie' ? 'pie' : 'donut', height: 340 },
      colors: (colors && colors.length) ? colors : ['#405189','#0ab39c','#f7b84b','#f06548','#299cdb','#6f42c1'],
      fill: { colors: (colors && colors.length) ? colors : ['#405189','#0ab39c','#f7b84b','#f06548','#299cdb','#6f42c1'] },
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
    };
    renderChart(chartId, opts);
  }

  function renderLinked(chartId, rows, type) {
    // Sum across channels to global linked/unlinked donut
    const linked = rows.reduce((acc, r) => acc + Number(r.linked || 0), 0);
    const unlinked = rows.reduce((acc, r) => acc + Number(r.unlinked || 0), 0);
    const totals = [
      { name: 'Linked', cnt: linked },
      { name: 'Unlinked', cnt: unlinked },
    ];
    renderDonut(chartId, totals, type);
  }

  function renderBuckets(chartId, buckets, type) {
    const labels = buckets.map((b) => b.bucket || 'Unknown');
    const data = buckets.map((b) => Number(b.cnt || 0));
    const series = [{ name: 'Conversations', data: data.length ? data : [0] }];
    renderSeriesChart(chartId, labels.length ? labels : ['No data'], series, type);
  }

  function renderAll(data) {
    if (!data) return;
    const defType = chartTypeDefault ? chartTypeDefault.value : 'bar';

    if (Object.prototype.hasOwnProperty.call(data, 'messages_by_channel')) {
      const msgTypeSel = document.querySelector('[data-chart=\"msgChannelChart\"]');
      const msgType = msgTypeSel ? msgTypeSel.value : defType;
      const labels = (data.messages_by_channel || []).map(r => r.name || 'Unknown');
      const series = [{ name: 'Messages', data: (data.messages_by_channel || []).map(r => Number(r.cnt || 0)) }];
      renderSeriesChart('msgChannelChart', labels, series, msgType);
    }

    if (Object.prototype.hasOwnProperty.call(data, 'linked_vs_unlinked')) {
      const linkTypeSel = document.querySelector('[data-chart=\"linkedChart\"]');
      const linkType = linkTypeSel ? linkTypeSel.value : 'donut';
      renderLinked('linkedChart', data.linked_vs_unlinked || [], linkType);
    }

    if (Object.prototype.hasOwnProperty.call(data, 'response_time_buckets')) {
      const respTypeSel = document.querySelector('[data-chart=\"responseChart\"]');
      const respType = respTypeSel ? respTypeSel.value : defType;
      renderBuckets('responseChart', data.response_time_buckets || [], respType);
    }

    if (Object.prototype.hasOwnProperty.call(data, 'open_by_channel')) {
      const openTypeSel = document.querySelector('[data-chart=\"openChart\"]');
      const openType = openTypeSel ? openTypeSel.value : defType;
      const openLabels = (data.open_by_channel || []).map(r => r.channel || 'Unknown');
      const openSeries = [{ name: 'Open', data: (data.open_by_channel || []).map(r => Number(r.cnt || 0)) }];
      renderSeriesChart('openChart', openLabels, openSeries, openType);
    }
  }

  function loadReports() {
    const mySeq = ++requestSeq;
    const { start, end } = getRangeParams();
    const baseParams = new URLSearchParams({ start_date: start, end_date: end });
    if (rangeLabel) {
      rangeLabel.textContent = `Showing ${start} to ${end}`;
    }
    cached = {};
    setChartLoading('msgChannelChart');
    setChartLoading('linkedChart');
    setChartLoading('responseChart');
    setChartLoading('openChart');
    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Computing values...';
    }

    const sections = ['messages', 'linked', 'response', 'open'];
    const sectionErrors = {
      messages: () => setChartError('msgChannelChart', 'Failed to load messages.'),
      linked: () => setChartError('linkedChart', 'Failed to load linkage.'),
      response: () => setChartError('responseChart', 'Failed to load response time.'),
      open: () => setChartError('openChart', 'Failed to load open conversations.')
    };

    const runSection = (section) => {
      const params = new URLSearchParams(baseParams.toString());
      params.set('section', section);
      return fetch(`${apiReports}?${params.toString()}`, { credentials: 'same-origin' })
        .then((res) => res.json().catch(() => ({})).then((body) => ({ ok: res.ok, body })))
        .then(({ ok, body }) => {
          if (mySeq !== requestSeq) return;
          if (!ok) throw new Error(body?.error || `Failed to load ${section}`);
          cached = Object.assign(cached || {}, body || {});
          renderAll(cached);
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

  function resetFilters() {
    if (picker) picker.clear();
    syncChartTypesFromDefault();
    loadReports().catch(() => {});
  }

  function initPicker() {
    if (!dateInput || !window.flatpickr) return;
    picker = flatpickr(dateInput, {
      mode: 'range',
      dateFormat: 'Y-m-d',
      defaultDate: getDefaultRange().map(d => formatDate(d)),
    });
  }

  if (btnApply) {
    btnApply.addEventListener('click', (e) => {
      e.preventDefault();
      loadReports().catch(() => {});
    });
  }
  if (btnReset) {
    btnReset.addEventListener('click', (e) => {
      e.preventDefault();
      resetFilters();
    });
  }

  chartTypeSelectors.forEach((sel) => {
    sel.addEventListener('change', () => renderAll(cached));
  });

  initPicker();
  syncChartTypesFromDefault();
  loadReports().catch(() => {});
})();
