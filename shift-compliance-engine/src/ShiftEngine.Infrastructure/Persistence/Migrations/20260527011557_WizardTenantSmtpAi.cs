using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WizardTenantSmtpAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiApiKeySecret",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpFromEmail",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "Tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiApiKeySecret",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SmtpFromEmail",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "Tenants");
        }
    }
}
