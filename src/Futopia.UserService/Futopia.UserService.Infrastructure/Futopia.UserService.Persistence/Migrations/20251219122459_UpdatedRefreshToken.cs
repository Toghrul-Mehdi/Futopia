using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Futopia.UserService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserRefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserRefreshTokens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserRefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserRefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "UserRefreshTokens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserRefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserRefreshTokens");
        }
    }
}
