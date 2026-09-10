using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;
using Pgvector;

#nullable disable

namespace NexusMail.Infrastructure.Search.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSearchDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "EmailSearchIndices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    EmbeddingModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmbeddingVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EmbeddingDimension = table.Column<int>(type: "integer", nullable: true),
                    SearchableText = table.Column<string>(type: "text", nullable: true),
                    PriorityScore = table.Column<int>(type: "integer", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IndexedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true, computedColumnSql: "to_tsvector('english', coalesce(\"SearchableText\", ''))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSearchIndices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSearchIndices_Embedding",
                table: "EmailSearchIndices",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailSearchIndices_SearchVector",
                table: "EmailSearchIndices",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSearchIndices_WorkspaceId",
                table: "EmailSearchIndices",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailSearchIndices");
        }
    }
}
