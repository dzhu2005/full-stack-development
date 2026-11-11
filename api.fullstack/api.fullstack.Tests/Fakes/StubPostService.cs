using api.fullstack.Models;
using api.fullstack.Services;

namespace api.fullstack.Tests.Fakes;

/// <summary>
/// Hand-written IPostService stub for controller tests. Returns fixed results or throws a fixed exception.
/// </summary>
public class StubPostService : IPostService
{
    /// <summary>Results returned by <see cref="SearchByTitleAsync"/>.</summary>
    public IReadOnlyList<UserPosts> Results { get; init; } = [];

    /// <summary>When set, <see cref="SearchByTitleAsync"/> throws this instead of returning results.</summary>
    public Exception? ExceptionToThrow { get; init; }

    /// <summary>The last term the controller passed in, so tests can assert on it.</summary>
    public string? LastTerm { get; private set; }

    /// <summary>
    /// Records the term, then returns <see cref="Results"/> or throws <see cref="ExceptionToThrow"/>.
    /// </summary>
    public Task<IReadOnlyList<UserPosts>> SearchByTitleAsync(string term, CancellationToken cancellationToken = default)
    {
        LastTerm = term;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(Results);
    }
}
