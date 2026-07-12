using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Service.Mapper.Interfaces;

public interface IGithubDomainToGithubDto
{
    public GithubRepositoryDto MapToDto(GithubRepositoryDomain entity);

    public GithubOwnerDto MapToDto(GithubOwnerDomain entity);

    public GithubLicenseDto MapToDto(GithubLicenseDomain entity);

    public GithubRepositoryDomain MapToDomain(GithubRepositoryDto entity);

    public GithubOwnerDomain MapToDomain(GithubOwnerDto entity);

    public GithubLicenseDomain MapToDomain(GithubLicenseDto entity);

    public GithubRepositoryInformationDto MapToDto(GithubRepositoryInformationDomain entity);

    public GithubBadgeDto MapToDto(GithubBadgeDomain entity);
}