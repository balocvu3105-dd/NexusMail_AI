using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Step26_ActionLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExecutionId",
                table: "AutomationAudits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "AutomationActionExecutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "AutomationActionExecutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionId",
                table: "AutomationAudits");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "AutomationActionExecutions");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "AutomationActionExecutions");
        }
    }
}
