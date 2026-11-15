using HomeRadar.Domain.Intelligence;
using HomeRadar.Infrastructure.Cadastre;
using HomeRadar.Infrastructure.ReferenceData;

namespace HomeRadar.Application.Tests.Intelligence;

public sealed class CadastreAndEuriborTests
{
    private const string Block = """
        {"consulta_dnprcResult":{"control":{"cudnp":3},"lrcdnp":{"rcdnp":[
          {"rc":{"pc1":"0763604","pc2":"DD7806C","car":"0001","cc1":"U","cc2":"Y"},
           "dt":{"locs":{"lous":{"lourb":{"loint":{"es":"1","pt":"00","pu":"01"}}}}},
           "debi":{"luso":"Comercial","sfc":"112","cpt":"9,120000","ant":"1974"}},
          {"rc":{"pc1":"0763604","pc2":"DD7806C","car":"0002","cc1":"I","cc2":"U"},
           "dt":{"locs":{"lous":{"lourb":{"loint":{"es":"1","pt":"01","pu":"A"}}}}},
           "debi":{"luso":"Residencial","sfc":"84","cpt":"6,830000","ant":"1974"}},
          {"rc":{"pc1":"0763604","pc2":"DD7806C","car":"0003","cc1":"O","cc2":"I"},
           "dt":{"locs":{"lous":{"lourb":{"loint":{"es":"1","pt":"01","pu":"B"}}}}},
           "debi":{"luso":"Residencial","sfc":"1.096","cpt":"8,410000","ant":"1974"}}
        ]}}}
        """;

    private const string Single = """
        {"consulta_dnprcResult":{"control":{"cudnp":1,"cucons":1},"bico":{
          "bi":{"idbi":{"cn":"RU","rc":{"pc1":"07040A0","pc2":"0100123","car":"0001","cc1":"Q","cc2":"Z"}},
                "ldt":"Polígono 1 Parcela 123 SA COMA. SELVA (ILLES BALEARS)",
                "debi":{"luso":"Agrario","sfc":"145","ant":"1932"}},
          "finca":{"ldt":"Polígono 1 Parcela 123 SA COMA. SELVA (ILLES BALEARS)"}}}}
        """;

    [Fact]
    public void Multi_unit_response_is_parsed_with_spanish_number_formats()
    {
        var parcel = CatastroOvcClient.ParseParcel("0763604DD7806C", Block);

        parcel.Status.ShouldBe(CadastralLookupStatus.Found);
        parcel.Units.Count.ShouldBe(3);
        parcel.Units[1].Reference.ShouldBe("0763604DD7806C0002IU");
        parcel.Units[1].BuiltSquareMetres.ShouldBe(84m);
        parcel.Units[2].BuiltSquareMetres.ShouldBe(1_096m);
        parcel.Units[1].Floor.ShouldBe("01");
        parcel.Units[1].YearBuilt.ShouldBe(1974);
        parcel.IsRustic.ShouldBeNull();
    }

    [Fact]
    public void Single_unit_response_reads_the_land_class()
    {
        var parcel = CatastroOvcClient.ParseParcel("07040A00100123", Single);

        parcel.Units.ShouldHaveSingleItem().Use.ShouldBe("Agrario");
        parcel.IsRustic.ShouldBe(true);
        parcel.Address.ShouldNotBeNull().ShouldContain("SELVA");
        CatastroOvcClient.IsRusticReference("07040A00100123").ShouldBeTrue();
        CatastroOvcClient.IsRusticReference("0763604DD7806C").ShouldBeFalse();
    }

    [Fact]
    public void Errors_map_to_not_found_or_exceptions()
    {
        const string notFound = """{"consulta_dnprcResult":{"control":{"cuerr":1},"lerr":[{"cod":"5","des":"NO EXISTE NINGÚN INMUEBLE CON LOS PARÁMETROS INDICADOS"}]}}""";
        const string malformed = """{"consulta_dnprcResult":{"control":{"cuerr":1},"lerr":[{"cod":"4","des":"LA REFERENCIA CATASTRAL NO ESTÁ BIEN FORMADA"}]}}""";
        const string noParcel = """{"Consulta_RCCOORResult":{"control":{"cuerr":1},"lerr":[{"cod":"16","des":"NO HAY REFERENCIA CATASTRAL EN ESAS COORDENADAS"}]}}""";
        const string coordinates = """{"Consulta_RCCOORResult":{"control":{"cucoor":1},"coordenadas":{"coord":[{"pc":{"pc1":"0763604","pc2":"DD7806C"},"geo":{"xcen":"2.65","ycen":"39.57","srs":"EPSG:4326"},"ldt":"CL EXEMPLE 10 PALMA (ILLES BALEARS)"}]}}}""";

        CatastroOvcClient.ParseParcel("0763604DD7806C", notFound).Status.ShouldBe(CadastralLookupStatus.NotFound);
        Should.Throw<InvalidOperationException>(() => CatastroOvcClient.ParseParcel("XX", malformed));
        CatastroOvcClient.ParseCoordinates(noParcel).ShouldBeNull();
        CatastroOvcClient.ParseCoordinates(coordinates).ShouldBe("0763604DD7806C");
    }

