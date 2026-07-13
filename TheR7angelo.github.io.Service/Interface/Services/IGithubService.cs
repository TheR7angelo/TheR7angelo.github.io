using TheR7angelo.github.io.Domain.Models.Validation;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Service.Interface.Services;

public interface IGithubService
{
    /// <summary>
    /// Retrieves all GitHub repositories.
    /// </summary>
    /// <param name="cancellationToken">CancellationToken to monitor for cancellation requests.</param>
    /// <returns>A Task containing a Result object with an IEnumerable of GithubRepositoryDto if successful, or an error message otherwise.</returns>
    public Task<Result<IEnumerable<GithubRepositoryDto>>> GetAllGithubRepository(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves information about GitHub repositories based on a list of repository DTOs.
    /// </summary>
    /// <param name="githubRepositoryDtos">An IEnumerable of GithubRepositoryDto representing the repositories to retrieve information for.</param>
    /// <param name="cancellationToken">CancellationToken to monitor for cancellation requests.</param>
    /// <returns>A Task containing a Result object with an IEnumerable of GithubRepositoryInformationDto if successful, or an error message otherwise.</returns>
    public Task<Result<IEnumerable<GithubRepositoryInformationDto>>> GetAllGithubRepositoryInformation(IEnumerable<GithubRepositoryDto> githubRepositoryDtos, CancellationToken cancellationToken = default);
}