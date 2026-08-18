using System.Text.Json;
using Microsoft.JSInterop;
using Moq;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Services;

public class InstallPromptServiceTests
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly AppSettingsService _appSettingsService;
    private readonly InstallPromptService _service;

    public InstallPromptServiceTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _appSettingsService = new AppSettingsService(new LocalStore(_jsMock.Object));
        _service = new InstallPromptService(_jsMock.Object, _appSettingsService);
    }

    [Fact]
    public async Task InitializeAsync_LoadsSettingsAndPlatformInfo()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":false,\"canInstall\":true,\"userAgent\":\"test\"}"));

        // Act
        await _service.InitializeAsync();

        // Assert
        Assert.Equal("DesktopChromium", _service.PlatformInfo.Platform);
        Assert.False(_service.PlatformInfo.IsStandalone);
        Assert.True(_service.PlatformInfo.CanInstall);
        Assert.True(_service.CanShowInstallPrompt);
    }

    [Fact]
    public async Task InitializeAsync_WhenStandalone_DoesNotShowPrompt()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":true,\"canInstall\":false,\"userAgent\":\"test\"}"));

        // Act
        await _service.InitializeAsync();

        // Assert
        Assert.True(_service.IsStandalone);
        Assert.False(_service.CanShowInstallPrompt);
    }

    [Fact]
    public async Task DismissInstallPromptAsync_PersistsPreference()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":false,\"canInstall\":true,\"userAgent\":\"test\"}"));
        await _service.InitializeAsync();

        // Act
        await _service.DismissInstallPromptAsync();

        // Assert
        Assert.False(_service.CanShowInstallPrompt);
        // Verify settings were persisted (AppSettingsService.SaveAsync updates the in-memory flag)
        Assert.False(_appSettingsService.Current.ShowInstallPrompt);
    }

    [Fact]
    public async Task ResetDismissalAsync_ClearsPreference()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":false,\"canInstall\":true,\"userAgent\":\"test\"}"));
        await _service.InitializeAsync();
        await _service.DismissInstallPromptAsync();
        Assert.False(_service.CanShowInstallPrompt);

        // Act
        await _service.ResetDismissalAsync();

        // Assert
        Assert.True(_service.CanShowInstallPrompt);
        Assert.True(_appSettingsService.Current.ShowInstallPrompt);
    }

    [Fact]
    public async Task OnAppInstalled_UpdatesState()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":false,\"canInstall\":true,\"userAgent\":\"test\"}"));
        await _service.InitializeAsync();

        // Act
        _service.GetType().GetMethod("OnAppInstalled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
            .Invoke(_service, null);

        // Assert
        Assert.True(_service.IsStandalone);
        Assert.False(_service.PlatformInfo.CanInstall);
    }
}