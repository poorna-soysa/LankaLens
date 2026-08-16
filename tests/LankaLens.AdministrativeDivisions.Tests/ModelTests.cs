using LankaLens.AdministrativeDivisions.Internal;

namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class ModelTests
{
    [Fact]
    public void LocalizedName_Retains_Supplied_Values()
    {
        var name = new LocalizedName("English", "සිංහල", "தமிழ்");

        Assert.Equal("English", name.English);
        Assert.Equal("සිංහල", name.Sinhala);
        Assert.Equal("தமிழ்", name.Tamil);
    }

    [Fact]
    public void Province_Retains_Code_And_Name()
    {
        var name = new LocalizedName("Alpha", "අ", "அ");
        var province = new Province("DEV-P-A", name);

        Assert.Equal("DEV-P-A", province.Code);
        Assert.Same(name, province.Name);
    }

    [Fact]
    public void District_Retains_Code_Parent_And_Name()
    {
        var name = new LocalizedName("Alpha One", "අ1", "அ1");
        var district = new District("DEV-D-A1", "DEV-P-A", name);

        Assert.Equal("DEV-D-A1", district.Code);
        Assert.Equal("DEV-P-A", district.ProvinceCode);
        Assert.Same(name, district.Name);
    }

    [Fact]
    public void DivisionalSecretariat_Retains_Code_Parent_And_Name()
    {
        var name = new LocalizedName("Alpha One X", "අ1X", "அ1X");
        var division = new DivisionalSecretariat("DEV-DS-A1X", "DEV-D-A1", name);

        Assert.Equal("DEV-DS-A1X", division.Code);
        Assert.Equal("DEV-D-A1", division.DistrictCode);
        Assert.Same(name, division.Name);
    }

    [Fact]
    public void GramaNiladhariDivision_Retains_Code_Parent_And_Name()
    {
        var name = new LocalizedName("Grove", "ග්‍රෝව්", "குரோவ்");
        var division = new GramaNiladhariDivision("DEV-GN-GROVE", "DEV-DS-A1X", name);

        Assert.Equal("DEV-GN-GROVE", division.Code);
        Assert.Equal("DEV-DS-A1X", division.DivisionalSecretariatCode);
        Assert.Same(name, division.Name);
    }

    [Fact]
    public void DatasetMetadata_Retains_Supplied_Values()
    {
        var metadata = new DatasetMetadata(
            "Org",
            "Name",
            "1.0",
            new DateOnly(2025, 6, 1),
            new DateOnly(2026, 1, 1));

        Assert.Equal("Org", metadata.SourceOrganization);
        Assert.Equal("Name", metadata.SourceName);
        Assert.Equal("1.0", metadata.SourceVersion);
        Assert.Equal(new DateOnly(2025, 6, 1), metadata.EffectiveDate);
        Assert.Equal(new DateOnly(2026, 1, 1), metadata.RetrievedDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LocalizedName_Rejects_Invalid_English(string? english)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new LocalizedName(english!, "සිංහල", "தமிழ்"));
    }

    [Fact]
    public void LocalizedName_Allows_Null_Sinhala_And_Tamil()
    {
        var name = new LocalizedName("English Only", null, null);

        Assert.Equal("English Only", name.English);
        Assert.Null(name.Sinhala);
        Assert.Null(name.Tamil);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LocalizedName_Rejects_Whitespace_Sinhala(string sinhala)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new LocalizedName("English", sinhala, "தமிழ்"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LocalizedName_Rejects_Whitespace_Tamil(string tamil)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new LocalizedName("English", "සිංහල", tamil));
    }
}
