using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Context.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_logs");

        builder.HasKey(n => n.Id);

        builder.HasIndex(n => n.PaymentId);

        builder.HasIndex(n => n.RecipientAccountId);

        builder.Property(n => n.Subject)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Body)
            .IsRequired();

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.FailureReason);

        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
