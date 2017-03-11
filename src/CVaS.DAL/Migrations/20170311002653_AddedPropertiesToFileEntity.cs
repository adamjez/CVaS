using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CVaS.DAL.Migrations
{
    public partial class AddedPropertiesToFileEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Files",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Files",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Hash",
                table: "Files",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Files");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");
        }
    }
}
