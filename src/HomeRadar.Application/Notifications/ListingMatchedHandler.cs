using System.Text.Json;
using HomeRadar.Application.Outbox;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Outbox;
using HomeRadar.Domain.Profiles;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.Notifications;

public sealed class ListingMatchedHandler(IHomeRadarDbContext db, IEnumerable<INotificationChannel> channels) : IOutboxHandler
{
    public string MessageType => OutboxMessageTypes.ListingMatched;

    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<ListingMatchedMessage>(message.Payload, PayloadSerialization.Options)!;
        var listing = await db.Listings.AsNoTracking().Include(l => l.Photos).SingleAsync(l => l.Id == payload.ListingId, cancellationToken);
        var source = await db.Sources.AsNoTracking().SingleAsync(s => s.Id == listing.SourceId, cancellationToken);
        var profiles = await db.SearchProfiles.AsNoTracking().Where(p => payload.SearchProfileIds.Contains(p.Id)).ToListAsync(cancellationToken);

        var text = ListingMessageFormatter.NewListing(listing, source.Name, profiles.Select(p => p.Name).ToList());
        var photo = listing.Photos.OrderBy(p => p.Position).FirstOrDefault()?.Url;
        var notification = new Notification(message.Id, listing.Title, text, listing.Url, photo);

        var wanted = profiles.SelectMany(p => p.Channels).Distinct().ToList();
        foreach (var channel in channels.Where(channel => wanted.Contains(channel.Channel)))
        {
            await channel.SendAsync(notification, cancellationToken);
        }
    }
}

public sealed class WatchlistChangedHandler(IHomeRadarDbContext db, IEnumerable<INotificationChannel> channels) : IOutboxHandler
{
    public string MessageType => OutboxMessageTypes.WatchlistChanged;

    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<WatchlistChangedMessage>(message.Payload, PayloadSerialization.Options)!;
        var listing = await db.Listings.AsNoTracking().SingleAsync(l => l.Id == payload.ListingId, cancellationToken);
        var events = await db.ListingEvents.AsNoTracking().Where(e => payload.EventIds.Contains(e.Id)).OrderBy(e => e.OccurredAt).ToListAsync(cancellationToken);

        var notification = new Notification(message.Id, listing.Title, ListingMessageFormatter.Changed(listing, events), listing.Url, null);
        foreach (var channel in channels.Where(channel => channel.Channel == AlertChannel.Telegram))
        {
            await channel.SendAsync(notification, cancellationToken);
        }
    }
}

public sealed class SourceHealthHandler(IEnumerable<INotificationChannel> channels) : IOutboxHandler
{
    public string MessageType => OutboxMessageTypes.SourceHealth;

    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<SourceHealthMessage>(message.Payload, PayloadSerialization.Options)!;
        var text = $"🩺 Fuente {payload.SourceKey}: {payload.Status}\n{payload.Detail}";
        var notification = new Notification(message.Id, $"Fuente {payload.SourceKey}", text, null, null);
        foreach (var channel in channels.Where(channel => channel.Channel == AlertChannel.Telegram))
        {
            await channel.SendAsync(notification, cancellationToken);
        }
    }
}

public sealed class ListingEventWebhookHandler : IOutboxHandler
{
    public string MessageType => OutboxMessageTypes.ListingEvent;

    public Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
}
