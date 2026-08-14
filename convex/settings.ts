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
  args: { userId: v.string(), lang: v.optional(v.string()), theme: v.optional(v.string()) },
  handler: async (ctx, args) => {
    const existing = await ctx.db
      .query("settings")
      .filter((q) => q.eq(q.field("userId"), args.userId))
      .first();
    if (existing) {
      await ctx.db.patch(existing._id, { lang: args.lang, theme: args.theme });
      return existing._id;
    }
    return await ctx.db.insert("settings", {
      userId: args.userId,
      lang: args.lang,
      theme: args.theme,
    });
  },
});
