namespace Mathilda.Models;

/// <summary>Port of Quicky's Attraction (name, distanceKm, type, openNow).</summary>
public record Attraction(string Name, double DistanceKm, string Type, bool OpenNow);
