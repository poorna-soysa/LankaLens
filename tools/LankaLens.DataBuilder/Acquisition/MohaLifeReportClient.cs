using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using LankaLens.DataBuilder.Sources;

namespace LankaLens.DataBuilder.Acquisition;

/// <summary>
/// One-shot acquisition of official MOHA LIFe GN reports.
/// Caches HTML under data/source/moha-life/ and does not re-request districts already on disk.
/// </summary>
internal sealed class MohaLifeReportClient : IDisposable
{
    public const string BaseUrl = "http://moha.gov.lk:8090/lifecode";
    public const string GnListPath = "/gn_list";
    public const string FetchPath = "/views/fetch.php";
    public const string GnReportPath = "/views/rpt_gn_list.php";
    public const string UserAgent = "LankaLens.DataBuilder/3.6 (official-source snapshot; MOHA LIFe GN reports)";

    public const string AcquisitionMechanism =
        "Official generated HTML GN report (POST /lifecode/views/rpt_gn_list.php) per district; cascade IDs from POST /lifecode/views/fetch.php";

    private static readonly JsonSerializerOptions ManifestJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _http;
    private readonly TimeSpan _reportDelay;
    private readonly TextWriter _log;

    public MohaLifeReportClient(HttpClient http, TimeSpan reportDelay, TextWriter log)
    {
        _http = http;
        _reportDelay = reportDelay;
        _log = log;
    }

