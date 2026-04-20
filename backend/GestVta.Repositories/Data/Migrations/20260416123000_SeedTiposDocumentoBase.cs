using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestVta.Api.Data.Migrations;

/// <summary>
/// Datos base para tipo de documento: RUC, DNI, CE. Idempotente por código.
/// </summary>
public partial class SeedTiposDocumentoBase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.TipoDocumento WHERE Codigo = N'RUC')
                INSERT INTO dbo.TipoDocumento (Codigo, Nombre, Activo) VALUES (N'RUC', N'RUC', 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.TipoDocumento WHERE Codigo = N'DNI')
                INSERT INTO dbo.TipoDocumento (Codigo, Nombre, Activo) VALUES (N'DNI', N'DNI', 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.TipoDocumento WHERE Codigo = N'CE')
                INSERT INTO dbo.TipoDocumento (Codigo, Nombre, Activo) VALUES (N'CE', N'Carné extranjería', 1);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM dbo.TipoDocumento WHERE Codigo IN (N'RUC', N'DNI', N'CE');
            """);
    }
}
