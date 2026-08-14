using Bunit;
using Mathilda.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Xunit;

namespace Mathilda.Tests.Pages;

public class LocationTests : TestContext
{
    [Fact]
    public void LocationPage_Denied_ShowsUnavailable()
    {
        Services.AddSingleton<IJSRuntime>(new DeniedJSRuntime());
        var cut = RenderComponent<LocationPage>();

        cut.Find("button").Click();

        Assert.Contains("Location unavailable", cut.Markup);
    }

    [Fact]
    public void LocationPage_Acquired_ShowsCoordinates()
    {
        Services.AddSingleton<IJSRuntime>(new AcquiredJSRuntime("13.7563,100.5018"));
        var cut = RenderComponent<LocationPage>();

        cut.Find("button").Click();

        Assert.Contains("13.7563", cut.Markup);
        Assert.Contains("100.5018", cut.Markup);
        Assert.Contains("Location acquired", cut.Markup);
    }

    private sealed class DeniedJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => ValueTask.FromResult((TValue)(object)"denied:1");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => ValueTask.FromResult((TValue)(object)"denied:1");
    }

    private sealed class AcquiredJSRuntime : IJSRuntime
    {
        private readonly string _value;
        public AcquiredJSRuntime(string value) => _value = value;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => ValueTask.FromResult((TValue)(object)_value);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => ValueTask.FromResult((TValue)(object)_value);
    }
}
