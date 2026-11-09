using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace api.fullstack.Controllers;

public class Post
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
}

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public PostController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("JsonPlaceholder");
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Please enter a search term.");

        try
        {
            var posts = await _httpClient.GetFromJsonAsync<List<Post>>("posts")
                         ?? new List<Post>();

            var results = posts
                .Where(post => post.Title.Contains(
                    q, StringComparison.OrdinalIgnoreCase))
                .GroupBy(post => post.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    Posts = group.ToList()
                });

            return Ok(results);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502,
                "Could not get posts from the external service.");
        }
    }
}