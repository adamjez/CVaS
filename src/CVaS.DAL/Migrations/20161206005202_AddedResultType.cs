using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using CVaS.DAL.Model;

namespace CVaS.DAL.Migrations
{
    public partial class AddedResultType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "Run",
                nullable: false,
                defaultValue: RunResultType.Success);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "Run");
        }
    }
}
