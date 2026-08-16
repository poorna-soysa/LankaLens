using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Validation;

internal static class DatasetValidator
{
    private static readonly HashSet<string> PlaceholderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "N/A",
        "Unknown",
        "TODO"
    };

    public static ValidationReport Validate(
        IReadOnlyList<RawAdministrativeRecord> rawRecords,
        CanonicalDataset dataset,
        IReadOnlyList<OfficialCountExpectation>? districtCountExpectations = null,
        IReadOnlyList<OfficialCountExpectation>? dsCountExpectations = null,
        IReadOnlyList<string>? datasetSources = null,
        IReadOnlySet<string>? allowedUnresolvedGnCodes = null,
        SnapshotExpectations? snapshotExpectations = null)
    {
        var report = new ValidationReport
        {
            DatasetSources = datasetSources ?? [],
            ProvinceCount = dataset.Provinces.Count,
            DistrictCount = dataset.Districts.Count,
            DivisionalSecretariatCount = dataset.DivisionalSecretariats.Count,
            GramaNiladhariDivisionCount = dataset.GramaNiladhariDivisions.Count
        };

        ValidateRequiredNames(dataset, report, allowedUnresolvedGnCodes);
        ValidateHierarchy(dataset, report);
        ValidateDuplicateCodes(dataset, report);
        ValidateDuplicateNames(dataset, report);
        ValidateParentConsistency(rawRecords, report);
        ValidateGnUidIdentity(rawRecords, report);
        ValidateCounts(dataset, districtCountExpectations, dsCountExpectations, report);
        if (snapshotExpectations is not null)
        {
            ValidateSnapshotExpectations(dataset, report, snapshotExpectations);
        }

        return report;
    }

    private static void ValidateRequiredNames(
        CanonicalDataset dataset,
        ValidationReport report,
        IReadOnlySet<string>? allowedUnresolvedGnCodes)
    {
        CountMissingBatch(
            dataset.Provinces.Select(p => (p.Code, p.Name)),
            "Province",
            report,
            () => report.MissingEnglishProvinces++,
            () => report.MissingSinhalaProvinces++,
            () => report.MissingTamilProvinces++,
            allowUnresolvedCodes: null);

        foreach (var district in dataset.Districts)
        {
            if (string.IsNullOrWhiteSpace(district.ProvinceCode))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "MISSING_PARENT",
                    "District is missing province code.",
                    "District",
                    district.Code));
            }
        }

        CountMissingBatch(
            dataset.Districts.Select(d => (d.Code, d.Name)),
            "District",
            report,
            () => report.MissingEnglishDistricts++,
            () => report.MissingSinhalaDistricts++,
            () => report.MissingTamilDistricts++,
            allowUnresolvedCodes: null);

        foreach (var ds in dataset.DivisionalSecretariats)
        {
            if (string.IsNullOrWhiteSpace(ds.DistrictCode))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "MISSING_PARENT",
                    "Divisional secretariat is missing district code.",
                    "DivisionalSecretariat",
                    ds.Code));
            }
        }

        CountMissingBatch(
            dataset.DivisionalSecretariats.Select(d => (d.Code, d.Name)),
            "DivisionalSecretariat",
            report,
            () => report.MissingEnglishDivisionalSecretariats++,
            () => report.MissingSinhalaDivisionalSecretariats++,
            () => report.MissingTamilDivisionalSecretariats++,
            allowUnresolvedCodes: null);

        foreach (var gn in dataset.GramaNiladhariDivisions)
        {
            if (string.IsNullOrWhiteSpace(gn.DivisionalSecretariatCode))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "MISSING_PARENT",
                    "Grama Niladhari division is missing divisional secretariat code.",
                    "GramaNiladhariDivision",
                    gn.Code));
            }
        }

        CountMissingBatch(
            dataset.GramaNiladhariDivisions.Select(g => (g.Code, g.Name)),
            "GramaNiladhariDivision",
            report,
            () => report.MissingEnglishGramaNiladhariDivisions++,
            () => report.MissingSinhalaGramaNiladhariDivisions++,
            () => report.MissingTamilGramaNiladhariDivisions++,
            allowUnresolvedCodes: allowedUnresolvedGnCodes);

        report.MissingEnglish =
            report.MissingEnglishProvinces
            + report.MissingEnglishDistricts
            + report.MissingEnglishDivisionalSecretariats
            + report.MissingEnglishGramaNiladhariDivisions;

        report.MissingSinhala =
            report.MissingSinhalaProvinces
            + report.MissingSinhalaDistricts
            + report.MissingSinhalaDivisionalSecretariats
            + report.MissingSinhalaGramaNiladhariDivisions;

        report.MissingTamil =
            report.MissingTamilProvinces
            + report.MissingTamilDistricts
            + report.MissingTamilDivisionalSecretariats
            + report.MissingTamilGramaNiladhariDivisions;
    }

    private static void CountMissingBatch(
        IEnumerable<(string Code, CanonicalLocalizedName Name)> entities,
        string entityType,
        ValidationReport report,
        Action refCountEnglish,
        Action refCountSinhala,
        Action refCountTamil,
        IReadOnlySet<string>? allowUnresolvedCodes)
    {
        const int maxSamplesPerLanguage = 5;
        var missingEnglishSamples = new List<string>();
        var missingSinhalaSamples = new List<string>();
        var missingTamilSamples = new List<string>();
        var unexpectedMissingSamples = new List<string>();
        var missingEnglish = 0;
        var missingSinhala = 0;
        var missingTamil = 0;
        var unexpectedMissing = 0;

        foreach (var (code, name) in entities)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "MISSING_CODE",
                    $"{entityType} is missing an official code.",
                    entityType,
                    code));
            }

            if (string.IsNullOrWhiteSpace(name.English) || IsPlaceholder(name.English))
            {
                missingEnglish++;
                refCountEnglish();
                if (missingEnglishSamples.Count < maxSamplesPerLanguage)
                {
                    missingEnglishSamples.Add(code);
                }
            }

            var sinhalaMissing = IsMissingOptionalLanguage(name.Sinhala);
            var tamilMissing = IsMissingOptionalLanguage(name.Tamil);

            if (sinhalaMissing)
            {
                missingSinhala++;
                refCountSinhala();
                if (missingSinhalaSamples.Count < maxSamplesPerLanguage)
                {
                    missingSinhalaSamples.Add(code);
                }
            }

            if (tamilMissing)
            {
                missingTamil++;
                refCountTamil();
                if (missingTamilSamples.Count < maxSamplesPerLanguage)
                {
                    missingTamilSamples.Add(code);
                }
            }

            if (allowUnresolvedCodes is not null
                && (sinhalaMissing || tamilMissing)
                && !allowUnresolvedCodes.Contains(code))
            {
                unexpectedMissing++;
                if (unexpectedMissingSamples.Count < maxSamplesPerLanguage)
                {
                    unexpectedMissingSamples.Add(code);
                }
            }
        }

        AddMissingLanguageSummary(report, entityType, "MISSING_ENGLISH", "English", missingEnglish, missingEnglishSamples);

        if (allowUnresolvedCodes is null)
        {
            // Strict mode (synthetic / pre-merge): any missing Sinhala/Tamil is an error.
            AddMissingLanguageSummary(report, entityType, "MISSING_SINHALA", "Sinhala", missingSinhala, missingSinhalaSamples);
            AddMissingLanguageSummary(report, entityType, "MISSING_TAMIL", "Tamil", missingTamil, missingTamilSamples);
        }
        else if (unexpectedMissing > 0)
        {
            var sampleText = unexpectedMissingSamples.Count == 0
                ? string.Empty
                : $" Sample codes: {string.Join(", ", unexpectedMissingSamples)}.";
            report.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "UNEXPECTED_MISSING_TRANSLATION",
                $"{unexpectedMissing} {entityType} record(s) are missing Sinhala and/or Tamil outside the documented unresolved-gap set.{sampleText}",
                entityType,
                unexpectedMissing == 1 ? unexpectedMissingSamples.FirstOrDefault() : null));
        }
        else if (entityType != "GramaNiladhariDivision" && (missingSinhala > 0 || missingTamil > 0))
        {
            // Province / District / DS must always be complete in production mode.
            AddMissingLanguageSummary(report, entityType, "MISSING_SINHALA", "Sinhala", missingSinhala, missingSinhalaSamples);
            AddMissingLanguageSummary(report, entityType, "MISSING_TAMIL", "Tamil", missingTamil, missingTamilSamples);
        }
    }

    private static bool IsMissingOptionalLanguage(string? value) =>
        value is null || string.IsNullOrWhiteSpace(value) || IsPlaceholder(value);

    private static bool IsPlaceholder(string? value) =>
        value is not null && PlaceholderNames.Contains(value.Trim());

    private static void AddMissingLanguageSummary(
        ValidationReport report,
        string entityType,
        string code,
        string language,
        int count,
        IReadOnlyList<string> samples)
    {
        if (count <= 0)
        {
            return;
        }

        var sampleText = samples.Count == 0
            ? string.Empty
            : $" Sample codes: {string.Join(", ", samples)}.";

        report.Add(new ValidationIssue(
            ValidationSeverity.Error,
            code,
            $"{count} {entityType} record(s) are missing a {language} name.{sampleText}",
            entityType,
            count == 1 ? samples.FirstOrDefault() : null));
    }

    private static void ValidateSnapshotExpectations(
        CanonicalDataset dataset,
        ValidationReport report,
        SnapshotExpectations expectations)
    {
        CompareCount(report, "Province", expectations.Counts.Provinces, dataset.Provinces.Count);
        CompareCount(report, "District", expectations.Counts.Districts, dataset.Districts.Count);
        CompareCount(
            report,
            "DivisionalSecretariat",
            expectations.Counts.DivisionalSecretariats,
            dataset.DivisionalSecretariats.Count);
        CompareCount(
            report,
            "GramaNiladhariDivision",
            expectations.Counts.GramaNiladhariDivisions,
            dataset.GramaNiladhariDivisions.Count);

        CompareCoverage(
            report,
            "Province",
            "Sinhala",
            expectations.Coverage.ProvinceSinhala,
            dataset.Provinces.Count(p => !string.IsNullOrWhiteSpace(p.Name.Sinhala)));
        CompareCoverage(
            report,
            "Province",
            "Tamil",
            expectations.Coverage.ProvinceTamil,
            dataset.Provinces.Count(p => !string.IsNullOrWhiteSpace(p.Name.Tamil)));
        CompareCoverage(
            report,
            "District",
            "Sinhala",
            expectations.Coverage.DistrictSinhala,
            dataset.Districts.Count(d => !string.IsNullOrWhiteSpace(d.Name.Sinhala)));
        CompareCoverage(
            report,
            "District",
            "Tamil",
            expectations.Coverage.DistrictTamil,
            dataset.Districts.Count(d => !string.IsNullOrWhiteSpace(d.Name.Tamil)));
        CompareCoverage(
            report,
            "DivisionalSecretariat",
            "Sinhala",
            expectations.Coverage.DivisionalSecretariatSinhala,
            dataset.DivisionalSecretariats.Count(d => !string.IsNullOrWhiteSpace(d.Name.Sinhala)));
        CompareCoverage(
            report,
            "DivisionalSecretariat",
            "Tamil",
            expectations.Coverage.DivisionalSecretariatTamil,
            dataset.DivisionalSecretariats.Count(d => !string.IsNullOrWhiteSpace(d.Name.Tamil)));
        CompareCoverage(
            report,
            "GramaNiladhariDivision",
            "Sinhala",
            expectations.Coverage.GramaNiladhariSinhala,
            dataset.GramaNiladhariDivisions.Count(g => !string.IsNullOrWhiteSpace(g.Name.Sinhala)));
        CompareCoverage(
            report,
            "GramaNiladhariDivision",
            "Tamil",
            expectations.Coverage.GramaNiladhariTamil,
            dataset.GramaNiladhariDivisions.Count(g => !string.IsNullOrWhiteSpace(g.Name.Tamil)));
    }

    private static void CompareCount(ValidationReport report, string entityType, int expected, int actual)
    {
        if (actual == expected)
        {
            return;
        }

        report.Add(new ValidationIssue(
            ValidationSeverity.Error,
            "UNEXPECTED_COUNT",
            $"{entityType} count is {actual}; expected snapshot count is {expected}.",
            entityType));
    }

    private static void CompareCoverage(
        ValidationReport report,
        string entityType,
        string language,
        int expected,
        int actual)
    {
        if (actual == expected)
        {
            return;
        }

        if (actual < expected)
        {
            report.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "COVERAGE_DECREASE",
                $"{entityType} {language} coverage is {actual}; expected at least {expected} for this source snapshot.",
                entityType));
            return;
        }

        report.Add(new ValidationIssue(
            ValidationSeverity.Error,
            "COVERAGE_INCREASE",
            $"{entityType} {language} coverage is {actual}; expected {expected}. Update snapshot-expectations.json and tests deliberately when authoritative coverage increases with provenance.",
            entityType));
    }

    private static void ValidateHierarchy(CanonicalDataset dataset, ValidationReport report)
    {
        var provinceCodes = dataset.Provinces.Select(p => p.Code).ToHashSet(StringComparer.Ordinal);
        var districtCodes = dataset.Districts.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);
        var dsCodes = dataset.DivisionalSecretariats.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);

        foreach (var district in dataset.Districts)
        {
            if (!provinceCodes.Contains(district.ProvinceCode))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "ORPHAN_DISTRICT",
                    $"District '{district.Code}' references unknown province '{district.ProvinceCode}'.",
                    "District",
                    district.Code));
            }
        }

        foreach (var ds in dataset.DivisionalSecretariats)
        {
            if (!districtCodes.Contains(ds.DistrictCode))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "ORPHAN_DS",
                    $"Divisional secretariat '{ds.Code}' references unknown district '{ds.DistrictCode}'.",
                    "DivisionalSecretariat",
                    ds.Code));
            }
        }

        foreach (var gn in dataset.GramaNiladhariDivisions)
        {
            if (!dsCodes.Contains(gn.DivisionalSecretariatCode))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "ORPHAN_GN",
                    $"Grama Niladhari division '{gn.Code}' references unknown DS '{gn.DivisionalSecretariatCode}'.",
                    "GramaNiladhariDivision",
                    gn.Code));
            }
        }
    }

    private static void ValidateDuplicateCodes(CanonicalDataset dataset, ValidationReport report)
    {
        ReportDuplicateCodes(dataset.Provinces.Select(p => p.Code), "Province", report);
        ReportDuplicateCodes(dataset.Districts.Select(d => d.Code), "District", report);
        ReportDuplicateCodes(dataset.DivisionalSecretariats.Select(d => d.Code), "DivisionalSecretariat", report);
        ReportDuplicateCodes(dataset.GramaNiladhariDivisions.Select(g => g.Code), "GramaNiladhariDivision", report);
    }

    private static void ReportDuplicateCodes(
        IEnumerable<string> codes,
        string entityType,
        ValidationReport report)
    {
        foreach (var group in codes.GroupBy(c => c, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            report.Add(new ValidationIssue(
                ValidationSeverity.Error,
                "DUPLICATE_CODE",
                $"Duplicate {entityType} code '{group.Key}' appears {group.Count()} times.",
                entityType,
                group.Key));
        }
    }

    private static void ValidateDuplicateNames(CanonicalDataset dataset, ValidationReport report)
    {
        foreach (var group in dataset.Districts
            .GroupBy(d => $"{d.ProvinceCode}\u001f{(d.Name.English ?? string.Empty).ToLowerInvariant()}", StringComparer.Ordinal)
            .Where(g => g.Key.Split('\u001f')[1].Length > 0 && g.Count() > 1))
        {
            var sample = group.First();
            report.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "DUPLICATE_NAME",
                $"Duplicate English district name '{sample.Name.English}' under province '{sample.ProvinceCode}'.",
                "District",
                string.Join(",", group.Select(d => d.Code))));
        }

        foreach (var group in dataset.DivisionalSecretariats
            .GroupBy(d => $"{d.DistrictCode}\u001f{(d.Name.English ?? string.Empty).ToLowerInvariant()}", StringComparer.Ordinal)
            .Where(g => g.Key.Split('\u001f')[1].Length > 0 && g.Count() > 1))
        {
            var sample = group.First();
            report.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "DUPLICATE_NAME",
                $"Duplicate English DS name '{sample.Name.English}' under district '{sample.DistrictCode}'.",
                "DivisionalSecretariat",
                string.Join(",", group.Select(d => d.Code))));
        }

        foreach (var group in dataset.GramaNiladhariDivisions
            .GroupBy(g => $"{g.DivisionalSecretariatCode}\u001f{(g.Name.English ?? string.Empty).ToLowerInvariant()}", StringComparer.Ordinal)
            .Where(g => g.Key.Split('\u001f')[1].Length > 0 && g.Count() > 1))
        {
            var sample = group.First();
            report.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "DUPLICATE_NAME",
                $"Duplicate English GN name '{sample.Name.English}' under DS '{sample.DivisionalSecretariatCode}'.",
                "GramaNiladhariDivision",
                string.Join(",", group.Select(g => g.Code))));
        }

        // Also check Sinhala / Tamil duplicates within the same parent when values exist.
        ReportLanguageDuplicates(
            dataset.GramaNiladhariDivisions.Select(g => (
                Parent: g.DivisionalSecretariatCode,
                Code: g.Code,
                Name: g.Name.Sinhala)),
            "Sinhala",
            "GramaNiladhariDivision",
            report);

        ReportLanguageDuplicates(
            dataset.GramaNiladhariDivisions.Select(g => (
                Parent: g.DivisionalSecretariatCode,
                Code: g.Code,
                Name: g.Name.Tamil)),
            "Tamil",
            "GramaNiladhariDivision",
            report);
    }

    private static void ReportLanguageDuplicates(
        IEnumerable<(string Parent, string Code, string? Name)> items,
        string language,
        string entityType,
        ValidationReport report)
    {
        foreach (var group in items
            .Where(i => !string.IsNullOrWhiteSpace(i.Name))
            .GroupBy(i => $"{i.Parent}\u001f{i.Name}", StringComparer.Ordinal)
            .Where(g => g.Count() > 1))
        {
            var sample = group.First();
            report.Add(new ValidationIssue(
                ValidationSeverity.Warning,
                "DUPLICATE_NAME",
                $"Duplicate {language} name '{sample.Name}' under parent '{sample.Parent}'.",
                entityType,
                string.Join(",", group.Select(g => g.Code))));
        }
    }

    private static void ValidateParentConsistency(
        IReadOnlyList<RawAdministrativeRecord> rawRecords,
        ValidationReport report)
    {
        var byDistrict = rawRecords
            .Where(r => TextNormalizer.NormalizeCode(r.GnUid) is { Length: >= 2 } uid)
            .GroupBy(r => TextNormalizer.NormalizeCode(r.GnUid)![..2], StringComparer.Ordinal);

        foreach (var group in byDistrict)
        {
            var provinceCodes = group
                .Select(r => TextNormalizer.NormalizeCode(r.ProvinceCode))
                .Where(c => c is not null)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var provinceNames = group
                .Select(r => TextNormalizer.NormalizeOptionalText(r.ProvinceEnglish))
                .Where(n => n is not null)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var districtNames = group
                .Select(r => TextNormalizer.NormalizeOptionalText(r.DistrictEnglish))
                .Where(n => n is not null)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (provinceCodes.Count > 1)
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "PARENT_INCONSISTENCY",
                    $"District code '{group.Key}' maps to multiple province codes: {string.Join(", ", provinceCodes)}.",
                    "District",
                    group.Key));
            }

            if (provinceNames.Count > 1)
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "PARENT_INCONSISTENCY",
                    $"District code '{group.Key}' maps to multiple province English names: {string.Join(", ", provinceNames)}.",
                    "District",
                    group.Key));
            }

            if (districtNames.Count > 1)
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "PARENT_INCONSISTENCY",
                    $"District code '{group.Key}' maps to multiple English names: {string.Join(", ", districtNames)}.",
                    "District",
                    group.Key));
            }
        }

        var byDs = rawRecords
            .Where(r => TextNormalizer.NormalizeCode(r.GnUid) is { Length: >= 4 } uid)
            .GroupBy(r => TextNormalizer.NormalizeCode(r.GnUid)![..4], StringComparer.Ordinal);

        foreach (var group in byDs)
        {
            var districtNames = group
                .Select(r => TextNormalizer.NormalizeOptionalText(r.DistrictEnglish))
                .Where(n => n is not null)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var dsNames = group
                .Select(r => TextNormalizer.NormalizeOptionalText(r.DsEnglish))
                .Where(n => n is not null)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (districtNames.Count > 1 || dsNames.Count > 1)
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "PARENT_INCONSISTENCY",
                    $"DS code '{group.Key}' has inconsistent parent/name values across rows.",
                    "DivisionalSecretariat",
                    group.Key));
            }
        }
    }

    private static void ValidateGnUidIdentity(
        IReadOnlyList<RawAdministrativeRecord> rawRecords,
        ValidationReport report)
    {
        foreach (var raw in rawRecords)
        {
            var uid = TextNormalizer.NormalizeCode(raw.GnUid);
            if (uid is null)
            {
                continue;
            }

            var expected = CanonicalNormalizer.BuildExpectedGnUid(
                raw.ProvinceCode,
                raw.DistrictCode,
                raw.DsCode,
                raw.GnCode);

            if (expected is null)
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    "UID_COMPONENTS_INCOMPLETE",
                    $"Row {raw.SourceRowNumber}: unable to reconstruct GND_UID from component codes.",
                    "GramaNiladhariDivision",
                    uid));
                continue;
            }

            if (!string.Equals(uid, expected, StringComparison.Ordinal))
            {
                report.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "UID_MISMATCH",
                    $"Row {raw.SourceRowNumber}: GND_UID '{uid}' does not match reconstructed '{expected}'.",
                    "GramaNiladhariDivision",
                    uid));
            }
        }
    }

    private static void ValidateCounts(
        CanonicalDataset dataset,
        IReadOnlyList<OfficialCountExpectation>? districtCountExpectations,
        IReadOnlyList<OfficialCountExpectation>? dsCountExpectations,
        ValidationReport report)
    {
        if (districtCountExpectations is { Count: > 0 })
        {
            var actualByDistrict = dataset.GramaNiladhariDivisions
                .Join(
                    dataset.DivisionalSecretariats,
                    gn => gn.DivisionalSecretariatCode,
                    ds => ds.Code,
                    (gn, ds) => ds)
                .Join(
                    dataset.Districts,
                    ds => ds.DistrictCode,
                    d => d.Code,
                    (ds, d) => d)
                .GroupBy(d => NormalizeNameKey(d.Name.English), StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            var actualDsByDistrict = dataset.DivisionalSecretariats
                .Join(
                    dataset.Districts,
                    ds => ds.DistrictCode,
                    d => d.Code,
                    (ds, d) => d)
                .GroupBy(d => NormalizeNameKey(d.Name.English), StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            foreach (var expectation in districtCountExpectations)
            {
                var key = NormalizeNameKey(expectation.DistrictEnglish);
                if (!actualByDistrict.TryGetValue(key, out var actualGn))
                {
                    // Tolerant match for known DCS spelling variants (Killinochchi / Kilinochchi).
                    var fuzzy = actualByDistrict.Keys.FirstOrDefault(k => NamesLikelyMatch(k, key));
                    if (fuzzy is null)
                    {
                        report.Add(new ValidationIssue(
                            ValidationSeverity.Warning,
                            "COUNT_DISTRICT_MISSING",
                            $"Official counts reference district '{expectation.DistrictEnglish}' which was not found in the generated dataset.",
                            "District",
                            expectation.DistrictEnglish));
                        continue;
                    }

                    key = fuzzy;
                    actualGn = actualByDistrict[key];
                }

                if (actualGn != expectation.GnCount)
                {
                    report.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "COUNT_MISMATCH",
                        $"District '{expectation.DistrictEnglish}': expected {expectation.GnCount} GN divisions, found {actualGn}.",
                        "District",
                        expectation.DistrictEnglish));
                }

                if (expectation.DsCount is int expectedDs
                    && actualDsByDistrict.TryGetValue(key, out var actualDs)
                    && actualDs != expectedDs)
                {
                    report.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "COUNT_MISMATCH",
                        $"District '{expectation.DistrictEnglish}': expected {expectedDs} DS divisions, found {actualDs}.",
                        "District",
                        expectation.DistrictEnglish));
                }
            }
        }

        if (dsCountExpectations is { Count: > 0 })
        {
            var actualByDs = dataset.GramaNiladhariDivisions
                .Join(
                    dataset.DivisionalSecretariats,
                    gn => gn.DivisionalSecretariatCode,
                    ds => ds.Code,
                    (gn, ds) => new { gn, ds })
                .Join(
                    dataset.Districts,
                    x => x.ds.DistrictCode,
                    d => d.Code,
                    (x, d) => new
                    {
                        District = NormalizeNameKey(d.Name.English),
                        Ds = NormalizeNameKey(x.ds.Name.English)
                    })
                .GroupBy(x => (x.District, x.Ds))
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var expectation in dsCountExpectations)
            {
                if (expectation.DsEnglish is null)
                {
                    continue;
                }

                var districtKey = NormalizeNameKey(expectation.DistrictEnglish);
                var dsKey = NormalizeNameKey(expectation.DsEnglish);
                var match = actualByDs.Keys.FirstOrDefault(k =>
                    NamesLikelyMatch(k.District, districtKey) && NamesLikelyMatch(k.Ds, dsKey));

                if (match.District is null)
                {
                    report.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "COUNT_DS_MISSING",
                        $"Official counts reference DS '{expectation.DsEnglish}' in district '{expectation.DistrictEnglish}' which was not found.",
                        "DivisionalSecretariat",
                        expectation.DsEnglish));
                    continue;
                }

                var actual = actualByDs[match];
                if (actual != expectation.GnCount)
                {
                    report.Add(new ValidationIssue(
                        ValidationSeverity.Warning,
                        "COUNT_MISMATCH",
                        $"DS '{expectation.DsEnglish}' ({expectation.DistrictEnglish}): expected {expectation.GnCount} GN divisions, found {actual}.",
                        "DivisionalSecretariat",
                        expectation.DsEnglish));
                }
            }
        }
    }

    private static string NormalizeNameKey(string? name)
    {
        return TextNormalizer.NormalizeOptionalText(name)?.ToLowerInvariant() ?? string.Empty;
    }

    private static bool NamesLikelyMatch(string a, string b)
    {
        if (string.Equals(a, b, StringComparison.Ordinal))
        {
            return true;
        }

        // Killinochchi vs Kilinochchi and similar doubled-letter DCS spelling drift.
        var compactA = CompactName(a);
        var compactB = CompactName(b);
        return string.Equals(compactA, compactB, StringComparison.Ordinal);
    }

    private static string CompactName(string value)
    {
        var withoutSpaces = value.Replace(" ", string.Empty, StringComparison.Ordinal);
        var collapsed = new System.Text.StringBuilder(withoutSpaces.Length);
        char? previous = null;
        foreach (var c in withoutSpaces)
        {
            if (previous == c)
            {
                continue;
            }

            collapsed.Append(c);
            previous = c;
        }

        return collapsed.ToString();
    }
}
