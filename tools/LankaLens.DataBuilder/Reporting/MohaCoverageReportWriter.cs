using System.Text;
using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Mappings;

namespace LankaLens.DataBuilder.Reporting;

internal static class MohaCoverageReportWriter
{
    public static void Write(
        MohaJoinReport report,
        string outputPath,
        ProjectedCoverageResult? projected = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var sb = new StringBuilder();
        sb.AppendLine("# Multilingual coverage report (DCS + MOHA LIFe)");
        sb.AppendLine();
        sb.AppendLine("DCS remains canonical for codes, hierarchy, and English. MOHA LIFe is the Sinhala/Tamil candidate.");
        sb.AppendLine();
        sb.AppendLine("## Provenance");
        sb.AppendLine();
        sb.AppendLine("- Code / English → `dcs-administrative-division-codes`");
        sb.AppendLine("- Sinhala / Tamil → `moha-life-location-codes` (when joined)");
        sb.AppendLine($"- DCS effective date: {report.Summary.DcsEffectiveDate}");
        sb.AppendLine($"- MOHA source date: {report.Summary.MohaSourceDate ?? "unknown"}");
        sb.AppendLine($"- MOHA retrieved date: {report.Summary.MohaRetrievedDate}");
        sb.AppendLine();
        sb.AppendLine("## Exact-join coverage");
        sb.AppendLine();
        AppendLevel(sb, "Province", report.ProvinceCoverage);
        AppendLevel(sb, "District", report.DistrictCoverage);
        AppendLevel(sb, "DS", report.DsCoverage);
        AppendLevel(sb, "GN", report.GnCoverage);

        if (projected is not null)
        {
            sb.AppendLine("## Projected coverage after confirmed mappings");
            sb.AppendLine();
            sb.AppendLine("In-memory only. Does not invent names. Production JSON is not written.");
            sb.AppendLine();
            sb.AppendLine($"Province Sinhala: {projected.ProvinceSinhala} / {report.ProvinceCoverage.DcsTotal}");
            sb.AppendLine($"Province Tamil: {projected.ProvinceTamil} / {report.ProvinceCoverage.DcsTotal}");
            sb.AppendLine();
            sb.AppendLine($"District Sinhala: {projected.DistrictSinhala} / {report.DistrictCoverage.DcsTotal}");
            sb.AppendLine($"District Tamil: {projected.DistrictTamil} / {report.DistrictCoverage.DcsTotal}");
            sb.AppendLine();
            sb.AppendLine($"DS Sinhala: {projected.DsSinhala} / {report.DsCoverage.DcsTotal}");
            sb.AppendLine($"DS Tamil: {projected.DsTamil} / {report.DsCoverage.DcsTotal}");
            sb.AppendLine();
            sb.AppendLine($"GN Sinhala: {projected.GnSinhala} / {report.GnCoverage.DcsTotal}");
            sb.AppendLine($"GN Tamil: {projected.GnTamil} / {report.GnCoverage.DcsTotal}");
            sb.AppendLine();
            sb.AppendLine($"Applied DS mappings: {projected.AppliedDsMappings}");
            sb.AppendLine($"Applied GN mappings: {projected.AppliedGnMappings}");
            sb.AppendLine($"Applied child propagations: {projected.AppliedChildPropagations}");
            sb.AppendLine($"Applied authoritative overlays: {projected.AppliedOverlays}");
            sb.AppendLine();
        }

        File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
    }

    private static void AppendLevel(StringBuilder sb, string heading, LevelCoverage coverage)
    {
        sb.AppendLine($"## {heading}");
        sb.AppendLine();
        sb.AppendLine($"Total DCS: {coverage.DcsTotal}");
        sb.AppendLine($"MOHA matched: {coverage.MohaMatched}");
        sb.AppendLine($"Sinhala available: {coverage.SinhalaAvailable}");
        sb.AppendLine($"Tamil available: {coverage.TamilAvailable}");
        sb.AppendLine($"Conflicts: {coverage.TranslationConflicts}");
        sb.AppendLine($"DCS without MOHA: {coverage.DcsWithoutMoha}");
        sb.AppendLine($"MOHA without DCS: {coverage.MohaWithoutDcs}");
        sb.AppendLine();
    }
}
