using System.Globalization;
using System.Text.RegularExpressions;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Sources.Common;

public static partial class Parsing
{
    public static Money? ParseEuros(string? text)
    {
        if (text is null)
        {
            return null;
        }

        var digits = DigitsAndSeparators().Match(text.Replace(" ", " ", StringComparison.Ordinal));
        if (!digits.Success)
        {
            return null;
        }

        var normalized = digits.Value.Replace(".", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount) && amount > 0 ? Money.Euros(amount) : null;
    }

    public static decimal? ParseDecimal(string? text)
    {
        if (text is null)
        {
            return null;
        }

        var match = DecimalNumber().Match(text.Replace(" ", " ", StringComparison.Ordinal));
        if (!match.Success)
        {
            return null;
        }

        var value = match.Value;
        if (value.Contains(',', StringComparison.Ordinal) && value.Contains('.', StringComparison.Ordinal))
        {
            value = value.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        }
        else if (value.Contains(',', StringComparison.Ordinal))
        {
            value = value.Replace(',', '.');
        }
        else if (value.Count(c => c == '.') > 1 || ThousandsOnly().IsMatch(value))
        {
            value = value.Replace(".", string.Empty, StringComparison.Ordinal);
        }

        return decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    public static int? ParseInt(string? text)
    {
        if (text is null)
        {
            return null;
        }

        var match = Integer().Match(text);
        return match.Success && int.TryParse(match.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    public static string Collapse(string? text) => text is null ? string.Empty : Whitespace().Replace(text, " ").Trim();

    public static PropertyType MapPropertyType(string? label)
    {
        var text = Collapse(label).ToLowerInvariant();
        if (text.Length == 0)
        {
            return PropertyType.Other;
        }

        if (text.Contains("ático", StringComparison.Ordinal) || text.Contains("atico", StringComparison.Ordinal) || text.Contains("penthouse", StringComparison.Ordinal))
        {
            return PropertyType.Penthouse;
        }

        if (text.Contains("estudio", StringComparison.Ordinal) || text.Contains("studio", StringComparison.Ordinal))
        {
            return PropertyType.Studio;
        }

        if (text.Contains("piso", StringComparison.Ordinal) || text.Contains("apartamento", StringComparison.Ordinal) || text.Contains("planta baja", StringComparison.Ordinal) || text.Contains("dúplex", StringComparison.Ordinal) || text.Contains("duplex", StringComparison.Ordinal) || text.Contains("vivienda", StringComparison.Ordinal) || text.Contains("flat", StringComparison.Ordinal))
        {
            return PropertyType.Flat;
        }

        if (text.Contains("adosad", StringComparison.Ordinal) || text.Contains("pareado", StringComparison.Ordinal) || text.Contains("townhouse", StringComparison.Ordinal))
        {
            return PropertyType.Townhouse;
        }

        if (text.Contains("finca", StringComparison.Ordinal) || text.Contains("rústic", StringComparison.Ordinal) || text.Contains("rustic", StringComparison.Ordinal) || text.Contains("terreno", StringComparison.Ordinal) || text.Contains("solar", StringComparison.Ordinal) || text.Contains("parcela", StringComparison.Ordinal))
        {
            return text.Contains("finca", StringComparison.Ordinal) ? PropertyType.Finca : PropertyType.Land;
        }

        if (text.Contains("chalet", StringComparison.Ordinal) || text.Contains("casa", StringComparison.Ordinal) || text.Contains("villa", StringComparison.Ordinal) || text.Contains("unifamiliar", StringComparison.Ordinal) || text.Contains("house", StringComparison.Ordinal))
        {
            return PropertyType.House;
        }

        if (text.Contains("garaje", StringComparison.Ordinal) || text.Contains("parking", StringComparison.Ordinal) || text.Contains("trastero", StringComparison.Ordinal))
        {
            return PropertyType.Garage;
        }

        if (text.Contains("local", StringComparison.Ordinal) || text.Contains("oficina", StringComparison.Ordinal) || text.Contains("nave", StringComparison.Ordinal) || text.Contains("edificio", StringComparison.Ordinal) || text.Contains("hotel", StringComparison.Ordinal))
        {
            return PropertyType.Commercial;
        }

        return PropertyType.Other;
    }

    [GeneratedRegex(@"\d{1,3}(?:[.\s]\d{3})*(?:,\d+)?|\d+(?:,\d+)?")]
    private static partial Regex DigitsAndSeparators();

    [GeneratedRegex(@"\d+(?:[.,]\d+)*")]
    private static partial Regex DecimalNumber();

    [GeneratedRegex(@"^\d{1,3}(\.\d{3})+$")]
    private static partial Regex ThousandsOnly();

    [GeneratedRegex(@"\d+")]
    private static partial Regex Integer();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
