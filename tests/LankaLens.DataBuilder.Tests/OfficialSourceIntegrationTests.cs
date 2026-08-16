using LankaLens.DataBuilder.Cli;
using LankaLens.DataBuilder.Generation;
using LankaLens.DataBuilder.Parsing;
using LankaLens.DataBuilder.Pipeline;

namespace LankaLens.DataBuilder.Tests;

public sealed class OfficialSourceIntegrationTests
{
    [Fact]
    public void Official_source_build_writes_production_json_with_expected_coverage()
    {
        var repoRoot = FindRepoRoot();
        var sourceDir = Path.Combine(repoRoot, "data", "source");
        var gndList = Path.Combine(sourceDir, "dcs-gndlist-final-2024-03-19.xlsx");
        var sourcesJson = Path.Combine(sourceDir, "sources.json");
        var mohaManifest = Path.Combine(sourceDir, "moha-life", "manifest.json");

        if (!File.Exists(gndList) || !File.Exists(sourcesJson) || !File.Exists(mohaManifest))
        {
            // Ordinary CI must not depend on redistributed government binaries.
            return;
        }

        var generatedDir = Path.Combine(Path.GetTempPath(), "lankalens-official-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(generatedDir);

        try
        {
            var paths = new PipelinePaths(
                sourceDir,
                generatedDir,
                Path.Combine(repoRoot, "data", "mappings"));
            var result = DataBuildPipeline.Run(paths, writeCanonicalJsonWhenValid: true);

            Assert.True(result.Report.Passed, string.Join("; ", result.Report.Issues.Select(i => $"{i.Code}:{i.Message}")));
            Assert.True(result.WroteCanonicalJson);
            Assert.True(File.Exists(paths.CanonicalJsonPath));

            Assert.Equal(9, result.Report.ProvinceCount);
            Assert.Equal(25, result.Report.DistrictCount);
            Assert.Equal(340, result.Report.DivisionalSecretariatCount);
            Assert.Equal(14008, result.Report.GramaNiladhariDivisionCount);
            Assert.Equal(0, result.Report.MissingEnglish);
            Assert.Equal(285, result.Report.MissingSinhalaGramaNiladhariDivisions);
            Assert.Equal(285, result.Report.MissingTamilGramaNiladhariDivisions);
            Assert.Equal(0, result.Report.MissingSinhalaProvinces);
            Assert.Equal(0, result.Report.MissingTamilProvinces);
            Assert.Equal(0, result.Report.MissingSinhalaDistricts);
            Assert.Equal(0, result.Report.MissingTamilDistricts);
            Assert.Equal(0, result.Report.MissingSinhalaDivisionalSecretariats);
            Assert.Equal(0, result.Report.MissingTamilDivisionalSecretariats);

            Assert.NotNull(result.ProjectedCoverage);
            Assert.Equal(13723, result.ProjectedCoverage.GnSinhala);
            Assert.Equal(13723, result.ProjectedCoverage.GnTamil);
            Assert.Equal(340, result.ProjectedCoverage.DsSinhala);
            Assert.Equal(340, result.ProjectedCoverage.DsTamil);

            var json = File.ReadAllText(paths.CanonicalJsonPath);
            Assert.Contains("\"sinhala\": null", json, StringComparison.Ordinal);
            Assert.Contains("\"tamil\": null", json, StringComparison.Ordinal);
            Assert.DoesNotContain("\"sinhala\": \"\"", json, StringComparison.Ordinal);
            Assert.DoesNotContain("\"tamil\": \"\"", json, StringComparison.Ordinal);
            Assert.DoesNotContain("N/A", json, StringComparison.Ordinal);
            Assert.DoesNotContain("TODO", json, StringComparison.Ordinal);

            var roundTrip = CanonicalJsonWriter.Deserialize(json);
            Assert.Equal(14008, roundTrip.GramaNiladhariDivisions.Count);
            Assert.Equal(13723, roundTrip.GramaNiladhariDivisions.Count(g => g.Name.Sinhala is not null));
            Assert.Equal(285, roundTrip.GramaNiladhariDivisions.Count(g => g.Name.Sinhala is null && g.Name.Tamil is null));

            var again = CanonicalJsonWriter.Serialize(roundTrip);
            Assert.Equal(CanonicalJsonWriter.Serialize(result.Dataset), again);

            Assert.True(File.Exists(paths.ValidationMarkdownPath));
            Assert.True(File.Exists(paths.UnresolvedMultilingualGapsJsonPath));
            Assert.NotNull(result.Delta);
            Assert.Equal(285, result.Delta.UnresolvedGaps.Count(g => g.Type == "GramaNiladhariDivision"));
        }
        finally
        {
            Directory.Delete(generatedDir, recursive: true);
        }
    }

    [Fact]
    public void Generated_and_runtime_embedded_json_are_identical_when_both_exist()
    {
        var repoRoot = FindRepoRoot();
        var generated = Path.Combine(repoRoot, "data", "generated", "administrative-divisions.json");
        var embedded = Path.Combine(
            repoRoot,
            "src",
            "LankaLens.AdministrativeDivisions",
            "Data",
            "administrative-divisions.json");

        if (!File.Exists(generated) || !File.Exists(embedded))
        {
            return;
        }

        var left = File.ReadAllBytes(generated);
        var right = File.ReadAllBytes(embedded);
        Assert.True(left.AsSpan().SequenceEqual(right));
    }

    [Fact]
    public void Inspect_command_succeeds_when_official_source_is_present()
    {
        var repoRoot = FindRepoRoot();
        var sourceDir = Path.Combine(repoRoot, "data", "source");
        var gndList = Path.Combine(sourceDir, "dcs-gndlist-final-2024-03-19.xlsx");
        if (!File.Exists(gndList))
        {
            return;
        }

        var exit = CommandLine.Run(["inspect", "--source-dir", sourceDir]);
        Assert.Equal(0, exit);
    }

    [Fact]
    public void Workbook_inspector_reports_missing_language_columns()
    {
        var repoRoot = FindRepoRoot();
        var gndList = Path.Combine(repoRoot, "data", "source", "dcs-gndlist-final-2024-03-19.xlsx");
        if (!File.Exists(gndList))
        {
            return;
        }

        var report = new WorkbookInspector().Inspect(gndList, sampleRows: 2);
        Assert.Contains("GNDList", report, StringComparison.Ordinal);
        Assert.Contains("no Sinhala name column", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no Tamil name column", report, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "LankaLens.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate LankaLens.sln from the test base directory.");
    }
}
