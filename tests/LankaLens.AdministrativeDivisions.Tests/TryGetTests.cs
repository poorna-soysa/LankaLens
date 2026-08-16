using LankaLens.AdministrativeDivisions.Tests.Fixtures;

namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class TryGetTests
{
    private readonly IAdministrativeDivisionProvider _provider = SyntheticAdministrativeDataset.CreateProvider();

    [Fact]
    public void TryGetProvince_Found_Returns_True_And_Object()
    {
        var found = _provider.TryGetProvince(SyntheticAdministrativeDataset.ProvinceAlphaCode, out var province);

        Assert.True(found);
        Assert.NotNull(province);
        Assert.Equal(SyntheticAdministrativeDataset.ProvinceAlphaCode, province.Code);
    }

    [Fact]
    public void TryGetProvince_NotFound_Returns_False_And_Null()
    {
        var found = _provider.TryGetProvince("DEV-P-MISSING", out var province);

        Assert.False(found);
        Assert.Null(province);
    }

    [Fact]
    public void TryGetDistrict_Found_Returns_True_And_Object()
    {
        var found = _provider.TryGetDistrict(SyntheticAdministrativeDataset.DistrictAlpha1Code, out var district);

        Assert.True(found);
        Assert.NotNull(district);
    }

    [Fact]
    public void TryGetDistrict_NotFound_Returns_False_And_Null()
    {
        var found = _provider.TryGetDistrict("DEV-D-MISSING", out var district);

        Assert.False(found);
        Assert.Null(district);
    }

    [Fact]
    public void TryGetDivisionalSecretariat_Found_Returns_True_And_Object()
    {
        var found = _provider.TryGetDivisionalSecretariat(
            SyntheticAdministrativeDataset.DivisionalSecretariatAlpha1XCode,
            out var division);

        Assert.True(found);
        Assert.NotNull(division);
    }

    [Fact]
    public void TryGetDivisionalSecretariat_NotFound_Returns_False_And_Null()
    {
        var found = _provider.TryGetDivisionalSecretariat("DEV-DS-MISSING", out var division);

        Assert.False(found);
        Assert.Null(division);
    }

    [Fact]
    public void TryGetGramaNiladhariDivision_Found_Returns_True_And_Object()
    {
        var found = _provider.TryGetGramaNiladhariDivision(
            SyntheticAdministrativeDataset.GramaNiladhariGroveCode,
            out var division);

        Assert.True(found);
        Assert.NotNull(division);
    }

    [Fact]
    public void TryGetGramaNiladhariDivision_NotFound_Returns_False_And_Null()
    {
        var found = _provider.TryGetGramaNiladhariDivision("DEV-GN-MISSING", out var division);

        Assert.False(found);
        Assert.Null(division);
    }
}
