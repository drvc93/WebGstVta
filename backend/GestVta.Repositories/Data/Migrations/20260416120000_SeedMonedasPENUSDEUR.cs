using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestVta.Api.Data.Migrations;

/// <summary>
/// Datos base: Sol (PEN), Dólar (USD), Euro (EUR). Idempotente por código ISO.
/// </summary>
public partial class SeedMonedasPENUSDEUR : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.Moneda WHERE Codigo = N'PEN')
                INSERT INTO dbo.Moneda (Codigo, Simbolo, Nombre, Activo) VALUES (N'PEN', N'S/', N'Sol peruano', 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.Moneda WHERE Codigo = N'USD')
                INSERT INTO dbo.Moneda (Codigo, Simbolo, Nombre, Activo) VALUES (N'USD', N'US$', N'Dólar estadounidense', 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.Moneda WHERE Codigo = N'EUR')
                INSERT INTO dbo.Moneda (Codigo, Simbolo, Nombre, Activo) VALUES (N'EUR', N'€', N'Euro', 1);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DECLARE @ids TABLE (Id INT);
            INSERT INTO @ids (Id)
            SELECT Id FROM dbo.Moneda WHERE Codigo IN (N'PEN', N'USD', N'EUR');

            DELETE FROM dbo.TipoCambio WHERE MonedaId IN (SELECT Id FROM @ids);
            DELETE FROM dbo.Moneda WHERE Id IN (SELECT Id FROM @ids);
            """);
    }
}
