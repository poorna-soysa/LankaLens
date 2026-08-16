using LankaLens.DataBuilder.Acquisition;
using LankaLens.DataBuilder.Delta;
using LankaLens.DataBuilder.Generation;
using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Mappings;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;
using LankaLens.DataBuilder.Parsing;
using LankaLens.DataBuilder.Reporting;
using LankaLens.DataBuilder.Sources;
using LankaLens.DataBuilder.Validation;

namespace LankaLens.DataBuilder.Pipeline;

internal sealed record PipelinePaths(
    string SourceDirectory,
    string GeneratedDirectory,
    string? MappingsDirectory = null)
{
    public string ResolvedMappingsDirectory =>
        MappingsDirectory ?? Path.GetFullPath(Path.Combine(SourceDirectory, "..", "mappings"));

    public string CanonicalJsonPath => Path.Combine(GeneratedDirectory, "administrative-divisions.json");

    public string RuntimeEmbeddedJsonPath
    {
        get
        {
            var repoRoot = Path.GetFullPath(Path.Combine(SourceDirectory, "..", ".."));
            return Path.Combine(
                repoRoot,
                "src",
                "LankaLens.AdministrativeDivisions",
                "Data",
                "administrative-divisions.json");
        }
    }

    public string ValidationMarkdownPath => Path.Combine(GeneratedDirectory, "validation-report.md");

    public string ValidationJsonPath => Path.Combine(GeneratedDirectory, "validation-report.json");

    public string ConflictsJsonPath => Path.Combine(GeneratedDirectory, "conflicts.json");

    public string MultilingualCoverageMarkdownPath =>
        Path.Combine(GeneratedDirectory, "multilingual-coverage-report.md");

    public string MohaDcsJoinJsonPath => Path.Combine(GeneratedDirectory, "moha-dcs-join-report.json");

    public string EnglishNameDifferencesMarkdownPath =>
        Path.Combine(GeneratedDirectory, "english-name-differences.md");

    public string AdministrativeDeltaMarkdownPath =>
        Path.Combine(GeneratedDirectory, "administrative-delta-report.md");

    public string UnresolvedMultilingualGapsJsonPath =>
        Path.Combine(GeneratedDirectory, "unresolved-multilingual-gaps.json");

    public string FinalGapResolutionMarkdownPath =>
        Path.Combine(GeneratedDirectory, "final-gap-resolution-report.md");
}

internal sealed record PipelineResult(
    CanonicalDataset Dataset,
    ValidationReport Report,
    IReadOnlyList<RawAdministrativeRecord> RawRecords,
    bool WroteCanonicalJson,
    MohaJoinReport? MohaJoin = null,
    AdministrativeDeltaReport? Delta = null,
    ProjectedCoverageResult? ProjectedCoverage = null,
    IReadOnlyList<AdministrativeCodeMapping>? ConfirmedMappings = null,
    IReadOnlyList<AuthoritativeNameOverlay>? NameOverlays = null);

internal static class DataBuildPipeline
{
    public const string PrimarySourceId = "dcs-administrative-division-codes";
    public const string CountsSourceId = "dcs-no-of-gn-by-ds";
    public const string MohaSourceId = MohaDcsJoiner.MohaSourceId;

