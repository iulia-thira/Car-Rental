using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.BookingId);
            e.Property(p => p.Status).HasConversion<string>();
            e.Property(p => p.Amount).HasPrecision(18, 2);
            e.Property(p => p.Deposit).HasPrecision(18, 2);
            e.Property(p => p.PlatformFee).HasPrecision(18, 2);
            e.Property(p => p.OwnerPayout).HasPrecision(18, 2);
        });
    }
}
