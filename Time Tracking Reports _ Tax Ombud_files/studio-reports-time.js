/* eslint-disable */
(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiReports = apiMap.timeLogsReports || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/time-logs/reports');

  const dateInput = document.getElementById('timeReportDateRange');
  const btnApply = document.getElementById('timeReportApply');
  const btnReset = document.getElementById('timeReportReset');
  const rangeLabel = document.getElementById('timeReportRangeLabel');
  const entitySelect = document.getElementById('timeReportEntity');
  const agentSelect = document.getElementById('timeReportAgent');
  const totalHoursEl = document.getElementById('timeTotalHours');
  const billableHoursEl = document.getElementById('timeBillableHours');
  const utilizationEl = document.getElementById('timeUtilization');
  const byUserBody = document.getElementById('timeByUserBody');
  const detailBody = document.getElementById('timeLogDetailBody');
  const detailTab = document.getElementById('timeReportDetailTab');

  const chartRefs = {};
  let picker = null;
  let requestSeq = 0;

  function formatHours(mins) {
    const m = Number(mins || 0);
    if (!m) return '0h';
    const hours = m / 60;
    const rounded = Math.round(hours * 10) / 10;
    return (rounded % 1 === 0 ? rounded.toFixed(0) : rounded.toFixed(1)) + 'h';
  }

  function escapeHtml(str) {
    return String(str || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  function getDefaultRange() {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 29);
    return [start, end];
  }

  function formatDate(d) {
    if (!(d instanceof Date)) return '';
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
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

  function updateRangeLabel(start, end) {
    if (rangeLabel) {
      rangeLabel.textContent = start && end ? `Showing ${start} to ${end}` : '';
    }
  }

  function destroyChart(id) {
    if (chartRefs[id]) {
      chartRefs[id].destroy();
      delete chartRefs[id];
    }
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

  function renderByDayChart(labels, billable, nonBillable) {
    destroyChart('timeByDayChart');
    const el = document.getElementById('timeByDayChart');
    if (!el) return;
    el.innerHTML = '';
    const toHours = (arr) => (arr || []).map((m) => Math.round((Number(m || 0) / 60) * 10) / 10);
    const series = [
      { name: 'Billable', data: toHours(billable) },
      { name: 'Non-billable', data: toHours(nonBillable) }
    ];
    chartRefs['timeByDayChart'] = new ApexCharts(el, {
      series,
      chart: { type: 'bar', height: 320, stacked: true, toolbar: { show: false } },
      plotOptions: { bar: { horizontal: false, columnWidth: '55%' } },
      dataLabels: { enabled: false },
      colors: getChartColorsArray('timeByDayChart') || undefined,
      xaxis: { categories: labels || [] },
      yaxis: { labels: { formatter: (val) => (val ?? 0).toString() }, min: 0 },
      legend: { position: 'top' },
      tooltip: { shared: true, intersect: false }
    });
    chartRefs['timeByDayChart'].render();
  }

  function setByDayChartLoading() {
    destroyChart('timeByDayChart');
    const el = document.getElementById('timeByDayChart');
    if (el) el.innerHTML = '<div class="text-center text-muted py-5">Computing values...</div>';
  }

  function setByDayChartError() {
    destroyChart('timeByDayChart');
    const el = document.getElementById('timeByDayChart');
    if (el) el.innerHTML = '<div class="text-center text-danger py-5">Failed to load chart.</div>';
  }

  function renderByUser(rows) {
    if (!byUserBody) return;
    byUserBody.innerHTML = '';
    if (!rows || !rows.length) {
      byUserBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">No data.</td></tr>';
      return;
    }
    rows.forEach((r) => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td class="fw-medium">${escapeHtml(r.name || 'User')}</td>
        <td>${formatHours(r.total_minutes || 0)}</td>
        <td>${formatHours(r.billable_minutes || 0)}</td>
        <td>${(r.utilization ?? 0).toFixed ? r.utilization.toFixed(1) : r.utilization}%</td>
      `;
      byUserBody.appendChild(tr);
    });
  }

  function renderDetail(rows) {
    if (!detailBody) return;
    detailBody.innerHTML = '';
    if (!rows || !rows.length) {
      detailBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No data.</td></tr>';
      return;
    }
    rows.forEach((r) => {
      const entityLabel = r.entity_label || `${r.entity_type || 'entity'} #${r.entity_id || ''}`;
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${escapeHtml(r.created_at || '')}</td>
        <td>${escapeHtml(r.user_name || 'User')}</td>
        <td>${escapeHtml(entityLabel)}</td>
        <td>${r.minutes || 0}</td>
        <td>${r.is_billable ? 'Yes' : 'No'}</td>
        <td>${escapeHtml(r.note || '')}</td>
      `;
      detailBody.appendChild(tr);
    });
  }

  async function fetchReports(includeDetail) {
    const mySeq = ++requestSeq;
    const { start, end } = getRangeParams();
    const baseParams = new URLSearchParams({ start_date: start, end_date: end });
    if (entitySelect && entitySelect.value) baseParams.set('entity_type', entitySelect.value);
    if (agentSelect && agentSelect.value) baseParams.set('agent_id', agentSelect.value);
    updateRangeLabel(start, end);

    if (totalHoursEl) totalHoursEl.textContent = '--';
    if (billableHoursEl) billableHoursEl.textContent = '--';
    if (utilizationEl) utilizationEl.textContent = '--';
    if (byUserBody) byUserBody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Computing values...</td></tr>';
    setByDayChartLoading();
    if (includeDetail && detailBody) {
      detailBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Computing values...</td></tr>';
    }

    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Computing values...';
    }

    try {
      const summaryParams = new URLSearchParams(baseParams.toString());
      summaryParams.set('section', 'summary');
      const requests = [
        fetch(apiReports + '?' + summaryParams.toString())
          .then((resp) => resp.json().catch(() => ({})).then((data) => ({ ok: resp.ok, data })))
          .then(({ ok, data }) => {
            if (mySeq !== requestSeq) return;
            if (!ok) throw new Error(data.error || 'Unable to load report summary');
            if (totalHoursEl) totalHoursEl.textContent = formatHours(data.totals?.total_minutes || 0);
            if (billableHoursEl) billableHoursEl.textContent = formatHours(data.totals?.billable_minutes || 0);
            if (utilizationEl) utilizationEl.textContent = `${data.totals?.utilization || 0}%`;
            renderByUser(data.by_user || []);
            renderByDayChart(data.by_day?.labels || [], data.by_day?.billable || [], data.by_day?.non_billable || []);
          })
          .catch((err) => {
            if (mySeq !== requestSeq) return;
            if (byUserBody) byUserBody.innerHTML = '<tr><td colspan="4" class="text-center text-danger">Failed to load summary.</td></tr>';
            setByDayChartError();
            if (rangeLabel) rangeLabel.textContent = err.message || 'Unable to load report';
          })
      ];

      if (includeDetail) {
        const detailParams = new URLSearchParams(baseParams.toString());
        detailParams.set('section', 'detail');
        requests.push(
          fetch(apiReports + '?' + detailParams.toString())
            .then((resp) => resp.json().catch(() => ({})).then((data) => ({ ok: resp.ok, data })))
            .then(({ ok, data }) => {
              if (mySeq !== requestSeq) return;
              if (!ok) throw new Error(data.error || 'Unable to load report detail');
              renderDetail(data.logs || []);
            })
            .catch((err) => {
              if (mySeq !== requestSeq) return;
              if (detailBody) detailBody.innerHTML = '<tr><td colspan="6" class="text-center text-danger">Failed to load detail.</td></tr>';
              if (rangeLabel) rangeLabel.textContent = err.message || 'Unable to load report';
            })
        );
      }

      await Promise.all(requests);
    } catch (e) {
      if (mySeq === requestSeq && rangeLabel) rangeLabel.textContent = e.message || 'Unable to load report';
    } finally {
      if (mySeq !== requestSeq) return;
      if (btnApply && btnApply.dataset.originalHtml) {
        btnApply.innerHTML = btnApply.dataset.originalHtml;
        btnApply.disabled = false;
        delete btnApply.dataset.originalHtml;
      }
    }
  }

  function init() {
    const defaultRange = getDefaultRange();
    if (dateInput && typeof flatpickr !== 'undefined') {
      picker = flatpickr(dateInput, {
        mode: 'range',
        dateFormat: 'Y-m-d',
        defaultDate: [formatDate(defaultRange[0]), formatDate(defaultRange[1])]
      });
    }

    fetchReports(false);

    if (btnApply) btnApply.addEventListener('click', (e) => { e.preventDefault(); fetchReports(isDetailActive()); });
    if (btnReset) {
      btnReset.addEventListener('click', () => {
        if (picker) {
          const [s, e] = getDefaultRange();
          picker.setDate([s, e], true);
        }
        if (entitySelect) entitySelect.value = '';
        if (agentSelect) agentSelect.value = '';
        fetchReports(isDetailActive());
      });
    }
    if (entitySelect) entitySelect.addEventListener('change', () => fetchReports(isDetailActive()));
    if (agentSelect) agentSelect.addEventListener('change', () => fetchReports(isDetailActive()));

    if (detailTab) {
      detailTab.addEventListener('shown.bs.tab', () => {
        fetchReports(true);
      });
    }
  }

  function isDetailActive() {
    const active = document.querySelector('#timeReportTabs .nav-link.active');
    return active && active.id === 'timeReportDetailTab';
  }

  document.addEventListener('DOMContentLoaded', init);
})();
