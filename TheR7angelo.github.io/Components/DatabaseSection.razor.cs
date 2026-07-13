using System.Globalization;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheR7angelo.github.io.Components;

public partial class DatabaseSection(ILogger<DatabaseSection> logger, IDialogService dialogService,
    IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
{
    private List<DataBaseTechnology> DataBaseTechnologies { get; } = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await FillProjets();
    }

    private async Task FillProjets()
    {
        var currentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        var httpClient = httpClientFactory.CreateClient();

        var skeletons = await httpClient.GetFromJsonAsync<List<DataBaseTechSkeleton>>($"{navigationManager.BaseUri}Data/databases_project.json");
        var langData = await httpClient.GetFromJsonAsync<Dictionary<string, List<ProjectDescription>>>($"{navigationManager.BaseUri}Data/databases_project_{currentCulture}.json");

        if (skeletons is not null)
        {
            DataBaseTechnologies.Clear();

            foreach (var skeleton in skeletons)
            {
                var techIdStr = ((int)skeleton.TechType).ToString();
                var projectsForThisTech = new List<ProjectDescription>();

                if (langData is not null && langData.TryGetValue(techIdStr, out var projects))
                {
                    projectsForThisTech = projects.OrderByDescending(p => p.Importance).ToList();
                }

                DataBaseTechnologies.Add(new DataBaseTechnology(
                    skeleton.TechType,
                    skeleton.Name,
                    skeleton.LogoPath,
                    skeleton.LearnMoreUrl,
                    projectsForThisTech
                ));
            }
        }
    }

    private Task<IDialogReference> NavigateToDatabase(DataBaseTechnology tech)
    {
        logger.LogInformation("Opening project dialog for {DbName}...", tech.Name);

        var parameters = new DialogParameters
        {
            { nameof(DatabaseProjectDialog.TechName), tech.Name },
            { nameof(DatabaseProjectDialog.Projects), tech.Projects }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        return dialogService.ShowAsync<DatabaseProjectDialog>($"Détails {tech.Name}", parameters, options);
    }
}

public record DataBaseTechSkeleton(
    DataBaseTechnologyEnum TechType,
    string Name,
    string LogoPath,
    string LearnMoreUrl);

public record DataBaseTechnology(
    DataBaseTechnologyEnum TechType,
    string Name,
    string LogoPath,
    string LearnMoreUrl,
    List<ProjectDescription>? Projects);

public record ProjectDescription(string IconAndTitle, string Description, int Importance = 0);

public enum DataBaseTechnologyEnum
{
    SqlServer,
    PostgreSql,
    SqLite
}