using Microsoft.AspNetCore.Mvc;
using api.fullstack.Dtos;
using api.fullstack.Services;
using System.Text.Json;

namespace api.fullstack.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostController> _logger;

    public PostController(IPostService postService, ILogger<PostController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<UserPostsDto>>> GetPosts([FromQuery] string? q = null)
    {
        try
        {
            
            var results = await _postService.GetPostsAsync(q);

            return Ok(results);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch posts from external API");
            return StatusCode(502, "Failed to fetch posts from external service. Please try again later.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse posts data from external API");
            return StatusCode(502, "Invalid response format from external service.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred in GetPosts endpoint");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}