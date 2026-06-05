const CACHE_NAME = 'acueducto-cache-v2';
const ASSETS = [
  '/',
  '/index.html',
  '/planillas/consumos-wasm',
  '/_framework/blazor.web.js',
  '/js/offlineSync.js',
  '/js/charts.js',
  '/js/ImprimirFactura.js',
  '/js/html2pdf.bundle.min.js',
  '/js/pdf-export.js',
  '/fondologo-acueducto.jpg'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => cache.addAll(ASSETS)).then(self.skipWaiting())
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys().then(keys => Promise.all(keys.map(k => k !== CACHE_NAME ? caches.delete(k) : null)))
  );
  self.clients.claim();
});

function isNavigationRequest(request) {
  return request.mode === 'navigate' || (request.method === 'GET' && request.headers.get('accept')?.includes('text/html'));
}

self.addEventListener('fetch', event => {
  const req = event.request;
  if (req.method !== 'GET') return;

  if (isNavigationRequest(req)) {
    // Para navegación, devolver shell WASM si no hay red
    event.respondWith(
      fetch(req).catch(() => caches.match('/planillas/consumos-wasm') || caches.match('/index.html'))
    );
    return;
  }

  // Para recursos, cache-first con actualización en segundo plano
  event.respondWith(
    caches.match(req).then(cached => {
      const fetchPromise = fetch(req).then(networkRes => {
        const resClone = networkRes.clone();
        caches.open(CACHE_NAME).then(cache => cache.put(req, resClone)).catch(()=>{});
        return networkRes;
      }).catch(() => cached);
      return cached || fetchPromise;
    })
  );
});
