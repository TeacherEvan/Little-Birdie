namespace Mathilda.Models;

/// <summary>
/// Platform information for PWA install flow adaptation.
/// </summary>
public sealed record PlatformInfo(
    string Platform,           // "iOS", "Android", "DesktopChromium", "Other"
    bool IsStandalone,         // Running as installed PWA
    bool CanInstall,           // Install prompt is available (beforeinstallprompt captured)
    string? UserAgent = null   // Raw user agent for debugging
);