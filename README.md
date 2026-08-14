# Mathilda

C# Blazor WebAssembly rebuild of the Flutter "Quicky" Thailand-travel utility.

- **Front-end:** Blazor WebAssembly (.NET 8, C#) — compiles to static WASM, hosted on Vercel.
- **Data layer:** [Convex](https://convex.dev) (reactive DB), reached from C# via the Convex HTTP API (no C# SDK).
- **CI:** GitHub Actions builds and tests on every push (`dotnet build` + `dotnet test`).

## Run locally
```
dotnet run --project src/Mathilda/Mathilda.csproj
```

## Deploy
- Vercel: imports this repo, runs `dotnet publish` and serves `publish/wwwroot` statically.
- Convex: `npx convex dev` (needs `CONVEX_DEPLOY_KEY`). Set `CONVEX_DEPLOYMENT` as a Vercel env var.

## Status
Scaffold + domain models + Convex schema in place. UI components and deploy wiring pending.
