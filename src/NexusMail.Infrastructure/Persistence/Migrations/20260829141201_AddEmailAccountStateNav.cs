using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAccountStateNav : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_MessageId",
                table: "Emails");

            migrationBuilder.AddColumn<string>(
                name: "ProviderCursor",
                table: "EmailSynchronizationStates",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emails_AccountId_MessageId",
                table: "Emails",
                columns: new[] { "AccountId", "MessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Emails_AccountId_MessageId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "ProviderCursor",
                table: "EmailSynchronizationStates");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_MessageId",
                table: "Emails",
                column: "MessageId");
        }
    }
}
