
    (function() {
        const el = document.getElementById('globalLoadingModal');
        if (!el || typeof bootstrap === 'undefined') return;
        const modal = new bootstrap.Modal(el, {
            backdrop: 'static',
            keyboard: false
        });
        const textEl = document.getElementById('globalLoadingText');
        window.showGlobalLoading = function(msg) {
            if (textEl) textEl.textContent = msg || 'Please wait...';
            modal.show();
        };
        window.hideGlobalLoading = function() {
            modal.hide();
        };
    })();
   
   // Helper: click targets with data-show-loading or .show-loading will open the global loading modal
    (function() {
        window.addEventListener('DOMContentLoaded', () => {
            document.body.addEventListener('click', (ev) => {
                const target = ev.target.closest('[data-show-loading], .show-loading');
                if (!target) return;
                if (typeof window.showGlobalLoading === 'function') {
                    window.showGlobalLoading(target.dataset.loadingText || 'Please wait...');
                }
            });
        });
    })();

    // Trigger custom loader (#loader) when sidebar/menu links are clicked
    (function() {
        const loaderEl = document.getElementById('loader');
        let skipLoading = false;
        let navLoaderTimer = null;
        const showCustomLoader = () => {
            if (skipLoading) return;
            if (typeof window.setGlobalUiLoading === 'function') {
                window.setGlobalUiLoading('nav-transition', true, 12000);
            } else if (loaderEl) {
                loaderEl.style.display = 'flex';
                loaderEl.style.pointerEvents = 'none';
            }
            if (navLoaderTimer) clearTimeout(navLoaderTimer);
            navLoaderTimer = setTimeout(() => {
                if (typeof window.setGlobalUiLoading === 'function') {
                    window.setGlobalUiLoading('nav-transition', false);
                } else if (loaderEl) {
                    loaderEl.style.display = 'none';
                }
                navLoaderTimer = null;
            }, 12000);
        };
        const shouldIgnore = (a) => {
            const href = (a.getAttribute('href') || '').trim();
            return (
                a.dataset.bsToggle ||
                a.dataset.bsTarget ||
                href === '' ||
                href.startsWith('#') ||
                href.startsWith('javascript:')
            );
        };
        window.addEventListener('DOMContentLoaded', () => {
            if (typeof window.setGlobalUiLoading === 'function') {
                window.setGlobalUiLoading('nav-transition', false);
            } else if (loaderEl) {
                loaderEl.style.display = 'none';
            }
            const links = document.querySelectorAll('#sidebar-menu a, .app-menu a, .vertical-menu a');
            links.forEach((a) => {
                a.addEventListener('click', () => {
                    if (shouldIgnore(a)) return;
                    showCustomLoader();
                });
            });
            // Export buttons: skip loader
            document.querySelectorAll('[id$="ExportBtn"]').forEach((btn) => {
                btn.addEventListener('click', () => {
                    skipLoading = true;
                });
            });

            // Show loader on any unload/navigation (refresh/back/forward)
            window.addEventListener('beforeunload', () => {
                if (skipLoading) return;
                if (typeof window.showGlobalLoading === 'function') {
                    window.showGlobalLoading('Loading...');
                } else {
                    showCustomLoader();
                }
            });

            // If page is restored from BFCache, ensure loader is hidden
            window.addEventListener('pageshow', (ev) => {
                if (typeof window.setGlobalUiLoading === 'function') {
                    window.setGlobalUiLoading('nav-transition', false);
                } else if (ev.persisted && loaderEl) {
                    loaderEl.style.display = 'none';
                }
            });
        });
    })();
    
    // Call Script overlay
    (function() {
        const overlay = document.getElementById('callScriptOverlay');
        const btnClose = document.getElementById('callScriptClose');
        const btnHide = document.getElementById('callScriptHide');
        const btnFloat = document.getElementById('callScriptShowBtn');
        const triggers = document.querySelectorAll('.call-script-show-trigger');
        const sessionActive = window.callScriptSessionActive === true;

        if (!overlay) return;

        const prefKey = 'callScriptVisible';
        const showOverlay = () => {
            overlay.classList.remove('d-none');
            requestAnimationFrame(() => overlay.classList.add('call-script-open'));
            localStorage.setItem(prefKey, 'visible');
        };
        const hideOverlay = () => {
            overlay.classList.remove('call-script-open');
            setTimeout(() => {
                overlay.classList.add('d-none');
            }, 250);
            localStorage.setItem(prefKey, 'hidden');
        };

        // expose for debugging or manual trigger
        window.showCallScript = showOverlay;
        window.hideCallScript = hideOverlay;

        overlay.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });
        const hideButton = () => {
            if (btnFloat) btnFloat.classList.add('d-none');
        };
        const showButton = () => {
            if (btnFloat) btnFloat.classList.remove('d-none');
        };

        if (btnFloat) {
            btnFloat.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                showOverlay();
                hideButton();
            });
        }
        triggers.forEach((btn) => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                showOverlay();
                hideButton();
            });
        });
        document.addEventListener('click', (e) => {
            const btn = e.target.closest('.call-script-activate-trigger');
            if (!btn) return;
            e.preventDefault();
            e.stopPropagation();
            fetch('_php/call_script?call_script=on', {method:'GET', credentials:'same-origin'}).catch(()=>{});
            showOverlay();
            hideButton();
        });
        if (btnClose) btnClose.addEventListener('click', (e) => {
            e.preventDefault();
            localStorage.setItem(prefKey, 'hidden');
            hideOverlay();
            showButton();
        });
        if (btnHide) btnHide.addEventListener('click', (e) => {
            e.preventDefault();
            hideOverlay();
            // Do not show the floating trigger; require a new incoming call/session to re-enable
            hideButton();
            fetch('_php/call_script?call_script=off', {method:'GET', credentials:'same-origin'}).catch(()=>{});
        });

        const pref = localStorage.getItem(prefKey);
        if (pref === 'hidden') {
            hideOverlay();
            showButton();
        } else if (sessionActive) {
            showOverlay();
            hideButton();
        } else {
            hideOverlay();
            showButton();
        }
    })();
 
