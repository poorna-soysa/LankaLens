using LankaLens.DataBuilder.Generation;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;
using LankaLens.DataBuilder.Parsing;
using LankaLens.DataBuilder.Pipeline;
using LankaLens.DataBuilder.Reporting;
using LankaLens.DataBuilder.Tests.Fixtures;
using LankaLens.DataBuilder.Validation;

namespace LankaLens.DataBuilder.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void Valid_trilingual_dataset_passes_validation()
    {
        var (raw, dataset) = Build(SyntheticWorkbookFactory.ValidTrilingualHierarchy(), includeLanguages: true);
        var report = DatasetValidator.Validate(raw, dataset);

        Assert.True(report.Passed);
        Assert.Equal(0, report.MissingEnglish);
        Assert.Equal(0, report.MissingSinhala);
        Assert.Equal(0, report.MissingTamil);
    }

    [Fact]
    public void Missing_tamil_fails_validation()
    {
        var rows = SyntheticWorkbookFactory.ValidTrilingualHierarchy()
            .Select(r => r with { GnTamil = null, ProvinceTamil = null, DistrictTamil = null, DsTamil = null })
            .ToList();

        var (raw, dataset) = Build(rows, includeLanguages: true);
        var report = DatasetValidator.Validate(raw, dataset);

        Assert.False(report.Passed);
        Assert.True(report.MissingTamil > 0);
        Assert.Contains(report.Issues, i => i.Code == "MISSING_TAMIL");
    }

    [Fact]
    public void Missing_sinhala_and_tamil_when_columns_absent()
    {
        using var stream = SyntheticWorkbookFactory.CreateGndList(
            SyntheticWorkbookFactory.ValidTrilingualHierarchy(),
            includeSinhalaTamilColumns: false);
        var raw = new GndListWorkbookParser().Parse(stream);
        var dataset = CanonicalNormalizer.Normalize(raw, Metadata());
        var report = DatasetValidator.Validate(raw, dataset);

        Assert.False(report.Passed);
        Assert.Equal(0, report.MissingEnglish);
        Assert.True(report.MissingSinhala > 0);
        Assert.True(report.MissingTamil > 0);
    }

    [Fact]
    public void Duplicate_gn_uid_is_reported_as_error_when_present_in_canonical_input()
    {
        // CanonicalNormalizer collapses by code; inject duplicates via a crafted dataset.
        var dataset = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("1", new CanonicalLocalizedName("Western", "බස්නාහිර", "மேல்"))],
            [new CanonicalDistrict("11", "1", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalDivisionalSecretariat("1103", "11", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [
                new CanonicalGramaNiladhariDivision("1103005", "1103", new CanonicalLocalizedName("A", "අ", "அ")),
                new CanonicalGramaNiladhariDivision("1103005", "1103", new CanonicalLocalizedName("B", "බ", "ப"))
            ]);

        var report = DatasetValidator.Validate([], dataset);
        Assert.Contains(report.Issues, i => i.Code == "DUPLICATE_CODE" && i.EntityCode == "1103005");
    }

    [Fact]
    public void Orphan_parent_is_reported()
    {
        var dataset = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("1", new CanonicalLocalizedName("Western", "බස්නාහිර", "மேல்"))],
            [new CanonicalDistrict("11", "9", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalDivisionalSecretariat("1103", "99", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalGramaNiladhariDivision("1103005", "9999", new CanonicalLocalizedName("A", "අ", "அ"))]);

        var report = DatasetValidator.Validate([], dataset);
        Assert.Contains(report.Issues, i => i.Code == "ORPHAN_DISTRICT");
        Assert.Contains(report.Issues, i => i.Code == "ORPHAN_DS");
        Assert.Contains(report.Issues, i => i.Code == "ORPHAN_GN");
    }

    [Fact]
    public void Parent_inconsistency_across_rows_is_reported()
    {
        var rows = new List<SyntheticGndRow>
        {
            SyntheticWorkbookFactory.ValidTrilingualHierarchy()[0],
            SyntheticWorkbookFactory.ValidTrilingualHierarchy()[1] with
            {
                ProvinceEnglish = "Western Conflict"
            }
        };

        var (raw, dataset) = Build(rows, includeLanguages: true);
        var report = DatasetValidator.Validate(raw, dataset);
        Assert.Contains(report.Issues, i => i.Code == "PARENT_INCONSISTENCY");
    }

    [Fact]
    public void Uid_mismatch_is_reported()
    {
        var bad = SyntheticWorkbookFactory.ValidTrilingualHierarchy()[0] with
        {
            GnUid = "1103999"
        };
        var (raw, dataset) = Build([bad], includeLanguages: true);
        var report = DatasetValidator.Validate(raw, dataset);
        Assert.Contains(report.Issues, i => i.Code == "UID_MISMATCH");
    }

    [Fact]
    public void Build_does_not_write_canonical_json_when_invalid()
    {
        var temp = CreateTempWorkspace(includeLanguages: false);
        try
        {
            var paths = new PipelinePaths(temp.SourceDir, temp.GeneratedDir);
            var result = DataBuildPipeline.Run(paths, writeCanonicalJsonWhenValid: true);

            Assert.False(result.Report.Passed);
            Assert.False(result.WroteCanonicalJson);
            Assert.False(File.Exists(paths.CanonicalJsonPath));
            Assert.True(File.Exists(paths.ValidationMarkdownPath));
            Assert.True(File.Exists(paths.ValidationJsonPath));
        }
        finally
        {
            Directory.Delete(temp.Root, recursive: true);
        }
    }

    [Fact]
    public void Build_writes_canonical_json_when_valid()
    {
        var temp = CreateTempWorkspace(includeLanguages: true);
        try
        {
            var paths = new PipelinePaths(temp.SourceDir, temp.GeneratedDir);
            var result = DataBuildPipeline.Run(paths, writeCanonicalJsonWhenValid: true);

            Assert.True(result.Report.Passed, string.Join("; ", result.Report.Issues.Select(i => i.Message)));
            Assert.True(result.WroteCanonicalJson);
            Assert.True(File.Exists(paths.CanonicalJsonPath));

            var json = File.ReadAllText(paths.CanonicalJsonPath);
            Assert.Contains("බස්නාහිර", json, StringComparison.Ordinal);
            Assert.Contains("மேல்", json, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(temp.Root, recursive: true);
        }
    }

    [Fact]
    public void Documented_unresolved_gn_nulls_pass_when_allowlisted()
    {
        var dataset = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("1", new CanonicalLocalizedName("Western", "බස්නාහිර", "மேல்"))],
            [new CanonicalDistrict("11", "1", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalDivisionalSecretariat("1103", "11", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [
                new CanonicalGramaNiladhariDivision("1103005", "1103", new CanonicalLocalizedName("A", "අ", "அ")),
                new CanonicalGramaNiladhariDivision("1103010", "1103", new CanonicalLocalizedName("B", null, null))
            ]);

        var allow = new HashSet<string>(StringComparer.Ordinal) { "1103010" };
        var report = DatasetValidator.Validate([], dataset, allowedUnresolvedGnCodes: allow);

        Assert.True(report.Passed, string.Join("; ", report.Issues.Select(i => i.Message)));
        Assert.Equal(1, report.MissingSinhalaGramaNiladhariDivisions);
        Assert.Equal(1, report.MissingTamilGramaNiladhariDivisions);
    }

    [Fact]
    public void Unexpected_missing_gn_translation_fails_when_not_allowlisted()
    {
        var dataset = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("1", new CanonicalLocalizedName("Western", "බස්නාහිර", "மேல்"))],
            [new CanonicalDistrict("11", "1", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalDivisionalSecretariat("1103", "11", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalGramaNiladhariDivision("1103005", "1103", new CanonicalLocalizedName("A", null, null))]);

        var allow = new HashSet<string>(StringComparer.Ordinal); // empty allowlist
        var report = DatasetValidator.Validate([], dataset, allowedUnresolvedGnCodes: allow);

        Assert.False(report.Passed);
        Assert.Contains(report.Issues, i => i.Code == "UNEXPECTED_MISSING_TRANSLATION");
    }

    [Fact]
    public void Canonical_json_serializes_null_not_empty_string_for_missing_languages()
    {
        var dataset = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("1", new CanonicalLocalizedName("Western", "බස්නාහිර", "மேல்"))],
            [new CanonicalDistrict("11", "1", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalDivisionalSecretariat("1103", "11", new CanonicalLocalizedName("Colombo", "කොළඹ", "கொழும்பு"))],
            [new CanonicalGramaNiladhariDivision("1103010", "1103", new CanonicalLocalizedName("Gap", null, null))]);

        var json = CanonicalJsonWriter.Serialize(dataset);
        Assert.Contains("\"sinhala\": null", json, StringComparison.Ordinal);
        Assert.Contains("\"tamil\": null", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"sinhala\": \"\"", json, StringComparison.Ordinal);
        Assert.Contains("බස්නාහිර", json, StringComparison.Ordinal);

        var roundTrip = CanonicalJsonWriter.Deserialize(json);
        Assert.Null(roundTrip.GramaNiladhariDivisions.Single().Name.Sinhala);
        Assert.Equal(CanonicalJsonWriter.Serialize(roundTrip), json);
    }

    private static (IReadOnlyList<RawAdministrativeRecord> Raw, CanonicalDataset Dataset) Build(
        IEnumerable<SyntheticGndRow> rows,
        bool includeLanguages)
    {
        using var stream = SyntheticWorkbookFactory.CreateGndList(rows, includeLanguages);
        var raw = new GndListWorkbookParser().Parse(stream);
        var dataset = CanonicalNormalizer.Normalize(raw, Metadata());
        return (raw, dataset);
    }

    private static CanonicalDatasetMetadata Metadata() =>
        new(
            "Department of Census and Statistics, Sri Lanka",
            "Test fixture",
            "test",
            new DateOnly(2024, 3, 19),
            new DateOnly(2026, 8, 16));

    private static TempWorkspace CreateTempWorkspace(bool includeLanguages)
    {
        var root = Path.Combine(Path.GetTempPath(), "lankalens-db-tests-" + Guid.NewGuid().ToString("n"));
        var sourceDir = Path.Combine(root, "source");
        var generatedDir = Path.Combine(root, "generated");
        Directory.CreateDirectory(sourceDir);
        Directory.CreateDirectory(generatedDir);

        var fileName = "dcs-gndlist-final-2024-03-19.xlsx";
        var path = Path.Combine(sourceDir, fileName);
        using (var stream = SyntheticWorkbookFactory.CreateGndList(
            SyntheticWorkbookFactory.ValidTrilingualHierarchy(),
            includeSinhalaTamilColumns: includeLanguages))
        using (var file = File.Create(path))
        {
            stream.CopyTo(file);
        }

        var sha = LankaLens.DataBuilder.Sources.SourceCatalogLoader.ComputeSha256(path);
        var sourcesJson = $$"""
        {
          "sources": [
            {
              "id": "dcs-administrative-division-codes",
              "organization": "Department of Census and Statistics, Sri Lanka",
              "title": "Administrative Division Codes (test fixture)",
              "url": "https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel",
              "retrievedDate": "2026-08-16",
              "publishedOrUpdatedDate": "2024-03-19",
              "fileName": "{{fileName}}",
              "sha256": "{{sha}}",
              "purpose": "Test fixture"
            }
          ]
        }
        """;
        File.WriteAllText(Path.Combine(sourceDir, "sources.json"), sourcesJson);
        return new TempWorkspace(root, sourceDir, generatedDir);
    }

    private sealed record TempWorkspace(string Root, string SourceDir, string GeneratedDir);
}
