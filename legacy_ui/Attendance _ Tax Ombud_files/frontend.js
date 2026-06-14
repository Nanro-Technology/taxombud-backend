// Page-load overlay helper.
// Call crmPageLoader.show() to keep the #preloader visible during an initial AJAX fetch.
// Call crmPageLoader.done() when the fetch resolves (or rejects).
// Subsequent done() calls are no-ops until show() is called again.
window.crmPageLoader = (function () {
    var _active = false;
    var _timer  = null;

    function show() {
        if (_timer) clearTimeout(_timer);
        _active = true;
        var el = document.getElementById('preloader');
        if (el) {
            el.style.opacity = '';
            el.style.visibility = '';
            el.style.backgroundColor = 'rgba(255,255,255,0.3)';
        }
        document.documentElement.setAttribute('data-preloader', 'enable');
        _timer = setTimeout(done, 12000);
    }

    function done() {
        if (!_active) return;
        if (_timer) { clearTimeout(_timer); _timer = null; }
        _active = false;
        var el = document.getElementById('preloader');
        if (el) el.style.backgroundColor = '';
        document.documentElement.setAttribute('data-preloader', 'disable');
    }

    return { show: show, done: done };
})();

$('#password2, #password1').on('keyup', function (i) {
    if ($('#password2').val().length != 0 && $('#password1').val().length != 0) {
        pass2val = $('#password2').val();
        pass1val = $('#password1').val();
        $('#password1, #password2').css('border', '2px solid red');
        if (pass2val == pass1val) {
            $('#password1, #password2').css('border', '2px solid #d3e0f3');
        }
    }
});

// Unified Bootstrap/SweetAlert-style alert+confirm helpers (no native alert/confirm).
(function initCrmUiAlert() {
    if (typeof window.crmUiAlert === 'function') return;
    window.crmUiAlert = function (message, title, opts) {
        var config = {};
        if (message && typeof message === 'object' && !Array.isArray(message)) {
            config = Object.assign({}, message);
        } else {
            config = Object.assign({}, opts || {}, {
                message: message,
                title: title
            });
        }

        var type = String(config.type || config.variant || config.status || 'info').toLowerCase();
        if (['danger', 'error', 'failed', 'fail'].indexOf(type) !== -1) type = 'danger';
        else if (['warn'].indexOf(type) !== -1) type = 'warning';
        else if (['ok', 'saved'].indexOf(type) !== -1) type = 'success';
        else if (['primary', 'notice'].indexOf(type) !== -1) type = 'info';

        var msg = String(config.message || config.text || 'Notice');
        var ttl = String(config.title || (type === 'success' ? 'Saved' : type === 'danger' ? 'Error' : type === 'warning' ? 'Warning' : 'Notice'));
        var okText = String(config.okText || 'OK');
        var bodyHtml = typeof config.bodyHtml === 'string' ? config.bodyHtml : '';
        var stackedBackdrop = !!config.stackedBackdrop;

        if (type === 'success' && typeof window.showSavedModal === 'function' && !bodyHtml) {
            window.showSavedModal(ttl, msg);
            return;
        }

        var metaMap = {
            success: {
                iconClass: 'ri-checkbox-circle-fill',
                iconWrap: 'bg-primary-subtle text-primary',
                buttonClass: 'btn-primary'
            },
            danger: {
                iconClass: 'ri-close-circle-fill',
                iconWrap: 'bg-danger-subtle text-danger',
                buttonClass: 'btn-danger'
            },
            warning: {
                iconClass: 'ri-error-warning-fill',
                iconWrap: 'bg-warning-subtle text-warning',
                buttonClass: 'btn-warning'
            },
            info: {
                iconClass: 'ri-information-fill',
                iconWrap: 'bg-info-subtle text-info',
                buttonClass: 'btn-info'
            }
        };
        var meta = metaMap[type] || metaMap.info;

        if (typeof bootstrap === 'undefined') {
            if (typeof window.Swal !== 'undefined' && typeof window.Swal.fire === 'function') {
                window.Swal.fire({
                    icon: type === 'danger' ? 'error' : (type === 'warning' ? 'warning' : (type === 'success' ? 'success' : 'info')),
                    title: ttl,
                    text: msg,
                    confirmButtonText: okText
                });
                return;
            }
            var fallbackId = 'crmUiAlertFallback';
            var fb = document.getElementById(fallbackId);
            if (!fb) {
                fb = document.createElement('div');
                fb.id = fallbackId;
                fb.style.position = 'fixed';
                fb.style.inset = '0';
                fb.style.zIndex = '99999';
                fb.style.background = 'rgba(0,0,0,0.35)';
                fb.style.display = 'flex';
                fb.style.alignItems = 'center';
                fb.style.justifyContent = 'center';
                fb.innerHTML = '' +
                    '<div style="background:#fff;max-width:420px;width:92%;border-radius:10px;padding:16px 16px 12px;font-family:inherit;">' +
                    '  <div style="display:flex;align-items:center;gap:10px;margin-bottom:10px;">' +
                    '    <div id="crmUiAlertFallbackIcon" style="width:42px;height:42px;border-radius:999px;display:flex;align-items:center;justify-content:center;font-size:20px;background:#eff6ff;color:#2563eb;">i</div>' +
                    '    <div id="crmUiAlertFallbackTitle" style="font-weight:600;"></div>' +
                    '  </div>' +
                    '  <div id="crmUiAlertFallbackMsg" style="font-size:14px;color:#334155;margin-bottom:12px;"></div>' +
                    '  <div style="text-align:right;"><button id="crmUiAlertFallbackOk" type="button" style="border:0;background:#0d6efd;color:#fff;border-radius:6px;padding:7px 12px;">OK</button></div>' +
                    '</div>';
                document.body.appendChild(fb);
                fb.querySelector('#crmUiAlertFallbackOk').addEventListener('click', function () {
                    fb.style.display = 'none';
                });
            }
            var tEl = document.getElementById('crmUiAlertFallbackTitle');
            var mEl = document.getElementById('crmUiAlertFallbackMsg');
            var iEl = document.getElementById('crmUiAlertFallbackIcon');
            var okFallback = document.getElementById('crmUiAlertFallbackOk');
            if (tEl) tEl.textContent = ttl;
            if (mEl) mEl.textContent = msg;
            if (iEl) {
                iEl.textContent = type === 'danger' ? '!' : (type === 'warning' ? '!' : (type === 'success' ? '✓' : 'i'));
                iEl.style.background = type === 'danger' ? '#fee2e2' : (type === 'warning' ? '#fef3c7' : (type === 'success' ? '#dbeafe' : '#e0f2fe'));
                iEl.style.color = type === 'danger' ? '#dc2626' : (type === 'warning' ? '#d97706' : (type === 'success' ? '#2563eb' : '#0891b2'));
            }
            if (okFallback) okFallback.textContent = okText;
            fb.style.display = 'flex';
            return;
        }
        var modalId = 'crmUiAlertModal';
        var el = document.getElementById(modalId);
        if (!el) {
            el = document.createElement('div');
            el.className = 'modal fade';
            el.id = modalId;
            el.tabIndex = -1;
            el.setAttribute('aria-hidden', 'true');
            el.innerHTML = '' +
                '<div class="modal-dialog modal-dialog-centered">' +
                '  <div class="modal-content">' +
                '    <div class="modal-body text-center p-4">' +
                '      <div class="avatar-md mx-auto mb-3">' +
                '        <div class="avatar-title rounded-circle fs-1 crm-alert-icon-wrap">' +
                '          <i class="crm-alert-icon"></i>' +
                '        </div>' +
                '      </div>' +
                '      <h5 class="mb-1 modal-title"></h5>' +
                '      <div class="crm-alert-msg text-muted mb-3"></div>' +
                '      <button type="button" class="btn w-100 crm-alert-ok" data-bs-dismiss="modal">OK</button>' +
                '    </div>' +
                '  </div>' +
                '</div>';
            document.body.appendChild(el);
        }
        var titleEl = el.querySelector('.modal-title');
        var bodyEl = el.querySelector('.modal-body .crm-alert-msg');
        var iconWrapEl = el.querySelector('.crm-alert-icon-wrap');
        var iconEl = el.querySelector('.crm-alert-icon');
        var okBtn = el.querySelector('.crm-alert-ok');
        if (titleEl) titleEl.textContent = ttl;
        if (bodyEl) {
            if (bodyHtml) {
                bodyEl.innerHTML = bodyHtml;
            } else {
                bodyEl.textContent = msg;
            }
        }
        if (iconWrapEl) iconWrapEl.className = 'avatar-title rounded-circle fs-1 crm-alert-icon-wrap ' + meta.iconWrap;
        if (iconEl) iconEl.className = meta.iconClass + ' crm-alert-icon';
        if (okBtn) {
            okBtn.className = 'btn w-100 crm-alert-ok ' + meta.buttonClass;
            okBtn.textContent = okText;
        }
        var modal = bootstrap.Modal.getOrCreateInstance(el);
        if (stackedBackdrop) {
            var onShown = function () {
                var openModals = Array.from(document.querySelectorAll('.modal.show'));
                var maxZ = 1050;
                openModals.forEach(function (openModal) {
                    var z = parseInt(window.getComputedStyle(openModal).zIndex || '0', 10);
                    if (!isNaN(z) && z > maxZ) {
                        maxZ = z;
                    }
                });
                var zIndex = maxZ + 10;
                el.style.zIndex = String(zIndex);
                window.setTimeout(function () {
                    var backdrops = document.querySelectorAll('.modal-backdrop');
                    var backdrop = backdrops.length ? backdrops[backdrops.length - 1] : null;
                    if (backdrop) {
                        backdrop.style.zIndex = String(zIndex - 5);
                        backdrop.style.opacity = '0.72';
                    }
                }, 0);
                el.removeEventListener('shown.bs.modal', onShown);
            };
            el.addEventListener('shown.bs.modal', onShown);
        }
        modal.show();
    };
})();

