using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Domain.Abstractions;
using NotificationService.Infrastructure.Context;
using NotificationService.Infrastructure.MailSender;
using NotificationService.Infrastructure.Repositories;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IEmailSender, MailtrapEmailSender>();

        return services;
    }
}
