using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NotificationService.Infrastructure.Context;

namespace NotificationService.Infrastructure;

class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=notificationservice;Username=postgres;Password=postgres");
        return new NotificationDbContext(optionsBuilder.Options);
    }
}
