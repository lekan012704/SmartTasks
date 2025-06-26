using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Identity.Migrations
{
    /// <inheritdoc />
    public partial class SuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgencyCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AgencyName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovalRankingId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MerchantCode",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgencyCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AgencyName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalRankingId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
