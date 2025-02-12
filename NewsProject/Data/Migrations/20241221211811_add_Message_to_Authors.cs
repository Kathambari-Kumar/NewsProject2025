using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_Message_to_Authors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Authors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Message",
                table: "Authors");
        }
    }
}
