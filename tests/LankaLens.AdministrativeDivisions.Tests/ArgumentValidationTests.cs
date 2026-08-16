namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class ArgumentValidationTests
{
    private readonly IAdministrativeDivisionProvider _provider = AdministrativeDivisions.Default;

    [Fact]
    public void GetProvinceByCode_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetProvinceByCode(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetProvinceByCode_EmptyOrWhitespace_Throws_ArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => _provider.GetProvinceByCode(code));
    }

    [Fact]
    public void TryGetDistrict_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.TryGetDistrict(null!, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("\t")]
    public void TryGetDistrict_EmptyOrWhitespace_Throws_ArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => _provider.TryGetDistrict(code, out _));
    }

    [Fact]
    public void Search_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.Search(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Search_EmptyOrWhitespace_Throws_ArgumentException(string query)
    {
        Assert.Throws<ArgumentException>(() => _provider.Search(query));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Search_Invalid_MaxResults_Throws_ArgumentOutOfRangeException(int maxResults)
    {
        var options = new AdministrativeDivisionSearchOptions { MaxResults = maxResults };

        Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Search("Grove", options));
    }

    [Fact]
    public void GetDistrictsByProvince_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetDistrictsByProvince(null!));
    }
}
