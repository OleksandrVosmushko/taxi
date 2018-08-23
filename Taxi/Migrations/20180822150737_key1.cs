using Microsoft.EntityFrameworkCore.Migrations;

namespace Taxi.Migrations
{
    public partial class key1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
