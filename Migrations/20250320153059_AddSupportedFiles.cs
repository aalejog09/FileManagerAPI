using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportedFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupportedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaxSizeKB = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportedFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportedFiles_Extension",
                table: "SupportedFiles",
                column: "Extension",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportedFiles");
        }
    }
}
