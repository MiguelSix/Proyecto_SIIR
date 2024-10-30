using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Document",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Document");
        }
    }
}
