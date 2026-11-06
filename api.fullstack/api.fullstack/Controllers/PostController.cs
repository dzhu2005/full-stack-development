using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace api.fullstack.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{

    private readonly IHttpClientFactory _httpClientFactory;
    public PostController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search term is required.");

        try
        {
            //HTTP call
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            // Deserialize JSON
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var posts = JsonSerializer.Deserialize<List<Post>>(content, options);

            if (posts == null) return Ok(new List<object>());

            // filter and group
            var groupedPosts = posts
                .Where(p => p.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
                .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Posts = g.Select(p => new { p.Id, p.Title, p.Body })
                })
                .ToList();

            // Return grouped results
            return Ok(groupedPosts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while fetching posts.");
        }
    }


}

// JSON shape model matching
public class Post
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
}