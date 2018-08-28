using Microsoft.EntityFrameworkCore.Migrations;

namespace Taxi.Migrations
{
    public partial class refundfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityId",
                table: "RefundRequests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "RefundRequests");
        }
    }
}
