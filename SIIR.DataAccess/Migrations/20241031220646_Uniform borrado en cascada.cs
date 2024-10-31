using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Uniformborradoencascada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Uniform_Representatives_RepresentativeId",
                table: "Uniform");

            migrationBuilder.DropForeignKey(
                name: "FK_Uniform_UniformCatalog_UniformCatalogId",
                table: "Uniform");

            migrationBuilder.DropIndex(
                name: "IX_Uniform_RepresentativeId",
                table: "Uniform");

            migrationBuilder.DropIndex(
                name: "IX_Uniform_UniformCatalogId",
                table: "Uniform");

            migrationBuilder.CreateIndex(
                name: "IX_Uniform_RepresentativeId_UniformCatalogId",
                table: "Uniform",
                columns: new[] { "RepresentativeId", "UniformCatalogId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Uniform_RepresentativeUniformCatalogs_RepresentativeId_UniformCatalogId",
                table: "Uniform",
                columns: new[] { "RepresentativeId", "UniformCatalogId" },
                principalTable: "RepresentativeUniformCatalogs",
                principalColumns: new[] { "RepresentativeId", "UniformCatalogId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Uniform_RepresentativeUniformCatalogs_RepresentativeId_UniformCatalogId",
                table: "Uniform");

            migrationBuilder.DropIndex(
                name: "IX_Uniform_RepresentativeId_UniformCatalogId",
                table: "Uniform");

            migrationBuilder.CreateIndex(
                name: "IX_Uniform_RepresentativeId",
                table: "Uniform",
                column: "RepresentativeId");

            migrationBuilder.CreateIndex(
                name: "IX_Uniform_UniformCatalogId",
                table: "Uniform",
                column: "UniformCatalogId");

            migrationBuilder.AddForeignKey(
                name: "FK_Uniform_Representatives_RepresentativeId",
                table: "Uniform",
                column: "RepresentativeId",
                principalTable: "Representatives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Uniform_UniformCatalog_UniformCatalogId",
                table: "Uniform",
                column: "UniformCatalogId",
                principalTable: "UniformCatalog",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
