using HomeRadar.Domain.Profiles;

namespace HomeRadar.Api.Contracts;

/// <summary>Mortgage offer assumed for affordability calculations.</summary>
public sealed record MortgageTermsDto
{
    /// <summary>Fixed, Variable or Mixed.</summary>
    public required string Type { get; init; }

    /// <summary>Maximum loan-to-value as a fraction (0.5–1.0). 0.80 is standard; up to 1.00 with an ICO/IBAVI guarantee.</summary>
    public required decimal MaxLoanToValue { get; init; }

    /// <summary>Annual nominal rate in % for Fixed, or the fixed leg of Mixed, e.g. 2.9.</summary>
    public decimal? NominalRatePercent { get; init; }

    /// <summary>Spread over Euríbor in % for Variable or Mixed.</summary>
    public decimal? SpreadPercent { get; init; }

    /// <summary>Length of the fixed leg in months for Mixed mortgages.</summary>
    public int? FixedMonths { get; init; }

    /// <summary>Term in months (60–480).</summary>
    public required int TermMonths { get; init; }

    /// <summary>Opening fee in % of the loan, if the bank charges one.</summary>
    public decimal? OpeningFeePercent { get; init; }

    public MortgageTerms ToDomain() => new(Enum.Parse<MortgageType>(Type, ignoreCase: true), MaxLoanToValue, NominalRatePercent, SpreadPercent, FixedMonths, TermMonths, OpeningFeePercent);

    public MortgageTerms? ToDomainOrNull() => Enum.TryParse<MortgageType>(Type, ignoreCase: true, out var type)
        ? new MortgageTerms(type, MaxLoanToValue, NominalRatePercent, SpreadPercent, FixedMonths, TermMonths, OpeningFeePercent)
        : null;

    public static MortgageTermsDto From(MortgageTerms terms) => new()
    {
        Type = terms.Type.ToString(),
        MaxLoanToValue = terms.MaxLoanToValue,
        NominalRatePercent = terms.NominalRatePercent,
        SpreadPercent = terms.SpreadPercent,
        FixedMonths = terms.FixedMonths,
        TermMonths = terms.TermMonths,
        OpeningFeePercent = terms.OpeningFeePercent,
    };
}

/// <summary>Personal circumstances that decide which tax reductions and aids apply.</summary>
public sealed record BuyerProfileDto
{
    /// <summary>Age in years (18–99) of the oldest buyer: age limits of reliefs and aid are checked against it.</summary>
    public required int Age { get; init; }

    /// <summary>First home purchase.</summary>
    public bool FirstHome { get; init; } = true;

    /// <summary>Will be the primary residence.</summary>
    public bool PrimaryResidence { get; init; } = true;

    /// <summary>Currently resident in the Balearic Islands.</summary>
    public bool ResidentInBalearics { get; init; } = true;

    /// <summary>Years of residence in the Balearic Islands.</summary>
    public int? YearsResidentInBalearics { get; init; }

    /// <summary>Large family (familia numerosa).</summary>
    public bool LargeFamily { get; init; }

    /// <summary>Single-parent family.</summary>
    public bool SingleParentFamily { get; init; }

    /// <summary>Recognised disability in % (0–100).</summary>
    public int? DisabilityPercent { get; init; }

    /// <summary>IRPF taxable base in EUR, needed for the 100 % ITP bonus (limit 52,800 € individual).</summary>
    public decimal? TaxableIncomeBase { get; init; }

    /// <summary>Net worth in EUR, needed for the ICO guarantee (≤ 150,000 €).</summary>
    public decimal? NetWorth { get; init; }

    /// <summary>Owns (≥ 50 %) another home now. When omitted, a first-home buyer is assumed to own none.</summary>
    public bool? OwnsOtherHome { get; init; }

    /// <summary>Years of legal residence in Spain (ICO requires 2).</summary>
    public int? YearsResidentInSpain { get; init; }

