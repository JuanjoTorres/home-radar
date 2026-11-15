using System.Globalization;
using System.Text;
using HomeRadar.Application.Ports;
using HomeRadar.Infrastructure.Http;

namespace HomeRadar.Infrastructure.ReferenceData;

public sealed class BdeEuriborFeed(IHttpClientFactory factory) : IEuriborFeed
{
    public static readonly Uri CsvUri = new("https://www.bde.es/webbe/es/estadisticas/compartido/datos/csv/be1901.csv");
    public const string SeriesCode = "D_1NBAF472";

    private static readonly Dictionary<string, int> Months = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ENE"] = 1,
        ["FEB"] = 2,
        ["MAR"] = 3,
        ["ABR"] = 4,
        ["MAY"] = 5,
        ["JUN"] = 6,
        ["JUL"] = 7,
        ["AGO"] = 8,
        ["SEP"] = 9,
        ["OCT"] = 10,
        ["NOV"] = 11,
        ["DIC"] = 12,
    };

    public async Task<EuriborObservation?> LatestAsync(CancellationToken cancellationToken)
    {
        using var client = factory.CreateClient(SourceHttpServiceCollectionExtensions.ClientName);
        var bytes = await client.GetByteArrayAsync(CsvUri, cancellationToken);
        return Parse(Encoding.Latin1.GetString(bytes), CsvUri);
    }

    public static EuriborObservation? Parse(string csv, Uri source)
    {
        var rows = csv.Split('\n').Select(line => Fields(line.TrimEnd('\r'))).Where(fields => fields.Count > 1).ToList();
        var header = rows.FirstOrDefault(fields => fields.Contains(SeriesCode));
        if (header is null)
        {
            return null;
        }

        var column = header.IndexOf(SeriesCode);
        EuriborObservation? latest = null;
        foreach (var fields in rows)
        {
            if (fields.Count <= column || !TryMonth(fields[0], out var month))
            {
                continue;
            }

            if (decimal.TryParse(fields[column], NumberStyles.Number, CultureInfo.InvariantCulture, out var percent) && (latest is null || month > latest.Month))
            {
                latest = new EuriborObservation(month, percent, source);
            }
        }

        return latest;
    }

    private static bool TryMonth(string label, out DateOnly month)
    {
        month = default;
        var parts = label.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !Months.TryGetValue(parts[0], out var number) || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var year))
        {
            return false;
        }

        month = new DateOnly(year, number, 1);
        return true;
    }

    private static List<string> Fields(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var quoted = false;
        foreach (var character in line)
        {
            if (character == '"')
            {
                quoted = !quoted;
            }
            else if (character == ',' && !quoted)
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        fields.Add(current.ToString().Trim());
        return fields;
    }
}
