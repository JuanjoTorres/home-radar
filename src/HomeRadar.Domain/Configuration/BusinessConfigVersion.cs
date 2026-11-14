using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Configuration;

public sealed class BusinessConfigVersion : Entity
{
    private BusinessConfigVersion()
    {
    }

    public BusinessConfigVersion(DateOnly validFrom, string payloadJson, string sourceDescription)
    {
        ValidFrom = validFrom;
        PayloadJson = payloadJson;
        SourceDescription = sourceDescription;
    }

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidTo { get; private set; }

    public string PayloadJson { get; private set; } = "{}";

    public string SourceDescription { get; private set; } = string.Empty;

    public void CloseOn(DateOnly validTo) => ValidTo = validTo;
}
