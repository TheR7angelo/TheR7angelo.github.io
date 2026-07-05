using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheR7angelo.github.io.Components;

public partial class DatabaseSection
{
    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Inject] private ILogger<DatabaseSection> Logger { get; set; } = null!;

    private List<DataBaseTechnology> DataBaseTechnologies { get; } = [];

    protected override void OnInitialized()
    {
        base.OnInitialized();

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
        // TODO trad
        var qGisProject = new ProjectDescription("🗺️ Système d'Information Géographique (SIG)",
            """
            🚀 Architecture & Optimisation : Modélisation d'une base spatiale (PostGIS) et indexation de requêtes géospatiales complexes.
            ⚙️ Automatisation & ETL : Écriture de fonctions stockées pour le traitement des géométries et scripts d'import/export multi-formats.
            🔐 Sécurité & Restitution : Administration fine des droits d'accès et intégration des flux de données dans QGIS et tableaux de bord.
            """,
            4);

        var erpProject = new ProjectDescription("🏢 ERP d'Entreprise — SIREO RCC",
            """
            📐 Modélisation : Conception d'une architecture relationnelle hautement normalisée et robuste.
            🔌 Pipeline .NET Core : Développement d'une API REST pour exposer les données à l'application web interne.
            📊 Reporting : Création de vues optimisées pour la génération de rapports financiers et opérationnels.
            """,
            3);

        return [qGisProject, erpProject];
    }

    private IEnumerable<ProjectDescription> FillSqlServerProjects()
    {
        // TODO trad
        var erpProject = new ProjectDescription("🏢 Infrastructure & Logique Métier ERP",
            """
            🏎️ Performance & Concurrence : Indexation, tuning de requêtes lourdes et gestion des transactions multi-utilisateurs.
            🧠 Règles Métiers : Centralisation de la logique applicative complexe via des procédures stockées (T-SQL).
            🛡️ Ops & Data : Déploiement de stratégies de sauvegarde (PRA), haute disponibilité et alimentation de flux BI en temps réel.
            """,
            5);

        return [erpProject];
    }

    private IEnumerable<ProjectDescription> FillSqLiteProjects()
    {
        // TODO trad
        var financeProject = new ProjectDescription("💰 Gestionnaire de Finances Personneles (Open Source)",
            """
            🚀 Architecture Évolutive : Refonte complète de l'application en MVVM pour migrer d'une interface WPF vers une solution multiplateforme .NET MAUI / Blazor.
            📊 Moteur de Données & Reporting : Modélisation locale sous SQLite pour le suivi des transactions, la gestion analytique des flux (entrées/sorties) et des récurrences (abonnements, charges fixes).
            🗺️ Module Géospatiale & BI : Intégration d'un système pour cartographier les dépenses et générer des rapports avancés par enseigne, type de paiement (carte, virement, chèque) et volume financier.
            """,
            3);

        // TODO trad
        var owfProject = new ProjectDescription("⚡ Outils d'Audit & Cohérence Data — OWF Control (Orange)",
            """
            🎯 Pipeline de Centralisation (ETL) : Extraction et consolidation massive de données hétérogènes (fichiers Excel complexes, calculs de charge télécom, Shapefiles SIG) vers une base locale SQLite unifiée.
            🧠 Moteur de Règles & Validation : Conception de requêtes relationnelles avancées et contrôles spatiaux pour pointer automatiquement les incohérences (validations INSEE, absence de coordonnées GPS, écarts techniques).
            📈 Impact Business : Génération automatisée de rapports d'erreurs visuels pour les projeteurs, divisant le temps de traitement des dossiers finaux de raccordement fibre par un facteur de 4 à 5 heures.
            """,
            5);

        return [financeProject, owfProject];
    }

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