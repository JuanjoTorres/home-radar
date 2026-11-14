using HomeRadar.Application.Ports;

namespace HomeRadar.Application.Ingestion;

public sealed class EnrichmentStage(IEnumerable<IListingEnricher> enrichers) : IIngestionStage
{
    public int Order => StageOrder.Enrich;

    public async Task ExecuteAsync(IngestionContext context, CancellationToken cancellationToken)
    {
        if (!context.ShouldContinue)
        {
            return;
        }

        foreach (var enricher in enrichers)
        {
            var facts = await enricher.EnrichAsync(context.Listing!, cancellationToken);
            context.Listing!.ApplyEnrichment(facts.Select(fact => new KeyValuePair<string, string>(fact.Key, fact.Value)));
        }
    }
}
