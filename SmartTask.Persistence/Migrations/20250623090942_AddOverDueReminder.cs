using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTask.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOverDueReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OverdueReminderSent",
                table: "TaskItem",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverdueReminderSent",
                table: "TaskItem");
        }
    }
}
