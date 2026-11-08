using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gatherly.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventManagementAndItemConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemClaims_ItemId",
                table: "ItemClaims");

            migrationBuilder.DropIndex(
                name: "IX_EventMembers_EventId",
                table: "EventMembers");

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "Items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "ManagementType",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemClaims_ItemId_IsActive",
                table: "ItemClaims",
                columns: new[] { "ItemId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EventMembers_EventId_UserId",
                table: "EventMembers",
                columns: new[] { "EventId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemClaims_ItemId_IsActive",
                table: "ItemClaims");

            migrationBuilder.DropIndex(
                name: "IX_EventMembers_EventId_UserId",
                table: "EventMembers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ManagementType",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_ItemClaims_ItemId",
                table: "ItemClaims",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EventMembers_EventId",
                table: "EventMembers",
                column: "EventId");
        }
    }
}
