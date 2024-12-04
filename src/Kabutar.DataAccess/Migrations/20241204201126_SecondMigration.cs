using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kabutar.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password_salt",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password_salt",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
