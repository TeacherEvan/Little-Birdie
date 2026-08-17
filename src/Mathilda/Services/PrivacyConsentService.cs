using System.Text.Json;
using Microsoft.JSInterop;
using Mathilda.Models;

namespace Mathilda.Services;

/// <summary>
/// Persists and manages the user's privacy / cookie consent choices (PDPA / GDPR compliant).
/// Consent is stored in localStorage via the mathilda.storage interop bridge.
/// </summary>
public sealed class PrivacyConsentService
{
    private readonly IJSRuntime _js;
    private const string STORAGE_KEY = "mathilda.privacy.consent";

    /// <summary>Currently loaded consent (defaults to essential-only until loaded).</summary>
    public PrivacyConsent Consent { get; private set; } = PrivacyConsent.Default();

    /// <summary>Raised whenever consent is (re)loaded or saved.</summary>
    public event Action<PrivacyConsent>? OnConsentChanged;

    public PrivacyConsentService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>Loads persisted consent from localStorage, falling back to defaults on any failure.</summary>
    public async Task<PrivacyConsent> LoadAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("mathilda.storage.getItem", STORAGE_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                var consent = JsonSerializer.Deserialize<PrivacyConsent>(json);
                if (consent is not null)
                {
                    Consent = consent;
                    return consent;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PrivacyConsentService] Load failed: {ex.Message}");
        }

        var defaults = PrivacyConsent.Default();
        Consent = defaults;
        return defaults;
    }

    /// <summary>Persists the given consent and notifies subscribers.</summary>
    public async Task SaveAsync(PrivacyConsent consent)
    {
        try
        {
            var json = JsonSerializer.Serialize(consent);
            await _js.InvokeVoidAsync("mathilda.storage.setItem", STORAGE_KEY, json);
            Consent = consent;
            OnConsentChanged?.Invoke(consent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PrivacyConsentService] Save failed: {ex.Message}");
        }
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
