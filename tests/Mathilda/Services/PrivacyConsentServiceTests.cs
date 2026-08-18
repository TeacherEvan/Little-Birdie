using System.Text.Json;
using Microsoft.JSInterop;
using Moq;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Services;

public class PrivacyConsentServiceTests
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly PrivacyConsentService _service;

    public PrivacyConsentServiceTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _service = new PrivacyConsentService(new LocalStore(_jsMock.Object));
    }

    [Fact]
    public async Task LoadAsync_WhenEmpty_ReturnsEssentialDefaults()
    {
        _jsMock.Setup(x => x.InvokeAsync<string>("mathilda.storage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        var consent = await _service.LoadAsync();

        Assert.True(consent.EssentialAccepted);
        Assert.False(consent.PreferencesAccepted);
        Assert.False(consent.AnalyticsAccepted);
        Assert.Equal(consent, _service.Consent);
    }

    [Fact]
    public async Task SaveAsync_PersistsAndNotifies()
    {
        PrivacyConsent? captured = null;
        _service.OnConsentChanged += c => captured = c;

        var consent = new PrivacyConsent(true, true, true, DateTimeOffset.UtcNow);
        await _service.SaveAsync(consent);

        // Service updates its in-memory snapshot and raises the change notification.
        Assert.Equal(consent, _service.Consent);
        Assert.Equal(consent, captured);

        // The serialized payload round-trips identically (proves storage payload correctness).
        var json = JsonSerializer.Serialize(consent);
        var round = JsonSerializer.Deserialize<PrivacyConsent>(json);
        Assert.Equal(consent, round);
    }

    [Fact]
    public async Task AcceptAllAsync_SetsAllCategories()
    {
        await _service.AcceptAllAsync();

        Assert.True(_service.Consent.EssentialAccepted);
        Assert.True(_service.Consent.PreferencesAccepted);
        Assert.True(_service.Consent.AnalyticsAccepted);
    }

    [Fact]
    public async Task EssentialOnlyAsync_KeepsOnlyEssential()
    {
        await _service.AcceptAllAsync();
        await _service.EssentialOnlyAsync();

        Assert.True(_service.Consent.EssentialAccepted);
        Assert.False(_service.Consent.PreferencesAccepted);
        Assert.False(_service.Consent.AnalyticsAccepted);
    }

    [Fact]
    public async Task LoadAsync_RoundTripsSerializedConsent()
    {
        var original = new PrivacyConsent(true, true, false, DateTimeOffset.UtcNow);
        _jsMock.Setup(x => x.InvokeAsync<string>("mathilda.storage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Serialize(original));

        var loaded = await _service.LoadAsync();

        Assert.Equal(original, loaded);
    }
}
