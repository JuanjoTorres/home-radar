using HomeRadar.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeRadar.Api.Infrastructure;

public static class Problems
{
    public static ProblemHttpResult From(DomainError error) => error.Code switch
    {
        "listing.notFound" or "financingProfile.notFound" => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: error.Message, type: ProblemTypes.NotFound, extensions: Code(error)),
        _ => TypedResults.Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: "The request cannot be analysed", detail: error.Message, type: ProblemTypes.BusinessRule, extensions: Code(error)),
    };

    private static Dictionary<string, object?> Code(DomainError error) => new(StringComparer.Ordinal) { ["code"] = error.Code };
}
