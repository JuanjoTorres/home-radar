namespace HomeRadar.Domain.ValueObjects;

public readonly record struct Percentage(decimal Value)
{
    public static Percentage Of(decimal part, decimal whole) =>
        whole == 0 ? new Percentage(0) : new Percentage(Math.Round(part / whole * 100m, 2, MidpointRounding.ToEven));

    public decimal AsFraction => Value / 100m;
}

public readonly record struct InterestRate(decimal AnnualPercent)
{
    public decimal MonthlyFraction => AnnualPercent / 100m / 12m;
}
