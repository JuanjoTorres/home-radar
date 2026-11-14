using HomeRadar.Domain.Common;
using HomeRadar.Domain.ValueObjects;
using NetTopologySuite.Geometries;

namespace HomeRadar.Domain.Listings;

public sealed class Listing : Entity
{
    private readonly List<ListingPhoto> _photos = [];
    private Dictionary<string, string> _features = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _enrichments = new(StringComparer.Ordinal);

    private Listing()
    {
    }

    private Listing(Guid sourceId, string sourceKey, SellerType sellerType, ListingObservation observation, DateTimeOffset now)
    {
        SourceId = sourceId;
        SourceKey = sourceKey;
        SellerType = sellerType;
        ExternalId = observation.ExternalId;
        FirstSeenAt = now;
        LastSeenAt = now;
        Apply(observation);
        ContentHash = observation.ComputeContentHash();
        Status = observation.Status;
        Possession = observation.Possession;
        PublishedAt = observation.PublishedAt;
    }

    public Guid SourceId { get; private init; }

    public string SourceKey { get; private init; } = string.Empty;

    public SellerType SellerType { get; private init; }

    public Guid? AgencyId { get; private set; }

    public Guid? DwellingId { get; private set; }

    public string ExternalId { get; private init; } = string.Empty;

    public Uri Url { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public decimal? PriceAmount { get; private set; }

    public decimal? BuiltSquareMetres { get; private set; }

    public decimal? UsableSquareMetres { get; private set; }

    public Money? Price => PriceAmount is { } amount ? new Money(amount) : null;

    public Area? BuiltArea => Area.FromNullable(BuiltSquareMetres);

    public Area? UsableArea => Area.FromNullable(UsableSquareMetres);

    public int? Rooms { get; private set; }

    public int? Bathrooms { get; private set; }

    public PropertyType PropertyType { get; private set; }

    public string? Municipality { get; private set; }

    public string? Zone { get; private set; }

    public Point? Location { get; private set; }

    public Point? LocationUtm { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyDictionary<string, string> Features => _features;

    public IReadOnlyDictionary<string, string> Enrichments => _enrichments;

    public bool? HasElevator { get; private set; }

    public bool? HasTerrace { get; private set; }

    public bool? HasParking { get; private set; }

    public int? YearBuilt { get; private set; }

    public bool IsNewBuild { get; private set; }

    public IReadOnlyList<ListingPhoto> Photos => _photos;

    public ListingStatus Status { get; private set; }

    public PossessionStatus Possession { get; private set; }

    public CadastralReference? CadastralReference { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset FirstSeenAt { get; private init; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public ContentHash ContentHash { get; private set; }

    public decimal? InitialPriceAmount { get; private set; }

    public Money? InitialPrice => InitialPriceAmount is { } amount ? new Money(amount) : null;

    public int PriceDropCount { get; private set; }

    public DateTimeOffset? LastPriceChangeAt { get; private set; }

    public decimal? PricePerSquareMetre =>
        Price is { } price && BuiltArea is { SquareMetres: > 0 } area ? area.PriceFor(price).EurosPerSquareMetre : null;

    public decimal CumulativeDropPercent =>
        InitialPrice is { Amount: > 0 } initial && Price is { } current
            ? Math.Round((initial.Amount - current.Amount) / initial.Amount * 100m, 2, MidpointRounding.ToEven)
            : 0m;

    public static Listing Create(Guid sourceId, string sourceKey, SellerType sellerType, ListingObservation observation, DateTimeOffset now)
    {
        var listing = new Listing(sourceId, sourceKey, sellerType, observation, now)
        {
            InitialPriceAmount = observation.Price?.Amount,
        };
        listing.Record(ListingEventType.Created, now, new CreatedPayload(observation.Price, observation.Municipality, observation.PropertyType));
        return listing;
    }

    public ObservationOutcome Observe(ListingObservation observation, DateTimeOffset now)
    {
        if (!string.Equals(observation.ExternalId, ExternalId, StringComparison.Ordinal))
        {
            throw new DomainException($"Observation {observation.ExternalId} does not belong to listing {ExternalId}.");
        }

        var wasMissing = Status is ListingStatus.Withdrawn;
        var newHash = observation.ComputeContentHash();
        var lastSeenBefore = LastSeenAt;
        LastSeenAt = now;

        if (wasMissing)
        {
            Record(ListingEventType.Relisted, now, new RelistedPayload(lastSeenBefore, Price, observation.Price));
        }

        if (newHash == ContentHash && !wasMissing)
        {
            return ObservationOutcome.Unchanged;
        }

        DetectPriceChange(observation, now);
        DetectDescriptionChange(observation, now);
        DetectPhotoChanges(observation, now);
        DetectFeatureChanges(observation, now);
        DetectStatusChange(observation, now, wasMissing);
        DetectAgencyChange(observation, now);

        Apply(observation);
        ContentHash = newHash;
        return ObservationOutcome.Changed;
    }

    public void MarkNotSeen(DateTimeOffset now)
    {
        if (Status is ListingStatus.Withdrawn or ListingStatus.Sold)
        {
            return;
        }

        Record(ListingEventType.StatusChanged, now, new StatusChangedPayload(Status, ListingStatus.Withdrawn, Possession, Possession));
        Status = ListingStatus.Withdrawn;
    }

    public void AssignDwelling(Guid dwellingId) => DwellingId = dwellingId;

    public void ApplyEnrichment(IEnumerable<KeyValuePair<string, string>> facts)
    {
        foreach (var fact in facts)
        {
            _enrichments[fact.Key] = fact.Value;
        }
    }

    public void SetPhotoHash(Uri url, ulong hash) => _photos.Find(photo => photo.Url == url)?.SetPerceptualHash(hash);

    private void DetectPriceChange(ListingObservation observation, DateTimeOffset now)
    {
        if (Price == observation.Price)
        {
            return;
        }

        decimal? delta = Price is { } before && observation.Price is { } after ? after.Amount - before.Amount : null;
        decimal? percent = delta is { } d && Price is { Amount: > 0 } p ? Math.Round(d / p.Amount * 100m, 2, MidpointRounding.ToEven) : null;
        if (delta < 0)
        {
            PriceDropCount++;
        }

        InitialPriceAmount ??= observation.Price?.Amount;
        LastPriceChangeAt = now;
        Record(ListingEventType.PriceChanged, now, new PriceChangedPayload(Price, observation.Price, delta, percent));
    }

    private void DetectDescriptionChange(ListingObservation observation, DateTimeOffset now)
    {
        if (string.Equals(Description, observation.Description, StringComparison.Ordinal))
        {
            return;
        }

        Record(ListingEventType.DescriptionChanged, now, new DescriptionChangedPayload(Description, observation.Description));
    }

    private void DetectPhotoChanges(ListingObservation observation, DateTimeOffset now)
    {
        var before = _photos.OrderBy(photo => photo.Position).Select(photo => photo.Url).ToList();
        var after = observation.PhotoUrls.ToList();
        if (before.SequenceEqual(after))
        {
            return;
        }

        var removed = before.Except(after).ToList();
        var added = after.Except(before).ToList();
        var replacedPairs = Math.Min(removed.Count, added.Count);

        for (var index = 0; index < replacedPairs; index++)
        {
            var position = after.IndexOf(added[index]);
            Record(ListingEventType.PhotoReplaced, now, new PhotoChangedPayload(added[index], position, removed[index]));
        }

        foreach (var url in added.Skip(replacedPairs))
        {
            Record(ListingEventType.PhotoAdded, now, new PhotoChangedPayload(url, after.IndexOf(url), null));
        }

        foreach (var url in removed.Skip(replacedPairs))
        {
            Record(ListingEventType.PhotoRemoved, now, new PhotoChangedPayload(url, before.IndexOf(url), null));
        }
    }

    private void DetectFeatureChanges(ListingObservation observation, DateTimeOffset now)
    {
        var keys = _features.Keys.Union(observation.Features.Keys, StringComparer.Ordinal).ToList();
        var changedBefore = new Dictionary<string, string?>(StringComparer.Ordinal);
        var changedAfter = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var key in keys)
        {
            _features.TryGetValue(key, out var oldValue);
            observation.Features.TryGetValue(key, out var newValue);
            if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
            {
                changedBefore[key] = oldValue;
                changedAfter[key] = newValue;
            }
        }

        var rooms = (Rooms, Bathrooms, BuiltArea, UsableArea, PropertyType);
        var newRooms = (observation.Rooms, observation.Bathrooms, observation.BuiltArea, observation.UsableArea, observation.PropertyType);
        if (rooms != newRooms)
        {
            changedBefore["_structure"] = rooms.ToString();
            changedAfter["_structure"] = newRooms.ToString();
        }

        if (changedBefore.Count > 0)
        {
            Record(ListingEventType.FeaturesChanged, now, new FeaturesChangedPayload(changedBefore, changedAfter));
        }
    }

    private void DetectStatusChange(ListingObservation observation, DateTimeOffset now, bool wasMissing)
    {
        var targetStatus = wasMissing && observation.Status == ListingStatus.Active ? ListingStatus.Relisted : observation.Status;
        if (Status == targetStatus && Possession == observation.Possession)
        {
            return;
        }

        if (!wasMissing)
        {
            Record(ListingEventType.StatusChanged, now, new StatusChangedPayload(Status, targetStatus, Possession, observation.Possession));
        }

        Status = targetStatus;
        Possession = observation.Possession;
    }

    private void DetectAgencyChange(ListingObservation observation, DateTimeOffset now)
    {
        if (AgencyId == observation.AgencyId)
        {
            return;
        }

        Record(ListingEventType.AgencyChanged, now, new AgencyChangedPayload(AgencyId, observation.AgencyId));
    }

    private void Apply(ListingObservation observation)
    {
        Url = observation.Url;
        Title = observation.Title;
        PriceAmount = observation.Price?.Amount;
        BuiltSquareMetres = observation.BuiltArea?.SquareMetres;
        UsableSquareMetres = observation.UsableArea?.SquareMetres;
        Rooms = observation.Rooms;
        Bathrooms = observation.Bathrooms;
        PropertyType = observation.PropertyType;
        Municipality = observation.Municipality;
        Zone = observation.Zone;
        Location = observation.Location;
        LocationUtm = observation.Location is null ? null : UtmProjection.ToUtm31N(observation.Location);
        Description = observation.Description;
        CadastralReference = observation.CadastralReference;
        PublishedAt ??= observation.PublishedAt;
        AgencyId = observation.AgencyId;
        _features = new Dictionary<string, string>(observation.Features, StringComparer.Ordinal);
        HasElevator = FeatureFlag(FeatureKeys.Elevator);
        HasTerrace = FeatureFlag(FeatureKeys.Terrace);
        HasParking = FeatureFlag(FeatureKeys.Parking);
        YearBuilt = _features.TryGetValue(FeatureKeys.YearBuilt, out var year) && int.TryParse(year, out var parsed) ? parsed : null;
        IsNewBuild = FeatureFlag(FeatureKeys.NewBuild) == true;
        SyncPhotos(observation.PhotoUrls);
    }

    private bool? FeatureFlag(string key) =>
        _features.TryGetValue(key, out var value) ? string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) : null;

    private void SyncPhotos(IReadOnlyList<Uri> urls)
    {
        _photos.RemoveAll(photo => !urls.Contains(photo.Url));
        for (var position = 0; position < urls.Count; position++)
        {
            var existing = _photos.Find(photo => photo.Url == urls[position]);
            if (existing is null)
            {
                _photos.Add(new ListingPhoto(urls[position], position));
            }
            else
            {
                existing.MoveTo(position);
            }
        }
    }

    private void Record(ListingEventType type, DateTimeOffset now, object payload)
    {
        Raise(ListingEvent.Create(Id, type, now, payload));
    }
}

public enum ObservationOutcome
{
    Unchanged = 0,
    Changed = 1,
}
