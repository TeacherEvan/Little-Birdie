namespace Mathilda.Models;

/// <summary>
/// A major Thai travel hub with its geographic coordinates.
/// </summary>
public sealed record ThaiProvince(
    /// <summary>Display name of the province / city.</summary>
    string Name,
    /// <summary>Latitude in decimal degrees.</summary>
    double Latitude,
    /// <summary>Longitude in decimal degrees.</summary>
    double Longitude
)
{
    /// <summary>
    /// Pre-populated coordinates for the 8 major Thai travel hubs, used as a manual
    /// location fallback when GPS is denied or unavailable.
    /// </summary>
    public static IReadOnlyList<ThaiProvince> All { get; } = new List<ThaiProvince>
    {
        new("Bangkok", 13.7563, 100.5018),
        new("Chiang Mai", 18.7883, 98.9853),
        new("Phuket", 7.8804, 98.3923),
        new("Pattaya", 12.9276, 100.8771),
        new("Koh Samui", 9.5120, 100.0136),
        new("Krabi", 8.0863, 98.9063),
        new("Hua Hin", 12.5684, 99.9577),
        new("Ayutthaya", 14.3532, 100.5684),
    };
}
