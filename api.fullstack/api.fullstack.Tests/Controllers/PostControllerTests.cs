using api.fullstack.Controllers;
using api.fullstack.Models;
using api.fullstack.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace api.fullstack.Tests.Controllers;

[TestFixture]
public class PostControllerTests
{
    /// <summary>
    /// Creates the controller with the given service stub and a no-op logger.
    /// </summary>
    private static PostController CreateController(StubPostService service)
    {
        return new PostController(service, NullLogger<PostController>.Instance);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task Search_BlankTerm_ReturnsBadRequest(string? term)
    {
        var controller = CreateController(new StubPostService());

        var response = await controller.Search(term, CancellationToken.None);

        var result = response.Result as ObjectResult;
        Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task Search_ValidTerm_ReturnsOkWithGroups()
    {
        var groups = new List<UserPosts>
        {
            new(1, [new Post(1, 2, "qui est esse", "body")])
        };
        var service = new StubPostService { Results = groups };
        var controller = CreateController(service);

        var response = await controller.Search("  qui  ", CancellationToken.None);

        var result = response.Result as OkObjectResult;
        Assert.That(result?.Value, Is.EqualTo(groups));
        Assert.That(service.LastTerm, Is.EqualTo("qui"));
    }

    [Test]
    public async Task Search_UpstreamFails_ReturnsBadGateway()
    {
        var service = new StubPostService { ExceptionToThrow = new HttpRequestException("down") };
        var controller = CreateController(service);

        var response = await controller.Search("qui", CancellationToken.None);

        var result = response.Result as ObjectResult;
        Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status502BadGateway));
    }

    [Test]
    public async Task Search_UpstreamTimesOut_ReturnsBadGateway()
    {
        var service = new StubPostService { ExceptionToThrow = new TaskCanceledException("timeout") };
        var controller = CreateController(service);

        var response = await controller.Search("qui", CancellationToken.None);

        var result = response.Result as ObjectResult;
        Assert.That(result?.StatusCode, Is.EqualTo(StatusCodes.Status502BadGateway));
    }
}
