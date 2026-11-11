using System.Net;
using api.fullstack.Services;
using api.fullstack.Tests.Fakes;

namespace api.fullstack.Tests.Services;

[TestFixture]
public class PostServiceTests
{
    private const string PostsJson = """
        [
          { "userId": 2, "id": 11, "title": "Qui est esse", "body": "body 11" },
          { "userId": 1, "id": 1, "title": "sunt aut facere", "body": "body 1" },
          { "userId": 1, "id": 2, "title": "qui est esse", "body": "body 2" },
          { "userId": 3, "id": 21, "title": "asperiores ea ipsam", "body": "body 21" }
        ]
        """;

    /// <summary>
    /// Builds a PostService whose HttpClient is backed by a fake handler.
    /// </summary>
    private static PostService CreateService(HttpStatusCode statusCode = HttpStatusCode.OK, string content = PostsJson)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(statusCode, content))
        {
            BaseAddress = new Uri("https://example.test/")
        };
        return new PostService(httpClient);
    }

    [Test]
    public async Task SearchByTitleAsync_MatchesTitleIgnoringCase()
    {
        var service = CreateService();

        var result = await service.SearchByTitleAsync("QUI EST");

        var matchedIds = result.SelectMany(group => group.Posts).Select(post => post.Id);
        Assert.That(matchedIds, Is.EquivalentTo(new[] { 11, 2 }));
    }

    [Test]
    public async Task SearchByTitleAsync_GroupsByUserOrderedByUserId()
    {
        var service = CreateService();

        var result = await service.SearchByTitleAsync("e");

        Assert.That(result.Select(group => group.UserId), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(result[0].Posts.Select(post => post.Id), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task SearchByTitleAsync_NoMatch_ReturnsEmptyList()
    {
        var service = CreateService();

        var result = await service.SearchByTitleAsync("zzzz");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void SearchByTitleAsync_UpstreamError_ThrowsHttpRequestException()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, "{}");

        Assert.ThrowsAsync<HttpRequestException>(() => service.SearchByTitleAsync("qui"));
    }
}
