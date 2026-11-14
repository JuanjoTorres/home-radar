using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HomeRadar.Api.Infrastructure;

public sealed class HomeRadarDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "HomeRadar Mallorca API";
        document.Info.Version = "1.0.0";
        document.Info.Contact = new OpenApiContact { Name = "HomeRadar", Url = new Uri("https://github.com/JuanjoTorres/home-radar") };
        document.Info.Description = "Headless API that watches low-competition property sources in Mallorca, records every listing change and tells its owner what they can afford. Conventions: kebab-case paths, camelCase JSON, ISO 8601 UTC timestamps, money as {amount, currency}, cursor pagination and RFC 9457 problem details for every error.";
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ApiKeyMiddleware.HeaderName,
            Description = "Static API key configured in HomeRadar:ApiKey.",
        };
        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("ApiKey", document)] = [] });

        if (document.Paths is { Count: > 0 })
        {
            var rewritten = new OpenApiPaths();
            foreach (var (path, item) in document.Paths)
            {
                rewritten[path.Replace("{version}", "1", StringComparison.Ordinal)] = item;
            }

            document.Paths = rewritten;
        }

        return Task.CompletedTask;
    }
}

public sealed class DecimalSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;
        if (type == typeof(decimal))
        {
            schema.Type = JsonSchemaType.Number;
            schema.Format = "decimal";
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}

public sealed class OperationConventionsTransformer : IOpenApiOperationTransformer
{
    private static readonly (string Status, string Description)[] StandardResponses =
    [
        ("401", "Missing or invalid X-Api-Key."),
        ("429", "Rate limit exceeded; honour Retry-After."),
        ("500", "Unexpected error; the traceId extension identifies the log entry."),
    ];

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Parameters is { Count: > 0 })
        {
            var version = operation.Parameters.FirstOrDefault(parameter => parameter.Name == "version");
            if (version is not null)
            {
                operation.Parameters.Remove(version);
            }
        }

        operation.Responses ??= [];
        foreach (var (status, description) in StandardResponses)
        {
            if (!operation.Responses.ContainsKey(status))
            {
                operation.Responses[status] = new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new() { Schema = new OpenApiSchemaReference("ProblemDetails", context.Document) },
                    },
                };
            }
        }

        return Task.CompletedTask;
    }
}