(function initCrmUiConfirm() {
    if (typeof window.crmUiConfirm === 'function') return;
    window.crmUiConfirm = function (message, title, opts) {
        var msg = String(message || 'Are you sure?');
        var ttl = String(title || 'Confirm');
        var o = opts || {};
        var okText = String(o.okText || 'Yes');
        var cancelText = String(o.cancelText || 'Cancel');
        var variant = String(o.variant || 'primary');
        var bodyHtml = typeof o.bodyHtml === 'string' ? o.bodyHtml : '';

        if (typeof bootstrap === 'undefined') {
            if (typeof window.Swal !== 'undefined' && typeof window.Swal.fire === 'function') {
                return window.Swal.fire({
                    icon: o.icon || 'warning',
                    title: ttl,
                    text: msg,
                    showCancelButton: true,
                    confirmButtonText: okText,
                    cancelButtonText: cancelText,
                    reverseButtons: true
                }).then(function (r) { return !!(r && r.isConfirmed); });
            }
            return new Promise(function (resolve) {
                var fallbackId = 'crmUiConfirmFallback';
                var fb = document.getElementById(fallbackId);
                if (!fb) {
                    fb = document.createElement('div');
                    fb.id = fallbackId;
                    fb.style.position = 'fixed';
                    fb.style.inset = '0';
                    fb.style.zIndex = '99999';
                    fb.style.background = 'rgba(0,0,0,0.35)';
                    fb.style.display = 'none';
                    fb.style.alignItems = 'center';
                    fb.style.justifyContent = 'center';
                    fb.innerHTML = '' +
                        '<div style="background:#fff;max-width:440px;width:92%;border-radius:10px;padding:16px 16px 12px;font-family:inherit;">' +
                        '  <div id="crmUiConfirmFallbackTitle" style="font-weight:600;margin-bottom:8px;"></div>' +
                        '  <div id="crmUiConfirmFallbackMsg" style="font-size:15px;line-height:1.55;color:#334155;margin-bottom:12px;"></div>' +
                        '  <div style="text-align:right;display:flex;justify-content:flex-end;gap:8px;">' +
                        '    <button id="crmUiConfirmFallbackCancel" type="button" style="border:1px solid #cbd5e1;background:#fff;color:#0f172a;border-radius:6px;padding:7px 12px;">Cancel</button>' +
                        '    <button id="crmUiConfirmFallbackOk" type="button" style="border:0;background:#0d6efd;color:#fff;border-radius:6px;padding:7px 12px;">Yes</button>' +
                        '  </div>' +
                        '</div>';
                    document.body.appendChild(fb);
                }
                var tEl = document.getElementById('crmUiConfirmFallbackTitle');
                var mEl = document.getElementById('crmUiConfirmFallbackMsg');
                var okEl = document.getElementById('crmUiConfirmFallbackOk');
                var cancelEl = document.getElementById('crmUiConfirmFallbackCancel');
                if (tEl) tEl.textContent = ttl;
                if (mEl) mEl.textContent = msg;
                if (okEl) okEl.textContent = okText;
                if (cancelEl) cancelEl.textContent = cancelText;
                fb.style.display = 'flex';

                var done = function (val) {
                    fb.style.display = 'none';
                    resolve(!!val);
                };
                if (okEl) okEl.onclick = function () { done(true); };
                if (cancelEl) cancelEl.onclick = function () { done(false); };
                fb.onclick = function (e) { if (e.target === fb) done(false); };
            });
        }

        return new Promise(function (resolve) {
            var modalId = 'crmUiConfirmModal';
            var el = document.getElementById(modalId);
            if (!el) {
                el = document.createElement('div');
                el.className = 'modal fade';
                el.id = modalId;
                el.tabIndex = -1;
                el.setAttribute('aria-hidden', 'true');
                el.innerHTML = '' +
                    '<div class="modal-dialog modal-dialog-centered">' +
                    '  <div class="modal-content">' +
                    '    <div class="modal-header">' +
                    '      <h5 class="modal-title"></h5>' +
                    '      <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>' +
                    '    </div>' +
                    '    <div class="modal-body"><div class="crm-confirm-msg"></div></div>' +
                    '    <div class="modal-footer">' +
                    '      <button type="button" class="btn btn-light crm-ui-cancel" data-bs-dismiss="modal">Cancel</button>' +
                    '      <button type="button" class="btn crm-ui-ok">Yes</button>' +
                    '    </div>' +
                    '  </div>' +
                    '</div>';
                document.body.appendChild(el);
            }
            var titleEl = el.querySelector('.modal-title');
            var bodyEl = el.querySelector('.modal-body .crm-confirm-msg');
            var okBtn = el.querySelector('.crm-ui-ok');
            var cancelBtn = el.querySelector('.crm-ui-cancel');
            var stackedBackdrop = !!(opts && opts.stackedBackdrop);
            if (titleEl) titleEl.textContent = ttl;
            if (bodyEl) {
                if (bodyHtml) {
                    bodyEl.innerHTML = bodyHtml;
                } else {
                    bodyEl.textContent = msg;
                }
                bodyEl.style.fontSize = '1rem';
                bodyEl.style.lineHeight = '1.55';
                bodyEl.style.color = '#334155';
            }
            if (okBtn) {
                okBtn.textContent = okText;
                okBtn.className = 'btn crm-ui-ok btn-' + variant;
            }
            if (cancelBtn) cancelBtn.textContent = cancelText;

            var modal = bootstrap.Modal.getOrCreateInstance(el);
            var settled = false;
            var onShown = null;
            var cleanup = function () {
                el.removeEventListener('hidden.bs.modal', onHidden);
                if (onShown) {
                    el.removeEventListener('shown.bs.modal', onShown);
                    onShown = null;
                }
                if (okBtn) okBtn.onclick = null;
                if (cancelBtn) cancelBtn.onclick = null;
            };
            var finish = function (val) {
                if (settled) return;
                settled = true;
                cleanup();
                resolve(!!val);
            };
            var onHidden = function () { finish(false); };
            if (stackedBackdrop) {
                onShown = function () {
                    var openModals = Array.from(document.querySelectorAll('.modal.show'));
                    var maxZ = 1050;
                    openModals.forEach(function (openModal) {
                        var z = parseInt(window.getComputedStyle(openModal).zIndex || '0', 10);
                        if (!isNaN(z) && z > maxZ) {
                            maxZ = z;
                        }
                    });
                    var zIndex = maxZ + 10;
                    el.style.zIndex = String(zIndex);
                    window.setTimeout(function () {
                        var backdrops = document.querySelectorAll('.modal-backdrop');
                        var backdrop = backdrops.length ? backdrops[backdrops.length - 1] : null;
                        if (backdrop) {
                            backdrop.style.zIndex = String(zIndex - 5);
                            backdrop.style.opacity = '0.72';
                        }
                    }, 0);
                };
                el.addEventListener('shown.bs.modal', onShown, { once: true });
            }
            el.addEventListener('hidden.bs.modal', onHidden);
            if (okBtn) okBtn.onclick = function () { modal.hide(); finish(true); };
            if (cancelBtn) cancelBtn.onclick = function () { /* data-bs handles hide; hidden event resolves false */ };
            modal.show();
        });
    };
})();

