namespace Mathilda.Models;

/// <summary>
/// Canonical application settings for Mathilda (Phase 5 advanced settings hub).
/// Properties are settable so Blazor two-way (@bind) binding works in the settings UI.
/// Only fields with a real consumer are kept; dead preference fields were removed
/// (see docs/.scratch-audit/INTEROP-CONTRACT.md).
/// </summary>
public sealed record AppSettings
{
    /// <summary>Bypass the themed startup animation on launch.</summary>
    public bool SkipStartupVideo { get; set; }

    /// <summary>Show the PWA install wizard prompt.</summary>
    public bool ShowInstallPrompt { get; set; } = true;

    /// <summary>Optional override for the Convex deployment URL (enables live data).</summary>
    public string? CustomConvexUrl { get; set; }
}
