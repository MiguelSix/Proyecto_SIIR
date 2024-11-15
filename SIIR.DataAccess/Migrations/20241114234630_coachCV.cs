using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class coachCV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVUrl",
                table: "Coaches",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVUrl",
                table: "Coaches");
        }
    }
}
