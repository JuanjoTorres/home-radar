using System.Globalization;
using System.Text.RegularExpressions;
using HomeRadar.Domain.Finance;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Intelligence;

public enum EstimateSource
{
    Rules = 0,
    Manual = 1,
}

public sealed record RenovationInput(
    string? Title,
    string? Description,
    decimal? BuiltSquareMetres,
    int? Bathrooms,
    int? Rooms,
    int? YearBuilt,
    bool IsNewBuild,
    PropertyType PropertyType,
    RenovationOverride? Override,
    int CurrentYear);

public sealed record RenovationOverride(RenovationLevel? Level, IReadOnlyList<RenovationOverrideItem> Items, decimal? TotalBudget, string? Notes, DateTimeOffset UpdatedAt);

public sealed record RenovationOverrideItem(string Code, string Description, decimal Amount);

public sealed record RenovationLine(string Code, string Name, decimal Quantity, CostUnit Unit, CostRange Cost, bool Manual);

public sealed record RenovationEstimate(
    RenovationLevel Level,
    EstimateSource Source,
    EstimateConfidence Confidence,
    CostRange Cost,
    decimal AreaSquareMetres,
    bool AreaAssumed,
    IReadOnlyList<RenovationLine> Lines,
    IReadOnlyList<string> Signals,
    IReadOnlyList<string> VisitChecklist,
    string? Notes);

public interface IRenovationCostEstimator
{
    RenovationEstimate Estimate(RenovationInput input, RenovationConfig config);
}

public sealed class RuleBasedRenovationEstimator : IRenovationCostEstimator
{
    public RenovationEstimate Estimate(RenovationInput input, RenovationConfig config)
    {
        var (area, areaAssumed) = input.BuiltSquareMetres is { } built and > 0m ? (built, false) : (config.DefaultBuiltAreaSquareMetres, true);
        if (input.Override is { } manual)
        {
            return Manual(input, manual, config, area, areaAssumed);
        }

        var (level, confidence, signals) = Classify(input, config);
        return Price(level, confidence, signals, input, config, area, areaAssumed, EstimateSource.Rules, null);
    }

    public static (RenovationLevel Level, EstimateConfidence Confidence, IReadOnlyList<string> Signals) Classify(RenovationInput input, RenovationConfig config)
    {
        var signals = new List<string>();
        if (input.IsNewBuild)
        {
            signals.Add("New build");
            return (RenovationLevel.ReadyToMoveIn, EstimateConfidence.High, signals);
        }

        var words = TextKeys.Words($"{input.Title} {input.Description}");
        var matches = config.Keywords
            .SelectMany(group => group.Phrases.Where(phrase => Mentions(words, phrase, config.Negations)).Select(phrase => (group.Level, Phrase: phrase)))
            .ToList();
        signals.AddRange(matches.Select(match => $"Description mentions \"{match.Phrase}\" ({match.Level})"));
        if (config.RecentRenovationPattern is { } pattern && Regex.Match(words, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)) is { Success: true } renovated
            && int.TryParse(renovated.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var renovatedYear))
        {
            if (renovatedYear >= input.CurrentYear - config.RecentRenovationMaxAgeYears && renovatedYear <= input.CurrentYear)
            {
                matches.Add((RenovationLevel.ReadyToMoveIn, renovated.Value));
                signals.Add(string.Create(CultureInfo.InvariantCulture, $"Renovated in {renovatedYear}"));
            }
            else
            {
                signals.Add(string.Create(CultureInfo.InvariantCulture, $"Renovated in {renovatedYear}: too long ago to count"));
            }
        }

        var ageRule = input.YearBuilt is { } year
            ? config.AgeRules.Where(rule => year < rule.BuiltBefore).OrderBy(rule => rule.BuiltBefore).FirstOrDefault()
            : null;
        var ageLevel = ageRule?.MinimumLevel;
        if (input.YearBuilt is { } built)
        {
            signals.Add(ageRule is null
                ? string.Create(CultureInfo.InvariantCulture, $"Built in {built}: no age rule applies")
                : string.Create(CultureInfo.InvariantCulture, $"Built in {built}: {ageRule.Reason}"));
        }

