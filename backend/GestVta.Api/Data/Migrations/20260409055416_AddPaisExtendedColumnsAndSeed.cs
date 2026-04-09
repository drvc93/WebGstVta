using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestVta.Api.Data.Migrations;

/// <summary>
/// Columnas extendidas (si faltan) y MERGE de países; equivalente idempotente a sql-server/06_SeedPaisesMerge.sql.
/// </summary>
public partial class AddPaisExtendedColumnsAndSeed : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(LoadEmbeddedSql("PaisAlterColumns.sql"));
        migrationBuilder.Sql(LoadEmbeddedSql("PaisesMerge.sql"));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }

    private static string LoadEmbeddedSql(string fileName)
    {
        var assembly = typeof(AddPaisExtendedColumnsAndSeed).Assembly;
        var resourceName = $"GestVta.Api.Data.Migrations.Sql.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"No se encontró el recurso incrustado '{resourceName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
