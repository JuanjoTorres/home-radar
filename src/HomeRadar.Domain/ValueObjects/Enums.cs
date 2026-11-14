namespace HomeRadar.Domain.ValueObjects;

public enum PossessionStatus
{
    Unknown = 0,
    Vacant = 1,
    Occupied = 2,
    RentedUntil = 3,
    NoPossession = 4,
}

public enum PropertyType
{
    Other = 0,
    Flat = 1,
    House = 2,
    Townhouse = 3,
    Finca = 4,
    Penthouse = 5,
    Studio = 6,
    Garage = 7,
    Commercial = 8,
    Land = 9,
}

public enum SellerType
{
    Servicer = 0,
    Bank = 1,
    Agency = 2,
    Auction = 3,
    Private = 4,
}
