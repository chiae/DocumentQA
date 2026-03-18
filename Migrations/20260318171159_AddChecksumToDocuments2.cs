using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocumentQA.Migrations
{
    /// <inheritdoc />
    public partial class AddChecksumToDocuments2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Checksum",
                table: "Documents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Checksum",
                table: "Documents");
        }
    }
}
