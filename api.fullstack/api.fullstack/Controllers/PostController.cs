using api.fullstack.Models;
using api.fullstack.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.fullstack.Controllers;

/// <summary>
/// Endpoints for searching posts.
/// </summary>
[ApiController]
[Route("[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostController> _logger;

    /// <summary>
    /// Creates the controller with its dependencies.
    /// </summary>
    /// <param name="postService">Service that loads and searches posts.</param>
    /// <param name="logger">Logger used when the upstream API fails.</param>
    public PostController(IPostService postService, ILogger<PostController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    /// <summary>
    /// Searches posts by title and returns the matches grouped by user.
    /// </summary>
    /// <remarks>Example: <c>GET /Post/search?q=qui</c></remarks>
    /// <param name="q">Text to look for in the post title. Matching is case-insensitive.</param>
    /// <param name="cancellationToken">Set by ASP.NET Core when the client cancels the request.</param>
    /// <response code="200">Matching posts grouped by user. Empty array when nothing matches.</response>
    /// <response code="400">The search term is missing or blank.</response>
    /// <response code="502">The external posts API could not be reached.</response>
    [HttpGet("search")]
    [ProducesResponseType<IReadOnlyList<UserPosts>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IReadOnlyList<UserPosts>>> Search([FromQuery] string? q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Problem(
                title: "Search term is required.",
                detail: "Pass the text to search for in the 'q' query parameter.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var results = await _postService.SearchByTitleAsync(q.Trim(), cancellationToken);
            return Ok(results);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to load posts from the upstream API.");
            return UpstreamUnavailable();
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The client did not cancel, so this is the HttpClient timeout.
            _logger.LogError(ex, "Timed out loading posts from the upstream API.");
            return UpstreamUnavailable();
        }
    }

    /// <summary>
    /// Builds the 502 ProblemDetails response used when the posts API fails or times out.
    /// </summary>
    /// <returns>A 502 Bad Gateway result.</returns>
    private ObjectResult UpstreamUnavailable()
    {
        return Problem(
            title: "Could not load posts from the upstream service.",
            detail: "Please try again in a moment.",
            statusCode: StatusCodes.Status502BadGateway);
    }
}
