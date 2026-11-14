using System.Globalization;

namespace HomeRadar.Domain.ValueObjects;

public readonly record struct Money(decimal Amount, Currency Currency = Currency.EUR) : IComparable<Money>
{
    public const int Scale = 2;

    public static Money Euros(decimal amount) => new(Round(amount));

    public static Money Zero => new(0m);

    public static decimal Round(decimal amount) => Math.Round(amount, Scale, MidpointRounding.ToEven);

    public Money Rounded() => this with { Amount = Round(Amount) };

    public static Money operator +(Money left, Money right) => Combine(left, right, (a, b) => a + b);

    public static Money operator -(Money left, Money right) => Combine(left, right, (a, b) => a - b);

    public static Money operator *(Money money, decimal factor) => new(Round(money.Amount * factor), money.Currency);

    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;

    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public static Money Add(Money left, Money right) => left + right;

    public static Money Subtract(Money left, Money right) => left - right;

    public static Money Multiply(Money money, decimal factor) => money * factor;

    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");

    private static Money Combine(Money left, Money right, Func<decimal, decimal, decimal> operation)
    {
        EnsureSameCurrency(left, right);
        return new Money(Round(operation(left.Amount, right.Amount)), left.Currency);
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot combine {left.Currency} with {right.Currency}.");
        }
    }
}

public enum Currency
{
    EUR,
}
