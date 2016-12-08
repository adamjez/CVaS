using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class ChangedApiKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "AspNetUsers",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ApiKey",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
