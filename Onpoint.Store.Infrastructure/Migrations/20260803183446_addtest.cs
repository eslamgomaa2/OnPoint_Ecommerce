using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onpoint.Store.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addtest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppLogoUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "AppName",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "AppStoreUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "FacebookUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "GooglePlayUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "SnapchatUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "TiktokUrl",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "WhatsappNumber",
                table: "StoreSettings");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "StoreSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "StoreSettings");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "StoreSettings");

            migrationBuilder.AddColumn<string>(
                name: "AppLogoUrl",
                table: "StoreSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppName",
                table: "StoreSettings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AppStoreUrl",
                table: "StoreSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StoreSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StoreSettings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookUrl",
                table: "StoreSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GooglePlayUrl",
                table: "StoreSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "StoreSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StoreSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StoreSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "StoreSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "StoreSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapchatUrl",
                table: "StoreSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TiktokUrl",
                table: "StoreSettings",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StoreSettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappNumber",
                table: "StoreSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
