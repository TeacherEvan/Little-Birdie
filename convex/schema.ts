import { defineSchema, defineTable } from "convex/server";
import { v } from "convex/values";

export default defineSchema({
  places: defineTable({
    name: v.string(),
    type: v.string(),
    lat: v.optional(v.number()),
    lng: v.optional(v.number()),
    addedBy: v.optional(v.string()),
  }),
  settings: defineTable({
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
  }),
});
