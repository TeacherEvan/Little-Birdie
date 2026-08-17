using System.Text.Json;
using Microsoft.JSInterop;
using Mathilda.Models;

namespace Mathilda.Services;

/// <summary>
/// Basic AppSettingsService for Phase 1 - stores ShowInstallPrompt setting.
/// Will be extended in Phase 5 with full settings model.
/// </summary>
public sealed class AppSettingsService
{
    private readonly IJSRuntime _js;
    private const string STORAGE_KEY = "mathilda.settings";

    // Current settings (minimal for Phase 1)
    public bool ShowInstallPrompt { get; private set; } = true;

    public event Action? OnSettingsChanged;

    public AppSettingsService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("mathilda.storage.getItem", STORAGE_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    ShowInstallPrompt = settings.ShowInstallPrompt;
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppSettingsService] Load failed: {ex.Message}");
        }

        return new AppSettings { ShowInstallPrompt = true };
    }

    public async Task SaveAsync(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings);
            await _js.InvokeVoidAsync("mathilda.storage.setItem", STORAGE_KEY, json);
            ShowInstallPrompt = settings.ShowInstallPrompt;
            OnSettingsChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppSettingsService] Save failed: {ex.Message}");
        }
    }

    public async Task UpdateSettingAsync(string key, object value)
    {
        var current = await LoadAsync();
        var prop = typeof(AppSettings).GetProperty(key);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(current, value);
            await SaveAsync(current);
        }
    }
}
