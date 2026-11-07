using System.Net;
using api.fullstack.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api.fullstack.Tests;

public class PostSearchServiceTests
{
    // Deliberately unsorted/shuffled so the tests prove the service orders its output.
    private const string SamplePostsJson = """
    [
      { "userId": 1, "id": 6, "title": "dolorem eum magni", "body": "body 6" },
      { "userId": 1, "id": 1, "title": "sunt aut facere repellat provident", "body": "body 1" },
      { "userId": 2, "id": 4, "title": "eum et est occaecati", "body": "body 4" },
      { "userId": 1, "id": 2, "title": "qui est esse", "body": "body 2" },
      { "userId": 3, "id": 5, "title": "nesciunt quas odio", "body": "body 5" },
      { "userId": 2, "id": 3, "title": "ea molestias quasi exercitationem", "body": "body 3" }
    ]
    """;

    private static (PostSearchService Service, StubHttpMessageHandler Handler) CreateService(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string content = SamplePostsJson)
    {
        var handler = new StubHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com/")
        };

        return (new PostSearchService(httpClient, NullLogger<PostSearchService>.Instance), handler);
    }

    [Fact]
    public async Task SearchByTitleAsync_GroupsMatchingPostsByUser_OrderedByUserThenPost()
    {
        var (service, _) = CreateService();

        var groups = await service.SearchByTitleAsync("e");

        // Every sample title contains an "e", so all three users should be represented.
        Assert.Equal([1, 2, 3], groups.Select(group => group.UserId));

        Assert.Equal([1, 2, 6], groups[0].Posts.Select(post => post.Id));
        Assert.Equal([3, 4], groups[1].Posts.Select(post => post.Id));
        Assert.Equal([5], groups[2].Posts.Select(post => post.Id));

        Assert.All(groups, group => Assert.All(group.Posts, post => Assert.Equal(group.UserId, post.UserId)));
    }

    [Fact]
    public async Task SearchByTitleAsync_MatchesTitleCaseInsensitively()
    {
        var (service, _) = CreateService();

        var groups = await service.SearchByTitleAsync("EUM");

        Assert.Equal(2, groups.Count);
        Assert.Equal(1, groups[0].UserId);
        Assert.Equal([6], groups[0].Posts.Select(post => post.Id));
        Assert.Equal(2, groups[1].UserId);
        Assert.Equal([4], groups[1].Posts.Select(post => post.Id));
    }

    [Fact]
    public async Task SearchByTitleAsync_ReturnsEmpty_WhenNoTitleMatches()
    {
        var (service, _) = CreateService();

        var groups = await service.SearchByTitleAsync("this-term-matches-nothing");

        Assert.Empty(groups);
    }

    [Fact]
    public async Task SearchByTitleAsync_ReturnsEmpty_WhenTermIsBlank()
    {
        var (service, _) = CreateService();

        var groups = await service.SearchByTitleAsync("   ");

        Assert.Empty(groups);
    }

    [Fact]
    public async Task SearchByTitleAsync_DownloadsPostsFromTheUpstreamPostsEndpoint()
    {
        var (service, handler) = CreateService();

        await service.SearchByTitleAsync("eum");

        var requestedUri = Assert.Single(handler.RequestedUris);
        Assert.Equal("https://jsonplaceholder.typicode.com/posts", requestedUri.ToString());
    }

    [Fact]
    public async Task SearchByTitleAsync_ThrowsHttpRequestException_WhenUpstreamFails()
    {
        var (service, _) = CreateService(HttpStatusCode.InternalServerError, "{}");

        await Assert.ThrowsAsync<HttpRequestException>(() => service.SearchByTitleAsync("eum"));
    }
}
