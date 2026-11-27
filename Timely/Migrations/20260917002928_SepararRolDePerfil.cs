using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timely.Migrations
{
    /// <inheritdoc />
    public partial class SepararRolDePerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Perfil",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            //Se agrega el Rol como NULLABLE para poder actualizar los registros existentes sin problemas. Luego se actualizan los registros existentes y finalmente se cambia a NOT NULL con valor por defecto.
            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            //Migra el dato
            migrationBuilder.Sql("UPDATE Usuarios SET Rol = Perfil");
            
            //Red de seguridad por si algun registro queda Null.
            migrationBuilder.Sql("UPDATE Usuarios SET Rol = 'Estudiante' WHERE Rol IS NULL OR Rol = ''");

            //Finalmente se cambia la columna a NOT NULL con valor por defecto.
            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Estudiante",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Perfil",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
