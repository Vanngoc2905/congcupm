using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vnfood.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeFieldsToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl2",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl3",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl4",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrepTime",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Servings",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl2",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ImageUrl3",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ImageUrl4",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PrepTime",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Servings",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Posts");
        }
    }
}
