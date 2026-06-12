(function () {
  const apiUrl = 'api/modules/erp/reports';

  const dateInput = document.getElementById('reportDateRange');
  const chartTypeDefault = document.getElementById('reportChartType');
  const btnApply = document.getElementById('reportApply');
  const btnReset = document.getElementById('reportReset');
  const rangeLabel = document.getElementById('reportRangeLabel');
  const chartTypeSelectors = Array.from(document.querySelectorAll('.chart-type'));
  const defaultColors = ['#405189','#0ab39c','#f7b84b','#f06548','#299cdb','#6f42c1'];

  const chartRefs = {};
  let picker = null;
  let cached = {};
  let requestSeq = 0;

  /* ---- helpers borrowed from cases reports ---- */
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
    // Guarantee colors and series
    options.colors = options.colors && options.colors.length ? options.colors : defaultColors;
    if (!options.fill) options.fill = { colors: options.colors };
    if (!options.markers) options.markers = { colors: options.colors };
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

  function setTableBody(id, html) {
    const el = document.getElementById(id);
    if (el) el.innerHTML = html;
  }

  function baseOptions(type, labels, series, colors, extra = {}) {
    const safeSeries = Array.isArray(series) && series.length ? series : [{ name: 'No data', data: (labels && labels.length ? labels : ['No data']).map(() => 0) }];
    const opts = {
      series: safeSeries,
      chart: { type, height: extra.height || 320, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      dataLabels: { enabled: false },
      xaxis: { categories: labels || [] },
      legend: { position: 'bottom' },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      ...extra
    };
    opts.colors = (Array.isArray(colors) && colors.length) ? colors : defaultColors;
    opts.fill = opts.fill || { colors: opts.colors };
    opts.markers = opts.markers || { colors: opts.colors };
    return opts;
  }

  function renderSeriesChart(chartId, labels, series, type) {
    const colors = getChartColorsArray(chartId);
    const safeLabels = labels && labels.length ? labels : ['No data'];
    const safeSeries = (series && series.length) ? series : [{ name: 'No data', data: safeLabels.map(() => 0) }];
    const t = (type === 'line' || type === 'area' || type === 'bar') ? type : 'bar';
    const opts = baseOptions(
      t,
      safeLabels,
      safeSeries,
      colors,
      {
        height: 340,
        plotOptions: t === 'bar' ? { bar: { columnWidth: '50%' } } : undefined,
        fill: t === 'area' ? { type: 'solid', opacity: 0.15 } : undefined
      }
    );
    renderChart(chartId, opts);
  }

  function renderDonut(chartId, labels, series, type = 'donut') {
    const colors = getChartColorsArray(chartId);
    const safeLabels = labels && labels.length ? labels : ['No data'];
    const safeSeries = (series && series.length) ? series : [0];
    const opts = {
      series: safeSeries,
      labels: safeLabels,
      chart: { type: type === 'pie' ? 'pie' : 'donut', height: 340, toolbar: { show: false } },
      colors: (colors && colors.length) ? colors : defaultColors,
      fill: { colors: (colors && colors.length) ? colors : defaultColors },
      markers: { colors: (colors && colors.length) ? colors : defaultColors },
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
    };
    renderChart(chartId, opts);
  }

  /* ---- date helpers ---- */
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

  function applyDefaultChartTypes() {
    const def = chartTypeDefault ? chartTypeDefault.value : 'donut';
    chartTypeSelectors.forEach((sel) => {
      if (!sel) return;
      const has = Array.from(sel.options || []).some((opt) => opt.value === def);
      if (has) sel.value = def;
    });
  }

  /* ---- render pipeline ---- */
  function renderAll() {
    if (!cached) return;

    // Snapshot cards
    if (cached.inventory_status_counts) {
      const totalInv = cached.inventory_status_counts.reduce((s, r) => s + Number(r.total || 0), 0);
      const invTotalEl = document.getElementById('cardInvTotal');
      if (invTotalEl) invTotalEl.textContent = totalInv;
    }
    if (cached.ticket_status) {
      const openTotal = cached.ticket_status
        .filter((r) => String(r.status || '').toLowerCase() !== 'closed')
        .reduce((s, r) => s + Number(r.total || 0), 0);
      const tEl = document.getElementById('cardTicketOpen');
      if (tEl) tEl.textContent = openTotal;
    }
    if (cached.staff_status_counts) {
      const staffTotal = cached.staff_status_counts.reduce((s, r) => s + Number(r.total || 0), 0);
      const stEl = document.getElementById('cardStaffTotal');
      if (stEl) stEl.textContent = staffTotal;
    }
    // File requests not provided; leave placeholder at --

    // Inventory status
    if (cached.inventory_status_counts) {
      const labels = cached.inventory_status_counts.map((r) => (r.status || 'UNKNOWN').toUpperCase());
      const series = cached.inventory_status_counts.map((r) => Number(r.total || 0));
      const typeSel = document.querySelector('[data-chart="chartInvStatus"]');
      const ctype = typeSel ? typeSel.value : 'donut';
      if (ctype === 'bar') {
        renderSeriesChart('chartInvStatus', labels, [{ name: 'Items', data: series }], 'bar');
      } else {
        renderDonut('chartInvStatus', labels, series, ctype);
      }
    }

    // Top categories removed per request

    // Inventory by department
    if (cached.inventory_by_department) {
      const labels = cached.inventory_by_department.map((r) => r.department_name || 'Unassigned');
      const data = cached.inventory_by_department.map((r) => Number(r.total || 0));
      const typeSel = document.querySelector('[data-chart="chartInvDept"]');
      const ctype = typeSel ? typeSel.value : 'donut';
      if (ctype === 'bar') {
        renderSeriesChart('chartInvDept', labels, [{ name: 'Items', data }], 'bar');
      } else {
        renderDonut('chartInvDept', labels, data, ctype);
      }
      const table = document.getElementById('tableInvDept');
      if (table) {
        if (!data.length) {
          table.innerHTML = '<tr><td colspan="2" class="text-center text-muted">No data</td></tr>';
        } else {
          table.innerHTML = labels.map((lbl, idx) => `
            <tr>
              <td>${lbl}</td>
              <td class="text-end">${data[idx]}</td>
            </tr>
          `).join('');
        }
      }
    }

    // Tickets by department
    if (cached.ticket_department) {
      const labels = cached.ticket_department.map((r) => r.department_name || 'Unassigned');
      const data = cached.ticket_department.map((r) => Number(r.total || 0));
      const typeSel = document.querySelector('[data-chart="chartTicketDept"]');
      const ctype = typeSel ? typeSel.value : 'donut';
      if (ctype === 'bar') {
        renderSeriesChart('chartTicketDept', labels, [{ name: 'Tickets', data }], 'bar');
      } else {
        renderDonut('chartTicketDept', labels, data, ctype);
      }
      const table = document.getElementById('tableTicketDept');
      if (table) {
        if (!data.length) {
          table.innerHTML = '<tr><td colspan="2" class="text-center text-muted">No data</td></tr>';
        } else {
          table.innerHTML = labels.map((lbl, idx) => `
            <tr>
              <td>${lbl}</td>
              <td class="text-end">${data[idx]}</td>
            </tr>
          `).join('');
        }
      }
    }

    // Tickets by status
    if (cached.ticket_status) {
      const labels = cached.ticket_status.map((r) => (r.status || 'UNKNOWN').toUpperCase());
      const data = cached.ticket_status.map((r) => Number(r.total || 0));
      const typeSel = document.querySelector('[data-chart="chartTicketStatus"]');
      const ctype = typeSel ? typeSel.value : 'donut';
      if (ctype === 'bar') {
        renderSeriesChart('chartTicketStatus', labels, [{ name: 'Tickets', data }], 'bar');
      } else {
        renderDonut('chartTicketStatus', labels, data, ctype);
      }
    }

    // HR headcount by department
    if (cached.staff_headcount_by_department) {
      const top = cached.staff_headcount_by_department.slice(0, 10);
      const labels = top.map((r) => r.department_name || 'Unassigned');
      const data = top.map((r) => Number(r.total || 0));
      const typeSel = document.querySelector('[data-chart="chartHrDept"]');
      const ctype = typeSel ? typeSel.value : 'donut';
      if (ctype === 'bar') {
        renderSeriesChart('chartHrDept', labels, [{ name: 'Headcount', data }], 'bar');
      } else {
        renderDonut('chartHrDept', labels, data, ctype);
      }
    }

    // HR status breakdown
    if (cached.staff_status_counts) {
      const labels = cached.staff_status_counts.map((r) => (r.staff_status || 'UNKNOWN').toUpperCase());
      const data = cached.staff_status_counts.map((r) => Number(r.total || 0));
      const typeSel = document.querySelector('[data-chart="chartHrStatus"]');
      const ctype = typeSel ? typeSel.value : 'donut';
      if (ctype === 'bar') {
        renderSeriesChart('chartHrStatus', labels, [{ name: 'Staff', data }], 'bar');
      } else {
        renderDonut('chartHrStatus', labels, data, ctype);
      }
    }
  }

  async function load() {
    const mySeq = ++requestSeq;
    const { start, end } = getRangeParams();
    cached = {};
    const cardInvTotal = document.getElementById('cardInvTotal');
    const cardTicketOpen = document.getElementById('cardTicketOpen');
    const cardStaffTotal = document.getElementById('cardStaffTotal');
    if (cardInvTotal) cardInvTotal.textContent = '--';
    if (cardTicketOpen) cardTicketOpen.textContent = '--';
    if (cardStaffTotal) cardStaffTotal.textContent = '--';
    setChartLoading('chartInvStatus');
    setChartLoading('chartInvDept');
    setChartLoading('chartTicketDept');
    setChartLoading('chartTicketStatus');
    setChartLoading('chartHrDept');
    setChartLoading('chartHrStatus');
    setTableBody('tableInvDept', '<tr><td colspan="2" class="text-center text-muted">Computing values...</td></tr>');
    setTableBody('tableTicketDept', '<tr><td colspan="2" class="text-center text-muted">Computing values...</td></tr>');
    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Computing values...';
    }
    try {
      if (rangeLabel) rangeLabel.textContent = `${start} → ${end}`;

      const sections = ['inventory', 'hr', 'tickets'];
      const sectionErrors = {
        inventory: () => {
          setChartError('chartInvStatus', 'Failed to load inventory status.');
          setChartError('chartInvDept', 'Failed to load inventory departments.');
          setTableBody('tableInvDept', '<tr><td colspan="2" class="text-center text-danger">Failed to load inventory table.</td></tr>');
          if (cardInvTotal) cardInvTotal.textContent = '--';
        },
        hr: () => {
          setChartError('chartHrDept', 'Failed to load HR department chart.');
          setChartError('chartHrStatus', 'Failed to load HR status chart.');
          if (cardStaffTotal) cardStaffTotal.textContent = '--';
        },
        tickets: () => {
          setChartError('chartTicketDept', 'Failed to load ticket department chart.');
          setChartError('chartTicketStatus', 'Failed to load ticket status chart.');
          setTableBody('tableTicketDept', '<tr><td colspan="2" class="text-center text-danger">Failed to load ticket table.</td></tr>');
          if (cardTicketOpen) cardTicketOpen.textContent = '--';
        }
      };

      const runSection = (section) => {
        const url = `${apiUrl}?from=${encodeURIComponent(start)}&to=${encodeURIComponent(end)}&section=${encodeURIComponent(section)}`;
        return fetch(url, { credentials: 'same-origin' })
          .then((res) => res.json().catch(() => ({})).then((data) => ({ ok: res.ok, data })))
          .then(({ ok, data }) => {
            if (mySeq !== requestSeq) return;
            if (!ok) throw new Error(data?.error || `Failed to load ${section}`);
            cached = Object.assign(cached || {}, data || {});
            if (rangeLabel && data.range) {
              rangeLabel.textContent = `${data.range.from} → ${data.range.to}`;
            }
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
      await new Promise((resolve) => {
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
      });
    } catch (err) {
      if (mySeq === requestSeq) {
        console.error(err);
        const el = document.querySelector('.page-content .container-fluid');
        if (el) {
          el.insertAdjacentHTML('afterbegin', `<div class="alert alert-danger">Failed to load ERP reports: ${err.message}</div>`);
        }
      }
    } finally {
      if (mySeq !== requestSeq) return;
      if (btnApply && btnApply.dataset.originalHtml) {
        btnApply.innerHTML = btnApply.dataset.originalHtml;
        btnApply.disabled = false;
        delete btnApply.dataset.originalHtml;
      }
    }
  }

  /* ---- init ---- */
  document.addEventListener('DOMContentLoaded', () => {
    if (window.flatpickr && dateInput) {
      picker = flatpickr(dateInput, { mode: 'range', dateFormat: 'Y-m-d' });
      const [s, e] = getDefaultRange();
      picker.setDate([s, e]);
    }

    chartTypeSelectors.forEach((sel) => {
      sel.addEventListener('change', renderAll);
    });

    if (btnApply) btnApply.addEventListener('click', load);
    if (btnReset) btnReset.addEventListener('click', () => {
      if (picker) {
        const [s, e] = getDefaultRange();
        picker.setDate([s, e], true);
      }
      if (chartTypeDefault) chartTypeDefault.value = 'donut';
      applyDefaultChartTypes();
      load();
    });

    if (chartTypeDefault) {
      chartTypeDefault.addEventListener('change', () => {
        applyDefaultChartTypes();
        renderAll();
      });
    }

    applyDefaultChartTypes();
    load();
  });
})();
