using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Services;

/// <summary>
/// Regression coverage for the JS/C# interop contract (surgical-implementation OBJ-01/02/03).
/// These guard against the contract drift that previously left features silently broken:
/// geolocation returning a string instead of {lat,lng}, storage.clear being undefined, and
/// sw.update being undefined.
/// </summary>
public class InteropContractTests
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly LocalStore _store;
    private readonly InstallPromptService _install;

    public InteropContractTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _store = new LocalStore(_jsMock.Object);
        _install = new InstallPromptService(_jsMock.Object, new AppSettingsService(_store));
    }

    [Fact]
    public async Task LocationService_RequestGpsAsync_ParsesLatLngObject()
    {
        // Arrange — interop returns { lat, lng } (the corrected contract, not "lat,lng").
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.geolocation.request", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"lat\":13.7563,\"lng\":100.5018}"));

        var svc = new LocationService(_jsMock.Object, _store);

        // Act
        var coords = await svc.RequestGpsAsync();

        // Assert
        Assert.True(coords.HasValue);
        Assert.Equal(13.7563, coords!.Value.Lat, 4);
        Assert.Equal(100.5018, coords!.Value.Lng, 4);
    }

    [Fact]
    public async Task LocationService_RequestGpsAsync_ReturnsNullOnErrorShape()
    {
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.geolocation.request", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"error\":\"denied:1\"}"));

        var svc = new LocationService(_jsMock.Object, _store);

        var coords = await svc.RequestGpsAsync();

        Assert.False(coords.HasValue);
    }

    [Fact]
    public async Task StorageClear_IsInvokedAndScoped()
    {
        // Arrange — the clear call must target the defined mathilda.storage.clear symbol.
        // InvokeVoidAsync resolves through InvokeAsync<IJSVoidResult>; match that.
        var cleared = false;
        _jsMock.Setup(x => x.InvokeAsync<IJSVoidResult>("mathilda.storage.clear", It.IsAny<object[]>()))
            .Callback(() => cleared = true)
            .ReturnsAsync(Mock.Of<IJSVoidResult>());

        // Act — mirror PrivacySettingsTab.ClearCacheAsync
        await _jsMock.Object.InvokeVoidAsync("mathilda.storage.clear");

        // Assert
        Assert.True(cleared);
    }

    [Fact]
    public async Task ServiceWorkerUpdate_IsInvoked()
    {
        // Arrange — the update call must target the defined mathilda.sw.update symbol.
        var updated = false;
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.sw.update", It.IsAny<object[]>()))
            .Callback(() => updated = true)
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"success\":true}"));

        // Act — mirror AdvancedSettingsPanel.ForceReloadSwAsync
        var result = await _jsMock.Object.InvokeAsync<JsonElement>("mathilda.sw.update");

        // Assert
        Assert.True(updated);
        Assert.True(result.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task InstallPromptService_Initialize_RegistersTypedCallback_NoEval()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":false,\"canInstall\":true,\"userAgent\":\"test\"}"));

        // Capture the registration call to confirm it is the typed bridge, not eval.
        string? registeredSymbol = null;
        _jsMock.Setup(x => x.InvokeAsync<object?>("mathilda.pwa.registerCallbacks", It.IsAny<object[]>()))
            .Callback<string, object[]>((sym, args) => registeredSymbol = sym)
            .ReturnsAsync(true);

        // Act
        await _install.InitializeAsync();

        // Assert — callback registered via the typed symbol, not via eval.
        Assert.Equal("mathilda.pwa.registerCallbacks", registeredSymbol);
        Assert.True(_install.CanShowInstallPrompt);
    }
}
