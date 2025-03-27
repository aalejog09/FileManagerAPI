using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateTypeSupportedFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MaxSizeKB",
                table: "SupportedFiles",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MaxSizeKB",
                table: "SupportedFiles",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
