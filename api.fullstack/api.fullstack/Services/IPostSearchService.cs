using api.fullstack.Models;

namespace api.fullstack.Services;

public interface IPostSearchService
{
    /// <summary>
    /// Finds the posts whose title contains <paramref name="term"/> and groups them by author.
    /// </summary>
    /// <param name="term">The search term. Matching is case-insensitive.</param>
    /// <param name="cancellationToken">Token used to cancel the upstream request.</param>
    /// <returns>
    /// The matching posts grouped by <see cref="UserPostsGroup.UserId"/> (users ordered ascending,
    /// posts within each group ordered by id ascending).
    /// </returns>
    Task<IReadOnlyList<UserPostsGroup>> SearchByTitleAsync(string term, CancellationToken cancellationToken = default);
}
