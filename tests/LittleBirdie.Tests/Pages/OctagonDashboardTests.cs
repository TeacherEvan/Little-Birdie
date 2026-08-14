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
        cut.Markup.Contains("Little-Birdie");
    }

    [Fact]
    public void Renders_Eight_Tiles()
    {
        var cut = RenderComponent<OctagonDashboard>();
        Assert.Equal(8, cut.FindAll("button.tile").Count);
    }
}
