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
    /// <remarks>
    /// The posts are already in memory after the download, so this is plain LINQ to Objects
    /// over <see cref="IEnumerable{T}"/> (IQueryable only helps when a provider such as EF Core
    /// can run the query at the data source). The result is materialized once with
    /// <c>ToArray()</c> so the query does not run again during JSON serialization, and arrays
    /// are used because nothing modifies the collections afterwards.
    /// </remarks>
    public async Task<IReadOnlyList<UserPosts>> SearchByTitleAsync(string term, CancellationToken cancellationToken = default)
    {
        IEnumerable<Post> posts = await GetPostsAsync(cancellationToken);

        return posts
            .Where(post => post.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
            .GroupBy(post => post.UserId)
            .OrderBy(group => group.Key)
            .Select(group => new UserPosts(group.Key, group.ToArray()))
            .ToArray();
    }

    /// <summary>
    /// Downloads all posts and deserializes them into <see cref="Post"/> objects.
    /// Throws <see cref="HttpRequestException"/> when the response is not successful.
    /// </summary>
    /// <param name="cancellationToken">Cancels the HTTP call.</param>
    /// <returns>All posts, or an empty array when the body is empty.</returns>
    private async Task<Post[]> GetPostsAsync(CancellationToken cancellationToken)
    {
        var posts = await _httpClient.GetFromJsonAsync<Post[]>("posts", cancellationToken);
        return posts ?? [];
    }
}
