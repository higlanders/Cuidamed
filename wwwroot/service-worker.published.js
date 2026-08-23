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

function isSensitiveRequest(request) {
    try {
        const url = new URL(request.url);
        const path = url.pathname.toLowerCase();
        if (path.endsWith('/appsettings.json') || path.includes('/appsettings.'))
            return true;
        // Respuestas de APILIS / PII: nunca cachear (passthrough sin Cache API).
        if (path.includes('/apilis/') || path.includes('/api/'))
            return true;
        return false;
    } catch {
        return false;
    }
}

// Handler fetch obligatorio para que Chrome considere la app instalable.
self.addEventListener('fetch', event => {
    const req = event.request;
    if (isSensitiveRequest(req)) {
        event.respondWith(fetch(req, { cache: 'no-store' }));
        return;
    }

    event.respondWith(fetch(req));
});
