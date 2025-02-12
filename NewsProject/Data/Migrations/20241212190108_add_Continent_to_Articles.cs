using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_Continent_to_Articles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Continent",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Continent",
                table: "Articles");
        }
    }
}
