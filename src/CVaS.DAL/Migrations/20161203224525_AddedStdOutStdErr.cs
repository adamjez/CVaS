using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class AddedStdOutStdErr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StdErr",
                table: "Run",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StdOut",
                table: "Run",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StdErr",
                table: "Run");

            migrationBuilder.DropColumn(
                name: "StdOut",
                table: "Run");
        }
    }
}
