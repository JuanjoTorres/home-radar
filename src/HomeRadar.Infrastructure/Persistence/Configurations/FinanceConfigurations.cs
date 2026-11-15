using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeRadar.Infrastructure.Persistence.Configurations;

internal sealed class TaxRuleSetConfiguration : IEntityTypeConfiguration<TaxRuleSet>
{
    public void Configure(EntityTypeBuilder<TaxRuleSet> builder)
    {
        builder.ToTable("tax_rule_sets");
        builder.HasKey(set => set.Id);
        builder.HasIndex(set => new { set.Jurisdiction, set.ValidFrom, set.Revision }).IsUnique();
        builder.Property(set => set.Jurisdiction).HasMaxLength(32);
        builder.Property(set => set.SourceFile).HasMaxLength(260);
        builder.Property(set => set.DocumentJson).HasColumnType("jsonb");
        builder.Ignore(set => set.Document);
        builder.Ignore(set => set.DomainEvents);
    }
}

internal sealed class ListingAnalysisConfiguration : IEntityTypeConfiguration<ListingAnalysis>
{
    public void Configure(EntityTypeBuilder<ListingAnalysis> builder)
    {
        builder.ToTable("listing_analyses");
        builder.HasKey(analysis => analysis.Id);
        builder.HasIndex(analysis => new { analysis.ListingId, analysis.FinancingProfileId, analysis.InputHash }).IsUnique();
        builder.HasIndex(analysis => new { analysis.ListingId, analysis.FinancingProfileId, analysis.CalculatedAt });
        builder.Property(analysis => analysis.InputHash).HasMaxLength(64);
        builder.Property(analysis => analysis.Verdict).HasConversion<string>().HasMaxLength(20);
        builder.Property(analysis => analysis.PriceAmount).HasPrecision(14, 2);
        builder.Property(analysis => analysis.MaximumAffordablePrice).HasPrecision(14, 2);
        builder.Property(analysis => analysis.TaxRuleSetIds).HasColumnType("uuid[]");
        builder.Property(analysis => analysis.InputJson).HasColumnType("jsonb");
        builder.Property(analysis => analysis.ResultJson).HasColumnType("jsonb");
        builder.HasOne<Listing>().WithMany().HasForeignKey(analysis => analysis.ListingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<FinancingProfile>().WithMany().HasForeignKey(analysis => analysis.FinancingProfileId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(analysis => analysis.DomainEvents);
    }
}
