namespace api.fullstack.Models;

/// <summary>
/// The posts that matched a search, grouped under the user who wrote them.
/// </summary>
/// <param name="UserId">Id of the user the posts belong to.</param>
/// <param name="Posts">The matching posts written by this user.</param>
public record UserPosts(int UserId, IReadOnlyList<Post> Posts);
