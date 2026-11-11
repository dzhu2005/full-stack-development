namespace api.fullstack.Models;

/// <summary>
/// A single post as returned by the JSONPlaceholder <c>/posts</c> endpoint.
/// </summary>
/// <param name="UserId">Id of the user who wrote the post.</param>
/// <param name="Id">Unique id of the post.</param>
/// <param name="Title">Post title. This is the field we search on.</param>
/// <param name="Body">Post content.</param>
public record Post(int UserId, int Id, string Title, string Body);
