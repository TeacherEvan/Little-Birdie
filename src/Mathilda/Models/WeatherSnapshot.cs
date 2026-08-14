namespace Mathilda.Models;

/// <summary>Port of Quicky's WeatherSnapshot (tempC, condition, forecast).</summary>
public record WeatherSnapshot(double TempC, string Condition, string[] Forecast);
