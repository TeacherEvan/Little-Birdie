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
        _service = new AppSettingsService(new LocalStore(_jsMock.Object));
    }

    [Fact]
    public async Task LoadAsync_WhenEmpty_ReturnsDefaults()
    {
        _jsMock.Setup(x => x.InvokeAsync<string>("mathilda.storage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(string.Empty);

        var settings = await _service.LoadAsync();

        Assert.False(settings.SkipStartupVideo);
        Assert.True(settings.ShowInstallPrompt);
        Assert.Null(settings.CustomConvexUrl);
    }

    [Fact]
    public void AppSettings_RoundTripsAllFieldsViaJson()
    {
        var settings = new AppSettings
        {
            SkipStartupVideo = true,
            ShowInstallPrompt = false,
            CustomConvexUrl = "https://example.convex.cloud"
        };

        var json = JsonSerializer.Serialize(settings);
        var round = JsonSerializer.Deserialize<AppSettings>(json);

        Assert.Equal(settings, round);
    }

    [Fact]
    public async Task SetShowInstallPromptAsync_PersistsGivenValue()
    {
        var stored = new System.Collections.Generic.Dictionary<string, string>();
        _jsMock.Setup(x => x.InvokeAsync<string>("mathilda.storage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(() => stored.TryGetValue("mathilda.settings", out var v) ? v : string.Empty);
        _jsMock.Setup(x => x.InvokeAsync<object?>("mathilda.storage.setItem", It.IsAny<object[]>()))
            .Callback<string, object[]>((id, args) =>
            {
                if (id == "mathilda.storage.setItem" && args.Length >= 2)
                    stored["mathilda.settings"] = (string)args[1]!;
            });

        await _service.LoadAsync();
        await _service.SetShowInstallPromptAsync(false);

        var saved = JsonSerializer.Deserialize<AppSettings>(stored["mathilda.settings"]);
        Assert.False(saved!.ShowInstallPrompt);
        Assert.False(_service.Current.ShowInstallPrompt);
    }
}
