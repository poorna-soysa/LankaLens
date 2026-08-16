using System.Text;
using LankaLens.DataBuilder.Joining;

namespace LankaLens.DataBuilder.Reporting;

internal static class EnglishDifferenceReportWriter
{
    public static void Write(MohaJoinReport report, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var sb = new StringBuilder();
        sb.AppendLine("# English name differences (DCS canonical vs MOHA LIFe)");
        sb.AppendLine();
        sb.AppendLine("Review artifact only. MOHA English must not overwrite DCS English.");
        sb.AppendLine();
        sb.AppendLine($"- Exact: {report.Summary.EnglishExact}");
        sb.AppendLine($"- Formatting-only (case / whitespace / punctuation): {report.Summary.EnglishFormattingOnly}");
        sb.AppendLine($"- Spelling or substantive: {report.Summary.EnglishSpellingOrSubstantive}");
        sb.AppendLine();

        AppendGroup(sb, "Province", report.EnglishDifferences.Where(d => d.EntityType == "Province"));
        AppendGroup(sb, "District", report.EnglishDifferences.Where(d => d.EntityType == "District"));
        AppendGroup(sb, "DS", report.EnglishDifferences.Where(d => d.EntityType == "DivisionalSecretariat"));
        AppendGroup(sb, "GN", report.EnglishDifferences.Where(d => d.EntityType == "GramaNiladhariDivision"));

        File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
    }

    private static void AppendGroup(StringBuilder sb, string heading, IEnumerable<EnglishNameDifference> rows)
    {
        var list = rows
            .OrderBy(r => r.Kind.ToString(), StringComparer.Ordinal)
            .ThenBy(r => r.Code, StringComparer.Ordinal)
            .ToList();

        sb.AppendLine($"## {heading}");
        sb.AppendLine();
        if (list.Count == 0)
        {
            sb.AppendLine("No differences.");
            sb.AppendLine();
            return;
        }

        foreach (var row in list)
        {
            sb.AppendLine($"- Code: `{row.Code}`");
            sb.AppendLine($"  - DCS English: {row.DcsEnglish ?? "(missing)"}");
            sb.AppendLine($"  - MOHA English: {row.MohaEnglish ?? "(missing)"}");
            sb.AppendLine($"  - Difference classification: {row.Kind}");
        }

        sb.AppendLine();
    }
}
