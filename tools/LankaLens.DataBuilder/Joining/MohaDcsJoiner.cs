using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Parsing;

namespace LankaLens.DataBuilder.Joining;

internal static class MohaDcsJoiner
{
    public const string DcsSourceId = "dcs-administrative-division-codes";
    public const string MohaSourceId = MohaGnReportParser.SourceId;

    public static MohaJoinReport Join(
        CanonicalDataset dcs,
        MohaParseResult moha,
        string mohaRetrievedDate,
        string? mohaSourceDate)
    {
        var dcsGnByCode = dcs.GramaNiladhariDivisions.ToDictionary(g => g.Code, StringComparer.Ordinal);
        var dcsDsByCode = dcs.DivisionalSecretariats.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var dcsDistrictByCode = dcs.Districts.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var dcsProvinceByCode = dcs.Provinces.ToDictionary(p => p.Code, StringComparer.Ordinal);

        var mohaByNormalized = moha.Records
            .GroupBy(r => r.NormalizedLifeCode, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var duplicateMoha = mohaByNormalized
            .Where(kv => kv.Value.Count > 1)
            .Select(kv => kv.Key)
            .OrderBy(c => c, StringComparer.Ordinal)
            .ToList();

        var duplicateDcs = dcs.GramaNiladhariDivisions
            .GroupBy(g => g.Code, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        var matched = new List<(CanonicalGramaNiladhariDivision Dcs, IReadOnlyList<RawMohaGnRecord> Moha)>();
        var unmatchedDcs = new List<UnmatchedRecord>();
        var unmatchedMoha = new List<UnmatchedRecord>();
        var hierarchyMismatches = new List<HierarchyMismatch>();

        foreach (var gn in dcs.GramaNiladhariDivisions)
        {
            if (!dcsDsByCode.TryGetValue(gn.DivisionalSecretariatCode, out var ds)
                || !dcsDistrictByCode.TryGetValue(ds.DistrictCode, out var district)
                || !dcsProvinceByCode.TryGetValue(district.ProvinceCode, out var province))
            {
                continue;
            }

            if (!mohaByNormalized.TryGetValue(gn.Code, out var rows))
            {
                unmatchedDcs.Add(new UnmatchedRecord(
                    "GramaNiladhariDivision",
                    gn.Code,
                    gn.Name.English,
                    ProvinceCode: province.Code,
                    DistrictCode: district.Code,
                    DsCode: ds.Code,
                    GnComponent: gn.Code.Length >= 3 ? gn.Code[^3..] : gn.Code,
                    EnglishProvince: province.Name.English,
                    EnglishDistrict: district.Name.English,
                    EnglishDs: ds.Name.English));
                continue;
            }

            matched.Add((gn, rows));
            foreach (var row in rows)
            {
                hierarchyMismatches.AddRange(CompareHierarchy(gn, row, dcsDsByCode, dcsDistrictByCode));
            }
        }

        foreach (var kv in mohaByNormalized)
        {
            if (!dcsGnByCode.ContainsKey(kv.Key))
            {
                var row = kv.Value[0];
                unmatchedMoha.Add(new UnmatchedRecord(
                    "GramaNiladhariDivision",
                    kv.Key,
                    row.EnglishName,
                    row.LifeCode,
                    ProvinceCode: row.HierarchicalProvinceCode,
                    DistrictCode: row.HierarchicalDistrictCode,
                    DsCode: row.HierarchicalDsCode,
                    GnComponent: row.GnComponent,
                    EnglishProvince: row.ProvinceEnglish,
                    EnglishDistrict: row.DistrictEnglish,
                    EnglishDs: row.DsEnglish,
                    Sinhala: row.SinhalaName,
                    Tamil: row.TamilName));
            }
        }

        var mohaDsCodes = moha.Records.Select(r => r.HierarchicalDsCode).ToHashSet(StringComparer.Ordinal);
        foreach (var ds in dcs.DivisionalSecretariats)
        {
            if (!mohaDsCodes.Contains(ds.Code))
            {
                if (!dcsDistrictByCode.TryGetValue(ds.DistrictCode, out var district)
                    || !dcsProvinceByCode.TryGetValue(district.ProvinceCode, out var province))
                {
                    unmatchedDcs.Add(new UnmatchedRecord("DivisionalSecretariat", ds.Code, ds.Name.English));
                    continue;
                }

                unmatchedDcs.Add(new UnmatchedRecord(
                    "DivisionalSecretariat",
                    ds.Code,
                    ds.Name.English,
                    ProvinceCode: province.Code,
                    DistrictCode: district.Code,
                    DsCode: ds.Code,
                    EnglishProvince: province.Name.English,
                    EnglishDistrict: district.Name.English,
                    EnglishDs: ds.Name.English));
            }
        }

        foreach (var code in mohaDsCodes)
        {
            if (!dcsDsByCode.ContainsKey(code))
            {
                var sample = moha.Records.First(r => r.HierarchicalDsCode == code);
                unmatchedMoha.Add(new UnmatchedRecord(
                    "DivisionalSecretariat",
                    code,
                    sample.DsEnglish,
                    sample.LifeCode,
                    ProvinceCode: sample.HierarchicalProvinceCode,
                    DistrictCode: sample.HierarchicalDistrictCode,
                    DsCode: code,
                    EnglishProvince: sample.ProvinceEnglish,
                    EnglishDistrict: sample.DistrictEnglish,
                    EnglishDs: sample.DsEnglish,
                    Sinhala: sample.DsSinhala,
                    Tamil: sample.DsTamil));
            }
        }

        var gnCandidates = mohaByNormalized
            .Select(kv => TranslationConsistencyChecker.Aggregate(
                kv.Key,
                kv.Value.Select(r => (r.EnglishName, r.SinhalaName, r.TamilName))))
            .ToList();
        var matchedGnCandidates = gnCandidates
            .Where(c => dcsGnByCode.ContainsKey(c.Code))
            .ToList();

        // Raw MOHA DS aggregation (all rows) — used for TRANSLATION_CONFLICT reporting (e.g. 5225).
        var dsCandidatesRaw = AggregateLevel(
            moha.Records,
            r => r.HierarchicalDsCode,
            r => (r.DsEnglish, r.DsSinhala, r.DsTamil));

        // Matched-row DS aggregation: MOHA rows that exact-join to DCS GNs under each DCS DS.
        // When those rows still conflict (e.g. DS 5225 mixing Kalmunai North + Sainthamaruthu,
        // or a mislabeled row), narrow to DS English Exact/formatting/spelling-compatible with DCS.
        // Do not majority-vote; do not apply the English filter when there is already a single agreed name.
        var matchedRowsByDcsDs = new Dictionary<string, List<RawMohaGnRecord>>(StringComparer.Ordinal);
        foreach (var item in matched)
        {
            if (!matchedRowsByDcsDs.TryGetValue(item.Dcs.DivisionalSecretariatCode, out var list))
            {
                list = [];
                matchedRowsByDcsDs[item.Dcs.DivisionalSecretariatCode] = list;
            }

            list.AddRange(item.Moha);
        }

        var dsCandidatesForCoverage = new List<MohaEntityCandidate>();
        foreach (var kv in matchedRowsByDcsDs.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            if (!dcsDsByCode.TryGetValue(kv.Key, out var dcsDs))
            {
                continue;
            }

            var rows = kv.Value;
            var candidate = TranslationConsistencyChecker.Aggregate(
                kv.Key,
                rows.Select(r => (r.DsEnglish, r.DsSinhala, r.DsTamil)));

            var hasConflict = candidate.EnglishVariants.Count > 1
                || candidate.SinhalaVariants.Count > 1
                || candidate.TamilVariants.Count > 1;

            if (hasConflict)
            {
                var compatible = rows.Where(r =>
                {
                    var kind = EnglishNameDifferenceClassifier.Classify(dcsDs.Name.English, r.DsEnglish);
                    return kind == EnglishNameDifferenceKind.Exact
                        || EnglishNameDifferenceClassifier.IsFormattingOnly(kind)
                        || kind == EnglishNameDifferenceKind.Spelling;
                }).ToList();

                if (compatible.Count > 0)
                {
                    candidate = TranslationConsistencyChecker.Aggregate(
                        kv.Key,
                        compatible.Select(r => (r.DsEnglish, r.DsSinhala, r.DsTamil)));
                }
            }

            dsCandidatesForCoverage.Add(candidate);
        }

        // For English diffs / unmatched MOHA DS inventory, still use full MOHA DS set.
        var dsCandidatesAll = dsCandidatesRaw;

        var districtCandidates = AggregateLevel(
            moha.Records,
            r => r.HierarchicalDistrictCode,
            r => (r.DistrictEnglish, r.DistrictSinhala, r.DistrictTamil));
        var provinceCandidates = AggregateLevel(
            moha.Records,
            r => r.HierarchicalProvinceCode,
            r => (r.ProvinceEnglish, r.ProvinceSinhala, r.ProvinceTamil));

        var translationConflicts = new List<TranslationConflict>();
        translationConflicts.AddRange(TranslationConsistencyChecker.CheckRepeatedEntities("Province", provinceCandidates));
        translationConflicts.AddRange(TranslationConsistencyChecker.CheckRepeatedEntities("District", districtCandidates));
        // Report raw MOHA DS conflicts (includes 5225 Kalmunai North vs Sainthamaruthu).
        translationConflicts.AddRange(TranslationConsistencyChecker.CheckRepeatedEntities("DivisionalSecretariat", dsCandidatesRaw));
        translationConflicts.AddRange(TranslationConsistencyChecker.CheckRepeatedEntities("GramaNiladhariDivision", gnCandidates));

        var suspicious = new List<TranslationConflict>();
        suspicious.AddRange(TranslationConsistencyChecker.CheckSuspiciousMultilingual("Province", provinceCandidates));
        suspicious.AddRange(TranslationConsistencyChecker.CheckSuspiciousMultilingual("District", districtCandidates));
        suspicious.AddRange(TranslationConsistencyChecker.CheckSuspiciousMultilingual("DivisionalSecretariat", dsCandidatesRaw));
        suspicious.AddRange(TranslationConsistencyChecker.CheckSuspiciousMultilingual("GramaNiladhariDivision", gnCandidates));

        var englishDiffs = new List<EnglishNameDifference>();
        englishDiffs.AddRange(CompareEnglish("Province", dcs.Provinces.Select(p => (p.Code, p.Name.English)), provinceCandidates));
        englishDiffs.AddRange(CompareEnglish("District", dcs.Districts.Select(d => (d.Code, d.Name.English)), districtCandidates));
        englishDiffs.AddRange(CompareEnglish(
            "DivisionalSecretariat",
            dcs.DivisionalSecretariats.Select(d => (d.Code, d.Name.English)),
            dsCandidatesAll));
        englishDiffs.AddRange(CompareEnglish(
            "GramaNiladhariDivision",
            dcs.GramaNiladhariDivisions.Select(g => (g.Code, g.Name.English)),
            gnCandidates));

        var missingEnglish = 0;
        var missingSinhala = 0;
        var missingTamil = 0;
        foreach (var candidate in matchedGnCandidates)
        {
            if (candidate.AgreedEnglish is null)
            {
                missingEnglish++;
            }

            if (candidate.AgreedSinhala is null)
            {
                missingSinhala++;
            }

            if (candidate.AgreedTamil is null)
            {
                missingTamil++;
            }
        }

        var gnCoverage = BuildCoverage(
            "GramaNiladhariDivision",
            dcs.GramaNiladhariDivisions.Select(g => g.Code),
            gnCandidates);
        var dsCoverage = BuildDsCoverage(
            dcs.DivisionalSecretariats.Select(d => d.Code),
            dsCandidatesAll,
            dsCandidatesForCoverage);
        var districtCoverage = BuildCoverage(
            "District",
            dcs.Districts.Select(d => d.Code),
            districtCandidates);
        var provinceCoverage = BuildCoverage(
            "Province",
            dcs.Provinces.Select(p => p.Code),
            provinceCandidates);

        var formatting = englishDiffs.Count(d => EnglishNameDifferenceClassifier.IsFormattingOnly(d.Kind));
        var spellingOrSubstantive = englishDiffs.Count(d =>
            d.Kind is EnglishNameDifferenceKind.Spelling or EnglishNameDifferenceKind.Substantive);
        var exact = englishDiffs.Count(d => d.Kind == EnglishNameDifferenceKind.Exact);
        var englishDifferencesForReport = englishDiffs
            .Where(d => d.Kind != EnglishNameDifferenceKind.Exact)
            .ToList();

        var dcsEffective = dcs.Metadata.EffectiveDate?.ToString("yyyy-MM-dd") ?? "(unknown)";
        var summary = new MohaJoinSummary(
            dcs.Provinces.Count,
            dcs.Districts.Count,
            dcs.DivisionalSecretariats.Count,
            dcs.GramaNiladhariDivisions.Count,
            provinceCandidates.Count,
            districtCandidates.Count,
            dsCandidatesAll.Count,
            mohaByNormalized.Count,
            matched.Count,
            unmatchedDcs.Count(r => r.EntityType == "GramaNiladhariDivision"),
            unmatchedMoha.Count(r => r.EntityType == "GramaNiladhariDivision"),
            duplicateMoha.Count,
            duplicateDcs.Count,
            moha.InvalidCodes.Count,
            hierarchyMismatches.Count,
            missingEnglish,
            missingSinhala,
            missingTamil,
            exact,
            formatting,
            spellingOrSubstantive,
            mohaSourceDate,
            mohaRetrievedDate,
            dcsEffective);

        var conflicts = BuildConflicts(
            unmatchedDcs,
            unmatchedMoha,
            duplicateMoha,
            moha.InvalidCodes,
            hierarchyMismatches,
            translationConflicts);

        var sampleProvenance = new Dictionary<string, FieldProvenance>(StringComparer.Ordinal);
        foreach (var item in matched.Take(5))
        {
            var candidate = matchedGnCandidates.First(c => c.Code == item.Dcs.Code);
            sampleProvenance[item.Dcs.Code] = new FieldProvenance(
                new LanguageProvenance(item.Dcs.Code, DcsSourceId),
                new LanguageProvenance(item.Dcs.Name.English, DcsSourceId),
                new LanguageProvenance(candidate.AgreedSinhala, candidate.AgreedSinhala is null ? null : MohaSourceId),
                new LanguageProvenance(candidate.AgreedTamil, candidate.AgreedTamil is null ? null : MohaSourceId));
        }

        return new MohaJoinReport(
            summary,
            provinceCoverage,
            districtCoverage,
            dsCoverage,
            gnCoverage,
            unmatchedDcs,
            unmatchedMoha,
            moha.InvalidCodes,
            duplicateMoha,
            hierarchyMismatches,
            translationConflicts,
            suspicious,
            englishDifferencesForReport,
            conflicts,
            sampleProvenance);
    }

    private static List<MohaEntityCandidate> AggregateLevel(
        IReadOnlyList<RawMohaGnRecord> records,
        Func<RawMohaGnRecord, string> codeSelector,
        Func<RawMohaGnRecord, (string? English, string? Sinhala, string? Tamil)> names)
    {
        return records
            .GroupBy(codeSelector, StringComparer.Ordinal)
            .Select(g => TranslationConsistencyChecker.Aggregate(g.Key, g.Select(names)))
            .OrderBy(c => c.Code, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<HierarchyMismatch> CompareHierarchy(
        CanonicalGramaNiladhariDivision gn,
        RawMohaGnRecord moha,
        IReadOnlyDictionary<string, CanonicalDivisionalSecretariat> dcsDs,
        IReadOnlyDictionary<string, CanonicalDistrict> dcsDistricts)
    {
        if (!dcsDs.TryGetValue(gn.DivisionalSecretariatCode, out var ds)
            || !dcsDistricts.TryGetValue(ds.DistrictCode, out var district))
        {
            yield break;
        }

        if (!string.Equals(moha.HierarchicalDsCode, gn.DivisionalSecretariatCode, StringComparison.Ordinal))
        {
            yield return new HierarchyMismatch(
                moha.LifeCode,
                moha.NormalizedLifeCode,
                "DivisionalSecretariat",
                moha.HierarchicalDsCode,
                gn.DivisionalSecretariatCode);
        }

        if (!string.Equals(moha.HierarchicalDistrictCode, ds.DistrictCode, StringComparison.Ordinal))
        {
            yield return new HierarchyMismatch(
                moha.LifeCode,
                moha.NormalizedLifeCode,
                "District",
                moha.HierarchicalDistrictCode,
                ds.DistrictCode);
        }

        if (!string.Equals(moha.HierarchicalProvinceCode, district.ProvinceCode, StringComparison.Ordinal))
        {
            yield return new HierarchyMismatch(
                moha.LifeCode,
                moha.NormalizedLifeCode,
                "Province",
                moha.HierarchicalProvinceCode,
                district.ProvinceCode);
        }

        if (!NumericEquals(moha.DsLabelPrefix, moha.DsComponent))
        {
            yield return new HierarchyMismatch(
                moha.LifeCode,
                moha.NormalizedLifeCode,
                "DsLabelPrefix",
                moha.DsLabelPrefix,
                moha.DsComponent);
        }

        if (!NumericEquals(moha.DistrictLabelPrefix, moha.DistrictComponent))
        {
            yield return new HierarchyMismatch(
                moha.LifeCode,
                moha.NormalizedLifeCode,
                "DistrictLabelPrefix",
                moha.DistrictLabelPrefix,
                moha.DistrictComponent);
        }

        if (!NumericEquals(moha.ProvinceLabelPrefix, moha.ProvinceComponent))
        {
            yield return new HierarchyMismatch(
                moha.LifeCode,
                moha.NormalizedLifeCode,
                "ProvinceLabelPrefix",
                moha.ProvinceLabelPrefix,
                moha.ProvinceComponent);
        }
    }

    private static bool NumericEquals(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return true;
        }

        if (!int.TryParse(left, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var a)
            || !int.TryParse(right, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var b))
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        return a == b;
    }

    private static IEnumerable<EnglishNameDifference> CompareEnglish(
        string entityType,
        IEnumerable<(string Code, string? English)> dcsEntities,
        IReadOnlyList<MohaEntityCandidate> moha)
    {
        var mohaByCode = moha.ToDictionary(c => c.Code, StringComparer.Ordinal);
        foreach (var entity in dcsEntities)
        {
            mohaByCode.TryGetValue(entity.Code, out var candidate);
            var mohaEnglish = candidate?.AgreedEnglish;
            var kind = EnglishNameDifferenceClassifier.Classify(entity.English, mohaEnglish);
            yield return new EnglishNameDifference(entityType, entity.Code, entity.English, mohaEnglish, kind);
        }
    }

    private static LevelCoverage BuildCoverage(
        string entityType,
        IEnumerable<string> dcsCodes,
        IReadOnlyList<MohaEntityCandidate> moha)
    {
        var dcsSet = dcsCodes.ToHashSet(StringComparer.Ordinal);
        var mohaByCode = moha.ToDictionary(c => c.Code, StringComparer.Ordinal);
        var matched = dcsSet.Count(code => mohaByCode.ContainsKey(code));
        var sinhala = dcsSet.Count(code =>
            mohaByCode.TryGetValue(code, out var candidate) && candidate.HasSinhala);
        var tamil = dcsSet.Count(code =>
            mohaByCode.TryGetValue(code, out var candidate) && candidate.HasTamil);
        var conflicts = moha
            .Where(c => dcsSet.Contains(c.Code))
            .Count(c => c.EnglishVariants.Count > 1 || c.SinhalaVariants.Count > 1 || c.TamilVariants.Count > 1);
        var dcsWithout = dcsSet.Count(code => !mohaByCode.ContainsKey(code));
        var mohaWithout = moha.Count(c => !dcsSet.Contains(c.Code));
        return new LevelCoverage(
            entityType,
            dcsSet.Count,
            moha.Count,
            matched,
            sinhala,
            tamil,
            conflicts,
            dcsWithout,
            mohaWithout);
    }

    /// <summary>
    /// DS code match uses full MOHA DS set; Sinhala/Tamil availability uses matched-row aggregation only.
    /// Raw translation conflicts are still counted from the full MOHA DS candidate set.
    /// </summary>
    private static LevelCoverage BuildDsCoverage(
        IEnumerable<string> dcsCodes,
        IReadOnlyList<MohaEntityCandidate> allMohaDs,
        IReadOnlyList<MohaEntityCandidate> matchedRowDs)
    {
        var dcsSet = dcsCodes.ToHashSet(StringComparer.Ordinal);
        var allByCode = allMohaDs.ToDictionary(c => c.Code, StringComparer.Ordinal);
        var matchedByCode = matchedRowDs.ToDictionary(c => c.Code, StringComparer.Ordinal);
        var matched = dcsSet.Count(code => allByCode.ContainsKey(code));
        var sinhala = dcsSet.Count(code =>
            matchedByCode.TryGetValue(code, out var candidate) && candidate.HasSinhala);
        var tamil = dcsSet.Count(code =>
            matchedByCode.TryGetValue(code, out var candidate) && candidate.HasTamil);
        var conflicts = allMohaDs
            .Where(c => dcsSet.Contains(c.Code))
            .Count(c => c.EnglishVariants.Count > 1 || c.SinhalaVariants.Count > 1 || c.TamilVariants.Count > 1);
        var dcsWithout = dcsSet.Count(code => !allByCode.ContainsKey(code));
        var mohaWithout = allMohaDs.Count(c => !dcsSet.Contains(c.Code));
        return new LevelCoverage(
            "DivisionalSecretariat",
            dcsSet.Count,
            allMohaDs.Count,
            matched,
            sinhala,
            tamil,
            conflicts,
            dcsWithout,
            mohaWithout);
    }

    private static List<SourceConflict> BuildConflicts(
        IReadOnlyList<UnmatchedRecord> unmatchedDcs,
        IReadOnlyList<UnmatchedRecord> unmatchedMoha,
        IReadOnlyList<string> duplicateMoha,
        IReadOnlyList<InvalidLifeCode> invalid,
        IReadOnlyList<HierarchyMismatch> hierarchy,
        IReadOnlyList<TranslationConflict> translations)
    {
        var conflicts = new List<SourceConflict>();

        foreach (var row in unmatchedDcs)
        {
            conflicts.Add(new SourceConflict(
                row.EntityType,
                row.Code,
                DcsSourceId,
                MohaSourceId,
                "DCS_WITHOUT_MOHA",
                row.English,
                null,
                $"DCS {row.EntityType} '{row.Code}' has no MOHA LIFe match.",
                DcsSourceId,
                MohaSourceId));
        }

        foreach (var row in unmatchedMoha)
        {
            conflicts.Add(new SourceConflict(
                row.EntityType,
                row.Code,
                MohaSourceId,
                DcsSourceId,
                "MOHA_WITHOUT_DCS",
                row.LifeCode ?? row.English,
                null,
                $"MOHA LIFe '{row.LifeCode}' (normalized {row.Code}) has no DCS GND_UID.",
                MohaSourceId,
                DcsSourceId));
        }

        foreach (var code in duplicateMoha)
        {
            conflicts.Add(new SourceConflict(
                "GramaNiladhariDivision",
                code,
                MohaSourceId,
                MohaSourceId,
                "DUPLICATE_MOHA_CODE",
                code,
                null,
                $"Duplicate MOHA normalized LIFe code '{code}'.",
                MohaSourceId,
                MohaSourceId));
        }

        foreach (var item in invalid)
        {
            conflicts.Add(new SourceConflict(
                "GramaNiladhariDivision",
                item.RawValue,
                MohaSourceId,
                DcsSourceId,
                "INVALID_LIFE_CODE",
                item.RawValue,
                null,
                item.Reason,
                MohaSourceId,
                DcsSourceId));
        }

        foreach (var item in hierarchy)
        {
            conflicts.Add(new SourceConflict(
                item.Field,
                item.NormalizedLifeCode,
                MohaSourceId,
                DcsSourceId,
                "HIERARCHY_MISMATCH",
                item.MohaValue,
                item.DcsValue,
                $"MOHA {item.Field} '{item.MohaValue}' does not match DCS '{item.DcsValue}' for LIFe '{item.LifeCode}'.",
                MohaSourceId,
                DcsSourceId));
        }

        foreach (var item in translations)
        {
            conflicts.Add(new SourceConflict(
                item.EntityType,
                item.Code,
                MohaSourceId,
                MohaSourceId,
                "TRANSLATION_CONFLICT",
                item.Field,
                string.Join(" | ", item.Values),
                $"MOHA {item.EntityType} '{item.Code}' has conflicting {item.Field} values.",
                MohaSourceId,
                MohaSourceId));
        }

        return conflicts;
    }
}
