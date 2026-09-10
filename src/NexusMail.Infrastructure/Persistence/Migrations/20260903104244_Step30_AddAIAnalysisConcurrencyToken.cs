using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Step30_AddAIAnalysisConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompletedEventPublished",
                table: "AIAnalyses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "AIAnalyses",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompletedEventPublished",
                table: "AIAnalyses");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "AIAnalyses");
        }
    }
}
