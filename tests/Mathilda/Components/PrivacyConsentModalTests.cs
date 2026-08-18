using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mathilda.Components;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Components;

public class PrivacyConsentModalTests : TestContext
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly PrivacyConsentService _consentService;

    public PrivacyConsentModalTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _consentService = new PrivacyConsentService(new LocalStore(_jsMock.Object));
        Services.AddSingleton(_consentService);
        Services.AddSingleton(_jsMock.Object);
    }

    [Fact]
    public void Render_ShowsAcceptAllAndEssentialOnly()
    {
        var cut = RenderComponent<PrivacyConsentModal>();

        Assert.Contains("Your Privacy, Your Choice", cut.Markup);
        Assert.Contains("Accept All", cut.Markup);
        Assert.Contains("Essential Only", cut.Markup);
    }

    [Fact]
    public async Task AcceptAll_ClickingPersistsAllCategories()
    {
        var cut = RenderComponent<PrivacyConsentModal>();
        var button = cut.FindAll("button").First(b => b.TextContent.Contains("Accept All"));

        await cut.InvokeAsync(() => button.Click());

        Assert.True(_consentService.Consent.EssentialAccepted);
        Assert.True(_consentService.Consent.PreferencesAccepted);
        Assert.True(_consentService.Consent.AnalyticsAccepted);
    }

    [Fact]
    public async Task EssentialOnly_ClickingKeepsOnlyEssential()
    {
        var cut = RenderComponent<PrivacyConsentModal>();
        var button = cut.FindAll("button").First(b => b.TextContent.Contains("Essential Only"));

        await cut.InvokeAsync(() => button.Click());

        Assert.True(_consentService.Consent.EssentialAccepted);
        Assert.False(_consentService.Consent.PreferencesAccepted);
        Assert.False(_consentService.Consent.AnalyticsAccepted);
    }
}
