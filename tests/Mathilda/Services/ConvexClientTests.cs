using System.Net;
using Mathilda.Models;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Services;

public class ConvexClientTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
        public int RequestCount;
        public string? LastUri;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            RequestCount++;
            LastUri = request.RequestUri?.ToString();
            return Task.FromResult(_respond(request));
        }
    }

    private static HttpClient OkJson(string json) =>
        new()
        {
            BaseAddress = new Uri("https://example.convex.cloud"),
            // handler swapped by caller
        };

    [Fact]
    public async Task QueryAsync_DeserializesEnvelopeToList()
    {
        const string json = "{\"status\":\"success\",\"value\":[{\"name\":\"Mock Cafe\",\"distanceKm\":1.2,\"type\":\"cafe\",\"openNow\":true}]}";
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.convex.cloud") };
        var client = new ConvexClient(http, "https://example.convex.cloud/");

        var result = await client.QueryAsync<List<Attraction>>("places/list", new { });

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Mock Cafe", result[0].Name);
        Assert.Equal("https://example.convex.cloud/api/query", handler.LastUri);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task QueryAsync_NonSuccessStatus_ReturnsDefault()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"status\":\"error\",\"value\":null}", System.Text.Encoding.UTF8, "application/json")
        });
        using var http = new HttpClient(handler);
        var client = new ConvexClient(http, "https://example.convex.cloud");

        var result = await client.QueryAsync<List<Attraction>>("places/list", new { });

        Assert.Null(result);
    }

    [Fact]
    public async Task MutationAsync_PostsToMutationEndpoint()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"status\":\"success\",\"value\":42}", System.Text.Encoding.UTF8, "application/json")
        });
        using var http = new HttpClient(handler);
        var client = new ConvexClient(http, "https://example.convex.cloud");

        var result = await client.MutationAsync<int>("settings/save", new { lang = "th" });

        Assert.Equal(42, result);
        Assert.Equal("https://example.convex.cloud/api/mutation", handler.LastUri);
    }
}
