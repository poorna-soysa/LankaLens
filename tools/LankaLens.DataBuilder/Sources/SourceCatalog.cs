using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LankaLens.DataBuilder.Sources;

internal sealed class SourceCatalog
{
    [JsonPropertyName("sources")]
    public List<SourceEntry> Sources { get; set; } = [];
}

internal sealed class SourceEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("organization")]
    public string Organization { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("pageUrl")]
    public string? PageUrl { get; set; }

    [JsonPropertyName("retrievedDate")]
    public string RetrievedDate { get; set; } = string.Empty;

    [JsonPropertyName("publishedOrUpdatedDate")]
    public string? PublishedOrUpdatedDate { get; set; }

    [JsonPropertyName("originalServerFileName")]
    public string? OriginalServerFileName { get; set; }

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    [JsonPropertyName("byteLength")]
    public long? ByteLength { get; set; }

    [JsonPropertyName("acquisitionMechanism")]
    public string? AcquisitionMechanism { get; set; }

    [JsonPropertyName("reportIdentifier")]
    public string? ReportIdentifier { get; set; }

    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

internal static class SourceCatalogLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static SourceCatalog Load(string sourceDir)
    {
        var path = Path.Combine(sourceDir, "sources.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "Provenance file sources.json was not found under the source directory.",
                path);
        }

        var json = File.ReadAllText(path);
        var catalog = JsonSerializer.Deserialize<SourceCatalog>(json, JsonOptions);

        return catalog ?? new SourceCatalog();
    }

    public static void Save(string sourceDir, SourceCatalog catalog)
    {
        var path = Path.Combine(sourceDir, "sources.json");
        var json = JsonSerializer.Serialize(catalog, JsonOptions) + Environment.NewLine;
        File.WriteAllText(path, json, new System.Text.UTF8Encoding(false));
    }

    public static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }

    public static void VerifyFileHash(string filePath, string expectedSha256)
    {
        var actual = ComputeSha256(filePath);
        if (!string.Equals(actual, expectedSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"SHA-256 mismatch for '{Path.GetFileName(filePath)}'. Expected {expectedSha256}, got {actual}.");
        }
    }

    public static string ResolveSourcePath(string sourceDir, SourceEntry entry)
    {
        var path = Path.Combine(sourceDir, entry.FileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Source file '{entry.FileName}' was not found. Download it from {entry.Url} into the source directory.",
                path);
        }

        if (!string.IsNullOrWhiteSpace(entry.Sha256))
        {
            VerifyFileHash(path, entry.Sha256);
        }

        return path;
    }
}
