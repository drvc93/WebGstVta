using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestVta.Api.Data.Migrations;

/// <inheritdoc />
public partial class RemoveMenuOpcionCompaniaExport : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM dbo.RolMenuPermiso WHERE MenuOpcionId IN (SELECT Id FROM dbo.MenuOpcion WHERE Codigo = N'OP_COMPANIA_EXPORT');
            DELETE FROM dbo.MenuOpcion WHERE Codigo = N'OP_COMPANIA_EXPORT';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_COMPANIA_EXPORT')
            INSERT INTO dbo.MenuOpcion (Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (N'OP_COMPANIA_EXPORT', N'Exportar listado', N'/maestros/compania/export', N'bi-download', (SELECT Id FROM dbo.MenuOpcion WHERE Codigo = N'OP_COMPANIA'), 1, 1);
            DECLARE @exportId INT = (SELECT Id FROM dbo.MenuOpcion WHERE Codigo = N'OP_COMPANIA_EXPORT');
            DECLARE @admin INT = (SELECT TOP 1 Id FROM dbo.Rol WHERE Codigo = N'ADMIN');
            IF @exportId IS NOT NULL AND @admin IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RolMenuPermiso WHERE RolId = @admin AND MenuOpcionId = @exportId)
            INSERT INTO dbo.RolMenuPermiso (RolId, MenuOpcionId, PuedeLeer, PuedeEscribir, PuedeModificar, PuedeEliminar)
            VALUES (@admin, @exportId, 1, 1, 1, 1);
            DECLARE @op INT = (SELECT TOP 1 Id FROM dbo.Rol WHERE Codigo = N'OPERADOR');
            IF @exportId IS NOT NULL AND @op IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RolMenuPermiso WHERE RolId = @op AND MenuOpcionId = @exportId)
            INSERT INTO dbo.RolMenuPermiso (RolId, MenuOpcionId, PuedeLeer, PuedeEscribir, PuedeModificar, PuedeEliminar)
            VALUES (@op, @exportId, 1, 1, 1, 0);
            """);
    }
}
