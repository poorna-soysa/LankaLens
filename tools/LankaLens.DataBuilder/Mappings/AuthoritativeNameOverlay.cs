using System.Text.Json.Serialization;

namespace LankaLens.DataBuilder.Mappings;

/// <summary>
/// Authoritative Sinhala/Tamil name overlay for a current DCS entity when the name
/// does not come from a MOHA→DCS code mapping (e.g. Gazette or District/DS site,
/// or MOHA row-label filtering for a known source inconsistency).
/// Does not alter DCS English.
/// </summary>
internal sealed record AuthoritativeNameOverlay(
    string Type,
    string DcsCode,
    string Sinhala,
    string Tamil,
    string SourceOrganization,
    string Evidence,
    string EvidenceUrl,
    string? RetrievedOrPublishedDate,
    string ReviewNote);

internal sealed class AuthoritativeNameOverlayFile
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("overlays")]
    public List<AuthoritativeNameOverlayDto> Overlays { get; set; } = [];
}

internal sealed class AuthoritativeNameOverlayDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("dcsCode")]
    public string DcsCode { get; set; } = string.Empty;

    [JsonPropertyName("sinhala")]
    public string Sinhala { get; set; } = string.Empty;

    [JsonPropertyName("tamil")]
    public string Tamil { get; set; } = string.Empty;

    [JsonPropertyName("sourceOrganization")]
    public string SourceOrganization { get; set; } = string.Empty;

    [JsonPropertyName("evidence")]
    public string Evidence { get; set; } = string.Empty;

    [JsonPropertyName("evidenceUrl")]
    public string EvidenceUrl { get; set; } = string.Empty;

    [JsonPropertyName("retrievedOrPublishedDate")]
    public string? RetrievedOrPublishedDate { get; set; }

    [JsonPropertyName("reviewNote")]
    public string ReviewNote { get; set; } = string.Empty;
}
