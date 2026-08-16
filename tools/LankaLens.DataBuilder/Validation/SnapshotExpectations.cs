using System.Text.Json;
using System.Text.Json.Serialization;

namespace LankaLens.DataBuilder.Validation;

internal sealed record SnapshotExpectations(
    SnapshotCountExpectations Counts,
    SnapshotCoverageExpectations Coverage);

internal sealed record SnapshotCountExpectations(
    int Provinces,
    int Districts,
    int DivisionalSecretariats,
    int GramaNiladhariDivisions);

internal sealed record SnapshotCoverageExpectations(
    int ProvinceSinhala,
    int ProvinceTamil,
    int DistrictSinhala,
    int DistrictTamil,
    int DivisionalSecretariatSinhala,
    int DivisionalSecretariatTamil,
    int GramaNiladhariSinhala,
    int GramaNiladhariTamil);

internal static class SnapshotExpectationsLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static string ResolvePath(string sourceDirectory) =>
        Path.Combine(sourceDirectory, "snapshot-expectations.json");

    public static SnapshotExpectations? TryLoad(string sourceDirectory)
    {
        var path = ResolvePath(sourceDirectory);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<SnapshotExpectationsDto>(json, Options)
            ?? throw new InvalidOperationException($"Failed to deserialize snapshot expectations: {path}");

        if (dto.Counts is null || dto.Coverage is null)
        {
            throw new InvalidOperationException($"Snapshot expectations file is missing counts or coverage: {path}");
        }

        return new SnapshotExpectations(
            new SnapshotCountExpectations(
                dto.Counts.Provinces,
                dto.Counts.Districts,
                dto.Counts.DivisionalSecretariats,
                dto.Counts.GramaNiladhariDivisions),
            new SnapshotCoverageExpectations(
                dto.Coverage.ProvinceSinhala,
                dto.Coverage.ProvinceTamil,
                dto.Coverage.DistrictSinhala,
                dto.Coverage.DistrictTamil,
                dto.Coverage.DivisionalSecretariatSinhala,
                dto.Coverage.DivisionalSecretariatTamil,
                dto.Coverage.GramaNiladhariSinhala,
                dto.Coverage.GramaNiladhariTamil));
    }

    private sealed class SnapshotExpectationsDto
    {
        [JsonPropertyName("counts")]
        public SnapshotCountExpectationsDto? Counts { get; set; }

        [JsonPropertyName("coverage")]
        public SnapshotCoverageExpectationsDto? Coverage { get; set; }
    }

    private sealed class SnapshotCountExpectationsDto
    {
        public int Provinces { get; set; }
        public int Districts { get; set; }
        public int DivisionalSecretariats { get; set; }
        public int GramaNiladhariDivisions { get; set; }
    }

    private sealed class SnapshotCoverageExpectationsDto
    {
        public int ProvinceSinhala { get; set; }
        public int ProvinceTamil { get; set; }
        public int DistrictSinhala { get; set; }
        public int DistrictTamil { get; set; }
        public int DivisionalSecretariatSinhala { get; set; }
        public int DivisionalSecretariatTamil { get; set; }
        public int GramaNiladhariSinhala { get; set; }
        public int GramaNiladhariTamil { get; set; }
    }
}
