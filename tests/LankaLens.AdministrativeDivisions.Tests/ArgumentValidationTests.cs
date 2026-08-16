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
    public void GetProvinceByCode_Unknown_Returns_Null()
    {
        Assert.Null(_provider.GetProvinceByCode("999"));
    }

    [Fact]
    public void TryGetProvince_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.TryGetProvince(null!, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("\t")]
    public void TryGetProvince_EmptyOrWhitespace_Throws_ArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => _provider.TryGetProvince(code, out _));
    }

    [Fact]
    public void TryGetProvince_Unknown_Returns_False()
    {
        Assert.False(_provider.TryGetProvince("999", out var province));
        Assert.Null(province);
    }

    [Fact]
    public void GetDistrictByCode_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetDistrictByCode(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void GetDistrictByCode_EmptyOrWhitespace_Throws_ArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => _provider.GetDistrictByCode(code));
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
    public void GetDivisionalSecretariatByCode_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetDivisionalSecretariatByCode(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void GetDivisionalSecretariatByCode_EmptyOrWhitespace_Throws_ArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => _provider.GetDivisionalSecretariatByCode(code));
    }

    [Fact]
    public void TryGetDivisionalSecretariat_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.TryGetDivisionalSecretariat(null!, out _));
    }

    [Fact]
    public void GetGramaNiladhariDivisionByCode_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetGramaNiladhariDivisionByCode(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    public void GetGramaNiladhariDivisionByCode_EmptyOrWhitespace_Throws_ArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => _provider.GetGramaNiladhariDivisionByCode(code));
    }

    [Fact]
    public void TryGetGramaNiladhariDivision_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.TryGetGramaNiladhariDivision(null!, out _));
    }

    [Fact]
    public void GetDistrictsByProvince_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetDistrictsByProvince(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetDistrictsByProvince_EmptyOrWhitespace_Throws_ArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => _provider.GetDistrictsByProvince(code));
    }

    [Fact]
    public void GetDistrictsByProvince_Unknown_Returns_Empty()
    {
        Assert.Empty(_provider.GetDistrictsByProvince("999"));
    }

    [Fact]
    public void GetDivisionalSecretariatsByDistrict_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetDivisionalSecretariatsByDistrict(null!));
    }

    [Fact]
    public void GetDivisionalSecretariatsByDistrict_Unknown_Returns_Empty()
    {
        Assert.Empty(_provider.GetDivisionalSecretariatsByDistrict("999"));
    }

    [Fact]
    public void GetGramaNiladhariDivisionsByDivisionalSecretariat_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _provider.GetGramaNiladhariDivisionsByDivisionalSecretariat(null!));
    }

    [Fact]
    public void GetGramaNiladhariDivisionsByDivisionalSecretariat_Unknown_Returns_Empty()
    {
        Assert.Empty(_provider.GetGramaNiladhariDivisionsByDivisionalSecretariat("999999"));
    }

    [Fact]
    public void GetProvinceForDistrict_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetProvinceForDistrict(null!));
    }

    [Fact]
    public void GetProvinceForDistrict_Unknown_Returns_Null()
    {
        Assert.Null(_provider.GetProvinceForDistrict("999"));
    }

    [Fact]
    public void GetDistrictForDivisionalSecretariat_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _provider.GetDistrictForDivisionalSecretariat(null!));
    }

    [Fact]
    public void GetDivisionalSecretariatForGramaNiladhariDivision_Null_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _provider.GetDivisionalSecretariatForGramaNiladhariDivision(null!));
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

        Assert.Throws<ArgumentOutOfRangeException>(() => _provider.Search("Colombo", options));
    }

    [Fact]
    public void Code_Lookup_Does_Not_Trim_Whitespace()
    {
        Assert.Null(_provider.GetProvinceByCode(" 1 "));
        Assert.NotNull(_provider.GetProvinceByCode("1"));
    }
}
