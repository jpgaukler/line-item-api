using System.IO;
using System.Reflection;
using FluentMigrator;

namespace LineItem.Migrations.Helpers;

public static class MigrationHelper
{
    public static string GetScriptFolder<TMigration>(
        this TMigration migration,
        string scriptFolderName
    )
        where TMigration : Migration
    {
        var migrationType = typeof(TMigration);
        var attr = migrationType.GetCustomAttribute<MigrationAttribute>();

        return Path.Combine(
            Path.GetDirectoryName(typeof(TMigration).Assembly.Location)!,
            "Migrations",
            $"{attr!.Version}_{migrationType.Name}",
            scriptFolderName
        );
    }

    public static string GetUpScript<TMigration>(this TMigration migration, string scriptName)
        where TMigration : Migration
    {
        return Path.Combine(migration.GetScriptFolder("Up"), scriptName);
    }

    public static string GetDownScript<TMigration>(this TMigration migration, string scriptName)
        where TMigration : Migration
    {
        return Path.Combine(migration.GetScriptFolder("Down"), scriptName);
    }
}
