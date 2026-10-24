using System.Reflection;

namespace BM2.UnitTests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_does_not_reference_ef_core_or_aspnetcore() =>
        References("BM2.Domain").ShouldNotContain(n =>
            n.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
            || n.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));

    [Fact]
    public void Domain_references_only_contracts_among_bm2_projects() =>
        References("BM2.Domain")
            .Where(n => n.StartsWith("BM2.", StringComparison.Ordinal))
            .ShouldAllBe(n => n == "BM2.Contracts");

    [Fact]
    public void Contracts_does_not_reference_other_bm2_projects() =>
        References("BM2.Contracts").ShouldNotContain(n => n.StartsWith("BM2.", StringComparison.Ordinal));

    private static string[] References(string assembly) =>
        Assembly.Load(assembly).GetReferencedAssemblies().Select(a => a.Name!).ToArray();
}
