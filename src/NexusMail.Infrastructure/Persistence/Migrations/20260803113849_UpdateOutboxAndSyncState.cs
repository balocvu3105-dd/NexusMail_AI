using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOutboxAndSyncState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_LockedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeadLetteredAt",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailureReason",
                table: "OutboxMessages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStage",
                table: "EmailSynchronizationStates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_LockedUntilUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "LockedUntilUtc" },
                filter: "\"ProcessedOnUtc\" IS NULL AND \"DeadLetteredAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_LockedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "DeadLetteredAt",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "EmailSynchronizationStates");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_LockedUntilUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "LockedUntilUtc" },
                filter: "\"ProcessedOnUtc\" IS NULL");
        }
    }
}
