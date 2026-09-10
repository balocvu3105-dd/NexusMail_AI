using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSyncAndOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CausationId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "EmailAccounts");

            migrationBuilder.RenameColumn(
                name: "RetryCount",
                table: "OutboxMessages",
                newName: "Attempts");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "OutboxMessages",
                newName: "Content");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "OutboxMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Headers",
                table: "OutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LockId",
                table: "OutboxMessages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockedUntilUtc",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailSynchronizationStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSuccessAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSynchronizationStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSynchronizationStates_EmailAccounts_EmailAccountId",
                        column: x => x.EmailAccountId,
                        principalTable: "EmailAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_LockedUntilUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "LockedUntilUtc" },
                filter: "\"ProcessedOnUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSynchronizationStates_EmailAccountId",
                table: "EmailSynchronizationStates",
                column: "EmailAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailSynchronizationStates_Status",
                table: "EmailSynchronizationStates",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailSynchronizationStates");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_LockedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Headers",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LockId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LockedUntilUtc",
                table: "OutboxMessages");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "OutboxMessages",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "Attempts",
                table: "OutboxMessages",
                newName: "RetryCount");

            migrationBuilder.AlterColumn<Guid>(
                name: "CorrelationId",
                table: "OutboxMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CausationId",
                table: "OutboxMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OutboxMessages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "OutboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "OutboxMessages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "OutboxMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "EmailAccounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
