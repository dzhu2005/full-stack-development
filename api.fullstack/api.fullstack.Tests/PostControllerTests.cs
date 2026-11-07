using System.Net;
using api.fullstack.Controllers;
using api.fullstack.Models;
using api.fullstack.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.fullstack.Tests;

public class PostControllerTests
{
    private static PostController CreateController(IPostSearchService service)
    {
        var services = new ServiceCollection();
        services.AddControllers();

        var provider = services.BuildServiceProvider();

        return new PostController(service, NullLogger<PostController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider }
            }
        };
    }

    [Fact]
    public async Task Search_ReturnsOk_WithGroupedResults()
    {
        var expected = new List<UserPostsGroup>
        {
            new()
            {
                UserId = 7,
                Posts = [new Post { UserId = 7, Id = 70, Title = "a matching title", Body = "body" }]
            }
        };

        var service = new StubPostSearchService((_, _) => Task.FromResult<IReadOnlyList<UserPostsGroup>>(expected));
        var controller = CreateController(service);

        var result = await controller.Search("matching", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.Same(expected, okResult.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Search_ReturnsBadRequest_WhenTermIsMissing(string? term)
    {
        var service = new StubPostSearchService((_, _) =>
            throw new InvalidOperationException("The service must not be called for a blank term."));

        var controller = CreateController(service);

        var result = await controller.Search(term, CancellationToken.None);

        var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Null(service.ReceivedTerm);
    }

    [Fact]
    public async Task Search_ReturnsBadGateway_WhenUpstreamIsUnreachable()
    {
        var service = new StubPostSearchService((_, _) =>
            throw new HttpRequestException("Connection refused."));

        var controller = CreateController(service);

        var result = await controller.Search("eum", CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, objectResult.StatusCode);
        Assert.IsType<ProblemDetails>(objectResult.Value);
    }

    [Fact]
    public async Task Search_ReturnsGatewayTimeout_WhenUpstreamTimesOut()
    {
        var service = new StubPostSearchService((_, _) =>
            throw new TaskCanceledException("The request timed out.", new TimeoutException()));

        var controller = CreateController(service);

        var result = await controller.Search("eum", CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status504GatewayTimeout, objectResult.StatusCode);
    }

    [Fact]
    public async Task Search_TrimsTheTermBeforeSearching()
    {
        var service = new StubPostSearchService((_, _) =>
            Task.FromResult<IReadOnlyList<UserPostsGroup>>([]));

        var controller = CreateController(service);

        await controller.Search("  eum  ", CancellationToken.None);

        Assert.Equal("eum", service.ReceivedTerm);
    }

    private sealed class StubPostSearchService(
        Func<string, CancellationToken, Task<IReadOnlyList<UserPostsGroup>>> handler) : IPostSearchService
    {
        public string? ReceivedTerm { get; private set; }

        public Task<IReadOnlyList<UserPostsGroup>> SearchByTitleAsync(
            string term,
            CancellationToken cancellationToken = default)
        {
            ReceivedTerm = term;
            return handler(term, cancellationToken);
        }
    }
}
