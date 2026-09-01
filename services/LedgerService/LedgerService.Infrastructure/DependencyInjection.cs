using LedgerService.Domain.Abstractions;
using LedgerService.Infrastructure.Context;
using LedgerService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<LedgerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ILedgerRepository, LedgerRepository>();

        return services;
    }
}
