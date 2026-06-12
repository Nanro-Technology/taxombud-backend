(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const api = apiMap.csatAnalysis || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/csat/analysis');
  const caseViewBase = (typeof url_root !== 'undefined' ? url_root : '../') + 'studio/cases/view.kml?id=';

  const dateInput = document.getElementById('csatDateRange');
  const agentFilter = document.getElementById('csatAgentFilter');
  const departmentFilter = document.getElementById('csatDepartmentFilter');
  const categoryFilter = document.getElementById('csatCategoryFilter');
  const priorityFilter = document.getElementById('csatPriorityFilter');
  const ratingFilter = document.getElementById('csatRatingFilter');
  const applyBtn = document.getElementById('csatApplyBtn');
  const resetBtn = document.getElementById('csatResetBtn');
  const exportBtn = document.getElementById('csatExportBtn');
  const reportModeBtn = document.getElementById('csatReportModeBtn');
  const tableModeBtn = document.getElementById('csatTableModeBtn');
  const reportSection = document.getElementById('csatReportSection');
  const tableSection = document.getElementById('csatTableSection');
  const modeHint = document.getElementById('csatModeHint');
  const alertBox = document.getElementById('csatAlert');
  const rangeLabel = document.getElementById('csatRangeLabel');
  const tableBody = document.getElementById('csatTableBody');
  const pagerInfo = document.getElementById('csatPagerInfo');
  const tableMeta = document.getElementById('csatTableMeta');
  const prevBtn = document.getElementById('csatPrevBtn');
  const nextBtn = document.getElementById('csatNextBtn');
  const detailBody = document.getElementById('csatDetailBody');
  const detailCaseRef = document.getElementById('csatDetailCaseRef');
  const detailModalEl = document.getElementById('csatDetailModal');
  const detailModal = detailModalEl ? new bootstrap.Modal(detailModalEl) : null;

  const metricEls = {
    sent: document.getElementById('csatSent'),
    responses: document.getElementById('csatResponses'),
    responseRate: document.getElementById('csatResponseRate'),
    avgScore: document.getElementById('csatAvgScore'),
    avgNps: document.getElementById('csatAvgNps'),
    lowRatings: document.getElementById('csatLowRatings'),
  };

  const chartRefs = {};
  let picker = null;
  let page = 1;
  let perPage = 25;
  let total = 0;
  let metaLoaded = false;
  let select2Ready = false;
  let viewMode = 'report';

  function reloadFromFilters() {
    page = 1;
    load();
  }

  function formatDate(d) {
    if (!(d instanceof Date)) return '';
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  function getDefaultRange() {
    const today = new Date();
    return [today, today];
  }

  function getRangeParams() {
    if (!picker || !picker.selectedDates.length) {
      const [s, e] = getDefaultRange();
      return { start: formatDate(s), end: formatDate(e) };
    }
    const [start, end] = picker.selectedDates;
    return {
      start: formatDate(start),
      end: formatDate(end || start),
    };
  }

  function setAlert(message) {
    if (!alertBox) return;
    if (!message) {
      alertBox.classList.add('d-none');
      alertBox.textContent = '';
      return;
    }
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
  }

  function applyViewMode() {
    const isReport = viewMode === 'report';
    reportSection?.classList.toggle('d-none', !isReport);
    tableSection?.classList.toggle('d-none', isReport);
    reportModeBtn?.classList.toggle('active', isReport);
    tableModeBtn?.classList.toggle('active', !isReport);
    if (modeHint) {
      modeHint.textContent = isReport
        ? 'Report mode shows cards and charts.'
        : 'Table view shows lean response rows only.';
    }
  }

  function escapeHtml(value) {
    const div = document.createElement('div');
    div.textContent = value == null ? '' : String(value);
    return div.innerHTML;
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
          return getComputedStyle(document.documentElement).getPropertyValue(color).trim() || color;
        }
        const parts = value.split(',');
        if (parts.length === 2) {
          return `rgba(${getComputedStyle(document.documentElement).getPropertyValue(parts[0]).trim()},${parts[1]})`;
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
    const safeOptions = options || {};
    if (!Array.isArray(safeOptions.colors) || !safeOptions.colors.length) {
      safeOptions.colors = getChartColorsArray(id) || ['#405189', '#0ab39c', '#f7b84b', '#f06548'];
    }
    chartRefs[id] = new ApexCharts(el, safeOptions);
    chartRefs[id].render();
  }

  function setChartPlaceholder(id, message) {
    destroyChart(id);
    const el = document.getElementById(id);
    if (el) {
      el.innerHTML = `<div class="text-center text-muted py-5">${escapeHtml(message || 'Loading...')}</div>`;
    }
  }

  function buildParams() {
    const range = getRangeParams();
    const params = new URLSearchParams({
      start_date: range.start,
      end_date: range.end,
      page: String(page),
      per_page: String(perPage),
    });
    if (agentFilter && agentFilter.value) params.set('agent_id', agentFilter.value);
    if (departmentFilter && departmentFilter.value) params.set('department_id', departmentFilter.value);
    if (categoryFilter && categoryFilter.value) params.set('domain_id', categoryFilter.value);
    if (priorityFilter && priorityFilter.value) params.set('priority', priorityFilter.value);
    if (ratingFilter && ratingFilter.value) params.set('rating', ratingFilter.value);
    return params;
  }

  function fillSelect(select, rows, valueKey, labelBuilder) {
    if (!select) return;
    const current = select.value;
    const first = select.querySelector('option');
    select.innerHTML = '';
    if (first) {
      select.appendChild(first);
    }
    (rows || []).forEach((row) => {
      const option = document.createElement('option');
      option.value = String(row[valueKey] ?? '');
      option.textContent = labelBuilder(row);
      select.appendChild(option);
    });
    if (current) {
      select.value = current;
    }
  }

  function renderMeta(meta) {
    if (!meta) return;
    fillSelect(agentFilter, meta.agents || [], 'id', (row) => row.department_name ? `${row.display_name} (${row.department_name})` : row.display_name);
    fillSelect(departmentFilter, meta.departments || [], 'id', (row) => row.name || '');
    fillSelect(categoryFilter, meta.categories || [], 'id', (row) => row.name || '');
    if (priorityFilter && !priorityFilter.dataset.loaded) {
      (meta.priorities || []).forEach((priority) => {
        const option = document.createElement('option');
        option.value = priority;
        option.textContent = priority.charAt(0).toUpperCase() + priority.slice(1);
        priorityFilter.appendChild(option);
      });
      priorityFilter.dataset.loaded = '1';
    }
    ensureAgentSelect2();
    metaLoaded = true;
  }

  function ensureAgentSelect2() {
    if (!agentFilter || !(window.jQuery && jQuery.fn && jQuery.fn.select2)) return;
    const $agent = jQuery(agentFilter);
    if ($agent.data('select2')) {
      $agent.select2('destroy');
    }
    $agent.select2({
      width: '100%',
      placeholder: agentFilter.getAttribute('data-placeholder') || 'All agents',
      allowClear: true,
    });
    select2Ready = true;
  }

  function renderSummary(summary) {
    if (!summary) return;
    if (metricEls.sent) metricEls.sent.textContent = Number(summary.sent || 0).toLocaleString();
    if (metricEls.responses) metricEls.responses.textContent = Number(summary.responses || 0).toLocaleString();
    if (metricEls.responseRate) metricEls.responseRate.textContent = `${Number(summary.response_rate || 0).toFixed(1)}%`;
    if (metricEls.avgScore) metricEls.avgScore.textContent = Number(summary.avg_score || 0).toFixed(2);
    if (metricEls.avgNps) metricEls.avgNps.textContent = Number(summary.avg_nps || 0).toFixed(2);
    if (metricEls.lowRatings) metricEls.lowRatings.textContent = Number(summary.low_ratings || 0).toLocaleString();
  }

  function renderTrendChart(trend) {
    renderChart('csatTrendChart', {
      chart: { type: 'line', height: 320, toolbar: { show: false } },
      series: [
        { name: 'Average CSAT', type: 'line', data: trend?.avg_scores || [] },
        { name: 'Responses', type: 'column', data: trend?.response_counts || [] },
      ],
      stroke: { width: [3, 0] },
      dataLabels: { enabled: false },
      xaxis: { categories: trend?.labels || [] },
      yaxis: [
        { min: 0, max: 5, tickAmount: 5, title: { text: 'CSAT' } },
        { opposite: true, min: 0, title: { text: 'Responses' } },
      ],
      legend: { position: 'top' },
    });
  }

  function renderBarChart(id, labels, series, extra) {
    renderChart(id, Object.assign({
      chart: { type: 'bar', height: 320, toolbar: { show: false } },
      series,
      dataLabels: { enabled: false },
      plotOptions: { bar: { horizontal: true, borderRadius: 4 } },
      xaxis: { categories: labels || [] },
      legend: { show: false },
    }, extra || {}));
  }

  function renderDonutChart(id, labels, series) {
    renderChart(id, {
      chart: { type: 'donut', height: 260, parentHeightOffset: 0 },
      series,
      labels,
      legend: { position: 'bottom', horizontalAlign: 'center' },
      dataLabels: { enabled: true },
      plotOptions: { pie: { donut: { size: '65%' } } },
    });
  }

  function renderCharts(charts) {
    renderTrendChart(charts?.trend || { labels: [], avg_scores: [], response_counts: [] });
    renderDonutChart('csatNpsChart', ['Promoters', 'Passives', 'Detractors'], [
      Number(charts?.nps_distribution?.promoters || 0),
      Number(charts?.nps_distribution?.passives || 0),
      Number(charts?.nps_distribution?.detractors || 0),
    ]);
    renderDonutChart('csatRatingChart', ['1 Star', '2 Stars', '3 Stars', '4 Stars', '5 Stars'], [
      Number(charts?.rating_distribution?.['1'] || charts?.rating_distribution?.[1] || 0),
      Number(charts?.rating_distribution?.['2'] || charts?.rating_distribution?.[2] || 0),
      Number(charts?.rating_distribution?.['3'] || charts?.rating_distribution?.[3] || 0),
      Number(charts?.rating_distribution?.['4'] || charts?.rating_distribution?.[4] || 0),
      Number(charts?.rating_distribution?.['5'] || charts?.rating_distribution?.[5] || 0),
    ]);
    const topAgents = charts?.top_agents || [];
    renderBarChart(
      'csatTopAgentsChart',
      topAgents.map((row) => row.agent_name || 'Unassigned'),
      [{ name: 'Avg Score', data: topAgents.map((row) => Number(row.avg_score || 0)) }],
      { xaxis: { categories: topAgents.map((row) => row.agent_name || 'Unassigned'), min: 0, max: 5 } }
    );
    const worstCategories = charts?.worst_categories || [];
    renderBarChart(
      'csatWorstCategoriesChart',
      worstCategories.map((row) => row.category_name || 'Uncategorized'),
      [{ name: 'Avg Score', data: worstCategories.map((row) => Number(row.avg_score || 0)) }],
      { xaxis: { categories: worstCategories.map((row) => row.category_name || 'Uncategorized'), min: 0, max: 5 } }
    );
    const responseAgent = charts?.response_rate_by_agent || [];
    renderBarChart(
      'csatResponseAgentChart',
      responseAgent.map((row) => row.agent_name || 'Unassigned'),
      [{ name: 'Response Rate %', data: responseAgent.map((row) => Number(row.response_rate || 0)) }],
      { xaxis: { categories: responseAgent.map((row) => row.agent_name || 'Unassigned'), min: 0, max: 100 } }
    );
  }

  function npsBadge(nps) {
    if (nps === null || nps === '' || typeof nps === 'undefined') {
      return '<span class="csat-pill csat-pill-empty">No NPS</span>';
    }
    const value = Number(nps);
    if (value >= 9) return '<span class="csat-pill csat-pill-promoter">Promoter</span>';
    if (value >= 7) return '<span class="csat-pill csat-pill-passive">Passive</span>';
    return '<span class="csat-pill csat-pill-detractor">Detractor</span>';
  }

  function commentBadge(comment, rating) {
    if (!comment) {
      return '<span class="csat-pill csat-pill-empty">No Comment</span>';
    }
    if (Number(rating || 0) <= 2) {
      return '<span class="csat-pill csat-pill-low">Low CSAT</span>';
    }
    return '';
  }

  function renderTable(table) {
    const rows = table?.data || [];
    total = Number(table?.total || 0);
    if (tableBody) {
      if (!rows.length) {
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No responded surveys found.</td></tr>';
      } else {
        tableBody.innerHTML = rows.map((row) => {
          const caseId = String(row.case_id || '').trim();
          const caseHref = caseId ? `${caseViewBase}${encodeURIComponent(caseId)}` : '';
          const caseNumberHtml = caseHref
            ? `<a href="${escapeHtml(caseHref)}" class="fw-semibold text-primary text-decoration-underline">${escapeHtml(row.case_number || '')}</a>`
            : `<div class="fw-semibold">${escapeHtml(row.case_number || '')}</div>`;
          return `<tr>
            <td>${escapeHtml(row.response_date || '')}</td>
            <td>
              <div class="fw-semibold">${escapeHtml(row.contact_name || '-')}</div>
              <div class="small text-muted">${escapeHtml(row.contact_email || '')}</div>
            </td>
            <td>
              ${caseNumberHtml}
              <div class="small text-muted">${escapeHtml(row.case_subject || '')}</div>
            </td>
            <td>${escapeHtml(row.agent_name || 'Unassigned')}</td>
            <td><span class="badge bg-primary-subtle text-primary">${escapeHtml(row.rating || '')}</span></td>
            <td class="text-end">
              <button type="button" class="btn btn-primary btn-sm csat-detail-btn" data-survey-id="${escapeHtml(row.survey_id || '')}">View More</button>
            </td>
          </tr>`;
        }).join('');
      }
    }
    const firstRow = total ? ((page - 1) * perPage) + 1 : 0;
    const lastRow = total ? Math.min(page * perPage, total) : 0;
    if (pagerInfo) pagerInfo.textContent = total ? `Showing ${firstRow}-${lastRow} of ${total}` : 'No records';
    if (tableMeta) tableMeta.textContent = total ? `${total} responded surveys` : 'No responded surveys';
    if (prevBtn) prevBtn.disabled = page <= 1;
    if (nextBtn) nextBtn.disabled = lastRow >= total;
  }

  function renderDetail(detail) {
    if (!detailBody) return;
    if (detailCaseRef) {
      detailCaseRef.textContent = detail?.case_number ? `${detail.case_number} · ${detail.case_subject || ''}` : '';
    }
    const commentBadgeHtml = commentBadge(detail?.comment, detail?.rating);
    detailBody.innerHTML = `
      <div class="row g-3">
        <div class="col-md-6">
          <div class="card border mb-0">
            <div class="card-body">
              <h6 class="mb-3">Customer</h6>
              <div class="mb-2"><span class="text-muted">Name:</span> <span class="fw-semibold">${escapeHtml(detail?.contact_name || '-')}</span></div>
              <div class="mb-2"><span class="text-muted">Email:</span> <span class="fw-semibold">${escapeHtml(detail?.contact_email || '-')}</span></div>
              <div><span class="text-muted">Organization:</span> <span class="fw-semibold">${escapeHtml(detail?.organization_name || '-')}</span></div>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <div class="card border mb-0">
            <div class="card-body">
              <h6 class="mb-3">Case</h6>
              <div class="mb-2"><span class="text-muted">Case:</span> <span class="fw-semibold">${escapeHtml(detail?.case_number || '-')}</span></div>
              <div class="mb-2"><span class="text-muted">Category:</span> <span class="fw-semibold">${escapeHtml(detail?.category_name || '-')}</span></div>
              <div class="mb-2"><span class="text-muted">Assigned Agent:</span> <span class="fw-semibold">${escapeHtml(detail?.agent_name || 'Unassigned')}</span></div>
              <div><span class="text-muted">Department:</span> <span class="fw-semibold">${escapeHtml(detail?.department_name || '-')}</span></div>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card border mb-0">
            <div class="card-body">
              <h6 class="mb-3">Rating</h6>
              <div class="display-6 fw-bold">${escapeHtml(detail?.rating || '-')}</div>
              <div class="text-muted small">CSAT score</div>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card border mb-0">
            <div class="card-body">
              <h6 class="mb-3">NPS</h6>
              <div class="display-6 fw-bold">${escapeHtml(detail?.nps_score ?? '-')}</div>
              <div class="mt-2">${npsBadge(detail?.nps_score)}</div>
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <div class="card border mb-0">
            <div class="card-body">
              <h6 class="mb-3">Survey Status</h6>
              <div class="small text-muted mb-2">Token hash stored</div>
              <code>${escapeHtml(detail?.token_hash || '')}</code>
            </div>
          </div>
        </div>
        <div class="col-12">
          <div class="card border mb-0">
            <div class="card-body">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="mb-0">Comment</h6>
                ${commentBadgeHtml || ''}
              </div>
              <div class="text-body">${escapeHtml(detail?.comment || 'No comment provided.')}</div>
            </div>
          </div>
        </div>
        <div class="col-12">
          <div class="card border mb-0">
            <div class="card-body">
              <h6 class="mb-3">Timeline</h6>
              <div class="row g-2">
                <div class="col-md-4"><span class="text-muted">Sent:</span> <span class="fw-semibold">${escapeHtml(detail?.sent_at || '-')}</span></div>
                <div class="col-md-4"><span class="text-muted">Responded:</span> <span class="fw-semibold">${escapeHtml(detail?.responded_at || '-')}</span></div>
                <div class="col-md-4"><span class="text-muted">Response Date:</span> <span class="fw-semibold">${escapeHtml(detail?.response_date || '-')}</span></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    `;
  }

  function setLoadingState() {
    Object.values(metricEls).forEach((el) => {
      if (el) el.textContent = '--';
    });
    ['csatTrendChart', 'csatNpsChart', 'csatRatingChart', 'csatTopAgentsChart', 'csatWorstCategoriesChart', 'csatResponseAgentChart']
      .forEach((id) => setChartPlaceholder(id, 'Loading...'));
    if (tableBody) tableBody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Loading...</td></tr>';
  }

  function load() {
    setAlert('');
    setLoadingState();
    const params = buildParams();
    const range = getRangeParams();
    if (rangeLabel) {
      rangeLabel.textContent = `${range.start} to ${range.end}`;
    }
    fetch(`${api}?${params.toString()}`, { cache: 'no-store' })
      .then((r) => r.json().then((body) => ({ ok: r.ok, body })))
      .then(({ ok, body }) => {
        if (!ok) throw new Error(body?.error || 'Unable to load CSAT analysis');
        renderMeta(body.meta || {});
        renderSummary(body.summary || {});
        renderCharts(body.charts || {});
        renderTable(body.table || {});
      })
      .catch((err) => {
        setAlert(err?.message || 'Unable to load CSAT analysis');
      });
  }

  function openDetail(surveyId) {
    if (!surveyId || !detailModal) return;
    detailBody.innerHTML = '<div class="text-center text-muted py-4">Loading...</div>';
    detailCaseRef.textContent = '';
    detailModal.show();
    fetch(`${api}?detail_id=${encodeURIComponent(surveyId)}`, { cache: 'no-store' })
      .then((r) => r.json().then((body) => ({ ok: r.ok, body })))
      .then(({ ok, body }) => {
        if (!ok) throw new Error(body?.error || 'Unable to load response detail');
        renderDetail(body || {});
      })
      .catch((err) => {
        detailBody.innerHTML = `<div class="alert alert-danger mb-0">${escapeHtml(err?.message || 'Unable to load response detail')}</div>`;
      });
  }

  function resetFilters() {
    page = 1;
    const [start, end] = getDefaultRange();
    if (picker) picker.setDate([start, end], true);
    if (agentFilter) agentFilter.value = '';
    if (departmentFilter) departmentFilter.value = '';
    if (categoryFilter) categoryFilter.value = '';
    if (priorityFilter) priorityFilter.value = '';
    if (ratingFilter) ratingFilter.value = '';
    if (select2Ready && window.jQuery && jQuery.fn && jQuery.fn.select2) {
      jQuery(agentFilter).trigger('change.select2');
    }
    load();
  }

  if (dateInput && window.flatpickr) {
    const [start, end] = getDefaultRange();
    picker = flatpickr(dateInput, {
      mode: 'range',
      dateFormat: 'Y-m-d',
      defaultDate: [start, end],
      onClose(selectedDates) {
        if (!selectedDates || !selectedDates.length) return;
        if (selectedDates.length === 1 || selectedDates.length === 2) {
          reloadFromFilters();
        }
      },
    });
  }

  applyBtn?.addEventListener('click', reloadFromFilters);
  reportModeBtn?.addEventListener('click', () => {
    viewMode = 'report';
    applyViewMode();
  });
  tableModeBtn?.addEventListener('click', () => {
    viewMode = 'table';
    applyViewMode();
  });
  resetBtn?.addEventListener('click', resetFilters);
  agentFilter?.addEventListener('change', reloadFromFilters);
  departmentFilter?.addEventListener('change', reloadFromFilters);
  categoryFilter?.addEventListener('change', reloadFromFilters);
  priorityFilter?.addEventListener('change', reloadFromFilters);
  ratingFilter?.addEventListener('change', reloadFromFilters);
  prevBtn?.addEventListener('click', () => {
    if (page <= 1) return;
    page -= 1;
    load();
  });
  nextBtn?.addEventListener('click', () => {
    if ((page * perPage) >= total) return;
    page += 1;
    load();
  });
  exportBtn?.addEventListener('click', () => {
    const params = buildParams();
    params.set('export', 'csv');
    window.location.href = `${api}?${params.toString()}`;
  });

  tableBody?.addEventListener('click', (e) => {
    const btn = e.target.closest('.csat-detail-btn');
    if (!btn) return;
    openDetail(btn.getAttribute('data-survey-id'));
  });

  applyViewMode();
  load();
})();
