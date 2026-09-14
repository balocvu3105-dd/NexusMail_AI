using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PhaseD_AIProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add new columns for Unified Processing
            migrationBuilder.AddColumn<string>(
                name: "ProcessingState",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "ProcessingError",
                table: "AIAnalyses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessingAttemptId",
                table: "AIAnalyses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsAttention",
                table: "AIAnalyses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingStartedAt",
                table: "AIAnalyses",
                type: "timestamp with time zone",
                nullable: true);

            // 2. Data Migration: Preserve semantics by checking ALL 3 old states
            migrationBuilder.Sql(@"
                UPDATE ""AIAnalyses""
                SET 
                    ""ProcessingState"" = CASE 
                        WHEN ""SummaryStatus"" = 'Succeeded' AND ""ClassificationStatus"" = 'Succeeded' AND ""PriorityStatus"" = 'Succeeded' THEN 'Succeeded'
                        ELSE 'Pending' -- This properly captures old 'Failed', 'Processing' and mismatched states to force re-processing in one-call
                    END,
                    ""ProcessingError"" = COALESCE(""SummaryError"", ""ClassificationError"", ""PriorityError""),
                    ""NeedsAttention"" = CASE WHEN ""PriorityScore"" >= 80 THEN true ELSE false END
            ");

            // 3. Drop old obsolete columns ONLY AFTER migration
            migrationBuilder.DropColumn(name: "ClassificationAttemptId", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "ClassificationError", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "ClassificationStatus", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "PriorityAttemptId", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "PriorityError", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "PriorityStatus", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "SummaryAttemptId", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "SummaryError", table: "AIAnalyses");
            migrationBuilder.DropColumn(name: "SummaryStatus", table: "AIAnalyses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsAttention",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedAt",
                table: "AIAnalyses");

            migrationBuilder.RenameColumn(
                name: "ProcessingState",
                table: "AIAnalyses",
                newName: "SummaryStatus");

            migrationBuilder.RenameColumn(
                name: "ProcessingError",
                table: "AIAnalyses",
                newName: "SummaryError");

            migrationBuilder.RenameColumn(
                name: "ProcessingAttemptId",
                table: "AIAnalyses",
                newName: "SummaryAttemptId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassificationAttemptId",
                table: "AIAnalyses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassificationError",
                table: "AIAnalyses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassificationStatus",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PriorityAttemptId",
                table: "AIAnalyses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityError",
                table: "AIAnalyses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityStatus",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
