using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Context.Configurations;

namespace PaymentService.Infrastructure.Context;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>().Property(p => p.Status).HasConversion<string>();

        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
