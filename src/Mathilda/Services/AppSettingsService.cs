using Microsoft.JSInterop;
using Mathilda.Models;

namespace Mathilda.Services;

/// <summary>
/// Persists and exposes the canonical application settings (Phase 5 advanced settings hub).
/// Settings live in localStorage via the <see cref="LocalStore"/> JSON bridge.
/// </summary>
public sealed class AppSettingsService
{
    private readonly LocalStore _store;
    private const string STORAGE_KEY = "mathilda.settings";

    /// <summary>Cached in-memory snapshot of the last loaded/saved settings.</summary>
    public AppSettings Current { get; private set; } = new();

    /// <summary>Raised whenever settings are loaded or saved, carrying the new snapshot.</summary>
    public event Action<AppSettings>? OnSettingsChanged;

    public AppSettingsService(LocalStore store)
    {
        _store = store;
    }

    /// <summary>Loads persisted settings, falling back to defaults on any failure.</summary>
    public async Task<AppSettings> LoadAsync()
    {
        var settings = await _store.LoadAsync(STORAGE_KEY, new AppSettings());
        Current = settings;
        OnSettingsChanged?.Invoke(settings);
        return settings;
    }

    /// <summary>Persists the given settings and notifies subscribers.</summary>
    public async Task SaveAsync(AppSettings settings)
    {
        await _store.SaveAsync(STORAGE_KEY, settings);
        Current = settings;
        OnSettingsChanged?.Invoke(settings);
    }

    /// <summary>Persists the "show install prompt" preference without mutating a full model.</summary>
    public async Task SetShowInstallPromptAsync(bool show)
    {
        var current = await LoadAsync();
        current = current with { ShowInstallPrompt = show };
        await SaveAsync(current);
    }
}
