using System.Text.Json;
using HomeRadar.Application.Seeding;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.Profiles;
using HomeRadar.Domain.Sources;
using HomeRadar.Domain.ValueObjects;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HomeRadar.Infrastructure.Seeding;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string Directory { get; set; } = "config/seed";

    public bool Enabled { get; set; } = true;
}

/// <summary>Reads the YAML seed files (ADR-0006) into the application seed records; `*.local.yaml` files override or extend the committed ones.</summary>
public static class YamlSeedLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static IReadOnlyList<SourceSeed> LoadSources(string directory) =>
        LoadAll<SourceSeedFile>(directory, "sources").SelectMany(file => file.Sources).Select(ToSeed).ToList();

    public static IReadOnlyList<FinancingProfileSeed> LoadFinancingProfiles(string directory) =>
        LoadAll<FinancingProfilesFile>(directory, "financing-profiles").SelectMany(file => file.Profiles).Select(ToSeed).ToList();

    public static IReadOnlyList<SearchProfileSeed> LoadSearchProfiles(string directory) =>
        LoadAll<SearchProfilesFile>(directory, "search-profiles").SelectMany(file => file.Profiles).Select(ToSeed).ToList();

    public static IReadOnlyList<TaxRuleSetSeed> LoadTaxRuleSets(string directory)
    {
        var folder = Path.Combine(directory, "tax-rules");
        if (!Directory.Exists(folder))
        {
            return [];
        }

        return Directory.EnumerateFiles(folder, "*.yaml")
            .Order(StringComparer.Ordinal)
            .Select(path => ToTaxRuleSetSeed(path, ToJson(File.ReadAllText(path))))
            .ToList();
    }

    public static IReadOnlyList<BusinessConfigSeed> LoadBusinessConfigs(string directory)
    {
        var seeds = new List<BusinessConfigSeed>();
        foreach (var name in new[] { "business-config.yaml", "business-config.local.yaml" })
        {
            var path = Path.Combine(directory, name);
            if (!File.Exists(path))
            {
                continue;
            }

            using var json = JsonDocument.Parse(ToJson(File.ReadAllText(path)));
            foreach (var version in json.RootElement.GetProperty("versions").EnumerateArray())
            {
                seeds.Add(new BusinessConfigSeed(
                    DateOnly.Parse(version.GetProperty("validFrom").GetString()!, System.Globalization.CultureInfo.InvariantCulture),
                    version.GetProperty("revision").GetInt32(),
                    BusinessConfig.Parse(version.GetProperty("config").GetRawText()),
                    version.TryGetProperty("source", out var source) ? source.GetString() ?? name : name));
            }
        }

        return seeds;
    }

    private static TaxRuleSetSeed ToTaxRuleSetSeed(string path, string json)
    {
        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        var document = JsonSerializer.Deserialize<TaxRuleDocument>(json, FinanceJson.Options)!;
        return new TaxRuleSetSeed(
            document,
            DateOnly.Parse(root.GetProperty("validFrom").GetString()!, System.Globalization.CultureInfo.InvariantCulture),
            root.GetProperty("revision").GetInt32(),
            root.TryGetProperty("verifiedOn", out var verified) ? DateOnly.Parse(verified.GetString()!, System.Globalization.CultureInfo.InvariantCulture) : null,
            Path.GetFileName(path));
    }

    private static string ToJson(string yaml) => JsonSerializer.Serialize(Normalize(Deserializer.Deserialize<object>(yaml)!));

    private static IEnumerable<T> LoadAll<T>(string directory, string stem)
    {
        foreach (var name in new[] { $"{stem}.yaml", $"{stem}.local.yaml" })
        {
            var path = Path.Combine(directory, name);
            if (!File.Exists(path))
            {
                continue;
            }

            var parsed = Deserializer.Deserialize<T>(File.ReadAllText(path));
            if (parsed is not null)
            {
                yield return parsed;
            }
        }
    }

    private static SourceSeed ToSeed(SourceEntry entry) => new(
        entry.Key,
        entry.Name,
        Enum.Parse<SourceKind>(entry.Kind, ignoreCase: true),
        Enum.Parse<SellerType>(entry.SellerType, ignoreCase: true),
        entry.Adapter,
        TimeSpan.FromMinutes(entry.PollMinutes),
        entry.PriorityPollMinutes is { } minutes ? TimeSpan.FromMinutes(minutes) : null,
        entry.Priority,
        entry.Enabled,
        entry.Configuration is null ? "{}" : System.Text.Json.JsonSerializer.Serialize(Normalize(entry.Configuration)));

    private static FinancingProfileSeed ToSeed(FinancingProfileEntry entry) => new(
        entry.Key,
        entry.Revision,
        entry.Name,
        entry.Savings,
        entry.NetMonthlyIncome,
        entry.ExistingMonthlyDebt,
        entry.MaxDebtToIncomeRatio,
        new MortgageTerms(Enum.Parse<MortgageType>(entry.Mortgage.Type, ignoreCase: true), entry.Mortgage.MaxLoanToValue, entry.Mortgage.NominalRatePercent, entry.Mortgage.SpreadPercent, entry.Mortgage.FixedMonths, entry.Mortgage.TermMonths, entry.Mortgage.OpeningFeePercent),
        new BuyerProfile(
            entry.Buyer.Age,
            entry.Buyer.FirstHome,
            entry.Buyer.PrimaryResidence,
            entry.Buyer.ResidentInBalearics,
            entry.Buyer.YearsResidentInBalearics,
            entry.Buyer.LargeFamily,
            entry.Buyer.SingleParentFamily,
            entry.Buyer.DisabilityPercent,
            entry.Buyer.TaxableIncomeBase,
            entry.Buyer.NetWorth,
            entry.Buyer.OwnsOtherHome,
            entry.Buyer.YearsResidentInSpain,
            entry.Buyer.JointTaxReturn,
            entry.Buyer.GrossAnnualIncome,
            entry.Buyer.BuyerCount,
            entry.Buyer.HasDependentChildren,
            entry.Buyer.DisabilityMinimumForRelatives),
        entry.IsDefault);

    private static SearchProfileSeed ToSeed(SearchProfileEntry entry) => new(
        entry.Key,
        entry.Revision,
        entry.Name,
        new ListingFilter
        {
            PriceMin = entry.Filter.PriceMin,
            PriceMax = entry.Filter.PriceMax,
            PricePerM2Max = entry.Filter.PricePerM2Max,
            BuiltM2Min = entry.Filter.BuiltM2Min,
            BuiltM2Max = entry.Filter.BuiltM2Max,
            RoomsMin = entry.Filter.RoomsMin,
            BathroomsMin = entry.Filter.BathroomsMin,
            PropertyTypes = entry.Filter.PropertyTypes.Select(type => Enum.Parse<PropertyType>(type, ignoreCase: true)).ToList(),
            Municipalities = entry.Filter.Municipalities,
            Radius = entry.Filter.Radius is { } radius ? new GeoRadius(radius.Latitude, radius.Longitude, radius.Metres) : null,
            PolygonWkt = entry.Filter.PolygonWkt,
            SellerTypes = entry.Filter.SellerTypes.Select(type => Enum.Parse<SellerType>(type, ignoreCase: true)).ToList(),
            SourceKeys = entry.Filter.SourceKeys,
            ExcludePossession = entry.Filter.ExcludePossession.Select(status => Enum.Parse<PossessionStatus>(status, ignoreCase: true)).ToList(),
            Elevator = entry.Filter.Elevator,
            IncludeNewBuild = entry.Filter.IncludeNewBuild,
            YearBuiltMin = entry.Filter.YearBuiltMin,
        },
        entry.Channels.Select(channel => Enum.Parse<AlertChannel>(channel, ignoreCase: true)).ToList(),
        entry.Priority,
        entry.Enabled);

    private static object Normalize(object value) => value switch
    {
        IDictionary<object, object> dictionary => dictionary.ToDictionary(pair => pair.Key.ToString()!, pair => Normalize(pair.Value)),
        IList<object> list => list.Select(Normalize).ToList(),
        string text when bool.TryParse(text, out var boolean) => boolean,
        string text when long.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var integer) => integer,
        string text when decimal.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) => number,
        _ => value,
    };

    public sealed class SourceSeedFile
    {
        public List<SourceEntry> Sources { get; set; } = [];
    }

    public sealed class SourceEntry
    {
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Kind { get; set; } = "Servicer";

        public string SellerType { get; set; } = "Servicer";

        public string Adapter { get; set; } = string.Empty;

        public int PollMinutes { get; set; } = 360;

        public int? PriorityPollMinutes { get; set; }

        public bool Priority { get; set; }

        public bool Enabled { get; set; } = true;

        public Dictionary<object, object>? Configuration { get; set; }
    }

    public sealed class FinancingProfilesFile
    {
        public List<FinancingProfileEntry> Profiles { get; set; } = [];
    }

    public sealed class FinancingProfileEntry
    {
        public string Key { get; set; } = string.Empty;

        public int Revision { get; set; } = 1;

        public string Name { get; set; } = string.Empty;

        public decimal Savings { get; set; }

        public decimal NetMonthlyIncome { get; set; }

        public decimal ExistingMonthlyDebt { get; set; }

        public decimal MaxDebtToIncomeRatio { get; set; } = 0.35m;

        public MortgageEntry Mortgage { get; set; } = new();

        public BuyerEntry Buyer { get; set; } = new();

        public bool IsDefault { get; set; }
    }

    public sealed class MortgageEntry
    {
        public string Type { get; set; } = "Fixed";

        public decimal MaxLoanToValue { get; set; } = 0.8m;

        public decimal? NominalRatePercent { get; set; }

        public decimal? SpreadPercent { get; set; }

        public int? FixedMonths { get; set; }

        public int TermMonths { get; set; } = 360;

        public decimal? OpeningFeePercent { get; set; }
    }

    public sealed class BuyerEntry
    {
        public int Age { get; set; } = 35;

        public bool FirstHome { get; set; } = true;

        public bool PrimaryResidence { get; set; } = true;

        public bool ResidentInBalearics { get; set; } = true;

        public int? YearsResidentInBalearics { get; set; }

        public bool LargeFamily { get; set; }

        public bool SingleParentFamily { get; set; }

        public int? DisabilityPercent { get; set; }

        public decimal? TaxableIncomeBase { get; set; }

        public decimal? NetWorth { get; set; }

        public bool? OwnsOtherHome { get; set; }

        public int? YearsResidentInSpain { get; set; }

        public bool? JointTaxReturn { get; set; }

        public decimal? GrossAnnualIncome { get; set; }

        public int BuyerCount { get; set; } = 1;

        public bool HasDependentChildren { get; set; }

        public bool DisabilityMinimumForRelatives { get; set; }
    }

    public sealed class SearchProfilesFile
    {
        public List<SearchProfileEntry> Profiles { get; set; } = [];
    }

    public sealed class SearchProfileEntry
    {
        public string Key { get; set; } = string.Empty;

        public int Revision { get; set; } = 1;

        public string Name { get; set; } = string.Empty;

        public FilterEntry Filter { get; set; } = new();

        public List<string> Channels { get; set; } = ["Telegram"];

        public bool Priority { get; set; }

        public bool Enabled { get; set; } = true;
    }

    public sealed class FilterEntry
    {
        public decimal? PriceMin { get; set; }

        public decimal? PriceMax { get; set; }

        public decimal? PricePerM2Max { get; set; }

        public decimal? BuiltM2Min { get; set; }

        public decimal? BuiltM2Max { get; set; }

        public int? RoomsMin { get; set; }

        public int? BathroomsMin { get; set; }

        public List<string> PropertyTypes { get; set; } = [];

        public List<string> Municipalities { get; set; } = [];

        public RadiusEntry? Radius { get; set; }

        public string? PolygonWkt { get; set; }

        public List<string> SellerTypes { get; set; } = [];

        public List<string> SourceKeys { get; set; } = [];

        public List<string> ExcludePossession { get; set; } = [];

        public bool? Elevator { get; set; }

        public bool IncludeNewBuild { get; set; } = true;

        public int? YearBuiltMin { get; set; }
    }

    public sealed class RadiusEntry
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int Metres { get; set; }
    }
}
