using System.Net.Http.Json;
using api.fullstack.Models;

namespace api.fullstack.Services;

/// <summary>
/// Downloads posts from the upstream posts API and filters/group them in memory.
/// The <see cref="HttpClient"/> (base address and timeout) is supplied via <c>IHttpClientFactory</c>.
/// </summary>
public sealed class PostSearchService : IPostSearchService
{
    private const string PostsPath = "posts";

    private readonly HttpClient _httpClient;
    private readonly ILogger<PostSearchService> _logger;

    public PostSearchService(HttpClient httpClient, ILogger<PostSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserPostsGroup>> SearchByTitleAsync(
        string term,
        CancellationToken cancellationToken = default)
    {
        var posts = await _httpClient
            .GetFromJsonAsync<List<Post>>(PostsPath, cancellationToken)
            .ConfigureAwait(false) ?? [];

        return FilterAndGroup(posts, term);
    }

    private IReadOnlyList<UserPostsGroup> FilterAndGroup(IEnumerable<Post> posts, string term)
    {
        var normalizedTerm = (term ?? string.Empty).Trim();

        if (normalizedTerm.Length == 0)
        {
            return [];
        }

        var matches = posts
            .Where(post => !string.IsNullOrEmpty(post.Title)
                           && post.Title.Contains(normalizedTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(post => post.UserId)
            .ThenBy(post => post.Id)
            .ToList();

        var groups = matches
            .GroupBy(post => post.UserId)
            .OrderBy(group => group.Key)
            .Select(group => new UserPostsGroup
            {
                UserId = group.Key,
                Posts = group.ToList()
            })
            .ToList();

        _logger.LogInformation(
            "Post search for '{Term}' returned {MatchCount} matching post(s) across {GroupCount} user(s).",
            normalizedTerm,
            matches.Count,
            groups.Count);

        return groups;
    }
}
