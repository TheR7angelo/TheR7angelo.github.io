using Microsoft.JSInterop;
using MudBlazor;
using TheR7angelo.github.io.Components;
using TheR7angelo.github.io.Resources.Resx.HomePage;

namespace TheR7angelo.github.io.Layout;

public partial class MainLayout(IJSRuntime jsRuntime)
{
    private bool _isDarkMode = true;
    private bool _isNavigationDrawerOpen;
    private MudTheme? _theme;
    private bool _isThemeInitialized;

    public static readonly List<AnchorSection> Sections =
    [
        new() {
            Title = HomePageResources.DatabasesSectionTitle,
            Icon = Icons.Material.Filled.Storage,
            ComponentType = typeof(DatabaseSection)
        },
        new() {
            Title = HomePageResources.GithubRepoSectionTitle,
            Icon = Icons.Custom.Brands.GitHub,
            ComponentType = typeof(GithubRepoSection)
        },
        new()
        {
            Title = HomePageResources.GithubStatTitle,
            Icon = Icons.Material.Filled.Leaderboard,
            ComponentType = typeof(GithubStatsSection)
        },
        new() {
            Title = HomePageResources.MapWorkingAreaSectionTitle,
            Icon = Icons.Material.Filled.Map,
            ComponentType = typeof(MapWorkingAreaSection)
        }
    ];

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _theme = new MudTheme
        {
            PaletteLight = _lightPalette,
            PaletteDark = _darkPalette,
            LayoutProperties = new LayoutProperties()
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _isThemeInitialized)
        {
            return;
        }

        var preferredTheme = await jsRuntime.InvokeAsync<string>("ThemeHelper.getPreferredTheme");
        _isDarkMode = preferredTheme == "dark";
        _isThemeInitialized = true;
        await jsRuntime.InvokeVoidAsync("ThemeHelper.setTheme", preferredTheme);
        StateHasChanged();
    }

    private async Task DarkModeToggle()
    {
        _isDarkMode = !_isDarkMode;
        await jsRuntime.InvokeVoidAsync("ThemeHelper.setTheme", _isDarkMode ? "dark" : "light");
    }

    private void ToggleNavigationDrawer()
        => _isNavigationDrawerOpen = !_isNavigationDrawerOpen;

    private readonly PaletteLight _lightPalette = new()
    {
        Black = "#110e2d",
        AppbarText = "#424242",
        AppbarBackground = "rgba(255,255,255,0.8)",
        DrawerBackground = "#ffffff",
        GrayLight = "#e8e8e8",
        GrayLighter = "#f9f9f9"
    };

    private readonly PaletteDark _darkPalette = new()
    {
        Primary = "#7e6fff",
        Surface = "#1e1e2d",
        Background = "#1a1a27",
        BackgroundGray = "#151521",
        AppbarText = "#92929f",
        AppbarBackground = "rgba(26,26,39,0.8)",
        DrawerBackground = "#1a1a27",
        ActionDefault = "#74718e",
        ActionDisabled = "#9999994d",
        ActionDisabledBackground = "#605f6d4d",
        TextPrimary = "#b2b0bf",
        TextSecondary = "#92929f",
        TextDisabled = "#ffffff33",
        DrawerIcon = "#92929f",
        DrawerText = "#92929f",
        GrayLight = "#2a2833",
        GrayLighter = "#1e1e2d",
        Info = "#4a86ff",
        Success = "#3dcb6c",
        Warning = "#ffb545",
        Error = "#ff3f5f",
        LinesDefault = "#33323e",
        TableLines = "#33323e",
        Divider = "#292838",
        OverlayLight = "#1e1e2d80"
    };

    private string DarkLightModeButtonIcon => _isDarkMode switch
    {
        true => Icons.Material.Rounded.LightMode,
        false => Icons.Material.Outlined.DarkMode
    };

    private async Task ScrollToSection(string elementId)
    {
        await jsRuntime.InvokeVoidAsync("ScrollHelper.scrollToElement", elementId);
    }
}
