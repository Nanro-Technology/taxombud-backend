// Shared client config (URL root + loading assets)
(function() {
  let base = (typeof window !== 'undefined' && window.url_root) ? window.url_root : '';
  if (!base) {
    const loc = window.location;
    base = loc.origin + '/';
  }
  if (!base.endsWith('/')) base += '/';
  window.url_root = base;

  const loadingImg = `${base}assets/images/loading.gif`;
  window.loading_combiner = `<img src="${loadingImg}" style="width:50px;" />`;
  window.load_horizontal = window.loading_combiner;
})();

window.appConfig = window.appConfig || {
  mailboxRefreshMs: 180000, // 3 minutes
  chatRefreshMs: 60000, // 1 minute
  chatRefreshHiddenMs: 300000, // 5 minutes
  chatRefreshRetryMs: 300000, // 5 minutes after repeated failures
  omniInboxRefreshMs: 120000, // 2 minutes
  omniContactRefreshMs: 120000, // 2 minutes
  notificationsRefreshMs: 180000, // 3 minutes
  notificationsFallbackRefreshMs: 900000, // 15 minutes
  securedFilingRefreshMs: 900000 // 15 minutes
};


function isValidEmail(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email);
}
