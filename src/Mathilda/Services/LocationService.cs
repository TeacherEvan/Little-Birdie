using System.Text.Json;
using Microsoft.JSInterop;
using Mathilda.Models;

namespace Mathilda.Services;

/// <summary>
/// Resolves the user's physical location (GPS or chosen city) and persists the
/// chosen coordinates to <see cref="LocalStore"/> so weather/attractions reflect
/// the user's real position instead of hardcoded Bangkok.
/// </summary>
public sealed class LocationService
{
    private readonly IJSRuntime _js;
    private readonly LocalStore _store;
    private const string STORAGE_KEY = "mathilda.location";

    public LocationService(IJSRuntime js, LocalStore store)
    {
        _js = js;
        _store = store;
    }

    /// <summary>
    /// Requests the browser's geolocation, returning the real { lat, lng }.
    /// Returns null when the user denies, the API is unavailable, or parsing fails
    /// (the caller then falls back to manual city selection).
    /// </summary>
    public async Task<(double Lat, double Lng)?> RequestGpsAsync()
    {
        try
        {
            var result = await _js.InvokeAsync<JsonElement>("mathilda.geolocation.request");
            if (result.TryGetProperty("lat", out var lat) &&
                result.TryGetProperty("lng", out var lng))
            {
                return (lat.GetDouble(), lng.GetDouble());
            }
        }
        catch
        {
            // GPS unavailable/denied — caller falls back to manual selection.
        }
        return null;
    }

    /// <summary>Persists a chosen location (GPS or city) to local storage.</summary>
    public async Task SaveAsync(double lat, double lng, string name)
    {
        await _store.SaveAsync(STORAGE_KEY, new SavedLocation(lat, lng, name));
    }

    /// <summary>Loads the last persisted location, or null if none.</summary>
    public async Task<(double Lat, double Lng, string Name)?> LoadAsync()
    {
        var saved = await _store.LoadAsync(STORAGE_KEY, (SavedLocation?)null);
        return saved is null ? null : (saved.Lat, saved.Lng, saved.Name);
    }
}

/// <summary>Persisted location snapshot.</summary>
public sealed record SavedLocation(double Lat, double Lng, string Name);
