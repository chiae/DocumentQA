using DocumentQA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace DocumentQA.Data
{
    public class VectorDbContext : DbContext
    {
        public VectorDbContext(DbContextOptions<VectorDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChunkEntity> Chunks => Set<ChunkEntity>();
        public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var floatArrayConverter = new ValueConverter<float[], string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<float>()
            );

            modelBuilder.Entity<DocumentEntity>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.UploadedAt).IsRequired();
                entity.Property(e => e.PageCount);
                entity.Property(e => e.ChunkCount);
            });

            modelBuilder.Entity<ChunkEntity>(entity =>
            {
                entity.ToTable("Chunks");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.DocumentId)
                      .IsRequired()
                      .HasMaxLength(450);

                entity.Property(e => e.Text)
                      .IsRequired();

                entity.Property(e => e.Index)
                      .IsRequired();

                entity.Property(e => e.Embedding)
                      .HasConversion(floatArrayConverter)
                      .HasColumnType("nvarchar(max)");

                entity.HasOne<DocumentEntity>().WithMany(d => d.Chunks).HasForeignKey(e => e.DocumentId);
            });
        }
    }
}