    public static PipelineResult Run(
        PipelinePaths paths,
        bool writeCanonicalJsonWhenValid)
    {
        Directory.CreateDirectory(paths.SourceDirectory);
        Directory.CreateDirectory(paths.GeneratedDirectory);

        var catalog = SourceCatalogLoader.Load(paths.SourceDirectory);
        var primary = catalog.Sources.FirstOrDefault(s => s.Id == PrimarySourceId)
            ?? throw new InvalidOperationException(
                $"Source catalog is missing required entry '{PrimarySourceId}'.");

        var primaryPath = SourceCatalogLoader.ResolveSourcePath(paths.SourceDirectory, primary);
        var parser = new GndListWorkbookParser();
        var rawRecords = parser.Parse(primaryPath);

        DateOnly? effectiveDate = null;
        if (DateOnly.TryParse(primary.PublishedOrUpdatedDate, out var parsedEffective))
        {
            effectiveDate = parsedEffective;
        }

        if (!DateOnly.TryParse(primary.RetrievedDate, out var retrievedDate))
        {
            throw new InvalidOperationException(
                $"Source catalog entry '{PrimarySourceId}' is missing a valid retrievedDate.");
        }

        var dcsMetadata = new CanonicalDatasetMetadata(
            SourceOrganization: primary.Organization,
            SourceName: primary.Title,
            SourceVersion: primary.PublishedOrUpdatedDate,
            EffectiveDate: effectiveDate,
            RetrievedDate: retrievedDate);

        var dcsDataset = CanonicalNormalizer.Normalize(rawRecords, dcsMetadata);

        IReadOnlyList<OfficialCountExpectation>? districtExpectations = null;
        IReadOnlyList<OfficialCountExpectation>? dsExpectations = null;
        var countsEntry = catalog.Sources.FirstOrDefault(s => s.Id == CountsSourceId);
        if (countsEntry is not null)
        {
            var countsPath = Path.Combine(paths.SourceDirectory, countsEntry.FileName);
            if (File.Exists(countsPath))
            {
                SourceCatalogLoader.VerifyFileHash(countsPath, countsEntry.Sha256);
                var countsParser = new OfficialCountsWorkbookParser();
                districtExpectations = countsParser.ParseDistrictTotals(countsPath);
                dsExpectations = countsParser.ParseDsTotals(countsPath);
            }
        }

        var sources = catalog.Sources
            .Select(s => $"{s.Organization} - {s.Title} ({s.FileName})")
            .ToList();

        MohaJoinReport? mohaJoin = null;
        AdministrativeDeltaReport? delta = null;
        ProjectedCoverageResult? projected = null;
        LocalizedNameMaps? nameMaps = null;
        IReadOnlyList<AdministrativeCodeMapping> confirmedMappings = [];
        IReadOnlyList<AuthoritativeNameOverlay> nameOverlays = [];
        MohaParseResult? mohaParsed = null;
        string? mohaLoadError = null;

        var mohaEntry = catalog.Sources.FirstOrDefault(s => s.Id == MohaSourceId);
        if (mohaEntry is not null)
        {
            if (MohaLifeReportClient.TryLoadVerifiedSnapshot(
                paths.SourceDirectory,
                mohaEntry,
                out var manifest,
                out var mohaError)
                && manifest is not null)
            {
                var mohaParser = new MohaGnReportParser();
                mohaParsed = mohaParser.ParseDirectory(
                    MohaLifeReportClient.ReportsDirectory(paths.SourceDirectory));
                mohaJoin = MohaDcsJoiner.Join(
                    dcsDataset,
                    mohaParsed,
                    manifest.RetrievedDate,
                    string.IsNullOrWhiteSpace(manifest.SourceDate) ? null : manifest.SourceDate);

                Directory.CreateDirectory(paths.ResolvedMappingsDirectory);
                confirmedMappings = MappingFileLoader.Load(paths.ResolvedMappingsDirectory);
                var mappingValidation = MappingFileValidator.Validate(confirmedMappings, dcsDataset, mohaParsed);
                if (!mappingValidation.Passed)
                {
                    var detail = string.Join("; ", mappingValidation.Issues.Select(i => $"{i.Code}: {i.Message}"));
                    throw new InvalidOperationException(
                        $"Mapping file validation failed ({MappingFileLoader.ResolvePath(paths.ResolvedMappingsDirectory)}): {detail}");
                }

                nameOverlays = AuthoritativeNameOverlayLoader.Load(paths.ResolvedMappingsDirectory);
                var overlayValidation = AuthoritativeNameOverlayValidator.Validate(nameOverlays, dcsDataset);
                if (!overlayValidation.Passed)
                {
                    var detail = string.Join("; ", overlayValidation.Issues.Select(i => $"{i.Code}: {i.Message}"));
                    throw new InvalidOperationException(
                        $"Overlay file validation failed ({AuthoritativeNameOverlayLoader.ResolvePath(paths.ResolvedMappingsDirectory)}): {detail}");
                }

                var application = MappingApplicator.Apply(
                    dcsDataset,
                    mohaParsed,
                    mohaJoin,
                    confirmedMappings,
                    nameOverlays);
                projected = application.Coverage;
                nameMaps = application.Names;

                delta = DeltaAnalyzer.Analyze(
                    dcsDataset,
                    mohaParsed,
                    mohaJoin,
                    confirmedMappings,
                    projected,
                    nameOverlays);

                MohaCoverageReportWriter.Write(mohaJoin, paths.MultilingualCoverageMarkdownPath, projected);
                MohaJoinReportWriter.Write(mohaJoin, paths.MohaDcsJoinJsonPath);
                EnglishDifferenceReportWriter.Write(mohaJoin, paths.EnglishNameDifferencesMarkdownPath);
                AdministrativeDeltaReportWriter.WriteMarkdown(
                    delta,
                    projected,
                    mohaJoin,
                    confirmedMappings,
                    dcsDataset,
                    paths.AdministrativeDeltaMarkdownPath);
                AdministrativeDeltaReportWriter.WriteUnresolvedGapsJson(
                    delta,
                    paths.UnresolvedMultilingualGapsJsonPath);
                FinalGapResolutionReportWriter.Write(
                    delta,
                    projected,
                    mohaJoin,
                    confirmedMappings,
                    nameOverlays,
                    dcsDataset,
                    paths.FinalGapResolutionMarkdownPath);
            }
            else
            {
                mohaLoadError = mohaError ?? "MOHA snapshot could not be loaded.";
                Console.WriteLine($"MOHA snapshot not applied: {mohaLoadError}");
            }
        }

        CanonicalDataset dataset;
        IReadOnlySet<string>? allowedUnresolvedGnCodes = null;
        SnapshotExpectations? snapshotExpectations = null;

        var hasAuthoritativeLanguages = DatasetHasAnySinhalaOrTamil(dcsDataset);

        if (nameMaps is not null && delta is not null)
        {
            var productionMetadata = ProductionDatasetAssembler.CreateProductionMetadata(dcsMetadata);
            dataset = ProductionDatasetAssembler.Assemble(dcsDataset, nameMaps, productionMetadata);
            allowedUnresolvedGnCodes = delta.UnresolvedGaps
                .Where(g => string.Equals(g.Type, "GramaNiladhariDivision", StringComparison.Ordinal))
                .Select(g => g.DcsCode)
                .ToHashSet(StringComparer.Ordinal);
            snapshotExpectations = SnapshotExpectationsLoader.TryLoad(paths.SourceDirectory);
        }
        else if (hasAuthoritativeLanguages)
        {
            // Synthetic / already-trilingual fixtures: validate and optionally write without MOHA.
            dataset = dcsDataset;
        }
        else if (writeCanonicalJsonWhenValid)
        {
            // Production build requires MOHA merge; do not emit English-only JSON.
            dataset = dcsDataset;
            var failReport = DatasetValidator.Validate(
                rawRecords,
                dataset,
                districtExpectations,
                dsExpectations,
                sources);
            failReport.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "MOHA_REQUIRED",
                mohaLoadError
                    ?? "Production canonical JSON requires a verified MOHA LIFe snapshot; English-only DCS data cannot be published."));
            WriteValidationArtifacts(failReport, paths);
            WriteConflicts(failReport, paths, primary, countsEntry, mohaJoin);
            return new PipelineResult(
                dataset,
                failReport,
                rawRecords,
                WroteCanonicalJson: false,
                mohaJoin,
                delta,
                projected,
                confirmedMappings,
                nameOverlays);
        }
        else
        {
            // validate without MOHA: report English-only gaps (pre-merge diagnostic).
            dataset = dcsDataset;
        }

        var report = DatasetValidator.Validate(
            rawRecords,
            dataset,
            districtExpectations,
            dsExpectations,
            sources,
            allowedUnresolvedGnCodes,
            snapshotExpectations);

        WriteValidationArtifacts(report, paths);
        WriteConflicts(report, paths, primary, countsEntry, mohaJoin);

        var wroteCanonical = false;
        if (writeCanonicalJsonWhenValid && report.Passed)
        {
            CanonicalJsonWriter.Write(dataset, paths.CanonicalJsonPath);
            wroteCanonical = true;

            // Keep the runtime embedded copy in sync when writing into the repo generated folder.
            TryCopyToRuntimeEmbedded(paths);
        }
        else if (writeCanonicalJsonWhenValid && !report.Passed && File.Exists(paths.CanonicalJsonPath))
        {
            // Do not overwrite a known-good generated dataset with invalid output.
        }

        return new PipelineResult(
            dataset,
            report,
            rawRecords,
            wroteCanonical,
            mohaJoin,
            delta,
            projected,
            confirmedMappings,
            nameOverlays);
    }

    private static bool DatasetHasAnySinhalaOrTamil(CanonicalDataset dataset) =>
        dataset.Provinces.Any(p => !string.IsNullOrWhiteSpace(p.Name.Sinhala) || !string.IsNullOrWhiteSpace(p.Name.Tamil))
        || dataset.Districts.Any(d => !string.IsNullOrWhiteSpace(d.Name.Sinhala) || !string.IsNullOrWhiteSpace(d.Name.Tamil))
        || dataset.DivisionalSecretariats.Any(d => !string.IsNullOrWhiteSpace(d.Name.Sinhala) || !string.IsNullOrWhiteSpace(d.Name.Tamil))
        || dataset.GramaNiladhariDivisions.Any(g => !string.IsNullOrWhiteSpace(g.Name.Sinhala) || !string.IsNullOrWhiteSpace(g.Name.Tamil));

    private static void WriteValidationArtifacts(ValidationReport report, PipelinePaths paths)
    {
        ValidationReportWriter.WriteMarkdown(report, paths.ValidationMarkdownPath);
        ValidationReportWriter.WriteJson(report, paths.ValidationJsonPath);
    }

    private static void WriteConflicts(
        ValidationReport report,
        PipelinePaths paths,
        SourceEntry primary,
        SourceEntry? countsEntry,
        MohaJoinReport? mohaJoin)
    {
        var conflicts = report.Issues
            .Where(i => i.Code is "PARENT_INCONSISTENCY" or "UID_MISMATCH" or "COUNT_MISMATCH")
            .Select(i => new SourceConflict(
                EntityType: i.EntityType ?? "Unknown",
                EntityCode: i.EntityCode,
                SourceA: primary.FileName,
                SourceB: countsEntry?.FileName ?? "(internal consistency)",
                ConflictingField: i.Code,
                ValueA: null,
                ValueB: null,
                Message: i.Message,
                SourceAId: PrimarySourceId,
                SourceBId: countsEntry is null ? null : CountsSourceId))
            .ToList();

        if (mohaJoin is not null)
        {
            conflicts.AddRange(mohaJoin.Conflicts);
        }

        if (conflicts.Count > 0)
        {
            ValidationReportWriter.WriteConflicts(conflicts, paths.ConflictsJsonPath);
        }
        else if (File.Exists(paths.ConflictsJsonPath))
        {
            File.Delete(paths.ConflictsJsonPath);
        }
    }

    private static void TryCopyToRuntimeEmbedded(PipelinePaths paths)
    {
        try
        {
            var runtimePath = paths.RuntimeEmbeddedJsonPath;
            var runtimeDir = Path.GetDirectoryName(runtimePath);
            if (runtimeDir is null || !Directory.Exists(Path.GetDirectoryName(runtimeDir)))
            {
                return;
            }

            Directory.CreateDirectory(runtimeDir);
            File.Copy(paths.CanonicalJsonPath, runtimePath, overwrite: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: could not copy canonical JSON to runtime embed path: {ex.Message}");
        }
    }
}
