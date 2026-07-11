using Riok.Mapperly.Abstractions;
using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Service.Mapper.Interfaces;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Service.Mapper;

[Mapper]
public partial class GithubDomainToGithubDto : IGithubDomainToGithubDto
{
    [MapProperty(nameof(GithubRepositoryDomain.OwnerDomain), nameof(GithubRepositoryDto.OwnerDto))]
    [MapProperty(nameof(GithubRepositoryDomain.LicenseDomain), nameof(GithubRepositoryDto.LicenseDto))]
    public partial GithubRepositoryDto MapToDto(GithubRepositoryDomain entity);

    public partial GithubOwnerDto MapToDto(GithubOwnerDomain entity);

    public partial GithubLicenseDto MapToDto(GithubLicenseDomain entity);

    [MapProperty(nameof(GithubRepositoryDto.OwnerDto), nameof(GithubRepositoryDomain.OwnerDomain))]
    [MapProperty(nameof(GithubRepositoryDto.LicenseDto), nameof(GithubRepositoryDomain.LicenseDomain))]
    public partial GithubRepositoryDomain MapToDomain(GithubRepositoryDto entity);

    public partial GithubOwnerDomain MapToDomain(GithubOwnerDto entity);

    public partial GithubLicenseDomain MapToDomain(GithubLicenseDto entity);

    public partial GithubRepositoryInformationDto MapToDto(GithubRepositoryInformationDomain entity);
}