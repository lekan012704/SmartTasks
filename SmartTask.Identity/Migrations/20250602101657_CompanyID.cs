using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Identity.Migrations
{
    /// <inheritdoc />
    public partial class CompanyID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Company_CompanyId",
                table: "Users",
                column: "CompanyId",
                principalTable: "Company",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Company_CompanyId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Users");
        }
    }
}
