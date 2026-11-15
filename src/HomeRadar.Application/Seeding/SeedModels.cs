using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;

namespace HomeRadar.Application.Seeding;

public sealed record FinancingProfileSeed(
    string Key,
    int Revision,
    string Name,
    decimal Savings,
    decimal NetMonthlyIncome,
    decimal ExistingMonthlyDebt,
    decimal MaxDebtToIncomeRatio,
    MortgageTerms Mortgage,
    BuyerProfile Buyer,
    bool IsDefault);

public sealed record SearchProfileSeed(
    string Key,
    int Revision,
    string Name,
    ListingFilter Filter,
    IReadOnlyList<AlertChannel> Channels,
    bool Priority,
    bool Enabled);

public sealed record SourceSeed(
    string Key,
    string Name,
    Domain.Sources.SourceKind Kind,
    Domain.ValueObjects.SellerType SellerType,
    string AdapterKey,
    TimeSpan PollInterval,
    TimeSpan? PriorityPollInterval,
    bool Priority,
    bool Enabled,
    string ConfigurationJson);

public sealed record TaxRuleSetSeed(TaxRuleDocument Document, DateOnly ValidFrom, int Revision, DateOnly? VerifiedOn, string SourceFile);

public sealed record BusinessConfigSeed(DateOnly ValidFrom, int Revision, BusinessConfig Config, string Source);
