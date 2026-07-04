using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheR7angelo.github.io.Components;

public partial class DatabaseProjectDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public DataBaseTechnologyEnum TechId { get; set; }

    [Parameter]
    public string TechName { get; set; } = string.Empty;

    [Parameter]
    public List<ProjectDescription> Projects { get; set; } = [];

    private void Close() => MudDialog.Close();
}