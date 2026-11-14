var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi("v1");
var app = builder.Build();
app.MapOpenApi();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
    .WithName("getLiveness")
    .WithSummary("Liveness probe.")
    .WithDescription("Returns 200 as soon as the process can serve requests; it does not check dependencies.")
    .WithTags("Health");
app.Run();

public partial class Program;
