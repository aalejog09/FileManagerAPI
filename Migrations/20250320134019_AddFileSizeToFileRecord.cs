using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSizeToFileRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Files",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Files");
        }
    }
}
