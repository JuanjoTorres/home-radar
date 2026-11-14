using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace HomeRadar.ArchitectureTests;

public sealed class DependencyRuleTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Domain.Common.Entity<>).Assembly,
            typeof(Application.Common.IApplicationMarker).Assembly,
            typeof(Infrastructure.Persistence.PersistenceAssembly).Assembly,
            typeof(Sources.Common.SourcesAssembly).Assembly,
            typeof(Program).Assembly)
        .Build();

    private static readonly IObjectProvider<IType> DomainLayer = Types().That().ResideInNamespaceMatching("^HomeRadar.Domain").As("Domain");
    private static readonly IObjectProvider<IType> ApplicationLayer = Types().That().ResideInNamespaceMatching("^HomeRadar.Application").As("Application");
    private static readonly IObjectProvider<IType> InfrastructureLayer = Types().That().ResideInNamespaceMatching("^HomeRadar.Infrastructure").As("Infrastructure");
    private static readonly IObjectProvider<IType> SourcesLayer = Types().That().ResideInNamespaceMatching("^HomeRadar.Sources").As("Sources");
    private static readonly IObjectProvider<IType> ApiLayer = Types().That().ResideInNamespaceMatching("^HomeRadar.Api").As("Api");

    [Fact]
    public void Domain_does_not_depend_on_any_other_layer() =>
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(SourcesLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void Application_does_not_depend_on_infrastructure_sources_or_api() =>
        Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(SourcesLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void Infrastructure_and_sources_do_not_depend_on_each_other_or_on_api() =>
        Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(SourcesLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void Domain_does_not_reference_entity_framework_or_http() =>
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching("^Microsoft.EntityFrameworkCore")
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching("^System.Net.Http")
            .Check(Architecture);
}
