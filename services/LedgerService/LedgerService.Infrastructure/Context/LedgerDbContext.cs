using LedgerService.Domain.Entities;
using LedgerService.Infrastructure.Context.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Infrastructure.Context;

public class LedgerDbContext : DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new LedgerEntryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
