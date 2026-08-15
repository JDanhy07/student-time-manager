using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timely.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEstadoPropiedad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Proyectos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Proyectos");
        }
    }
}
