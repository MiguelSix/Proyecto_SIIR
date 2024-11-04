using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Numeroenstudentenlugardeuniforme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "number",
                table: "Uniform");

            migrationBuilder.AddColumn<int>(
                name: "numberUniform",
                table: "Students",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "numberUniform",
                table: "Students");

            migrationBuilder.AddColumn<int>(
                name: "number",
                table: "Uniform",
                type: "int",
                nullable: true);
        }
    }
}
