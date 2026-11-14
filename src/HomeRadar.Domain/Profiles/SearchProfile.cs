using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Profiles;

public sealed class SearchProfile : Entity
{
    private SearchProfile()
    {
    }

    public SearchProfile(string key, int revision, string name, ListingFilter filter, IReadOnlyList<AlertChannel> channels, bool priority, bool enabled)
    {
        Key = key;
        Revision = revision;
        Name = name;
        Filter = filter;
        Channels = channels.ToList();
        Priority = priority;
        Enabled = enabled;
    }

    public string Key { get; private set; } = string.Empty;

    public int Revision { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ListingFilter Filter { get; private set; } = new();

    public List<AlertChannel> Channels { get; private set; } = [];

    public bool Priority { get; private set; }

    public bool Enabled { get; private set; } = true;

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }

    public void Update(int revision, string name, ListingFilter filter, IReadOnlyList<AlertChannel> channels, bool priority, bool enabled, DateTimeOffset now)
    {
        Revision = revision;
        Name = name;
        Filter = filter;
        Channels = channels.ToList();
        Priority = priority;
        Enabled = enabled;
        UpdatedAt = now;
    }
}

public enum AlertChannel
{
    Telegram = 0,
    Email = 1,
    Webhook = 2,
}
