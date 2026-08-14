import { query, mutation } from "./_generated/server";
import { v } from "convex/values";

// Returns all saved places. Called by C# via POST {DEPLOY_URL}/api/query
// body { "path": "places/list", "args": {}, "format": "json" }.
export const list = query({
  args: {},
  handler: async (ctx) => {
    const rows = await ctx.db.query("places").collect();
    return rows.map((r) => ({
      name: r.name,
      type: r.type,
      distanceKm: 0,
      openNow: true,
    }));
  },
});

// Adds a place. Called by C# via POST {DEPLOY_URL}/api/mutation
// body { "path": "places/add", "args": { name, type }, "format": "json" }.
export const add = mutation({
  args: { name: v.string(), type: v.string() },
  handler: async (ctx, args) => {
    return await ctx.db.insert("places", { name: args.name, type: args.type });
  },
});
