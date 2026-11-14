using Microsoft.Net.Http.Headers;

namespace HomeRadar.Api.Infrastructure;

public static class ETags
{
    public static string For(uint version) => $"\"{version}\"";

    public static void Write(HttpResponse response, uint version) => response.Headers.ETag = For(version);

    /// <summary>Returns a problem result when If-Match is missing or does not match the current version; null otherwise.</summary>
    public static IResult? Check(HttpRequest request, uint version)
    {
        if (!request.Headers.TryGetValue(HeaderNames.IfMatch, out var values) || values.Count == 0)
        {
            return Results.Problem(statusCode: StatusCodes.Status428PreconditionRequired, title: "If-Match header is required", type: ProblemTypes.PreconditionRequired);
        }

        var expected = For(version);
        var matches = values.Any(value => value is not null && (value == "*" || value.Split(',').Select(part => part.Trim().Replace("W/", string.Empty, StringComparison.Ordinal)).Contains(expected, StringComparer.Ordinal)));
        return matches ? null : Results.Problem(statusCode: StatusCodes.Status412PreconditionFailed, title: "The resource changed since it was read", type: ProblemTypes.PreconditionFailed);
    }
}
