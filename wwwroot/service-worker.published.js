// PWA service worker (publicado): permite instalar la app sin cachear WASM/DLL.
// Tras cada deploy el navegador usa red; se limpian caches offline-cache-* viejos.
// La línea `const base` la parchea gh-pages.yml en GitHub Pages.
const base = "/";

self.addEventListener('install', event => {
    event.waitUntil(self.skipWaiting());
});

self.addEventListener('activate', event => {
    event.waitUntil((async () => {
        const keys = await caches.keys();
        await Promise.all(
            keys
                .filter(key => key.startsWith('offline-cache-'))
                .map(key => caches.delete(key))
        );
        await self.clients.claim();
    })());
});

// Handler fetch obligatorio para que Chrome considere la app instalable.
self.addEventListener('fetch', event => {
    event.respondWith(fetch(event.request));
});
