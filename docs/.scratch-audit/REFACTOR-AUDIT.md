# Mathilda — Refactoring Audit (prioritized)

Method: read every `.cs`/`.razor`/`.js` file fresh from disk; cross-checked every
C# JSInterop call against symbols actually defined in `interop.js`; built
`src/Mathilda` (0 errors) and `tests/Mathilda` (1 warning) for ground truth.

Rank: functional breakage > dead/no-op UI > dead code & unused model surface >
polish.

---

## 1. LocationPromptModal.razor + wwwroot/js/interop.js  (BROKEN — core feature)
- `RequestGpsAsync` calls `mathilda.geolocation.request` (modal line 56) but
  `interop.js` only defines `mathilda.getLocation`. The GPS success path can never
  parse; every tap falls through to manual city select.
- Even when a location IS chosen, `MainLayout.HandleLocation` is a no-op
  (`StateHasChanged()` only) — coords are discarded. No `LocationService` exists,
  so the "nearby attractions / weather" promise is unfulfilled end-to-end.
- Fix: rename interop to one consistent contract (`mathilda.geolocation.request`
  returning `{lat,lng}`), add a `LocationService` that persists the choice, and
  have `WeatherPage`/`AttractionsPage` consume it instead of hardcoding Bangkok.

## 2. AdvancedSettingsPanel.razor + Program.cs + ConvexClient.cs + Places/WeatherService  (DEAD DATA PATH)
- `ConvexClient` is never registered in DI; `PlacesService`/`WeatherService` always
  receive `convex: null` → always return mock data. "Custom Convex URL" is saved to
  `AppSettings` but nothing reads it; the Ping button works (plain HttpClient) but
  the URL has zero effect on data fetching.
- `ForceReloadSwAsync` calls `mathilda.sw.update`, which is undefined in `interop.js`
  → swallowed exception, dead button.
- Fix: register `ConvexClient` from `AppSettings.CustomConvexUrl` (or a null
  factory); read the URL in `PlacesService`/`WeatherService`; define
  `mathilda.sw.update` (post `skipWaiting` / call `registration.update()`).

## 3. StartupVideoIntro.razor + wwwroot/media  (NON-FUNCTIONAL PHASE)
- References `media/startup-intro.webm` and `.mp4`, but the folder contains only
  `startup-intro.svg`. The `<video>` 404s → `@onerror` → always shows the SVG.
  The startup *video* (a full Phase) never actually plays.
- `mathilda.video.preload` in `interop.js` is defined but never called.
- Fix: ship the real video assets, or downgrade the feature to the SVG splash and
  remove the dead `<video>` + `preload` interop.

## 4. PrivacySettingsTab.razor + interop.js  (BROKEN BUTTON)
- `ClearCacheAsync` calls `mathilda.storage.clear` (line 56); `interop.js` only
  defines `getItem`/`setItem`/`removeItem`. Always throws → "Failed" message.
- Fix: add `mathilda.storage.clear` to `interop.js`, or scope the clear to known
  keys (`mathilda.settings`, `mathilda.privacy.consent`).

## 5. InstallWizardModal.razor  (DEAD UX — "Don't show again")
- `HandleDismissal` (line 186) is never wired to any control. The `DontShowAgain`
  checkbox only toggles a field; checking it does nothing. The X / "Remind Me
  Later" buttons call `CloseModal`/`RemindLater` with no persistence.
- Dismissal is only persisted when a native install is *dismissed* (reason==
  "dismissed") inside `InstallPromptService`. So the headline "Don't show again"
  control is non-functional.
- Fix: bind the checkbox to `HandleDismissal`; persist via
  `InstallPromptService.DismissInstallPromptAsync()`.

## 6. Models/AppSettings.cs  (FALSE AFFORDANCES — large unused surface)
- Unused fields with no consumer anywhere (grep-confirmed): `Language`, `Theme`,
  `Currency`, `UnitSystem`, `MockLocationEnabled`, `MockCoordinates`,
  `HighAccuracyGps`, `GpsTimeoutSeconds`, `EnableDebugTelemetry`.
- The settings UI lets users change them; nothing in the app reads them (no
  localization, theming, currency/unit conversion, telemetry, or mock-location).
- Fix: either wire each to real behavior or strip from model + UI to avoid
  misleading controls.

## 7. Services/InstallPromptService.cs  (LEAK + FRAGILE INTEROP)
- `InitializeAsync` creates a `DotNetObjectReference` inside an `eval` string and
  never disposes it → app-lifetime leak.
- Uses `eval` and races `interop.js` auto-`init()` (handler may not be set when
  `beforeinstallprompt` fires). `GetSettingsAsync`/`ResetDismissalAsync` duplicate
  `AppSettingsService` surface.
- Fix: drop `eval`, register a permanent JS callback via a typed interop wrapper;
  dispose the `DotNetObjectReference` on a `Dispose()`; collapse the settings
  reads to `AppSettingsService` only.

## 8. MainLayout.razor  (NO-OP ONBOARDING HANDLERS)
- `HandleLocation` / `HandleLocationDismissed` do nothing; consent is shown but
  never enforced downstream (analytics/telemetry gating absent).
- Fix: route location into a `LocationService`; gate telemetry on consent.

## 9. wwwroot/js/interop.js  (CONTRACT DRIFT)
- Called-from-C# but MISSING: `mathilda.geolocation.request`, `mathilda.storage.clear`,
  `mathilda.sw.update`.
- Defined-but-UNUSED from C#: `mathilda.video.preload`, `mathilda.pwa.canInstall`,
  `mathilda.pwa.isStandalone`.
- Fix: make the JS/C# contract explicit (one doc table) and remove both gaps.

## 10. OctagonDashboard.razor + placeholder pages  (CLUTTER)
- Inline `<style>` + hardcoded tile coordinates; shipped demo pages
  (`CounterPage`, `BankingPage`, `SplashPage` redirects immediately) add nav
  noise. `SplashPage` is dead (instant redirect to `/`).
- Fix: extract dashboard styles to `app.css`; drop or clearly mark demo pages.

## 11. wwwroot/service-worker.js  (MINOR)
- Cache version `v0.2.0` hardcoded; no client posts `skipWaiting`; the UI reload
  hook (`mathilda.sw.update`) is undefined (see #2).

## 12. Tests — InstallPromptServiceTests.cs  (WARNING)
- xUnit1031: blocking `.Result`/`.Wait` in a test method (line 96). Make the test
  async + await.

---
Build evidence: `dotnet build src/Mathilda -c Release` → 0 error / 0 warning.
`dotnet build tests/Mathilda -c Release` → 1 warning (xUnit1031).
