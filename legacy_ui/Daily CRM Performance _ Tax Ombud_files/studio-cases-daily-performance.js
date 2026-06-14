(function () {
  const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
  const apiDaily = apiMap.caseDailyPerformance || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/cases/daily_performance');
  const apiInteractionsDaily = apiMap.interactionsDaily || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/interactions/daily');
  const apiAgents = apiMap.agentsIndex || ((typeof url_root !== 'undefined' ? url_root : '../') + 'api/modules/agents/index');

  const dateInput = document.getElementById('caseDailyDateRange');
  const categorySelect = document.getElementById('caseDailyCategory');
  const agentsSelect = document.getElementById('caseDailyAgents');
  const btnApply = document.getElementById('caseDailyApply');
  const btnReset = document.getElementById('caseDailyReset');
  const tableBody = document.getElementById('caseDailyTableBody');
  const interactionsTableBody = document.getElementById('interactionDailyTableBody');
  const rangeLabel = document.getElementById('caseDailyRangeLabel');
  const alertBox = document.getElementById('caseDailyAlert');

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
      end: formatDate(end || start)
    };
  }

  function updateRangeLabel(start, end) {
    if (!rangeLabel) return;
    rangeLabel.textContent = (start && end) ? `Showing ${start} to ${end}` : '';
  }

  function renderRows(rows) {
    if (!tableBody) return;
    if (!rows.length) {
      tableBody.innerHTML = `
        <tr>
          <td>
            <div class="case-daily-agent">
              <span class="case-daily-agent__avatar">0</span>
              <div>
                <div class="case-daily-agent__name text-muted">No activity</div>
                <div class="case-daily-agent__email">No agents matched this range.</div>
              </div>
            </div>
          </td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
        </tr>
      `;
      return;
    }
    const frag = document.createDocumentFragment();
    rows.forEach((row) => {
      const agentName = row.agent_name || 'Agent';
      const agentEmail = row.agent_email || '';
      const avatarUrl = row.agent_avatar_url || '';
      const initials = String(agentName)
        .trim()
        .split(/\s+/)
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part.charAt(0).toUpperCase())
        .join('') || 'AG';
      const avatarMarkup = avatarUrl
        ? `<img src="${esc(avatarUrl)}" alt="${esc(agentName)}">`
        : esc(initials);
      const wfCount = Number(row.workflow_actions_count || 0);
      const wfBadge = wfCount > 0
        ? `<span class="badge bg-primary-subtle text-primary ms-1" title="passed / returned / dismissed">${wfCount}</span>`
        : `<span class="text-muted">0</span>`;
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>
          <div class="case-daily-agent">
            <span class="case-daily-agent__avatar">${avatarMarkup}</span>
            <div>
              <div class="case-daily-agent__name">${esc(agentName)}</div>
              <div class="case-daily-agent__email">${esc(agentEmail || 'No email')}</div>
            </div>
          </div>
        </td>
        <td class="case-daily-metric">${Number(row.assigned_count || 0)}</td>
        <td class="case-daily-metric">${Number(row.closed_count || 0)}</td>
        <td class="case-daily-metric">${Number(row.pending_count || 0)}</td>
        <td class="case-daily-metric">${wfBadge}</td>
      `;
      frag.appendChild(tr);
    });
    tableBody.innerHTML = '';
    tableBody.appendChild(frag);
  }

  function setLoading() {
    if (!tableBody) return;
    tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Loading...</td></tr>';
  }

  function setInteractionsLoading() {
    if (!interactionsTableBody) return;
    interactionsTableBody.innerHTML = '<tr><td colspan="10" class="text-center text-muted">Loading...</td></tr>';
  }

  function renderInteractionRows(rows) {
    if (!interactionsTableBody) return;
    if (!rows || !rows.length) {
      interactionsTableBody.innerHTML = `
        <tr>
          <td>
            <div class="case-daily-agent">
              <span class="case-daily-agent__avatar">--</span>
              <div>
                <div class="case-daily-agent__name text-muted">No activity</div>
                <div class="case-daily-agent__email">No interactions logged in this range.</div>
              </div>
            </div>
          </td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
          <td class="case-daily-metric">0</td>
        </tr>
      `;
      return;
    }
    const frag = document.createDocumentFragment();
    rows.forEach((row) => {
      const agentName = row.agent_name || 'Agent';
      const agentEmail = row.agent_email || '';
      const avatarUrl = row.agent_avatar_url || '';
      const initials = String(agentName)
        .trim()
        .split(/\s+/)
        .filter(Boolean)
        .slice(0, 2)
        .map((p) => p.charAt(0).toUpperCase())
        .join('') || 'AG';
      const avatarMarkup = avatarUrl
        ? `<img src="${esc(avatarUrl)}" alt="${esc(agentName)}">`
        : esc(initials);
      const followupPending = Number(row.followup_pending || 0);
      const followupBadge = followupPending > 0
        ? `<span class="badge bg-warning-subtle text-warning">${followupPending}</span>`
        : `<span class="text-muted">0</span>`;
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>
          <div class="case-daily-agent">
            <span class="case-daily-agent__avatar">${avatarMarkup}</span>
            <div>
              <div class="case-daily-agent__name">${esc(agentName)}</div>
              <div class="case-daily-agent__email">${esc(agentEmail || 'No email')}</div>
            </div>
          </div>
        </td>
        <td class="case-daily-metric"><strong>${Number(row.total || 0)}</strong></td>
        <td class="case-daily-metric">${Number(row.inbound || 0)}</td>
        <td class="case-daily-metric">${Number(row.outbound || 0)}</td>
        <td class="case-daily-metric">${Number(row.enquiry || 0)}</td>
        <td class="case-daily-metric">${Number(row.information || 0)}</td>
        <td class="case-daily-metric">${Number(row.notice || 0)}</td>
        <td class="case-daily-metric">${Number(row.followup || 0)}</td>
        <td class="case-daily-metric">${Number(row.other || 0)}</td>
        <td class="case-daily-metric">${followupBadge}</td>
      `;
      frag.appendChild(tr);
    });
    interactionsTableBody.innerHTML = '';
    interactionsTableBody.appendChild(frag);
  }

  async function fetchInteractions(start, end, agentIdsCsv) {
    if (!interactionsTableBody) return;
    setInteractionsLoading();
    const params = new URLSearchParams({ start_date: start, end_date: end });
    if (agentIdsCsv) params.set('agent_ids', agentIdsCsv);
    try {
      const response = await fetch(`${apiInteractionsDaily}?${params.toString()}`, { credentials: 'same-origin' });
      const data = await response.json().catch(() => ({}));
      if (!response.ok) {
        throw new Error(data.error || 'Unable to load daily interactions');
      }
      renderInteractionRows(Array.isArray(data.rows) ? data.rows : []);
    } catch (err) {
      interactionsTableBody.innerHTML = `<tr><td colspan="10" class="text-center text-danger">${esc(err.message || 'Failed to load daily interactions.')}</td></tr>`;
    }
  }

  async function fetchData(e) {
    if (e) e.preventDefault();
    showAlert('');
    setLoading();

    const { start, end } = getRangeParams();
    updateRangeLabel(start, end);

    const params = new URLSearchParams({
      start_date: start,
      end_date: end
    });
    if (categorySelect && categorySelect.value) {
      params.set('domain_id', String(categorySelect.value).trim());
    }
    let agentIdsCsv = '';
    if (agentsSelect) {
      const selected = Array.from(agentsSelect.selectedOptions || []).map((opt) => String(opt.value || '').trim()).filter(Boolean);
      if (selected.length) {
        agentIdsCsv = selected.join(',');
        params.set('agent_ids', agentIdsCsv);
      }
    }

    if (btnApply) {
      btnApply.disabled = true;
      btnApply.dataset.originalHtml = btnApply.innerHTML;
      btnApply.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>Loading...';
    }

    // Cases table (existing)
    const casesPromise = (async () => {
      try {
        const response = await fetch(`${apiDaily}?${params.toString()}`, { credentials: 'same-origin' });
        const data = await response.json().catch(() => ({}));
        if (!response.ok) {
          throw new Error(data.error || 'Unable to load daily case performance');
        }
        renderRows(Array.isArray(data.rows) ? data.rows : []);
      } catch (err) {
        showAlert(err.message || 'Unable to load daily case performance');
        if (tableBody) {
          tableBody.innerHTML = '<tr><td colspan="5" class="text-center text-danger">Failed to load daily case performance.</td></tr>';
        }
      }
    })();

    // Interactions table (new): shares date + agents filter, ignores Service Domain
    const interactionsPromise = fetchInteractions(start, end, agentIdsCsv);

    try {
      await Promise.all([casesPromise, interactionsPromise]);
    } finally {
      if (btnApply && btnApply.dataset.originalHtml) {
        btnApply.innerHTML = btnApply.dataset.originalHtml;
        btnApply.disabled = false;
        delete btnApply.dataset.originalHtml;
      }
    }
  }

  function loadAgents() {
    if (!agentsSelect) return;
    if (typeof window.initEnhancedSelect === 'function') {
      window.initEnhancedSelect(agentsSelect);
    }
    fetch(apiAgents, { credentials: 'same-origin' })
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
      .catch(() => {});
  }

  function init() {
    const [start, end] = getDefaultRange();
    if (dateInput && typeof flatpickr !== 'undefined') {
      picker = flatpickr(dateInput, {
        mode: 'range',
        dateFormat: 'Y-m-d',
        defaultDate: [formatDate(start), formatDate(end)]
      });
    }
    loadAgents();
    fetchData();
    if (btnApply) btnApply.addEventListener('click', fetchData);
    if (btnReset) {
      btnReset.addEventListener('click', () => {
        const [s, e] = getDefaultRange();
        if (picker) {
          picker.setDate([s, e], true);
        }
        if (categorySelect) categorySelect.value = '';
        if (agentsSelect) {
          Array.from(agentsSelect.options || []).forEach((opt) => {
            opt.selected = false;
          });
          if (typeof window.refreshEnhancedSelect === 'function') {
            window.refreshEnhancedSelect(agentsSelect);
          }
        }
        fetchData();
      });
    }
  }

  document.addEventListener('DOMContentLoaded', init);
})();
