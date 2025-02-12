using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEditorChoiceToArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EditorChoice",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditorChoice",
                table: "Articles");
        }
    }
}
