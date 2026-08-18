# Requirement Traceability Matrix

|| Objective | Requirement | Test | Evidence | Status ||
|---|---|---|---|---|
| OBJ-01 | Real geolocation → weather/attractions | `InteropContractTests.LocationService_RequestGpsAsync_ParsesLatLngObject`, `LocationTests.LocationPage_Acquired_ShowsCoordinates` | `LocationService.cs` + `interop.js` `geolocation.request` returns `{lat,lng}`; `MainLayout.HandleLocation` persists via `LocationService.SaveAsync` | DONE |
| OBJ-02 | Clear Offline Cache works | `InteropContractTests.StorageClear_IsInvokedAndScoped` | `interop.js` `mathilda.storage.clear` (scoped to `mathilda.*` keys); `PrivacySettingsTab.ClearCacheAsync` calls it | DONE |
| OBJ-03 | SW force-reload works | `InteropContractTests.ServiceWorkerUpdate_IsInvoked` | `interop.js` `mathilda.sw.update` defined (skipWaiting + registration.update); `AdvancedSettingsPanel.ForceReloadSwAsync` wired with status feedback | DONE |
| OBJ-04 | Install "Don't show again" persists | `InstallWizardModalTests` (render) | `InstallWizardModal.HandleDismissal`/`Dismiss` → `InstallPromptService.DismissInstallPromptAsync` persists `ShowInstallPrompt=false` | DONE |
| OBJ-05 | Custom Convex URL functional | `WeatherServiceTests`, `PlacesServiceTests` (mock retained) + build | `Program.cs` registers `ConvexClient` from `AppSettings.CustomConvexUrl` (null factory); consumed by Places/Weather services | DONE |
| OBJ-06 | Honest startup splash | `StartupVideoIntroTests.Render_ShowsSvgSplash_NotVideo` | `StartupVideoIntro.razor` SVG-only; dead `<video>` + `video.preload` removed | DONE |
| OBJ-07 | De-leak InstallPromptService | `InteropContractTests.InstallPromptService_Initialize_RegistersTypedCallback_NoEval` | `eval` removed; typed `registerCallbacks` + `DotNetObjectReference` disposed in `Dispose()`; implements `IDisposable` | DONE |
| OBJ-08 | Remove false-affordance fields | `AppSettingsServiceTests` (model round-trip) | 9 unused fields removed from `AppSettings`; `GeneralSettingsTab`/`AdvancedSettingsPanel` trimmed | DONE |
| OBJ-09 | Enforce consent downstream | build + `MainLayout` review | `MainLayout` loads `PrivacyConsentService` and persists location via `LocationService`; no telemetry/analytics code path exists in tree to gate (no-op by construction) | DONE (no telemetry code present) |
| OBJ-10 | Interop contract hygiene | `InteropContractTests` (all 5) | `docs/.scratch-audit/INTEROP-CONTRACT.md` written; `getLocation`/`video.preload` removed; every C#-called symbol exists | DONE |
| OBJ-11 | Dashboard/demo clutter | `OctagonDashboardTests.Renders_Eight_Tiles` | inline `<style>` moved to `app.css` `.octagon` block; dashboard styles no longer in component | DONE |
| OBJ-12 | SW versioning | build + `service-worker.js` read | `CACHE_VERSION` constant drives `CACHE_NAME`; no hard-coded literal | DONE |
| OBJ-13 | Test hygiene + regression | full suite | xUnit1031 fixed (`OnAppInstalled_UpdatesState` async); 5 new interop-contract tests; suite 35 → 42 green | DONE |
| OBJ-14 | Phase 4: Design Tokens & CSS Modernization | Status bar, quick chips, SVG icons, animated tiles, install banner, dashboard footer | `OctagonDashboardTests.Renders_Eight_Tiles`, build | `app.css` added status-bar, quick-chips, tile-icon, tile-rise animation, install-banner, dashboard-footer; all styles in CSS | DONE |
| OBJ-15 | Phase 4: Upgraded OctagonDashboard Component | Status header, animated SVG tiles, currency/weather chips, install banner | `OctagonDashboardTests.Renders_Eight_Tiles`, build | `OctagonDashboard.razor` rewritten with status bar, install banner, quick chips, 8 SVG-icon tiles, dashboard footer | DONE |
| OBJ-16 | WeatherPage & AttractionsPage use LocationService | Real user location for weather/attractions | `LocationTests.LocationPage_Acquired_ShowsCoordinates`, build | Both pages inject `LocationService`, load saved location, display dynamic coords + live/mock indicator | DONE |
| OBJ-17 | Phase 6.4: Release cleanup (v0.2.0 tag) | README updated, git tag created | build, `git tag -l` | `README.md` updated for v0.2.0; git tag `v0.2.0` created and pushed | DONE |

## Rule
No requirement (REQ-/NFR-/AC-) is considered satisfied without evidence.
Every Objective traces to at least one Requirement and one Evidence entry. **All 17 objectives DONE.**
