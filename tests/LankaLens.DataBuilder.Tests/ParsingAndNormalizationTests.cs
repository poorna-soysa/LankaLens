using LankaLens.DataBuilder.Generation;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;
using LankaLens.DataBuilder.Parsing;
using LankaLens.DataBuilder.Tests.Fixtures;
using LankaLens.DataBuilder.Validation;

namespace LankaLens.DataBuilder.Tests;

public sealed class ParsingAndNormalizationTests
{
    [Fact]
    public void Parser_preserves_leading_zero_codes_via_GndUid()
    {
        using var stream = SyntheticWorkbookFactory.CreateGndList(
            SyntheticWorkbookFactory.ValidTrilingualHierarchy());

        var records = new GndListWorkbookParser().Parse(stream);

        Assert.Equal(3, records.Count);
        Assert.Equal("1103005", records[0].GnUid);
        Assert.Equal("3", records[0].DsCode); // Excel numeric cell loses leading zero
        Assert.Equal("5", records[0].GnCode);

        var expected = CanonicalNormalizer.BuildExpectedGnUid(
            records[0].ProvinceCode,
            records[0].DistrictCode,
            records[0].DsCode,
            records[0].GnCode);
        Assert.Equal("1103005", expected);
    }

    [Fact]
    public void Normalizer_builds_hierarchical_census_codes()
    {
        using var stream = SyntheticWorkbookFactory.CreateGndList(
            SyntheticWorkbookFactory.ValidTrilingualHierarchy());
        var raw = new GndListWorkbookParser().Parse(stream);
        var dataset = CanonicalNormalizer.Normalize(raw, CreateMetadata());

        Assert.Equal(["1", "2"], dataset.Provinces.Select(p => p.Code).ToArray());
        Assert.Contains(dataset.Districts, d => d.Code == "11" && d.ProvinceCode == "1");
        Assert.Contains(dataset.Districts, d => d.Code == "21" && d.ProvinceCode == "2");
        Assert.Contains(dataset.DivisionalSecretariats, d => d.Code == "1103");
        Assert.Contains(dataset.GramaNiladhariDivisions, g => g.Code == "1103005");
    }

    [Fact]
    public void TextNormalizer_trims_and_collapses_spaces_without_changing_unicode()
    {
        Assert.Equal("කොළඹ", TextNormalizer.NormalizeOptionalText("  කොළඹ  "));
        Assert.Equal("கொழும்பு", TextNormalizer.NormalizeOptionalText("கொழும்பு"));
        Assert.Equal("Foo Bar", TextNormalizer.NormalizeOptionalText("Foo   Bar"));
        Assert.Null(TextNormalizer.NormalizeOptionalText("TODO"));
        Assert.Null(TextNormalizer.NormalizeOptionalText("N/A"));
        Assert.Null(TextNormalizer.NormalizeOptionalText("NULL"));
        Assert.Null(TextNormalizer.NormalizeOptionalText("-"));
        Assert.Equal("Name", TextNormalizer.NormalizeOptionalText("Name_x000D_"));
    }

    [Fact]
    public void Canonical_ordering_is_deterministic_by_code()
    {
        using var stream = SyntheticWorkbookFactory.CreateGndList(
            SyntheticWorkbookFactory.ValidTrilingualHierarchy().Reverse());
        var raw = new GndListWorkbookParser().Parse(stream);
        var dataset = CanonicalNormalizer.Normalize(raw, CreateMetadata());
        var json1 = CanonicalJsonWriter.Serialize(dataset);
        var json2 = CanonicalJsonWriter.Serialize(dataset);

        Assert.Equal(json1, json2);
        Assert.Equal(["1", "2"], dataset.Provinces.Select(p => p.Code));
        Assert.Equal(["11", "21"], dataset.Districts.Select(d => d.Code));
        Assert.True(dataset.GramaNiladhariDivisions[0].Code.CompareTo(
            dataset.GramaNiladhariDivisions[^1].Code, StringComparison.Ordinal) < 0);
    }

    [Fact]
    public void Json_preserves_sinhala_and_tamil_unicode()
    {
        using var stream = SyntheticWorkbookFactory.CreateGndList(
            SyntheticWorkbookFactory.ValidTrilingualHierarchy());
        var raw = new GndListWorkbookParser().Parse(stream);
        var dataset = CanonicalNormalizer.Normalize(raw, CreateMetadata());
        var json = CanonicalJsonWriter.Serialize(dataset);

        Assert.Contains("බස්නාහිර", json, StringComparison.Ordinal);
        Assert.Contains("மேல்", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\\u0", json, StringComparison.Ordinal);
    }

    private static CanonicalDatasetMetadata CreateMetadata() =>
        new(
            "Department of Census and Statistics, Sri Lanka",
            "Test fixture",
            "test",
            new DateOnly(2024, 3, 19),
            new DateOnly(2026, 8, 16));
}
