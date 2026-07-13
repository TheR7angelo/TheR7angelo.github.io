using Microsoft.JSInterop;
using System.Text.Json;
using TheR7angelo.github.io.Domain.Models.Validation;
using TheR7angelo.github.io.Infrastructure.Interface.Repositories;
using TheR7angelo.github.io.Service.Interface.Services;
using TheR7angelo.github.io.Service.Mapper.Interfaces;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Service.Github;

public class GithubService(
    IGithubRepository githubRepository,
    IGithubDomainToGithubDto githubDomainToGithubDto,
    IJSRuntime jsRuntime) : IGithubService
{
    private const string CacheKeyRepos = "github_repos_cache";
    private const string CacheKeyInfos = "github_infos_cache";

    public async Task<Result<IEnumerable<GithubRepositoryDto>>> GetAllGithubRepository(CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedJson = await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", cancellationToken, CacheKeyRepos);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                var cachedDtos = JsonSerializer.Deserialize<IEnumerable<GithubRepositoryDto>>(cachedJson);
                if (cachedDtos != null)
                {
                    return Result<IEnumerable<GithubRepositoryDto>>.Success(cachedDtos);
                }
            }
        }
        catch
        {
            // Ignore
        }

        var result = await githubRepository.GetAllGithubRepository(cancellationToken);
        var mappedResult = result.MapSequence(githubDomainToGithubDto.MapToDto);

        if (mappedResult is not { IsSuccess: true, Value: not null }) return mappedResult;

        try
        {
            var jsonToCache = JsonSerializer.Serialize(mappedResult.Value);
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, CacheKeyRepos, jsonToCache);
        }
        catch
        {
            // Ignore
        }

        return mappedResult;
    }

    public async Task<Result<IEnumerable<GithubRepositoryInformationDto>>> GetAllGithubRepositoryInformation(
        IEnumerable<GithubRepositoryDto> githubRepositoryDtos,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedJson =
                await jsRuntime.InvokeAsync<string>("sessionStorage.getItem", cancellationToken, CacheKeyInfos);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                var cachedInfos = JsonSerializer.Deserialize<IEnumerable<GithubRepositoryInformationDto>>(cachedJson);
                if (cachedInfos != null)
                {
                    return Result<IEnumerable<GithubRepositoryInformationDto>>.Success(cachedInfos);
                }
            }
        }
        catch
        {
            // Ignore
        }

        var domains = githubRepositoryDtos.Select(githubDomainToGithubDto.MapToDomain);
        var result = await githubRepository.GetAllGithubRepositoryInformation(domains, cancellationToken);
        var mappedResult = result.MapSequence(githubDomainToGithubDto.MapToDto);

        if (mappedResult is not { IsSuccess: true, Value: not null }) return mappedResult;

        try
        {
            var jsonToCache = JsonSerializer.Serialize(mappedResult.Value);
            await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, CacheKeyInfos, jsonToCache);
        }
        catch
        {
            // Ignore
        }

        return mappedResult;
    }
}