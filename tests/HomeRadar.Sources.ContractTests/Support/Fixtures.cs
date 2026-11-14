namespace HomeRadar.Sources.ContractTests.Support;

internal static class Fixtures
{
    public static string Read(string source, string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", source, name);
        return File.ReadAllText(path);
    }
}
