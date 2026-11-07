namespace api.fullstack.Models;

/// <summary>
/// A single post as returned by the upstream posts API.
/// </summary>
public sealed record Post
{
    public int UserId { get; init; }

    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;
}
