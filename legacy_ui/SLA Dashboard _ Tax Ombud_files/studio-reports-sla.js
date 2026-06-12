/* eslint-disable */
(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiDashboard = apiMap.slaDashboard || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/sla/dashboard');

  const dateInput = document.getElementById('slaReportDateRange');
  const domainInput = document.getElementById('slaReportDomain');
  const applyBtn = document.getElementById('slaReportApply');
  const resetBtn = document.getElementById('slaReportReset');
  const rangeLabel = document.getElementById('slaReportRangeLabel');

  const trackedEl = document.getElementById('slaTrackedCount');
  const onTimeEl = document.getElementById('slaOnTimeCount');
  const breachedEl = document.getElementById('slaBreachedCount');
  const complianceEl = document.getElementById('slaCompliance');
  const byPriorityBody = document.getElementById('slaByPriorityBody');
  const byDomainBody = document.getElementById('slaByDomainBody');
  const breachedCasesBody = document.getElementById('slaBreachedCasesBody');
  const breachedMetaEl = document.getElementById('slaBreachedMeta');

  let picker = null;
  let trendChart = null;
  let requestSeq = 0;

  function defaultRange() {
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

  function getRange() {
    if (!picker || !picker.selectedDates.length) {
      const [s, e] = defaultRange();
      return { start: formatDate(s), end: formatDate(e) };
    }
    const [start, end] = picker.selectedDates;
    return { start: formatDate(start), end: formatDate(end || start) };
  }

  function getChartColorsArray(id) {
    const el = document.getElementById(id);
    if (!el) return null;
    const attr = el.getAttribute('data-colors');
    if (!attr) return null;
    try {
      const colors = JSON.parse(attr);
      return colors.map((v) => {
        const c = (v || '').replace(' ', '');
        if (c.indexOf(',') === -1) return getComputedStyle(document.documentElement).getPropertyValue(c) || c;
        const parts = c.split(',');
        return `rgba(${getComputedStyle(document.documentElement).getPropertyValue(parts[0])},${parts[1]})`;
      });
    } catch (e) {
      return null;
    }
  }

  function renderTableRows(tbody, rows, mapper) {
    if (!tbody) return;
    tbody.innerHTML = '';
    if (!rows || !rows.length) {
      const tr = document.createElement('tr');
      tr.innerHTML = '<td colspan="3" class="text-center text-muted">No data.</td>';
      tbody.appendChild(tr);
      return;
    }
    rows.forEach((row) => {
      const tr = document.createElement('tr');
      tr.innerHTML = mapper(row);
      tbody.appendChild(tr);
    });
  }

  function renderBreachedCases(rows, totalCount, limitCount) {
    if (!breachedCasesBody) return;
    breachedCasesBody.innerHTML = '';
    if (breachedMetaEl) {
      const shown = Array.isArray(rows) ? rows.length : 0;
      const total = Number(totalCount || 0);
      const limit = Number(limitCount || 0);
      if (total > shown && limit > 0) {
        breachedMetaEl.textContent = `Showing latest ${shown} of ${total} breached cases in range (cap ${limit}).`;
      } else if (total > 0) {
        breachedMetaEl.textContent = `Showing ${shown} breached cases in range.`;
      } else {
        breachedMetaEl.textContent = 'No breaches in selected range.';
      }
    }
    if (!rows || !rows.length) {
      breachedCasesBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No breaches in range.</td></tr>';
      return;
    }
    rows.forEach((row) => {
      const caseId = row.id_s || row.id || '';
      const caseLabel = row.case_number || ('Case #' + (row.id || ''));
      const subject = row.subject || '';
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td><a href="studio/cases/view.kml?id=${encodeURIComponent(caseId)}">${caseLabel}</a><div class="small text-muted">${subject}</div></td>
        <td>${row.domain_name || '-'}</td>
        <td>${row.priority || '-'}</td>
        <td><span class="badge bg-danger-subtle text-danger">${row.breach_kind || 'breached'}</span></td>
        <td>${row.breached_at || '-'}</td>
      `;
      breachedCasesBody.appendChild(tr);
    });
  }

  function renderTrend(labels, onTime, breached) {
    const el = document.getElementById('slaTrendChart');
    if (!el) return;
    if (trendChart) {
      trendChart.destroy();
      trendChart = null;
    }
    trendChart = new ApexCharts(el, {
      chart: { type: 'bar', height: 320, stacked: true, toolbar: { show: false } },
      series: [
        { name: 'On Time', data: onTime || [] },
        { name: 'Breached', data: breached || [] }
      ],
      xaxis: { categories: labels || [] },
      dataLabels: { enabled: false },
      legend: { position: 'top' },
      plotOptions: { bar: { columnWidth: '55%' } },
      colors: getChartColorsArray('slaTrendChart') || undefined
    });
    trendChart.render();
  }

  function setTrendLoading() {
    const el = document.getElementById('slaTrendChart');
    if (!el) return;
    if (trendChart) {
      trendChart.destroy();
      trendChart = null;
    }
    el.innerHTML = '<div class="text-center text-muted py-5">Computing values...</div>';
  }

  function setTrendError() {
    const el = document.getElementById('slaTrendChart');
    if (!el) return;
    if (trendChart) {
      trendChart.destroy();
      trendChart = null;
    }
    el.innerHTML = '<div class="text-center text-danger py-5">Failed to load trend.</div>';
  }

  function setLoading(isLoading) {
    if (!applyBtn) return;
    if (isLoading) {
      applyBtn.disabled = true;
      applyBtn.dataset.originalHtml = applyBtn.innerHTML;
      applyBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Computing values...';
      return;
    }
    if (applyBtn.dataset.originalHtml) {
      applyBtn.innerHTML = applyBtn.dataset.originalHtml;
      delete applyBtn.dataset.originalHtml;
    }
    applyBtn.disabled = false;
  }

  function fetchJsonWithTimeout(url, timeoutMs = 30000) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);
    return fetch(url, { signal: controller.signal })
      .finally(() => clearTimeout(timer))
      .then((resp) => resp.json().catch(() => ({})).then((data) => ({ ok: resp.ok, data })));
  }

  async function loadDashboard() {
    const mySeq = ++requestSeq;
    const { start, end } = getRange();
    if (rangeLabel) rangeLabel.textContent = `Showing ${start} to ${end}`;
    const baseParams = new URLSearchParams({ start_date: start, end_date: end });
    if (domainInput && domainInput.value) baseParams.set('domain_id', domainInput.value);

    if (trackedEl) trackedEl.textContent = '--';
    if (onTimeEl) onTimeEl.textContent = '--';
    if (breachedEl) breachedEl.textContent = '--';
    if (complianceEl) complianceEl.textContent = '--';
    if (byPriorityBody) byPriorityBody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Computing values...</td></tr>';
    if (byDomainBody) byDomainBody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Computing values...</td></tr>';
    if (breachedCasesBody) breachedCasesBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Computing values...</td></tr>';
    if (breachedMetaEl) breachedMetaEl.textContent = 'Computing values...';
    setTrendLoading();

    setLoading(true);
    try {
      const sections = ['totals', 'priority', 'domain', 'trend', 'breached'];
      const sectionErrors = {
        totals: () => {
          if (trackedEl) trackedEl.textContent = '--';
          if (onTimeEl) onTimeEl.textContent = '--';
          if (breachedEl) breachedEl.textContent = '--';
          if (complianceEl) complianceEl.textContent = '--';
        },
        priority: () => { if (byPriorityBody) byPriorityBody.innerHTML = '<tr><td colspan="3" class="text-center text-danger">Failed to load priority table.</td></tr>'; },
        domain: () => { if (byDomainBody) byDomainBody.innerHTML = '<tr><td colspan="3" class="text-center text-danger">Failed to load domain table.</td></tr>'; },
        trend: () => setTrendError(),
        breached: () => { if (breachedCasesBody) breachedCasesBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Failed to load breached cases.</td></tr>'; }
      };

      const runSection = (section) => {
        const params = new URLSearchParams(baseParams.toString());
        params.set('section', section);
        return fetchJsonWithTimeout(apiDashboard + '?' + params.toString(), 30000)
          .then(({ ok, data }) => {
            if (mySeq !== requestSeq) return;
            if (!ok) throw new Error(data.error || `Unable to load ${section}`);
            if (data.range && rangeLabel) rangeLabel.textContent = `Showing ${data.range.start} to ${data.range.end}`;
            if (Object.prototype.hasOwnProperty.call(data, 'totals')) {
              const totals = data.totals || {};
              if (trackedEl) trackedEl.textContent = String(totals.tracked || 0);
              if (onTimeEl) onTimeEl.textContent = String(totals.on_time || 0);
              if (breachedEl) breachedEl.textContent = String(totals.breached || 0);
              if (complianceEl) complianceEl.textContent = `${Number(totals.compliance || 0).toFixed(2)}%`;
            }
            if (Object.prototype.hasOwnProperty.call(data, 'by_priority')) {
              renderTableRows(byPriorityBody, data.by_priority || [], (r) =>
                `<td>${r.bucket || 'unknown'}</td><td>${Number(r.tracked || 0)}</td><td>${Number(r.breached || 0)}</td>`
              );
            }
            if (Object.prototype.hasOwnProperty.call(data, 'by_domain')) {
              renderTableRows(byDomainBody, data.by_domain || [], (r) =>
                `<td>${r.bucket || 'Uncategorized'}</td><td>${Number(r.tracked || 0)}</td><td>${Number(r.breached || 0)}</td>`
              );
            }
            if (Object.prototype.hasOwnProperty.call(data, 'trend')) {
              const trend = data.trend || {};
              renderTrend(trend.labels || [], trend.on_time || [], trend.breached || []);
            }
            if (Object.prototype.hasOwnProperty.call(data, 'breached_cases')) {
              renderBreachedCases(data.breached_cases || [], data.breached_total || 0, data.breached_limit || 0);
            }
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
    } catch (e) {
      if (mySeq === requestSeq && rangeLabel) rangeLabel.textContent = e.message || 'Unable to load SLA dashboard';
    } finally {
      if (mySeq !== requestSeq) return;
      setLoading(false);
    }
  }

  function init() {
    const [s, e] = defaultRange();
    if (dateInput && typeof flatpickr !== 'undefined') {
      picker = flatpickr(dateInput, {
        mode: 'range',
        dateFormat: 'Y-m-d',
        defaultDate: [formatDate(s), formatDate(e)]
      });
    }
    if (applyBtn) applyBtn.addEventListener('click', (ev) => { ev.preventDefault(); loadDashboard(); });
    if (resetBtn) {
      resetBtn.addEventListener('click', () => {
        const [rs, re] = defaultRange();
        if (picker) picker.setDate([rs, re], true);
        if (domainInput) domainInput.value = '';
        loadDashboard();
      });
    }
    if (domainInput) domainInput.addEventListener('change', loadDashboard);
    loadDashboard();
  }

  document.addEventListener('DOMContentLoaded', init);
})();
