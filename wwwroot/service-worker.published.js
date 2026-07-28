// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm$/, /\.html$/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.webmanifest$/ ];
// Nunca cachear appsettings: credenciales/endpoints desactualizados provocan 401 fantasma.
const offlineAssetsExclude = [ /^service-worker\.js$/, /appsettings/i ];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');
    // Activate updated SW immediately so users leave the old Home/login cache behind
    self.skipWaiting();

    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');
    await self.clients.claim();

    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    // Navigations: network-first so deploys (new Home/login) show up without stuck PWA cache
    if (event.request.method === 'GET' && event.request.mode === 'navigate') {
        try {
            const networkResponse = await fetch(event.request);
            const cache = await caches.open(cacheName);
            cache.put('index.html', networkResponse.clone());
            return networkResponse;
        } catch {
            const cache = await caches.open(cacheName);
            return (await cache.match('index.html')) || Response.error();
        }
    }

    if (event.request.method === 'GET') {
        const cache = await caches.open(cacheName);
        const cachedResponse = await cache.match(event.request);
        if (cachedResponse) {
            // Revalidate in background
            event.waitUntil(
                fetch(event.request).then(networkResponse => {
                    if (networkResponse && networkResponse.ok) {
                        return cache.put(event.request, networkResponse.clone());
                    }
                }).catch(() => { })
            );
            return cachedResponse;
        }

        try {
            const networkResponse = await fetch(event.request);
            if (networkResponse && networkResponse.ok) {
                cache.put(event.request, networkResponse.clone());
            }
            return networkResponse;
        } catch {
            return Response.error();
        }
    }

    return fetch(event.request);
}
