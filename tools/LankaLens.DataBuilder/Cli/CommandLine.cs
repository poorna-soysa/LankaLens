using LankaLens.DataBuilder.Acquisition;
using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Parsing;
using LankaLens.DataBuilder.Pipeline;
using LankaLens.DataBuilder.Sources;

namespace LankaLens.DataBuilder.Cli;

internal static class CommandLine
{
    public static int Run(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp();
            return args.Length == 0 ? 1 : 0;
        }

        var command = args[0].ToLowerInvariant();
        var options = ParseOptions(args.Skip(1).ToArray());
        var repoRoot = FindRepoRoot();
        var sourceDir = options.SourceDirectory
            ?? Path.Combine(repoRoot, "data", "source");
        var generatedDir = options.GeneratedDirectory
            ?? Path.Combine(repoRoot, "data", "generated");
        var mappingsDir = options.MappingsDirectory
            ?? Path.Combine(repoRoot, "data", "mappings");

        var paths = new PipelinePaths(sourceDir, generatedDir, mappingsDir);

        try
        {
            return command switch
            {
                "inspect" => RunInspect(paths),
                "validate" => RunValidate(paths),
                "build" => RunBuild(paths),
                "acquire-moha" => RunAcquireMoha(paths, options.Force, options.DelayMs).GetAwaiter().GetResult(),
                _ => UnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return 2;
        }
    }

    private static int RunInspect(PipelinePaths paths)
    {
        if (!Directory.Exists(paths.SourceDirectory))
        {
            Console.Error.WriteLine($"Source directory not found: {paths.SourceDirectory}");
            return 2;
        }

        var catalog = SourceCatalogLoader.Load(paths.SourceDirectory);
        var inspector = new WorkbookInspector();
        var any = false;

        foreach (var entry in catalog.Sources)
        {
            var extension = Path.GetExtension(entry.FileName);
            if (!extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                && !extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Skipping non-workbook source: {entry.FileName}");
                Console.WriteLine();
                continue;
            }

            var path = Path.Combine(paths.SourceDirectory, entry.FileName);
            if (!File.Exists(path))
            {
                Console.WriteLine($"Skipping missing source file: {entry.FileName}");
                Console.WriteLine($"  Download from: {entry.Url}");
                Console.WriteLine();
                continue;
            }

            any = true;
            SourceCatalogLoader.VerifyFileHash(path, entry.Sha256);
            Console.Write(inspector.Inspect(path));
        }

        if (!any)
        {
            Console.Error.WriteLine("No source workbooks found to inspect.");
            return 2;
        }

        return 0;
    }

    private static int RunValidate(PipelinePaths paths)
    {
        var result = DataBuildPipeline.Run(paths, writeCanonicalJsonWhenValid: false);
        PrintSummary(result);
        PrintMohaSummary(result);
        return result.Report.Passed ? 0 : 1;
    }

    private static int RunBuild(PipelinePaths paths)
    {
        var result = DataBuildPipeline.Run(paths, writeCanonicalJsonWhenValid: true);
        PrintSummary(result);
        PrintMohaSummary(result);

        if (!result.Report.Passed)
        {
            Console.WriteLine("Build FAILED. Production canonical JSON was not written.");
            Console.WriteLine($"Validation report: {Path.GetFileName(paths.ValidationMarkdownPath)}");
            return 1;
        }

        Console.WriteLine($"Wrote canonical JSON: {Path.GetFileName(paths.CanonicalJsonPath)}");
        return 0;
    }

    private static void PrintSummary(PipelineResult result)
    {
        var report = result.Report;
        Console.WriteLine("LankaLens DataBuilder");
        Console.WriteLine($"  Provinces: {report.ProvinceCount}");
        Console.WriteLine($"  Districts: {report.DistrictCount}");
        Console.WriteLine($"  Divisional Secretariats: {report.DivisionalSecretariatCount}");
        Console.WriteLine($"  Grama Niladhari Divisions: {report.GramaNiladhariDivisionCount}");
        Console.WriteLine($"  Missing English: {report.MissingEnglish}");
        Console.WriteLine($"  Missing Sinhala: {report.MissingSinhala}");
        Console.WriteLine($"  Missing Tamil: {report.MissingTamil}");
        Console.WriteLine($"  Warnings: {report.WarningCount}");
        Console.WriteLine($"  Errors: {report.ErrorCount}");
        Console.WriteLine($"  Status: {(report.Passed ? "PASS" : "FAIL")}");
    }

    private static async Task<int> RunAcquireMoha(PipelinePaths paths, bool force, int delayMs)
    {
        Directory.CreateDirectory(paths.SourceDirectory);
        var delay = TimeSpan.FromMilliseconds(Math.Max(1000, delayMs));
        using var client = MohaLifeReportClient.Create(delay, Console.Out);
        var manifest = await client.AcquireNationalSnapshotAsync(
            paths.SourceDirectory,
            force,
            CancellationToken.None).ConfigureAwait(false);

        UpsertMohaSourceEntry(paths.SourceDirectory, manifest);
        Console.WriteLine("Updated data/source/sources.json with MOHA LIFe provenance.");
        Console.WriteLine("Source date: unknown");
        Console.WriteLine($"Retrieved date: {manifest.RetrievedDate}");
        return 0;
    }

