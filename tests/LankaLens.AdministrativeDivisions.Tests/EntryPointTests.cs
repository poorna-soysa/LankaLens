namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class EntryPointTests
{
    [Fact]
    public void Default_Is_Not_Null()
    {
        Assert.NotNull(AdministrativeDivisions.Default);
    }

    [Fact]
    public void Default_Returns_Same_Instance()
    {
        var first = AdministrativeDivisions.Default;
        var second = AdministrativeDivisions.Default;

        Assert.Same(first, second);
    }

    [Fact]
    public void Default_DatasetMetadata_Is_Production()
    {
        var metadata = AdministrativeDivisions.Default.DatasetMetadata;

        Assert.DoesNotContain("development", metadata.SourceOrganization, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Non-authoritative", metadata.SourceName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Census", metadata.SourceOrganization, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Home Affairs", metadata.SourceOrganization, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(new DateOnly(2024, 3, 19), metadata.EffectiveDate);
        Assert.Equal(new DateOnly(2026, 8, 16), metadata.RetrievedDate);
        Assert.Equal("2024-03-19", metadata.SourceVersion);
    }
}
