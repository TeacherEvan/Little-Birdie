using LittleBirdie.Models;
using LittleBirdie.Services;
using Xunit;

namespace LittleBirdie.Tests.Services;

public class WeatherServiceTests
{
    [Fact]
    public async Task Fetch_NoConvex_ReturnsMockSunny()
    {
        var svc = new WeatherService();
        var w = await svc.Fetch(13.7, 100.5);
        Assert.Equal("Sunny", w.Condition);
        Assert.Equal(31, w.TempC);
    }
}
