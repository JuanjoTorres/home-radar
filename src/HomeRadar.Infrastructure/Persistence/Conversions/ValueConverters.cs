using System.Text.Json;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HomeRadar.Infrastructure.Persistence.Conversions;

internal static class ValueConverters
{
    public static readonly ValueConverter<Money, decimal> Money = new(money => money.Amount, amount => new Money(amount));

    public static readonly ValueConverter<Money?, decimal?> NullableMoney = new(
        money => money.HasValue ? money.Value.Amount : null,
        amount => amount.HasValue ? new Money(amount.Value) : null);

    public static readonly ValueConverter<Area?, decimal?> NullableArea = new(
        area => area.HasValue ? area.Value.SquareMetres : null,
        value => value.HasValue ? new Area(value.Value) : null);

    public static readonly ValueConverter<Area, decimal> Area = new(area => area.SquareMetres, value => new Area(value));

    public static readonly ValueConverter<ContentHash, string> ContentHash = new(hash => hash.Value, value => new ContentHash(value));

    public static readonly ValueConverter<CadastralReference?, string?> NullableCadastralReference = new(
        reference => reference.HasValue ? reference.Value.Value : null,
        value => value == null ? null : Domain.ValueObjects.CadastralReference.Parse(value));

    public static readonly ValueConverter<Uri, string> Uri = new(uri => uri.ToString(), value => new Uri(value));

    public static readonly ValueConverter<Uri?, string?> NullableUri = new(uri => uri == null ? null : uri.ToString(), value => value == null ? null : new Uri(value));

    public static readonly ValueConverter<ulong?, long?> NullableUInt64 = new(
        value => value.HasValue ? unchecked((long)value.Value) : null,
        value => value.HasValue ? unchecked((ulong)value.Value) : null);

    public static ValueConverter<T, string> Json<T>()
        where T : class =>
        new(value => JsonSerializer.Serialize(value, PayloadSerialization.Options), value => JsonSerializer.Deserialize<T>(value, PayloadSerialization.Options)!);

    public static ValueComparer<T> JsonComparer<T>()
        where T : class =>
        new(
            (left, right) => JsonSerializer.Serialize(left, PayloadSerialization.Options) == JsonSerializer.Serialize(right, PayloadSerialization.Options),
            value => JsonSerializer.Serialize(value, PayloadSerialization.Options).GetHashCode(StringComparison.Ordinal),
            value => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, PayloadSerialization.Options), PayloadSerialization.Options)!);

    public static readonly ValueComparer<IReadOnlyDictionary<string, string>> DictionaryComparer = new(
        (left, right) => left!.Count == right!.Count && !left.Except(right).Any(),
        value => value.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.Key.GetHashCode(StringComparison.Ordinal), pair.Value.GetHashCode(StringComparison.Ordinal))),
        value => new Dictionary<string, string>(value, StringComparer.Ordinal));
}
