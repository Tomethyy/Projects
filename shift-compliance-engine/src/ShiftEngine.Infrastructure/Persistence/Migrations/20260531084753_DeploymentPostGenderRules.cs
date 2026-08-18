using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeploymentPostGenderRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinRequiredFemale",
                table: "DeploymentPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinRequiredMale",
                table: "DeploymentPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinRequiredFemale",
                table: "DeploymentPosts");

            migrationBuilder.DropColumn(
                name: "MinRequiredMale",
                table: "DeploymentPosts");
        }
    }
}
