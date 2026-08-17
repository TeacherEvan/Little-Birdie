namespace Mathilda.Models;

/// <summary>
/// Canonical application settings for Mathilda (Phase 5 advanced settings hub).
/// Properties are settable so Blazor two-way (@bind) binding works in the settings UI.
/// </summary>
public sealed record AppSettings
{
    /// <summary>UI language code ("en", "th").</summary>
    public string Language { get; set; } = "en";

    /// <summary>Theme: "light", "dark", or "system".</summary>
    public string Theme { get; set; } = "system";

    /// <summary>Display currency (e.g. "THB", "USD", "EUR", "GBP").</summary>
    public string Currency { get; set; } = "THB";

    /// <summary>Unit system: "metric" or "imperial".</summary>
    public string UnitSystem { get; set; } = "metric";

    /// <summary>Bypass the themed startup animation on launch.</summary>
    public bool SkipStartupVideo { get; set; }

    /// <summary>Show the PWA install wizard prompt.</summary>
    public bool ShowInstallPrompt { get; set; } = true;

    /// <summary>Optional override for the Convex deployment URL.</summary>
    public string? CustomConvexUrl { get; set; }

    /// <summary>Request high-accuracy geolocation fixes.</summary>
    public bool HighAccuracyGps { get; set; }

    /// <summary>Geolocation fix timeout in seconds.</summary>
    public int GpsTimeoutSeconds { get; set; } = 10;

    /// <summary>Inject a simulated location instead of real GPS.</summary>
    public bool MockLocationEnabled { get; set; }

    /// <summary>Simulated "lat,lng" coordinates when mocking.</summary>
    public string? MockCoordinates { get; set; }

    /// <summary>Capture and surface diagnostic telemetry.</summary>
    public bool EnableDebugTelemetry { get; set; }
}
