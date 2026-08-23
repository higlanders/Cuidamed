// Desarrollo: sin caché offline (los cambios deben verse al recargar).
// El handler fetch es necesario para criterios de instalabilidad PWA.
self.addEventListener('install', event => event.waitUntil(self.skipWaiting()));
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('fetch', event => {
    const url = event.request.url || '';
    if (url.indexOf('appsettings') !== -1 || url.indexOf('/api/') !== -1 || url.indexOf('/APILIS/') !== -1) {
        event.respondWith(fetch(event.request, { cache: 'no-store' }));
        return;
    }
});
