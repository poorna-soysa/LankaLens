using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;
using LankaLens.DataBuilder.Parsing;
using LankaLens.DataBuilder.Tests.Fixtures;

namespace LankaLens.DataBuilder.Tests;

public sealed class LifeCodeParserTests
{
    [Fact]
    public void Parses_official_four_part_life_code()
    {
        Assert.True(LifeCodeParser.TryParse("1-1-03-005", out var parsed, out var error));
        Assert.Null(error);
        Assert.NotNull(parsed);
        Assert.Equal("1-1-03-005", parsed!.LifeCode);
        Assert.Equal("1103005", parsed.NormalizedLifeCode);
        Assert.Equal("1", parsed.ProvinceComponent);
        Assert.Equal("1", parsed.DistrictComponent);
        Assert.Equal("03", parsed.DsComponent);
        Assert.Equal("005", parsed.GnComponent);
        Assert.Equal("1", parsed.HierarchicalProvinceCode);
        Assert.Equal("11", parsed.HierarchicalDistrictCode);
        Assert.Equal("1103", parsed.HierarchicalDsCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1-1-03")]
    [InlineData("1-1-03-005-1")]
    [InlineData("1-1-3-5")]
    [InlineData("1-1-03-05")]
    [InlineData("11-03-005")]
    [InlineData("1-1-03-00A")]
    [InlineData("1_1_03_005")]
    public void Rejects_invalid_formats(string? value)
    {
        Assert.False(LifeCodeParser.TryParse(value, out var parsed, out var error));
        Assert.Null(parsed);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void Does_not_invent_leading_zeros()
    {
        Assert.False(LifeCodeParser.TryParse("1-1-3-005", out _, out var error));
        Assert.Contains("DSD", error, StringComparison.Ordinal);
    }
}

public sealed class MohaGnReportParserTests
{
    [Fact]
    public void Parses_trilingual_row_and_preserves_unicode()
    {
        var html = SyntheticMohaHtmlFactory.GnReport(SyntheticMohaHtmlFactory.Sammanthranapura());
        var parsed = new MohaGnReportParser().ParseHtml(html, "colombo.html");

        Assert.Empty(parsed.InvalidCodes);
        var row = Assert.Single(parsed.Records);
        Assert.Equal("1-1-03-005", row.LifeCode);
        Assert.Equal("1103005", row.NormalizedLifeCode);
        Assert.Equal("සම්මන්ත්‍රණපුර", row.SinhalaName);
        Assert.Equal("சம்மந்திரணபுர", row.TamilName);
        Assert.Equal("Sammanthranapura", row.EnglishName);
        Assert.Equal("Western", row.ProvinceEnglish);
        Assert.Equal("බස්නාහිර", row.ProvinceSinhala);
        Assert.Equal("மேற்கு", row.ProvinceTamil);
        Assert.Equal("Colombo", row.DsEnglish);
        Assert.Equal("කොළඹ", row.DsSinhala);
        Assert.Equal("கொழும்பு", row.DsTamil);
        Assert.Equal("colombo.html", row.SourceReportFile);
    }

    [Fact]
    public void Treats_placeholders_as_missing()
    {
        var row = SyntheticMohaHtmlFactory.Sammanthranapura() with
        {
            Sinhala = "N/A",
            Tamil = "NULL",
            English = "-"
        };
        var parsed = new MohaGnReportParser().ParseHtml(SyntheticMohaHtmlFactory.GnReport(row));
        var record = Assert.Single(parsed.Records);
        Assert.Null(record.SinhalaName);
        Assert.Null(record.TamilName);
        Assert.Null(record.EnglishName);
    }

    [Fact]
    public void Records_invalid_life_codes()
    {
        var row = SyntheticMohaHtmlFactory.Sammanthranapura() with { LifeCode = "1-1-3-5" };
        var parsed = new MohaGnReportParser().ParseHtml(SyntheticMohaHtmlFactory.GnReport(row));
        Assert.Empty(parsed.Records);
        var invalid = Assert.Single(parsed.InvalidCodes);
        Assert.Equal("1-1-3-5", invalid.RawValue);
    }

    [Fact]
    public void Strips_bidirectional_marks_without_removing_zwj()
    {
        var row = SyntheticMohaHtmlFactory.Sammanthranapura() with
        {
            DistrictLabel = "1: කොළඹ/ \u200Eகொழும்பு/ Colombo"
        };
        var parsed = new MohaGnReportParser().ParseHtml(SyntheticMohaHtmlFactory.GnReport(row));
        var record = Assert.Single(parsed.Records);
        Assert.Equal("கொழும்பு", record.DistrictTamil);
        Assert.NotNull(record.SinhalaName);
        Assert.Contains('\u200D', record.SinhalaName);
    }
}

public sealed class EnglishNameDifferenceClassifierTests
{
    [Theory]
    [InlineData("Colombo", "Colombo", "Exact")]
    [InlineData("Colombo", "colombo", "CaseOnly")]
    [InlineData("Dehiwala  Mount", "Dehiwala Mount", "WhitespaceOnly")]
    [InlineData("St. Mary's", "St Mary's", "Punctuation")]
    [InlineData("Colombo", "Kolombo", "Spelling")]
    [InlineData("Hanwella", "Seethawaka", "Substantive")]
    [InlineData("Colombo", null, "MissingMohaEnglish")]
    public void Classifies_differences(string? dcs, string? moha, string expected)
    {
        Assert.Equal(expected, EnglishNameDifferenceClassifier.Classify(dcs, moha).ToString());
    }
}

public sealed class MohaDcsJoinTests
{
    [Fact]
    public void Joins_on_normalized_life_code()
    {
        var dcs = CreateDcsDataset();
        var html = SyntheticMohaHtmlFactory.GnReport(
            SyntheticMohaHtmlFactory.Sammanthranapura(),
            SyntheticMohaHtmlFactory.Mattakkuliya());
        var moha = new MohaGnReportParser().ParseHtml(html);
        var report = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Equal(2, report.Summary.GnMatched);
        Assert.Equal(0, report.Summary.DcsGnUnmatched);
        Assert.Equal(0, report.Summary.MohaGnUnmatched);
        Assert.Equal(0, report.Summary.InvalidLifeCodes);
        Assert.Equal(0, report.Summary.HierarchyMismatches);
        Assert.Equal(0, report.Summary.DuplicateMohaCodes);
        Assert.Equal(2, report.GnCoverage.SinhalaAvailable);
        Assert.Equal(2, report.GnCoverage.TamilAvailable);
        Assert.Equal("unknown", report.Summary.MohaSourceDate ?? "unknown");
        var provenance = report.SampleProvenance["1103005"];
        Assert.Equal(MohaDcsJoiner.DcsSourceId, provenance.English.SourceId);
        Assert.Equal(MohaDcsJoiner.MohaSourceId, provenance.Sinhala.SourceId);
        Assert.Equal(MohaDcsJoiner.MohaSourceId, provenance.Tamil.SourceId);
    }

    [Fact]
    public void Reports_unmatched_dcs_and_moha()
    {
        var dcs = CreateDcsDataset();
        var extra = SyntheticMohaHtmlFactory.Sammanthranapura() with
        {
            LifeCode = "9-1-01-001",
            GnComponent = "001",
            English = "Elsewhere"
        };
        var html = SyntheticMohaHtmlFactory.GnReport(SyntheticMohaHtmlFactory.Sammanthranapura(), extra);
        var moha = new MohaGnReportParser().ParseHtml(html);
        var report = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Equal(1, report.Summary.GnMatched);
        Assert.Equal(1, report.Summary.DcsGnUnmatched);
        Assert.Equal(1, report.Summary.MohaGnUnmatched);
        Assert.Contains(report.UnmatchedDcs, r => r.Code == "1103010");
        Assert.Contains(report.UnmatchedMoha, r => r.Code == "9101001");
    }

    [Fact]
    public void Reports_hierarchy_mismatch()
    {
        var dcs = CreateDcsDataset();
        var row = SyntheticMohaHtmlFactory.Sammanthranapura() with
        {
            DsLabel = "9: කොළඹ/ கொழும்பு/ Colombo"
        };
        var html = SyntheticMohaHtmlFactory.GnReport(row);
        var moha = new MohaGnReportParser().ParseHtml(html);
        var report = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Equal(1, report.Summary.GnMatched);
        Assert.Contains(report.HierarchyMismatches, h => h.Field == "DsLabelPrefix" && h.LifeCode == "1-1-03-005");
    }

    [Fact]
    public void Reports_duplicate_life_codes()
    {
        var dcs = CreateDcsDataset();
        var html = SyntheticMohaHtmlFactory.GnReport(
            SyntheticMohaHtmlFactory.Sammanthranapura(),
            SyntheticMohaHtmlFactory.Sammanthranapura() with { English = "Duplicate" });
        var moha = new MohaGnReportParser().ParseHtml(html);
        var report = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Contains("1103005", report.DuplicateMohaCodes);
        Assert.Contains(report.TranslationConflicts, c => c.Code == "1103005" && c.Field == "English");
    }

    [Fact]
    public void Repeated_ds_names_must_agree()
    {
        var dcs = CreateDcsDataset();
        var conflict = SyntheticMohaHtmlFactory.Mattakkuliya() with
        {
            DsLabel = "3: කොළඹ/ கொழும்பு/ Colombo DS"
        };
        var html = SyntheticMohaHtmlFactory.GnReport(
            SyntheticMohaHtmlFactory.Sammanthranapura(),
            conflict);
        var moha = new MohaGnReportParser().ParseHtml(html);
        var report = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Contains(
            report.TranslationConflicts,
            c => c.EntityType == "DivisionalSecretariat" && c.Code == "1103" && c.Field == "English");
        Assert.Equal(1, report.DsCoverage.TranslationConflicts);
    }

    [Fact]
    public void Missing_sinhala_and_tamil_are_counted()
    {
        var dcs = CreateDcsDataset();
        var html = SyntheticMohaHtmlFactory.GnReport(
            SyntheticMohaHtmlFactory.Sammanthranapura() with { Sinhala = "TBD", Tamil = "" },
            SyntheticMohaHtmlFactory.Mattakkuliya());
        var moha = new MohaGnReportParser().ParseHtml(html);
        var report = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Equal(1, report.Summary.MissingMohaSinhala);
        Assert.Equal(1, report.Summary.MissingMohaTamil);
        Assert.Equal(1, report.GnCoverage.SinhalaAvailable);
        Assert.Equal(1, report.GnCoverage.TamilAvailable);
    }

    [Fact]
    public void Does_not_overwrite_dcs_english()
    {
        var dcs = CreateDcsDataset();
        var html = SyntheticMohaHtmlFactory.GnReport(
            SyntheticMohaHtmlFactory.Sammanthranapura() with { English = "Completely Different Name" });
        var moha = new MohaGnReportParser().ParseHtml(html);
        var report = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Equal("Sammanthranapura", dcs.GramaNiladhariDivisions[0].Name.English);
        Assert.Contains(
            report.EnglishDifferences,
            d => d.Code == "1103005" && d.Kind == EnglishNameDifferenceKind.Substantive);
    }

    private static CanonicalDataset CreateDcsDataset()
    {
        return new CanonicalDataset(
            new CanonicalDatasetMetadata(
                "Department of Census and Statistics, Sri Lanka",
                "Test",
                "2024-03-19",
                new DateOnly(2024, 3, 19),
                new DateOnly(2026, 8, 16)),
            [new CanonicalProvince("1", new CanonicalLocalizedName("Western", null, null))],
            [new CanonicalDistrict("11", "1", new CanonicalLocalizedName("Colombo", null, null))],
            [new CanonicalDivisionalSecretariat("1103", "11", new CanonicalLocalizedName("Colombo", null, null))],
            [
                new CanonicalGramaNiladhariDivision(
                    "1103005",
                    "1103",
                    new CanonicalLocalizedName("Sammanthranapura", null, null),
                    "005"),
                new CanonicalGramaNiladhariDivision(
                    "1103010",
                    "1103",
                    new CanonicalLocalizedName("Mattakkuliya", null, null),
                    "010")
            ]);
    }
}
