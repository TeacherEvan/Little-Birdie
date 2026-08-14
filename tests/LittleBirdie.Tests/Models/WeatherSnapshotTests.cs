using LittleBirdie.Models;
using Xunit;

namespace LittleBirdie.Tests.Models;

public class WeatherSnapshotTests
{
    [Fact]
    public void Ctor_HoldsValues()
    {
        var w = new WeatherSnapshot(31, "Sunny", new[] { "32°", "30°", "29°" });
        Assert.Equal(31, w.TempC);
        Assert.Equal("Sunny", w.Condition);
        Assert.Equal(3, w.Forecast.Length);
    }
}
