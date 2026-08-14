using Mathilda.Models;
using Xunit;

namespace Mathilda.Tests.Models;

public class TripCostEntryTests
{
    [Fact]
    public void Total_EqualsAmount()
    {
        var e = new TripCostEntry(100, "THB", "food");
        Assert.Equal(100, e.Total);
    }
}
