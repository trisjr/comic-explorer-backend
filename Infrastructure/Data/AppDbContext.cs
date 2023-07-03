using System.Reflection;
using ApplicationCore.Annotations;
using ApplicationCore.Entities.Model;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comic> Comics { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<ComicAndCategory> ComicAndCategories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relationships between Comic and Category
        modelBuilder.Entity<ComicAndCategory>().HasKey(cc => cc.Id);

        modelBuilder
            .Entity<ComicAndCategory>()
            .HasOne(cc => cc.Comic)
            .WithMany(c => c.ComicAndCategories)
            .HasForeignKey(cc => cc.ComicId);

        modelBuilder
            .Entity<ComicAndCategory>()
            .HasOne(cc => cc.Category)
            .WithMany(cat => cat.ComicAndCategories)
            .HasForeignKey(cc => cc.CategoryId);

        // Default schema
        modelBuilder.HasDefaultSchema("BasicSchema");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        now = DateTime.SpecifyKind(now, DateTimeKind.Utc);
        foreach (var entityEntry in ChangeTracker.Entries())
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    SetAutoDateTimeProperties(entityEntry.Entity, now, true);
                    SetAutoDateTimeProperties(entityEntry.Entity, now, false);
                    break;
                case EntityState.Modified:
                    SetAutoDateTimeProperties(entityEntry.Entity, now, false);
                    break;
                case EntityState.Detached
                    or EntityState.Unchanged
                    or EntityState.Deleted:
                    break;
            }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void SetAutoDateTimeProperties(
        object entity,
        DateTime dateTime,
        bool isCreatedAt
    )
    {
        foreach (var property in entity.GetType().GetProperties())
        {
            var autoDateTimeAttribute = property.GetCustomAttribute<DateTimeAttribute>();

            if (autoDateTimeAttribute != null && autoDateTimeAttribute.IsCreatedAt == isCreatedAt)
                property.SetValue(entity, dateTime);
        }
    }
}
