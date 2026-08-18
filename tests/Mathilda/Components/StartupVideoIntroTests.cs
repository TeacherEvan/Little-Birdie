using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mathilda.Components;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Components;

/// <summary>
/// Coverage for OBJ-06: the startup intro is an honest SVG splash (no dead &lt;video&gt;
/// element). The skip button must still complete the intro.
/// </summary>
public class StartupVideoIntroTests : TestContext
{
    private readonly Mock<IJSRuntime> _jsMock;

    public StartupVideoIntroTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        Services.AddSingleton(_jsMock.Object);
        Services.AddScoped<LocalStore>();
        Services.AddScoped<AppSettingsService>();
    }

    [Fact]
    public void Render_ShowsSvgSplash_NotVideo()
    {
        var cut = RenderComponent<StartupVideoIntro>();

        // The async init (settings load) must complete before the splash renders.
        cut.WaitForElements("img.startup-fallback");

        // Honest splash: SVG fallback image, no dead <video> element.
        Assert.Contains("startup-intro.svg", cut.Markup);
        Assert.DoesNotContain("<video", cut.Markup);
        Assert.DoesNotContain("startup-video", cut.Markup);
    }

    [Fact]
    public void SkipButton_CompletesIntro()
    {
        var completed = false;
        var cut = RenderComponent<StartupVideoIntro>(
            parameters => parameters.Add(p => p.OnComplete, EventCallback.Factory.Create(this, () => completed = true)));

        cut.Find("button.skip-btn").Click();

        Assert.True(completed);
    }
}
