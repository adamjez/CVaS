using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class RefactoringFileEntityAndAddingFileSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Files_Path",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Files",
                newName: "LocationId");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Files",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Files_LocationId",
                table: "Files",
                column: "LocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Files_LocationId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "Files",
                newName: "Path");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Files_Path",
                table: "Files",
                column: "Path");
        }
    }
}
