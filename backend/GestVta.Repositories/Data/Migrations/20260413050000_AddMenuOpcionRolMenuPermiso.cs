using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestVta.Api.Data.Migrations;

/// <inheritdoc />
public partial class AddMenuOpcionRolMenuPermiso : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MenuOpcion",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Codigo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Ruta = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                Icono = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                ParentId = table.Column<int>(type: "int", nullable: true),
                Orden = table.Column<int>(type: "int", nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MenuOpcion", x => x.Id);
                table.ForeignKey(
                    name: "FK_MenuOpcion_MenuOpcion_ParentId",
                    column: x => x.ParentId,
                    principalTable: "MenuOpcion",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_MenuOpcion_Codigo",
            table: "MenuOpcion",
            column: "Codigo",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MenuOpcion_ParentId",
            table: "MenuOpcion",
            column: "ParentId");

        migrationBuilder.CreateTable(
            name: "RolMenuPermiso",
            columns: table => new
            {
                RolId = table.Column<int>(type: "int", nullable: false),
                MenuOpcionId = table.Column<int>(type: "int", nullable: false),
                PuedeLeer = table.Column<bool>(type: "bit", nullable: false),
                PuedeEscribir = table.Column<bool>(type: "bit", nullable: false),
                PuedeModificar = table.Column<bool>(type: "bit", nullable: false),
                PuedeEliminar = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolMenuPermiso", x => new { x.RolId, x.MenuOpcionId });
                table.ForeignKey(
                    name: "FK_RolMenuPermiso_MenuOpcion_MenuOpcionId",
                    column: x => x.MenuOpcionId,
                    principalTable: "MenuOpcion",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolMenuPermiso_Rol_RolId",
                    column: x => x.RolId,
                    principalTable: "Rol",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RolMenuPermiso_MenuOpcionId",
            table: "RolMenuPermiso",
            column: "MenuOpcionId");

        migrationBuilder.Sql(
            """
            SET IDENTITY_INSERT dbo.MenuOpcion ON;

            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'SEC_MAESTROS')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (1, N'SEC_MAESTROS', N'Maestros', NULL, N'bi-database', NULL, 1, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'SEC_ENTIDADES')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (2, N'SEC_ENTIDADES', N'Entidades', NULL, N'bi-people', NULL, 2, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'SEC_PRODUCTO')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (3, N'SEC_PRODUCTO', N'Producto', NULL, N'bi-box-seam', NULL, 3, 1);

            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_COMPANIA')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (10, N'OP_COMPANIA', N'Compañía', N'/maestros/compania', N'bi-building', 1, 10, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_USUARIOS')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (12, N'OP_USUARIOS', N'Usuarios', N'/maestros/usuarios', N'bi-person-badge', 1, 20, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_MENU_OPCIONES')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (13, N'OP_MENU_OPCIONES', N'Opciones de menú', N'/maestros/menu-opciones', N'bi-list-nested', 1, 25, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_ROL_PERMISOS')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (14, N'OP_ROL_PERMISOS', N'Permisos por rol', N'/maestros/rol-permisos', N'bi-shield-lock', 1, 26, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_PROCESO')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (20, N'OP_PROCESO', N'Proceso', N'/maestros/proceso', N'bi-diagram-3', 1, 30, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_ADICIONALES')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (21, N'OP_ADICIONALES', N'Adicionales', N'/maestros/adicionales', N'bi-plus-square', 1, 40, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_RPTA_SEG')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (22, N'OP_RPTA_SEG', N'Rpta. seguimiento', N'/maestros/rpta-seguimiento', N'bi-chat-left-text', 1, 50, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_FORMA_PAGO')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (23, N'OP_FORMA_PAGO', N'Forma pago', N'/maestros/forma-pago', N'bi-credit-card-2-front', 1, 60, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_MONEDA')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (24, N'OP_MONEDA', N'Moneda', N'/maestros/moneda', N'bi-currency-dollar', 1, 70, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_TIPO_DOC')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (25, N'OP_TIPO_DOC', N'Tipo documento', N'/maestros/tipo-documento', N'bi-file-earmark-text', 1, 80, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_TIPO_CAMBIO')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (26, N'OP_TIPO_CAMBIO', N'Tipo cambio', N'/maestros/tipo-cambio', N'bi-arrow-left-right', 1, 90, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_SEGMENTO')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (27, N'OP_SEGMENTO', N'Segmento', N'/maestros/segmento', N'bi-diagram-2', 1, 100, 1);

            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_CLIENTE')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (30, N'OP_CLIENTE', N'Reg. cliente', N'/entidades/cliente', N'bi-person-vcard', 2, 10, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_AGENCIA')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (31, N'OP_AGENCIA', N'Agencia transp.', N'/entidades/agencia-transporte', N'bi-truck', 2, 20, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_GRUPO_CLI')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (32, N'OP_GRUPO_CLI', N'Grupo cliente', N'/entidades/grupo-cliente', N'bi-people', 2, 30, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_CONDUCTOR')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (33, N'OP_CONDUCTOR', N'Conductor', N'/entidades/conductor', N'bi-person-badge', 2, 40, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_PROVEEDOR')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (34, N'OP_PROVEEDOR', N'Proveedor', N'/entidades/proveedor', N'bi-shop', 2, 50, 1);

            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_ITEMS')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (40, N'OP_ITEMS', N'Items', N'/producto/items', N'bi-boxes', 3, 10, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_ITEMS_EXPORT')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (41, N'OP_ITEMS_EXPORT', N'Exportar ítems', N'/producto/items/export', N'bi-file-earmark-spreadsheet', 40, 1, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_UNIDAD')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (42, N'OP_UNIDAD', N'Unidad', N'/producto/unidad', N'bi-rulers', 3, 20, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_FAMILIA')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (43, N'OP_FAMILIA', N'Familia', N'/producto/familia', N'bi-collection', 3, 30, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_MARCA')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (44, N'OP_MARCA', N'Marca', N'/producto/marca', N'bi-award', 3, 40, 1);
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuOpcion WHERE Codigo = N'OP_MODELO')
            INSERT INTO dbo.MenuOpcion (Id, Codigo, Nombre, Ruta, Icono, ParentId, Orden, Activo)
            VALUES (45, N'OP_MODELO', N'Modelo', N'/producto/modelo', N'bi-grid-1x2', 3, 50, 1);

            SET IDENTITY_INSERT dbo.MenuOpcion OFF;

            /* Permisos: ADMIN = todo; OPERADOR = lectura en mayoría; VENDEDOR = lectura en producto y clientes */
            DECLARE @admin INT = (SELECT TOP 1 Id FROM dbo.Rol WHERE Codigo = N'ADMIN');
            DECLARE @op INT = (SELECT TOP 1 Id FROM dbo.Rol WHERE Codigo = N'OPERADOR');
            DECLARE @ven INT = (SELECT TOP 1 Id FROM dbo.Rol WHERE Codigo = N'VENDEDOR');

            IF @admin IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RolMenuPermiso WHERE RolId = @admin)
            INSERT INTO dbo.RolMenuPermiso (RolId, MenuOpcionId, PuedeLeer, PuedeEscribir, PuedeModificar, PuedeEliminar)
            SELECT @admin, m.Id, 1, 1, 1, 1 FROM dbo.MenuOpcion m;

            IF @op IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RolMenuPermiso WHERE RolId = @op)
            INSERT INTO dbo.RolMenuPermiso (RolId, MenuOpcionId, PuedeLeer, PuedeEscribir, PuedeModificar, PuedeEliminar)
            SELECT @op, m.Id, 1, 1, 1, 0 FROM dbo.MenuOpcion m;

            IF @ven IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.RolMenuPermiso WHERE RolId = @ven)
            INSERT INTO dbo.RolMenuPermiso (RolId, MenuOpcionId, PuedeLeer, PuedeEscribir, PuedeModificar, PuedeEliminar)
            SELECT @ven, m.Id,
                CASE WHEN m.Id IN (2,3,30,31,32,33,34,40,41,42,43,44,45) OR m.ParentId IN (2,3) THEN 1 ELSE 0 END,
                0, 0, 0
            FROM dbo.MenuOpcion m;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RolMenuPermiso");
        migrationBuilder.DropTable(name: "MenuOpcion");
    }
}
