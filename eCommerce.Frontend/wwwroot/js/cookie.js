window.cookieService = {
    get: function (key) {
        var match = document.cookie.match(new RegExp('(^| )' + encodeURIComponent(key) + '=([^;]+)'));
        return match ? decodeURIComponent(match[2]) : null;
    },
    set: function (key, value, days, path) {
        var expires = new Date(Date.now() + days * 864e5).toUTCString();
        document.cookie = encodeURIComponent(key) + '=' + encodeURIComponent(value)
            + '; expires=' + expires + '; path=' + (path || '/');
    },
    remove: function (key) {
        document.cookie = encodeURIComponent(key) + '=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
    }
};
