using Microsoft.AspNetCore.Components;

namespace TheR7angelo.github.io.Components;

public partial class DatabaseSection
{
    [Inject]
    private ILogger<DatabaseSection> Logger { get; set; } = null!;

    private List<DataBaseTechnology> DataBaseTechnologies { get; } =
    [
        new("sql-server", "SQL Server", "/Assets/DataBase/sql-server.svg",
            "https://learn.microsoft.com/en-us/sql/sql-server/"),
        new("postgresql", "PostgreSQL", "/Assets/DataBase/postgresql.svg", "https://www.postgresql.org/"),
        new("sqlite", "SQLite", "/Assets/DataBase/sqlite.svg", "https://www.sqlite.org/")
    ];

    private Task NavigateToDatabase(string dataBase)
    {
        Logger.LogInformation("Navigating to {Mysql} database page...", dataBase);

        return Task.CompletedTask;
    }
}

public record DataBaseTechnology(string Id, string Name, string LogoPath, string LearnMoreUrl);