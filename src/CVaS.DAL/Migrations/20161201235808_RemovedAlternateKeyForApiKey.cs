using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class RemovedAlternateKeyForApiKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_ApiKey",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApiKey",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ApiKey",
                table: "AspNetUsers",
                nullable: false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_ApiKey",
                table: "AspNetUsers",
                column: "ApiKey");
        }
    }
}
