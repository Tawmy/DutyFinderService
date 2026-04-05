using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DutyFinderService.Db.Migrations
{
    /// <inheritdoc />
    public partial class LastUpdatedPatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "last_updated_patch",
                table: "images",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_updated_patch",
                table: "images");
        }
    }
}