    private static void UpsertMohaSourceEntry(string sourceDirectory, MohaSnapshotManifest manifest)
    {
        var catalog = SourceCatalogLoader.Load(sourceDirectory);
        var existing = catalog.Sources.FirstOrDefault(s => s.Id == MohaDcsJoiner.MohaSourceId);
        if (existing is null)
        {
            existing = new SourceEntry { Id = MohaDcsJoiner.MohaSourceId };
            catalog.Sources.Add(existing);
        }

        existing.Organization = "Ministry of Home Affairs, Sri Lanka — Home Affairs Division (IT Unit)";
        existing.Title = "LIFe Location Codes (Grama Niladhari Division List)";
        existing.Url = MohaLifeReportClient.BaseUrl + MohaLifeReportClient.GnReportPath;
        existing.PageUrl = MohaLifeReportClient.BaseUrl + "/";
        existing.RetrievedDate = manifest.RetrievedDate;
        existing.PublishedOrUpdatedDate = manifest.SourceDate;
        existing.OriginalServerFileName = "rpt_gn_list.php";
        existing.FileName = "moha-life/manifest.json";
        existing.Sha256 = manifest.CombinedSha256;
        existing.ByteLength = manifest.CombinedByteLength;
        existing.AcquisitionMechanism = manifest.AcquisitionMechanism;
        existing.ReportIdentifier = "POST /lifecode/views/rpt_gn_list.php (per district)";
        existing.Purpose =
            "Authoritative Sinhala/Tamil administrative names for DCS codes joinable via LIFe → GND_UID (national validation incomplete for recoded DS/GN units)";
        existing.Notes =
            "No published source date on the LIFe UI or GN reports. Raw HTML is gitignored. Reproduce with: dotnet run --project tools/LankaLens.DataBuilder -- acquire-moha. DataBuilder validate/build never fetch live MOHA.";
        SourceCatalogLoader.Save(sourceDirectory, catalog);
    }

    private static void PrintMohaSummary(PipelineResult result)
    {
        if (result.MohaJoin is null)
        {
            Console.WriteLine("  MOHA join: not applied (snapshot missing)");
            return;
        }

        var join = result.MohaJoin.Summary;
        Console.WriteLine($"  MOHA GN: {join.MohaGramaNiladhariDivisions}");
        Console.WriteLine($"  GN matched: {join.GnMatched}");
        Console.WriteLine($"  DCS unmatched: {join.DcsGnUnmatched}");
        Console.WriteLine($"  MOHA unmatched: {join.MohaGnUnmatched}");
        Console.WriteLine($"  Invalid LIFe codes: {join.InvalidLifeCodes}");
        Console.WriteLine($"  Hierarchy mismatches: {join.HierarchyMismatches}");
        if (result.ProjectedCoverage is not null)
        {
            var p = result.ProjectedCoverage;
            Console.WriteLine(
                $"  Projected Si/Ta after mappings — DS: {p.DsSinhala}/{p.DsTamil}; GN: {p.GnSinhala}/{p.GnTamil}");
        }
        if (result.ConfirmedMappings is { Count: > 0 })
        {
            Console.WriteLine($"  Confirmed mappings applied: {result.ConfirmedMappings.Count}");
        }
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintHelp();
        return 2;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("LankaLens.DataBuilder");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project tools/LankaLens.DataBuilder -- <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  inspect       Inspect official source workbooks");
        Console.WriteLine("  validate      Parse, normalize, and validate without writing production JSON");
        Console.WriteLine("  build         Validate and write canonical JSON only when valid");
        Console.WriteLine("  acquire-moha  Download/cache official MOHA LIFe GN reports (rate-limited)");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --source-dir <path>      Directory containing sources.json and workbooks");
        Console.WriteLine("  --generated-dir <path>   Directory for generated outputs");
        Console.WriteLine("  --mappings-dir <path>    Directory for moha-to-dcs.json (default data/mappings)");
        Console.WriteLine("  --force                  Re-download MOHA reports even if cached");
        Console.WriteLine("  --delay-ms <n>           Delay between MOHA district report requests (default 3000)");
    }

    private static bool IsHelp(string value) =>
        value is "-h" or "--help" or "help" or "/?";

    private static CliOptions ParseOptions(string[] args)
    {
        string? sourceDir = null;
        string? generatedDir = null;
        string? mappingsDir = null;
        var force = false;
        var delayMs = 3000;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--source-dir" when i + 1 < args.Length:
                    sourceDir = args[++i];
                    break;
                case "--generated-dir" when i + 1 < args.Length:
                    generatedDir = args[++i];
                    break;
                case "--mappings-dir" when i + 1 < args.Length:
                    mappingsDir = args[++i];
                    break;
                case "--force":
                    force = true;
                    break;
                case "--delay-ms" when i + 1 < args.Length:
                    delayMs = int.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture);
                    break;
            }
        }

        return new CliOptions(sourceDir, generatedDir, mappingsDir, force, delayMs);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "LankaLens.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        // Fallback: tools/LankaLens.DataBuilder -> repo root
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    private sealed record CliOptions(
        string? SourceDirectory,
        string? GeneratedDirectory,
        string? MappingsDirectory,
        bool Force,
        int DelayMs);
}
