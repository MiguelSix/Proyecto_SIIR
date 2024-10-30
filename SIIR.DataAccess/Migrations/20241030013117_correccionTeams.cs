using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class correccionTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Students_StudentId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_StudentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Teams");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_StudentId",
                table: "Teams",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Students_StudentId",
                table: "Teams",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
