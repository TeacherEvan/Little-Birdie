using Mathilda.Pages;
using Mathilda.Services;
using Bunit;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mathilda.Tests.Pages;

public class OctagonDashboardTests : TestContext
{
    public OctagonDashboardTests()
    {
        // Register required services for OctagonDashboard
        Services.AddScoped<LocalStore>();
        Services.AddScoped<LocationService>();
        Services.AddScoped<InstallPromptService>();
        Services.AddScoped<IJSRuntime, TestJSRuntime>();
    }

    [Fact]
    public void Renders_Title()
    {
        var cut = RenderComponent<OctagonDashboard>();
        Assert.Contains("Mathilda", cut.Markup);
    }

    [Fact]
    public void Renders_Eight_Tiles()
    {
        var cut = RenderComponent<OctagonDashboard>();
        Assert.Equal(8, cut.FindAll("button.tile").Count);
    }

    // Minimal test JSRuntime stub
    private class TestJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    }
}
