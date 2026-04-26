using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kabutar.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureThumbnail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profile_picture_thumbnail",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_picture_thumbnail",
                table: "users");
        }
    }
}
