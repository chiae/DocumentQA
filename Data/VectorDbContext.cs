using DocumentQA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        public DbSet<UserEntity> Users => Set<UserEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- FLOAT[] JSON CONVERTER ---
            var floatArrayConverter = new ValueConverter<float[], string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<float>()
            );

            // --- REQUIRED VALUE COMPARER FOR SQLITE ---
            var floatArrayComparer = new ValueComparer<float[]>(
                (a, b) => a.SequenceEqual(b),
                a => a.Aggregate(0, (hash, x) => HashCode.Combine(hash, x.GetHashCode())),
                a => a.ToArray()
            );

            // -------------------------
            // DocumentEntity
            // -------------------------
            modelBuilder.Entity<DocumentEntity>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileName)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.UploadedAt)
                      .IsRequired();

                // Optional fields (SQLite handles nulls fine)
                entity.Property(e => e.PageCount);
                entity.Property(e => e.ChunkCount);
                entity.Property(e => e.UserId);
            });

            // -------------------------
            // ChunkEntity
            // -------------------------
            modelBuilder.Entity<ChunkEntity>(entity =>
            {
                entity.ToTable("Chunks");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DocumentId)
                      .IsRequired(); // SQLite does not need max length

                entity.Property(e => e.Text)
                      .IsRequired();

                entity.Property(e => e.Index)
                      .IsRequired();

                entity.Property(e => e.Embedding)
                      .HasConversion(floatArrayConverter)
                      .Metadata.SetValueComparer(floatArrayComparer);

                entity.HasOne(c => c.Document)
                      .WithMany(d => d.Chunks)
                      .HasForeignKey(c => c.DocumentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------
            // UserEntity
            // -------------------------
            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}