// Normalize select placeholders/options to prevent duplicate "Unassigned/Select..." entries.
(function initSelectOptionNormalizer() {
    if (typeof window.normalizeSelectOptions === 'function') return;

    window.normalizeSelectOptions = function (selectEl) {
        if (!selectEl || !selectEl.options) return;
        var isEmptyLike = function (val) {
            var v = String(val || '').trim().toLowerCase();
            return v === '' || v === '__placeholder__' || v === 'placeholder' || v === 'null';
        };

        // Keep a single empty-value placeholder option.
        var emptySeen = false;
        Array.from(selectEl.options).forEach(function (opt) {
            var isEmpty = isEmptyLike(opt.value);
            if (!isEmpty) return;
            if (!emptySeen) {
                emptySeen = true;
                return;
            }
            if (opt.parentNode === selectEl) {
                selectEl.removeChild(opt);
            }
        });

        // Keep first unique non-empty value to avoid duplicate loaded entries.
        var seenValues = {};
        Array.from(selectEl.options).forEach(function (opt) {
            var key = String(opt.value || '');
            if (isEmptyLike(key)) return;
            if (seenValues[key]) {
                if (opt.parentNode === selectEl) {
                    selectEl.removeChild(opt);
                }
                return;
            }
            seenValues[key] = true;
        });

        // Guard against duplicated placeholder labels rendered by mixed HTML/JS sources.
        var seenEmptyLabels = {};
        Array.from(selectEl.options).forEach(function (opt) {
            var key = String(opt.value || '');
            if (!isEmptyLike(key)) return;
            var label = String(opt.textContent || '').trim().toLowerCase();
            if (!label) return;
            if (seenEmptyLabels[label]) {
                if (opt.parentNode === selectEl) {
                    selectEl.removeChild(opt);
                }
                return;
            }
            seenEmptyLabels[label] = true;
        });
    };

    window.normalizeAllSelectOptions = function () {
        Array.from(document.querySelectorAll('select')).forEach(function (sel) {
            window.normalizeSelectOptions(sel);
        });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', window.normalizeAllSelectOptions);
    } else {
        window.normalizeAllSelectOptions();
    }

    // Auto-normalize dynamically updated selects as options are injected.
    try {
        var observer = new MutationObserver(function (mutations) {
            var touched = [];
            mutations.forEach(function (m) {
                var target = m && m.target;
                if (target && target.tagName === 'SELECT') {
                    touched.push(target);
                    return;
                }
                if (target && target.closest) {
                    var s = target.closest('select');
                    if (s) touched.push(s);
                }
            });
            var unique = Array.from(new Set(touched));
            unique.forEach(function (sel) { window.normalizeSelectOptions(sel); });
        });
        observer.observe(document.documentElement || document.body, { childList: true, subtree: true });
    } catch (e) {
        // Ignore observer errors; normalizer still runs on initial load/manual calls.
    }
})();

// Money input formatter — adds comma separators as user types.
// Usage: add class="money-input" to any amount/salary input.
// Helpers: window.moneyVal(el) → raw numeric string (commas stripped)
//          window.moneySet(el, val) → set formatted value programmatically
(function initMoneyInputs() {
    if (window._moneyInputsInit) return;
    window._moneyInputsInit = true;

    function stripCommas(v) {
        return String(v == null ? '' : v).replace(/,/g, '');
    }

    function formatRaw(raw) {
        var s = stripCommas(String(raw == null ? '' : raw));
        var hasDot = s.indexOf('.') !== -1;
        var parts = s.split('.');
        var intStr = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        if (!intStr && !hasDot) return '';
        return hasDot ? intStr + '.' + (parts[1] || '').slice(0, 2) : intStr;
    }

    function attachMoneyInput(el) {
        if (!el || el.dataset.moneyInit) return;
        el.dataset.moneyInit = '1';
        el.type = 'text';
        el.inputMode = 'decimal';
        el.addEventListener('input', function () {
            // Preserve only valid chars: digits and at most one decimal point
            var raw = el.value.replace(/[^0-9.]/g, '');
            var firstDot = raw.indexOf('.');
            if (firstDot !== -1) raw = raw.slice(0, firstDot + 1) + raw.slice(firstDot + 1).replace(/\./g, '');
            var pos = el.selectionStart;
            var oldLen = el.value.length;
            var formatted = formatRaw(raw);
            el.value = formatted;
            var diff = el.value.length - oldLen;
            el.setSelectionRange(Math.max(0, pos + diff), Math.max(0, pos + diff));
        });
        el.addEventListener('blur', function () {
            var s = stripCommas(el.value);
            if (s === '' || s === '.') { el.value = ''; return; }
            var n = parseFloat(s);
            if (isNaN(n)) { el.value = ''; return; }
            el.value = n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        });
    }

    window.moneyVal = function (el) {
        if (!el) return '';
        return stripCommas(el.value);
    };

    window.moneySet = function (el, rawValue) {
        if (!el) return;
        if (rawValue === null || rawValue === undefined || rawValue === '') { el.value = ''; return; }
        var n = parseFloat(String(rawValue).replace(/,/g, ''));
        if (isNaN(n)) { el.value = ''; return; }
        el.value = n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    };

    function autoInit() {
        document.querySelectorAll('.money-input').forEach(attachMoneyInput);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', autoInit);
    } else {
        autoInit();
    }

    try {
        var obs = new MutationObserver(function (mutations) {
            mutations.forEach(function (m) {
                m.addedNodes.forEach(function (node) {
                    if (node.nodeType !== 1) return;
                    if (node.classList && node.classList.contains('money-input')) attachMoneyInput(node);
                    node.querySelectorAll && node.querySelectorAll('.money-input').forEach(attachMoneyInput);
                });
            });
        });
        var bodyTarget = document.body || document.documentElement;
        obs.observe(bodyTarget, { childList: true, subtree: true });
    } catch (e) {}
})();

function showLoading(displayEl) {
    if (displayEl) {
        displayEl.innerHTML = `
            <div style="
                width:100%;
                min-height: 80px;
                display:flex;
                justify-content:center;
                align-items:center;
                text-align:center;">
                ${loading_combiner}
            </div>`;
    }
}

function buildAuthRedirectUrl() {
    var path = String(window.location.pathname || '').replace(/^\/+/, '');
    var query = String(window.location.search || '');
    var current = path + query;
    if (!current) {
        current = 'studio/index.kml';
    }
    if (path.indexOf('portal/') === 0) {
        return 'auth/auth-sign-in.kml?redirect=' + encodeURIComponent(current);
    }
    return 'auth/auth-sign-in-staff.kml?redirect=' + encodeURIComponent(current);
}

