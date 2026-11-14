using System.Text.RegularExpressions;

namespace HomeRadar.Domain.ValueObjects;

public readonly partial record struct CadastralReference
{
    private CadastralReference(string value) => Value = value;

    public string Value { get; }

    public static bool TryParse(string? candidate, out CadastralReference reference)
    {
        reference = default;
        if (candidate is null)
        {
            return false;
        }

        var normalized = candidate.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        if (!Pattern().IsMatch(normalized))
        {
            return false;
        }

        reference = new CadastralReference(normalized);
        return true;
    }

    public static CadastralReference Parse(string candidate) =>
        TryParse(candidate, out var reference) ? reference : throw new FormatException($"'{candidate}' is not a cadastral reference.");

    public string ParcelPart => Value[..14];

    public override string ToString() => Value;

    [GeneratedRegex("^[0-9A-Z]{20}$")]
    private static partial Regex Pattern();
}
