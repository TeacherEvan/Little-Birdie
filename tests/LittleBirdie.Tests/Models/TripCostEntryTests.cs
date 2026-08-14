using LittleBirdie.Models;
using Xunit;

namespace LittleBirdie.Tests.Models;

public class TripCostEntryTests
{
    [Fact]
    public void Total_EqualsAmount()
    {
        var e = new TripCostEntry(100, "THB", "food");
        Assert.Equal(100, e.Total);
    }
}
