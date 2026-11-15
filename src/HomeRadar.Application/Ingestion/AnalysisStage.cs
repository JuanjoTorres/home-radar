using HomeRadar.Application.Analysis;
using HomeRadar.Application.Intelligence;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Application.Ingestion;

public sealed partial class AnalysisStage(AnalysisService analysis, ListingInsightService insights, ILogger<AnalysisStage> logger) : IIngestionStage
{
    private static readonly ListingEventType[] Triggers = [ListingEventType.PriceChanged, ListingEventType.Relisted, ListingEventType.DescriptionChanged, ListingEventType.FeaturesChanged];

    private Result<FinancingProfile>? _defaultProfile;

    public int Order => StageOrder.Analyze;

    public async Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken)
    {
        if (!context.ShouldContinue)
        {
            return;
        }

        if (!context.IsNew && !context.NewEvents.Any(listingEvent => Triggers.Contains(listingEvent.Type)))
        {
            return;
        }

        _defaultProfile ??= await analysis.ResolveProfileAsync(null, cancellationToken);
        var profile = _defaultProfile.Value.IsSuccess ? _defaultProfile.Value.Value : null;
        var result = await insights.RefreshAsync(context.Listing!, profile, cancellationToken);
        if (result.IsSuccess)
        {
            context.Analysis = result.Value!.Financial?.Analysis;
        }
        else
        {
            AnalysisSkipped(logger, context.Listing!.Id, result.Error!.Code);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Listing {ListingId} not analysed: {Code}")]
    private static partial void AnalysisSkipped(ILogger logger, Guid listingId, string code);
}
