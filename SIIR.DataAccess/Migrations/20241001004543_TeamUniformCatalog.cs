using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TeamUniformCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativeId",
                table: "UniformCatalog");

            migrationBuilder.DropIndex(
                name: "IX_UniformCatalog_RepresentativeId",
                table: "UniformCatalog");

            migrationBuilder.DropColumn(
                name: "HasNumber",
                table: "UniformCatalog");

            migrationBuilder.DropColumn(
                name: "RepresentativeId",
                table: "UniformCatalog");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UniformCatalog",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UniformCatalog",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeamUniformCatalog");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "UniformCatalog");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UniformCatalog",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "HasNumber",
                table: "UniformCatalog",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepresentativeId",
                table: "UniformCatalog",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniformCatalog_RepresentativeId",
                table: "UniformCatalog",
                column: "RepresentativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UniformCatalog_Representatives_RepresentativeId",
                table: "UniformCatalog",
                column: "RepresentativeId",
                principalTable: "Representatives",
                principalColumn: "Id");
        }
    }
}