    [Fact]
    public void Street_points_fall_back_to_the_nearest_parcel_within_a_few_metres()
    {
        const string nearby = """{"Consulta_RCCOOR_DistanciaResult":{"control":{"cucoor":1},"coordenadas_distancias":{"coordd":[{"geo":{"xcen":"2.65","ycen":"39.57","srs":"EPSG:4326"},"lpcd":[{"pc":{"pc1":"0763605","pc2":"DD7806C"},"ldt":"CL EXEMPLE 12","dis":"9.40"},{"pc":{"pc1":"0763604","pc2":"DD7806C"},"ldt":"CL EXEMPLE 10","dis":"3.18"}]}]}}}""";
        const string empty = """{"Consulta_RCCOOR_DistanciaResult":{"control":{"cucoor":1},"coordenadas_distancias":{"coordd":[{"geo":{"xcen":"2.60","ycen":"39.40","srs":"EPSG:4326"}}]}}}""";

        CatastroOvcClient.ParseNearest(nearby, 15d).ShouldBe("0763604DD7806C");
        CatastroOvcClient.ParseNearest(nearby, 2d).ShouldBeNull();
        CatastroOvcClient.ParseNearest(empty, 15d).ShouldBeNull();
    }

    [Fact]
    public void Units_are_matched_by_reference_area_or_fall_back_to_the_parcel()
    {
        var parcel = new CadastralParcel("0763604DD7806C", CadastralLookupStatus.Found, CatastroOvcClient.ParseParcel("0763604DD7806C", Block).Units, null, null, DateTimeOffset.UnixEpoch);

        CadastralMatcher.Match(parcel, "0763604DD7806C0003OI", null, 20m).Match.ShouldBe(CadastralMatchQuality.Unit);
        var byArea = CadastralMatcher.Match(parcel, null, 80m, 20m);
        byArea.Match.ShouldBe(CadastralMatchQuality.Unit);
        byArea.Reference.ShouldBe("0763604DD7806C0002IU");
        var fallback = CadastralMatcher.Match(parcel, null, 300m, 20m);
        fallback.Match.ShouldBe(CadastralMatchQuality.Parcel);
        fallback.YearBuilt.ShouldBe(1974);
        fallback.BuiltSquareMetres.ShouldBeNull();
    }

    [Fact]
    public void Banco_de_espana_csv_yields_the_latest_twelve_month_euribor()
    {
        const string csv = "\"CÓDIGO DE LA SERIE\",\"D_1NBAF472\",\"D_OTHER\"\n"
            + "\"NÚMERO SECUENCIAL\",\"587853\",\"1\"\n"
            + "\"ALIAS DE LA SERIE\",\"BE_19_1.5\",\"X\"\n"
            + "\"DESCRIPCIÓN DE LA SERIE\",\"Euríbor a 12 meses\",\"Otro\"\n"
            + "\"UNIDADES\",\"Porcentaje\",\"Porcentaje\"\n"
            + "\"FRECUENCIA\",\"MENSUAL\",\"MENSUAL\"\n"
            + "\"AGO 2026\",\"2.954\",\"1.0\"\n"
            + "\"SEP 2026\",\"3.247\",\"1.0\"\n"
            + "\"OCT 2026\",\"3.239\",\"1.0\"\n"
            + "\"NOV 2026\",\"_\",\"1.0\"\n";

        var observation = BdeEuriborFeed.Parse(csv, BdeEuriborFeed.CsvUri);

        observation.ShouldNotBeNull();
        observation.Month.ShouldBe(new DateOnly(2026, 10, 1));
        observation.Percent.ShouldBe(3.239m);
    }
}
