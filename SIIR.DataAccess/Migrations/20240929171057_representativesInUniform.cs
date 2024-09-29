using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class representativesInUniform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepresentativesId",
                table: "UniformCatalog",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniformCatalog_RepresentativesId",
                table: "UniformCatalog",
                column: "RepresentativesId");

            migrationBuilder.AddForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativesId",
                table: "UniformCatalog",
                column: "RepresentativesId",
                principalTable: "Representatives",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativesId",
                table: "UniformCatalog");

            migrationBuilder.DropIndex(
                name: "IX_UniformCatalog_RepresentativesId",
                table: "UniformCatalog");

            migrationBuilder.DropColumn(
                name: "RepresentativesId",
                table: "UniformCatalog");
        }
    }
}
