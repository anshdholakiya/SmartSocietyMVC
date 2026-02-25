using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSocietyMVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amenities",
                table: "Societies");

            migrationBuilder.DropColumn(
                name: "Gallery",
                table: "Societies");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Societies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Societies");

            migrationBuilder.AddColumn<string>(
                name: "Amenities",
                table: "Societies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gallery",
                table: "Societies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
