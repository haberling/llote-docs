const CACHE = "llote-docs-v1";

const PRECACHE = [
  "/",
  "/css/framework.css",
  "/css/theme.css",
  "/css/widgets/code.css",
  "/css/widgets/corner-tag.css",
  "/css/widgets/downloads.css",
  "/css/widgets/page-number.css",
  "/css/widgets/slideshow.css",
  "/js/notebook.js",
  "/js/router.js",
  "/js/widgets/downloads.js",
  "/js/widgets/slideshow.js",
  "/fonts/arvo-400.woff2",
  "/fonts/arvo-700.woff2",
  "/favicon.svg",
  "/img/icon-192.png",
  "/img/icon-512.png",
  "/content/manifest.json",
  "/manifest.webmanifest"
];

self.addEventListener("install", (event) => {
  event.waitUntil(
    caches.open(CACHE).then((cache) => cache.addAll(PRECACHE)).then(() => self.skipWaiting())
  );
});

self.addEventListener("activate", (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((key) => key !== CACHE).map((key) => caches.delete(key)))
    ).then(() => self.clients.claim())
  );
});

self.addEventListener("fetch", (event) => {
  const req = event.request;
  if (req.method !== "GET") return;
  const url = new URL(req.url);
  if (url.origin !== self.location.origin) return;

  event.respondWith(
    caches.open(CACHE).then(async (cache) => {
      try {
        const fresh = await fetch(req);
        if (fresh.ok) cache.put(req, fresh.clone());
        return fresh;
      } catch {
        const cached = await cache.match(req);
        if (cached) return cached;
        throw new Error("offline and uncached");
      }
    })
  );
});
