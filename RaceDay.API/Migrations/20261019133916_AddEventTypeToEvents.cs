using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDay.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "Events",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Run");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Events_EventType",
                table: "Events",
                sql: "[EventType] IN ('Run','Walk','Cycle')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Events_EventType",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Events");
        }
    }
}
