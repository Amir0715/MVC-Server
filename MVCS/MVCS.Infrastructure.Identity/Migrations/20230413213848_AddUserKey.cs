using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCS.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddUserKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "AspNetUsers");
        }
    }
}
