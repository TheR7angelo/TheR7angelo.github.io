using System.Net.Http.Json;
using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Domain.Models.Validation;
using TheR7angelo.github.io.Infrastructure.Interface.Repositories;
using TheR7angelo.github.io.Infrastructure.Mapper.Interfaces;

namespace TheR7angelo.github.io.Infrastructure.Github;

public class GithubRepository(IHttpClientFactory httpClientFactory,
    IGithubEntitiesToGithubDomain githubEntitiesToGithubDomain) : IGithubRepository
{
    public async Task<Result<IEnumerable<GithubRepositoryDomain>>> GetAllGithubRepository(CancellationToken cancellationToken = default)
    {
        var blackListNames = new List<string> { "winget-pkgs", "TheR7angelo.github.io" };

        var httpClient = httpClientFactory.CreateClient(IGithubRepository.HttpGithubClientName);

        try
        {
            using var response = await httpClient.GetAsync(
                "/users/TheR7angelo/repos",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<IEnumerable<GithubRepositoryDomain>>.Failure(ErrorCode.Http, response.ReasonPhrase ?? string.Empty);
            }

            var repositories = await response.Content.ReadFromJsonAsync<List<Entities.GithubRepository>>(cancellationToken);
            var repositoriesDomain = repositories!.Where(s => !blackListNames.Contains(s.Name))
                .Select(githubEntitiesToGithubDomain.MapToDomain);

            return Result<IEnumerable<GithubRepositoryDomain>>.Success(repositoriesDomain);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<GithubRepositoryDomain>>.Failure(ErrorCode.Http, ex.Message);
        }
    }
}