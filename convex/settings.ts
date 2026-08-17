import { query, mutation } from "./_generated/server";
import { v } from "convex/values";

export const get = query({
  args: { userId: v.string() },
  handler: async (ctx, args) => {
    return await ctx.db
      .query("settings")
      .filter((q) => q.eq(q.field("userId"), args.userId))
      .first();
  },
});

export const save = mutation({
  args: {
    userId: v.string(),
    lang: v.optional(v.string()),
    theme: v.optional(v.string()),
    currency: v.optional(v.string()),
    units: v.optional(v.string()),
    highAccuracyGps: v.optional(v.boolean()),
    skipStartupVideo: v.optional(v.boolean()),
    showInstallPrompt: v.optional(v.boolean()),
    mockLocationEnabled: v.optional(v.boolean()),
    mockCoordinates: v.optional(v.string()),
    enableDebugTelemetry: v.optional(v.boolean()),
  },
  handler: async (ctx, args) => {
    const existing = await ctx.db
      .query("settings")
      .filter((q) => q.eq(q.field("userId"), args.userId))
      .first();
    if (existing) {
      await ctx.db.patch(existing._id, {
        lang: args.lang,
        theme: args.theme,
        currency: args.currency,
        units: args.units,
        highAccuracyGps: args.highAccuracyGps,
        skipStartupVideo: args.skipStartupVideo,
        showInstallPrompt: args.showInstallPrompt,
        mockLocationEnabled: args.mockLocationEnabled,
        mockCoordinates: args.mockCoordinates,
        enableDebugTelemetry: args.enableDebugTelemetry,
      });
      return existing._id;
    }
    return await ctx.db.insert("settings", {
      userId: args.userId,
      lang: args.lang,
      theme: args.theme,
      currency: args.currency,
      units: args.units,
      highAccuracyGps: args.highAccuracyGps,
      skipStartupVideo: args.skipStartupVideo,
      showInstallPrompt: args.showInstallPrompt,
      mockLocationEnabled: args.mockLocationEnabled,
      mockCoordinates: args.mockCoordinates,
      enableDebugTelemetry: args.enableDebugTelemetry,
    });
  },
});
