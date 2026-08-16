using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LankaLens.DataBuilder.Delta;
using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Mappings;
using LankaLens.DataBuilder.Models;

namespace LankaLens.DataBuilder.Reporting;

internal static class AdministrativeDeltaReportWriter
{
    public static void WriteMarkdown(
        AdministrativeDeltaReport delta,
        ProjectedCoverageResult projected,
        MohaJoinReport join,
        IReadOnlyList<AdministrativeCodeMapping> mappings,
        CanonicalDataset dcs,
        string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var sb = new StringBuilder();
        sb.AppendLine("# Administrative delta report (Phase 3.8)");
        sb.AppendLine();
        sb.AppendLine("DCS remains canonical for codes, hierarchy, and English. MOHA is the Sinhala/Tamil candidate.");
        sb.AppendLine("Confirmed mappings are explicit and evidenced; name similarity is discovery evidence only.");
        sb.AppendLine();

        sb.AppendLine("## Count gap explanation");
        sb.AppendLine();
        sb.AppendLine(delta.CountGapExplanation);
        sb.AppendLine();

        sb.AppendLine("## Exact uncovered DS set (projected Si/Ta incomplete)");
        sb.AppendLine();
        sb.AppendLine("Projected coverage requires both Sinhala and Tamil. The 340 − projected count accounts for:");
        sb.AppendLine();
        var uncoveredDs = dcs.DivisionalSecretariats
            .Where(d => !projected.CoveredDsCodes.Contains(d.Code))
            .OrderBy(d => d.Code, StringComparer.Ordinal)
            .ToList();
        sb.AppendLine("| DCS Code | DCS English | Notes |");
        sb.AppendLine("| --- | --- | --- |");
        foreach (var ds in uncoveredDs)
        {
            var note = delta.UnresolvedGaps.FirstOrDefault(g =>
                g.Type == "DivisionalSecretariat" && g.DcsCode == ds.Code);
            sb.AppendLine($"| {ds.Code} | {Esc(ds.Name.English)} | {Esc(note?.ReasonUnresolved ?? "Lacks both Si and Ta")} |");
        }

        sb.AppendLine();
        sb.AppendLine($"Uncovered DS count: {uncoveredDs.Count} (expected 340 − {projected.DsSinhala} when Si==Ta counts).");
        sb.AppendLine();

        sb.AppendLine("## DS deltas");
        sb.AppendLine();
        sb.AppendLine("| DCS Code | DCS English | MOHA Candidate Code | MOHA English | Sinhala | Tamil | Classification | Evidence | Status |");
        sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- |");
        foreach (var row in delta.DsDeltaTable)
        {
            sb.AppendLine(
                $"| {Esc(row.DcsCode)} | {Esc(row.DcsEnglish)} | {Esc(row.MohaCandidateCode)} | {Esc(row.MohaEnglish)} | {Esc(row.Sinhala)} | {Esc(row.Tamil)} | {Esc(row.Classification)} | {Esc(row.Evidence)} | {Esc(row.Status)} |");
        }

        sb.AppendLine();
        sb.AppendLine("### DS summary");
        sb.AppendLine();
        var dcsUnmatchedDs = join.UnmatchedDcs.Count(r => r.EntityType == "DivisionalSecretariat");
        var mohaUnmatchedDs = join.UnmatchedMoha.Count(r => r.EntityType == "DivisionalSecretariat");
        var confirmedDs = mappings.Count(m => m.Type == AdministrativeMappingTypes.DivisionalSecretariat);
        sb.AppendLine($"- DCS unmatched DS: {dcsUnmatchedDs}");
        sb.AppendLine($"- MOHA unmatched DS: {mohaUnmatchedDs}");
        sb.AppendLine($"- Confirmed recodes (mapping file): {confirmedDs}");
        sb.AppendLine($"- Unresolved DS rows: {delta.DsDeltaTable.Count(r => r.Status == "Unresolved")}");
        sb.AppendLine();

        sb.AppendLine("## GN unmatched — DCS grouped by Province / District / DS");
        sb.AppendLine();
        sb.AppendLine("| Province | District | DS | DS English | Count |");
        sb.AppendLine("| --- | --- | --- | --- | ---: |");
        foreach (var g in delta.DcsGroups)
        {
            sb.AppendLine($"| {g.ProvinceCode} | {g.DistrictCode} | {g.DsCode} | {Esc(g.DsEnglish)} | {g.Count} |");
        }

        sb.AppendLine();
        sb.AppendLine($"Total DCS unmatched GN: {delta.UnmatchedDcsGn.Count}");
        sb.AppendLine();

        sb.AppendLine("## GN unmatched — MOHA grouped by Province / District / DS");
        sb.AppendLine();
        sb.AppendLine("| Province | District | DS | DS English | Count |");
        sb.AppendLine("| --- | --- | --- | --- | ---: |");
        foreach (var g in delta.MohaGroups)
        {
            sb.AppendLine($"| {g.ProvinceCode} | {g.DistrictCode} | {g.DsCode} | {Esc(g.DsEnglish)} | {g.Count} |");
        }

        sb.AppendLine();
        sb.AppendLine($"Total MOHA unmatched GN: {delta.UnmatchedMohaGn.Count}");
        sb.AppendLine();

        sb.AppendLine("## Confirmed mappings");
        sb.AppendLine();
        if (mappings.Count == 0)
        {
            sb.AppendLine("None confirmed yet.");
        }
        else
        {
            sb.AppendLine("| Type | Source (MOHA) | Target (DCS) | Child propagation | Translation reuse | Evidence |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (var m in mappings)
            {
                sb.AppendLine(
                    $"| {m.Type} | {m.SourceCode} | {m.TargetCode} | {m.ChildPropagation ?? "(none)"} | {m.AllowTranslationReuse} | {Esc(m.Evidence)} |");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## GN membership for DS pairs");
        sb.AppendLine();
        if (delta.MembershipComparisons.Count == 0)
        {
            sb.AppendLine("No membership comparisons.");
        }
        else
        {
            sb.AppendLine("| DCS DS | MOHA DS | DCS GN | MOHA GN | Same components | English OK | Unchanged membership |");
            sb.AppendLine("| --- | --- | ---: | ---: | --- | --- | --- |");
            foreach (var m in delta.MembershipComparisons)
            {
                sb.AppendLine(
                    $"| {m.DcsDsCode} | {m.MohaDsCode} | {m.DcsGnCount} | {m.MohaGnCount} | {m.SameGnComponents} | {m.EnglishNamesFormattingOnlyOrExact} | {m.UnchangedMembership} |");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## DS 5225");
        sb.AppendLine();
        sb.AppendLine($"- DCS `5225` English: {delta.Ds5225.Dcs5225English}");
        sb.AppendLine($"- DCS `5221` English: {delta.Ds5225.Dcs5221English}");
        sb.AppendLine($"- MOHA English variants: {string.Join(" | ", delta.Ds5225.MohaEnglishVariants)}");
        sb.AppendLine($"- MOHA Sinhala variants: {string.Join(" | ", delta.Ds5225.MohaSinhalaVariants)}");
        sb.AppendLine($"- MOHA Tamil variants: {string.Join(" | ", delta.Ds5225.MohaTamilVariants)}");
        sb.AppendLine($"- Diagnosis: {delta.Ds5225.Diagnosis}");
        sb.AppendLine($"- Resolved: {delta.Ds5225.Resolved}");
        sb.AppendLine();

        sb.AppendLine("## Projected translation coverage after confirmed mappings");
        sb.AppendLine();
        sb.AppendLine($"| Level | Exact-join Si/Ta | Projected Si/Ta | DCS total |");
        sb.AppendLine($"| --- | --- | --- | ---: |");
        sb.AppendLine($"| Province | {join.ProvinceCoverage.SinhalaAvailable} / {join.ProvinceCoverage.TamilAvailable} | {projected.ProvinceSinhala} / {projected.ProvinceTamil} | {dcs.Provinces.Count} |");
        sb.AppendLine($"| District | {join.DistrictCoverage.SinhalaAvailable} / {join.DistrictCoverage.TamilAvailable} | {projected.DistrictSinhala} / {projected.DistrictTamil} | {dcs.Districts.Count} |");
        sb.AppendLine($"| DS | {join.DsCoverage.SinhalaAvailable} / {join.DsCoverage.TamilAvailable} | {projected.DsSinhala} / {projected.DsTamil} | {dcs.DivisionalSecretariats.Count} |");
        sb.AppendLine($"| GN | {join.GnCoverage.SinhalaAvailable} / {join.GnCoverage.TamilAvailable} | {projected.GnSinhala} / {projected.GnTamil} | {dcs.GramaNiladhariDivisions.Count} |");
        sb.AppendLine();
        sb.AppendLine($"Applied DS mappings: {projected.AppliedDsMappings}; GN mappings: {projected.AppliedGnMappings}; child propagations: {projected.AppliedChildPropagations}; overlays: {projected.AppliedOverlays}");
        sb.AppendLine();

        sb.AppendLine("## Unresolved gaps");
        sb.AppendLine();
        sb.AppendLine($"Count: {delta.UnresolvedGaps.Count} (see `unresolved-multilingual-gaps.json`)");
        sb.AppendLine();

        sb.AppendLine("## Evidence sources investigated");
        sb.AppendLine();
        sb.AppendLine("- DCS Administrative Division Codes workbook (2024-03-19) — structural authority");
        sb.AppendLine("- MOHA LIFe national GN reports (retrieved 2026-08-16) — Sinhala/Tamil candidate");
        sb.AppendLine("- documents.gov.lk — targeted Gazette search for identified DS recode clusters");
        sb.AppendLine("- Cabinet Office 2019-05-07 — establishment of Kotmale West, Norwood, Mathurata, Nildandahinna, Talawakale");
        sb.AppendLine("- PubAd Ampara DS list — Kalmunai North and Saindamarudu as separate DS");
        sb.AppendLine("- Relevant District / Divisional Secretariat sites for named divisions");
        sb.AppendLine();

        File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
    }

    public static void WriteUnresolvedGapsJson(AdministrativeDeltaReport delta, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var dto = new
        {
            generatedFor = "Phase 3.8",
            unresolvedCount = delta.UnresolvedGaps.Count,
            gaps = delta.UnresolvedGaps.Select(g => new
            {
                type = g.Type,
                dcsCode = g.DcsCode,
                englishName = g.EnglishName,
                province = g.Province,
                district = g.District,
                ds = g.Ds,
                reasonUnresolved = g.ReasonUnresolved,
                candidateMohaRecords = g.CandidateMohaRecords,
                evidenceInvestigated = g.EvidenceInvestigated,
                sourcesInvestigated = g.SourcesInvestigated,
                reasonResolutionWasRejected = g.ReasonResolutionWasRejected
            })
        };

        File.WriteAllText(outputPath, JsonSerializer.Serialize(dto, options) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static string Esc(string? value) =>
        string.IsNullOrEmpty(value) ? "" : value.Replace("|", "\\|", StringComparison.Ordinal);
}
