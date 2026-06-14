/* eslint-disable */
(function () {
  "use strict";

  const apiMap = (typeof apiEndpoints === "function") ? apiEndpoints() : {};
  const apiHealth =
    apiMap.accountsHealth || ((typeof url_root !== "undefined" ? url_root : "../") + "api/modules/accounts/health");

  const avgEl = document.getElementById("accountHealthAvg");
  const riskEl = document.getElementById("accountHealthAtRisk");
  const totalEl = document.getElementById("accountHealthTotal");

  function badgeClass(score) {
    if (score >= 70) return "bg-success-subtle text-success";
    if (score >= 40) return "bg-warning-subtle text-warning";
    return "bg-danger-subtle text-danger";
  }

  function applyBadge(el, score) {
    if (!el) return;
    const s = Number(score || 0);
    el.textContent = s.toString();
    el.classList.remove("bg-success-subtle", "text-success", "bg-warning-subtle", "text-warning", "bg-danger-subtle", "text-danger", "bg-light", "text-muted");
    el.classList.add(...badgeClass(s).split(" "));
  }

  function loadBadges() {
    const badges = Array.from(document.querySelectorAll("[data-account-health]"));
    if (!badges.length) return;
    const ids = Array.from(new Set(badges.map((b) => b.dataset.accountHealth).filter(Boolean)));
    if (!ids.length) return;
    fetch(apiHealth + "?ids=" + encodeURIComponent(ids.join(",")))
      .then((r) => r.json())
      .then((data) => {
        const list = Array.isArray(data?.data) ? data.data : [];
        const map = {};
        list.forEach((row) => {
          map[String(row.account_id)] = row;
        });
        badges.forEach((badge) => {
          const row = map[String(badge.dataset.accountHealth)];
          if (!row) {
            badge.textContent = "0";
            badge.classList.add("bg-light", "text-muted");
            return;
          }
          applyBadge(badge, row.score);
          badge.title = `Open cases: ${row.open_cases || 0}${row.csat_avg ? " | CSAT: " + row.csat_avg : ""}`;
        });
      })
      .catch(() => {});
  }

  function loadSummary() {
    if (!avgEl && !riskEl && !totalEl) return;
    fetch(apiHealth + "?summary=1")
      .then((r) => r.json())
      .then((data) => {
        const s = data?.summary || {};
        if (avgEl) avgEl.textContent = (s.avg_score ?? 0).toString();
        if (riskEl) riskEl.textContent = (s.at_risk ?? 0).toString();
        if (totalEl) totalEl.textContent = (s.total ?? 0).toString();
      })
      .catch(() => {});
  }

  window.refreshAccountHealth = loadBadges;

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => {
      loadBadges();
      loadSummary();
    });
  } else {
    loadBadges();
    loadSummary();
  }
})();
