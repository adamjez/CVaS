using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class RefactoredFileHandling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "Run");

            migrationBuilder.AddColumn<int>(
                name: "FileId",
                table: "Run",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Run_FileId",
                table: "Run",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Run_Files_FileId",
                table: "Run",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Run_Files_FileId",
                table: "Run");

            migrationBuilder.DropIndex(
                name: "IX_Run_FileId",
                table: "Run");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Run");

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Run",
                maxLength: 256,
                nullable: true);
        }
    }
}
