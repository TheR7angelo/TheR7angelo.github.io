using MudBlazor;
using TheR7angelo.github.io.Resources.Resx.DatabaseProject;

namespace TheR7angelo.github.io.Components;

public partial class DatabaseSection(ILogger<DatabaseSection> logger, IDialogService dialogService)
{
    private List<DataBaseTechnology> DataBaseTechnologies { get; } = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        FillProjets();
    }

    private void FillProjets()
    {
        DataBaseTechnologies.Clear();

        var sqlServerProjects = FillSqlServerProjects().OrderByDescending(p => p.Importance);
        var postgreSqlProjects = FillPostgreSqlProjects().OrderByDescending(p => p.Importance);
        var sqLiteProjects = FillSqLiteProjects().OrderByDescending(p => p.Importance);

        DataBaseTechnologies.AddRange([
            new DataBaseTechnology(DataBaseTechnologyEnum.SqlServer, "SQL Server", "/Assets/DataBase/sql-server.svg",
                "https://learn.microsoft.com/en-us/sql/sql-server/", sqlServerProjects.ToList()),

            new DataBaseTechnology(DataBaseTechnologyEnum.PostgreSql, "PostgreSQL", "/Assets/DataBase/postgresql.svg",
                "https://www.postgresql.org/", postgreSqlProjects.ToList()),

            new DataBaseTechnology(DataBaseTechnologyEnum.SqLite, "SQLite", "/Assets/DataBase/sqlite.svg",
                "https://www.sqlite.org/", sqLiteProjects.ToList())
        ]);
    }

    private IEnumerable<ProjectDescription> FillPostgreSqlProjects()
    {
        var qGisProject = new ProjectDescription(DatabaseProjectResources.PostgreSqlProjectQgisHeader,
            DatabaseProjectResources.PostgreSqlProjectQgisDescription, 4);

        var erpProject = new ProjectDescription(DatabaseProjectResources.PostgreSqlProjectSireoRCCHeader,
            DatabaseProjectResources.PostgreSqlProjectSireoRCCDescription, 3);

        return [qGisProject, erpProject];
    }

    private IEnumerable<ProjectDescription> FillSqlServerProjects()
    {
        var erpProject = new ProjectDescription(DatabaseProjectResources.SqlServerProjectErpExploitationHeader,
            DatabaseProjectResources.SqlServerProjectErpExploitationDescription, 5);

        return [erpProject];
    }

    private IEnumerable<ProjectDescription> FillSqLiteProjects()
    {
        var financeProject = new ProjectDescription(DatabaseProjectResources.SqliteProjectMyExpenseHeader,
            DatabaseProjectResources.SqliteProjectMyExpenseDescription, 3);

        var owfProject = new ProjectDescription(DatabaseProjectResources.SqliteProjectControlOwfHeader,
            DatabaseProjectResources.SqliteProjectControlOwfDescription, 5);

        return [financeProject, owfProject];
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