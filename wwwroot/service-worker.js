// Desarrollo: sin caché offline (los cambios deben verse al recargar).
// El handler fetch es necesario para criterios de instalabilidad PWA.
self.addEventListener('install', event => event.waitUntil(self.skipWaiting()));
self.addEventListener('activate', event => event.waitUntil(self.clients.claim()));
self.addEventListener('fetch', () => { });
