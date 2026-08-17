// Mathilda browser interop. Loaded before the Blazor WASM bootstrap.
window.mathilda = window.mathilda || {};

// ==================== GEOLOCATION ====================
// Resolves to "lat,lng" on success, or "denied:<reason>" when unavailable.
window.mathilda.getLocation = function (options) {
    const opts = options || { enableHighAccuracy: false, timeout: 10000, maximumAge: 60000 };
    return new Promise(function (resolve) {
        if (!navigator.geolocation) {
            resolve("denied:unsupported");
            return;
        }
        navigator.geolocation.getCurrentPosition(
            function (pos) {
                resolve(pos.coords.latitude + "," + pos.coords.longitude);
            },
            function (err) {
                resolve("denied:" + (err && err.code));
            },
            opts
        );
    });
};

// ==================== PWA INSTALL ====================
window.mathilda.pwa = window.mathilda.pwa || {};
let deferredPrompt = null;
let platformInfo = null;

// Initialize platform detection and install prompt listener
window.mathilda.pwa.init = function () {
    // Detect platform
    const ua = navigator.userAgent;
    let platform = 'Other';
    if (/iPhone|iPad|iPod/i.test(ua) && /Safari/i.test(ua)) platform = 'iOS';
    else if (/Android/i.test(ua)) platform = 'Android';
    else if (/Chrome|Edg|Chromium/i.test(ua)) platform = 'DesktopChromium';

    // Detect standalone mode
    const isStandalone = window.matchMedia('(display-mode: standalone)').matches ||
                         window.navigator.standalone === true;

    platformInfo = { platform, isStandalone, canInstall: false };
    console.log('[PWA] Platform:', platformInfo);

    // Listen for beforeinstallprompt (Chromium desktop & Android)
    window.addEventListener('beforeinstallprompt', (e) => {
        e.preventDefault();
        deferredPrompt = e;
        platformInfo.canInstall = true;
        console.log('[PWA] Install prompt deferred');
        // Notify C# if needed
        if (window.mathilda.onInstallPromptReady) {
            window.mathilda.onInstallPromptReady(platformInfo);
        }
    });

    // Listen for appinstalled
    window.addEventListener('appinstalled', () => {
        deferredPrompt = null;
        platformInfo.canInstall = false;
        platformInfo.isStandalone = true;
        console.log('[PWA] App installed');
        if (window.mathilda.onAppInstalled) {
            window.mathilda.onAppInstalled();
        }
    });
};

// Check if install prompt is available
window.mathilda.pwa.canInstall = function () {
    return platformInfo ? platformInfo.canInstall : false;
};

// Trigger the native install prompt
window.mathilda.pwa.promptInstall = function () {
    return new Promise(async (resolve) => {
        if (!deferredPrompt) {
            resolve({ success: false, reason: 'no_prompt' });
            return;
        }
        try {
            await deferredPrompt.prompt();
            const choice = await deferredPrompt.userChoice;
            deferredPrompt = null;
            platformInfo.canInstall = false;
            if (choice.outcome === 'accepted') {
                resolve({ success: true });
            } else {
                resolve({ success: false, reason: 'dismissed' });
            }
        } catch (err) {
            resolve({ success: false, reason: err.message });
        }
    });
};

// Check if running in standalone mode
window.mathilda.pwa.isStandalone = function () {
    return platformInfo ? platformInfo.isStandalone : 
           window.matchMedia('(display-mode: standalone)').matches || 
           window.navigator.standalone === true;
};

// Get platform info
window.mathilda.pwa.getPlatformInfo = function () {
    if (!platformInfo) {
        window.mathilda.pwa.init();
    }
    return platformInfo;
};

// ==================== STORAGE ====================
window.mathilda.storage = window.mathilda.storage || {};

window.mathilda.storage.getItem = function (key) {
    try {
        return localStorage.getItem(key);
    } catch (e) {
        console.warn('[Storage] getItem failed:', e);
        return null;
    }
};

window.mathilda.storage.setItem = function (key, value) {
    try {
        localStorage.setItem(key, value);
        return true;
    } catch (e) {
        console.warn('[Storage] setItem failed:', e);
        return false;
    }
};

window.mathilda.storage.removeItem = function (key) {
    try {
        localStorage.removeItem(key);
        return true;
    } catch (e) {
        console.warn('[Storage] removeItem failed:', e);
        return false;
    }
};

// ==================== VIDEO PLAYBACK ====================
window.mathilda.video = window.mathilda.video || {};

window.mathilda.video.preload = function (src) {
    return new Promise((resolve) => {
        const video = document.createElement('video');
        video.src = src;
        video.muted = true;
        video.playsInline = true;
        video.preload = 'auto';
        video.onloadeddata = () => resolve({ success: true, duration: video.duration });
        video.onerror = (e) => resolve({ success: false, error: e.message });
        video.load();
    });
};

// Initialize on load
window.mathilda.pwa.init();