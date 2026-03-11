using Microsoft.EntityFrameworkCore;
using BookingService.Models;

namespace BookingService.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasIndex(b => b.RenterId);
            e.HasIndex(b => b.OwnerId);
            e.HasIndex(b => b.CarId);
            e.Property(b => b.Status).HasConversion<string>();
            e.Property(b => b.TotalPrice).HasPrecision(18, 2);
            e.Property(b => b.PricePerDay).HasPrecision(18, 2);
            e.Property(b => b.Deposit).HasPrecision(18, 2);
        });
    }
}
