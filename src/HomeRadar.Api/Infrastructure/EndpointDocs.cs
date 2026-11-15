using System.Reflection;
using System.Text.Json.Nodes;

namespace HomeRadar.Api.Infrastructure;

/// <summary>Loads the long operation descriptions from the embedded Markdown files in Docs/Endpoints.</summary>
public static class EndpointDocs
{
    private static readonly Assembly Assembly = typeof(EndpointDocs).Assembly;

    public static string For(string operationId)
    {
        var resourceName = Assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith($".Docs.Endpoints.{operationId}.md", StringComparison.Ordinal));
        if (resourceName is null)
        {
            return string.Empty;
        }

        using var stream = Assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }

    public static JsonNode? Example(string operationId, string kind)
    {
        var resourceName = Assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith($".Docs.Examples.{operationId}.{kind}.json", StringComparison.Ordinal));
        if (resourceName is null)
        {
            return null;
        }

        using var stream = Assembly.GetManifestResourceStream(resourceName)!;
        return JsonNode.Parse(stream);
    }

    public static RouteHandlerBuilder Document(this RouteHandlerBuilder builder, string operationId, string summary, string tag) =>
        builder.WithName(operationId).WithSummary(summary).WithDescription(For(operationId)).WithTags(tag);
}
