using System.Net;
using System.Text;

namespace api.fullstack.Tests.Fakes;

/// <summary>
/// HttpMessageHandler that returns a fixed response, so tests never call the real API.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;

    /// <summary>
    /// Creates a handler that always answers with the given status code and JSON body.
    /// </summary>
    /// <param name="statusCode">Status code to return.</param>
    /// <param name="content">JSON body to return.</param>
    public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    /// <summary>
    /// Returns the configured response for every request.
    /// </summary>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}
