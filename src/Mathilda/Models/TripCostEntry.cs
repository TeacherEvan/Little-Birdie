namespace Mathilda.Models;

/// <summary>A single trip cost line item.</summary>
public record TripCostEntry(double Amount, string Currency, string Category)
{
    /// <summary>Converted/normalized total (same as Amount for the MVP; kept for future FX).</summary>
    public double Total => Amount;
}
