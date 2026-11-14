using HomeRadar.Domain.Common;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Profiles;

public sealed class FinancingProfile : Entity
{
    private FinancingProfile()
    {
    }

    public FinancingProfile(string key, int revision, string name, Money savings, Money netMonthlyIncome, Money existingMonthlyDebt, decimal maxDebtToIncomeRatio, MortgageTerms mortgage, BuyerProfile buyer, bool isDefault)
    {
        Key = key;
        Revision = revision;
        Name = name;
        Savings = savings;
        NetMonthlyIncome = netMonthlyIncome;
        ExistingMonthlyDebt = existingMonthlyDebt;
        MaxDebtToIncomeRatio = maxDebtToIncomeRatio;
        Mortgage = mortgage;
        Buyer = buyer;
        IsDefault = isDefault;
    }

    public string Key { get; private set; } = string.Empty;

    public int Revision { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Money Savings { get; private set; }

    public Money NetMonthlyIncome { get; private set; }

    public Money ExistingMonthlyDebt { get; private set; }

    public decimal MaxDebtToIncomeRatio { get; private set; }

    public MortgageTerms Mortgage { get; private set; } = null!;

    public BuyerProfile Buyer { get; private set; } = null!;

    public bool IsDefault { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }

    public void Update(int revision, string name, Money savings, Money netMonthlyIncome, Money existingMonthlyDebt, decimal maxDebtToIncomeRatio, MortgageTerms mortgage, BuyerProfile buyer, bool isDefault, DateTimeOffset now)
    {
        Revision = revision;
        Name = name;
        Savings = savings;
        NetMonthlyIncome = netMonthlyIncome;
        ExistingMonthlyDebt = existingMonthlyDebt;
        MaxDebtToIncomeRatio = maxDebtToIncomeRatio;
        Mortgage = mortgage;
        Buyer = buyer;
        IsDefault = isDefault;
        UpdatedAt = now;
    }
}

public sealed record MortgageTerms(
    MortgageType Type,
    decimal MaxLoanToValue,
    decimal? NominalRatePercent,
    decimal? SpreadPercent,
    int? FixedMonths,
    int TermMonths);

public sealed record BuyerProfile(
    int Age,
    bool FirstHome,
    bool PrimaryResidence,
    bool ResidentInBalearics,
    int? YearsResidentInBalearics,
    bool LargeFamily,
    bool SingleParentFamily,
    int? DisabilityPercent,
    decimal? TaxableIncomeBase,
    decimal? NetWorth);

public enum MortgageType
{
    Fixed = 0,
    Variable = 1,
    Mixed = 2,
}
