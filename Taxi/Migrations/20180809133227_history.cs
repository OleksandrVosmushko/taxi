using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Taxi.Migrations
{
    public partial class history : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TripHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false),
                    DriverId = table.Column<Guid>(nullable: false),
                    DriverTakeTripTime = table.Column<DateTime>(nullable: false),
                    FinishTime = table.Column<DateTime>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripHistories_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripHistories_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinishTripPlaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsFrom = table.Column<bool>(nullable: false),
                    IsTo = table.Column<bool>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    TripHistoryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishTripPlaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinishTripPlaces_TripHistories_TripHistoryId",
                        column: x => x.TripHistoryId,
                        principalTable: "TripHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinishTripPlaces_TripHistoryId",
                table: "FinishTripPlaces",
                column: "TripHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TripHistories_CustomerId",
                table: "TripHistories",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TripHistories_DriverId",
                table: "TripHistories",
                column: "DriverId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinishTripPlaces");

            migrationBuilder.DropTable(
                name: "TripHistories");
        }
    }
}
