using System.Collections.ObjectModel;
using TheR7angelo.github.io.Service.Interface.Services;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Pages;

public partial class Home(IGithubService githubService, ILogger<Home> logger)
{
    private ObservableCollection<GithubRepositoryInformationDto> GithubRepositoryInformationDtos { get; } = [];
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await GetAllGithubRepository();
    }

    private async Task GetAllGithubRepository()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            var resultGithubRepositories = await githubService.GetAllGithubRepository();
            if (!resultGithubRepositories.IsSuccess)
            {
                logger.LogError("Failed to retrieve GitHub repositories");
                return;
            }

            var result = await githubService.GetAllGithubRepositoryInformation(resultGithubRepositories.Value!);
            if (result.IsSuccess)
            {
                GithubRepositoryInformationDtos.Clear();
                foreach (var githubRepositoryInformationDto in result.Value!)
                {
                    GithubRepositoryInformationDtos.Add(githubRepositoryInformationDto);
                }
            }
        }
        finally
        {
            IsLoading = false;

            StateHasChanged();
        }
    }
}