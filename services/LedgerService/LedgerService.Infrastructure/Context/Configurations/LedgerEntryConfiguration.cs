using LedgerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LedgerService.Infrastructure.Context.Configurations;

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.PaymentId);

        builder.HasIndex(e => e.AccountId);

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(e => e.BalanceAfter)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasDefaultValue(DateTime.UtcNow)
            .IsRequired();
    }
}
