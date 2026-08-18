using Mathilda.Models;

namespace Mathilda.Services;

/// <summary>Port of Quicky's WeatherService. Mock fallback when no Convex URL.</summary>
public sealed class WeatherService
{
    private readonly ConvexClient? _convex;

    public WeatherService(ConvexClient? convex = null) => _convex = convex;

    public async Task<WeatherSnapshot> Fetch(double lat, double lng)
    {
        if (_convex is null)
        {
            return new WeatherSnapshot(31, "Sunny", new[] { "32°", "30°", "29°" });
        }

        var snap = await _convex.QueryAsync<WeatherSnapshot>("weather/get", new { lat, lng });
        return snap ?? new WeatherSnapshot(0, "", Array.Empty<string>());
    }

    /// <summary>Returns true if a Convex client is registered and available.</summary>
    public bool IsConvexConnected() => _convex != null;
}
