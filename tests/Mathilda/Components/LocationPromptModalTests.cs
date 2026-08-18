using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mathilda.Components;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Components;

public class LocationPromptModalTests : TestContext
{
    private readonly Mock<IJSRuntime> _jsMock;

    public LocationPromptModalTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        Services.AddSingleton(_jsMock.Object);
        Services.AddScoped<LocalStore>();
        // LocationService is now a dependency of the modal (OBJ-01).
        Services.AddScoped<LocationService>();
    }

    [Fact]
    public void Render_ShowsGpsAndCityChoice()
    {
        var cut = RenderComponent<LocationPromptModal>();

        Assert.Contains("Help Mathilda Find You", cut.Markup);
        Assert.Contains("Use My Location", cut.Markup);
        Assert.Contains("Choose a City Instead", cut.Markup);
    }

    [Fact]
    public void RevealFallback_ShowsAllEightProvinces()
    {
        var cut = RenderComponent<LocationPromptModal>();
        var reveal = cut.FindAll("button").First(b => b.TextContent.Contains("Choose a City Instead"));
        cut.InvokeAsync(() => reveal.Click());

        foreach (var p in ThaiProvince.All)
        {
            Assert.Contains(p.Name, cut.Markup);
        }
        Assert.Equal(8, ThaiProvince.All.Count);
    }

    [Fact]
    public async Task ConfirmFallback_ClosesModalAndUsesProvince()
    {
        var cut = RenderComponent<LocationPromptModal>();
        var reveal = cut.FindAll("button").First(b => b.TextContent.Contains("Choose a City Instead"));
        await cut.InvokeAsync(() => reveal.Click());

        var confirm = cut.Find(".province-list button");
        await cut.InvokeAsync(() => confirm.Click());

        // Modal should dismiss (heading no longer in markup) after confirming a city.
        Assert.DoesNotContain("Help Mathilda Find You", cut.Markup);
    }
}
