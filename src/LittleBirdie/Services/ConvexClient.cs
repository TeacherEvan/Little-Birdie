using System.Net.Http.Json;
using System.Text.Json;

namespace LittleBirdie.Services;

/// <summary>
/// Thin typed client over the Convex HTTP API. Convex has no C# SDK, so we
/// POST to /api/query and /api/mutation with the documented envelope
/// { path, args, format:"json" } and read back { status, value }.
/// </summary>
public sealed class ConvexClient
{
    private readonly HttpClient _http;
    private readonly string _deployUrl;

    public ConvexClient(HttpClient http, string deployUrl)
    {
        _http = http;
        _deployUrl = deployUrl.TrimEnd('/');
    }

    public async Task<T?> QueryAsync<T>(string path, object? args = null)
    {
        var body = new ConvexRequest(path, args ?? new { }, "json");
        using var resp = await _http.PostAsJsonAsync($"{_deployUrl}/api/query", body);
        resp.EnsureSuccessStatusCode();
        var env = await resp.Content.ReadFromJsonAsync<ConvexEnvelope<T>>();
        return env is { Status: "success" } ? env.Value : default;
    }

    public async Task<T?> MutationAsync<T>(string path, object? args = null)
    {
        var body = new ConvexRequest(path, args ?? new { }, "json");
        using var resp = await _http.PostAsJsonAsync($"{_deployUrl}/api/mutation", body);
        resp.EnsureSuccessStatusCode();
        var env = await resp.Content.ReadFromJsonAsync<ConvexEnvelope<T>>();
        return env is { Status: "success" } ? env.Value : default;
    }

    private sealed record ConvexRequest(string Path, object Args, string Format);
}

public sealed record ConvexEnvelope<T>(string Status, T? Value);
