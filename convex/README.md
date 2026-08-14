# Convex backend

Deploys via `npx convex dev` (requires CONVEX_DEPLOY_KEY). Schema + functions in this dir.
The C# front-end calls these over the Convex HTTP API:
- query: POST {DEPLOY_URL}/api/query  { path: "places/list", args: {}, format: "json" }
- mutation: POST {DEPLOY_URL}/api/mutation { path: "places/add", args: { name, type }, format: "json" }
Set CONVEX_DEPLOYMENT as a Vercel env var at build time.