    public static MohaLifeReportClient Create(TimeSpan reportDelay, TextWriter log)
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl + "/"),
            Timeout = TimeSpan.FromMinutes(3)
        };
        http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        http.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
        return new MohaLifeReportClient(http, reportDelay, log);
    }

    public static string ReportsDirectory(string sourceDirectory) =>
        Path.Combine(sourceDirectory, "moha-life", "reports");

    public static string ManifestPath(string sourceDirectory) =>
        Path.Combine(sourceDirectory, "moha-life", "manifest.json");

    public async Task<MohaSnapshotManifest> AcquireNationalSnapshotAsync(
        string sourceDirectory,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        var reportsDir = ReportsDirectory(sourceDirectory);
        Directory.CreateDirectory(reportsDir);

        _log.WriteLine("MOHA LIFe acquisition");
        _log.WriteLine($"  Mechanism: {AcquisitionMechanism}");
        _log.WriteLine($"  Cache: {reportsDir}");

        var provinces = await DiscoverProvincesAsync(cancellationToken).ConfigureAwait(false);
        _log.WriteLine($"  Provinces discovered: {provinces.Count}");

        var files = new List<MohaReportFileEntry>();
        var firstReport = true;

        foreach (var province in provinces)
        {
            var districts = await FetchDistrictsAsync(province.Id, cancellationToken).ConfigureAwait(false);
            _log.WriteLine($"  Province {province.Id} ({province.Label}): {districts.Count} district(s)");

            foreach (var district in districts)
            {
                var fileName = $"p{province.Id}-d{district.Id}.html";
                var path = Path.Combine(reportsDir, fileName);
                string html;
                DateTimeOffset retrievedUtc;

                if (!forceRefresh && File.Exists(path))
                {
                    html = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                    retrievedUtc = File.GetLastWriteTimeUtc(path);
                    _log.WriteLine($"    Cache hit {fileName} ({district.Label})");
                }
                else
                {
                    if (!firstReport)
                    {
                        await Task.Delay(_reportDelay, cancellationToken).ConfigureAwait(false);
                    }

                    firstReport = false;
                    _log.WriteLine($"    Fetching GN report province={province.Id} district={district.Id} ({district.Label})");
                    html = await FetchGnReportAsync(province.Id, district.Id, cancellationToken).ConfigureAwait(false);
                    await File.WriteAllTextAsync(path, html, new UTF8Encoding(false), cancellationToken)
                        .ConfigureAwait(false);
                    retrievedUtc = DateTimeOffset.UtcNow;
                    _log.WriteLine($"    Saved {fileName} ({html.Length} bytes)");
                }

                var sha = SourceCatalogLoader.ComputeSha256(path);
                files.Add(new MohaReportFileEntry
                {
                    FileName = "reports/" + fileName,
                    ProvinceId = province.Id,
                    ProvinceLabel = province.Label,
                    DistrictId = district.Id,
                    DistrictLabel = district.Label,
                    Sha256 = sha,
                    ByteLength = new FileInfo(path).Length,
                    RetrievedUtc = retrievedUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")
                });
            }
        }

        files = files.OrderBy(f => f.FileName, StringComparer.Ordinal).ToList();
        var combined = ComputeCombinedHash(files);
        var manifest = new MohaSnapshotManifest
        {
            RetrievedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            SourceDate = null,
            AcquisitionMechanism = AcquisitionMechanism,
            ReportEndpoint = BaseUrl + GnReportPath,
            CascadeEndpoint = BaseUrl + FetchPath,
            CombinedSha256 = combined.Hash,
            CombinedByteLength = combined.Bytes,
            ProvinceCount = provinces.Count,
            DistrictCount = files.Count,
            Files = files
        };

        var manifestPath = ManifestPath(sourceDirectory);
        var json = JsonSerializer.Serialize(manifest, ManifestJson) + Environment.NewLine;
        await File.WriteAllTextAsync(manifestPath, json, new UTF8Encoding(false), cancellationToken)
            .ConfigureAwait(false);
        _log.WriteLine($"  Wrote manifest: {manifestPath}");
        _log.WriteLine($"  Combined SHA-256: {manifest.CombinedSha256}");
        _log.WriteLine("  Source date: unknown");
        return manifest;
    }

    public static MohaSnapshotManifest LoadManifest(string sourceDirectory)
    {
        var path = ManifestPath(sourceDirectory);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("MOHA snapshot manifest was not found.", path);
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MohaSnapshotManifest>(json, ManifestJson)
            ?? throw new InvalidOperationException("MOHA snapshot manifest is empty.");
    }

    public static bool TryLoadVerifiedSnapshot(
        string sourceDirectory,
        SourceEntry entry,
        out MohaSnapshotManifest? manifest,
        out string? error)
    {
        manifest = null;
        error = null;
        var path = ManifestPath(sourceDirectory);
        if (!File.Exists(path))
        {
            error = $"MOHA snapshot manifest '{entry.FileName}' was not found. Run acquire-moha.";
            return false;
        }

        manifest = LoadManifest(sourceDirectory);
        var reportsDir = ReportsDirectory(sourceDirectory);
        foreach (var file in manifest.Files)
        {
            var htmlPath = Path.Combine(sourceDirectory, "moha-life", file.FileName.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(htmlPath))
            {
                error = $"MOHA report '{file.FileName}' is missing from {reportsDir}.";
                return false;
            }

            var actual = SourceCatalogLoader.ComputeSha256(htmlPath);
            if (!string.Equals(actual, file.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                error = $"SHA-256 mismatch for MOHA report '{file.FileName}'.";
                return false;
            }
        }

        var combined = ComputeCombinedHash(manifest.Files);
        if (!string.Equals(combined.Hash, manifest.CombinedSha256, StringComparison.OrdinalIgnoreCase))
        {
            error = "MOHA combined SHA-256 does not match the manifest.";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(entry.Sha256)
            && !string.Equals(combined.Hash, entry.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            error = $"SHA-256 mismatch for MOHA snapshot. Expected {entry.Sha256}, got {combined.Hash}.";
            return false;
        }

        return true;
    }

    private async Task<IReadOnlyList<MohaOption>> DiscoverProvincesAsync(CancellationToken cancellationToken)
    {
        using var response = await SendWithRetry(() => new HttpRequestMessage(HttpMethod.Get, "gn_list"), cancellationToken)
            .ConfigureAwait(false);
        var html = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var options = document.DocumentNode.SelectNodes("//select[@id='province']/option[@value]")
            ?? Enumerable.Empty<HtmlNode>();

        var provinces = new List<MohaOption>();
        foreach (var option in options)
        {
            var id = option.GetAttributeValue("value", string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var label = HtmlEntity.DeEntitize(option.InnerText).Trim();
            provinces.Add(new MohaOption(id, label));
        }

        if (provinces.Count == 0)
        {
            throw new InvalidOperationException("Could not discover MOHA provinces from the official GN list page.");
        }

        return provinces;
    }

    private async Task<IReadOnlyList<MohaOption>> FetchDistrictsAsync(string provinceId, CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, string>
        {
            ["action"] = "province",
            ["query"] = provinceId
        };
        using var response = await PostFormWithRetry("views/fetch.php", fields, cancellationToken)
            .ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        using var doc = JsonDocument.Parse(json);
        var output = doc.RootElement.TryGetProperty("output", out var outputEl)
            ? outputEl.GetString() ?? string.Empty
            : string.Empty;

        var districts = new List<MohaOption>();
        foreach (Match match in Regex.Matches(
            output,
            """<option value="(?<id>[^"]+)">(?<label>.*?)</option>""",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            var id = WebUtility.HtmlDecode(match.Groups["id"].Value).Trim();
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            var label = WebUtility.HtmlDecode(match.Groups["label"].Value).Trim();
            districts.Add(new MohaOption(id, label));
        }

        if (districts.Count == 0)
        {
            throw new InvalidOperationException($"MOHA fetch.php returned no districts for province '{provinceId}'.");
        }

        return districts;
    }

    private async Task<string> FetchGnReportAsync(
        string provinceId,
        string districtId,
        CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, string>
        {
            ["province"] = provinceId,
            ["district"] = districtId
        };
        using var response = await PostFormWithRetry("views/rpt_gn_list.php", fields, cancellationToken)
            .ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> PostFormWithRetry(
        string relativeUrl,
        IReadOnlyDictionary<string, string> fields,
        CancellationToken cancellationToken)
    {
        return await SendWithRetry(
            () => new HttpRequestMessage(HttpMethod.Post, relativeUrl)
            {
                Content = new FormUrlEncodedContent(fields)
            },
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendWithRetry(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        Exception? last = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            HttpResponseMessage? response = null;
            try
            {
                using var request = requestFactory();
                response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if ((int)response.StatusCode >= 500)
                {
                    last = new HttpRequestException($"MOHA returned {(int)response.StatusCode}.");
                    response.Dispose();
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                response?.Dispose();
                last = ex;
                await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException("MOHA request failed after retries.", last);
    }

    internal static (string Hash, long Bytes) ComputeCombinedHash(IReadOnlyList<MohaReportFileEntry> files)
    {
        var ordered = files.OrderBy(f => f.FileName, StringComparer.Ordinal).ToList();
        var payload = string.Join('\n', ordered.Select(f => $"{f.FileName}:{f.Sha256}:{f.ByteLength}"));
        var bytes = ordered.Sum(f => f.ByteLength);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        return (hash, bytes);
    }

    private sealed record MohaOption(string Id, string Label);

    public void Dispose() => _http.Dispose();
}
