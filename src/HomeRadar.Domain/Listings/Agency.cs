using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Listings;

public sealed class Agency : Entity
{
    private Agency()
    {
    }

    public Agency(string name, Uri website, string? municipality, string? crmKey, Guid? sourceId)
    {
        Name = name;
        Website = website;
        Municipality = municipality;
        CrmKey = crmKey;
        SourceId = sourceId;
    }

    public string Name { get; private set; } = string.Empty;

    public Uri Website { get; private set; } = null!;

    public string? Municipality { get; private set; }

    public string? CrmKey { get; private set; }

    public Guid? SourceId { get; private set; }

    public void AttachSource(Guid sourceId) => SourceId = sourceId;
}
