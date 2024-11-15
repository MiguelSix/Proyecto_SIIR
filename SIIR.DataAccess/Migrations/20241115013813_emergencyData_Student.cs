using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIIR.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class emergencyData_Student : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Weight",
                table: "Students",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Height",
                table: "Students",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "emergencyContact",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "emergencyPhone",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emergencyContact",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "emergencyPhone",
                table: "Students");

            migrationBuilder.AlterColumn<float>(
                name: "Weight",
                table: "Students",
                type: "real",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Height",
                table: "Students",
                type: "real",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);
        }
    }
}
