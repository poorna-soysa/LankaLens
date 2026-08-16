using System.Text.Json;
using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Mappings;

internal static class AuthoritativeNameOverlayLoader
{
    public const string DefaultFileName = "authoritative-name-overlays.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static string ResolvePath(string mappingsDirectory) =>
        Path.Combine(mappingsDirectory, DefaultFileName);

    /// <summary>
    /// Loads overlays. Missing file yields an empty set (valid).
    /// </summary>
    public static IReadOnlyList<AuthoritativeNameOverlay> Load(string mappingsDirectory)
    {
        var path = ResolvePath(mappingsDirectory);
        if (!File.Exists(path))
        {
            return [];
        }

        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<AuthoritativeNameOverlayFile>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize overlay file '{path}'.");

        if (file.Version < 1)
        {
            throw new InvalidOperationException($"Overlay file '{path}' has invalid version '{file.Version}'.");
        }

        return file.Overlays.Select(dto => new AuthoritativeNameOverlay(
            Type: dto.Type?.Trim() ?? string.Empty,
            DcsCode: dto.DcsCode?.Trim() ?? string.Empty,
            Sinhala: dto.Sinhala?.Trim() ?? string.Empty,
            Tamil: dto.Tamil?.Trim() ?? string.Empty,
            SourceOrganization: dto.SourceOrganization?.Trim() ?? string.Empty,
            Evidence: dto.Evidence?.Trim() ?? string.Empty,
            EvidenceUrl: dto.EvidenceUrl?.Trim() ?? string.Empty,
            RetrievedOrPublishedDate: string.IsNullOrWhiteSpace(dto.RetrievedOrPublishedDate)
                ? null
                : dto.RetrievedOrPublishedDate.Trim(),
            ReviewNote: dto.ReviewNote?.Trim() ?? string.Empty)).ToList();
    }
}

internal static class AuthoritativeNameOverlayValidator
{
    public static MappingValidationResult Validate(
        IReadOnlyList<AuthoritativeNameOverlay> overlays,
        CanonicalDataset dcs)
    {
        var issues = new List<MappingValidationIssue>();
        var dcsGn = dcs.GramaNiladhariDivisions.Select(g => g.Code).ToHashSet(StringComparer.Ordinal);
        var dcsDs = dcs.DivisionalSecretariats.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);
        var dcsDistrict = dcs.Districts.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);
        var dcsProvince = dcs.Provinces.Select(p => p.Code).ToHashSet(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var overlay in overlays)
        {
            if (!AdministrativeMappingTypes.Supported.Contains(overlay.Type))
            {
                issues.Add(new MappingValidationIssue(
                    "UNSUPPORTED_ENTITY_TYPE",
                    $"Overlay '{overlay.DcsCode}' has unsupported type '{overlay.Type}'."));
            }

            if (string.IsNullOrWhiteSpace(overlay.DcsCode)
                || string.IsNullOrWhiteSpace(overlay.Evidence)
                || string.IsNullOrWhiteSpace(overlay.EvidenceUrl)
                || string.IsNullOrWhiteSpace(overlay.SourceOrganization)
                || string.IsNullOrWhiteSpace(overlay.ReviewNote))
            {
                issues.Add(new MappingValidationIssue(
                    "MISSING_EVIDENCE",
                    $"Overlay '{overlay.DcsCode}' is missing required provenance fields."));
            }

            var sinhala = MohaNameNormalizer.Normalize(overlay.Sinhala);
            var tamil = MohaNameNormalizer.Normalize(overlay.Tamil);
            if (sinhala is null || tamil is null)
            {
                issues.Add(new MappingValidationIssue(
                    "PARTIAL_TRANSLATION_OVERLAY",
                    $"Overlay '{overlay.DcsCode}' requires both non-empty Sinhala and Tamil (partial translations do not count)."));
            }
            else
            {
                if (!MohaNameNormalizer.HasSinhalaScript(sinhala))
                {
                    issues.Add(new MappingValidationIssue(
                        "OVERLAY_SINHALA_SCRIPT",
                        $"Overlay '{overlay.DcsCode}' Sinhala value lacks Sinhala script."));
                }

                if (!MohaNameNormalizer.HasTamilScript(tamil))
                {
                    issues.Add(new MappingValidationIssue(
                        "OVERLAY_TAMIL_SCRIPT",
                        $"Overlay '{overlay.DcsCode}' Tamil value lacks Tamil script."));
                }
            }

            var known = overlay.Type switch
            {
                AdministrativeMappingTypes.GramaNiladhariDivision => dcsGn.Contains(overlay.DcsCode),
                AdministrativeMappingTypes.DivisionalSecretariat => dcsDs.Contains(overlay.DcsCode),
                AdministrativeMappingTypes.District => dcsDistrict.Contains(overlay.DcsCode),
                AdministrativeMappingTypes.Province => dcsProvince.Contains(overlay.DcsCode),
                _ => false
            };
            if (!known)
            {
                issues.Add(new MappingValidationIssue(
                    "UNKNOWN_TARGET_CODE",
                    $"Overlay target {overlay.Type} '{overlay.DcsCode}' is not present in DCS dataset."));
            }

            var key = $"{overlay.Type}|{overlay.DcsCode}";
            if (!seen.Add(key))
            {
                issues.Add(new MappingValidationIssue(
                    "DUPLICATE_TARGET_MAPPING",
                    $"Duplicate overlay for {overlay.Type} '{overlay.DcsCode}'."));
            }
        }

        return issues.Count == 0
            ? MappingValidationResult.Success()
            : MappingValidationResult.Failure(issues);
    }
}
