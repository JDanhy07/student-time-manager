using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timely.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioYCorreoUnico : Migration
    {

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            //Red de seguridad: si ya existen duplicados, la migracion se detiene con un mensaje claro en lugar de fallar al crear el indice.
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM Usuarios GROUP BY Usuario HAVING COUNT(*) > 1)
                THROW 50000, 'Existen nombres de usuario duplicados. Depuralos antes de aplicar esta migracion.', 1;");

            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM Usuarios GROUP BY Correo HAVING COUNT(*) > 1)
                THROW 50000, 'Existen correos duplicados. Depuralos antes de aplicar esta migracion.', 1;");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Usuario",
                table: "Usuarios",
                column: "Usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Usuario",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
