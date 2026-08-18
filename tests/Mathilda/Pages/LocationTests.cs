using Bunit;
using Mathilda.Pages;
using Mathilda.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System.Text.Json;
using Xunit;

namespace Mathilda.Tests.Pages;

public class LocationTests : TestContext
{
    private static string LatLngJson(double lat, double lng) =>
        JsonSerializer.Serialize(new { lat, lng });

    [Fact]
    public void LocationPage_Denied_ShowsUnavailable()
    {
        Services.AddSingleton<IJSRuntime>(new DeniedJSRuntime());
        Services.AddScoped<LocalStore>();
        Services.AddScoped<LocationService>();
        var cut = RenderComponent<LocationPage>();

        cut.InvokeAsync(() => cut.Find("button").Click());

        Assert.Contains("Location unavailable", cut.Markup);
    }

    [Fact]
    public void LocationPage_Acquired_ShowsCoordinates()
    {
        Services.AddSingleton<IJSRuntime>(new AcquiredJSRuntime(LatLngJson(13.7563, 100.5018)));
        Services.AddScoped<LocalStore>();
        Services.AddScoped<LocationService>();
        var cut = RenderComponent<LocationPage>();

        cut.InvokeAsync(() => cut.Find("button").Click());

        Assert.Contains("13.7563", cut.Markup);
        Assert.Contains("100.5018", cut.Markup);
        Assert.Contains("Location acquired", cut.Markup);
    }

    private sealed class DeniedJSRuntime : IJSRuntime
    {
        private readonly JsonElement _value = JsonSerializer.Deserialize<JsonElement>("{\"error\":\"denied:1\"}");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => ValueTask.FromResult((TValue)(object)_value);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => ValueTask.FromResult((TValue)(object)_value);
    }

    private sealed class AcquiredJSRuntime : IJSRuntime
    {
        private readonly JsonElement _value;
        public AcquiredJSRuntime(string json) => _value = JsonSerializer.Deserialize<JsonElement>(json);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => ValueTask.FromResult((TValue)(object)_value);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => ValueTask.FromResult((TValue)(object)_value);
    }
}
