using System.Net;
using System.Text;

namespace api.fullstack.Tests;

/// <summary>
/// Test double that stands in for the upstream posts API, so tests never touch the network.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public StubHttpMessageHandler(HttpStatusCode statusCode, string content)
        : this(_ => Json(statusCode, content))
    {
    }

    public List<Uri> RequestedUris { get; } = [];

    public static HttpResponseMessage Json(HttpStatusCode statusCode, string content) =>
        new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestedUris.Add(request.RequestUri!);

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_responder(request));
    }
}
