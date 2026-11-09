using api.fullstack.Models;
using api.fullstack.Dtos;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace api.fullstack.Services;

public class PostService : IPostService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PostService> _logger;
    private const string PostsUrl = "https://jsonplaceholder.typicode.com/posts";

    public PostService(IHttpClientFactory httpClientFactory, ILogger<PostService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<UserPostsDto>> GetPostsAsync(string? q = null)
    {
        try
        {
            
            // Get HttpClient from factory
            HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(PostsUrl);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();

            // Deserialize JSON into C# objects
            List<Post>? posts = JsonSerializer.Deserialize<List<Post>>(jsonContent);

            if (posts == null || posts.Count == 0)
            {
                _logger.LogWarning("No posts received from external API");
                return new List<UserPostsDto>();
            }

            
            if (string.IsNullOrWhiteSpace(q))
            {
                // If no search term, return all posts grouped by user
                var groupedByUser = posts
                    .GroupBy(p => p.userId)
                    .Select(g => new UserPostsDto
                    {
                        UserId = g.Key,
                        Posts = g.Select(post => new PostDto
                        {
                            Id = post.id,
                            Title = post.title,
                            Body = post.body
                        }).ToList()
                    })
                    .ToList();

                return groupedByUser;
            }

            // Filter posts by title matching search term (case-insensitive)
            var filteredPosts = posts
                .Where(p => p.title.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

            
            // Group filtered posts by user
            var groupedResults = filteredPosts
                .GroupBy(p => p.userId)
                .Select(g => new UserPostsDto
                {
                    UserId = g.Key,
                    Posts = g.Select(post => new PostDto
                    {
                        Id = post.id,
                        Title = post.title,
                        Body = post.body
                    }).ToList()
                })
                .ToList();

            return groupedResults;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while fetching posts from {Url}", PostsUrl);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize JSON response from posts API");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching posts");
            throw;
        }
    }
}

