using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Step29_AIStatusTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "AIAnalyses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "AIAnalyses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "AIAnalyses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "AIAnalyses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

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
                name: "EmbeddingAttemptId",
                table: "AIAnalyses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingError",
                table: "AIAnalyses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingStatus",
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

            migrationBuilder.AddColumn<Guid>(
                name: "SummaryAttemptId",
                table: "AIAnalyses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryError",
                table: "AIAnalyses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryStatus",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AIAnalyses_EmailId",
                table: "AIAnalyses",
                column: "EmailId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AIAnalyses_EmailId",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "ClassificationAttemptId",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "ClassificationError",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "ClassificationStatus",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "EmbeddingAttemptId",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "EmbeddingError",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "EmbeddingStatus",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "PriorityAttemptId",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "PriorityError",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "PriorityStatus",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "SummaryAttemptId",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "SummaryError",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "SummaryStatus",
                table: "AIAnalyses");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "AIAnalyses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
