using System.Diagnostics;
using System.Reflection;
using LankaLens.AdministrativeDivisions.Internal;

namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class ProductionDatasetTests
{
    private readonly IAdministrativeDivisionProvider _provider = AdministrativeDivisions.Default;

    [Fact]
    public void Embedded_Resource_Is_Present()
    {
        var names = typeof(AdministrativeDivisions).Assembly.GetManifestResourceNames();
        Assert.Contains(EmbeddedAdministrativeDivisionLoader.ResourceName, names);
    }

    [Fact]
    public void Exact_Counts_Match_Snapshot()
    {
        Assert.Equal(9, _provider.GetProvinces().Count);
        Assert.Equal(25, _provider.GetDistricts().Count);
        Assert.Equal(340, _provider.GetDivisionalSecretariats().Count);
        Assert.Equal(14008, _provider.GetGramaNiladhariDivisions().Count);
    }

    [Fact]
    public void Multilingual_Coverage_Matches_Snapshot()
    {
        Assert.Equal(9, _provider.GetProvinces().Count(p => p.Name.Sinhala is not null));
        Assert.Equal(9, _provider.GetProvinces().Count(p => p.Name.Tamil is not null));
        Assert.Equal(25, _provider.GetDistricts().Count(d => d.Name.Sinhala is not null));
        Assert.Equal(25, _provider.GetDistricts().Count(d => d.Name.Tamil is not null));
        Assert.Equal(340, _provider.GetDivisionalSecretariats().Count(d => d.Name.Sinhala is not null));
        Assert.Equal(340, _provider.GetDivisionalSecretariats().Count(d => d.Name.Tamil is not null));
        Assert.Equal(13723, _provider.GetGramaNiladhariDivisions().Count(g => g.Name.Sinhala is not null));
        Assert.Equal(13723, _provider.GetGramaNiladhariDivisions().Count(g => g.Name.Tamil is not null));
    }

    [Fact]
    public void All_Codes_Are_Unique_And_Parents_Exist()
    {
        var provinces = _provider.GetProvinces();
        var districts = _provider.GetDistricts();
        var dsList = _provider.GetDivisionalSecretariats();
        var gnList = _provider.GetGramaNiladhariDivisions();

        Assert.Equal(provinces.Count, provinces.Select(p => p.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(districts.Count, districts.Select(d => d.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(dsList.Count, dsList.Select(d => d.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(gnList.Count, gnList.Select(g => g.Code).Distinct(StringComparer.Ordinal).Count());

        var provinceCodes = provinces.Select(p => p.Code).ToHashSet(StringComparer.Ordinal);
        var districtCodes = districts.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);
        var dsCodes = dsList.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);

        Assert.All(districts, d => Assert.Contains(d.ProvinceCode, provinceCodes));
        Assert.All(dsList, d => Assert.Contains(d.DistrictCode, districtCodes));
        Assert.All(gnList, g => Assert.Contains(g.DivisionalSecretariatCode, dsCodes));
    }

    [Fact]
    public void Names_Have_No_Placeholders_And_Verified_Are_NonEmpty()
    {
        foreach (var province in _provider.GetProvinces())
        {
            AssertValidEnglish(province.Name.English, province.Code);
            AssertValidOptional(province.Name.Sinhala, province.Code);
            AssertValidOptional(province.Name.Tamil, province.Code);
            Assert.False(string.IsNullOrWhiteSpace(province.Name.Sinhala));
            Assert.False(string.IsNullOrWhiteSpace(province.Name.Tamil));
        }

        foreach (var district in _provider.GetDistricts())
        {
            AssertValidEnglish(district.Name.English, district.Code);
            Assert.False(string.IsNullOrWhiteSpace(district.Name.Sinhala));
            Assert.False(string.IsNullOrWhiteSpace(district.Name.Tamil));
        }

        foreach (var ds in _provider.GetDivisionalSecretariats())
        {
            AssertValidEnglish(ds.Name.English, ds.Code);
            Assert.False(string.IsNullOrWhiteSpace(ds.Name.Sinhala));
            Assert.False(string.IsNullOrWhiteSpace(ds.Name.Tamil));
        }

        var unresolved = 0;
        foreach (var gn in _provider.GetGramaNiladhariDivisions())
        {
            AssertValidEnglish(gn.Name.English, gn.Code);
            Assert.False(gn.Code.StartsWith("DEV-", StringComparison.Ordinal));

            if (gn.Name.Sinhala is null || gn.Name.Tamil is null)
            {
                Assert.Null(gn.Name.Sinhala);
                Assert.Null(gn.Name.Tamil);
                unresolved++;
            }
            else
            {
                Assert.False(string.IsNullOrWhiteSpace(gn.Name.Sinhala));
                Assert.False(string.IsNullOrWhiteSpace(gn.Name.Tamil));
            }
        }

        Assert.Equal(285, unresolved);
    }

    [Fact]
    public void No_Development_Codes_In_Production()
    {
        Assert.DoesNotContain(_provider.GetProvinces(), p => p.Code.StartsWith("DEV-", StringComparison.Ordinal));
        Assert.DoesNotContain(_provider.GetDistricts(), d => d.Code.StartsWith("DEV-", StringComparison.Ordinal));
        Assert.DoesNotContain(_provider.GetDivisionalSecretariats(), d => d.Code.StartsWith("DEV-", StringComparison.Ordinal));
        Assert.DoesNotContain(_provider.GetGramaNiladhariDivisions(), g => g.Code.StartsWith("DEV-", StringComparison.Ordinal));
    }

    [Fact]
    public void Representative_Lookups_Across_Regions()
    {
        // Western / Colombo
        var western = _provider.GetProvinceByCode("1");
        Assert.NotNull(western);
        Assert.Equal("Western", western.Name.English);
        Assert.False(string.IsNullOrWhiteSpace(western.Name.Sinhala));
        Assert.False(string.IsNullOrWhiteSpace(western.Name.Tamil));

        var colombo = _provider.GetDistrictByCode("11");
        Assert.NotNull(colombo);
        Assert.Equal("1", colombo.ProvinceCode);
        Assert.Equal("Colombo", colombo.Name.English);

        var colomboDs = _provider.GetDivisionalSecretariatByCode("1103");
        Assert.NotNull(colomboDs);
        Assert.Equal("11", colomboDs.DistrictCode);

        var sampleGn = _provider.GetGramaNiladhariDivisionByCode("1103005");
        Assert.NotNull(sampleGn);
        Assert.Equal("1103", sampleGn.DivisionalSecretariatCode);
        Assert.False(string.IsNullOrWhiteSpace(sampleGn.Name.Sinhala));
        Assert.False(string.IsNullOrWhiteSpace(sampleGn.Name.Tamil));

        // Northern / Jaffna
        var northern = _provider.GetProvinceByCode("4");
        Assert.NotNull(northern);
        Assert.Contains("Northern", northern.Name.English, StringComparison.OrdinalIgnoreCase);

        var jaffna = _provider.GetDistrictByCode("41");
        Assert.NotNull(jaffna);
        Assert.Equal("4", jaffna.ProvinceCode);

        // Eastern
        var eastern = _provider.GetProvinceByCode("5");
        Assert.NotNull(eastern);
        Assert.Contains("Eastern", eastern.Name.English, StringComparison.OrdinalIgnoreCase);

        // Central
        var central = _provider.GetProvinceByCode("2");
        Assert.NotNull(central);
        Assert.Contains("Central", central.Name.English, StringComparison.OrdinalIgnoreCase);

        var districtsInWestern = _provider.GetDistrictsByProvince("1");
        Assert.NotEmpty(districtsInWestern);
        Assert.All(districtsInWestern, d => Assert.Equal("1", d.ProvinceCode));

        var parentProvince = _provider.GetProvinceForDistrict("11");
        Assert.NotNull(parentProvince);
        Assert.Equal("1", parentProvince.Code);
    }

    [Fact]
    public void Representative_English_Sinhala_Tamil_Search()
    {
        var english = _provider.Search(
            "Colombo",
            new AdministrativeDivisionSearchOptions
            {
                Language = Language.English,
                Type = AdministrativeDivisionType.District,
                MaxResults = 5
            });
        Assert.Contains(english, r => r.Code == "11");

        var western = _provider.GetProvinceByCode("1");
        Assert.NotNull(western);
        Assert.NotNull(western.Name.Sinhala);
        var sinhala = _provider.Search(
            western.Name.Sinhala!,
            new AdministrativeDivisionSearchOptions
            {
                Language = Language.Sinhala,
                Type = AdministrativeDivisionType.Province,
                MaxResults = 5
            });
        Assert.Contains(sinhala, r => r.Code == "1");

        Assert.NotNull(western.Name.Tamil);
        var tamil = _provider.Search(
            western.Name.Tamil!,
            new AdministrativeDivisionSearchOptions
            {
                Language = Language.Tamil,
                Type = AdministrativeDivisionType.Province,
                MaxResults = 5
            });
        Assert.Contains(tamil, r => r.Code == "1");
    }

    [Fact]
    public void Unresolved_Gn_Has_Null_Translations_And_No_Language_Fallback()
    {
        var gn = _provider.GetGramaNiladhariDivisionByCode("2124315");
        Assert.NotNull(gn);
        Assert.Equal("Rambukwella East", gn.Name.English);
        Assert.Null(gn.Name.Sinhala);
        Assert.Null(gn.Name.Tamil);

        var sinhalaHits = _provider.Search(
            gn.Name.English,
            new AdministrativeDivisionSearchOptions { Language = Language.Sinhala });
        Assert.DoesNotContain(sinhalaHits, r => r.Code == gn.Code);

        var tamilHits = _provider.Search(
            gn.Name.English,
            new AdministrativeDivisionSearchOptions { Language = Language.Tamil });
        Assert.DoesNotContain(tamilHits, r => r.Code == gn.Code);
    }

    [Fact]
    public void Lookup_All_Gn_Codes_Is_Fast_Enough()
    {
        var codes = _provider.GetGramaNiladhariDivisions().Select(g => g.Code).ToArray();
        var sw = Stopwatch.StartNew();
        foreach (var code in codes)
        {
            Assert.NotNull(_provider.GetGramaNiladhariDivisionByCode(code));
        }

        sw.Stop();
        Assert.True(
            sw.ElapsedMilliseconds < 5_000,
            $"Looking up all {codes.Length} GN codes took {sw.ElapsedMilliseconds} ms.");
    }

    private static void AssertValidEnglish(string english, string code)
    {
        Assert.False(string.IsNullOrWhiteSpace(english), $"English missing for {code}");
        Assert.False(IsPlaceholder(english), $"Placeholder English for {code}: {english}");
    }

    private static void AssertValidOptional(string? value, string code)
    {
        if (value is null)
        {
            return;
        }

        Assert.False(string.IsNullOrWhiteSpace(value), $"Whitespace localized name for {code}");
        Assert.False(IsPlaceholder(value), $"Placeholder localized name for {code}: {value}");
    }

    private static bool IsPlaceholder(string value) =>
        value.Equals("N/A", StringComparison.OrdinalIgnoreCase)
        || value.Equals("Unknown", StringComparison.OrdinalIgnoreCase)
        || value.Equals("TODO", StringComparison.OrdinalIgnoreCase);
}
