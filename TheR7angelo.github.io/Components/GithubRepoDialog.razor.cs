using Microsoft.AspNetCore.Components;
using MudBlazor;
using TheR7angelo.github.io.Service.Models.GitHub;

namespace TheR7angelo.github.io.Components;

public partial class GithubRepoDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public GithubRepositoryInformationDto Project { get; set; } = null!;

    private void Close()
        => MudDialog.Close();
}