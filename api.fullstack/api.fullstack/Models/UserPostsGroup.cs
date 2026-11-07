namespace api.fullstack.Models;

/// <summary>
/// The posts that matched a search term, grouped under the user who authored them.
/// </summary>
public sealed record UserPostsGroup
{
    public int UserId { get; init; }

    public IReadOnlyList<Post> Posts { get; init; } = [];
}
