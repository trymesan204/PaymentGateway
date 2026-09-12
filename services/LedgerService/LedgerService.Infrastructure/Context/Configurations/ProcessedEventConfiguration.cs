using LedgerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerService.Infrastructure.Context.Configurations;

public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        builder.ToTable("processed_events");

        builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.ProcessedAt)
            .IsRequired();
    }
}
