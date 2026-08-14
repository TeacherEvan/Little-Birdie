# Mathilda — C# Rebuild of Quicky (Vercel + Convex) Implementation Plan

> **For Hermes:** Execute task-by-task via Hermes tooling. The previously-referenced
> `subagent-driven-development` skill does NOT exist in this environment; the
> `writing-plans-enhanced` skill mandates a `superpowers:executing-plans` pointer, but
> that sub-skill is not loaded here either. Use `delegate_task` (one leaf subagent per
> task, review between tasks) or direct tool calls. Reconcile every task against the live
> tree before claiming completion (see "Current State Reconciliation").

**Goal:** Rebuild the Quicky Thailand-travel utility as a C# Blazor WebAssembly app deployable to Vercel, backed by a Convex data layer reached from C# over the Convex HTTP REST API (no C# SDK exists).

**Architecture:** Static Blazor WebAssembly front-end (pure C#, compiled to WASM, served as static files by Vercel — Vercel cannot run .NET servers). A thin C# `ConvexClient` service calls the Convex HTTP API (`/api/query`, `/api/mutation`) for server-persisted data. Quicky is client-only with MOCK services; Mathilda preserves the feature set and migrates mock-able data (saved places, user settings) into Convex documents. Device capabilities map to web APIs: Geolocation API (GPS), `navigator.mediaDevices` (camera), `window.open` (URL launch). `installed_apps`/secure-storage have no browser equivalent → degrade gracefully.

**Tech Stack:**
- .NET 8 SDK (project targets `net8.0`; CI pins `dotnet-version: '8.0.x'`, local SDK is 10.0.110 — both build net8.0 fine)
- Blazor WebAssembly (`Microsoft.AspNetCore.Components.WebAssembly` 8.0.0)
- `System.Net.Http.Json` for Convex HTTP calls
- xUnit 2.6.6 + `bunit` 1.28.9 for unit/component tests (runs via `dotnet test`)
- Vercel (static hosting; `vercel.json` + `publish/wwwroot` output)
- Convex (HTTP API data layer; schema in `convex/`)

**Effort:** ~2 weeks (largely COMPLETE as of 2026-08-15; see reconciliation) | **Surfaces touched:** 1 (.NET solution: `src/Mathilda`, `tests/Mathilda`) | **New tables:** 2 (`places`, `settings` in `convex/schema.ts`) | **Feature flag:** none (single static deploy)

---

## Current State Reconciliation (ground truth from live tree + `dotnet test`)

Verified against the working tree on `main` (commit `8456b8c`). 7/7 tests pass.

**DONE (live, committed):**
- Phase 0: Repo scaffold, `Mathilda.sln`, `src/Mathilda` + `tests/Mathilda/Mathilda.Tests.csproj` (+ `.sln`/dirs). Paths differ from the original plan's `/home/android` fiction — authoritative layout is below.
- Phase 1: Models `Attraction`, `WeatherSnapshot`, `TripCostEntry`, `CostResult` + 3 model test files (all green).
- Phase 2: `ConvexClient` (HTTP base), `PlacesService`, `WeatherService` + `PlacesServiceTests`, `WeatherServiceTests`. `convex/schema.ts`, `convex/places.ts`, `convex/settings.ts`, `convex/README.md`.
- Phase 3: `App.razor` (Router inline, no `Routes.razor`), `MainLayout.razor`, and 14 razor files: `OctagonDashboard`, `SplashPage`, `AttractionsPage`, `WeatherPage`, `CostPage`, `BankingPage`, `BathroomPage`, `BoltPage`, `CounterPage`, `LocationPage`, `SettingsPage`. `OctagonDashboardTests` present (the 7th passing test).
- Phase 4: `vercel.json` (output `publish/wwwroot`), `.vercelignore`, `convex/README.md`, root `README.md`, `.github/workflows/build.yml` (dotnet 8.0.x, restore/build/test).

**REMAINING / GAPS (open work):**
- **G1 — `ConvexClientTests.cs` missing.** Plan Task 2.1 specifies a unit test for `ConvexClient.QueryAsync` via a mocked `HttpMessageHandler`; it was never written. 7 tests exist but none cover `ConvexClient`.
- **G2 — i18n not implemented as specced.** Task 3.7 specified `Localization.resx` (en + th) + `AppTheme.cs` resource-manager switching. Actual `SettingsPage.razor` is a plain `<select @bind>` with no resource manager and no theme application beyond a string. Decide: implement resx i18n (Task 3.7 rework) OR revise the plan to drop i18n and document Settings as en/th-by-binding only.
- **G3 — Component structure differs from plan.** Plan named separate `OctagonTile.razor`, `AttractionCard.razor`, `CostResultCard.razor`; actual code inlines these into `OctagonDashboard.razor`, `AttractionsPage.razor`, `CostPage.razor`. Functionally equivalent; update plan wording to match reality.
- **G4 — `LocationPage` geolocation JSInterop unverified.** Plan Phase 3.6 says `Location` uses `navigator.geolocation` via JSInterop; confirm the interop exists and degrades when denied.
- **G5 — Phase 5 final review incomplete:** `dotnet publish -c Release -o publish` not yet exercised to confirm `publish/wwwroot` is Vercel-ready; no `v0.1.0` tag.

---

## Milestone Timeline

The rebuild is already past M1–M4. Remaining work is the gaps above plus final release.

### Milestone 1: Scaffold + Domain (DONE)
Solution, Blazor WASM project, xUnit/bunit, 4 domain models + tests.

### Milestone 2: Convex Data Layer (DONE except G1)
`ConvexClient` HTTP base, `PlacesService`, `WeatherService` (+ fallback), convex schema/functions. **Open: G1 (ConvexClient test).**

### Milestone 3: UI Port (DONE except G2/G3)
App shell, octagon dashboard, 8 feature pages, settings, splash. **Open: G2 (i18n), G3 (component naming), G4 (location interop).**

### Milestone 4: Deploy Config (DONE)
Vercel static config, Convex wiring docs, CI build+test.

### Milestone 5: Hardening + Release (REMAINING)
- Fix G1: add `ConvexClientTests.cs`.
- Resolve G2: implement resx i18n OR formally descope.
- Resolve G3/G4: align plan to actual components; verify location interop.
- G5: `dotnet publish` smoke test + `v0.1.0` tag.

---

## Data Flow

### Convex Query Path (C# → HTTP → Convex)
```
Client (Blazor WASM)          ConvexClient                Convex HTTP
  │                                │                          │
  │ FetchNearby(radiusKm)          │                          │
  ├──────────────────────────────►│ POST {DEPLOY_URL}/api/query
  │                                ├─────────────────────────►│ query("places/list", {radiusKm})
  │                                │   body {path,args,format:"json"}
  │                                │◄─────────────────────────┤ scans `places` table
  │                                │◄── {status:"success",value:[...]}
  │  List<Attraction> (mapped)     │                          │
  │◄───────────────────────────────┤                          │
  │  render cards                  │                          │
```
Fallback: when `CONVEX_DEPLOYMENT` (deploy URL) is empty, `WeatherService` returns a mock snapshot (tempC 31, "Sunny") — preserving Quicky client-only behavior.

---

## Mockups (text/ASCII — layout intent, not pixel-final)

### Octagon dashboard (port of `octagon_tile.dart`)
```
        [Attractions]   [Weather]
              \           /
        [Cost] — [ MATHILDA ] — [Banking]
              /           \
        [Bathroom]     [Bolt]
              \           /
        [Counter]     [Location]   (+ Settings, Splash off-grid)
```
Each tile = clip-path octagon `<a href="/route">` with label; verified by `OctagonDashboardTests` (8 tiles, expected labels).

### Settings (current reality — G2 open)
```
┌──────────────────────────┐
│ Settings                 │
│ Language [English ▾]     │  <- @bind Language ("en"/"th")
│ Theme    [Light   ▾]     │  <- @bind Theme ("light"/"dark")
│ [Save]                   │
│ Saved: lang=en, theme=light │
└──────────────────────────┘
```
i18n via `.resx` (specced, not built) OR accept as static en/th select (descope).

---

## Risk Table

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| `ConvexClient` untested (G1) → silent envelope/HTTP breakage | Medium | High | Add `ConvexClientTests.cs` (mock HttpMessageHandler) before release |
| i18n scope creep (G2) — resx not started, blocks Settings spec | Medium | Medium | Decide implement-vs-descope this milestone; do not leave half-done |
| Vercel serves stale WASM (no cache-bust) on redeploy | Medium | Medium | Verify `publish/wwwroot` asset hashing in `dotnet publish` smoke test (G5) |
| `LocationPage` geolocation denied/unsupported → silent no-op (G4) | Medium | Low | Degrade to "location unavailable" message; verify interop |
| CI pins dotnet 8.0.x while local is 10.0.110 → drift confusion | Low | Low | Both target net8.0; keep CI pin, note local SDK in README |

---

## Phase 0 — Repository scaffold (DONE)

- Repo: `https://github.com/TeacherEvan/Mathilda.git` cloned to `/home/ewaldt/Documents/VS/Other/Mathilda`.
- `Mathilda.sln`; `src/Mathilda` (Blazor WASM), `tests/Mathilda` (xUnit/bunit), `convex/`, `docs/`.
- `src/Mathilda/Mathilda.csproj` (`Microsoft.NET.Sdk.BlazorWebAssembly`, `net8.0`, `Nullable`/`ImplicitUsings` enabled).
- `tests/Mathilda/Mathilda.Tests.csproj` (`Microsoft.NET.Sdk.Razor`, xUnit 2.6.6, bunit 1.28.9).

## Phase 1 — Domain models (DONE, tested)

- `src/Mathilda/Models/Attraction.cs` (`record Attraction(string Name, double DistanceKm, string Type, bool OpenNow)`) + `tests/Mathilda/Models/AttractionTests.cs`.
- `src/Mathilda/Models/WeatherSnapshot.cs` (`record WeatherSnapshot(double TempC, string Condition, string[] Forecast)`) + `WeatherSnapshotTests.cs`.
- `src/Mathilda/Models/TripCostEntry.cs` + `CostResult.cs` + `TripCostEntryTests.cs`.
- Verify: `dotnet test` → 3 model tests green.

## Phase 2 — Convex data layer (DONE except G1)

### Task 2.1: ConvexClient base (HTTP) — DONE, NEEDS TEST (G1)
- `src/Mathilda/Services/ConvexClient.cs`: `QueryAsync<T>(path, args?)` / `MutationAsync<T>(path, args?)` POST to `{deployUrl}/api/query|mutation` with `{path, args, format:"json"}`, reads `{status, value}`. Real signature uses `object? args = null` and `ConvexEnvelope<T>`; plan's original snippet (`ConvexClient(HttpClient, string)` ctor, no nullable default) is accurate on ctor, add the nullable default.
- **G1 (TODO):** add `tests/Mathilda/Services/ConvexClientTests.cs` — mock `HttpMessageHandler` returns `{"status":"success","value":[{"name":"Mock Cafe"}]}`; assert `QueryAsync<List<Attraction>>("places/list", new {})` deserializes 1 item. Then `dotnet test` → 8 green.

### Task 2.2: PlacesService over Convex (DONE)
- `src/Mathilda/Services/PlacesService.cs` wraps `ConvexClient.QueryAsync<List<Attraction>>("places/list", new { radiusKm })`; `tests/Mathilda/Services/PlacesServiceTests.cs` (green).

### Task 2.3: WeatherService over Convex + fallback (DONE)
- `src/Mathilda/Services/WeatherService.cs`: empty deploy URL → mock snapshot (tempC 31, "Sunny"); else query. `WeatherServiceTests.cs` (green).

### Task 2.4: Convex schema (DONE)
- `convex/schema.ts` (`places`: name, type, lat?, lng?, addedBy?; `settings`: userId, lang?, theme?), `places.ts` (list/add), `settings.ts`. `convex/README.md` documents `npx convex dev`.

## Phase 3 — UI (DONE except G2/G3/G4)

### Task 3.1: App shell + routing (DONE)
- `src/Mathilda/App.razor` uses `Router` directly (no `Routes.razor`). Routes: `/` Dashboard, `/attractions`, `/weather`, `/cost`, `/banking`, `/bathroom`, `/bolt`, `/counter`, `/location`, `/settings`, `/splash`. `MainLayout.razor`.

### Task 3.2: Octagon dashboard (DONE)
- `src/Mathilda/Pages/OctagonDashboard.razor` (+ css) — clip-path octagon grid of 8 nav tiles (inlined; no separate `OctagonTile.razor` — G3). `tests/Mathilda/Pages/OctagonDashboardTests.cs` asserts 8 tiles (green).

### Task 3.3: Attractions page (DONE)
- `src/Mathilda/Pages/AttractionsPage.razor` — `@foreach` over `PlacesService.FetchNearby`, card markup inlined (no `AttractionCard.razor` — G3).

### Task 3.4: Weather page (DONE)
- `src/Mathilda/Pages/WeatherPage.razor` — renders temp + condition from `WeatherService`.

### Task 3.5: Cost calculator page (DONE)
- `src/Mathilda/Pages/CostPage.razor` — input amount/currency/category → `CostResult` (result card inlined, no `CostResultCard.razor` — G3).

### Task 3.6: Banking / Bathroom / Bolt / Counter / Location (DONE, G4 open)
- One `.razor` per feature under `src/Mathilda/Pages/`. `BathroomPage` lists mock nearest; `BoltPage`/`CounterPage` stateful UI. **G4:** confirm `LocationPage` uses `navigator.geolocation` via JSInterop and degrades gracefully.

### Task 3.7: Settings i18n + theme (PARTIAL — G2 open)
- Current: `src/Mathilda/Pages/SettingsPage.razor` plain `<select @bind>` for Language (en/th) + Theme (light/dark), no `.resx`, no `AppTheme.cs`. **Decision required (G2):** implement `Localization.resx` (en+th) + resource-manager switching + `AppTheme.cs` (as originally specced), OR descope i18n and document Settings as en/th-by-binding. Until decided, this task is INCOMPLETE.

### Task 3.8: Splash screen (DONE)
- `src/Mathilda/Pages/SplashPage.razor` — loading state, redirect to `/` after init.

## Phase 4 — Deploy config (DONE)

### Task 4.1: Vercel static config (DONE)
- `vercel.json`: `buildCommand: dotnet publish src/Mathilda/Mathilda.csproj -c Release -o publish`, `outputDirectory: publish/wwwroot`, SPA rewrite `/(.*) → /index.html`. `.vercelignore` present.

### Task 4.2: Convex deploy wiring (DONE)
- `convex/README.md` + root `README.md` document `npx convex dev` + `CONVEX_DEPLOYMENT` env in Vercel.

### Task 4.3: CI build check (DONE)
- `.github/workflows/build.yml`: ubuntu-latest, `actions/setup-dotnet@v4` `dotnet-version: '8.0.x'`, restore → `dotnet build --no-restore -c Release` → `dotnet test --no-build -c Release`. Green on push/PR.

## Phase 5 — Final review (REMAINING — G5)

1. Close G1: add + pass `ConvexClientTests.cs` (target 8 green).
2. Close G2: i18n decision implemented or formally descoped.
3. Close G3/G4: plan wording aligned to actual components; location interop verified.
4. `dotnet test` → all green.
5. `dotnet publish src/Mathilda/Mathilda.csproj -c Release -o publish` → confirm `publish/wwwroot` produced (static, Vercel-ready) (G5).
6. Push `main`; tag `v0.1.0`.

---

## Hard constraints (do NOT violate)
- Vercel hosts ONLY static output (`publish/wwwroot`). No server-side .NET.
- Convex reached ONLY via HTTP API from C# (no C# SDK exists).
- No browser API for `installed_apps` → omit; for `flutter_secure_storage` → use localStorage with note.
- Secrets (Convex deploy key) via Vercel env vars, never committed.

## Verification gate per task
- `dotnet test` green for that task's tests (CI mirrors locally).
- `dotnet build` succeeds for the whole solution after each Phase-3 task.
- Reconcile against the live tree (`git status` / `find src tests`) before marking any task DONE — do not trust the plan's own status.
- No `dotnet build` warnings treated as errors unless Phase complete.
