using Riok.Mapperly.Abstractions;
using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Infrastructure.Github.Entities;
using TheR7angelo.github.io.Infrastructure.Mapper.Interfaces;

namespace TheR7angelo.github.io.Infrastructure.Mapper;

[Mapper]
public partial class GithubEntitiesToGithubDomain : IGithubEntitiesToGithubDomain
{
    [MapProperty(nameof(GithubRepository.Owner), nameof(GithubRepositoryDomain.OwnerDomain))]
    [MapProperty(nameof(GithubRepository.License), nameof(GithubRepositoryDomain.LicenseDomain))]
    public partial GithubRepositoryDomain MapToDomain(GithubRepository entity);

    public partial GithubOwnerDomain MapToDomain(GithubOwner entity);

    public partial GithubLicenseDomain MapToDomain(GithubLicense entity);
}