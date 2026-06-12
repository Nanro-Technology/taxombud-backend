/**
 * DopRealtime — provider-agnostic real-time client.
 *
 * Reads window.REALTIME_CONFIG injected by footer_logged.php:
 *   { provider: 'pusher'|'ably'|'null', enabled: bool,
 *     key: string, cluster?: string, authEndpoint: string,
 *     userId: number, agentId?: number, userName?: string }
 *
 * Usage:
 *   DopRealtime.init(window.REALTIME_CONFIG);
 *   DopRealtime.on('notification:new', function(data) { … });
 *   DopRealtime.off('notification:new', fn);
 *   DopRealtime.isConnected();              // → bool
 *   DopRealtime.getPresence(callback);      // callback([{userId,agentId,name}, …])
 *
 * Fires document-level CustomEvents:
 *   realtime:connected      provider is live
 *   realtime:disconnected   connection dropped → start fallback polling
 *   realtime:failed         permanent failure (wrong key / network error)
 *   presence:join           { userId, agentId, name } joined
 *   presence:leave          { userId, agentId, name } left
 */
(function (global) {
    'use strict';

    var _config    = null;
    var _client    = null;
    var _live      = false;
    var _listeners = {};   // { eventName: [fn, …] }

    // Shared presence channel — all online agents subscribe to this
    var PRESENCE_CHANNEL = 'dop-presence';

    // Internal presence state: map of clientId → memberData
    var _presenceMembers = {};
    var _presenceChannel = null; // Ably channel ref (for getPresence)

    // ── Event bus ─────────────────────────────────────────────────────────────

    function _emit(event, data) {
        var fns = _listeners[event] || [];
        for (var i = 0; i < fns.length; i++) {
            try { fns[i](data); } catch (e) {}
        }
        try {
            document.dispatchEvent(new CustomEvent(event, { bubbles: false, detail: data || {} }));
        } catch (e) {}
    }

    function _setLive(val) {
        _live = val;
        _emit(val ? 'realtime:connected' : 'realtime:disconnected', {});
    }

    function _closeClient() {
        try {
            if (_client && typeof _client.disconnect === 'function') {
                _client.disconnect();
            } else if (_client && typeof _client.close === 'function') {
                _client.close();
            }
        } catch (e) {}
        _client = null;
        _live = false;
        _presenceMembers = {};
        _presenceChannel = null;
    }

    // ── Presence helpers ──────────────────────────────────────────────────────

    function _presenceMemberData(cfg) {
        return { userId: cfg.userId, agentId: cfg.agentId || 0, name: cfg.userName || '' };
    }

    function _applyPresenceUpdate(action, memberId, data) {
        if (action === 'enter' || action === 'update' || action === 'present') {
            _presenceMembers[memberId] = data || {};
        } else if (action === 'leave') {
            delete _presenceMembers[memberId];
            _emit('presence:leave', data || {});
            return;
        }
        if (action === 'enter') _emit('presence:join', data || {});
    }

    // ── Pusher driver ─────────────────────────────────────────────────────────

    function _initPusher(cfg) {
        if (typeof Pusher === 'undefined') {
            _emit('realtime:failed', { reason: 'pusher_sdk_missing' });
            return false;
        }
        try {
            // Build connection options — wsHost/wsPort present means Soketi (self-hosted)
            var pusherOpts = {
                cluster: cfg.cluster || 'eu',
                channelAuthorization: {
                    endpoint: (global.url_root || '../') + cfg.authEndpoint,
                    transport: 'ajax',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                }
            };
            if (cfg.wsHost) {
                pusherOpts.wsHost           = cfg.wsHost;
                pusherOpts.wsPort           = cfg.wsPort || 6001;
                pusherOpts.forceTLS         = !!cfg.forceTLS;
                pusherOpts.disableStats     = true;
                pusherOpts.enabledTransports = ['ws', 'wss'];
            }
            var conn = new Pusher(cfg.key, pusherOpts);
            _client = conn;

            conn.connection.bind('connected',    function () { _setLive(true); });
            conn.connection.bind('disconnected', function () { _setLive(false); });
            conn.connection.bind('unavailable',  function () { _setLive(false); });
            conn.connection.bind('failed',       function () {
                _setLive(false);
                _emit('realtime:failed', { reason: 'pusher_failed' });
            });

            // Private user channel — server-pushed events
            var ch = conn.subscribe('private-user-' + cfg.userId);
            ch.bind_global(function (event, data) {
                if (typeof event === 'string' && event.indexOf('pusher:') === 0) return;
                _emit(event, data);
            });

            // Pusher presence channel — tracks who's online across agents
            // Requires presence-* channel support + server-side auth (same auth endpoint)
            try {
                var presenceCh = conn.subscribe('presence-' + PRESENCE_CHANNEL);
                presenceCh.bind('pusher:subscription_succeeded', function (members) {
                    _presenceMembers = {};
                    members.each(function (m) { _presenceMembers[m.id] = m.info || {}; });
                });
                presenceCh.bind('pusher:member_added',   function (m) { _applyPresenceUpdate('enter',  m.id, m.info); });
                presenceCh.bind('pusher:member_removed', function (m) { _applyPresenceUpdate('leave',  m.id, m.info); });
            } catch (e) { /* presence channel optional — non-fatal */ }

            return true;
        } catch (e) {
            _emit('realtime:failed', { reason: 'pusher_init_error', message: String(e) });
            return false;
        }
    }

    // ── Ably driver ───────────────────────────────────────────────────────────

    function _initAbly(cfg) {
        if (typeof Ably === 'undefined') {
            _emit('realtime:failed', { reason: 'ably_sdk_missing' });
            return false;
        }
        try {
            // Use authUrl so the Ably key never reaches the browser.
            // The server returns a signed TokenRequest scoped to this user's channels.
            var client = new Ably.Realtime({
                authUrl:     (global.url_root || '../') + cfg.authEndpoint,
                authMethod:  'POST',
                authHeaders: { 'X-Requested-With': 'XMLHttpRequest' },
            });
            _client = client;

            client.connection.on('connected',    function () { _setLive(true); });
            client.connection.on('disconnected', function () { _setLive(false); });
            client.connection.on('suspended',    function () { _setLive(false); });
            client.connection.on('failed',       function () {
                _setLive(false);
                _emit('realtime:failed', { reason: 'ably_failed' });
            });

            // Private user channel — server-pushed events
            var ch = client.channels.get('private-user-' + cfg.userId);
            ch.subscribe(function (msg) { _emit(msg.name, msg.data); });

            // Ably presence channel — tracks online agents via WebSocket heartbeat
            // Ably automatically removes members when the connection drops (no 30s ping needed)
            var memberData = _presenceMemberData(cfg);
            var presenceCh = client.channels.get(PRESENCE_CHANNEL);
            _presenceChannel = presenceCh;

            client.connection.on('connected', function () {
                presenceCh.presence.enter(memberData, function (err) {
                    if (err) return; // non-fatal
                    // Snapshot current members
                    presenceCh.presence.get(function (err2, members) {
                        if (err2 || !Array.isArray(members)) return;
                        _presenceMembers = {};
                        members.forEach(function (m) { _presenceMembers[m.clientId] = m.data || {}; });
                    });
                });
            });

            presenceCh.presence.subscribe('enter',  function (m) { _applyPresenceUpdate('enter',  m.clientId, m.data); });
            presenceCh.presence.subscribe('leave',  function (m) { _applyPresenceUpdate('leave',  m.clientId, m.data); });
            presenceCh.presence.subscribe('update', function (m) { _applyPresenceUpdate('update', m.clientId, m.data); });

            return true;
        } catch (e) {
            _emit('realtime:failed', { reason: 'ably_init_error', message: String(e) });
            return false;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    var DopRealtime = {
        init: function (cfg) {
            _config = cfg || {};
            var provider = String(_config.provider || 'null').toLowerCase();
            if (!_config.enabled || !_config.userId) {
                return false;
            }
            if (provider === 'pusher' && !_config.key) {
                _emit('realtime:failed', { reason: 'missing_key', provider: provider });
                return false;
            }
            var ok = false;
            if      (provider === 'pusher') ok = _initPusher(_config);
            else if (provider === 'ably')   ok = _initAbly(_config);
            if (!ok) {
                _emit('realtime:failed', { reason: 'init_failed', provider: provider });
            }
            return ok;
        },

        retry: function () {
            if (!_config) return false;
            _closeClient();
            _emit('realtime:disconnected', { reason: 'manual_retry' });
            return DopRealtime.init(_config);
        },

        on: function (event, fn) {
            if (typeof fn !== 'function') return;
            if (!_listeners[event]) _listeners[event] = [];
            _listeners[event].push(fn);
        },

        off: function (event, fn) {
            if (!_listeners[event]) return;
            _listeners[event] = _listeners[event].filter(function (f) { return f !== fn; });
        },

        isConnected: function () { return _live; },

        /**
         * Get current presence members.
         * callback([{ userId, agentId, name }, …])
         * Works from local cache — no extra HTTP hit.
         */
        getPresence: function (callback) {
            if (typeof callback !== 'function') return;
            var list = Object.keys(_presenceMembers).map(function (k) { return _presenceMembers[k]; });
            callback(list);
        }
    };

    global.DopRealtime = DopRealtime;

})(window);
