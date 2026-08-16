using LankaLens.AdministrativeDivisions.Tests.Fixtures;

namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class LookupTests
{
    private readonly IAdministrativeDivisionProvider _provider = SyntheticAdministrativeDataset.CreateProvider();

    [Fact]
    public void GetProvinceByCode_Known_Returns_Province()
    {
        var province = _provider.GetProvinceByCode(SyntheticAdministrativeDataset.ProvinceAlphaCode);

        Assert.NotNull(province);
        Assert.Equal(SyntheticAdministrativeDataset.ProvinceAlphaCode, province.Code);
        Assert.Equal("Alpha Province", province.Name.English);
    }

    [Fact]
    public void GetProvinceByCode_Unknown_Returns_Null()
    {
        Assert.Null(_provider.GetProvinceByCode("DEV-P-UNKNOWN"));
    }

    [Fact]
    public void GetDistrictByCode_Known_Returns_District()
    {
        var district = _provider.GetDistrictByCode(SyntheticAdministrativeDataset.DistrictAlpha1Code);

        Assert.NotNull(district);
        Assert.Equal(SyntheticAdministrativeDataset.ProvinceAlphaCode, district.ProvinceCode);
    }

    [Fact]
    public void GetDistrictByCode_Unknown_Returns_Null()
    {
        Assert.Null(_provider.GetDistrictByCode("DEV-D-UNKNOWN"));
    }

    [Fact]
    public void GetDistrictsByProvince_Filters_Hierarchy()
    {
        var districts = _provider.GetDistrictsByProvince(SyntheticAdministrativeDataset.ProvinceAlphaCode);

        Assert.Equal(2, districts.Count);
        Assert.All(districts, d => Assert.Equal(SyntheticAdministrativeDataset.ProvinceAlphaCode, d.ProvinceCode));
    }

    [Fact]
    public void GetDistrictsByProvince_Unknown_Returns_Empty()
    {
        var districts = _provider.GetDistrictsByProvince("DEV-P-UNKNOWN");

        Assert.Empty(districts);
    }

    [Fact]
    public void GetProvinceForDistrict_Resolves_Parent()
    {
        var province = _provider.GetProvinceForDistrict(SyntheticAdministrativeDataset.DistrictBeta1Code);

        Assert.NotNull(province);
        Assert.Equal(SyntheticAdministrativeDataset.ProvinceBetaCode, province.Code);
    }

    [Fact]
    public void GetDistrictForDivisionalSecretariat_Resolves_Parent()
    {
        var district = _provider.GetDistrictForDivisionalSecretariat(
            SyntheticAdministrativeDataset.DivisionalSecretariatAlpha1XCode);

        Assert.NotNull(district);
        Assert.Equal(SyntheticAdministrativeDataset.DistrictAlpha1Code, district.Code);
    }

    [Fact]
    public void GetDivisionalSecretariatForGramaNiladhariDivision_Resolves_Parent()
    {
        var parent = _provider.GetDivisionalSecretariatForGramaNiladhariDivision(
            SyntheticAdministrativeDataset.GramaNiladhariGroveCode);

        Assert.NotNull(parent);
        Assert.Equal(SyntheticAdministrativeDataset.DivisionalSecretariatAlpha1XCode, parent.Code);
    }

    [Fact]
    public void GetDivisionalSecretariatsByDistrict_Filters_Hierarchy()
    {
        var divisions = _provider.GetDivisionalSecretariatsByDistrict(
            SyntheticAdministrativeDataset.DistrictAlpha1Code);

        Assert.Equal(2, divisions.Count);
        Assert.All(
            divisions,
            d => Assert.Equal(SyntheticAdministrativeDataset.DistrictAlpha1Code, d.DistrictCode));
    }

    [Fact]
    public void GetGramaNiladhariDivisionsByDivisionalSecretariat_Filters_Hierarchy()
    {
        var divisions = _provider.GetGramaNiladhariDivisionsByDivisionalSecretariat(
            SyntheticAdministrativeDataset.DivisionalSecretariatAlpha1XCode);

        Assert.Equal(2, divisions.Count);
        Assert.Contains(divisions, d => d.Code == SyntheticAdministrativeDataset.GramaNiladhariGroveCode);
        Assert.Contains(divisions, d => d.Code == SyntheticAdministrativeDataset.GramaNiladhariGrovelandCode);
    }

    [Fact]
    public void Returned_Collections_Are_Not_Mutable_Lists()
    {
        var provinces = _provider.GetProvinces();

        Assert.IsNotType<List<Province>>(provinces);
        Assert.ThrowsAny<NotSupportedException>(() => ((IList<Province>)provinces).Add(
            new Province("X", new LocalizedName("a", "b", "c"))));
    }
}
