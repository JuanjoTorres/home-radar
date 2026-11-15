using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using HomeRadar.Infrastructure.Persistence.Conversions;
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

internal sealed class ListingRenovationOverrideConfiguration : IEntityTypeConfiguration<ListingRenovationOverride>
{
    public void Configure(EntityTypeBuilder<ListingRenovationOverride> builder)
    {
        builder.ToTable("renovation_overrides");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.ListingId).IsUnique();
        builder.Property(item => item.Level).HasConversion<string>().HasMaxLength(16);
        builder.Property(item => item.TotalBudget).HasPrecision(14, 2);
        builder.Property(item => item.Notes).HasMaxLength(2000);
        builder.Property(item => item.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field).HasColumnType("jsonb")
            .HasConversion(ValueConverters.Json<IReadOnlyList<RenovationOverrideItem>>(), ValueConverters.JsonComparer<IReadOnlyList<RenovationOverrideItem>>());
        builder.Property(item => item.Version).IsRowVersion();
        builder.HasOne<Listing>().WithMany().HasForeignKey(item => item.ListingId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(item => item.DomainEvents);
    }
}

internal sealed class CadastralParcelConfiguration : IEntityTypeConfiguration<CadastralParcel>
{
    public void Configure(EntityTypeBuilder<CadastralParcel> builder)
    {
        builder.ToTable("cadastral_parcels");
        builder.HasKey(parcel => parcel.Id);
        builder.HasIndex(parcel => parcel.ParcelReference).IsUnique();
        builder.Property(parcel => parcel.ParcelReference).HasMaxLength(14);
        builder.Property(parcel => parcel.Status).HasConversion<string>().HasMaxLength(16);
        builder.Property(parcel => parcel.Address).HasMaxLength(300);
        builder.Property(parcel => parcel.Units).HasField("_units").UsePropertyAccessMode(PropertyAccessMode.Field).HasColumnType("jsonb")
            .HasConversion(ValueConverters.Json<IReadOnlyList<CadastralUnit>>(), ValueConverters.JsonComparer<IReadOnlyList<CadastralUnit>>());
        builder.Ignore(parcel => parcel.DomainEvents);
    }
}

internal sealed class ReferenceRateConfiguration : IEntityTypeConfiguration<ReferenceRate>
{
    public void Configure(EntityTypeBuilder<ReferenceRate> builder)
    {
        builder.ToTable("reference_rates");
        builder.HasKey(rate => rate.Id);
        builder.HasIndex(rate => new { rate.Code, rate.Month }).IsUnique();
        builder.Property(rate => rate.Code).HasMaxLength(32);
        builder.Property(rate => rate.Percent).HasPrecision(8, 4);
        builder.Property(rate => rate.Source).HasMaxLength(300);
        builder.Ignore(rate => rate.MonthLabel);
        builder.Ignore(rate => rate.DomainEvents);
    }
}
