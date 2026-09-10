using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Step25_AutomationExecutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationActionExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationActionExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationExecutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationActionExecutions_ExecutionId_ActionKey",
                table: "AutomationActionExecutions",
                columns: new[] { "ExecutionId", "ActionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AutomationExecutions_RuleId_EmailId",
                table: "AutomationExecutions",
                columns: new[] { "RuleId", "EmailId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationActionExecutions");

            migrationBuilder.DropTable(
                name: "AutomationExecutions");
        }
    }
}
