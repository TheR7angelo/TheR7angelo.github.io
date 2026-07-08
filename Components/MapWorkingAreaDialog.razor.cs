using Mapsui;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheR7angelo.github.io.Components;

public partial class MapWorkingAreaDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public IFeature Feature { get; set; } = null!;

    private void Close() => MudDialog.Close();
}