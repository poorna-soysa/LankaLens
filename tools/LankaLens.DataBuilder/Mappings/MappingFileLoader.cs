using System.Text.Json;
using System.Text.Json.Serialization;

namespace LankaLens.DataBuilder.Mappings;

internal static class MappingFileLoader
{
    public const string DefaultFileName = "moha-to-dcs.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string ResolvePath(string mappingsDirectory) =>
        Path.Combine(mappingsDirectory, DefaultFileName);

    /// <summary>
    /// Loads the mapping file. Missing file yields an empty confirmed set (valid).
    /// Malformed JSON throws.
    /// </summary>
    public static IReadOnlyList<AdministrativeCodeMapping> Load(string mappingsDirectory)
    {
        var path = ResolvePath(mappingsDirectory);
        if (!File.Exists(path))
        {
            return [];
        }

        var json = File.ReadAllText(path);
        var file = JsonSerializer.Deserialize<AdministrativeCodeMappingFile>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize mapping file '{path}'.");

        if (file.Version < 1)
        {
            throw new InvalidOperationException($"Mapping file '{path}' has invalid version '{file.Version}'.");
        }

        var mappings = new List<AdministrativeCodeMapping>();
        foreach (var dto in file.Mappings)
        {
            DateOnly? effective = null;
            if (!string.IsNullOrWhiteSpace(dto.EffectiveDate))
            {
                if (!DateOnly.TryParse(dto.EffectiveDate, out var parsed))
                {
                    throw new InvalidOperationException(
                        $"Mapping '{dto.SourceCode}' → '{dto.TargetCode}' has invalid effectiveDate '{dto.EffectiveDate}'.");
                }

                effective = parsed;
            }

            mappings.Add(new AdministrativeCodeMapping(
                Type: dto.Type?.Trim() ?? string.Empty,
                SourceCode: dto.SourceCode?.Trim() ?? string.Empty,
                TargetCode: dto.TargetCode?.Trim() ?? string.Empty,
                Reason: dto.Reason?.Trim() ?? string.Empty,
                SourceId: dto.SourceId?.Trim() ?? string.Empty,
                Evidence: dto.Evidence?.Trim() ?? string.Empty,
                EvidenceUrl: dto.EvidenceUrl?.Trim() ?? string.Empty,
                EffectiveDate: effective,
                ReviewNote: dto.ReviewNote?.Trim() ?? string.Empty,
                ChildPropagation: string.IsNullOrWhiteSpace(dto.ChildPropagation)
                    ? null
                    : dto.ChildPropagation.Trim(),
                AllowTranslationReuse: dto.AllowTranslationReuse));
        }

        return mappings;
    }
}
