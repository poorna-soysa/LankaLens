using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;
using LankaLens.DataBuilder.Parsing;

namespace LankaLens.DataBuilder.Mappings;

/// <summary>
/// Applies confirmed MOHA→DCS mappings and authoritative name overlays.
/// Builds both coverage counts and the actual Sinhala/Tamil name maps used for production assembly.
/// </summary>
internal static class MappingApplicator
{
    public static MappingApplicationResult Apply(
        CanonicalDataset dcs,
        MohaParseResult moha,
        MohaJoinReport exactJoin,
        IReadOnlyList<AdministrativeCodeMapping> mappings,
        IReadOnlyList<AuthoritativeNameOverlay>? overlays = null)
    {
        overlays ??= [];

        var mohaByNormalized = moha.Records
            .GroupBy(r => r.NormalizedLifeCode, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var dcsGnByCode = dcs.GramaNiladhariDivisions.ToDictionary(g => g.Code, StringComparer.Ordinal);
        var dcsDsByCode = dcs.DivisionalSecretariats.ToDictionary(d => d.Code, StringComparer.Ordinal);

        var gnNames = new Dictionary<string, (string? Sinhala, string? Tamil)>(StringComparer.Ordinal);
        var dsNames = new Dictionary<string, (string? Sinhala, string? Tamil)>(StringComparer.Ordinal);

        // Exact-matched GN codes start with Sinhala/Tamil from MOHA.
        foreach (var gn in dcs.GramaNiladhariDivisions)
        {
            if (!mohaByNormalized.TryGetValue(gn.Code, out var rows))
            {
                continue;
            }

            var candidate = TranslationConsistencyChecker.Aggregate(
                gn.Code,
                rows.Select(r => (r.EnglishName, r.SinhalaName, r.TamilName)));
            SetNames(gnNames, gn.Code, candidate.AgreedSinhala, candidate.AgreedTamil);
        }

        var appliedGnMappings = 0;
        var appliedDsMappings = 0;
        var appliedChildPropagations = 0;

        foreach (var mapping in mappings)
        {
            if (string.Equals(mapping.Type, AdministrativeMappingTypes.GramaNiladhariDivision, StringComparison.Ordinal))
            {
                if (!dcsGnByCode.ContainsKey(mapping.TargetCode)
                    || !mohaByNormalized.TryGetValue(mapping.SourceCode, out var rows))
                {
                    continue;
                }

                appliedGnMappings++;
                if (!mapping.AllowTranslationReuse)
                {
                    continue;
                }

                var candidate = TranslationConsistencyChecker.Aggregate(
                    mapping.TargetCode,
                    rows.Select(r => (r.EnglishName, r.SinhalaName, r.TamilName)));
                SetNames(gnNames, mapping.TargetCode, candidate.AgreedSinhala, candidate.AgreedTamil);
            }
            else if (string.Equals(mapping.Type, AdministrativeMappingTypes.DivisionalSecretariat, StringComparison.Ordinal))
            {
                // Count every applied DS mapping once, whether or not child propagation is set.
                appliedDsMappings++;

                if (string.Equals(
                        mapping.ChildPropagation,
                        AdministrativeMappingTypes.ChildPropagationGnComponentUnchanged,
                        StringComparison.Ordinal)
                    && mapping.AllowTranslationReuse)
                {
                    var mohaByComponent = moha.Records
                        .Where(r => string.Equals(r.HierarchicalDsCode, mapping.SourceCode, StringComparison.Ordinal))
                        .GroupBy(r => r.GnComponent, StringComparer.Ordinal)
                        .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

                    foreach (var gn in dcs.GramaNiladhariDivisions
                                 .Where(g => string.Equals(g.DivisionalSecretariatCode, mapping.TargetCode, StringComparison.Ordinal)))
                    {
                        var component = gn.Code.Length >= 3 ? gn.Code[^3..] : gn.Code;
                        if (!mohaByComponent.TryGetValue(component, out var rows))
                        {
                            continue;
                        }

                        appliedChildPropagations++;
                        var candidate = TranslationConsistencyChecker.Aggregate(
                            gn.Code,
                            rows.Select(r => (r.EnglishName, r.SinhalaName, r.TamilName)));
                        SetNames(gnNames, gn.Code, candidate.AgreedSinhala, candidate.AgreedTamil);
                    }
                }
            }
        }

        // DS-level: aggregate from exact-joined GN rows under each DCS DS.
        // If variants conflict, narrow to DCS-English-compatible MOHA DS labels (5225 fix).
        var matchedGnRowsByDs = BuildMatchedRowsByDcsDs(dcs, mohaByNormalized);

        foreach (var ds in dcs.DivisionalSecretariats)
        {
            if (!matchedGnRowsByDs.TryGetValue(ds.Code, out var matchedRows) || matchedRows.Count == 0)
            {
                continue;
            }

            var candidate = TranslationConsistencyChecker.Aggregate(
                ds.Code,
                matchedRows.Select(r => (r.DsEnglish, r.DsSinhala, r.DsTamil)));

            var hasConflict = candidate.EnglishVariants.Count > 1
                || candidate.SinhalaVariants.Count > 1
                || candidate.TamilVariants.Count > 1;

            if (hasConflict)
            {
                var compatible = matchedRows.Where(r =>
                {
                    var kind = EnglishNameDifferenceClassifier.Classify(ds.Name.English, r.DsEnglish);
                    return kind == EnglishNameDifferenceKind.Exact
                        || EnglishNameDifferenceClassifier.IsFormattingOnly(kind)
                        || kind == EnglishNameDifferenceKind.Spelling;
                }).ToList();

                if (compatible.Count > 0)
                {
                    candidate = TranslationConsistencyChecker.Aggregate(
                        ds.Code,
                        compatible.Select(r => (r.DsEnglish, r.DsSinhala, r.DsTamil)));
                }
            }

            SetNames(dsNames, ds.Code, candidate.AgreedSinhala, candidate.AgreedTamil);
        }

        foreach (var mapping in mappings.Where(m =>
                     string.Equals(m.Type, AdministrativeMappingTypes.DivisionalSecretariat, StringComparison.Ordinal)
                     && m.AllowTranslationReuse))
        {
            if (!dcsDsByCode.ContainsKey(mapping.TargetCode))
            {
                continue;
            }

            var rows = moha.Records
                .Where(r => string.Equals(r.HierarchicalDsCode, mapping.SourceCode, StringComparison.Ordinal))
                .ToList();
            if (rows.Count == 0)
            {
                continue;
            }

            var candidate = TranslationConsistencyChecker.Aggregate(
                mapping.TargetCode,
                rows.Select(r => (r.DsEnglish, r.DsSinhala, r.DsTamil)));
            SetNames(dsNames, mapping.TargetCode, candidate.AgreedSinhala, candidate.AgreedTamil);
        }

        var appliedOverlays = 0;
        foreach (var overlay in overlays)
        {
            var sinhala = MohaNameNormalizer.Normalize(overlay.Sinhala);
            var tamil = MohaNameNormalizer.Normalize(overlay.Tamil);
            // Partial overlays never count toward coverage.
            if (sinhala is null
                || tamil is null
                || !MohaNameNormalizer.HasSinhalaScript(sinhala)
                || !MohaNameNormalizer.HasTamilScript(tamil))
            {
                continue;
            }

            appliedOverlays++;
            if (string.Equals(overlay.Type, AdministrativeMappingTypes.GramaNiladhariDivision, StringComparison.Ordinal))
            {
                gnNames[overlay.DcsCode] = (sinhala, tamil);
            }
            else if (string.Equals(overlay.Type, AdministrativeMappingTypes.DivisionalSecretariat, StringComparison.Ordinal))
            {
                dsNames[overlay.DcsCode] = (sinhala, tamil);
            }
        }

        var provinceNames = BuildLevelNames(
            moha.Records,
            r => r.HierarchicalProvinceCode,
            r => (r.ProvinceEnglish, r.ProvinceSinhala, r.ProvinceTamil));
        var districtNames = BuildLevelNames(
            moha.Records,
            r => r.HierarchicalDistrictCode,
            r => (r.DistrictEnglish, r.DistrictSinhala, r.DistrictTamil));

        var gnSinhala = gnNames.Where(kv => kv.Value.Sinhala is not null).Select(kv => kv.Key).ToHashSet(StringComparer.Ordinal);
        var gnTamil = gnNames.Where(kv => kv.Value.Tamil is not null).Select(kv => kv.Key).ToHashSet(StringComparer.Ordinal);
        var dsSinhala = dsNames.Where(kv => kv.Value.Sinhala is not null).Select(kv => kv.Key).ToHashSet(StringComparer.Ordinal);
        var dsTamil = dsNames.Where(kv => kv.Value.Tamil is not null).Select(kv => kv.Key).ToHashSet(StringComparer.Ordinal);

        var provinceSinhala = provinceNames.Count(kv => kv.Value.Sinhala is not null);
        var provinceTamil = provinceNames.Count(kv => kv.Value.Tamil is not null);
        var districtSinhala = districtNames.Count(kv => kv.Value.Sinhala is not null);
        var districtTamil = districtNames.Count(kv => kv.Value.Tamil is not null);

        // Prefer exact-join coverage counts for province/district (already complete) for report continuity.
        _ = exactJoin;

        var coverage = new ProjectedCoverageResult(
            ProvinceSinhala: provinceSinhala,
            ProvinceTamil: provinceTamil,
            DistrictSinhala: districtSinhala,
            DistrictTamil: districtTamil,
            DsSinhala: dsSinhala.Count,
            DsTamil: dsTamil.Count,
            GnSinhala: gnSinhala.Count,
            GnTamil: gnTamil.Count,
            AppliedGnMappings: appliedGnMappings,
            AppliedDsMappings: appliedDsMappings,
            AppliedChildPropagations: appliedChildPropagations,
            AppliedOverlays: appliedOverlays,
            ExactGnSinhala: exactJoin.GnCoverage.SinhalaAvailable,
            ExactGnTamil: exactJoin.GnCoverage.TamilAvailable,
            ExactDsSinhala: exactJoin.DsCoverage.SinhalaAvailable,
            ExactDsTamil: exactJoin.DsCoverage.TamilAvailable,
            CoveredDsCodes: dsSinhala.Intersect(dsTamil, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal),
            CoveredGnCodes: gnSinhala.Intersect(gnTamil, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal));

        var names = new LocalizedNameMaps(provinceNames, districtNames, dsNames, gnNames);
        return new MappingApplicationResult(coverage, names);
    }

    private static Dictionary<string, (string? Sinhala, string? Tamil)> BuildLevelNames(
        IReadOnlyList<RawMohaGnRecord> records,
        Func<RawMohaGnRecord, string> codeSelector,
        Func<RawMohaGnRecord, (string? English, string? Sinhala, string? Tamil)> nameSelector)
    {
        var result = new Dictionary<string, (string? Sinhala, string? Tamil)>(StringComparer.Ordinal);
        foreach (var group in records.GroupBy(codeSelector, StringComparer.Ordinal))
        {
            var candidate = TranslationConsistencyChecker.Aggregate(
                group.Key,
                group.Select(nameSelector));
            SetNames(result, group.Key, candidate.AgreedSinhala, candidate.AgreedTamil);
        }

        return result;
    }

    private static void SetNames(
        Dictionary<string, (string? Sinhala, string? Tamil)> map,
        string code,
        string? sinhala,
        string? tamil)
    {
        if (sinhala is null && tamil is null)
        {
            return;
        }

        if (map.TryGetValue(code, out var existing))
        {
            map[code] = (sinhala ?? existing.Sinhala, tamil ?? existing.Tamil);
        }
        else
        {
            map[code] = (sinhala, tamil);
        }
    }

    private static Dictionary<string, List<RawMohaGnRecord>> BuildMatchedRowsByDcsDs(
        CanonicalDataset dcs,
        IReadOnlyDictionary<string, List<RawMohaGnRecord>> mohaByNormalized)
    {
        var result = new Dictionary<string, List<RawMohaGnRecord>>(StringComparer.Ordinal);
        foreach (var gn in dcs.GramaNiladhariDivisions)
        {
            if (!mohaByNormalized.TryGetValue(gn.Code, out var rows))
            {
                continue;
            }

            if (!result.TryGetValue(gn.DivisionalSecretariatCode, out var list))
            {
                list = [];
                result[gn.DivisionalSecretariatCode] = list;
            }

            list.AddRange(rows);
        }

        return result;
    }
}

internal sealed record LocalizedNameMaps(
    IReadOnlyDictionary<string, (string? Sinhala, string? Tamil)> Provinces,
    IReadOnlyDictionary<string, (string? Sinhala, string? Tamil)> Districts,
    IReadOnlyDictionary<string, (string? Sinhala, string? Tamil)> DivisionalSecretariats,
    IReadOnlyDictionary<string, (string? Sinhala, string? Tamil)> GramaNiladhariDivisions);

internal sealed record MappingApplicationResult(
    ProjectedCoverageResult Coverage,
    LocalizedNameMaps Names);

internal sealed record ProjectedCoverageResult(
    int ProvinceSinhala,
    int ProvinceTamil,
    int DistrictSinhala,
    int DistrictTamil,
    int DsSinhala,
    int DsTamil,
    int GnSinhala,
    int GnTamil,
    int AppliedGnMappings,
    int AppliedDsMappings,
    int AppliedChildPropagations,
    int AppliedOverlays,
    int ExactGnSinhala,
    int ExactGnTamil,
    int ExactDsSinhala,
    int ExactDsTamil,
    IReadOnlySet<string> CoveredDsCodes,
    IReadOnlySet<string> CoveredGnCodes);
