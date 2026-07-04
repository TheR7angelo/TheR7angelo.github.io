using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheR7angelo.github.io.Components;

public partial class DatabaseSection
{
    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Inject] private ILogger<DatabaseSection> Logger { get; set; } = null!;

    private List<DataBaseTechnology> DataBaseTechnologies { get; } =
    [
        // TODO trad
        new(DataBaseTechnologyEnum.SqlServer, "SQL Server", "/Assets/DataBase/sql-server.svg",
            "https://learn.microsoft.com/en-us/sql/sql-server/",
            [
                new ProjectDescription("🏢 ERP d'Entreprise",
                    "- Optimisation de requêtes pour des rapports financiers complexes\n" +
                    "- Gestion des transactions multi-utilisateurs avec isolation des données\n" +
                    "- Implémentation de procédures stockées pour automatiser les tâches récurrentes\n" +
                    "- Surveillance et tuning des performances pour des bases de données volumineuses\n" +
                    "- Mise en place de sauvegardes et de stratégies de récupération après sinistre\n" +
                    "- Intégration avec des outils de BI pour l'analyse des données en temps réel")
            ]),
        // TODO make
        new(DataBaseTechnologyEnum.PostgreSql, "PostgreSQL", "/Assets/DataBase/postgresql.svg",
            "https://www.postgresql.org/", null
            // [
            //     new ProjectDescription("🛒 Plateforme E-Commerce",
            //         "Mise en place d'une base de données relationnelle robuste avec gestion fine des transactions et des stocks en temps réel.")
            // ]
            ),

        // TODO make
        new(DataBaseTechnologyEnum.SqLite, "SQLite", "/Assets/DataBase/sqlite.svg",
            "https://www.sqlite.org/", null
            // [
            //     new ProjectDescription("📱 Application Mobile MAUI",
            //         "Stockage local embarqué et synchronisation hors-ligne des données utilisateurs.")
            // ]
            )
    ];

    private Task NavigateToDatabase(DataBaseTechnology tech)
    {
        Logger.LogInformation("Opening project dialog for {DbName}...", tech.Name);

        var parameters = new DialogParameters
        {
            { nameof(DatabaseProjectDialog.TechName), tech.Name },
            { nameof(DatabaseProjectDialog.Projects), tech.Projects }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };

        return DialogService.ShowAsync<DatabaseProjectDialog>($"Détails {tech.Name}", parameters, options);
    }
}

public record DataBaseTechnology(
    DataBaseTechnologyEnum Id,
    string Name,
    string LogoPath,
    string LearnMoreUrl,
    List<ProjectDescription>? Projects);

public record ProjectDescription(string IconAndTitle, string Description);

public enum DataBaseTechnologyEnum
{
    SqlServer,
    PostgreSql,
    SqLite
}