import { query } from "./_generated/server";
import { v } from "convex/values";

// Returns a weather snapshot for the given coordinates.
// Called by C# via POST {DEPLOY_URL}/api/query
// body { "path": "weather:get", "args": { lat, lng }, "format": "json" }.
// TODO: replace the static snapshot with a real provider (Open-Meteo, etc.)
// once a key/cron is wired; the C# WeatherService already handles the shape.
export const get = query({
  args: { lat: v.number(), lng: v.number() },
  handler: async (_ctx, args) => {
    void args; // coordinates reserved for a future real provider
    return {
      tempC: 31,
      condition: "Sunny",
      forecast: ["32°", "30°", "29°"],
    };
  },
});