    /// <summary>Files a joint IRPF return (limit 84,480 € instead of 52,800 € for the 100 % bonus).</summary>
    public bool? JointTaxReturn { get; init; }

    /// <summary>Gross annual income of the household in EUR, needed for the ICO and IBAVI guarantees and the 10,000 € grant.</summary>
    public decimal? GrossAnnualIncome { get; init; }

    /// <summary>Number of buyers (1–2).</summary>
    public int BuyerCount { get; init; } = 1;

    /// <summary>Has dependent children (ICO guarantee without age limit).</summary>
    public bool HasDependentChildren { get; init; }

    /// <summary>Applies the IRPF disability minimum for ascendants or descendants (2 % ITP).</summary>
    public bool DisabilityMinimumForRelatives { get; init; }

    public BuyerProfile ToDomain() => new(Age, FirstHome, PrimaryResidence, ResidentInBalearics, YearsResidentInBalearics, LargeFamily, SingleParentFamily, DisabilityPercent, TaxableIncomeBase, NetWorth, OwnsOtherHome, YearsResidentInSpain, JointTaxReturn, GrossAnnualIncome, BuyerCount, HasDependentChildren, DisabilityMinimumForRelatives);

    public static BuyerProfileDto From(BuyerProfile buyer) => new()
    {
        Age = buyer.Age,
        FirstHome = buyer.FirstHome,
        PrimaryResidence = buyer.PrimaryResidence,
        ResidentInBalearics = buyer.ResidentInBalearics,
        YearsResidentInBalearics = buyer.YearsResidentInBalearics,
        LargeFamily = buyer.LargeFamily,
        SingleParentFamily = buyer.SingleParentFamily,
        DisabilityPercent = buyer.DisabilityPercent,
        TaxableIncomeBase = buyer.TaxableIncomeBase,
        NetWorth = buyer.NetWorth,
        OwnsOtherHome = buyer.OwnsOtherHome,
        YearsResidentInSpain = buyer.YearsResidentInSpain,
        JointTaxReturn = buyer.JointTaxReturn,
        GrossAnnualIncome = buyer.GrossAnnualIncome,
        BuyerCount = buyer.BuyerCount,
        HasDependentChildren = buyer.HasDependentChildren,
        DisabilityMinimumForRelatives = buyer.DisabilityMinimumForRelatives,
    };
}

/// <summary>Input to create or replace a financing profile.</summary>
public sealed record FinancingProfileInputDto
{
    /// <summary>Stable key, e.g. default.</summary>
    public required string Key { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Savings available for deposit and costs.</summary>
    public required MoneyDto Savings { get; init; }

    /// <summary>Net monthly income of all borrowers.</summary>
    public required MoneyDto NetMonthlyIncome { get; init; }

    /// <summary>Existing monthly debt payments.</summary>
    public MoneyDto ExistingMonthlyDebt { get; init; } = new(0);

    /// <summary>Maximum debt-to-income ratio (0.1–0.6), default 0.35.</summary>
    public decimal MaxDebtToIncomeRatio { get; init; } = 0.35m;

    /// <summary>Mortgage assumptions.</summary>
    public required MortgageTermsDto Mortgage { get; init; }

    /// <summary>Buyer circumstances.</summary>
    public required BuyerProfileDto Buyer { get; init; }

    /// <summary>Use this profile when none is specified.</summary>
    public bool IsDefault { get; init; }
}

/// <summary>Stored financing profile.</summary>
public sealed record FinancingProfileDto
{
    /// <summary>Identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>Stable key.</summary>
    public required string Key { get; init; }

