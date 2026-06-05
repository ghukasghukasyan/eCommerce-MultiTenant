// Development service worker — passes all requests through without caching.
// In production (dotnet publish), this file is replaced by service-worker.published.js.
self.addEventListener('fetch', () => { });
