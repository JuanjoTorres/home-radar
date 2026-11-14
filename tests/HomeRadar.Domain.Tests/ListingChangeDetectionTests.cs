using HomeRadar.Domain.Listings;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Tests;

public sealed class ListingChangeDetectionTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);
    private static readonly Guid SourceId = Guid.CreateVersion7();

    [Fact]
    public void Creating_a_listing_raises_a_created_event_and_sets_initial_price()
    {
        var listing = Listing.Create(SourceId, ListingObservationBuilder.Sample(), T0);

        listing.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ListingEvent>().Type.ShouldBe(ListingEventType.Created);
        listing.InitialPrice.ShouldBe(Money.Euros(245000m));
        listing.PricePerSquareMetre.ShouldBe(2578.95m);
    }

    [Fact]
    public void Observing_identical_content_is_a_no_op_except_last_seen()
    {
        var listing = Listing.Create(SourceId, ListingObservationBuilder.Sample(), T0);
        listing.ClearDomainEvents();

        var outcome = listing.Observe(ListingObservationBuilder.Sample(), T0.AddHours(6));

        outcome.ShouldBe(ObservationOutcome.Unchanged);
        listing.DomainEvents.ShouldBeEmpty();
        listing.LastSeenAt.ShouldBe(T0.AddHours(6));
    }

    [Fact]
    public void A_price_drop_records_delta_percentage_and_drop_count()
    {
        var listing = Listing.Create(SourceId, ListingObservationBuilder.Sample(), T0);
        listing.ClearDomainEvents();

        listing.Observe(ListingObservationBuilder.Sample(price: 229000m), T0.AddDays(10));

        var priceEvent = listing.DomainEvents.OfType<ListingEvent>().ShouldHaveSingleItem();
        priceEvent.Type.ShouldBe(ListingEventType.PriceChanged);
        var payload = priceEvent.ReadPayload<PriceChangedPayload>();
        payload.DeltaAmount.ShouldBe(-16000m);
        payload.DeltaPercent.ShouldBe(-6.53m);
        listing.PriceDropCount.ShouldBe(1);
        listing.CumulativeDropPercent.ShouldBe(6.53m);
    }

    [Fact]
    public void Replacing_a_photo_is_reported_as_replacement_not_add_plus_remove()
    {
        var listing = Listing.Create(SourceId, ListingObservationBuilder.Sample(), T0);
        listing.ClearDomainEvents();

        listing.Observe(ListingObservationBuilder.Sample(photos: ["https://example.test/p/1.jpg", "https://example.test/p/3.jpg"]), T0.AddDays(1));

        var events = listing.DomainEvents.OfType<ListingEvent>().ToList();
        events.ShouldHaveSingleItem().Type.ShouldBe(ListingEventType.PhotoReplaced);
        var payload = events[0].ReadPayload<PhotoChangedPayload>();
        payload.ReplacedUrl.ShouldBe(new Uri("https://example.test/p/2.jpg"));
        payload.Url.ShouldBe(new Uri("https://example.test/p/3.jpg"));
        listing.Photos.Select(photo => photo.Position).ShouldBe([0, 1]);
    }

    [Fact]
    public void Disappearing_then_reappearing_marks_withdrawn_then_relisted()
    {
        var listing = Listing.Create(SourceId, ListingObservationBuilder.Sample(), T0);
        listing.ClearDomainEvents();

        listing.MarkNotSeen(T0.AddDays(2));
        listing.Status.ShouldBe(ListingStatus.Withdrawn);
        listing.ClearDomainEvents();

        listing.Observe(ListingObservationBuilder.Sample(price: 239000m), T0.AddDays(9));

        listing.Status.ShouldBe(ListingStatus.Relisted);
        var types = listing.DomainEvents.OfType<ListingEvent>().Select(e => e.Type).ToList();
        types.ShouldContain(ListingEventType.Relisted);
        types.ShouldContain(ListingEventType.PriceChanged);
        types.ShouldNotContain(ListingEventType.StatusChanged);
    }

    [Fact]
    public void Description_change_keeps_before_and_after_for_diffing()
    {
        var listing = Listing.Create(SourceId, ListingObservationBuilder.Sample(), T0);
        listing.ClearDomainEvents();

        listing.Observe(ListingObservationBuilder.Sample(description: "¡REBAJADO! Piso luminoso en Inca"), T0.AddDays(3));

        var payload = listing.DomainEvents.OfType<ListingEvent>().Single(e => e.Type == ListingEventType.DescriptionChanged).ReadPayload<DescriptionChangedPayload>();
        payload.Before.ShouldBe("Piso luminoso en Inca");
        payload.After.ShouldStartWith("¡REBAJADO!");
    }

    [Fact]
    public void Observation_from_another_listing_is_rejected()
    {
        var listing = Listing.Create(SourceId, ListingObservationBuilder.Sample(), T0);
        var foreign = ListingObservationBuilder.Sample() with { ExternalId = "999" };

        Should.Throw<Domain.Common.DomainException>(() => listing.Observe(foreign, T0));
    }
}
