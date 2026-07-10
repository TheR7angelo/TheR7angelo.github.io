using TheR7angelo.github.io.Domain.Models.Validation;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Service.Interface.Services;

public interface IGithubService
{
    public Task<Result<IEnumerable<GithubRepositoryDto>>> GetAllGithubRepository(CancellationToken cancellationToken = default);
}