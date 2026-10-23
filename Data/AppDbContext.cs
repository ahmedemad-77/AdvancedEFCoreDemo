using AdvancedEFCoreDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCoreDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name)
                  .IsRequired()
                  .HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(p => p.Price)
                  .HasPrecision(18, 2);

            // Requirement 6: concurrency token (SQL Server rowversion)
            entity.Property(p => p.RowVersion)
                  .IsRowVersion();

            // Relationship: Category 1 --- * Products
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Requirement 3: index on the frequently searched column
            entity.HasIndex(p => p.Name)
                  .HasDatabaseName("IX_Products_Name");

            // Requirement 4: soft delete global query filter
            entity.HasQueryFilter(p => !p.IsDeleted);
        });
    }
}
