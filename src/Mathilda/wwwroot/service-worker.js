// Mathilda Service Worker - Cache-first strategy for offline PWA
const CACHE_NAME = 'mathilda-cache-v0.2.0';
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/manifest.json',
  '/css/app.css',
  '/js/interop.js',
  // Framework and WASM assets will be cached dynamically
];

// Install event - cache static assets
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      console.log('[SW] Caching static assets');
      return cache.addAll(STATIC_ASSETS.map(url => new Request(url, { credentials: 'same-origin' })));
    }).then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames
          .filter((name) => name !== CACHE_NAME)
          .map((name) => {
            console.log('[SW] Deleting old cache:', name);
            return caches.delete(name);
          })
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch event - cache-first for static assets, network-first for API
self.addEventListener('fetch', (event) => {
  const url = new URL(event.request.url);
  
  // Skip non-GET requests
  if (event.request.method !== 'GET') return;
  
  // Skip cross-origin requests (Convex API, etc.)
  if (url.origin !== location.origin) {
    // Network-first for API calls
    event.respondWith(
      fetch(event.request).catch(() => {
        // Offline fallback for API would need specific handling
        return new Response(JSON.stringify({ error: 'Offline' }), {
          headers: { 'Content-Type': 'application/json' }
        });
      })
    );
    return;
  }

  // Check if it's a static asset (_framework, css, js, media, icons)
  const isStaticAsset = url.pathname.startsWith('/_framework/') ||
                        url.pathname.startsWith('/css/') ||
                        url.pathname.startsWith('/js/') ||
                        url.pathname.startsWith('/media/') ||
                        url.pathname.startsWith('/icons/') ||
                        url.pathname === '/' ||
                        url.pathname === '/index.html' ||
                        url.pathname === '/manifest.json';

  if (isStaticAsset) {
    // Cache-first for static assets
    event.respondWith(
      caches.match(event.request).then((cachedResponse) => {
        if (cachedResponse) {
          return cachedResponse;
        }
        return fetch(event.request).then((networkResponse) => {
          // Cache successful responses
          if (networkResponse.ok) {
            const responseClone = networkResponse.clone();
            caches.open(CACHE_NAME).then((cache) => {
              cache.put(event.request, responseClone);
            });
          }
          return networkResponse;
        });
      })
    );
  } else {
    // Network-first for navigation and other requests
    event.respondWith(
      fetch(event.request)
        .then((networkResponse) => {
          // Cache successful navigation responses
          if (networkResponse.ok && (event.request.mode === 'navigate' || url.pathname === '/')) {
            const responseClone = networkResponse.clone();
            caches.open(CACHE_NAME).then((cache) => {
              cache.put(event.request, responseClone);
            });
          }
          return networkResponse;
        })
        .catch(() => {
          // Offline fallback - try cache
          return caches.match(event.request).then((cachedResponse) => {
            if (cachedResponse) return cachedResponse;
            // Fallback to index.html for SPA routing
            return caches.match('/index.html');
          });
        })
    );
  }
});

// Listen for messages from the client
self.addEventListener('message', (event) => {
  if (event.data === 'skipWaiting') {
    self.skipWaiting();
  }
  if (event.data === 'getVersion') {
    event.ports[0].postMessage({ version: CACHE_NAME });
  }
});