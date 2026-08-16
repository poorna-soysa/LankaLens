using System.Text.Json.Serialization;

namespace LankaLens.DataBuilder.Mappings;

/// <summary>
/// Explicit MOHA → DCS administrative code mapping with full provenance.
/// DataBuilder-only; never invents codes or hides recodes in string rewriting.
/// </summary>
internal sealed record AdministrativeCodeMapping(
    string Type,
    string SourceCode,
    string TargetCode,
    string Reason,
    string SourceId,
    string Evidence,
    string EvidenceUrl,
    DateOnly? EffectiveDate,
    string ReviewNote,
    string? ChildPropagation,
    bool AllowTranslationReuse);

internal sealed class AdministrativeCodeMappingFile
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("mappings")]
    public List<AdministrativeCodeMappingDto> Mappings { get; set; } = [];
}

internal sealed class AdministrativeCodeMappingDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("sourceCode")]
    public string SourceCode { get; set; } = string.Empty;

    [JsonPropertyName("targetCode")]
    public string TargetCode { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("sourceId")]
    public string SourceId { get; set; } = string.Empty;

    [JsonPropertyName("evidence")]
    public string Evidence { get; set; } = string.Empty;

    [JsonPropertyName("evidenceUrl")]
    public string EvidenceUrl { get; set; } = string.Empty;

    [JsonPropertyName("effectiveDate")]
    public string? EffectiveDate { get; set; }

    [JsonPropertyName("reviewNote")]
    public string ReviewNote { get; set; } = string.Empty;

    [JsonPropertyName("childPropagation")]
    public string? ChildPropagation { get; set; }

    [JsonPropertyName("allowTranslationReuse")]
    public bool AllowTranslationReuse { get; set; }
}

internal static class AdministrativeMappingTypes
{
    public const string Province = "Province";
    public const string District = "District";
    public const string DivisionalSecretariat = "DivisionalSecretariat";
    public const string GramaNiladhariDivision = "GramaNiladhariDivision";

    public const string ChildPropagationGnComponentUnchanged = "GnComponentUnchanged";

    public static readonly HashSet<string> Supported = new(StringComparer.Ordinal)
    {
        Province,
        District,
        DivisionalSecretariat,
        GramaNiladhariDivision
    };
}
