using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Domain.Models.Validation;

namespace TheR7angelo.github.io.Infrastructure.Interface.Repositories;

public interface IGithubRepository
{
    public const string HttpGithubClientName = "GithubClient";

    public Task InitializeAsync();

    /// <summary>
    /// Retrieves all GitHub repositories.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result of the task is a Result containing an IEnumerable of GithubRepositoryDomain objects if successful, or a failure indicating an error.</returns>
    public Task<Result<IEnumerable<GithubRepositoryDomain>>> GetAllGithubRepository(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all GitHub repository information.
    /// </summary>
    /// <param name="domains">The collection of GithubRepositoryDomain objects to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result of the task is a Result containing an IEnumerable of GithubRepositoryInformationDomain objects if successful, or a failure indicating an error.</returns>
    public Task<Result<IEnumerable<GithubRepositoryInformationDomain>>> GetAllGithubRepositoryInformation(IEnumerable<GithubRepositoryDomain> domains, CancellationToken cancellationToken = default);
}