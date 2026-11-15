using System.Text;
using HomeRadar.Domain.Intelligence;
using HomeRadar.Infrastructure.Cadastre;
using HomeRadar.Infrastructure.ReferenceData;

namespace HomeRadar.Sources.ContractTests;

public sealed class ReferenceDataContractTests
{
    private static string? Recorded(string source, string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", source, name);
        return File.Exists(path) && new FileInfo(path).Length > 0 ? File.ReadAllText(path, source == "bde" ? Encoding.Latin1 : Encoding.UTF8) : null;
    }

    [Fact]
    public void Catastro_point_lookup_and_parcel_parse_recorded_responses()
    {
        var point = Recorded("catastro", "rccoor-palma-centre.json");
        Assert.SkipWhen(point is null, "Catastro fixtures not recorded yet: run the record-fixtures workflow with source=catastro");

        var reference = CatastroOvcClient.ParseCoordinates(point!).ShouldNotBeNull();
        reference.Length.ShouldBe(14);
        var parcel = Recorded("catastro", "dnprc-palma-centre.json").ShouldNotBeNull();
        var result = CatastroOvcClient.ParseParcel(reference, parcel);
        result.Status.ShouldBe(CadastralLookupStatus.Found);
        result.Units.ShouldNotBeEmpty();
        result.Units.ShouldAllBe(unit => unit.Reference.StartsWith(reference, StringComparison.Ordinal) && unit.Reference.Length == 20);
        result.Units.ShouldContain(unit => unit.BuiltSquareMetres > 0m && unit.YearBuilt > 1800);
    }

    [Fact]
    public void Catastro_errors_parse_recorded_responses()
    {
        var sea = Recorded("catastro", "rccoor-sea.json");
        var missing = Recorded("catastro", "dnprc-not-found.json");
        Assert.SkipWhen(sea is null || missing is null, "Catastro fixtures not recorded yet");

        CatastroOvcClient.ParseCoordinates(sea!).ShouldBeNull();
        try
        {
            CatastroOvcClient.ParseParcel("0000000AA0000A", missing!).Status.ShouldBe(CadastralLookupStatus.NotFound);
        }
        catch (InvalidOperationException error)
        {
            error.Message.ShouldContain("Catastro error");
        }
    }

    [Fact]
    public void Banco_de_espana_recorded_csv_has_a_recent_euribor()
    {
        var csv = Recorded("bde", "euribor-be1901.csv");
        Assert.SkipWhen(csv is null, "Banco de España fixture not recorded yet: run the record-fixtures workflow with source=bde");

        var observation = BdeEuriborFeed.Parse(csv!, BdeEuriborFeed.CsvUri).ShouldNotBeNull();
        observation.Month.Year.ShouldBeGreaterThanOrEqualTo(2026);
        observation.Percent.ShouldBeInRange(-1m, 10m);
    }
}
