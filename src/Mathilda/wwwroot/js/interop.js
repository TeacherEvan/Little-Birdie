// Mathilda browser interop. Loaded before the Blazor WASM bootstrap.
window.mathilda = window.mathilda || {};
// Resolves to "lat,lng" on success, or "denied:<reason>" when unavailable.
window.mathilda.getLocation = function () {
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
            { enableHighAccuracy: false, timeout: 10000, maximumAge: 60000 }
        );
    });
};