    /// <summary>Revision applied from seed files; API updates do not change it.</summary>
    public required int Revision { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Savings available.</summary>
    public required MoneyDto Savings { get; init; }

    /// <summary>Net monthly income.</summary>
    public required MoneyDto NetMonthlyIncome { get; init; }

    /// <summary>Existing monthly debt.</summary>
    public required MoneyDto ExistingMonthlyDebt { get; init; }

    /// <summary>Maximum debt-to-income ratio.</summary>
    public required decimal MaxDebtToIncomeRatio { get; init; }

    /// <summary>Mortgage assumptions.</summary>
    public required MortgageTermsDto Mortgage { get; init; }

    /// <summary>Buyer circumstances.</summary>
    public required BuyerProfileDto Buyer { get; init; }

    /// <summary>Default profile flag.</summary>
    public required bool IsDefault { get; init; }

    /// <summary>Last update (UTC).</summary>
    public required DateTimeOffset UpdatedAt { get; init; }

    public static FinancingProfileDto From(FinancingProfile profile) => new()
    {
        Id = profile.Id,
        Key = profile.Key,
        Revision = profile.Revision,
        Name = profile.Name,
        Savings = MoneyDto.From(profile.Savings),
        NetMonthlyIncome = MoneyDto.From(profile.NetMonthlyIncome),
        ExistingMonthlyDebt = MoneyDto.From(profile.ExistingMonthlyDebt),
        MaxDebtToIncomeRatio = profile.MaxDebtToIncomeRatio,
        Mortgage = MortgageTermsDto.From(profile.Mortgage),
        Buyer = BuyerProfileDto.From(profile.Buyer),
        IsDefault = profile.IsDefault,
        UpdatedAt = profile.UpdatedAt,
    };
}

/// <summary>Input to create or replace a saved search.</summary>
public sealed record SearchProfileInputDto
{
    /// <summary>Stable key, e.g. inca-centro.</summary>
    public required string Key { get; init; }

    /// <summary>Display name used in alerts.</summary>
    public required string Name { get; init; }

    /// <summary>Filters; the same schema as <c>searchListings</c> minus sort and pagination.</summary>
    public required ListingSearchRequestDto Filter { get; init; }

    /// <summary>Alert channels: Telegram, Email, Webhook.</summary>
    public IReadOnlyList<string> Channels { get; init; } = ["Telegram"];

    /// <summary>Priority searches drive the 15-minute quick scans of their sources.</summary>
    public bool Priority { get; init; }

    /// <summary>Disabled searches keep their definition but raise no alerts.</summary>
    public bool Enabled { get; init; } = true;
}

/// <summary>Stored saved search.</summary>
public sealed record SearchProfileDto
{
    /// <summary>Identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>Stable key.</summary>
    public required string Key { get; init; }

    /// <summary>Seed revision.</summary>
    public required int Revision { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Filters.</summary>
    public required ListingSearchRequestDto Filter { get; init; }

    /// <summary>Alert channels.</summary>
    public required IReadOnlyList<string> Channels { get; init; }

    /// <summary>Priority flag.</summary>
    public required bool Priority { get; init; }

    /// <summary>Enabled flag.</summary>
    public required bool Enabled { get; init; }

    /// <summary>Last update (UTC).</summary>
    public required DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Input to start following a listing.</summary>
/// <param name="ListingId">Listing to follow.</param>
/// <param name="Notes">Free-text notes.</param>
public sealed record WatchlistItemInputDto(Guid ListingId, string? Notes);

/// <summary>Partial update of a watchlist item.</summary>
/// <param name="Status">Seen, Visited, OfferMade or Discarded.</param>
/// <param name="Notes">Free-text notes; omit to keep the current ones.</param>
public sealed record WatchlistItemPatchDto(string? Status, string? Notes);

/// <summary>A followed listing.</summary>
public sealed record WatchlistItemDto
{
    /// <summary>Identifier.</summary>
    public required Guid Id { get; init; }

    /// <summary>The followed listing.</summary>
    public required ListingSummaryDto Listing { get; init; }

    /// <summary>Seen, Visited, OfferMade or Discarded.</summary>
    public required string Status { get; init; }

    /// <summary>Notes.</summary>
    public string? Notes { get; init; }

    /// <summary>When it was added (UTC).</summary>
    public required DateTimeOffset AddedAt { get; init; }

    /// <summary>Last update (UTC).</summary>
    public required DateTimeOffset UpdatedAt { get; init; }
}
