using System.Security.Cryptography;
using System.Text;
using HomeRadar.Application.Ports;
using HomeRadar.Domain.Outbox;
using Microsoft.Extensions.Options;

namespace HomeRadar.Api.Infrastructure;

/// <summary>Replays the stored response when the same Idempotency-Key and body are sent again within the lifetime; rejects key reuse with a different body.</summary>
public sealed class IdempotencyFilter(IHomeRadarDbContext db, IOptions<ApiOptions> options, TimeProvider clock) : IEndpointFilter
{
    public const string HeaderName = "Idempotency-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        if (!request.Headers.TryGetValue(HeaderName, out var header) || string.IsNullOrWhiteSpace(header))
        {
            return await next(context);
        }

        var key = $"{request.Path}:{header}";
        var payload = context.Arguments.FirstOrDefault(argument => argument is not null && argument.GetType().Namespace?.StartsWith("HomeRadar.Api.Contracts", StringComparison.Ordinal) == true);
        var body = payload is null ? string.Empty : System.Text.Json.JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions);
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(body)));

        var now = clock.GetUtcNow();
        var stored = await db.IdempotencyRecords.FindAsync([key], context.HttpContext.RequestAborted);
        if (stored is not null && stored.ExpiresAt > now)
        {
            if (!string.Equals(stored.RequestHash, hash, StringComparison.Ordinal))
            {
                return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Idempotency-Key was already used with a different payload", type: ProblemTypes.Conflict);
            }

            context.HttpContext.Response.Headers["Idempotent-Replayed"] = "true";
            return Results.Content(stored.ResponseBody, "application/json", Encoding.UTF8, stored.ResponseStatus);
        }

        var result = await next(context);
        var inner = result is INestedHttpResult nested ? nested.Result : result;
        if (inner is IValueHttpResult { Value: { } value } and IStatusCodeHttpResult { StatusCode: { } status } && status is >= 200 and < 300)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value, value.GetType(), JsonOptions);
            if (stored is not null)
            {
                db.IdempotencyRecords.Remove(stored);
            }

            db.IdempotencyRecords.Add(new IdempotencyRecord(key, hash, status, json, now, options.Value.IdempotencyLifetime));
            await db.SaveChangesAsync(context.HttpContext.RequestAborted);
        }

        return result;
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web);
}
