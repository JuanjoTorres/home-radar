using HomeRadar.Application.Analysis;
using HomeRadar.Domain.Common;
using HomeRadar.Domain.Listings;
using HomeRadar.Domain.Profiles;
using Microsoft.Extensions.Logging;

namespace HomeRadar.Application.Ingestion;

public sealed partial class AnalysisStage(AnalysisService analysis, ILogger<AnalysisStage> logger) : IIngestionStage
{
    private Result<FinancingProfile>? _defaultProfile;

    public int Order => StageOrder.Analyze;

    public async Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken)
    {
        if (!context.ShouldContinue || context.Listing!.Price is null)
        {
            return;
        }

        if (!context.IsNew && !context.NewEvents.Any(listingEvent => listingEvent.Type is ListingEventType.PriceChanged or ListingEventType.Relisted))
        {
            return;
        }

        _defaultProfile ??= await analysis.ResolveProfileAsync(null, cancellationToken);
        if (!_defaultProfile.Value.IsSuccess)
        {
            return;
        }

        var result = await analysis.AnalyzeAsync(context.Listing, _defaultProfile.Value.Value!, cancellationToken);
        if (result.IsSuccess)
        {
            context.Analysis = result.Value!.Analysis;
        }
        else
        {
            AnalysisSkipped(logger, context.Listing.Id, result.Error!.Code);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Listing {ListingId} not analysed: {Code}")]
    private static partial void AnalysisSkipped(ILogger logger, Guid listingId, string code);
}
