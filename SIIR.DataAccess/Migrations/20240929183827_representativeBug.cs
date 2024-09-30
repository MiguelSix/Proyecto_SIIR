using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class representativeBug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativesId",
                table: "UniformCatalog");

            migrationBuilder.RenameColumn(
                name: "RepresentativesId",
                table: "UniformCatalog",
                newName: "RepresentativeId");

            migrationBuilder.RenameIndex(
                name: "IX_UniformCatalog_RepresentativesId",
                table: "UniformCatalog",
                newName: "IX_UniformCatalog_RepresentativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativeId",
                table: "UniformCatalog",
                column: "RepresentativeId",
                principalTable: "Representatives",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativeId",
                table: "UniformCatalog");

            migrationBuilder.RenameColumn(
                name: "RepresentativeId",
                table: "UniformCatalog",
                newName: "RepresentativesId");

            migrationBuilder.RenameIndex(
                name: "IX_UniformCatalog_RepresentativeId",
                table: "UniformCatalog",
                newName: "IX_UniformCatalog_RepresentativesId");

            migrationBuilder.AddForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativesId",
                table: "UniformCatalog",
                column: "RepresentativesId",
                principalTable: "Representatives",
                principalColumn: "Id");
        }
    }
}
