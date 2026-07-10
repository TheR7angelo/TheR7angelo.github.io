using TheR7angelo.github.io.Domain.Models.Validation;
using TheR7angelo.github.io.Infrastructure.Interface.Repositories;
using TheR7angelo.github.io.Service.Interface.Services;
using TheR7angelo.github.io.Service.Mapper.Interfaces;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Service.Github;

public class GithubService(IGithubRepository githubRepository,
    IGithubDomainToGithubDto githubDomainToGithubDto) : IGithubService
{
    public async Task<Result<IEnumerable<GithubRepositoryDto>>> GetAllGithubRepository(CancellationToken cancellationToken = default)
    {
        var result = await githubRepository.GetAllGithubRepository(cancellationToken);
        return result.MapSequence(githubDomainToGithubDto.MapToDto);
    }
}