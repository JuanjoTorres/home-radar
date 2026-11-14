namespace HomeRadar.Domain.Listings;

public enum ListingEventType
{
    Created = 0,
    PriceChanged = 1,
    DescriptionChanged = 2,
    PhotoAdded = 3,
    PhotoRemoved = 4,
    PhotoReplaced = 5,
    FeaturesChanged = 6,
    StatusChanged = 7,
    Relisted = 8,
    CrossListed = 9,
    AgencyChanged = 10,
}
