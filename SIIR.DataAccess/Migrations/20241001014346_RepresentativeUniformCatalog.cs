using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RepresentativeUniformCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamUniformCatalog");

            migrationBuilder.CreateTable(
                name: "RepresentativeUniformCatalog",
                columns: table => new
                {
                    RepresentativeId = table.Column<int>(type: "int", nullable: false),
                    UniformCatalogsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepresentativeUniformCatalog", x => new { x.RepresentativeId, x.UniformCatalogsId });
                    table.ForeignKey(
                        name: "FK_RepresentativeUniformCatalog_Representatives_RepresentativeId",
                        column: x => x.RepresentativeId,
                        principalTable: "Representatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepresentativeUniformCatalog_UniformCatalog_UniformCatalogsId",
                        column: x => x.UniformCatalogsId,
                        principalTable: "UniformCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepresentativeUniformCatalog_UniformCatalogsId",
                table: "RepresentativeUniformCatalog",
                column: "UniformCatalogsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepresentativeUniformCatalog");

            migrationBuilder.CreateTable(
                name: "TeamUniformCatalog",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    UniformCatalogsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamUniformCatalog", x => new { x.TeamId, x.UniformCatalogsId });
                    table.ForeignKey(
                        name: "FK_TeamUniformCatalog_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamUniformCatalog_UniformCatalog_UniformCatalogsId",
                        column: x => x.UniformCatalogsId,
                        principalTable: "UniformCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamUniformCatalog_UniformCatalogsId",
                table: "TeamUniformCatalog",
                column: "UniformCatalogsId");
        }
    }
}
