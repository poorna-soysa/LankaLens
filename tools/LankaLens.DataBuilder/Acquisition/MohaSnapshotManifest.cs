using System.Text.Json.Serialization;

namespace LankaLens.DataBuilder.Acquisition;

internal sealed class MohaSnapshotManifest
{
    [JsonPropertyName("retrievedDate")]
    public string RetrievedDate { get; set; } = string.Empty;

    [JsonPropertyName("sourceDate")]
    public string? SourceDate { get; set; }

    [JsonPropertyName("acquisitionMechanism")]
    public string AcquisitionMechanism { get; set; } = string.Empty;

    [JsonPropertyName("reportEndpoint")]
    public string ReportEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("cascadeEndpoint")]
    public string CascadeEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("combinedSha256")]
    public string CombinedSha256 { get; set; } = string.Empty;

    [JsonPropertyName("combinedByteLength")]
    public long CombinedByteLength { get; set; }

    [JsonPropertyName("provinceCount")]
    public int ProvinceCount { get; set; }

    [JsonPropertyName("districtCount")]
    public int DistrictCount { get; set; }

    [JsonPropertyName("files")]
    public List<MohaReportFileEntry> Files { get; set; } = [];
}

internal sealed class MohaReportFileEntry
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("provinceId")]
    public string ProvinceId { get; set; } = string.Empty;

    [JsonPropertyName("provinceLabel")]
    public string? ProvinceLabel { get; set; }

    [JsonPropertyName("districtId")]
    public string DistrictId { get; set; } = string.Empty;

    [JsonPropertyName("districtLabel")]
    public string? DistrictLabel { get; set; }

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("byteLength")]
    public long ByteLength { get; set; }

    [JsonPropertyName("retrievedUtc")]
    public string RetrievedUtc { get; set; } = string.Empty;
}