        if (matches.Count > 0)
        {
            var renovatedClaim = matches.Any(match => match.Level == RenovationLevel.ReadyToMoveIn);
            var worst = matches.Where(match => match.Level != RenovationLevel.ReadyToMoveIn).Select(match => match.Level).DefaultIfEmpty(RenovationLevel.ReadyToMoveIn).Max();
            var level = worst != RenovationLevel.ReadyToMoveIn ? worst : RenovationLevel.ReadyToMoveIn;
            var conflicting = renovatedClaim && worst != RenovationLevel.ReadyToMoveIn;
            if (level == RenovationLevel.ReadyToMoveIn && ageLevel is { } minimum && minimum >= RenovationLevel.Medium)
            {
                signals.Add("Described as renovated in an old building: installations may still be original");
                return (RenovationLevel.Light, EstimateConfidence.Medium, signals);
            }

            var consistent = ageLevel is null || Math.Abs((int)level - (int)ageLevel.Value) <= 1;
            return (level, conflicting ? EstimateConfidence.Low : consistent && ageLevel is not null ? EstimateConfidence.High : EstimateConfidence.Medium, signals);
        }

        if (ageLevel is { } byAge)
        {
            return (byAge, EstimateConfidence.Medium, signals);
        }

        if (input.YearBuilt is not null)
        {
            return (RenovationLevel.ReadyToMoveIn, EstimateConfidence.Medium, signals);
        }

