using TheR7angelo.github.io.Service.Interface.Services;

namespace TheR7angelo.github.io.Pages;

public partial class Home(IGithubService githubService, ILogger<Home> logger)
{
    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        _ = Task.Run(async () =>
        {
            var result = await githubService.GetAllGithubRepository();
            logger.LogInformation(@"{Result}", result);
        });
    }
}