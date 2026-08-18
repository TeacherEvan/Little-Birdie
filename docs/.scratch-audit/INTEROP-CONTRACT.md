# Mathilda JS/C# Interop Contract

Single source of truth for the `window.mathilda.*` bridge consumed by Blazor WASM.
Every C#-called symbol MUST exist here; every exported symbol SHOULD be called by C#.
Last reconciled: 2026-08-18 (surgical-implementation OBJ-01..OBJ-12).

## Geolocation (`mathilda.geolocation`)
| Symbol | Args | Returns | Called by |
|---|---|---|---|
| `request(options?)` | `{enableHighAccuracy,timeout,maximumAge}` | `{lat,lng}` on success, or `{error}` on denial/unavailable | `LocationService.RequestGpsAsync` |
> Replaces the old `mathilda.getLocation` (returned `"lat,lng"` string) which no C# consumer could parse.

## PWA install (`mathilda.pwa`)
| Symbol | Args | Returns | Called by |
|---|---|---|---|
| `init()` | — | void (auto-runs on script load) | script load |
| `registerCallbacks(dotNetRef)` | `DotNetObjectReference` | void | `InstallPromptService.InitializeAsync` |
| `promptInstall()` | — | `{success, reason?}` | `InstallPromptService.PromptInstallAsync` |
| `getPlatformInfo()` | — | `PlatformInfo` | `InstallPromptService.InitializeAsync` |
> `onInstallPromptReady` / `onAppInstalled` are NOT global free functions anymore — they are
> invoked through the registered `DotNetObjectReference` (no `eval`, no leak — see OBJ-07).

## Storage (`mathilda.storage`)
| Symbol | Args | Returns | Called by |
|---|---|---|---|
| `getItem(key)` | string | string? | `LocalStore.LoadAsync` |
| `setItem(key,value)` | string,string | bool | `LocalStore.SaveAsync` |
| `removeItem(key)` | string | bool | (utility) |
| `clear()` | — | bool | `PrivacySettingsTab.ClearCacheAsync` (scoped to `mathilda.*` keys) |
> `clear()` only removes `mathilda.settings` and `mathilda.privacy.consent` — it never wipes
> unrelated `localStorage` entries.

## Service worker (`mathilda.sw`)
| Symbol | Args | Returns | Called by |
|---|---|---|---|
| `update()` | — | `{success, reason?}` | `AdvancedSettingsPanel.ForceReloadSwAsync` |
> Posts `skipWaiting` to a waiting worker and calls `registration.update()`.

## Removed / no longer emitted
- `mathilda.geolocation.getLocation` — superseded by `geolocation.request`.
- `mathilda.video.preload` — dead export; the startup `<video>` was removed (OBJ-06, SVG splash only).
- `mathilda.pwa.canInstall` / `mathilda.pwa.isStandalone` — were unused from C#; platform info is
  read via `getPlatformInfo()` returning the full `PlatformInfo` record.