        signals.Add("No description keywords nor construction year: light renovation assumed");
        return (RenovationLevel.Light, EstimateConfidence.Low, signals);
    }

    private static RenovationEstimate Manual(RenovationInput input, RenovationOverride manual, RenovationConfig config, decimal area, bool areaAssumed)
    {
        if (manual.Items.Count > 0)
        {
            var lines = manual.Items.Select(item => new RenovationLine(item.Code, item.Description, 1m, CostUnit.Dwelling, new CostRange(item.Amount, item.Amount, item.Amount), Manual: true)).ToList();
            var total = lines.Aggregate(CostRange.Zero, (sum, line) => sum.Plus(line.Cost)).Rounded();
            return new RenovationEstimate(manual.Level ?? Classify(input, config).Level, EstimateSource.Manual, EstimateConfidence.High, total, area, areaAssumed, lines, ["Items set manually"], Checklist(manual.Level ?? RenovationLevel.Light, config), manual.Notes);
        }

        if (manual.TotalBudget is { } budget)
        {
            var level = manual.Level ?? Classify(input, config).Level;
            var line = new RenovationLine("manualBudget", "Budget set manually", 1m, CostUnit.Dwelling, new CostRange(budget, budget, budget), Manual: true);
            return new RenovationEstimate(level, EstimateSource.Manual, EstimateConfidence.High, new CostRange(budget, budget, budget).Rounded(), area, areaAssumed, [line], ["Budget set manually"], Checklist(level, config), manual.Notes);
        }

        var manualLevel = manual.Level ?? Classify(input, config).Level;
        return Price(manualLevel, EstimateConfidence.High, ["Level set manually"], input, config, area, areaAssumed, EstimateSource.Manual, manual.Notes);
    }

    private static RenovationEstimate Price(
        RenovationLevel level,
        EstimateConfidence confidence,
        IReadOnlyList<string> signals,
        RenovationInput input,
        RenovationConfig config,
        decimal area,
        bool areaAssumed,
        EstimateSource source,
        string? notes)
    {
        var lines = config.Items
            .Where(item => item.AppliesTo.Contains(level))
            .Select(item =>
            {
                var quantity = Quantity(item, input, area);
                return new RenovationLine(item.Code, item.Name, quantity, item.Unit, new CostRange(item.Min, item.Probable, item.Max).Times(quantity).Rounded(), Manual: false);
            })
            .Where(line => line.Quantity > 0m)
            .ToList();
        var itemsTotal = lines.Aggregate(CostRange.Zero, (sum, line) => sum.Plus(line.Cost));
        var total = itemsTotal;
        if (config.Levels.FirstOrDefault(candidate => candidate.Level == level) is { } rate)
        {
            var levelTotal = new CostRange(rate.MinPerSquareMetre, rate.ProbablePerSquareMetre, rate.MaxPerSquareMetre).Times(area).Rounded();
            total = new CostRange(Math.Max(levelTotal.Min, itemsTotal.Min), Math.Max(levelTotal.Probable, itemsTotal.Probable), Math.Max(levelTotal.Max, itemsTotal.Max));
            var remainder = new CostRange(total.Min - itemsTotal.Min, total.Probable - itemsTotal.Probable, total.Max - itemsTotal.Max).Rounded();
            if (remainder.Probable > 0m)
            {
                lines.Add(new RenovationLine("otherWorks", rate.Description ?? "Masonry, carpentry, waste and other works of this level", area, CostUnit.SquareMetre, remainder, Manual: false));
            }
        }

        if (config.PermitAndTaxPercent > 0m && total.Probable > 0m)
        {
            var permits = total.Times(config.PermitAndTaxPercent / 100m).Rounded();
            lines.Add(new RenovationLine("permitsAndTax", "Works tax (ICIO) and permit fees", 1m, CostUnit.Dwelling, permits, Manual: false));
            total = total.Plus(permits);
        }

        total = total.Rounded();

        var effectiveConfidence = areaAssumed && confidence > EstimateConfidence.Low ? confidence - 1 : confidence;
        var allSignals = areaAssumed ? signals.Append(string.Create(CultureInfo.InvariantCulture, $"Built area unknown: {area} m² assumed")).ToList() : signals;
        return new RenovationEstimate(level, source, effectiveConfidence, total, area, areaAssumed, lines, allSignals, Checklist(level, config), notes);
    }

    private static decimal Quantity(RenovationItemCost item, RenovationInput input, decimal area) => item.Unit switch
    {
        CostUnit.SquareMetre => area,
        _ => item.Quantity switch
        {
            QuantityBasis.Bathrooms => (Math.Max(1, input.Bathrooms ?? 1) * item.FixedQuantity) + item.QuantityOffset,
            QuantityBasis.Rooms => (Math.Max(1, input.Rooms ?? 2) * item.FixedQuantity) + item.QuantityOffset,
            QuantityBasis.BuiltArea => (area * item.FixedQuantity) + item.QuantityOffset,
            _ => item.FixedQuantity + item.QuantityOffset,
        },
    };

    private static bool Mentions(string words, string phrase, IReadOnlyList<string> negations)
    {
        var needle = TextKeys.Words(phrase);
        if (needle.Length == 0)
        {
            return false;
        }

        var haystack = $" {words} ";
        var target = $" {needle} ";
        var index = haystack.IndexOf(target, StringComparison.Ordinal);
        while (index >= 0)
        {
            var prefix = string.Join(' ', haystack[..index].Split(' ', StringSplitOptions.RemoveEmptyEntries).TakeLast(2));
            var combined = $"{prefix} {needle}";
            var negated = prefix.Length > 0 && negations.Select(TextKeys.Words).Any(negation =>
            {
                var position = combined.IndexOf(negation, StringComparison.Ordinal);
                return position >= 0 && position < prefix.Length && (position == 0 || combined[position - 1] == ' ');
            });
            if (!negated)
            {
                return true;
            }

            index = haystack.IndexOf(target, index + 1, StringComparison.Ordinal);
        }

        return false;
    }

    private static List<string> Checklist(RenovationLevel level, RenovationConfig config) => config.VisitChecklist
        .Where(group => group.Level <= level)
        .SelectMany(group => group.Checks)
        .Distinct(StringComparer.Ordinal)
        .ToList();
}
