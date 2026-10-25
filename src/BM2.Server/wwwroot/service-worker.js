// Fast start only; the app needs the network. API calls and navigations are never cached.
// Fingerprinted static files (name.<hash>.ext) never change: cache first. Other static files: network first, cache as fallback.
const version = "v1";
const cacheName = `bm2-static-${version}`;
const staticPath = /^\/(_framework|_content|css|fonts|icons|js|lib|Components)\/|^\/[^/]+\.(css|js|webmanifest|png|svg)$/;
const fingerprinted = /\.[a-z0-9]{10}\.[a-z0-9]+$/;

self.addEventListener("install", () => self.skipWaiting());

self.addEventListener("activate", event => {
    event.waitUntil((async () => {
        for (const key of await caches.keys()) {
            if (key.startsWith("bm2-") && key !== cacheName) await caches.delete(key);
        }
        await self.clients.claim();
    })());
});

self.addEventListener("fetch", event => {
    const request = event.request;
    const url = new URL(request.url);
    if (request.method !== "GET" || url.origin !== self.location.origin || request.mode === "navigate"
        || url.pathname.startsWith("/api/") || url.pathname === "/service-worker.js" || !staticPath.test(url.pathname)) {
        return;
    }

    event.respondWith(fingerprinted.test(url.pathname) ? cacheFirst(request) : networkFirst(request));
});

async function cacheFirst(request) {
    const cache = await caches.open(cacheName);
    const cached = await cache.match(request);
    if (cached) return cached;
    const response = await fetch(request);
    if (response.ok) await cache.put(request, response.clone());
    return response;
}

async function networkFirst(request) {
    const cache = await caches.open(cacheName);
    try {
        const response = await fetch(request);
        if (response.ok) await cache.put(request, response.clone());
        return response;
    } catch (error) {
        const cached = await cache.match(request);
        if (cached) return cached;
        throw error;
    }
}
