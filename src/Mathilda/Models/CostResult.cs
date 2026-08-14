namespace Mathilda.Models;

/// <summary>Aggregated cost result for the dashboard card.</summary>
public record CostResult(double Total, IReadOnlyList<TripCostEntry> Entries);
