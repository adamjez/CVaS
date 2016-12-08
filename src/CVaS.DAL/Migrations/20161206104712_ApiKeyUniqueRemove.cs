using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class ApiKeyUniqueRemove : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_ApiKey",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_ApiKey",
                table: "AspNetUsers",
                column: "ApiKey");
        }
    }
}
