using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LeaveCarryoverFrozen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCarryoverFrozen",
                table: "LeaveRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCarryoverFrozen",
                table: "LeaveRecords");
        }
    }
}
