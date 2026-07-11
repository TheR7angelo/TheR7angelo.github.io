using TheR7angelo.github.io.Service.Interface.Services;

namespace TheR7angelo.github.io.Pages;

public partial class Home(IGithubService githubService, ILogger<Home> logger)
{
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        _ = GetAllGithubRepository();
    }

    private async Task GetAllGithubRepository()
    {
        var resultGithubRepositories = await githubService.GetAllGithubRepository();
        if (!resultGithubRepositories.IsSuccess)
        {
            logger.LogError("Failed to retrieve GitHub repositories");
            return;
        }

        var result = await githubService.GetAllGithubRepositoryInformation(resultGithubRepositories.Value!);

        Console.WriteLine(result.Value?.Count());
    }
}