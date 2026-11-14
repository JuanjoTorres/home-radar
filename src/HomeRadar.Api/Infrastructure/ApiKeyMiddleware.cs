using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace HomeRadar.Api.Infrastructure;

public sealed class ApiKeyMiddleware(RequestDelegate next, IOptions<ApiOptions> options)
{
    public const string HeaderName = "X-Api-Key";

    private readonly byte[] _expected = Encoding.UTF8.GetBytes(options.Value.ApiKey);

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health") || context.Request.Path.StartsWithSegments("/openapi") || context.Request.Path.StartsWithSegments("/scalar"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var provided) || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided.ToString()), _expected))
        {
            await Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Missing or invalid API key", type: ProblemTypes.Unauthorized).ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}

public static class ProblemTypes
{
    private const string Base = "https://homeradar.invalid/problems/";

    public const string Unauthorized = Base + "unauthorized";
    public const string NotFound = Base + "not-found";
    public const string Validation = Base + "validation";
    public const string Conflict = Base + "conflict";
    public const string PreconditionFailed = Base + "precondition-failed";
    public const string PreconditionRequired = Base + "precondition-required";
    public const string RateLimited = Base + "rate-limited";
    public const string BusinessRule = Base + "business-rule";
}
