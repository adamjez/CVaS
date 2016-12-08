using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class ChangedApiKeyConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "AspNetUsers",
                maxLength: 44,
                nullable: false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_ApiKey",
                table: "AspNetUsers",
                column: "ApiKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_ApiKey",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "AspNetUsers",
                maxLength: 30,
                nullable: true);
        }
    }
}
