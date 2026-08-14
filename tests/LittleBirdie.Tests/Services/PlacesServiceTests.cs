using LittleBirdie.Models;
using LittleBirdie.Services;
using Xunit;

namespace LittleBirdie.Tests.Services;

public class PlacesServiceTests
{
    [Fact]
    public async Task FetchNearby_NoConvex_ReturnsMockThree()
    {
        var svc = new PlacesService();
        var list = await svc.FetchNearby(10);
        Assert.Equal(3, list.Count);
    }
}
