using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Parsing;

namespace LankaLens.DataBuilder.Mappings;

internal sealed record MappingValidationIssue(
    string Code,
    string Message);

internal sealed record MappingValidationResult(
    bool Passed,
    IReadOnlyList<MappingValidationIssue> Issues)
{
    public static MappingValidationResult Success() => new(true, []);

    public static MappingValidationResult Failure(IReadOnlyList<MappingValidationIssue> issues) =>
        new(false, issues);
}

internal static class MappingFileValidator
{
    public static MappingValidationResult Validate(
        IReadOnlyList<AdministrativeCodeMapping> mappings,
        CanonicalDataset dcs,
        MohaParseResult moha)
    {
        var issues = new List<MappingValidationIssue>();
        var mohaGnCodes = moha.Records
            .Select(r => r.NormalizedLifeCode)
            .ToHashSet(StringComparer.Ordinal);
        var mohaDsCodes = moha.Records
            .Select(r => r.HierarchicalDsCode)
            .ToHashSet(StringComparer.Ordinal);
        var mohaDistrictCodes = moha.Records
            .Select(r => r.HierarchicalDistrictCode)
            .ToHashSet(StringComparer.Ordinal);
        var mohaProvinceCodes = moha.Records
            .Select(r => r.HierarchicalProvinceCode)
            .ToHashSet(StringComparer.Ordinal);

        var dcsGnByCode = dcs.GramaNiladhariDivisions.ToDictionary(g => g.Code, StringComparer.Ordinal);
        var dcsDsByCode = dcs.DivisionalSecretariats.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var dcsDistrictByCode = dcs.Districts.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var dcsProvinceByCode = dcs.Provinces.ToDictionary(p => p.Code, StringComparer.Ordinal);

        var sourceKeys = new HashSet<string>(StringComparer.Ordinal);
        var targetKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var mapping in mappings)
        {
            ValidateProvenance(mapping, issues);
            ValidateType(mapping, issues);
            ValidateCodesExist(
                mapping,
                mohaGnCodes,
                mohaDsCodes,
                mohaDistrictCodes,
                mohaProvinceCodes,
                dcsGnByCode,
                dcsDsByCode,
                dcsDistrictByCode,
                dcsProvinceByCode,
                issues);

            var sourceKey = $"{mapping.Type}|{mapping.SourceCode}";
            if (!sourceKeys.Add(sourceKey))
            {
                issues.Add(new MappingValidationIssue(
                    "DUPLICATE_SOURCE_MAPPING",
                    $"Duplicate source mapping for {mapping.Type} '{mapping.SourceCode}'."));
            }

            var targetKey = $"{mapping.Type}|{mapping.TargetCode}";
            if (!targetKeys.Add(targetKey))
            {
                issues.Add(new MappingValidationIssue(
                    "DUPLICATE_TARGET_MAPPING",
                    $"Contradictory target mapping for {mapping.Type} '{mapping.TargetCode}' (multiple sources)."));
            }

            if (string.Equals(
                    mapping.ChildPropagation,
                    AdministrativeMappingTypes.ChildPropagationGnComponentUnchanged,
                    StringComparison.Ordinal)
                && string.Equals(
                    mapping.Type,
                    AdministrativeMappingTypes.DivisionalSecretariat,
                    StringComparison.Ordinal))
            {
                ValidateGnComponentBijection(mapping, dcs, moha, issues);
            }
            else if (!string.IsNullOrWhiteSpace(mapping.ChildPropagation)
                     && !string.Equals(
                         mapping.ChildPropagation,
                         AdministrativeMappingTypes.ChildPropagationGnComponentUnchanged,
                         StringComparison.Ordinal))
            {
                issues.Add(new MappingValidationIssue(
                    "UNSUPPORTED_CHILD_PROPAGATION",
                    $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' has unsupported childPropagation '{mapping.ChildPropagation}'."));
            }

            if (mapping.AllowTranslationReuse
                && string.IsNullOrWhiteSpace(mapping.Evidence))
            {
                issues.Add(new MappingValidationIssue(
                    "TRANSLATION_REUSE_WITHOUT_EVIDENCE",
                    $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' allows translation reuse without evidence."));
            }
        }

        return issues.Count == 0
            ? MappingValidationResult.Success()
            : MappingValidationResult.Failure(issues);
    }

    private static void ValidateProvenance(
        AdministrativeCodeMapping mapping,
        List<MappingValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(mapping.SourceCode)
            || string.IsNullOrWhiteSpace(mapping.TargetCode))
        {
            issues.Add(new MappingValidationIssue(
                "MISSING_CODES",
                "Mapping is missing sourceCode or targetCode."));
        }

        if (string.IsNullOrWhiteSpace(mapping.Reason))
        {
            issues.Add(new MappingValidationIssue(
                "MISSING_EVIDENCE",
                $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' is missing reason."));
        }

        if (string.IsNullOrWhiteSpace(mapping.SourceId))
        {
            issues.Add(new MappingValidationIssue(
                "MISSING_EVIDENCE",
                $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' is missing sourceId."));
        }

        if (string.IsNullOrWhiteSpace(mapping.Evidence))
        {
            issues.Add(new MappingValidationIssue(
                "MISSING_EVIDENCE",
                $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' is missing evidence."));
        }

        if (string.IsNullOrWhiteSpace(mapping.EvidenceUrl))
        {
            issues.Add(new MappingValidationIssue(
                "MISSING_EVIDENCE",
                $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' is missing evidenceUrl."));
        }

        if (string.IsNullOrWhiteSpace(mapping.ReviewNote))
        {
            issues.Add(new MappingValidationIssue(
                "MISSING_EVIDENCE",
                $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' is missing reviewNote."));
        }
    }

    private static void ValidateType(
        AdministrativeCodeMapping mapping,
        List<MappingValidationIssue> issues)
    {
        if (!AdministrativeMappingTypes.Supported.Contains(mapping.Type))
        {
            issues.Add(new MappingValidationIssue(
                "UNSUPPORTED_ENTITY_TYPE",
                $"Mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' has unsupported type '{mapping.Type}'."));
        }
    }

    private static void ValidateCodesExist(
        AdministrativeCodeMapping mapping,
        HashSet<string> mohaGn,
        HashSet<string> mohaDs,
        HashSet<string> mohaDistrict,
        HashSet<string> mohaProvince,
        IReadOnlyDictionary<string, CanonicalGramaNiladhariDivision> dcsGn,
        IReadOnlyDictionary<string, CanonicalDivisionalSecretariat> dcsDs,
        IReadOnlyDictionary<string, CanonicalDistrict> dcsDistrict,
        IReadOnlyDictionary<string, CanonicalProvince> dcsProvince,
        List<MappingValidationIssue> issues)
    {
        var sourceKnown = mapping.Type switch
        {
            AdministrativeMappingTypes.GramaNiladhariDivision => mohaGn.Contains(mapping.SourceCode),
            AdministrativeMappingTypes.DivisionalSecretariat => mohaDs.Contains(mapping.SourceCode),
            AdministrativeMappingTypes.District => mohaDistrict.Contains(mapping.SourceCode),
            AdministrativeMappingTypes.Province => mohaProvince.Contains(mapping.SourceCode),
            _ => false
        };

        if (!sourceKnown)
        {
            issues.Add(new MappingValidationIssue(
                "UNKNOWN_SOURCE_CODE",
                $"Mapping source {mapping.Type} '{mapping.SourceCode}' is not present in MOHA snapshot."));
        }

        var targetKnown = mapping.Type switch
        {
            AdministrativeMappingTypes.GramaNiladhariDivision => dcsGn.ContainsKey(mapping.TargetCode),
            AdministrativeMappingTypes.DivisionalSecretariat => dcsDs.ContainsKey(mapping.TargetCode),
            AdministrativeMappingTypes.District => dcsDistrict.ContainsKey(mapping.TargetCode),
            AdministrativeMappingTypes.Province => dcsProvince.ContainsKey(mapping.TargetCode),
            _ => false
        };

        if (!targetKnown)
        {
            issues.Add(new MappingValidationIssue(
                "UNKNOWN_TARGET_CODE",
                $"Mapping target {mapping.Type} '{mapping.TargetCode}' is not present in DCS dataset."));
        }
    }

    private static void ValidateGnComponentBijection(
        AdministrativeCodeMapping mapping,
        CanonicalDataset dcs,
        MohaParseResult moha,
        List<MappingValidationIssue> issues)
    {
        var dcsComponents = dcs.GramaNiladhariDivisions
            .Where(g => string.Equals(g.DivisionalSecretariatCode, mapping.TargetCode, StringComparison.Ordinal))
            .Select(g => g.Code.Length >= 3 ? g.Code[^3..] : g.Code)
            .ToHashSet(StringComparer.Ordinal);

        var mohaComponents = moha.Records
            .Where(r => string.Equals(r.HierarchicalDsCode, mapping.SourceCode, StringComparison.Ordinal))
            .Select(r => r.GnComponent)
            .ToHashSet(StringComparer.Ordinal);

        if (dcsComponents.Count == 0 || mohaComponents.Count == 0)
        {
            issues.Add(new MappingValidationIssue(
                "CHILD_PROPAGATION_NOT_BIJECTION",
                $"DS mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' cannot use GnComponentUnchanged: empty GN set on one side (DCS={dcsComponents.Count}, MOHA={mohaComponents.Count})."));
            return;
        }

        if (!dcsComponents.SetEquals(mohaComponents))
        {
            var onlyDcs = dcsComponents.Except(mohaComponents, StringComparer.Ordinal).OrderBy(c => c).ToList();
            var onlyMoha = mohaComponents.Except(dcsComponents, StringComparer.Ordinal).OrderBy(c => c).ToList();
            issues.Add(new MappingValidationIssue(
                "CHILD_PROPAGATION_NOT_BIJECTION",
                $"DS mapping '{mapping.SourceCode}' → '{mapping.TargetCode}' GN-component sets are not a bijection. OnlyDCS=[{string.Join(',', onlyDcs)}]; OnlyMOHA=[{string.Join(',', onlyMoha)}]."));
        }
    }
}
