(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const root = (typeof url_root !== 'undefined' ? url_root : '../');
  const apiResolved = (window.caseMyResolvedCasesPage && window.caseMyResolvedCasesPage.apiUrl)
    ? (window.caseMyResolvedCasesPage.apiUrl.startsWith('http') ? window.caseMyResolvedCasesPage.apiUrl : root + window.caseMyResolvedCasesPage.apiUrl.replace(/^\/+/, ''))
    : (apiMap.caseMyResolvedCases || (root + 'api/modules/cases/my_resolved_cases'));

  const dateInput = document.getElementById('myResolvedCasesDateRange');
  const btnApply = document.getElementById('myResolvedCasesApply');
  const btnReset = document.getElementById('myResolvedCasesReset');
  const searchInput = document.getElementById('myResolvedCasesSearch');
  const table = document.getElementById('myResolvedCasesTable');
  const body = document.getElementById('myResolvedCasesBody');
  const rangeLabel = document.getElementById('myResolvedCasesRangeLabel');
  const alertBox = document.getElementById('myResolvedCasesAlert');
  const statusBox = document.getElementById('myResolvedCasesStatus');

  let picker = null;

  function esc(value) {
    return String(value || '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  function showAlert(message) {
    if (!alertBox) return;
    if (!message) {
      alertBox.classList.add('d-none');
      alertBox.textContent = '';
      return;
    }
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
  }

  function setStatus(message) {
    if (!statusBox) return;
    statusBox.textContent = message || '';
  }

  function formatDateTimeInput(d) {
    if (!(d instanceof Date)) return '';
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${y}-${m}-${day} ${hh}:${mm}`;
  }

  function getDefaultRange() {
    const now = new Date();
    const since = new Date(now.getTime() - (24 * 60 * 60 * 1000));
    return [since, now];
  }

  function getRangeParams() {
    if (!picker || !picker.selectedDates.length) {
      const [s, e] = getDefaultRange();
      return { start: formatDateTimeInput(s), end: formatDateTimeInput(e) };
    }
    const [start, end] = picker.selectedDates;
    return {
      start: formatDateTimeInput(start),
      end: formatDateTimeInput(end || start)
    };
  }

  function getSearchValue() {
    return searchInput ? String(searchInput.value || '').trim() : '';
  }

  function updateRangeLabel(start, end) {
    if (!rangeLabel) return;
    rangeLabel.textContent = start && end ? `Showing ${start} to ${end}` : '';
  }

  function statusBadge(status) {
    if (typeof window.renderStatusBadge === 'function') return window.renderStatusBadge(status);
    const s = String(status || '').toLowerCase();
    const label = s ? s.charAt(0).toUpperCase() + s.slice(1) : 'Unknown';
    return `<span class="badge bg-secondary-subtle text-secondary">${esc(label)}</span>`;
  }

  function typeBadge(type) {
    const t = String(type || '').toLowerCase();
    const cls = t === 'approval' ? 'approval' : (t === 'assignment' ? 'workflow' : 'workflow');
    const label = t === 'approval' ? 'Approval' : (t === 'assignment' ? 'Assignment' : (t === 'created' ? 'Creation' : 'Workflow'));
    return `<span class="my-solved-case-pill ${cls}">${esc(label)}</span>`;
  }

  function actionMarkup(row) {
    const actionLabel = row.activity_label || 'Activity';
    const note = String(row.activity_note || '').trim();
    const noteMarkup = note ? `<div class="my-solved-case-subtext mt-1">${esc(note)}</div>` : '<div class="my-solved-case-subtext mt-1">No note recorded.</div>';
    return `<div><div class="my-solved-case-subject">${esc(actionLabel)}</div>${noteMarkup}</div>`;
  }

  function formatDateTime(value) {
    if (!value) return '-';
    return esc(value);
  }

  function renderRows(rows) {
    if (!body || !table) return;
    if ($.fn.DataTable.isDataTable('#myResolvedCasesTable')) {
      $('#myResolvedCasesTable').DataTable().clear().destroy();
    }
    body.innerHTML = '';
    rows.forEach((row) => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>
          <a href="${esc(row.view_url || '#')}" class="fw-semibold text-primary text-decoration-underline">${esc(row.case_number || '-')}</a>
        </td>
        <td>
          <div class="my-solved-case-subject">${esc(row.subject || '[Case record missing]')}</div>
        </td>
        <td>${typeBadge(row.event_type)}</td>
        <td>${actionMarkup(row)}</td>
        <td>${statusBadge(row.case_status)}</td>
        <td>${formatDateTime(row.event_at)}</td>
        <td class="text-end">
          <a class="btn btn-sm btn-outline-primary" href="${esc(row.view_url || '#')}">View</a>
        </td>
      `;
      body.appendChild(tr);
    });

      $('#myResolvedCasesTable').DataTable({
        destroy: true,
        pageLength: 25,
        order: [[5, 'desc']],
        columnDefs: [{ orderable: false, targets: 6 }],
        language: {
        emptyTable: 'No case history found for this time frame.'
        }
      });
  }

  function setLoading() {
    setStatus('Loading cases...');
    showAlert('');
    if (body) {
      body.innerHTML = '';
    }
  }

  async function fetchData(e) {
    if (e) e.preventDefault();
    const { start, end } = getRangeParams();
    const q = getSearchValue();
    updateRangeLabel(start, end);
    setLoading();

    const params = new URLSearchParams({
      start_date: start,
      end_date: end
    });
    if (q !== '') {
      params.set('q', q);
    }

    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Loading...';
    }

    try {
      const response = await fetch(`${apiResolved}?${params.toString()}`, { credentials: 'same-origin' });
      const rawText = await response.text();
      let data = {};
      if (rawText) {
        try {
          data = JSON.parse(rawText);
        } catch (parseErr) {
          data = {};
        }
      }
      if (!response.ok) {
          console.error('[My Case History] API error', {
          url: `${apiResolved}?${params.toString()}`,
          status: response.status,
          statusText: response.statusText,
          body: rawText || '',
          response: data || {}
        });
        const detail = typeof data.error === 'string' && data.error.trim() !== ''
          ? data.error.trim()
          : (rawText && rawText.trim() ? rawText.trim() : `HTTP ${response.status} ${response.statusText || 'Internal Server Error'}`);
        throw new Error(detail);
      }
      const rows = Array.isArray(data.rows) ? data.rows : [];
      renderRows(rows);
      setStatus(rows.length ? `${rows.length} case${rows.length === 1 ? '' : 's'} loaded.` : 'No cases found.');
    } catch (err) {
      console.error('[My Case History] fetch failed', err);
      showAlert(err.message || 'Unable to load case history');
      setStatus('Unable to load case history.');
      if ($.fn.DataTable.isDataTable('#myResolvedCasesTable')) {
        $('#myResolvedCasesTable').DataTable().clear().destroy();
      }
      if (body) {
        body.innerHTML = '';
      }
    } finally {
      if (btnApply && btnApply.dataset.originalHtml) {
        btnApply.innerHTML = btnApply.dataset.originalHtml;
        btnApply.disabled = false;
        delete btnApply.dataset.originalHtml;
      }
    }
  }

  function init() {
    const [start, end] = getDefaultRange();
    if (dateInput && typeof flatpickr !== 'undefined') {
      picker = flatpickr(dateInput, {
        mode: 'range',
        enableTime: true,
        time_24hr: true,
        dateFormat: 'Y-m-d H:i',
        defaultDate: [formatDateTimeInput(start), formatDateTimeInput(end)]
      });
    }

    if (btnApply) btnApply.addEventListener('click', fetchData);
    if (btnReset) {
      btnReset.addEventListener('click', () => {
        const [s, e] = getDefaultRange();
        if (picker) {
          picker.setDate([s, e], true);
        }
        if (searchInput) {
          searchInput.value = '';
        }
        fetchData();
      });
    }
    if (searchInput) {
      searchInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
          event.preventDefault();
          fetchData();
        }
      });
    }

    fetchData();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
