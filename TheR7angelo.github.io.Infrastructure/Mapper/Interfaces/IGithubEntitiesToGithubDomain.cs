using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Infrastructure.Github.Entities;

namespace TheR7angelo.github.io.Infrastructure.Mapper.Interfaces;

public interface IGithubEntitiesToGithubDomain
{
    public GithubRepositoryDomain MapToDomain(GithubRepository entity);

    public GithubOwnerDomain MapToDomain(GithubOwner entity);

    public GithubLicenseDomain MapToDomain(GithubLicense entity);
}