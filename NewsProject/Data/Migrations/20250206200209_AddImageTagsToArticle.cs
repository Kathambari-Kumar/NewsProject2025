using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageTagsToArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArticleId",
                table: "ImageTags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_ArticleId",
                table: "ImageTags",
                column: "ArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageTags_Articles_ArticleId",
                table: "ImageTags",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageTags_Articles_ArticleId",
                table: "ImageTags");

            migrationBuilder.DropIndex(
                name: "IX_ImageTags_ArticleId",
                table: "ImageTags");

            migrationBuilder.DropColumn(
                name: "ArticleId",
                table: "ImageTags");
        }
    }
}
