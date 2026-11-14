using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using HomeRadar.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeRadar.Infrastructure.Persistence.Configurations;

internal sealed class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("listings");
        builder.HasKey(listing => listing.Id);
        builder.HasIndex(listing => new { listing.SourceId, listing.ExternalId }).IsUnique();
        builder.HasIndex(listing => listing.FirstSeenAt);
        builder.HasIndex(listing => listing.Municipality);
        builder.HasIndex(listing => listing.Status);
        builder.HasIndex(listing => listing.DwellingId);
        builder.HasIndex(listing => listing.Location).HasMethod("gist");
        builder.HasIndex(listing => listing.LocationUtm).HasMethod("gist");

        builder.Property(listing => listing.ExternalId).HasMaxLength(200);
        builder.Property(listing => listing.SourceKey).HasMaxLength(64);
        builder.Property(listing => listing.Url).HasConversion(ValueConverters.Uri).HasMaxLength(2048);
        builder.Property(listing => listing.Title).HasMaxLength(500);
        builder.Property(listing => listing.PriceAmount).HasPrecision(14, 2);
        builder.Property(listing => listing.InitialPriceAmount).HasPrecision(14, 2);
        builder.Property(listing => listing.BuiltSquareMetres).HasPrecision(10, 2);
        builder.Property(listing => listing.UsableSquareMetres).HasPrecision(10, 2);
        builder.Ignore(listing => listing.Price);
        builder.Ignore(listing => listing.InitialPrice);
        builder.Ignore(listing => listing.BuiltArea);
        builder.Ignore(listing => listing.UsableArea);
        builder.Property(listing => listing.Municipality).HasMaxLength(120);
        builder.Property(listing => listing.Zone).HasMaxLength(200);
        builder.Property(listing => listing.Location).HasColumnType($"geometry(Point,{UtmProjection.Wgs84Srid})");
        builder.Property(listing => listing.LocationUtm).HasColumnType($"geometry(Point,{UtmProjection.Utm31NSrid})");
        builder.Property(listing => listing.ContentHash).HasConversion(ValueConverters.ContentHash).HasMaxLength(64);
        builder.Property(listing => listing.CadastralReference).HasConversion(ValueConverters.NullableCadastralReference).HasMaxLength(20);
        builder.Property(listing => listing.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(listing => listing.Possession).HasConversion<string>().HasMaxLength(20);
        builder.Property(listing => listing.PropertyType).HasConversion<string>().HasMaxLength(20);
        builder.Property(listing => listing.SellerType).HasConversion<string>().HasMaxLength(20);
        builder.Property(listing => listing.Features).HasColumnType("jsonb").HasConversion(ValueConverters.Json<IReadOnlyDictionary<string, string>>(), ValueConverters.DictionaryComparer);
        builder.Property(listing => listing.Enrichments).HasColumnType("jsonb").HasConversion(ValueConverters.Json<IReadOnlyDictionary<string, string>>(), ValueConverters.DictionaryComparer);
        builder.Ignore(listing => listing.DomainEvents);
        builder.Ignore(listing => listing.PricePerSquareMetre);
        builder.Ignore(listing => listing.CumulativeDropPercent);

        builder.HasMany(listing => listing.Photos).WithOne().HasForeignKey("ListingId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(listing => listing.Photos).AutoInclude();
        builder.Metadata.FindNavigation(nameof(Listing.Photos))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasOne<Domain.Sources.Source>().WithMany().HasForeignKey(listing => listing.SourceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Agency>().WithMany().HasForeignKey(listing => listing.AgencyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Dwelling>().WithMany().HasForeignKey(listing => listing.DwellingId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class ListingPhotoConfiguration : IEntityTypeConfiguration<ListingPhoto>
{
    public void Configure(EntityTypeBuilder<ListingPhoto> builder)
    {
        builder.ToTable("listing_photos");
        builder.HasKey(photo => photo.Id);
        builder.Property(photo => photo.Url).HasConversion(ValueConverters.Uri).HasMaxLength(2048);
        builder.Property(photo => photo.StoredCopy).HasConversion(ValueConverters.NullableUri).HasMaxLength(2048);
        builder.Property(photo => photo.PerceptualHash).HasConversion(ValueConverters.NullableUInt64);
    }
}

internal sealed class ListingSnapshotConfiguration : IEntityTypeConfiguration<ListingSnapshot>
{
    public void Configure(EntityTypeBuilder<ListingSnapshot> builder)
    {
        builder.ToTable("listing_snapshots");
        builder.HasKey(snapshot => snapshot.Id);
        builder.HasIndex(snapshot => new { snapshot.ListingId, snapshot.CapturedAt });
        builder.Property(snapshot => snapshot.ContentHash).HasConversion(ValueConverters.ContentHash).HasMaxLength(64);
        builder.Property(snapshot => snapshot.ContentType).HasMaxLength(100);
        builder.HasOne<Listing>().WithMany().HasForeignKey(snapshot => snapshot.ListingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Domain.Sources.SourceRun>().WithMany().HasForeignKey(snapshot => snapshot.SourceRunId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ListingEventConfiguration : IEntityTypeConfiguration<ListingEvent>
{
    public void Configure(EntityTypeBuilder<ListingEvent> builder)
    {
        builder.ToTable("listing_events");
        builder.HasKey(listingEvent => listingEvent.Id);
        builder.HasIndex(listingEvent => new { listingEvent.ListingId, listingEvent.OccurredAt });
        builder.HasIndex(listingEvent => listingEvent.OccurredAt);
        builder.Property(listingEvent => listingEvent.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(listingEvent => listingEvent.Payload).HasColumnType("jsonb");
        builder.HasOne<Listing>().WithMany().HasForeignKey(listingEvent => listingEvent.ListingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ListingSnapshot>().WithMany().HasForeignKey(listingEvent => listingEvent.SnapshotId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DwellingConfiguration : IEntityTypeConfiguration<Dwelling>
{
    public void Configure(EntityTypeBuilder<Dwelling> builder)
    {
        builder.ToTable("dwellings");
        builder.HasKey(dwelling => dwelling.Id);
        builder.Property(dwelling => dwelling.CadastralReference).HasConversion(ValueConverters.NullableCadastralReference).HasMaxLength(20);
        builder.HasIndex(dwelling => dwelling.CadastralReference);
        builder.Property(dwelling => dwelling.Location).HasColumnType($"geometry(Point,{UtmProjection.Wgs84Srid})");
        builder.Property(dwelling => dwelling.BuiltArea).HasConversion(ValueConverters.NullableArea).HasPrecision(10, 2);
        builder.Property(dwelling => dwelling.PropertyType).HasConversion<string>().HasMaxLength(20);
        builder.Property(dwelling => dwelling.MatchConfidence).HasPrecision(4, 3);
        builder.Ignore(dwelling => dwelling.DomainEvents);
    }
}

internal sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("agencies");
        builder.HasKey(agency => agency.Id);
        builder.Property(agency => agency.Name).HasMaxLength(200);
        builder.Property(agency => agency.Website).HasConversion(ValueConverters.Uri).HasMaxLength(2048);
        builder.HasIndex(agency => agency.Website).IsUnique();
        builder.Property(agency => agency.Municipality).HasMaxLength(120);
        builder.Property(agency => agency.CrmKey).HasMaxLength(64);
        builder.Ignore(agency => agency.DomainEvents);
    }
}