function sendAjaxRequest(formClass, displaySelector) {
	// Usage
	// sendAjaxRequest('.myFormClass', '#messageDiv');
	var form = $(formClass);

	form.on('submit', function(event) {
		event.preventDefault(); // Prevent the default form submission
		$(displaySelector).html(`<div class="d-flex justify-content-center align-items-center py-2">${loading_combiner}</div>`)

		var submitButton = form.find(':submit'); // Find the submit button within the form
		submitButton.prop('disabled', true); // Disable the submit button

		var formData = form.serialize(); // Serialize the form data
		var actionUrl = form.attr('action'); // Get the action URL from the form

		$.ajax({
			type: 'POST',
			url: actionUrl,
			data: formData,
			timeout: 15000, // Set a timeout of 15 seconds (15000 milliseconds)
			success: function(data, textStatus, xhr) {
				// Support server-driven redirects for AJAX form submissions (e.g. signin/signup/reset).
				var headerRedirect = '';
				try {
					headerRedirect = xhr && xhr.getResponseHeader ? (xhr.getResponseHeader('X-Redirect-To') || xhr.getResponseHeader('Location') || '') : '';
				} catch (e) { headerRedirect = ''; }
				if (headerRedirect) {
					window.location.href = headerRedirect;
					return;
				}
				if (data && typeof data === 'object') {
					if (data.redirect) {
						window.location.href = data.redirect;
						return;
					}
					if (data.error) {
						$(displaySelector).html('<div class="alert alert-danger">' + String(data.error) + '</div>');
						submitButton.prop('disabled', false);
						return;
					}
					if (data.message) {
						$(displaySelector).html('<div class="alert alert-success">' + String(data.message) + '</div>');
						submitButton.prop('disabled', false);
						return;
					}
				}
				if (typeof data === 'string') {
					var m = data.match(/window\\.location(?:\\.href)?\\s*=\\s*([\"'])(.*?)\\1/i) || data.match(/self\\.location\\s*=\\s*([\"'])(.*?)\\1/i);
					if (m && m[2]) {
						window.location.href = m[2];
						return;
					}
				}

				$(displaySelector).html(data); // Update the success target with the response
				submitButton.prop('disabled', false); // Re-enable the submit button after success
			},
				error: function(xhr, status, error) {
					var errorMessage = '';
					if (xhr && (xhr.status === 401 || xhr.status === 419)) {
						window.location.href = buildAuthRedirectUrl();
						return;
					}
					if (status === "timeout") {
						errorMessage = 'Network request timed out. Please try again.';
					} else {
					if (xhr && xhr.responseJSON && xhr.responseJSON.error) {
						errorMessage = xhr.responseJSON.error;
					} else {
						errorMessage = (xhr && xhr.responseText) ? xhr.responseText : 'An error occurred while processing the request. Please try again.';
					}
				}
				$(displaySelector).html('<div class="alert alert-danger">' + errorMessage + '</div>'); // Update the error target with the error message
				submitButton.prop('disabled', false); // Re-enable the submit button after error
			}
		});
	});
}

function ajax_link(selector, targetSelector) {
    $(document).on('click', selector, function (e) {
        e.preventDefault();

        var href_link = $(this).attr('href');  // use the clicked link's href

        $.ajax({
            type: 'POST',
            url: href_link,         // href must be the API URL
            data: {},
            timeout: 15000,
            success: function (response) {
                if (targetSelector) {
                    $(targetSelector).html(response);
                } else {
                    $('#general_message').html(response); 
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                var errorMessage = textStatus === 'timeout'
                    ? 'Network request timed out. Please try again.'
                    : 'Could not connect to the server. Please try again.';
                if (targetSelector) {
                    $(targetSelector).html(errorMessage);
                } else {
                    $('#general_message').html(errorMessage); 
                }
            }
        });

        return false; // IMPORTANT: stops propagation + default navigation
    });
}

$(document).ready(function() {

    ajax_link('.ajax-link'); // For all links having the class ajax-links
});

document.addEventListener('click', async (e) => {
    const deleteBtn = e.target.closest('.delete-btn');
    if (!deleteBtn) return;
    if (deleteBtn.classList.contains('crm-confirmed-delete')) {
        deleteBtn.classList.remove('crm-confirmed-delete');
        return;
    }
    if (deleteBtn.classList.contains('quote-del')
        || deleteBtn.classList.contains('js-esign-delete')
        || deleteBtn.classList.contains('visitor-delete')
        || deleteBtn.classList.contains('s3-del-file')
        || deleteBtn.classList.contains('pv-delete-task')
        || deleteBtn.classList.contains('pv-delete-ticket')
        || deleteBtn.classList.contains('pv-timeline-delete')) {
        return;
    }
    e.preventDefault();
    const ok = await window.crmUiConfirm('Are you sure you want to delete this record?', 'Delete Record', {
        okText: 'Delete',
        cancelText: 'Cancel',
        variant: 'danger',
        icon: 'warning'
    });
    if (!ok) return;
    if (deleteBtn.tagName === 'A' && deleteBtn.href) {
        window.location.href = deleteBtn.href;
        return;
    }
    const form = deleteBtn.closest('form');
    if (form && (deleteBtn.type === 'submit' || deleteBtn.tagName === 'BUTTON' || deleteBtn.tagName === 'INPUT')) {
        if (typeof form.requestSubmit === 'function') {
            form.requestSubmit(deleteBtn);
        } else {
            form.submit();
        }
        return;
    }
    if (typeof deleteBtn.click === 'function') {
        deleteBtn.classList.add('crm-confirmed-delete');
        deleteBtn.click();
    }
});

// Unified file edit handler
document.addEventListener('click', (e) => {
    const btn = e.target.closest('.js-file-edit');
    if (!btn) return;
    e.preventDefault();
    const fileId = btn.getAttribute('data-id') || btn.dataset.id;
    if (!fileId) return;
    const base = (typeof url_root !== 'undefined') ? url_root : '/';
    window.location.href = `${base}editdocument.kml?file_id=${encodeURIComponent(fileId)}`;
});

// Central API endpoint helper
function apiBase() {
    return (typeof url_root !== 'undefined' ? url_root : '/');
}

function apiEndpoints() {
    const base = apiBase();
    return {
        contactsDetail: base + 'api/modules/contacts/detail',
        casesDetail: base + 'api/modules/cases/detail',
        casesIndex: base + 'api/modules/cases/index',
        casesStatuses: base + 'api/modules/cases/statuses',
        contactsIndex: base + 'api/modules/contacts/index',
        organizationsIndex: base + 'api/modules/organizations/index',
        organizationDetail: base + 'api/modules/organizations/detail',
        // Portal-specific APIs
        portalContactsIndex: base + 'api/portal/contacts/index',
        portalorganizationsIndex: base + 'api/portal/organizations/index',
        portalorganizationDetail: base + 'api/portal/organizations/detail',
        portalTicketsIndex: base + 'api/portal/tickets/index',
        portalTicketDetail: base + 'api/portal/tickets/detail',
        portalQuotesIndex: base + 'api/portal/quotes/index',
        portalQuoteDetail: base + 'api/portal/quotes/detail',
        portalContractsIndex: base + 'api/portal/contracts/index',
        portalContractDetail: base + 'api/portal/contracts/detail',
        portalInvoicesIndex: base + 'api/portal/invoices/index',
        portalInvoiceDetail: base + 'api/portal/invoices/detail',
        portalInvoicePay: base + 'api/portal/invoices/pay',
        portalInvoiceVerify: base + 'api/portal/invoices/verify',
        casesReports: base + 'api/modules/cases/reports',
        caseMyResolvedCases: base + 'api/modules/cases/my_resolved_cases',
        csatAnalysis: base + 'api/modules/csat/analysis',
        omniReports: base + 'api/modules/omni/reports',
        tasksReports: base + 'api/modules/tasks/reports',
        callsReports: base + 'api/modules/calls/reports',
        interactionsReports: base + 'api/modules/interactions/reports',
        interactionsDaily: base + 'api/modules/interactions/daily',
        agentsReports: base + 'api/modules/agents/reports',
        leadsReports: base + 'api/modules/leads/reports',
        bpaReports: base + 'api/modules/bpa/reports',
        leadSourceSpend: base + 'api/modules/leads/source_spend',
        vendorContactsIndex: base + 'api/modules/vendor-contacts/index',
        vendorContactsDetail: base + 'api/modules/vendor-contacts/detail',
        timeLogsIndex: base + 'api/modules/time-logs/index',
        timeLogsDetail: base + 'api/modules/time-logs/detail',
        timeLogsReports: base + 'api/modules/time-logs/reports',
        caseCategories: base + 'api/modules/case-categories/index',
        filesIndex: base + 'api/modules/files/index',
        leadsIndex: base + 'api/modules/leads/index',
        departmentsIndex: base + 'api/modules/departments/index',
        accountsIndex: base + 'api/modules/accounts/index',
        accountsHealth: base + 'api/modules/accounts/health',
        caseCategories: base + 'api/modules/case-categories/index',
        industries: base + 'api/modules/config/industries',
        countries: base + 'api/modules/geo/index?action=countries',
        geoCountries: base + 'api/modules/geo/index?action=countries',
        geoStates: base + 'api/modules/geo/index?action=states',
        geoCities: base + 'api/modules/geo/index?action=cities',
        geoCreateCity: base + 'api/modules/geo/index',
        currenciesConfig: base + 'api/modules/config/currencies',
        notificationsIndex: base + 'api/modules/notifications/index',
        announcementsIndex: base + 'api/modules/announcements/index',
        mailchimpCampaigns: base + 'api/modules/mailchimp/campaigns',
        agentChatThreads: base + 'api/modules/agent-chats/threads',
        agentChatMessages: base + 'api/modules/agent-chats/messages',
        agentChatUpload: base + 'api/modules/agent-chats/upload',
        agentChatDownload: base + 'api/modules/agent-chats/download',
        agentsIndex: base + 'api/modules/agents/index',
        tasksIndex: base + 'api/modules/tasks/index',
        taskDetail: base + 'api/modules/tasks/detail',
        quotesIndex: base + 'api/modules/quotes/index',
        quotesDetail: base + 'api/modules/quotes/detail',
        quotesPdf: base + 'api/modules/quotes/pdf',
        commercialReview: base + 'api/modules/commercial-review/index',
        ticketsIndex: base + 'api/modules/tickets/index',
        contractsIndex: base + 'api/modules/contracts/index',
        contractsDetail: base + 'api/modules/contracts/detail',
        invoicesIndex: base + 'api/modules/invoices/index',
        invoicesDetail: base + 'api/modules/invoices/detail',
        invoicesPdf: base + 'api/modules/invoices/pdf',
        invoicePayments: base + 'api/modules/invoices/payments',
        invoicesPrefill: base + 'api/modules/invoices/prefill',
        paymentMethods: base + 'api/modules/config/payment_methods',
        projectsIndex: base + 'api/modules/projects/index',
        projectsDetail: base + 'api/modules/projects/detail'
    };
}

window.__managedCurrenciesCache = window.__managedCurrenciesCache || null;

function loadManagedCurrencies(options) {
    const opts = options || {};
    const activeOnly = opts.activeOnly !== false;
    const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
    const endpoint = apiMap.currenciesConfig || ((typeof url_root !== 'undefined' ? url_root : '/') + 'api/modules/config/currencies');
    if (Array.isArray(window.__managedCurrenciesCache) && window.__managedCurrenciesCache.length) {
        const cached = window.__managedCurrenciesCache.slice();
        return Promise.resolve(activeOnly ? cached.filter(r => Number(r.active || 0) === 1) : cached);
    }
    return fetch(endpoint)
        .then(r => { if (!r.ok) throw new Error('Unable to load currencies'); return r.json(); })
        .then(json => {
            const rows = Array.isArray(json.data) ? json.data : [];
            window.__managedCurrenciesCache = rows;
            return activeOnly ? rows.filter(r => Number(r.active || 0) === 1) : rows;
        });
}

function populateCurrencySelect(selectEl, cfg) {
    if (!selectEl) return Promise.resolve([]);
    const options = cfg || {};
    const defaultCode = String(options.defaultCode || '').toUpperCase();
    const includeBlank = !!options.includeBlank;
    const blankLabel = options.blankLabel || 'Select';
    const preserveCurrent = options.preserveCurrent !== false;
    const current = preserveCurrent ? String(selectEl.value || '').toUpperCase() : '';
    return loadManagedCurrencies({ activeOnly: options.activeOnly !== false })
        .then(rows => {
            const allowed = Array.isArray(rows) ? rows : [];
            selectEl.innerHTML = '';
            if (includeBlank) {
                const blank = document.createElement('option');
                blank.value = '';
                blank.textContent = blankLabel;
                selectEl.appendChild(blank);
            }
            allowed.forEach(row => {
                const opt = document.createElement('option');
                opt.value = String(row.code || '').toUpperCase();
                const symbol = String(row.symbol || '').trim();
                opt.textContent = symbol ? `${opt.value} (${symbol})` : opt.value;
                selectEl.appendChild(opt);
            });
            let next = current || defaultCode;
            if (next && !Array.from(selectEl.options).some(o => String(o.value).toUpperCase() === next)) {
                next = '';
            }
            if (!next && defaultCode && Array.from(selectEl.options).some(o => String(o.value).toUpperCase() === defaultCode)) {
                next = defaultCode;
            }
            if (next) selectEl.value = next;
            return allowed;
        })
        .catch(() => {
            // Keep existing options as fallback.
            return [];
        });
}

// Safe global loader for server-side search/filter requests only.
(function initSafeGlobalSearchLoader() {
    if (window.__safeGlobalSearchLoaderInit) return;
    window.__safeGlobalSearchLoaderInit = true;

    const activeKeys = Object.create(null);
    const keyTimers = Object.create(null);
    let activeCount = 0;

    function applyState() {
        const loader = document.getElementById('loader');
        if (!loader) return;
        loader.style.display = activeCount > 0 ? 'flex' : 'none';
        loader.style.pointerEvents = 'none';
    }

    function clearKey(key) {
        if (!key || !activeKeys[key]) return;
        delete activeKeys[key];
        if (keyTimers[key]) {
            clearTimeout(keyTimers[key]);
            delete keyTimers[key];
        }
        activeCount = Math.max(0, activeCount - 1);
        applyState();
    }

    function setKey(key, isLoading, timeoutMs) {
        if (!key) return;
        if (!isLoading) {
            clearKey(key);
            return;
        }
        if (!activeKeys[key]) {
            activeKeys[key] = 1;
            activeCount += 1;
        }
        const ttl = Number(timeoutMs || 30000);
        if (keyTimers[key]) clearTimeout(keyTimers[key]);
        keyTimers[key] = setTimeout(function () {
            clearKey(key);
        }, (Number.isFinite(ttl) && ttl > 0) ? ttl : 30000);
        applyState();
    }

    window.setGlobalUiLoading = function (key, isLoading, timeoutMs) {
        const safeKey = String(key || '').trim();
        if (!safeKey) return;
        setKey(safeKey, !!isLoading, timeoutMs);
    };

    window.clearAllGlobalUiLoading = function () {
        Object.keys(activeKeys).forEach(clearKey);
        applyState();
    };

    if (window.jQuery && jQuery.fn) {
        const tableKey = function (settings) {
            if (!settings) return '';
            if (settings.sTableId) return 'dt:' + settings.sTableId;
            if (settings.nTable && settings.nTable.id) return 'dt:' + settings.nTable.id;
            return '';
        };

        jQuery(document).on('preXhr.dt', function (_e, settings) {
            if (!settings || !settings.oFeatures || !settings.oFeatures.bServerSide) return;
            const key = tableKey(settings);
            if (!key) return;
            setKey(key, true, 30000);
        });

        jQuery(document).on('xhr.dt error.dt', function (_e, settings) {
            const key = tableKey(settings);
            if (!key) return;
            clearKey(key);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            window.clearAllGlobalUiLoading();
        });
    } else {
        window.clearAllGlobalUiLoading();
    }
})();

// CRM communication helpers (call/email)
function crmEscapeAttr(val) {
    return String(val ?? '').replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

function crmBuildActionButton(type, opts = {}) {
    const callEnabled = !(window.crmFeatures && window.crmFeatures.callEnabled === false);
    const phone = opts.phone || '';
    const email = opts.email || '';
    const name = opts.name || '';
    const entityType = opts.entityType || 'contact';
    const entityId = opts.entityId || '';
    const entityLabel = opts.entityLabel || name || '';
    const subject = opts.subject || '';
    if (type === 'call' && (!phone || !callEnabled)) return '';
    if (type === 'email' && !email) return '';
    const attrs = [
        `data-crm-action="${type}"`,
        phone ? `data-phone="${crmEscapeAttr(phone)}"` : '',
        email ? `data-email="${crmEscapeAttr(email)}"` : '',
        name ? `data-name="${crmEscapeAttr(name)}"` : '',
        entityType ? `data-entity-type="${crmEscapeAttr(entityType)}"` : '',
        entityId ? `data-entity-id="${crmEscapeAttr(entityId)}"` : '',
        entityLabel ? `data-entity-label="${crmEscapeAttr(entityLabel)}"` : '',
        subject ? `data-subject="${crmEscapeAttr(subject)}"` : ''
    ].filter(Boolean).join(' ');
    const icon = type === 'call' ? 'ri-phone-line' : 'ri-mail-line';
    const btnClass = type === 'call' ? 'btn-soft-success' : 'btn-soft-primary';
    const title = type === 'call' ? 'Call' : 'Email';
    return `<button type="button" class="btn btn-icon btn-sm ${btnClass} ms-1 crm-action-btn" ${attrs} title="${title}">
        <i class="${icon}"></i>
    </button>`;
}

function crmBuildInlineButtons(opts = {}) {
    const callBtn = crmBuildActionButton('call', opts);
    const emailBtn = crmBuildActionButton('email', opts);
    if (!callBtn && !emailBtn) return '';
    return `<span class="crm-comm-actions ms-1">${callBtn}${emailBtn}</span>`;
}

function crmOutboundCallHref(phone) {
    const callEnabled = !(window.crmFeatures && window.crmFeatures.callEnabled === false);
    if (!callEnabled) {
        return 'tel:' + encodeURIComponent(phone || '');
    }
    const base = (typeof url_root !== 'undefined') ? url_root : '../';
    return base + 'studio/outbound.kml?call=1&phone=' + encodeURIComponent(phone || '');
}

function crmOutboundEmailHref(email) {
    const base = (typeof url_root !== 'undefined') ? url_root : '../';
    return base + 'studio/outbound.kml?email=1&to=' + encodeURIComponent(email || '');
}

function crmOpenCallPrefill(opts = {}) {
    const callEnabled = !(window.crmFeatures && window.crmFeatures.callEnabled === false);
    if (!callEnabled) {
        if (opts.phone) window.location.href = 'tel:' + encodeURIComponent(opts.phone);
        return;
    }
    const base = (typeof url_root !== 'undefined' ? url_root : '../');
    const params = new URLSearchParams();
    params.set('call', '1');
    if (opts.phone) params.set('phone', opts.phone);
    if (opts.name) params.set('name', opts.name);
    if (opts.entityType) params.set('entity_type', opts.entityType);
    if (opts.entityId) params.set('entity_id', opts.entityId);
    if (opts.entityLabel) params.set('entity_label', opts.entityLabel);
    const url = base + 'studio/outbound.kml?' + params.toString();
    window.location.href = url;
}

function crmOpenEmailCompose(opts = {}) {
    const base = (typeof url_root !== 'undefined' ? url_root : '../');
    const params = new URLSearchParams();
    params.set('email', '1');
    if (opts.email) params.set('to', opts.email);
    if (opts.name) params.set('name', opts.name);
    if (opts.entityType) params.set('entity_type', opts.entityType);
    if (opts.entityId) params.set('entity_id', opts.entityId);
    if (opts.subject) params.set('subject', opts.subject);
    const url = base + 'studio/outbound.kml?' + params.toString();
    window.location.href = url;
}

window.crmComms = {
    buildActionButton: crmBuildActionButton,
    buildInlineButtons: crmBuildInlineButtons,
    outboundCallHref: crmOutboundCallHref,
    outboundEmailHref: crmOutboundEmailHref,
    openCallPrefill: crmOpenCallPrefill,
    openEmailCompose: crmOpenEmailCompose
};

// Confirm outbound call links
document.addEventListener('click', (e) => {
    const link = e.target.closest('a.confirm-call');
    if (!link) return;
    e.preventDefault();
    const phone = link.getAttribute('data-phone') || link.textContent || '';
    const msg = phone ? `Are you sure you want to dial ${phone}?` : 'Are you sure you want to place this call?';
    const proceed = window.crmUiConfirm(msg, 'Confirm Call', {
        okText: 'Dial',
        cancelText: 'Cancel',
        variant: 'primary',
        icon: 'warning'
    });
    Promise.resolve(proceed).then((ok) => {
        if (!ok) return;
        window.location.href = link.href;
    });
});

document.addEventListener('click', async (e) => {
    const callBtn = e.target.closest('[data-crm-action="call"]');
    if (callBtn) {
        e.preventDefault();
        const phoneVal = callBtn.dataset.phone || '';
        const msg = phoneVal ? `Are you sure you want to dial ${phoneVal}?` : 'Are you sure you want to place this call?';
        const ok = await window.crmUiConfirm(msg, 'Confirm Call', {
            okText: 'Dial',
            cancelText: 'Cancel',
            variant: 'primary',
            icon: 'warning'
        });
        if (!ok) {
            return;
        }
        crmOpenCallPrefill({
            phone: phoneVal,
            name: callBtn.dataset.name || '',
            entityType: callBtn.dataset.entityType || 'contact',
            entityId: callBtn.dataset.entityId || '',
            entityLabel: callBtn.dataset.entityLabel || ''
        });
        return;
    }
    const emailBtn = e.target.closest('[data-crm-action="email"]');
    if (emailBtn) {
        e.preventDefault();
        crmOpenEmailCompose({
            email: emailBtn.dataset.email || '',
            name: emailBtn.dataset.name || '',
            entityType: emailBtn.dataset.entityType || 'contact',
            entityId: emailBtn.dataset.entityId || '',
            subject: emailBtn.dataset.subject || ''
        });
    }
});

// Generic button loading helper
function toggleButtonLoading(btn, state, text) {
    if (!btn) return;
    const el = (btn instanceof HTMLElement) ? btn : null;
    if (!el) return;
    if (state) {
        if (!el.dataset.originalHtml) {
            el.dataset.originalHtml = el.innerHTML;
        }
        el.disabled = true;
        const label = text || 'Loading...';
        el.innerHTML = `<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>${label}`;
    } else {
        if (el.dataset.originalHtml) {
            el.innerHTML = el.dataset.originalHtml;
            delete el.dataset.originalHtml;
        }
        el.disabled = false;
    }
}
window.toggleButtonLoading = toggleButtonLoading;

// File preview helpers
function fileExtensionFromName(name) {
    if (!name) return '';
    const idx = name.lastIndexOf('.');
    if (idx === -1) return '';
    return name.slice(idx + 1).toLowerCase();
}
function isOfficeExt(ext) {
    return ['doc','docx','xls','xlsx','csv','ppt','pptx'].includes(ext);
}
function isPdfExt(ext) {
    return ext === 'pdf';
}
function isImageExt(ext) {
    return ['jpg','jpeg','png','gif','webp','bmp','svg'].includes(ext);
}
function appendCacheParam(url) {
    if (!url) return url;
    const sep = url.includes('?') ? '&' : '?';
    return `${url}${sep}v=${Date.now()}`;
}
function resolveFileViewUrl(fileName, publicUrl, downloadUrl) {
    const ext = fileExtensionFromName(fileName);
    if (isOfficeExt(ext) && publicUrl) {
        const src = appendCacheParam(publicUrl);
        return { url: 'https://view.officeapps.live.com/op/view.aspx?src=' + encodeURIComponent(src), type: 'office' };
    }
    if (isPdfExt(ext)) {
        return { url: publicUrl || downloadUrl || '', type: 'pdf' };
    }
    if (isImageExt(ext)) {
        return { url: publicUrl || downloadUrl || '', type: 'image' };
    }
    return { url: publicUrl || downloadUrl || '', type: 'other' };
}
window.resolveFileViewUrl = resolveFileViewUrl;

// Announcements badge helper
function loadAnnouncementBadges() {
    const badges = document.querySelectorAll('[data-announcement-badge]');
    const alerts = document.querySelectorAll('[data-announcement-alert]');
    const counts = document.querySelectorAll('[data-announcement-count]');
    if (!badges.length && !alerts.length) return;
    const api = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
    const url = api.announcementsIndex || (apiBase() + 'api/modules/announcements/index');
    const cacheKey = 'announcements_unread_count';
    const cacheTsKey = 'announcements_unread_ts';
    const cacheTtlMs = 15 * 1000;

    function applyCount(cnt) {
        badges.forEach((badge) => {
            if (cnt > 0) {
                badge.textContent = cnt > 99 ? '99+' : String(cnt);
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        });
        counts.forEach((el) => {
            el.textContent = cnt > 99 ? '99+' : String(cnt);
        });
        alerts.forEach((alert) => {
            if (cnt > 0) {
                alert.classList.remove('d-none');
            } else {
                alert.classList.add('d-none');
            }
        });
    }

    try {
        const cached = localStorage.getItem(cacheKey);
        const cachedTs = localStorage.getItem(cacheTsKey);
        const cachedNum = cached !== null ? parseInt(cached, 10) : NaN;
        const tsNum = cachedTs !== null ? parseInt(cachedTs, 10) : 0;
        if (!Number.isNaN(cachedNum)) {
            applyCount(cachedNum);
        }
        if (tsNum && (Date.now() - tsNum) < cacheTtlMs) {
            return;
        }
    } catch (e) {}

    fetch(`${url}?scope=unread&limit=1`, { credentials: 'same-origin' })
        .then((r) => r.json())
        .then((data) => {
            const cnt = parseInt(data?.unread_count ?? 0, 10) || 0;
            applyCount(cnt);
            try {
                localStorage.setItem(cacheKey, String(cnt));
                localStorage.setItem(cacheTsKey, String(Date.now()));
            } catch (e) {}
        })
        .catch(() => {
            badges.forEach((badge) => (badge.style.display = 'none'));
            alerts.forEach((alert) => alert.classList.add('d-none'));
        });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadAnnouncementBadges);
} else {
    loadAnnouncementBadges();
}

function fitBrandText() {
    const nodes = Array.from(document.querySelectorAll('[data-fit-brand]'));
    if (!nodes.length) return;
    nodes.forEach((el) => {
        const minSize = parseFloat(el.dataset.minSize || '16');
        const maxSize = parseFloat(el.dataset.maxSize || '22');
        const container = el.parentElement;
        const maxWidth = container ? container.clientWidth : el.clientWidth;
        if (!maxWidth) return;
        let size = maxSize;
        el.style.fontSize = size + 'px';
        el.style.whiteSpace = 'nowrap';
        el.style.overflow = 'hidden';
        el.style.textOverflow = 'ellipsis';
        while (size > minSize && el.scrollWidth > maxWidth) {
            size -= 0.5;
            el.style.fontSize = size + 'px';
        }
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', fitBrandText);
} else {
    fitBrandText();
}

document.addEventListener('click', (e) => {
    const viewBtn = e.target.closest('.js-file-view');
    if (!viewBtn) return;
    const url = viewBtn.getAttribute('data-view-url');
    const type = viewBtn.getAttribute('data-view-type');
    if (!url) return;
    if (type === 'image' && window.jQuery && jQuery.fancybox) {
        e.preventDefault();
        jQuery.fancybox.open([{ href: url, type: 'iframe' }]);
        return;
    }
});

// Badge helpers for statuses/priorities
window.caseStatusConfig = window.caseStatusConfig || {};

function normalizeCaseStatusCode(value) {
    return (value || '').toString().trim().toLowerCase();
}

function getCaseStatusConfig(status) {
    const key = normalizeCaseStatusCode(status);
    if (!key) return null;
    const map = window.caseStatusConfig || {};
    return map[key] || null;
}

function setCaseStatusConfig(list) {
    const next = {};
    (list || []).forEach(item => {
        const code = normalizeCaseStatusCode(item && (item.code || item.label || ''));
        if (!code) return;
        next[code] = {
            code: code,
            label: (item && (item.label || item.code)) ? String(item.label || item.code) : code,
            color: item && item.color ? String(item.color).trim() : ''
        };
    });
    window.caseStatusConfig = next;
    return next;
}

window.getCaseStatusConfig = getCaseStatusConfig;
window.setCaseStatusConfig = setCaseStatusConfig;

function ensureCaseStatusConfigLoaded() {
    try {
        if (window.disableCaseStatusBootstrap) return;
        const map = window.caseStatusConfig || {};
        if (Object.keys(map).length > 0) return;
        if (window.__caseStatusesLoading) return;
        window.__caseStatusesLoading = true;

        // Fast path: cached config so badges render with correct colors immediately.
        try {
            const cached = localStorage.getItem('case_statuses_cache_v1');
            if (cached) {
                const parsed = JSON.parse(cached);
                const list = Array.isArray(parsed?.data) ? parsed.data : (Array.isArray(parsed) ? parsed : []);
                if (list.length) {
                    setCaseStatusConfig(list);
                }
            }
        } catch (e) {}

        const base = (typeof window.url_root !== 'undefined' && window.url_root) ? window.url_root : '../';
        const api = (typeof window.apiEndpoints === 'function' && window.apiEndpoints().casesStatuses)
            ? window.apiEndpoints().casesStatuses
            : (base + 'api/modules/cases/statuses');
        const asset = base + 'assets/json/case_statuses.json';
        const bust = (api.indexOf('?') === -1 ? '?' : '&') + '_=' + Date.now();

        const applyList = (data) => {
            const list = Array.isArray(data?.data) ? data.data : (Array.isArray(data) ? data : []);
            if (list.length) {
                setCaseStatusConfig(list);
                try {
                    localStorage.setItem('case_statuses_cache_v1', JSON.stringify(list));
                } catch (e) {}
            }
        };

        fetch(api + bust, { credentials: 'same-origin' })
            .then(r => r.ok ? r.json() : Promise.reject(new Error('api_not_ok')))
            .then(applyList)
            .catch(() => {
                return fetch(asset + '?_=' + Date.now(), { credentials: 'same-origin' })
                    .then(r => r.ok ? r.json() : Promise.reject(new Error('asset_not_ok')))
                    .then(applyList)
                    .catch(() => {});
            })
            .finally(() => {
                window.__caseStatusesLoading = false;
            });
    } catch (e) {
        window.__caseStatusesLoading = false;
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', ensureCaseStatusConfigLoaded);
} else {
    ensureCaseStatusConfigLoaded();
}

function caseStatusTextColor(bgColor) {
    const color = (bgColor || '').trim();
    const hexMatch = color.match(/^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/);
    if (!hexMatch) return '#ffffff';
    let hex = hexMatch[1];
    if (hex.length === 3) {
        hex = hex.split('').map(c => c + c).join('');
    }
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b);
    return luminance >= 165 ? '#111827' : '#ffffff';
}

function renderStatusBadge(status) {
    const s = (status || '').toString();
    if (!s) return '<span class="badge bg-light text-muted">N/A</span>';
    const key = s.toLowerCase();
    const cfg = getCaseStatusConfig(key);
    if (cfg && cfg.color) {
        const text = caseStatusTextColor(cfg.color);
        return `<span class="badge text-uppercase" style="background:${cfg.color};color:${text};">${cfg.label || s}</span>`;
    }
    // Config loaded but no color set — still use the resolved label
    const label = (cfg && cfg.label) ? cfg.label : s;
    return `<span class="badge bg-secondary-subtle text-secondary text-uppercase">${label}</span>`;
}

function renderPriorityBadge(priority) {
    const p = (priority || '').toString();
    if (!p) return '<span class="badge bg-light text-muted">N/A</span>';
    const key = p.toLowerCase();
    const map = {
        urgent: 'bg-danger-subtle text-danger',
        high: 'bg-warning-subtle text-danger',
        medium: 'bg-info-subtle text-info',
        low: 'bg-secondary-subtle text-secondary'
    };
    const cls = map[key] || 'bg-light text-muted';
    return `<span class="badge ${cls} text-uppercase">${p}</span>`;
}

function getFileIconDetails(extension) {
    const ext = (extension || '').toString().toLowerCase();
    const map = {
        pdf: { icon: 'ri-file-pdf-line', colorClass: 'file-icon-pdf', textClass: 'text-danger' },
        doc: { icon: 'ri-file-word-line', colorClass: 'file-icon-doc', textClass: 'text-primary' },
        docx: { icon: 'ri-file-word-line', colorClass: 'file-icon-doc', textClass: 'text-primary' },
        xls: { icon: 'ri-file-excel-line', colorClass: 'file-icon-xls', textClass: 'text-success' },
        xlsx: { icon: 'ri-file-excel-line', colorClass: 'file-icon-xls', textClass: 'text-success' },
        csv: { icon: 'ri-file-excel-line', colorClass: 'file-icon-xls', textClass: 'text-success' },
        txt: { icon: 'ri-file-text-line', colorClass: 'file-icon-txt', textClass: 'text-muted' },
        png: { icon: 'ri-image-line', colorClass: 'file-icon-img', textClass: 'text-warning' },
        jpg: { icon: 'ri-image-line', colorClass: 'file-icon-img', textClass: 'text-warning' },
        jpeg: { icon: 'ri-image-line', colorClass: 'file-icon-img', textClass: 'text-warning' },
        gif: { icon: 'ri-image-line', colorClass: 'file-icon-img', textClass: 'text-warning' },
        webp: { icon: 'ri-image-line', colorClass: 'file-icon-img', textClass: 'text-warning' },
        svg: { icon: 'ri-image-line', colorClass: 'file-icon-img', textClass: 'text-warning' },
        zip: { icon: 'ri-folder-zip-line', colorClass: 'file-icon-zip', textClass: 'text-warning' },
        rar: { icon: 'ri-folder-zip-line', colorClass: 'file-icon-zip', textClass: 'text-warning' }
    };
    return map[ext] || { icon: 'ri-file-line', colorClass: 'file-icon-default', textClass: 'text-muted' };
}

// Notifications
function initNotificationsDropdown() {
    if (window.disableFrontendNotifications) return;
    const notifWrap = document.getElementById('notificationDropdown');
    const btn = notifWrap ? notifWrap.querySelector('#page-header-notifications-dropdown') : document.getElementById('page-header-notifications-dropdown');
    const listEl = document.getElementById('notif-list');
    const emptyEl = document.getElementById('notif-empty');
    const badgeEl = document.getElementById('notif-badge');
    const newCountEl = document.getElementById('notif-new-count');
    if (!btn || !listEl) return;
    const apiMap = (typeof apiEndpoints === 'function') ? apiEndpoints() : {};
    const apiNotif = apiMap.notificationsIndex;
    if (!apiNotif) return;
    const maxItems = 20;

    function timeAgo(input) {
        const date = input ? new Date(input) : null;
        if (!date || isNaN(date.getTime())) return '';
        const diff = (Date.now() - date.getTime()) / 1000;
        if (diff < 60) return 'just now';
        if (diff < 3600) return Math.floor(diff / 60) + 'm ago';
        if (diff < 86400) return Math.floor(diff / 3600) + 'h ago';
        if (diff < 604800) return Math.floor(diff / 86400) + 'd ago';
        return date.toLocaleDateString();
    }

    function updateUnreadMeta(unread) {
        const safeUnread = Math.max(0, parseInt(unread, 10) || 0);
        if (badgeEl) badgeEl.textContent = safeUnread || 0;
        if (newCountEl) newCountEl.textContent = safeUnread + ' New';
    }

    function notificationItemMarkup(item) {
        const time = item.created_at || item.created || item.createdAt || '';
        const body = item.body || '';
        return `
              <div class="d-flex">
                <div class="avatar-xs me-3 flex-shrink-0">
                  <span class="avatar-title bg-info-subtle text-info rounded-circle fs-16">
                    <i class="bx bx-bell"></i>
                  </span>
                </div>
                <div class="flex-grow-1">
                  <div class="d-flex justify-content-between">
                    <h6 class="mt-0 mb-1 fs-13 fw-semibold">${item.title || 'Notification'}</h6>
                    <small class="text-muted">${timeAgo(time)}</small>
                  </div>
                  <div class="fs-13 text-muted">${body}</div>
                </div>
              </div>`;
    }

    function buildNotificationNode(item) {
        const li = document.createElement('div');
        li.className = 'text-reset notification-item d-block dropdown-item position-relative';
        if (item && item.id) {
            li.dataset.notificationId = String(item.id);
        }
        li.innerHTML = notificationItemMarkup(item || {});
        return li;
    }

    function render(items) {
        listEl.innerHTML = '';
        const unread = (items || []).filter(i => !Number(i.is_read)).length;
        updateUnreadMeta(unread);
        if (!items || !items.length) {
            if (emptyEl) emptyEl.classList.remove('d-none');
            return;
        }
        if (emptyEl) emptyEl.classList.add('d-none');
        items.forEach(item => {
            listEl.appendChild(buildNotificationNode(item));
        });
    }

    function fetchNotifs() {
        fetch(apiNotif + '?limit=20')
          .then(r => r.json())
          .then(data => render(data.data || []))
          .catch(() => {});
    }

    function markAllRead() {
        fetch(apiNotif, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ mark_all: true }) })
          .catch(() => {});
    }

    btn.addEventListener('show.bs.dropdown', () => {
        fetchNotifs();
        // Do not auto mark-all-read on open; let a separate action handle it
    });
    // initial load: fetch list but keep unread counts intact
    fetchNotifs();

    window.addEventListener('crm:notification-created', function (ev) {
        const detail = (ev && ev.detail) || {};
        const item = detail.notification || null;
        if (!item) return;
        const existing = item.id ? listEl.querySelector('[data-notification-id="' + String(item.id) + '"]') : null;
        if (existing) existing.remove();
        listEl.prepend(buildNotificationNode(item));
        while (listEl.children.length > maxItems) {
            listEl.removeChild(listEl.lastElementChild);
        }
        if (emptyEl) emptyEl.classList.add('d-none');
        if (typeof detail.unread_count !== 'undefined') {
            updateUnreadMeta(detail.unread_count);
        }
    });

    window.addEventListener('crm:notification-state', function (ev) {
        const detail = (ev && ev.detail) || {};
        const action = String(detail.action || '');
        const ids = Array.isArray(detail.ids) ? detail.ids.map(function (id) { return String(id); }) : [];
        if (action === 'clear_all' || action === 'mark_all') {
            listEl.innerHTML = '';
            if (emptyEl) emptyEl.classList.remove('d-none');
        } else if (action === 'mark_ids' && ids.length) {
            ids.forEach(function (id) {
                const node = listEl.querySelector('[data-notification-id="' + id + '"]');
                if (node) node.remove();
            });
            if (!listEl.children.length && emptyEl) {
                emptyEl.classList.remove('d-none');
            }
        }
        if (typeof detail.unread_count !== 'undefined') {
            updateUnreadMeta(detail.unread_count);
        }
    });
}

document.addEventListener('DOMContentLoaded', initNotificationsDropdown);


/* ────────────────────────────────────────────────────────────────────────── *
 *  Stream attachments helper
 *  Wires a paperclip button + multi-file input + selected-file pills next to
 *  any stream submit form. Returns a small controller with:
 *    .getFileIds()  → Promise<int[]>  (uploads pending files, returns ids)
 *    .clear()       → drops all selected files
 *    .hasFiles()    → bool
 *
 *  Usage from any entity view JS:
 *    const att = window.crmStreamAttachments({
 *      anchorEl: document.getElementById('ctActivityText').parentNode,
 *      buttonText: 'Attach',
 *    });
 *    // before posting:
 *    if (att.hasFiles()) {
 *      const ids = await att.getFileIds();
 *      payload.file_ids = ids;
 *    }
 *    att.clear();   // after success
 * ────────────────────────────────────────────────────────────────────────── */
(function () {
    if (typeof window.crmStreamAttachments === 'function') return;

    function fmtSize(b) {
        if (!b) return '';
        if (b < 1024) return b + 'B';
        if (b < 1024 * 1024) return Math.round(b / 1024) + 'KB';
        return (b / 1024 / 1024).toFixed(1) + 'MB';
    }
    function escAttr(s) {
        return String(s == null ? '' : s).replace(/"/g, '&quot;').replace(/</g, '&lt;');
    }

    window.crmStreamAttachments = function (opts) {
        opts = opts || {};
        const anchor = opts.anchorEl;
        if (!anchor) throw new Error('crmStreamAttachments: anchorEl required');
        const buttonLabel = opts.buttonText || 'Attach';
        const uploadUrl = opts.uploadUrl || 'api/modules/activities/upload';

        // Wrapper UI
        const wrap = document.createElement('div');
        wrap.className = 'crm-stream-attach mt-2';
        wrap.innerHTML =
            '<button type="button" class="btn btn-sm btn-outline-primary me-2 crm-attach-btn">' +
              '<i class="ri-attachment-2 me-1"></i>' + escAttr(buttonLabel) +
            '</button>' +
            '<input type="file" class="d-none crm-attach-input" multiple>' +
            '<div class="crm-attach-pills d-inline-flex flex-wrap gap-1 align-middle"></div>';
        anchor.appendChild(wrap);

        const btn   = wrap.querySelector('.crm-attach-btn');
        const input = wrap.querySelector('.crm-attach-input');
        const pills = wrap.querySelector('.crm-attach-pills');

        // pending = locally selected (not yet uploaded), uploaded = file IDs
        let pending = [];   // File[]
        let uploaded = [];  // {id,name,mime,size,url}[]

        function renderPills() {
            pills.innerHTML = '';
            const all = uploaded.map(f => ({ name: f.name, size: f.size, src: 'up', id: f.id }))
                .concat(pending.map((f, i) => ({ name: f.name, size: f.size, src: 'p', idx: i })));
            all.forEach(item => {
                const pill = document.createElement('span');
                pill.className = 'badge bg-light text-dark border d-inline-flex align-items-center gap-1';
                pill.style.fontWeight = '400';
                pill.innerHTML =
                    '<i class="ri-file-3-line"></i>' +
                    '<span>' + escAttr(item.name) + ' <span class="text-muted">(' + fmtSize(item.size) + ')</span></span>' +
                    '<button type="button" class="btn-close ms-1" style="font-size:.6rem;" aria-label="Remove"></button>';
                pill.querySelector('button').addEventListener('click', () => {
                    if (item.src === 'p') pending.splice(item.idx, 1);
                    else uploaded = uploaded.filter(u => u.id !== item.id);
                    renderPills();
                });
                pills.appendChild(pill);
            });
        }

        btn.addEventListener('click', () => input.click());
        input.addEventListener('change', () => {
            for (const f of input.files) pending.push(f);
            input.value = '';
            renderPills();
        });

        async function uploadPending() {
            if (!pending.length) return;
            const fd = new FormData();
            pending.forEach(f => fd.append('files[]', f));
            const res = await fetch(uploadUrl, { method: 'POST', body: fd, credentials: 'same-origin' });
            const data = await res.json().catch(() => ({}));
            if (!res.ok || !data.ok) {
                throw new Error((data && (data.error || data.message)) || 'Upload failed');
            }
            uploaded = uploaded.concat(data.files || []);
            pending = [];
            renderPills();
        }

        return {
            hasFiles: () => pending.length > 0 || uploaded.length > 0,
            getFileIds: async () => {
                if (pending.length) await uploadPending();
                return uploaded.map(f => f.id);
            },
            clear: () => { pending = []; uploaded = []; renderPills(); },
        };
    };
})();

/* ────────────────────────────────────────────────────────────────────────── *
 *  Render attachment chips inside an activity card
 *  Returns an HTML string (or '') given an activity.metadata object.
 * ────────────────────────────────────────────────────────────────────────── */
(function () {
    if (typeof window.crmRenderActivityAttachments === 'function') return;
    window.crmRenderActivityAttachments = function (metadata) {
        if (typeof metadata === 'string') {
            try { metadata = JSON.parse(metadata); } catch (e) { return ''; }
        }
        if (!metadata || !Array.isArray(metadata.attachments) || metadata.attachments.length === 0) return '';
        const items = metadata.attachments.map(att => {
            const name = String(att.name || '').replace(/[<>&"]/g, c => ({
                '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;'
            }[c]));
            const url = String(att.url || '#').replace(/"/g, '&quot;');
            return '<a href="' + url + '" target="_blank" rel="noopener" '
                 + 'class="badge bg-light text-dark border me-1 mt-1 d-inline-flex align-items-center gap-1" '
                 + 'style="font-weight:400;text-decoration:none;">'
                 + '<i class="ri-attachment-2"></i>' + name
                 + '</a>';
        }).join('');
        return '<div class="mt-1">' + items + '</div>';
    };
})();
