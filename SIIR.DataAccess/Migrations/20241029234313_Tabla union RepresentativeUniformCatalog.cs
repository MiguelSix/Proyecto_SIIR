using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TablaunionRepresentativeUniformCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepresentativeUniformCatalogs",
                columns: table => new
                {
                    RepresentativeId = table.Column<int>(type: "int", nullable: false),
                    UniformCatalogId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepresentativeUniformCatalogs", x => new { x.RepresentativeId, x.UniformCatalogId });
                    table.ForeignKey(
                        name: "FK_RepresentativeUniformCatalogs_Representatives_RepresentativeId",
                        column: x => x.RepresentativeId,
                        principalTable: "Representatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepresentativeUniformCatalogs_UniformCatalog_UniformCatalogId",
                        column: x => x.UniformCatalogId,
                        principalTable: "UniformCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepresentativeUniformCatalogs_UniformCatalogId",
                table: "RepresentativeUniformCatalogs",
                column: "UniformCatalogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepresentativeUniformCatalogs");
        }
    }
}
