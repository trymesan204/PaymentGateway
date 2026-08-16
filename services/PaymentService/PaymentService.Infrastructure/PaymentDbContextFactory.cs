using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PaymentService.Infrastructure.Context;

namespace PaymentService.Infrastructure;

class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=paymentservice;Username=postgres;Password=postgres");
        return new PaymentDbContext(optionsBuilder.Options);
    }
}
