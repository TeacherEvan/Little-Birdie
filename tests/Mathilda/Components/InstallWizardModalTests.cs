using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mathilda.Components;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Components;

public class InstallWizardModalTests : TestContext
{
    private readonly Mock<IJSRuntime> _jsMock;
    private readonly InstallPromptService _installPromptService;
    private readonly AppSettingsService _appSettingsService;

    public InstallWizardModalTests()
    {
        _jsMock = new Mock<IJSRuntime>();
        _appSettingsService = new AppSettingsService(new LocalStore(_jsMock.Object));
        _installPromptService = new InstallPromptService(_jsMock.Object, _appSettingsService);
        
        Services.AddSingleton(_installPromptService);
        Services.AddSingleton(_appSettingsService);
        Services.AddSingleton(_jsMock.Object);
    }

    [Fact]
    public async Task Render_WhenInstallPromptAvailable_ShowsModal()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":false,\"canInstall\":true,\"userAgent\":\"test\"}"));
        
        await _installPromptService.InitializeAsync();

        // Act
        var cut = RenderComponent<InstallWizardModal>();

        // Assert
        // The component gates ShowModal behind await Task.Delay(100) in CheckShowModalAsync,
        // so the .modal-overlay is not present synchronously after RenderComponent.
        // WaitForElements polls until the async state change renders it (or times out).
        cut.WaitForElements(".modal-overlay");
        Assert.Contains("Install Mathilda on your Device", cut.Markup);
        Assert.Contains("1-Click Install Mathilda", cut.Markup);
    }

    [Fact]
    public async Task Render_WhenIOSPlatform_ShowsIOSGuide()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"iOS\",\"isStandalone\":false,\"canInstall\":false,\"userAgent\":\"test\"}"));
        
        await _installPromptService.InitializeAsync();

        // Act
        var cut = RenderComponent<InstallWizardModal>();

        // Assert
        // Same async gating as above: WaitForElements awaits the Task.Delay(100) continuation.
        cut.WaitForElements(".modal-overlay");
        Assert.Contains("iOS Safari - Add to Home Screen", cut.Markup);
        Assert.Contains("Share button", cut.Markup);
        Assert.Contains("Add to Home Screen", cut.Markup);
    }

    [Fact]
    public async Task Render_WhenStandalone_DoesNotShowModal()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":true,\"canInstall\":false,\"userAgent\":\"test\"}"));
        
        await _installPromptService.InitializeAsync();

        // Act
        var cut = RenderComponent<InstallWizardModal>();

        // Assert - modal should not be rendered when standalone
        Assert.DoesNotContain("Install Mathilda on your Device", cut.Markup);
    }

    [Fact]
    public async Task Render_WhenDismissed_DoesNotShowModal()
    {
        // Arrange
        _jsMock.Setup(x => x.InvokeAsync<JsonElement>("mathilda.pwa.getPlatformInfo", It.IsAny<object[]>()))
            .ReturnsAsync(JsonSerializer.Deserialize<JsonElement>("{\"platform\":\"DesktopChromium\",\"isStandalone\":false,\"canInstall\":true,\"userAgent\":\"test\"}"));
        
        await _installPromptService.InitializeAsync();
        await _installPromptService.DismissInstallPromptAsync();

        // Act
        var cut = RenderComponent<InstallWizardModal>();

        // Assert
        Assert.DoesNotContain("Install Mathilda on your Device", cut.Markup);
    }
}