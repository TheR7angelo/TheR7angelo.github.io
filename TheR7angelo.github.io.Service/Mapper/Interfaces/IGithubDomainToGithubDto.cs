using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Service.Mapper.Interfaces;

public interface IGithubDomainToGithubDto
{
    public GithubRepositoryDto MapToDto(GithubRepositoryDomain entity);

    public GithubOwnerDto MapToDto(GithubOwnerDomain entity);

    public GithubLicenseDto MapToDto(GithubLicenseDomain entity);
}