using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Services;

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
