using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Validation;

namespace LankaLens.DataBuilder.Reporting;

internal static class ValidationReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void WriteMarkdown(ValidationReport report, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var sb = new StringBuilder();
        sb.AppendLine("# LankaLens DataBuilder Validation Report");
        sb.AppendLine();
        sb.AppendLine("## Dataset Sources");
        sb.AppendLine();
        if (report.DatasetSources.Count == 0)
        {
            sb.AppendLine("- (none recorded)");
        }
        else
        {
            foreach (var source in report.DatasetSources)
            {
                sb.AppendLine($"- {source}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Counts");
        sb.AppendLine();
        sb.AppendLine($"- Provinces: {report.ProvinceCount}");
        sb.AppendLine($"- Districts: {report.DistrictCount}");
        sb.AppendLine($"- Divisional Secretariats: {report.DivisionalSecretariatCount}");
        sb.AppendLine($"- Grama Niladhari Divisions: {report.GramaNiladhariDivisionCount}");
        sb.AppendLine();
        sb.AppendLine("## Translation Completeness");
        sb.AppendLine();
        sb.AppendLine($"- Missing English names: {report.MissingEnglish}");
        sb.AppendLine($"  - Province: {report.MissingEnglishProvinces}");
        sb.AppendLine($"  - District: {report.MissingEnglishDistricts}");
        sb.AppendLine($"  - DS: {report.MissingEnglishDivisionalSecretariats}");
        sb.AppendLine($"  - GN: {report.MissingEnglishGramaNiladhariDivisions}");
        sb.AppendLine($"- Missing Sinhala names: {report.MissingSinhala}");
        sb.AppendLine($"  - Province: {report.MissingSinhalaProvinces}");
        sb.AppendLine($"  - District: {report.MissingSinhalaDistricts}");
        sb.AppendLine($"  - DS: {report.MissingSinhalaDivisionalSecretariats}");
        sb.AppendLine($"  - GN: {report.MissingSinhalaGramaNiladhariDivisions}");
        sb.AppendLine($"- Missing Tamil names: {report.MissingTamil}");
        sb.AppendLine($"  - Province: {report.MissingTamilProvinces}");
        sb.AppendLine($"  - District: {report.MissingTamilDistricts}");
        sb.AppendLine($"  - DS: {report.MissingTamilDivisionalSecretariats}");
        sb.AppendLine($"  - GN: {report.MissingTamilGramaNiladhariDivisions}");
        sb.AppendLine();
        sb.AppendLine("## Issues Summary");
        sb.AppendLine();
        sb.AppendLine($"- Duplicate codes: {report.Issues.Count(i => i.Code == "DUPLICATE_CODE")}");
        sb.AppendLine($"- Duplicate names: {report.Issues.Count(i => i.Code == "DUPLICATE_NAME")}");
        sb.AppendLine($"- Orphans: {report.Issues.Count(i => i.Code.StartsWith("ORPHAN_", StringComparison.Ordinal))}");
        sb.AppendLine($"- Source conflicts: {report.Issues.Count(i => i.Code is "PARENT_INCONSISTENCY" or "COUNT_MISMATCH" or "UID_MISMATCH")}");
        sb.AppendLine($"- Warnings: {report.WarningCount}");
        sb.AppendLine($"- Errors: {report.ErrorCount}");
        sb.AppendLine();
        sb.AppendLine($"## Overall status: {(report.Passed ? "PASS" : "FAIL")}");
        sb.AppendLine();

        if (report.Issues.Count > 0)
        {
            sb.AppendLine("## Issues");
            sb.AppendLine();
            // Cap markdown detail for huge missing-translation sets.
            const int maxListed = 200;
            var listed = 0;
            foreach (var issue in report.Issues
                .OrderBy(i => i.Severity)
                .ThenBy(i => i.Code, StringComparer.Ordinal)
                .ThenBy(i => i.EntityCode, StringComparer.Ordinal))
            {
                if (listed >= maxListed)
                {
                    sb.AppendLine($"- ... and {report.Issues.Count - maxListed} more (see validation-report.json).");
                    break;
                }

                sb.AppendLine(
                    $"- **{issue.Severity}** `{issue.Code}` {issue.EntityType}/{issue.EntityCode}: {issue.Message}");
                listed++;
            }
        }

        File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
    }

    public static void WriteJson(ValidationReport report, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var dto = new
        {
            status = report.Passed ? "PASS" : "FAIL",
            datasetSources = report.DatasetSources,
            counts = new
            {
                provinces = report.ProvinceCount,
                districts = report.DistrictCount,
                divisionalSecretariats = report.DivisionalSecretariatCount,
                gramaNiladhariDivisions = report.GramaNiladhariDivisionCount
            },
            missingTranslations = new
            {
                english = new
                {
                    total = report.MissingEnglish,
                    provinces = report.MissingEnglishProvinces,
                    districts = report.MissingEnglishDistricts,
                    divisionalSecretariats = report.MissingEnglishDivisionalSecretariats,
                    gramaNiladhariDivisions = report.MissingEnglishGramaNiladhariDivisions
                },
                sinhala = new
                {
                    total = report.MissingSinhala,
                    provinces = report.MissingSinhalaProvinces,
                    districts = report.MissingSinhalaDistricts,
                    divisionalSecretariats = report.MissingSinhalaDivisionalSecretariats,
                    gramaNiladhariDivisions = report.MissingSinhalaGramaNiladhariDivisions
                },
                tamil = new
                {
                    total = report.MissingTamil,
                    provinces = report.MissingTamilProvinces,
                    districts = report.MissingTamilDistricts,
                    divisionalSecretariats = report.MissingTamilDivisionalSecretariats,
                    gramaNiladhariDivisions = report.MissingTamilGramaNiladhariDivisions
                }
            },
            warningCount = report.WarningCount,
            errorCount = report.ErrorCount,
            issues = report.Issues.Select(i => new
            {
                severity = i.Severity.ToString(),
                code = i.Code,
                message = i.Message,
                entityType = i.EntityType,
                entityCode = i.EntityCode
            }).ToList()
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions) + Environment.NewLine;
        File.WriteAllText(outputPath, json, new UTF8Encoding(false));
    }

    public static void WriteConflicts(IReadOnlyList<SourceConflict> conflicts, string outputPath)
    {
        if (conflicts.Count == 0)
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var json = JsonSerializer.Serialize(conflicts, JsonOptions) + Environment.NewLine;
        File.WriteAllText(outputPath, json, new UTF8Encoding(false));
    }
}
