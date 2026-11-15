using HomeRadar.Domain.Common;

namespace HomeRadar.Domain.Finance;

public sealed class ReferenceRate : Entity
{
    public const string EuriborTwelveMonths = "EURIBOR12M";

    private ReferenceRate()
    {
    }

    public ReferenceRate(string code, DateOnly month, decimal percent, string source, DateTimeOffset fetchedAt)
    {
        Code = code;
        Month = new DateOnly(month.Year, month.Month, 1);
        Percent = percent;
        Source = source;
        FetchedAt = fetchedAt;
    }

    public string Code { get; private init; } = string.Empty;

    public DateOnly Month { get; private init; }

    public decimal Percent { get; private init; }

    public string Source { get; private init; } = string.Empty;

    public DateTimeOffset FetchedAt { get; private init; }

    public string MonthLabel => Month.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
}
