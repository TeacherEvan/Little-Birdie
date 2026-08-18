using System.Text.Json;
using Microsoft.JSInterop;

namespace Mathilda.Services;

/// <summary>
/// Thin wrapper over the <c>mathilda.storage</c> JS interop bridge that
/// serializes/deserializes strongly-typed values to localStorage as JSON.
/// Used by the settings and privacy-consent services so each one does not
/// re-implement the same try/catch + fallback pattern.
/// </summary>
public sealed class LocalStore
{
    private readonly IJSRuntime _js;

    public LocalStore(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>Loads and deserializes the value for <paramref name="key"/>, or returns <paramref name="fallback"/> on empty/absent/corrupt data.</summary>
    public async Task<T> LoadAsync<T>(string key, T fallback)
    {
        try
        {
            var json = await _js.InvokeAsync<string>("mathilda.storage.getItem", key);
            if (!string.IsNullOrEmpty(json))
            {
                var value = JsonSerializer.Deserialize<T>(json);
                if (value is not null)
                {
                    return value;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalStore] Load failed for '{key}': {ex.Message}");
        }

        return fallback;
    }

    /// <summary>Serializes and persists <paramref name="value"/> under <paramref name="key"/>.</summary>
    public async Task SaveAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _js.InvokeVoidAsync("mathilda.storage.setItem", key, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalStore] Save failed for '{key}': {ex.Message}");
        }
    }
}
