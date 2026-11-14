using HomeRadar.Domain.Common;
using HomeRadar.Domain.ValueObjects;
using NetTopologySuite.Geometries;

namespace HomeRadar.Domain.Listings;

public sealed class Dwelling : Entity
{
    private Dwelling()
    {
    }

    public Dwelling(CadastralReference? cadastralReference, Point? location, Area? builtArea, int? rooms, PropertyType propertyType, decimal matchConfidence)
    {
        CadastralReference = cadastralReference;
        Location = location;
        BuiltArea = builtArea;
        Rooms = rooms;
        PropertyType = propertyType;
        MatchConfidence = matchConfidence;
    }

    public CadastralReference? CadastralReference { get; private set; }

    public Point? Location { get; private set; }

    public Area? BuiltArea { get; private set; }

    public int? Rooms { get; private set; }

    public PropertyType PropertyType { get; private set; }

    public decimal MatchConfidence { get; private set; }

    public bool ManuallyConfirmed { get; private set; }

    public void ConfirmManually() => ManuallyConfirmed = true;
}
