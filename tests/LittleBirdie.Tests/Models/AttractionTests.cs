using LittleBirdie.Models;
using Xunit;

namespace LittleBirdie.Tests.Models;

public class AttractionTests
{
    [Fact]
    public void Ctor_SetsAllFields()
    {
        var a = new Attraction("Cafe", 1.2, "cafe", true);
        Assert.Equal("Cafe", a.Name);
        Assert.Equal(1.2, a.DistanceKm);
        Assert.Equal("cafe", a.Type);
        Assert.True(a.OpenNow);
    }
}
