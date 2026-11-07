using System.Text.Json;
using api.fullstack.Models;
using api.fullstack.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.fullstack.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class PostController : ControllerBase
{
    private readonly IPostSearchService _postSearchService;
    private readonly ILogger<PostController> _logger;

    public PostController(IPostSearchService postSearchService, ILogger<PostController> logger)
    {
        _postSearchService = postSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Searches posts by title and returns the matches grouped by user.
    /// </summary>
    /// <param name="q">The search term. Matching is case-insensitive.</param>
    /// <param name="cancellationToken">Cancellation token for the inbound request.</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<UserPostsGroup>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<IReadOnlyList<UserPostsGroup>>> Search(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var term = q?.Trim();

        if (string.IsNullOrEmpty(term))
        {
            ModelState.AddModelError(nameof(q), "A non-empty search term ('q') is required.");
            return ValidationProblem(ModelState);
        }

        try
        {
            var groups = await _postSearchService.SearchByTitleAsync(term, cancellationToken);
            return Ok(groups);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The caller is still connected, so the upstream call itself timed out.
            _logger.LogWarning("Post search for '{Term}' timed out while calling the upstream posts API.", term);

            return Problem(
                statusCode: StatusCodes.Status504GatewayTimeout,
                title: "The posts service timed out.",
                detail: "The upstream posts API did not respond in time. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Post search for '{Term}' failed calling the upstream posts API.", term);

            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "The posts service is unavailable.",
                detail: "The upstream posts API could not be reached. Please try again later.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Post search for '{Term}' received an unreadable response from the upstream posts API.", term);

            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "The posts service returned an invalid response.",
                detail: "The upstream posts API returned data that could not be read.");
        }
    }
}