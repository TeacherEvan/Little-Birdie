using System.Text.Json;
using Microsoft.JSInterop;
using Moq;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Services;

public class AppSettingsServiceTests
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly AppSettingsService _service;

    public AppSettingsServiceTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _service = new AppSettingsService(_jsMock.Object);
    }

    [Fact]
    public async Task LoadAsync_WhenEmpty_ReturnsDefaults()
    {
        _jsMock.Setup(x => x.InvokeAsync<string>("mathilda.storage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        var settings = await _service.LoadAsync();

        Assert.Equal("en", settings.Language);
        Assert.Equal("system", settings.Theme);
        Assert.Equal("THB", settings.Currency);
        Assert.Equal("metric", settings.UnitSystem);
        Assert.True(settings.ShowInstallPrompt);
        Assert.False(settings.SkipStartupVideo);
    }

    [Fact]
    public void AppSettings_RoundTripsAllFieldsViaJson()
    {
        var settings = new AppSettings
        {
            Language = "th",
            Theme = "dark",
            Currency = "USD",
            UnitSystem = "imperial",
            SkipStartupVideo = true,
            ShowInstallPrompt = false,
            CustomConvexUrl = "https://example.convex.cloud",
            HighAccuracyGps = true,
            GpsTimeoutSeconds = 30,
            MockLocationEnabled = true,
            MockCoordinates = "13.7563,100.5018",
            EnableDebugTelemetry = true
        };

        var json = JsonSerializer.Serialize(settings);
        var round = JsonSerializer.Deserialize<AppSettings>(json);

        Assert.Equal(settings, round);
    }

    [Fact]
    public async Task UpdateSettingAsync_MutatesAndPersists()
    {
        var stored = new System.Collections.Generic.Dictionary<string, string>();
        _jsMock.Setup(x => x.InvokeAsync<string>("mathilda.storage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(() => stored.TryGetValue("settings", out var v) ? v : string.Empty);
        _jsMock.Setup(x => x.InvokeAsync<object?>("mathilda.storage.setItem", It.IsAny<object[]>()))
            .Callback<string, object[]>((id, args) =>
            {
                if (id == "mathilda.storage.setItem" && args.Length >= 2)
                    stored["settings"] = (string)args[1]!;
            })
            .ReturnsAsync((object?)null);

        await _service.LoadAsync();
        await _service.UpdateSettingAsync("Theme", "dark");

        var saved = JsonSerializer.Deserialize<AppSettings>(stored["settings"]);
        Assert.Equal("dark", saved!.Theme);
    }
}
