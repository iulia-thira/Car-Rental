using Microsoft.EntityFrameworkCore;
using ReviewService.Models;

namespace ReviewService.Data;

public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => new { r.BookingId, r.AuthorId, r.TargetType }).IsUnique();
            e.Property(r => r.TargetType).HasConversion<string>();
            e.Property(r => r.Comment).HasMaxLength(2000);
        });
    }
}