// ── Notification hub ─────────────────────────────────────────────────────────
// Uses DopRealtime when connected; falls back to polling only on disconnect.
(function () {
  const cfg          = window.appConfig || {};
  const fallbackMs   = cfg.notificationsFallbackRefreshMs || 900000; // 15 min safety net when live
  const pollOnlyMs   = cfg.notificationsRefreshMs         || 180000; // 3 min when no realtime
  const lastKey      = 'notifications_last_unread';
  const lastIdKey    = 'notifications_last_id';
  let lastNotifId    = null;
  let latestUnread   = 0;
  let pollTimer      = null;
  let pollInterval   = pollOnlyMs; // set properly after init
  let audio;
  let audioReady     = false;

  function notifApiBase() {
    return (window.url_root || '../') + 'api/modules/notifications/index';
  }
  function dispatchEv(name, detail) {
    window.dispatchEvent(new CustomEvent(name, { detail: detail || {} }));
  }
  function applyUnreadCount(count) {
    latestUnread = Math.max(0, parseInt(count, 10) || 0);
    const badge = document.querySelector('.notification-badge, #notification-badge, #notif-badge');
    if (badge) {
      badge.textContent = latestUnread > 99 ? '99+' : (latestUnread || '');
      badge.style.display = latestUnread ? 'inline-block' : 'none';
      badge.classList.toggle('d-none', !latestUnread);
    }
    try { localStorage.setItem(lastKey, String(latestUnread)); } catch (e) {}
  }
  function ensureAudio() {
    if (audio) return audio;
    audio = new Audio((window.url_root || '../') + 'assets/vendor/sound.wav');
    audio.volume = 0.35;
    return audio;
  }
  function enableAudio() {
    audioReady = true;
    const a = ensureAudio();
    const v = a.volume; a.volume = 0;
    a.play().then(() => { a.pause(); a.currentTime = 0; a.volume = v; }).catch(() => {});
  }

  // ── Realtime event handlers (called by DopRealtime and by the poll path) ──
  function handleRealtimeCreated(payload) {
    const data   = payload || {};
    const notif  = data.notification || null;
    const nid    = notif && notif.id ? parseInt(notif.id, 10) : null;
    if (nid !== null) {
      if (lastNotifId !== null && nid > lastNotifId && audioReady) {
        ensureAudio().play().catch(() => {});
      }
      lastNotifId = nid;
      try { localStorage.setItem(lastIdKey, String(nid)); } catch (e) {}
    }
    const next = typeof data.unread_count !== 'undefined'
      ? parseInt(data.unread_count, 10) : (latestUnread + 1);
    applyUnreadCount(next);
    dispatchEv('crm:notification-created', {
      notification: notif,
      unread_count: latestUnread,
      action: data.action || 'created',
    });
  }
  function handleRealtimeState(payload) {
    const data = payload || {};
    if (typeof data.unread_count !== 'undefined') {
      applyUnreadCount(data.unread_count);
    }
    dispatchEv('crm:notification-state', {
      action: data.action || 'refresh',
      ids: Array.isArray(data.ids) ? data.ids : [],
      unread_count: latestUnread,
    });
  }

  // ── Fallback polling (only runs when DopRealtime is not connected) ─────────
  async function pollNotif() {
    if (document.visibilityState === 'hidden') {
      pollTimer = setTimeout(pollNotif, pollInterval);
      return;
    }
    try {
      const res  = await fetch(notifApiBase() + '?limit=1', { credentials: 'same-origin' });
      const data = await res.json().catch(() => ({}));
      applyUnreadCount(data.unread_count || 0);
      const newest  = Array.isArray(data.data) && data.data.length ? data.data[0] : null;
      const newestId = newest && newest.id ? parseInt(newest.id, 10) : null;
      if (newestId !== null) {
        if (lastNotifId !== null && newestId > lastNotifId && latestUnread > 0 && audioReady) {
          ensureAudio().play().catch(() => {});
        }
        lastNotifId = newestId;
        try { localStorage.setItem(lastIdKey, String(newestId)); } catch (e) {}
      }
    } catch (e) {}
    pollTimer = setTimeout(pollNotif, pollInterval);
  }
  function startPoll(ms) {
    clearTimeout(pollTimer);
    pollTimer   = null;
    pollInterval = ms || pollOnlyMs;
    pollNotif();
  }
  function stopPoll() {
    clearTimeout(pollTimer);
    pollTimer = null;
  }

  // ── DopRealtime wiring ─────────────────────────────────────────────────────
  const rt = window.DopRealtime;
  if (rt) {
    rt.on('notification:new',   handleRealtimeCreated);
    rt.on('notification:state', handleRealtimeState);

    // When live: stop poll; resume on disconnect as slow safety net
    document.addEventListener('realtime:connected',    function () { stopPoll(); });
    document.addEventListener('realtime:disconnected', function () { startPoll(fallbackMs); });
    document.addEventListener('realtime:failed',       function () { startPoll(pollOnlyMs); });

    // Grace period: if still not connected after 3 s, start poll as fallback
    setTimeout(function () {
      if (!rt.isConnected()) { startPoll(pollOnlyMs); }
    }, 3000);
  } else {
    // No realtime at all — classic polling
    startPoll(pollOnlyMs);
  }

  // Resume immediately on tab focus
  document.addEventListener('visibilitychange', function () {
    if (document.visibilityState !== 'visible') return;
    if (rt && rt.isConnected()) return; // realtime is live, no poll needed
    clearTimeout(pollTimer);
    pollNotif();
  });

  // ── Restore state from localStorage ───────────────────────────────────────
  try {
    const c = localStorage.getItem(lastKey);
    if (c) { const n = parseInt(c, 10); if (!isNaN(n)) applyUnreadCount(n); }
    const ci = localStorage.getItem(lastIdKey);
    if (ci) { const n = parseInt(ci, 10); if (!isNaN(n)) lastNotifId = n; }
  } catch (e) {}

  // ── Mark-all-read button ───────────────────────────────────────────────────
  (function bindMarkRead() {
    const btn = document.getElementById('page-header-notifications-dropdown');
    if (!btn || btn.dataset.notifBound === '1') return;
    btn.dataset.notifBound = '1';
    btn.addEventListener('click', function () {
      fetch(notifApiBase(), {
        method: 'POST',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mark_all: true }),
      }).catch(() => {});
      handleRealtimeState({ action: 'mark_all', unread_count: 0 });
    });
  })();

  document.addEventListener('click',   enableAudio, { once: true });
  document.addEventListener('keydown', enableAudio, { once: true });
})();

