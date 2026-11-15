using HomeRadar.Application.Ports;
using HomeRadar.Domain.Finance;
using Microsoft.EntityFrameworkCore;

namespace HomeRadar.Application.ReferenceData;

public sealed class EuriborUpdateService(IHomeRadarDbContext db, IEuriborFeed feed, TimeProvider clock)
{
    public async Task<ReferenceRate?> UpdateAsync(CancellationToken cancellationToken)
    {
        var observation = await feed.LatestAsync(cancellationToken);
        if (observation is null)
        {
            return null;
        }

        var month = new DateOnly(observation.Month.Year, observation.Month.Month, 1);
        var exists = await db.ReferenceRates.AnyAsync(rate => rate.Code == ReferenceRate.EuriborTwelveMonths && rate.Month == month, cancellationToken);
        if (exists)
        {
            return null;
        }

        var rate = new ReferenceRate(ReferenceRate.EuriborTwelveMonths, month, observation.Percent, observation.Source.ToString(), clock.GetUtcNow());
        db.ReferenceRates.Add(rate);
        await db.SaveChangesAsync(cancellationToken);
        return rate;
    }
}
