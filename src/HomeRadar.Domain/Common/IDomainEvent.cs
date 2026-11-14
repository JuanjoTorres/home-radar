namespace HomeRadar.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
