# Mathilda — C# Rebuild of Quicky (Vercel + Convex) Implementation Plan

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Rebuild the Quicky Thailand-travel utility as a C# Blazor WebAssembly app deployable to Vercel, backed by a Convex data layer (C# → Convex HTTP REST API, since Convex has no C# SDK).

**Architecture:** Static Blazor WebAssembly front-end (pure C#, compiled to WASM, served as static files by Vercel — Vercel cannot run .NET servers). A thin C# `ConvexClient` service calls the Convex HTTP API (`/api/...` + REST mutation/query endpoints) for any server-persisted data. Quicky is currently client-only with MOCK services; Mathilda preserves the feature set and migrates the mock-able data (saved places, trip cost entries, user settings) into Convex documents. Device capabilities map to web APIs: Geolocation API (GPS), `navigator.mediaDevices` (camera), `window.open` (URL launch). `installed_apps`/secure-storage have no browser equivalent → degrade gracefully.

**Tech Stack:**
- .NET 8 SDK, Blazor WebAssembly (C#)
- `System.Net.Http.Json` for Convex HTTP calls
- `IndexedDb`/`localStorage` (`Microsoft.AspNetCore.Components.WebAssembly` + JSInterop) for client cache
- xUnit + `bunit` for unit/component tests (runs via `dotnet test`)
- Vercel (static hosting, `vercel.json` + `wwwroot` publish output)
- Convex (HTTP API data layer; schema in `convex/`)

---

## Phase 0 — Repository scaffold

### Task 0.1: Clone Mathilda and create solution
**Objective:** Establish the repo working tree and a .NET solution.
**Files:** Create `/home/android/Mathilda/Mathilda.sln`; dirs `src/Mathilda`, `tests/Mathilda.Tests`, `convex/`, `docs/`.
**Step 1:** Clone the reserved repo.
```
cd /home/android && git clone https://github.com/TeacherEvan/Mathilda.git && cd Mathilda
mkdir -p src/Mathilda tests/Mathilda.Tests convex docs
```
**Step 2:** Verify dotnet present.
```
dotnet --version   # expect 8.0.x
```
**Step 3:** Commit scaffold.
```
git add -A && git commit -m "chore: scaffold Mathilda repo"
```

### Task 0.2: Create Blazor WebAssembly project
**Objective:** Stand up the WASM app project.
**Files:** Create `src/Mathilda/Mathilda.csproj`.
**Step 1:** Write `Mathilda.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.DevServer" Version="8.0.0" PrivateAssets="all" />
  </ItemGroup>
</Project>
```
**Step 2:** `dotnet build src/Mathilda/Mathilda.csproj` → expect Build succeeded.
**Step 3:** Commit.

### Task 0.3: Add xUnit + bunit test project
**Objective:** Enable TDD for the rebuild.
**Files:** Create `tests/Mathilda.Tests/Mathilda.Tests.csproj`.
**Step 1:** Write csproj:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="bunit" Version="1.28.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Mathilda\Mathilda.csproj" />
  </ItemGroup>
</Project>
```
**Step 2:** `dotnet test tests/Mathilda.Tests` → expect 0 tests, Build succeeded.
**Step 3:** Commit.

---

## Phase 1 — Domain models (TDD)

### Task 1.1: Attraction model
**Objective:** Port `Attraction` (name, distanceKm, type, openNow).
**Files:** Create `src/Mathilda/Models/Attraction.cs`; Test `tests/Mathilda.Tests/Models/AttractionTests.cs`.
**Step 1 (failing test):**
```csharp
using Xunit;
public class AttractionTests {
    [Fact]
    public void Ctor_SetsAllFields() {
        var a = new Attraction("Cafe", 1.2, "cafe", true);
        Assert.Equal("Cafe", a.Name);
        Assert.Equal(1.2, a.DistanceKm);
        Assert.Equal("cafe", a.Type);
        Assert.True(a.OpenNow);
    }
}
```
**Step 2:** `dotnet test` → FAIL (type missing).
**Step 3 (impl):**
```csharp
namespace Mathilda.Models;
public record Attraction(string Name, double DistanceKm, string Type, bool OpenNow);
```
**Step 4:** `dotnet test` → PASS. Commit `feat: add Attraction model`.

### Task 1.2: WeatherSnapshot model
**Objective:** Port `WeatherSnapshot` (tempC, condition, forecast[]).
**Files:** Create `src/Mathilda/Models/WeatherSnapshot.cs`; test `WeatherSnapshotTests.cs`.
**Step 1 (test):** assert record holds tempC=31, condition="Sunny", forecast=["32°","30°","29°"].
**Step 2:** `dotnet test` → FAIL.
**Step 3 (impl):** `public record WeatherSnapshot(double TempC, string Condition, string[] Forecast);`
**Step 4:** PASS. Commit.

### Task 1.3: TripCostEntry + CostResult models
**Objective:** Port cost calculator domain (amount, currency, category, converted total).
**Files:** Create `src/Mathilda/Models/TripCostEntry.cs`, `CostResult.cs`; tests.
**Step 1 (test):** entry with amount=100, currency="THB", category="food" → CostResult.Total=100.
**Step 2:** FAIL.
**Step 3 (impl):** records with computed Total.
**Step 4:** PASS. Commit.

---

## Phase 2 — Convex data layer (C# → HTTP API)

### Task 2.1: ConvexClient base (HTTP)
**Objective:** Thin typed client over Convex HTTP API.
**Files:** Create `src/Mathilda/Services/ConvexClient.cs`; test `ConvexClientTests.cs`.
**Context:** Convex HTTP API endpoints:
- Query: `POST {DEPLOY_URL}/api/query` body `{ "path": "name", "args": {...}, "format": "json" }` → `{ "status": "success", "value": ... }`
- Mutation: `POST {DEPLOY_URL}/api/mutation` same shape.
Deploy URL from env `CONVEX_DEPLOYMENT` or config.
**Step 1 (test):** mock HttpMessageHandler returns `{"status":"success","value":[{"name":"Mock Cafe"}]}`; `QueryAsync<List<Attraction>>("places/list", new {})` deserializes to 1 item.
**Step 2:** FAIL.
**Step 3 (impl):**
```csharp
public class ConvexClient {
    private readonly HttpClient _http;
    private readonly string _deployUrl;
    public ConvexClient(HttpClient http, string deployUrl) { _http = http; _deployUrl = deployUrl; }
    public async Task<T?> QueryAsync<T>(string path, object args) {
        var body = new { path, args, format = "json" };
        var resp = await _http.PostAsJsonAsync($"{_deployUrl}/api/query", body);
        var raw = await resp.Content.ReadFromJsonAsync<ConvexEnvelope<T>>();
        return raw?.Value;
    }
    // MutationAsync analogous
}
public record ConvexEnvelope<T>(string Status, T? Value);
```
**Step 4:** PASS. Commit `feat: ConvexClient HTTP base`.

### Task 2.2: PlacesService over Convex
**Objective:** Replace mock `PlacesService` with Convex-backed equivalent.
**Files:** Create `src/Mathilda/Services/PlacesService.cs`; test `PlacesServiceTests.cs`.
**Step 1 (test):** with injected ConvexClient returning mock list → `FetchNearby(5)` returns 3 items and maps distanceKm.
**Step 2:** FAIL.
**Step 3 (impl):** wraps `ConvexClient.QueryAsync<List<Attraction>>("places/list", new { radiusKm })`.
**Step 4:** PASS. Commit.

### Task 2.3: WeatherService over Convex + fallback
**Objective:** Convex weather with mock fallback when unconfigured (preserve Quicky behavior).
**Files:** `src/Mathilda/Services/WeatherService.cs`; test.
**Step 1 (test):** no deploy URL → returns mock snapshot (tempC 31, "Sunny"). With client → returns API value.
**Step 2:** FAIL. **Step 3 (impl):** if `_deployUrl` empty return mock; else query. **Step 4:** PASS. Commit.

### Task 2.4: Convex schema (convex/)
**Objective:** Define server schema for persisted data.
**Files:** Create `convex/schema.ts`, `convex/places.ts`, `convex/settings.ts`.
**Step 1:** write `schema.ts` with `places` (name, type, lat, lng, addedBy), `settings` (userId, lang, theme).
**Step 2:** `convex/places.ts` exports `list`, `add` query/mutation returning JSON.
**Step 3:** Commit `feat: convex schema + query/mutation functions`.

---

## Phase 3 — UI (Blazor components, ported from Flutter)

### Task 3.1: App shell + routing
**Objective:** Port `main.dart` MaterialApp.router → Blazor `App.razor` + routes.
**Files:** `src/Mathilda/App.razor`, `Routes.razor`, `MainLayout.razor`.
**Step 1:** define routes: `/` (Dashboard), `/attractions`, `/weather`, `/cost`, `/banking`, `/bathroom`, `/bolt`, `/counter`, `/location`, `/settings`, `/splash`.
**Step 2:** `dotnet build` → succeeded.
**Step 3:** Commit.

### Task 3.2: Octagon dashboard layout
**Objective:** Port `dashboard_layout.dart` / `octagon_tile.dart`.
**Files:** `src/Mathilda/Components/OctagonDashboard.razor` (+ `.razor.css`), `OctagonTile.razor`.
**Step 1 (bunit test):** renders 8 tiles with expected labels.
**Step 2:** FAIL (no component). **Step 3 (impl):** CSS clip-path octagon grid of nav tiles. **Step 4:** PASS. Commit.

### Task 3.3: Attractions page
**Objective:** Port `attractions_page.dart` + `attraction_card.dart`.
**Files:** `AttractionsPage.razor`, `AttractionCard.razor`.
**Step 1 (test):** inject mock PlacesService → renders cards for each attraction.
**Step 2:** FAIL. **Step 3 (impl):** `@foreach` over `PlacesService.FetchNearby`. **Step 4:** PASS. Commit.

### Task 3.4: Weather page
**Objective:** Port `weather_page.dart`.
**Files:** `WeatherPage.razor`.
**Step 1 (test):** renders temp + condition from WeatherService.
**Step 2:** FAIL. **Step 3 (impl).** **Step 4:** PASS. Commit.

### Task 3.5: Cost calculator page
**Objective:** Port `cost_page.dart` + `cost_result_card.dart`.
**Files:** `CostPage.razor`, `CostResultCard.razor`.
**Step 1 (test):** input 100 THB → result card shows 100.
**Step 2:** FAIL. **Step 3 (impl).** **Step 4:** PASS. Commit.

### Task 3.6: Banking / Bathroom / Bolt / Counter / Location pages
**Objective:** Port remaining simple pages (degrade device-only features).
**Files:** one `.razor` per feature under `src/Mathilda/Components/Pages/`.
**Step 1:** each builds; `Location` uses `navigator.geolocation` via JSInterop; `Bathroom` lists mock nearest; `Bolt`/`Counter` are stateful UI.
**Step 2:** `dotnet build` → succeeded. **Step 3:** Commit `feat: port remaining feature pages`.

### Task 3.7: Settings (i18n + theme)
**Objective:** Port `settings_page.dart` + l10n (en/th).
**Files:** `SettingsPage.razor`, `Localization.resx` (en + th), `AppTheme.cs`.
**Step 1 (test):** switching lang updates a rendered string.
**Step 2:** FAIL. **Step 3 (impl):** resource manager + theme toggle. **Step 4:** PASS. Commit.

### Task 3.8: Splash screen
**Objective:** Port `splash_page.dart` (startup animation).
**Files:** `SplashPage.razor`.
**Step 1:** renders loading state, redirects to `/` after init.
**Step 2:** `dotnet build` → succeeded. **Step 3:** Commit.

---

## Phase 4 — Deploy config (Vercel static + Convex)

### Task 4.1: Vercel static config
**Objective:** Serve Blazor WASM as static files on Vercel.
**Files:** Create `vercel.json`, `.vercelignore`.
**Step 1:** write `vercel.json`:
```json
{
  "buildCommand": "dotnet publish src/Mathilda/Mathilda.csproj -c Release -o publish",
  "outputDirectory": "publish/wwwroot",
  "rewrites": [{ "source": "/(.*)", "destination": "/index.html" }]
}
```
**Step 2:** Commit `feat: vercel static config for Blazor WASM`.

### Task 4.2: Convex deploy wiring
**Objective:** Document + env wiring for Convex.
**Files:** Create `convex/README.md`, update root `README.md`.
**Step 1:** document `npx convex dev` to push schema; set `CONVEX_DEPLOYMENT` env in Vercel.
**Step 2:** Commit.

### Task 4.3: CI build check
**Objective:** GitHub Actions builds the app on push.
**Files:** `.github/workflows/build.yml`.
**Step 1:** workflow runs `dotnet build` + `dotnet test` on ubuntu with dotnet 8.
**Step 2:** push, confirm green. **Step 3:** Commit `ci: build + test workflow`.

---

## Phase 5 — Final review
- Run full `dotnet test` → all green.
- `dotnet publish` → `publish/wwwroot` produced (static, Vercel-ready).
- Dispatch integration reviewer (final task of subagent-driven-development).
- Push `main`; tag `v0.1.0`.

---

## Hard constraints (do NOT violate)
- Vercel hosts ONLY static output (`wwwroot`). No server-side .NET.
- Convex reached ONLY via HTTP API from C# (no C# SDK exists).
- No browser API for `installed_apps` → omit; for `flutter_secure_storage` → use localStorage with note.
- Secrets (Convex deploy key) via Vercel env vars, never committed.

## Verification gate per task
- `dotnet test` green for that task's tests.
- `dotnet build` succeeds for the whole solution after each Phase-3 task.
- No `dotnet build` warnings treated as errors unless Phase complete.
