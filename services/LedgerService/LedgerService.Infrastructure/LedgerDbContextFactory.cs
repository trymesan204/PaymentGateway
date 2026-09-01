using LedgerService.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LedgerService.Infrastructure;

class LedgerDbContextFactory : IDesignTimeDbContextFactory<LedgerDbContext>
{
    public LedgerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LedgerDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=ledgerservice;Username=postgres;Password=postgres");
        return new LedgerDbContext(optionsBuilder.Options);
    }
}
