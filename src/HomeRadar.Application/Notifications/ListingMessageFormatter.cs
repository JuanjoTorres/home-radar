using System.Globalization;
using System.Text;
using HomeRadar.Domain.Listings;

namespace HomeRadar.Application.Notifications;

public static class ListingMessageFormatter
{
    private static readonly NumberFormatInfo Spanish = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalSeparator = ",",
        NegativeSign = "-",
        PositiveSign = "+",
    };

    public static string NewListing(Listing listing, string sourceName, IReadOnlyList<string> matchedProfiles)
    {
        var text = new StringBuilder();
        text.Append("🏠 Nuevo: ").Append(Escape(listing.Title)).Append('\n');
        text.Append(FormatPrice(listing)).Append('\n');
        if (listing.Municipality is { Length: > 0 })
        {
            text.Append("📍 ").Append(Escape(listing.Municipality));
            if (listing.Zone is { Length: > 0 })
            {
                text.Append(" · ").Append(Escape(listing.Zone));
            }

            text.Append('\n');
        }

        text.Append(FormatStructure(listing)).Append('\n');
        if (listing.Possession is not Domain.ValueObjects.PossessionStatus.Unknown and not Domain.ValueObjects.PossessionStatus.Vacant)
        {
            text.Append("⚠️ Posesión: ").Append(listing.Possession).Append('\n');
        }

        text.Append("Fuente: ").Append(Escape(sourceName)).Append('\n');
        if (matchedProfiles.Count > 0)
        {
            text.Append("Alertas: ").Append(Escape(string.Join(", ", matchedProfiles))).Append('\n');
        }

        text.Append(listing.Url);
        return text.ToString();
    }

    public static string Changed(Listing listing, IReadOnlyList<ListingEvent> events)
    {
        var text = new StringBuilder();
        text.Append("🔔 Cambio en seguimiento: ").Append(Escape(listing.Title)).Append('\n');
        foreach (var listingEvent in events)
        {
            text.Append("• ").Append(Describe(listingEvent)).Append('\n');
        }

        text.Append(FormatPrice(listing)).Append('\n').Append(listing.Url);
        return text.ToString();
    }

    public static string Describe(ListingEvent listingEvent) => listingEvent.Type switch
    {
        ListingEventType.PriceChanged => DescribePrice(listingEvent.ReadPayload<PriceChangedPayload>()),
        ListingEventType.DescriptionChanged => "Descripción modificada",
        ListingEventType.PhotoAdded => "Foto añadida",
        ListingEventType.PhotoRemoved => "Foto eliminada",
        ListingEventType.PhotoReplaced => "Foto sustituida",
        ListingEventType.FeaturesChanged => "Características modificadas",
        ListingEventType.StatusChanged => DescribeStatus(listingEvent.ReadPayload<StatusChangedPayload>()),
        ListingEventType.Relisted => "Ha vuelto a aparecer",
        ListingEventType.CrossListed => "Publicado también en otra fuente",
        ListingEventType.AgencyChanged => "Cambio de agencia",
        ListingEventType.Created => "Publicado",
        _ => listingEvent.Type.ToString(),
    };

    private static string DescribePrice(PriceChangedPayload payload)
    {
        var before = payload.Before?.Amount.ToString("N0", Spanish) ?? "?";
        var after = payload.After?.Amount.ToString("N0", Spanish) ?? "?";
        var arrow = payload.DeltaAmount < 0 ? "⬇️" : "⬆️";
        var percent = payload.DeltaPercent is { } p ? $" ({p.ToString("+0.0;-0.0", Spanish)} %)" : string.Empty;
        return $"{arrow} Precio {before} € → {after} €{percent}";
    }

    private static string DescribeStatus(StatusChangedPayload payload) => $"Estado {payload.Before} → {payload.After}";

    private static string FormatPrice(Listing listing)
    {
        if (listing.Price is not { } price)
        {
            return "💶 Precio no publicado";
        }

        var text = $"💶 {price.Amount.ToString("N0", Spanish)} €";
        if (listing.PricePerSquareMetre is { } ppm2)
        {
            text += $" · {ppm2.ToString("N0", Spanish)} €/m²";
        }

        if (listing.PriceDropCount > 0)
        {
            text += $" · {listing.PriceDropCount} bajada(s), -{listing.CumulativeDropPercent.ToString("0.0", Spanish)} %";
        }

        return text;
    }

    private static string FormatStructure(Listing listing)
    {
        var parts = new List<string>();
        if (listing.BuiltArea is { } area)
        {
            parts.Add($"{area.SquareMetres.ToString("0", Spanish)} m²");
        }

        if (listing.Rooms is { } rooms)
        {
            parts.Add($"{rooms} hab.");
        }

        if (listing.Bathrooms is { } baths)
        {
            parts.Add($"{baths} baños");
        }

        parts.Add(listing.PropertyType.ToString());
        return string.Join(" · ", parts);
    }

    private static string Escape(string text) => text.Replace('<', '‹').Replace('>', '›');
}
