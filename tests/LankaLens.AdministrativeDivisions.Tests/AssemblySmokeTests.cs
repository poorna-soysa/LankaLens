using LankaLens.AdministrativeDivisions;

namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class AssemblySmokeTests
{
    [Fact]
    public void AdministrativeDivisions_Assembly_IsLoadable()
    {
        var assembly = typeof(AdministrativeDivisions).Assembly;

        Assert.NotNull(assembly);
        Assert.Equal("LankaLens.AdministrativeDivisions", assembly.GetName().Name);
    }
}
