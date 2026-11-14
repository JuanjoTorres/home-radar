using HomeRadar.Domain.Configuration;
using HomeRadar.Domain.Outbox;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.Watchlist;
using HomeRadar.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeRadar.Infrastructure.Persistence.Configurations;

internal sealed class SourceConfiguration : IEntityTypeConfiguration<Source>
{
    public void Configure(EntityTypeBuilder<Source> builder)
    {
        builder.ToTable("sources");
        builder.HasKey(source => source.Id);
        builder.HasIndex(source => source.Key).IsUnique();
        builder.Property(source => source.Key).HasMaxLength(64);
        builder.Property(source => source.Name).HasMaxLength(200);
        builder.Property(source => source.AdapterKey).HasMaxLength(64);
        builder.Property(source => source.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(source => source.SellerType).HasConversion<string>().HasMaxLength(20);
        builder.Property(source => source.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(source => source.ConfigurationJson).HasColumnType("jsonb");
        builder.Ignore(source => source.DomainEvents);
    }
}

internal sealed class SourceRunConfiguration : IEntityTypeConfiguration<SourceRun>
{
    public void Configure(EntityTypeBuilder<SourceRun> builder)
    {
        builder.ToTable("source_runs");
        builder.HasKey(run => run.Id);
        builder.HasIndex(run => new { run.SourceId, run.StartedAt });
        builder.Property(run => run.Outcome).HasConversion<string>().HasMaxLength(20);
        builder.Property(run => run.Message).HasMaxLength(2000);
        builder.HasOne<Source>().WithMany().HasForeignKey(run => run.SourceId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class FinancingProfileConfiguration : IEntityTypeConfiguration<FinancingProfile>
{
    public void Configure(EntityTypeBuilder<FinancingProfile> builder)
    {
        builder.ToTable("financing_profiles");
        builder.HasKey(profile => profile.Id);
        builder.HasIndex(profile => profile.Key).IsUnique();
        builder.Property(profile => profile.Key).HasMaxLength(64);
        builder.Property(profile => profile.Name).HasMaxLength(200);
        builder.Property(profile => profile.Savings).HasConversion(ValueConverters.Money).HasPrecision(14, 2);
        builder.Property(profile => profile.NetMonthlyIncome).HasConversion(ValueConverters.Money).HasPrecision(14, 2);
        builder.Property(profile => profile.ExistingMonthlyDebt).HasConversion(ValueConverters.Money).HasPrecision(14, 2);
        builder.Property(profile => profile.MaxDebtToIncomeRatio).HasPrecision(5, 4);
        builder.Property(profile => profile.Mortgage).HasColumnType("jsonb").HasConversion(ValueConverters.Json<MortgageTerms>(), ValueConverters.JsonComparer<MortgageTerms>());
        builder.Property(profile => profile.Buyer).HasColumnType("jsonb").HasConversion(ValueConverters.Json<BuyerProfile>(), ValueConverters.JsonComparer<BuyerProfile>());
        builder.Property(profile => profile.Version).IsRowVersion();
        builder.Ignore(profile => profile.DomainEvents);
    }
}

internal sealed class SearchProfileConfiguration : IEntityTypeConfiguration<SearchProfile>
{
    public void Configure(EntityTypeBuilder<SearchProfile> builder)
    {
        builder.ToTable("search_profiles");
        builder.HasKey(profile => profile.Id);
        builder.HasIndex(profile => profile.Key).IsUnique();
        builder.Property(profile => profile.Key).HasMaxLength(64);
        builder.Property(profile => profile.Name).HasMaxLength(200);
        builder.Property(profile => profile.Filter).HasColumnType("jsonb").HasConversion(ValueConverters.Json<ListingFilter>(), ValueConverters.JsonComparer<ListingFilter>());
        builder.Property(profile => profile.Channels).HasColumnType("jsonb").HasConversion(ValueConverters.Json<List<AlertChannel>>(), ValueConverters.JsonComparer<List<AlertChannel>>());
        builder.Property(profile => profile.Version).IsRowVersion();
        builder.Ignore(profile => profile.DomainEvents);
    }
}

internal sealed class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.ToTable("watchlist_items");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.ListingId).IsUnique();
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(item => item.Notes).HasMaxLength(4000);
        builder.Property(item => item.Version).IsRowVersion();
        builder.HasOne<Domain.Listings.Listing>().WithMany().HasForeignKey(item => item.ListingId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(item => item.DomainEvents);
    }
}

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.HasIndex(message => new { message.ProcessedAt, message.NextAttemptAt, message.CreatedAt });
        builder.Property(message => message.Type).HasMaxLength(64);
        builder.Property(message => message.Payload).HasColumnType("jsonb");
        builder.Property(message => message.LastError).HasMaxLength(2000);
        builder.Ignore(message => message.Exhausted);
    }
}

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");
        builder.HasKey(record => record.Key);
        builder.Property(record => record.Key).HasMaxLength(128);
        builder.Property(record => record.RequestHash).HasMaxLength(64);
        builder.HasIndex(record => record.ExpiresAt);
    }
}

internal sealed class BusinessConfigVersionConfiguration : IEntityTypeConfiguration<BusinessConfigVersion>
{
    public void Configure(EntityTypeBuilder<BusinessConfigVersion> builder)
    {
        builder.ToTable("business_config_versions");
        builder.HasKey(version => version.Id);
        builder.HasIndex(version => version.ValidFrom).IsUnique();
        builder.Property(version => version.PayloadJson).HasColumnType("jsonb");
        builder.Property(version => version.SourceDescription).HasMaxLength(500);
        builder.Ignore(version => version.DomainEvents);
    }
}
