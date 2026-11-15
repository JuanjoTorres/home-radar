using HomeRadar.Domain.Common;
using HomeRadar.Domain.Finance;

namespace HomeRadar.Domain.Configuration;

public sealed class BusinessConfigVersion : Entity
{
    private BusinessConfig? _config;

    private BusinessConfigVersion()
    {
    }

    public BusinessConfigVersion(DateOnly validFrom, int revision, BusinessConfig config, string sourceDescription, DateTimeOffset loadedAt)
    {
        ValidFrom = validFrom;
        Revision = revision;
        PayloadJson = config.ToJson();
        SourceDescription = sourceDescription;
        LoadedAt = loadedAt;
        _config = config;
    }

    public DateOnly ValidFrom { get; private init; }

    public int Revision { get; private init; }

    public string PayloadJson { get; private init; } = "{}";

    public string SourceDescription { get; private init; } = string.Empty;

    public DateTimeOffset LoadedAt { get; private init; }

    public BusinessConfig Config => _config ??= BusinessConfig.Parse(PayloadJson);
}
