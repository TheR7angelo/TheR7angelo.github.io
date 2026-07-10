using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Domain.Models.Validation;

namespace TheR7angelo.github.io.Infrastructure.Interface.Repositories;

public interface IGithubRepository
{
    public const string HttpGithubClientName = "GithubClient";

    public Task<Result<IEnumerable<GithubRepositoryDomain>>> GetAllGithubRepository(CancellationToken cancellationToken = default);
}