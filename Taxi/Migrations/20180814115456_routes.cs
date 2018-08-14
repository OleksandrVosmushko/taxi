using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Taxi.Migrations
{
    public partial class routes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripRouteNode_Trips_TripId",
                table: "TripRouteNode");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripRouteNode",
                table: "TripRouteNode");

            migrationBuilder.RenameTable(
                name: "TripRouteNode",
                newName: "TripRouteNodes");

            migrationBuilder.RenameIndex(
                name: "IX_TripRouteNode_TripId",
                table: "TripRouteNodes",
                newName: "IX_TripRouteNodes_TripId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TripId",
                table: "TripRouteNodes",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "TripHistoryId",
                table: "TripRouteNodes",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripRouteNodes",
                table: "TripRouteNodes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TripRouteNodes_TripHistoryId",
                table: "TripRouteNodes",
                column: "TripHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TripRouteNodes_TripHistories_TripHistoryId",
                table: "TripRouteNodes",
                column: "TripHistoryId",
                principalTable: "TripHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TripRouteNodes_Trips_TripId",
                table: "TripRouteNodes",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TripRouteNodes_TripHistories_TripHistoryId",
                table: "TripRouteNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_TripRouteNodes_Trips_TripId",
                table: "TripRouteNodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TripRouteNodes",
                table: "TripRouteNodes");

            migrationBuilder.DropIndex(
                name: "IX_TripRouteNodes_TripHistoryId",
                table: "TripRouteNodes");

            migrationBuilder.DropColumn(
                name: "TripHistoryId",
                table: "TripRouteNodes");

            migrationBuilder.RenameTable(
                name: "TripRouteNodes",
                newName: "TripRouteNode");

            migrationBuilder.RenameIndex(
                name: "IX_TripRouteNodes_TripId",
                table: "TripRouteNode",
                newName: "IX_TripRouteNode_TripId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TripId",
                table: "TripRouteNode",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TripRouteNode",
                table: "TripRouteNode",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TripRouteNode_Trips_TripId",
                table: "TripRouteNode",
                column: "TripId",
                principalTable: "Trips",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
