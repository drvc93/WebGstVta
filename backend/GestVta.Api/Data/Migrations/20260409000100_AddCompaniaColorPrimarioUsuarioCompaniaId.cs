using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestVta.Api.Data.Migrations;

/// <summary>
/// Alineado con sql-server/04_AlterCompaniaColorUsuarioCompania.sql (tablas dbo.Compania y dbo.Usuario).
/// </summary>
public partial class AddCompaniaColorPrimarioUsuarioCompaniaId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.Compania', N'ColorPrimario') IS NULL
                ALTER TABLE dbo.Compania ADD ColorPrimario NVARCHAR(7) NULL;

            IF COL_LENGTH(N'dbo.Usuario', N'CompaniaId') IS NULL
                ALTER TABLE dbo.Usuario ADD CompaniaId INT NULL;

            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Usuario_Compania')
                ALTER TABLE dbo.Usuario ADD CONSTRAINT FK_Usuario_Compania FOREIGN KEY (CompaniaId) REFERENCES dbo.Compania (Id);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Usuario_Compania')
                ALTER TABLE dbo.Usuario DROP CONSTRAINT FK_Usuario_Compania;

            IF COL_LENGTH(N'dbo.Usuario', N'CompaniaId') IS NOT NULL
                ALTER TABLE dbo.Usuario DROP COLUMN CompaniaId;

            IF COL_LENGTH(N'dbo.Compania', N'ColorPrimario') IS NOT NULL
                ALTER TABLE dbo.Compania DROP COLUMN ColorPrimario;
            """);
    }
}
