namespace Mathilda.Models;

/// <summary>
/// User privacy and cookie consent choices (PDPA / GDPR compliant).
/// </summary>
public sealed record PrivacyConsent(
    /// <summary>Strictly necessary storage (offline cache, settings). Always required.</summary>
    bool EssentialAccepted,
    /// <summary>Travel preferences (currency, language, recent places).</summary>
    bool PreferencesAccepted,
    /// <summary>Anonymous diagnostics (network latency, crash reports).</summary>
    bool AnalyticsAccepted,
    /// <summary>UTC timestamp of when the consent was recorded.</summary>
    DateTimeOffset ConsentTimestamp
)
{
    /// <summary>
    /// Creates a default consent: only essential storage accepted, timestamped at the current UTC time.
    /// </summary>
    public static PrivacyConsent Default() => new(
        EssentialAccepted: true,
        PreferencesAccepted: false,
        AnalyticsAccepted: false,
        ConsentTimestamp: DateTimeOffset.UtcNow
    );
}
