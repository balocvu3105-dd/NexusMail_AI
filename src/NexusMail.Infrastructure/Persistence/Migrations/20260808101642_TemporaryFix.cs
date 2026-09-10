using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TemporaryFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    PriorityScore = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAnalyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Executor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRuleMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleVersion = table.Column<int>(type: "integer", nullable: false),
                    EmailId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExecutionLatencyMs = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRuleMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutomationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TriggerType = table.Column<int>(type: "integer", nullable: false),
                    ConditionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DryRun = table.Column<bool>(type: "boolean", nullable: false),
                    RuleVersion = table.Column<int>(type: "integer", nullable: false),
                    ExecutionCount = table.Column<int>(type: "integer", nullable: false),
                    LastExecutedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Sender = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    LifecycleState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationAudits_EmailId",
                table: "AutomationAudits",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationAudits_RuleId",
                table: "AutomationAudits",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleMetrics_EmailId",
                table: "AutomationRuleMetrics",
                column: "EmailId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRuleMetrics_RuleId",
                table: "AutomationRuleMetrics",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomationRules_WorkspaceId",
                table: "AutomationRules",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_AccountId",
                table: "Emails",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_MessageId",
                table: "Emails",
                column: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIAnalyses");

            migrationBuilder.DropTable(
                name: "AutomationAudits");

            migrationBuilder.DropTable(
                name: "AutomationRuleMetrics");

            migrationBuilder.DropTable(
                name: "AutomationRules");

            migrationBuilder.DropTable(
                name: "Emails");
        }
    }
}
