using Microsoft.JSInterop;
using Mathilda.Models;

namespace Mathilda.Services;

/// <summary>
/// Persists and manages the user's privacy / cookie consent choices (PDPA / GDPR compliant).
/// Consent is stored in localStorage via the <see cref="LocalStore"/> JSON bridge.
/// </summary>
public sealed class PrivacyConsentService
{
    private readonly LocalStore _store;
    private const string STORAGE_KEY = "mathilda.privacy.consent";

    /// <summary>Currently loaded consent (defaults to essential-only until loaded).</summary>
    public PrivacyConsent Consent { get; private set; } = PrivacyConsent.Default();

    /// <summary>Raised whenever consent is (re)loaded or saved.</summary>
    public event Action<PrivacyConsent>? OnConsentChanged;

    public PrivacyConsentService(LocalStore store)
    {
        _store = store;
    }

    /// <summary>Loads persisted consent from localStorage, falling back to defaults on any failure.</summary>
    public async Task<PrivacyConsent> LoadAsync()
    {
        var consent = await _store.LoadAsync(STORAGE_KEY, PrivacyConsent.Default());
        Consent = consent;
        OnConsentChanged?.Invoke(consent);
        return consent;
    }

    /// <summary>Persists the given consent and notifies subscribers.</summary>
    public async Task SaveAsync(PrivacyConsent consent)
    {
        await _store.SaveAsync(STORAGE_KEY, consent);
        Consent = consent;
        OnConsentChanged?.Invoke(consent);
    }

    /// <summary>Accepts all categories and persists.</summary>
    public Task AcceptAllAsync() =>
        SaveAsync(new PrivacyConsent(
            EssentialAccepted: true,
            PreferencesAccepted: true,
            AnalyticsAccepted: true,
            ConsentTimestamp: DateTimeOffset.UtcNow));

    /// <summary>Accepts only strictly-necessary storage and persists.</summary>
    public Task EssentialOnlyAsync() => SaveAsync(PrivacyConsent.Default());
}
