using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TheR7angelo.github.io.Domain.Models.GitHub;
using TheR7angelo.github.io.Domain.Models.Validation;
using TheR7angelo.github.io.Infrastructure.Github.Entities;
using TheR7angelo.github.io.Infrastructure.Interface.Repositories;
using TheR7angelo.github.io.Infrastructure.Mapper.Interfaces;
using YamlDotNet.Serialization;

namespace TheR7angelo.github.io.Infrastructure.Github;

public class GithubRepository(
    IHttpClientFactory httpClientFactory,
    IGithubEntitiesToGithubDomain githubEntitiesToGithubDomain,
    ILogger<GithubRepository> logger,
    IMemoryCache memoryCache) : IGithubRepository
{
    private static Dictionary<string, LanguageMetadata>? LanguageMetadatas { get; set; }

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(2);

    /// <summary>
    /// Initializes the repository by fetching language metadata from GitHub Linguist.
    /// This method is called once to set up the necessary data for other operations.
    /// <strong>GitHub API Token Cost:</strong> 0 tokens (Uses raw.githubusercontent.com, which does not count against the API rate limit).
    /// </summary>
    public async Task InitializeAsync()
    {
        if (LanguageMetadatas is not null) return;

        logger.LogInformation("Initializing GithubRepository: Fetching linguist languages metadata...");
        try
        {
            const string url = "https://raw.githubusercontent.com/github-linguist/linguist/master/lib/linguist/languages.yml";

            var httpClient = httpClientFactory.CreateClient(IGithubRepository.HttpGithubClientName);
            var yamlContent = await httpClient.GetStringAsync(url);

            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            LanguageMetadatas = deserializer.Deserialize<Dictionary<string, LanguageMetadata>>(yamlContent);
            logger.LogInformation("GithubRepository initialized successfully with {Count} languages mapped", LanguageMetadatas.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize language metadata from GitHub Linguist");
        }
    }

    /// <summary>
    /// Retrieves all GitHub repositories.
    /// <strong>GitHub API Token Cost:</strong> 1 token per execution (0 if cache hit).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of GitHub repository domains or an error if the operation fails.</returns>
    public async Task<Result<IEnumerable<GithubRepositoryDomain>>> GetAllGithubRepository(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = "github_all_repositories";

        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<GithubRepositoryDomain>? cachedRepos) && cachedRepos is not null)
        {
            logger.LogInformation("Cache HIT for key '{CacheKey}'. Returning cached repositories", cacheKey);
            return Result<IEnumerable<GithubRepositoryDomain>>.Success(cachedRepos);
        }

        logger.LogInformation("Cache MISS for key '{CacheKey}'. Fetching repositories from GitHub API...", cacheKey);
        var blackListNames = new List<string> { "winget-pkgs", "TheR7angelo.github.io", "TheR7angelo" };
        var httpClient = httpClientFactory.CreateClient(IGithubRepository.HttpGithubClientName);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/users/TheR7angelo/repos");
            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Forbidden) // Erreur 403 Rate Limit
            {
                if (!response.Headers.TryGetValues("x-ratelimit-reset", out var values))
                {
                    return Result<IEnumerable<GithubRepositoryDomain>>.Failure(ErrorCode.Http,
                        "GitHub API quota exceeded or denied access (403)");
                }

                var rawUnixTime = values.FirstOrDefault();
                if (!long.TryParse(rawUnixTime, out var unixSeconds))
                {
                    return Result<IEnumerable<GithubRepositoryDomain>>.Failure(ErrorCode.GithubRateLimited,
                        "GitHub API quota exceeded or denied access (403)");
                }

                var resetDateTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
                var msgRateLimit = $"GitHub API quota exceeded. Scheduled reset to {resetDateTime.ToShortTimeString()}";

                logger.LogWarning("{MsgRateLimit}", msgRateLimit);
                return Result<IEnumerable<GithubRepositoryDomain>>.Failure(ErrorCode.GithubRateLimited, msgRateLimit);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("GitHub API returned non-success status: {StatusCode} ({Reason})", response.StatusCode, response.ReasonPhrase);
                return Result<IEnumerable<GithubRepositoryDomain>>.Failure(ErrorCode.Http, response.ReasonPhrase ?? string.Empty);
            }

            var repositories = await response.Content.ReadFromJsonAsync<List<Entities.GithubRepository>>(cancellationToken);
            var repositoriesDomain = repositories!.Where(s => !blackListNames.Contains(s.Name))
                .Select(githubEntitiesToGithubDomain.MapToDomain)
                .ToList();

            memoryCache.Set(cacheKey, repositoriesDomain, CacheDuration);
            logger.LogInformation("Successfully fetched and cached {Count} repositories from GitHub", repositoriesDomain.Count);

            return Result<IEnumerable<GithubRepositoryDomain>>.Success(repositoriesDomain);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while fetching repositories");
            return Result<IEnumerable<GithubRepositoryDomain>>.Failure(ErrorCode.Http, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves detailed information for a list of GitHub repositories.
    /// <strong>GitHub API Token Cost:</strong> 3 * N tokens where N is the number of processed repositories (0 for individual assets if cache hit).
    /// </summary>
    /// <param name="domains">A collection of GitHub repository domains.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A result containing either a collection of GitHub repository information domains or an error.</returns>
    public async Task<Result<IEnumerable<GithubRepositoryInformationDomain>>> GetAllGithubRepositoryInformation(
        IEnumerable<GithubRepositoryDomain> domains,
        CancellationToken cancellationToken = default)
    {
        var repositoryDomains = domains.ToList();
        logger.LogInformation("Starting parallel processing for {Count} repositories...", repositoryDomains.Count);

        try
        {
            var results = new ConcurrentBag<GithubRepositoryInformationDomain>();

            var tasks = repositoryDomains.Select(async domain =>
            {
                var owner = domain.OwnerDomain!.Login;
                var repo = domain.Name;

                logger.LogDebug("[{Repo}] Generating stat badges...", repo);
                var statsBadges = await CreateStatsBadges(
                    domain.Stars,
                    domain.WatchersCount,
                    domain.ForksCount,
                    domain.PushedAt,
                    domain.Size,
                    domain.OpenIssuesCount,
                    domain.LicenseDomain?.SpdxId);

                logger.LogDebug("[{Repo}] Dispatching tasks for languages, custom logo, and README...", repo);
                var createLanguageBadgesTask = CreateLanguageBadges(owner, repo, cancellationToken);
                var getCustomLogoUrlAsyncTask = GetCustomLogoUrlAsync(owner, repo, domain.DefaultBranch, cancellationToken);
                var getReadmeHtmlAsyncTask = GetReadmeHtmlAsync(owner, repo, domain.DefaultBranch, cancellationToken);

                await Task.WhenAll(createLanguageBadgesTask, getCustomLogoUrlAsyncTask, getReadmeHtmlAsyncTask);

                results.Add(new GithubRepositoryInformationDomain
                {
                    RepositoryUrl = domain.HtmlUrl,
                    Name = repo,
                    Description = domain.Description,
                    LogoUrl = await getCustomLogoUrlAsyncTask,
                    ReadmeHtml = await getReadmeHtmlAsyncTask,
                    LanguagesBadges = await createLanguageBadgesTask,
                    StatsBadges = statsBadges
                });
                logger.LogInformation("[{Repo}] Successfully aggregated all metrics", repo);
            });

            await Task.WhenAll(tasks);
            logger.LogInformation("Parallel aggregation completed for all repositories");

            return Result<IEnumerable<GithubRepositoryInformationDomain>>.Success(results.ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while fetching detailed repository information");
            return Result<IEnumerable<GithubRepositoryInformationDomain>>.Failure(ErrorCode.Http, e.Message);
        }
    }

    /// <summary>
    /// Retrieves the HTML content of a repository's README file from GitHub.
    /// <strong>GitHub API Token Cost:</strong> 1 token per repository (0 if cache hit).
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="defaultBranch">The default branch of the repository.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The result is a string containing the HTML content of the README file, or null if an error occurs.</returns>
    private async Task<string?> GetReadmeHtmlAsync(string owner, string repoName, string defaultBranch,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"readme_{owner}_{repoName}";

        if (memoryCache.TryGetValue(cacheKey, out string? cachedReadme))
        {
            logger.LogDebug("[{Repo}] Cache HIT for README", repoName);
            return cachedReadme;
        }

        logger.LogDebug("[{Repo}] Cache MISS for README. Querying GitHub API...", repoName);
        try
        {
            var httpClient = httpClientFactory.CreateClient(IGithubRepository.HttpGithubClientName);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/repos/{owner}/{repoName}/readme");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.html"));

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.NotFound)
            {
                logger.LogWarning("[{Repo}] No README file found (404)", repoName);
                memoryCache.Set<string?>(cacheKey, null, CacheDuration);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("[{Repo}] Failed to fetch README. HTTP status: {StatusCode}", repoName, response.StatusCode);
                return null;
            }

            var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var rawBaseUrl = $"https://raw.githubusercontent.com/{owner}/{repoName}/{defaultBranch}/";
            var blobBaseUrl = $"https://github.com/{owner}/{repoName}/blob/{defaultBranch}/";

            htmlContent = Regex.Replace(htmlContent,
                @"<img\s+[^>]*src=[""'](?!(?:https?://|/))([^""']+)[""']",
                match => match.Value.Replace(match.Groups[1].Value, rawBaseUrl + match.Groups[1].Value));

            htmlContent = Regex.Replace(htmlContent,
                @"<a\s+[^>]*href=[""'](?!(?:https?://|/|#))([^""']+)[""']",
                match => match.Value.Replace(match.Groups[1].Value, blobBaseUrl + match.Groups[1].Value));

            memoryCache.Set(cacheKey, htmlContent, CacheDuration);
            logger.LogDebug("[{Repo}] README parsed, paths fixed, and stored in cache", repoName);
            return htmlContent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Repo}] Exception thrown during GetReadmeHtmlAsync", repoName);
            return null;
        }
    }

    /// <summary>
    /// Retrieves the URL of a custom logo for a specified GitHub repository.
    /// <strong>GitHub API Token Cost:</strong> 1 token per repository (0 if cache hit).
    /// </summary>
    /// <param name="owner">The owner of the GitHub repository.</param>
    /// <param name="repoName">The name of the GitHub repository.</param>
    /// <param name="defaultBranch">The default branch of the repository.</param>
    /// <param name="cancellationToken">A cancellation token to allow for asynchronous operation cancellation.</param>
    /// <returns>A task that represents the asynchronous operation and returns a string representing the URL of the custom logo.</returns>
    private async Task<string> GetCustomLogoUrlAsync(string owner, string repoName, string defaultBranch,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"logo_{owner}_{repoName}";

        if (memoryCache.TryGetValue(cacheKey, out string? cachedLogo) && cachedLogo is not null)
        {
            logger.LogDebug("[{Repo}] Cache HIT for custom logo", repoName);
            return cachedLogo;
        }

        var defaultLogo = $"https://github.com/{owner}.png";
        logger.LogDebug("[{Repo}] Cache MISS for custom logo. Fetching repository tree...", repoName);

        try
        {
            var httpClient = httpClientFactory.CreateClient(IGithubRepository.HttpGithubClientName);

            var response = await httpClient.GetAsync($"/repos/{owner}/{repoName}/git/trees/{defaultBranch}?recursive=1", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("[{Repo}] Failed to fetch tree. Status: {StatusCode}. Falling back to default logo", repoName, response.StatusCode);
                return defaultLogo;
            }

            var result = await response.Content.ReadFromJsonAsync<GitHubTreeResponse>(cancellationToken);
            if (result?.Tree is null || result.Tree.Count == 0)
            {
                logger.LogDebug("[{Repo}] Empty file tree returned. Using default logo", repoName);
                memoryCache.Set(cacheKey, defaultLogo, CacheDuration);
                return defaultLogo;
            }

            var targetFile = result.Tree
                .Where(item =>
                    item.Path.Contains(".github/assets/", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.Contains(".github/images/", StringComparison.OrdinalIgnoreCase))
                .Select(item => new {
                    item.Path,
                    FileName = Path.GetFileName(item.Path)
                })
                .OrderBy(f => GetLogoPriority(f.FileName))
                .FirstOrDefault(f => IsMatchingLogoName(f.FileName));

            var finalLogo = targetFile is not null
                ? $"https://raw.githubusercontent.com/{owner}/{repoName}/{defaultBranch}/{targetFile.Path}"
                : defaultLogo;

            memoryCache.Set(cacheKey, finalLogo, CacheDuration);
            logger.LogDebug("[{Repo}] Logo resolve strategy finished. Selection: {LogoUrl}", repoName, finalLogo);
            return finalLogo;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Repo}] Error resolving custom logo. Falling back to default", repoName);
            return defaultLogo;
        }
    }

    private static bool IsMatchingLogoName(string fileName)
    {
        return fileName.Equals("logo.svg", StringComparison.OrdinalIgnoreCase) ||
               fileName.Equals("icon.svg", StringComparison.OrdinalIgnoreCase) ||
               fileName.Equals("logo.png", StringComparison.OrdinalIgnoreCase) ||
               fileName.Equals("icon.png", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetLogoPriority(string fileName)
    {
        if (fileName.Equals("logo.svg", StringComparison.OrdinalIgnoreCase)) return 1;
        if (fileName.Equals("icon.svg", StringComparison.OrdinalIgnoreCase)) return 2;
        if (fileName.Equals("logo.png", StringComparison.OrdinalIgnoreCase)) return 3;
        return fileName.Equals("icon.png", StringComparison.OrdinalIgnoreCase) ? 4 : 99;
    }

    /// <summary>
    /// Creates a list of badges for repository statistics.
    /// <strong>GitHub API Token Cost:</strong> 0 tokens (Calculated entirely on client-side using metadata).
    /// </summary>
    /// <param name="stargazersCount">Number of stargazers on the repository.</param>
    /// <param name="subscribersCount">Number of subscribers to the repository.</param>
    /// <param name="forksCount">Number of forks of the repository.</param>
    /// <param name="pushedAt">The last commit date of the repository.</param>
    /// <param name="repoSizeCount">Size of the repository in bytes.</param>
    /// <param name="openIssuesCount">Number of open issues in the repository.</param>
    /// <param name="licenceName">License under which the repository is published.</param>
    /// <returns>A list of URLs for repository statistics badges.</returns>
private Task<List<GithubBadgeDomain>> CreateStatsBadges(int stargazersCount, int subscribersCount, int forksCount,
    DateTime pushedAt, int repoSizeCount, int openIssuesCount, string? licenceName)
{
    var starsUrl = $"https://img.shields.io/badge/Stars-{stargazersCount}-gold?style=flat&logo=github-sponsors&logoColor=white";
    var starsAlt = $"GitHub Stars: {stargazersCount}";

    var watchersUrl = $"https://img.shields.io/badge/Watchers-{subscribersCount}-4183C4?style=flat&logo=visibility&logoColor=white";
    var watchersAlt = $"Watchers: {subscribersCount}";

    var forksUrl = $"https://img.shields.io/badge/Forks-{forksCount}-586069?style=flat&logo=git-fork&logoColor=white";
    var forksAlt = $"Forks: {forksCount}";

    var dateText = pushedAt.ToString("dd MMM yyyy");
    var encodedDate = Uri.EscapeDataString(dateText);
    var activityUrl = $"https://img.shields.io/badge/Activity-{encodedDate}-brightgreen?style=flat&logo=git-commit&logoColor=white";
    var activityAlt = $"Last activity: {dateText}";

    var sizeInMb = (double)repoSizeCount / 1024;
    var sizeText = sizeInMb < 1 ? $"{repoSizeCount} KB" : $"{sizeInMb:F2} MB";
    var sizeUrl = $"https://img.shields.io/badge/Size-{Uri.EscapeDataString(sizeText)}-blueviolet?style=flat&logo=files&logoColor=white";
    var sizeAlt = $"Repository size: {sizeText}";

    var issueColor = openIssuesCount is 0 ? "brightgreen" : "orange";
    var issuesUrl = $"https://img.shields.io/badge/Issues-{openIssuesCount}-{issueColor}?style=flat&logo=github-actions&logoColor=white";
    var issuesAlt = $"Open issues: {openIssuesCount}";

    var licenseText = licenceName ?? "None";
    var licenseUrl = $"https://img.shields.io/badge/Licence-{Uri.EscapeDataString(licenseText)}-blue?style=flat&logo=scale&logoColor=white";
    var licenseAlt = $"License: {licenseText}";

    var badges = new List<GithubBadgeDomain>
    {
        new() { Url = starsUrl, AltText = starsAlt},
        new() { Url = watchersUrl, AltText = watchersAlt},
        new() { Url = forksUrl, AltText = forksAlt},
        new() { Url = activityUrl, AltText = activityAlt},
        new() { Url = sizeUrl, AltText = sizeAlt},
        new() { Url = issuesUrl, AltText = issuesAlt},
        new() { Url = licenseUrl, AltText = licenseAlt}
    };

    return Task.FromResult(badges);
}

    /// <summary>
    /// Asynchronously creates language badges for a specified repository.
    /// <strong>GitHub API Token Cost:</strong> 1 token per repository (0 if cache hit).
    /// </summary>
    /// <param name="ownerName">The owner of the GitHub repository.</param>
    /// <param name="repoName">The name of the GitHub repository.</param>
    /// <param name="cancellationToken">A token to allow cancellation of the task.</param>
    /// <returns>A task that represents the asynchronous operation and returns an enumerable GithubBadgeDomain collection containing language badges.</returns>
    private async Task<IEnumerable<GithubBadgeDomain>> CreateLanguageBadges(string ownerName, string repoName, CancellationToken cancellationToken)
    {
        var cacheKey = $"languages_{ownerName}_{repoName}";

        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<GithubBadgeDomain>? cachedLanguages) && cachedLanguages is not null)
        {
            logger.LogDebug("[{Repo}] Cache HIT for language badges", repoName);
            return cachedLanguages;
        }

        logger.LogDebug("[{Repo}] Cache MISS for language badges. Fetching language data...", repoName);
        var httpClient = httpClientFactory.CreateClient(IGithubRepository.HttpGithubClientName);

        try
        {
            var languagesResponse = await httpClient.GetAsync($"/repos/{ownerName}/{repoName}/languages", cancellationToken);

            if (!languagesResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("[{Repo}] Failed to fetch languages. Status: {StatusCode}", repoName, languagesResponse.StatusCode);
                return [];
            }

            var languages = await languagesResponse.Content.ReadFromJsonAsync<Dictionary<string, long>>(cancellationToken);
            if (languages is not { Count: > 0 })
            {
                logger.LogDebug("[{Repo}] No languages returned for this repository", repoName);
                return [];
            }

            var totalBytes = languages.Values.Sum();
            var badges = new List<GithubBadgeDomain>();

            foreach (var (languageName, bytes) in languages)
            {
                if (LanguageMetadatas is null || !LanguageMetadatas.TryGetValue(languageName, out var metadata))
                {
                    logger.LogDebug("[{Repo}] Language metadata missing for target: {Language}", repoName, languageName);
                    continue;
                }

                var percentage = Math.Round((double)bytes / totalBytes * 100, 2);
                var percentageText = bytes > 0 && percentage is 0 ? "<0.01" : percentage.ToString("F2");

                var urlBadge = $"https://img.shields.io/badge/{Uri.EscapeDataString(languageName)}-{Uri.EscapeDataString(percentageText)}-{metadata.CleanColor}?style=flat&logo={metadata.AceMode}&logoColor=white";
                var altText = $"{languageName} ({percentageText}%)";

                badges.Add(new GithubBadgeDomain {Url = urlBadge, AltText = altText});
            }

            memoryCache.Set(cacheKey, badges, CacheDuration);
            logger.LogDebug("[{Repo}] Generated {Count} language badges and saved to cache", repoName, badges.Count);
            return badges;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Repo}] Error occurred during CreateLanguageBadges", repoName);
            return [];
        }
    }
}