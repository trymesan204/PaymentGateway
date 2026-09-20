using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Context;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Infrastructure.Services;

namespace PaymentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentProcessor, MockPaymentProcessor>();

        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddHostedService<OutboxRelayService>();

        var ledgerBaseUrl = configuration["Ledger:BaseUrl"]
            ?? throw new InvalidOperationException("Configuration value 'Ledger:BaseUrl' not found.");

        services.AddHttpClient<ILedgerClient, LedgerClient>(client =>
        {
            client.BaseAddress = new Uri(ledgerBaseUrl);
        });

        return services;
    }
}