// ── Secured Filing inbox sidebar badge ───────────────────────────────────────
// Uses DopRealtime sf:badge event when connected; polls only as fallback.
(function () {
  const badgeEl = document.querySelector('[data-sf-inbox-badge]');
  if (!badgeEl) return;

  const apiBase    = (window.url_root || '../') + 'api/modules/secured-filing/inbox';
  const pollMs     = cfg.securedFilingRefreshMs || 900000; // 15 min fallback
  let   pollTimer  = null;

  function applyCount(count) {
    count = parseInt(count, 10) || 0;
    badgeEl.textContent  = count > 0 ? (count > 99 ? '99+' : count) : '';
    badgeEl.style.display = count > 0 ? 'inline-block' : 'none';
  }

  function fetchCount() {
    if (document.visibilityState === 'hidden') { schedulePoll(); return; }
    fetch(apiBase + '?tab=counts', {
      credentials: 'same-origin',
      headers: { 'X-Requested-With': 'XMLHttpRequest' },
    })
      .then(function (r) { return r.ok ? r.json() : Promise.reject(); })
      .then(function (d) { applyCount((d.counts && d.counts.to_acknowledge) || 0); })
      .catch(function () {})
      .finally(schedulePoll);
  }
  function schedulePoll() { pollTimer = setTimeout(fetchCount, pollMs); }
  function stopPoll()     { clearTimeout(pollTimer); pollTimer = null; }

  const rt = window.DopRealtime;
  if (rt) {
    rt.on('sf:badge', function (data) {
      if (typeof data.to_acknowledge !== 'undefined') {
        applyCount(data.to_acknowledge);
      } else {
        fetchCount(); // server pushed refresh signal — do a lightweight fetch
      }
    });
    document.addEventListener('realtime:connected',    stopPoll);
    document.addEventListener('realtime:disconnected', function () { fetchCount(); });
    document.addEventListener('realtime:failed',       function () { fetchCount(); });
    setTimeout(function () { if (!rt.isConnected()) fetchCount(); }, 3000);
  } else {
    fetchCount();
  }

  document.addEventListener('visibilitychange', function () {
    if (document.visibilityState !== 'visible') return;
    if (rt && rt.isConnected()) return;
    clearTimeout(pollTimer);
    fetchCount();
  });
})();
 
