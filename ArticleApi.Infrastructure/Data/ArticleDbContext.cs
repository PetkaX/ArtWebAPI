using ArticleApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Infrastructure.Data;

public class ArticleDbContext(DbContextOptions<ArticleDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Section> Sections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Статья
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Тэг
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Name).IsUnique(); // Регистрозависимый индекс
        });

        // Раздел
        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(1024);
        });

        // Связь статей с тэгами
        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity.HasKey(et => new { et.ArticleId, et.TagId });
            entity.HasIndex(et => et.Order);

            entity.HasOne(et => et.Article)
                .WithMany(e => e.ArticleTags)
                .HasForeignKey(et => et.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(et => et.Tag)
                .WithMany(e => e.ArticleTags)
                .HasForeignKey(et => et.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Связь разделов с тэгами
        modelBuilder.Entity<SectionTag>(entity =>
        {
            entity.HasKey(et => new { et.SectionId, et.TagId });
            entity.HasIndex(et => et.Order);

            entity.HasOne(et => et.Section)
                .WithMany(e => e.SectionTags)
                .HasForeignKey(et => et.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(et => et.Tag)
                .WithMany(e => e.SectionTags)
                .HasForeignKey(et => et.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Регистронезависимый уникальный индекс для Tag.Name
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("IX_Tags_Name_UniqueIgnoreCase")
            .HasFilter("\"Name\" IS NOT NULL");

        // Альтернатива: использование .HasCollation("case_insensitive") в PostgreSQL
        // Но лучше — в миграции или вручную создать индекс с `LOWER("Name")`
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Преобразование имени тега в нормализованное (для регистронезависимости)
        foreach (var entry in ChangeTracker.Entries<Tag>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.Name = NormalizeTagName(entry.Entity.Name);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeTagName(string name) => name?.Trim().ToLowerInvariant() ?? string.Empty;
}