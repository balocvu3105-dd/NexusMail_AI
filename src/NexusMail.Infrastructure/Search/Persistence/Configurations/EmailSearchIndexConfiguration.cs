using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusMail.Domain.Search.Entities;

namespace NexusMail.Infrastructure.Search.Persistence.Configurations;

public class EmailSearchIndexConfiguration : IEntityTypeConfiguration<EmailSearchIndex>
{
    public void Configure(EntityTypeBuilder<EmailSearchIndex> builder)
    {
        builder.ToTable("EmailSearchIndices");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.WorkspaceId).IsRequired();
        builder.HasIndex(e => e.WorkspaceId);
        
        // Define vector column with dimension 1536 (default for text-embedding-3-small)
        // If it varies, pgvector can also omit the dimension size, but having it is better for indexing.
        // For simplicity, we just use Vector type. We can use HasColumnType("vector(1536)") if we know it's always 1536.
        builder.Property(e => e.Embedding)
               .HasColumnType("vector(1536)");
               
        builder.Property(e => e.EmbeddingModel).HasMaxLength(100);
        builder.Property(e => e.EmbeddingVersion).HasMaxLength(20);
        
        // Full Text Search column (Computed Shadow Property)
        builder.Property<NpgsqlTypes.NpgsqlTsVector>("SearchVector")
               .HasComputedColumnSql("to_tsvector('english', coalesce(\"SearchableText\", ''))", stored: true);
               
        // Create GIN index for Full Text Search
        builder.HasIndex("SearchVector")
               .HasMethod("GIN");
               
        // Create HNSW index for Vector search (using cosine distance)
        builder.HasIndex(e => e.Embedding)
               .HasMethod("hnsw")
               .HasOperators("vector_cosine_ops");
    }
}
