using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class AddedFutherInfoToArgument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Argument",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Argument",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Argument");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Argument");
        }
    }
}
