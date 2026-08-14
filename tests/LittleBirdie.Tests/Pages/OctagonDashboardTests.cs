using LittleBirdie.Pages;
using Bunit;
using Xunit;

namespace LittleBirdie.Tests.Pages;

public class OctagonDashboardTests : TestContext
{
    [Fact]
    public void Renders_Title()
    {
        var cut = RenderComponent<OctagonDashboard>();
        Assert.Contains("Little-Birdie", cut.Markup);
    }

    [Fact]
    public void Renders_Eight_Tiles()
    {
        var cut = RenderComponent<OctagonDashboard>();
        Assert.Equal(8, cut.FindAll("button.tile").Count);
    }
}
