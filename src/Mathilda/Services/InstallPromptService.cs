using System.Text.Json;
using Microsoft.JSInterop;
using Mathilda.Models;

namespace Mathilda.Services;

/// <summary>
/// Manages PWA installation lifecycle, prompt triggering, dismissal tracking,
/// and platform detection via JSInterop.
/// </summary>
public sealed class InstallPromptService
{
    private readonly IJSRuntime _js;
    private readonly AppSettingsService? _settings;

    // State
    private PlatformInfo _platformInfo = new("Unknown", false, false);
    private bool _installPromptDismissed = false;
    private bool _initialized = false;

    // Events
    public event Action<PlatformInfo>? OnPlatformInfoChanged;
    public event Action? OnInstallPromptAvailable;
    public event Action<bool>? OnInstallCompleted; // true = accepted, false = dismissed

    public PlatformInfo PlatformInfo => _platformInfo;
    public bool CanShowInstallPrompt => _platformInfo.CanInstall && !_platformInfo.IsStandalone && !_installPromptDismissed && _initialized;
    public bool IsStandalone => _platformInfo.IsStandalone;

    public InstallPromptService(IJSRuntime js, AppSettingsService? settings = null)
    {
        _js = js;
        _settings = settings;
    }

    /// <summary>
    /// Initializes the service by loading dismissal state and querying platform info from JS.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;

        // Load dismissal state from settings
        if (_settings != null)
        {
            var settings = await _settings.LoadAsync();
            _installPromptDismissed = settings.ShowInstallPrompt == false; // Reusing ShowInstallPrompt as "don't show again"
        }

        // Get platform info from JS
        try
        {
            var jsInfo = await _js.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo");
            _platformInfo = new PlatformInfo(
                jsInfo.GetProperty("platform").GetString() ?? "Unknown",
                jsInfo.GetProperty("isStandalone").GetBoolean(),
                jsInfo.GetProperty("canInstall").GetBoolean(),
                jsInfo.GetProperty("userAgent").GetString()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InstallPromptService] Failed to get platform info: {ex.Message}");
            // Fallback: basic detection
            _platformInfo = new PlatformInfo("Unknown", false, false);
        }

        // Set up JS callbacks
        try
        {
            var dotNetRef = DotNetObjectReference.Create(this);
            await _js.InvokeVoidAsync("eval", $@"
                window.mathilda.onInstallPromptReady = function(info) {{
                    {dotNetRef}.invokeMethodAsync('OnInstallPromptReady', info);
                }};
                window.mathilda.onAppInstalled = function() {{
                    {dotNetRef}.invokeMethodAsync('OnAppInstalled');
                }};
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InstallPromptService] Failed to set up JS callbacks: {ex.Message}");
        }

        _initialized = true;
        OnPlatformInfoChanged?.Invoke(_platformInfo);

        if (_platformInfo.CanInstall && !_platformInfo.IsStandalone && !_installPromptDismissed)
        {
            OnInstallPromptAvailable?.Invoke();
        }
    }

    [JSInvokable]
    public void OnInstallPromptReady(PlatformInfo info)
    {
        _platformInfo = info;
        OnPlatformInfoChanged?.Invoke(_platformInfo);
        if (!_platformInfo.IsStandalone && !_installPromptDismissed)
        {
            OnInstallPromptAvailable?.Invoke();
        }
    }

    [JSInvokable]
    public void OnAppInstalled()
    {
        _platformInfo = _platformInfo with { IsStandalone = true, CanInstall = false };
        OnPlatformInfoChanged?.Invoke(_platformInfo);
        OnInstallCompleted?.Invoke(true);
    }

    /// <summary>
    /// Triggers the native browser install prompt.
    /// </summary>
    public async Task<bool> PromptInstallAsync()
    {
        if (!_platformInfo.CanInstall)
        {
            return false;
        }

        try
        {
            var result = await _js.InvokeAsync<JsonElement>("mathilda.pwa.promptInstall");
            var success = result.GetProperty("success").GetBoolean();
            OnInstallCompleted?.Invoke(success);
            
            if (!success)
            {
                var reason = result.GetProperty("reason").GetString();
                if (reason == "dismissed")
                {
                    await DismissInstallPromptAsync();
                }
            }
            else
            {
                _platformInfo = _platformInfo with { IsStandalone = true, CanInstall = false };
                OnPlatformInfoChanged?.Invoke(_platformInfo);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[InstallPromptService] PromptInstallAsync error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// User dismissed the install prompt - persist preference.
    /// </summary>
    public async Task DismissInstallPromptAsync()
    {
        _installPromptDismissed = true;
        if (_settings != null)
        {
            await _settings.UpdateSettingAsync("ShowInstallPrompt", false);
        }
        OnInstallCompleted?.Invoke(false);
    }

    /// <summary>
    /// Resets the dismissal state (for testing or "remind me later").
    /// </summary>
    public async Task ResetDismissalAsync()
    {
        _installPromptDismissed = false;
        if (_settings != null)
        {
            await _settings.UpdateSettingAsync("ShowInstallPrompt", true);
        }
    }

    /// <summary>
    /// Gets current settings for the install prompt.
    /// </summary>
    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_settings != null)
        {
            return await _settings.LoadAsync();
        }
        return new AppSettings { ShowInstallPrompt = true };
    }
}