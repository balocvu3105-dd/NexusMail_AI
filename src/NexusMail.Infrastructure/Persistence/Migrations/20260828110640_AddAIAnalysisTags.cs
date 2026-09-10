using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusMail.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAIAnalysisTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Tags",
                table: "AIAnalyses",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "AIAnalyses");
        }
    }
}
