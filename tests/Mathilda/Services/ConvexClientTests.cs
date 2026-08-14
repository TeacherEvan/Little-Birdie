using System.Net;
using System.Text.Json;
using Mathilda.Services;
using Xunit;

namespace Mathilda.Tests.Services;

public class ConvexClientTests
{
    [Fact]
    public async Task QueryAsync_Parses_Success_Envelope()
    {
        var handler = new StubHandler(req =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.EndsWith("/api/query", req.RequestUri!.ToString());
            return new ConvexEnvelope<string>("success", "hello");
        });
        using var http = new HttpClient(handler);
        var client = new ConvexClient(http, "https://demo.convex.cloud/");

        var result = await client.QueryAsync<string>("tasks:list");

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task QueryAsync_Returns_Default_On_NonSuccess_Status()
    {
        var handler = new StubHandler(_ =>
            new ConvexEnvelope<string>("error", "boom"));
        using var http = new HttpClient(handler);
        var client = new ConvexClient(http, "https://demo.convex.cloud");

        var result = await client.QueryAsync<string>("tasks:list");

        Assert.Null(result);
    }

    [Fact]
    public async Task MutationAsync_Posts_To_Mutation_Endpoint()
    {
        var seen = false;
        var handler = new StubHandler(req =>
        {
            seen = req.RequestUri!.ToString().EndsWith("/api/mutation");
            return new ConvexEnvelope<int>("success", 42);
        });
        using var http = new HttpClient(handler);
        var client = new ConvexClient(http, "https://demo.convex.cloud");

        var result = await client.MutationAsync<int>("tasks:add", new { name = "x" });

        Assert.True(seen);
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Constructor_Trims_Trailing_Slash()
    {
        var handler = new StubHandler(req =>
        {
            Assert.DoesNotContain("//api", req.RequestUri!.ToString());
            Assert.EndsWith("/api/query", req.RequestUri.ToString());
            return new ConvexEnvelope<bool>("success", true);
        });
        using var http = new HttpClient(handler);
        var client = new ConvexClient(http, "https://demo.convex.cloud///");

        var result = await client.QueryAsync<bool>("ping");

        Assert.True(result);
    }

    [Fact]
    public async Task QueryAsync_Throws_On_NonSuccess_StatusCode()
    {
        var handler = new StubHandler(_ => null, HttpStatusCode.InternalServerError);
        using var http = new HttpClient(handler);
        var client = new ConvexClient(http, "https://demo.convex.cloud");

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.QueryAsync<string>("tasks:list"));
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, object?> _responder;
        private readonly HttpStatusCode _status;

        public StubHandler(Func<HttpRequestMessage, object?> responder,
            HttpStatusCode status = HttpStatusCode.OK)
        {
            _responder = responder;
            _status = status;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var payload = _responder(request);
            var json = payload is null ? "{}" : JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return new HttpResponseMessage(_status) { Content = content };
        }
    }
}
