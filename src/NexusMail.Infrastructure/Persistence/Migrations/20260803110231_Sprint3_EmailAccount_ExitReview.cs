using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_EmailAccount_ExitReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "EmailAccounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "EmailAccounts",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_EmailAccounts_WorkspaceId_Status",
                table: "EmailAccounts",
                columns: new[] { "WorkspaceId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmailAccounts_Workspaces_WorkspaceId",
                table: "EmailAccounts",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailAccounts_Workspaces_WorkspaceId",
                table: "EmailAccounts");

            migrationBuilder.DropIndex(
                name: "IX_EmailAccounts_WorkspaceId_Status",
                table: "EmailAccounts");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "EmailAccounts");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "EmailAccounts");
        }
    }
}
