using LankaLens.AdministrativeDivisions.Tests.Fixtures;

namespace LankaLens.AdministrativeDivisions.Tests;

public sealed class SearchTests
{
    private readonly IAdministrativeDivisionProvider _provider = SyntheticAdministrativeDataset.CreateProvider();

    [Fact]
    public void Search_Ranks_Exact_Before_Prefix_Before_Contains()
    {
        var results = _provider.Search("Grove");

        Assert.Equal(3, results.Count);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariGroveCode, results[0].Code);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariGrovelandCode, results[1].Code);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariNorthgroveCode, results[2].Code);
    }

    [Fact]
    public void Search_English_Is_Case_Insensitive()
    {
        var results = _provider.Search("grove");

        Assert.Contains(results, r => r.Code == SyntheticAdministrativeDataset.GramaNiladhariGroveCode);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariGroveCode, results[0].Code);
    }

    [Fact]
    public void Search_Sinhala_Exact_Match()
    {
        var results = _provider.Search(
            SyntheticAdministrativeDataset.SinhalaExactToken,
            new AdministrativeDivisionSearchOptions { Language = Language.Sinhala });

        Assert.Single(results);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariGroveCode, results[0].Code);
    }

    [Fact]
    public void Search_Tamil_Exact_Match()
    {
        var results = _provider.Search(
            SyntheticAdministrativeDataset.TamilExactToken,
            new AdministrativeDivisionSearchOptions { Language = Language.Tamil });

        Assert.Single(results);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariCedarCode, results[0].Code);
    }

    [Fact]
    public void Search_Type_Filter_Restricts_Results()
    {
        var results = _provider.Search(
            "Alpha",
            new AdministrativeDivisionSearchOptions { Type = AdministrativeDivisionType.Province });

        Assert.Single(results);
        Assert.Equal(AdministrativeDivisionType.Province, results[0].Type);
        Assert.Equal(SyntheticAdministrativeDataset.ProvinceAlphaCode, results[0].Code);
    }

    [Fact]
    public void Search_Language_Filter_Uses_Only_Selected_Language()
    {
        var results = _provider.Search(
            "Grove",
            new AdministrativeDivisionSearchOptions { Language = Language.Sinhala });

        Assert.Empty(results);
    }

    [Fact]
    public void Search_MaxResults_Applies_After_Ranking()
    {
        var results = _provider.Search(
            "Grove",
            new AdministrativeDivisionSearchOptions { MaxResults = 2 });

        Assert.Equal(2, results.Count);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariGroveCode, results[0].Code);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariGrovelandCode, results[1].Code);
    }

    [Fact]
    public void Search_Stable_Ordering_Within_Equal_Rank()
    {
        // "Alpha" is a prefix for multiple English names across types.
        var results = _provider.Search("Alpha");

        Assert.True(results.Count >= 2);

        for (var i = 1; i < results.Count; i++)
        {
            var previous = results[i - 1];
            var current = results[i];

            var typeCompare = previous.Type.CompareTo(current.Type);
            if (typeCompare < 0)
            {
                continue;
            }

            if (typeCompare > 0)
            {
                Assert.Fail("Results are not ordered by AdministrativeDivisionType within equal rank.");
            }

            var nameCompare = string.Compare(
                previous.Name.English,
                current.Name.English,
                StringComparison.OrdinalIgnoreCase);
            if (nameCompare < 0)
            {
                continue;
            }

            if (nameCompare > 0)
            {
                Assert.Fail("Results are not ordered by English name within equal type.");
            }

            Assert.True(
                string.CompareOrdinal(previous.Code, current.Code) <= 0,
                "Results are not ordered by Code within equal English name.");
        }
    }

    [Fact]
    public void Search_Result_Provides_Hierarchy_Context()
    {
        var results = _provider.Search(
            "Grove",
            new AdministrativeDivisionSearchOptions
            {
                Type = AdministrativeDivisionType.GramaNiladhariDivision,
                MaxResults = 1
            });

        var hit = Assert.Single(results);
        Assert.Equal(SyntheticAdministrativeDataset.GramaNiladhariGroveCode, hit.Code);
        Assert.Equal(SyntheticAdministrativeDataset.ProvinceAlphaCode, hit.ProvinceCode);
        Assert.Equal(SyntheticAdministrativeDataset.DistrictAlpha1Code, hit.DistrictCode);
        Assert.Equal(SyntheticAdministrativeDataset.DivisionalSecretariatAlpha1XCode, hit.DivisionalSecretariatCode);
    }

    [Fact]
    public void Search_Province_Result_Has_Null_Parent_Codes()
    {
        var results = _provider.Search(
            "Alpha Province",
            new AdministrativeDivisionSearchOptions { Type = AdministrativeDivisionType.Province });

        var hit = Assert.Single(results);
        Assert.Null(hit.ProvinceCode);
        Assert.Null(hit.DistrictCode);
        Assert.Null(hit.DivisionalSecretariatCode);
    }
}
