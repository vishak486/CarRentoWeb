using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnedAtToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnedAt",
                table: "Orders");
        }
    }
}
