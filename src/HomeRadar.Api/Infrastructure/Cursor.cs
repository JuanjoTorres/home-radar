using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace HomeRadar.Api.Infrastructure;

/// <summary>Opaque keyset cursor: sort value plus id, base64url-encoded.</summary>
public sealed record Cursor(string SortValue, Guid Id)
{
    public string Encode() => Base64Url.EncodeToString(Encoding.UTF8.GetBytes($"{SortValue}|{Id:N}"));

    public static Cursor? Decode(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return null;
        }

        try
        {
            var text = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(encoded));
            var separator = text.LastIndexOf('|');
            return separator < 0 ? null : new Cursor(text[..separator], Guid.ParseExact(text[(separator + 1)..], "N"));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public decimal? AsDecimal() => decimal.TryParse(SortValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

    public DateTimeOffset? AsTimestamp() => DateTimeOffset.TryParse(SortValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value) ? value : null;

    public static string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    public static string Format(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);
}
