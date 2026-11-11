using System.Net.Http.Json;
using api.fullstack.Models;

namespace api.fullstack.Services;

/// <summary>
/// Default <see cref="IPostService"/> that downloads posts from JSONPlaceholder.
/// The <see cref="HttpClient"/> is a typed client created by <c>IHttpClientFactory</c>,
/// so its base address is set once in <c>Program.cs</c>.
/// </summary>
public class PostService : IPostService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates the service with the HttpClient that is configured for the posts API.
    /// </summary>
    /// <param name="httpClient">Typed client with its BaseAddress pointing to the posts API.</param>
    public PostService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserPosts>> SearchByTitleAsync(string term, CancellationToken cancellationToken = default)
    {
        var posts = await GetPostsAsync(cancellationToken);

        return posts
            .Where(post => post.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
            .GroupBy(post => post.UserId)
            .OrderBy(group => group.Key)
            .Select(group => new UserPosts(group.Key, group.ToList()))
            .ToList();
    }

    /// <summary>
    /// Downloads all posts and deserializes them into <see cref="Post"/> objects.
    /// Throws <see cref="HttpRequestException"/> when the response is not successful.
    /// </summary>
    /// <param name="cancellationToken">Cancels the HTTP call.</param>
    /// <returns>All posts, or an empty list when the body is empty.</returns>
    private async Task<List<Post>> GetPostsAsync(CancellationToken cancellationToken)
    {
        var posts = await _httpClient.GetFromJsonAsync<List<Post>>("posts", cancellationToken);
        return posts ?? [];
    }
}
