using api.fullstack.Models;

namespace api.fullstack.Services;

/// <summary>
/// Reads posts from the external posts API and searches them.
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Finds the posts whose title contains <paramref name="term"/> (case-insensitive)
    /// and groups them by user.
    /// </summary>
    /// <param name="term">Text to look for in the post title.</param>
    /// <param name="cancellationToken">Cancels the upstream call if the client goes away.</param>
    /// <returns>One entry per user, ordered by user id. Empty when nothing matches.</returns>
    /// <exception cref="HttpRequestException">The posts API could not be reached or returned an error.</exception>
    Task<IReadOnlyList<UserPosts>> SearchByTitleAsync(string term, CancellationToken cancellationToken = default);
}
