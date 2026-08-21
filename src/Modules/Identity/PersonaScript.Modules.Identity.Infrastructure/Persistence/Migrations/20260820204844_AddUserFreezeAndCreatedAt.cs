using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFreezeAndCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "identity",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "FreezeReason",
                schema: "identity",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FrozenAt",
                schema: "identity",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrozen",
                schema: "identity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FreezeReason",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FrozenAt",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsFrozen",
                schema: "identity",
                table: "Users");
        }
    }
}
