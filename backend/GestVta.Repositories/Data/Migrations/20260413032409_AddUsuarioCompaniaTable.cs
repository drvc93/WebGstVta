using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestVta.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioCompaniaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuarioCompania",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    CompaniaId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioCompania", x => new { x.UsuarioId, x.CompaniaId });
                    table.ForeignKey(
                        name: "FK_UsuarioCompania_Compania_CompaniaId",
                        column: x => x.CompaniaId,
                        principalTable: "Compania",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioCompania_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioCompania_CompaniaId",
                table: "UsuarioCompania",
                column: "CompaniaId");

            migrationBuilder.Sql(
                """
                INSERT INTO UsuarioCompania (UsuarioId, CompaniaId)
                SELECT u.Id, u.CompaniaId FROM Usuario u
                WHERE u.CompaniaId IS NOT NULL
                AND NOT EXISTS (
                  SELECT 1 FROM UsuarioCompania uc
                  WHERE uc.UsuarioId = u.Id AND uc.CompaniaId = u.CompaniaId);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UsuarioCompania");
        }
    }
}
