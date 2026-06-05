(function () {
    var _deferred = null;

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        _deferred = e;
        console.log('[PWA] beforeinstallprompt captured');
    });

    window.PwaInstall = {
        isInstallable: function () { return !!_deferred; },

        isIosSafari: function () {
            var ua = navigator.userAgent;
            // iPadOS 13+ reports as MacIntel — detect by touch points
            var isIos = /iphone|ipad|ipod/i.test(ua) ||
                        (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
            var isSafari = /safari/i.test(ua) && !/crios|fxios|chrome/i.test(ua);
            var result = isIos && isSafari;
            console.log('[PWA] isIosSafari:', result, '| ua:', ua.substring(0, 80));
            return result;
        },

        isStandalone: function () {
            var result = navigator.standalone === true ||
                matchMedia('(display-mode: standalone)').matches;
            console.log('[PWA] isStandalone:', result);
            return result;
        },

        wasDismissed: function () {
            var result = localStorage.getItem('pwa-prompt-v2-dismissed') === '1';
            console.log('[PWA] wasDismissed:', result);
            return result;
        },

        dismiss: function () {
            localStorage.setItem('pwa-prompt-v2-dismissed', '1');
        },

        promptInstall: async function () {
            if (!_deferred) return false;
            await _deferred.prompt();
            var result = await _deferred.userChoice;
            _deferred = null;
            return result.outcome === 'accepted';
        }
    };

    console.log('[PWA] pwa-install.js loaded');
})();
