using System.Text;
using LankaLens.DataBuilder.Delta;
using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Mappings;
using LankaLens.DataBuilder.Models;

namespace LankaLens.DataBuilder.Reporting;

/// <summary>
/// Phase 3.8 final gap-resolution narrative organized by administrative area.
/// </summary>
internal static class FinalGapResolutionReportWriter
{
    public static void Write(
        AdministrativeDeltaReport delta,
        ProjectedCoverageResult projected,
        MohaJoinReport join,
        IReadOnlyList<AdministrativeCodeMapping> mappings,
        IReadOnlyList<AuthoritativeNameOverlay> overlays,
        CanonicalDataset dcs,
        string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var sb = new StringBuilder();
        sb.AppendLine("# Final gap resolution report (Phase 3.8)");
        sb.AppendLine();
        sb.AppendLine("Authoritative multilingual gap investigation. DCS English/codes remain canonical.");
        sb.AppendLine("No AI translation. No production JSON. Runtime library unchanged.");
        sb.AppendLine();

        sb.AppendLine("## Coverage summary");
        sb.AppendLine();
        sb.AppendLine("| Level | Before 3.8 (projected) | After 3.8 (projected) | Total |");
        sb.AppendLine("| --- | --- | --- | ---: |");
        sb.AppendLine($"| Province | 9 / 9 | {projected.ProvinceSinhala} / {projected.ProvinceTamil} | {dcs.Provinces.Count} |");
        sb.AppendLine($"| District | 25 / 25 | {projected.DistrictSinhala} / {projected.DistrictTamil} | {dcs.Districts.Count} |");
        sb.AppendLine($"| DS | 336 / 336 | {projected.DsSinhala} / {projected.DsTamil} | {dcs.DivisionalSecretariats.Count} |");
        sb.AppendLine($"| GN | 13722 / 13722 | {projected.GnSinhala} / {projected.GnTamil} | {dcs.GramaNiladhariDivisions.Count} |");
        sb.AppendLine();
        sb.AppendLine($"Applied DS mappings: {projected.AppliedDsMappings}; GN mappings: {projected.AppliedGnMappings}; child propagations: {projected.AppliedChildPropagations}; overlays: {projected.AppliedOverlays}");
        sb.AppendLine();

        sb.AppendLine("## Exact uncovered DS before Phase 3.8 mappings/overlays");
        sb.AppendLine();
        sb.AppendLine("The 336/340 projected DS figure left exactly four current DCS DS without both Si and Ta:");
        sb.AppendLine();
        sb.AppendLine("| DCS | English | Cause |");
        sb.AppendLine("| --- | --- | --- |");
        sb.AppendLine("| 2302 | Kothmale West | Unmatched DS code vs MOHA `2304` |");
        sb.AppendLine("| 2314 | Norwood | Unmatched DS code vs MOHA `2316` |");
        sb.AppendLine("| 5221 | Kalmunai North Sub | No MOHA `5221`; Kalmunai North rows parked on LIFe `5225` |");
        sb.AppendLine("| 3136 | Hikkaduwa | Same DS code, but zero GN exact-joins after 2019 renumbering |");
        sb.AppendLine();
        sb.AppendLine("Note: DS `5225` Sainthamaruthu is code-matched and already receives Si/Ta from matched-row English narrowing; the raw MOHA `TRANSLATION_CONFLICT` remains. Phase 3.8 still records an explicit filtered overlay for provenance clarity.");
        sb.AppendLine();

        WriteArea(
            sb,
            "Ratmalana (MOHA `1139` / DCS `1131`)",
            "MOHA retains Mount Lavinia under LIFe DS segment `39` (`1-1-39-005`) while DCS places Mount Lavinia at `1131005` under Ratmalana `1131`. Other Ratmalana GNs already exact-join under `1131`. MOHA DS label on the Mount Lavinia row is `31: …/ Ratmalana` (label prefix matches DCS `1131`, life segment lags).",
            "DCS GNDList places Mount Lavinia under Ratmalana. MOHA English is Mount Lavinia with Sinhala `ගල්කිස්ස` and Tamil `மனிட்லாவனியா`. MOHA DS label names Ratmalana with numeric prefix 31. GIC lists Ratmalana DS. Dual-authority identity for the GN; LIFe DS segment `39` is source lag, not a second DS.",
            "CONFIRMED GN mapping `1139005` → `1131005` (parent transfer / LIFe segment lag). No DS mapping `1139`→`1131` (target `1131` already exists in both sources).",
            "1139005 → 1131005",
            "MOHA LIFe row for `1-1-39-005`",
            1,
            RemainingUnderDs(delta, "1131"));

        WriteArea(
            sb,
            "Kothmale West (`2302`) / MOHA `2304`",
            "Same district, compatible English (`Kothmale West` / `Kothmale (West)`), equal GN count 49/49, but GN-component sets differ (not `GnComponentUnchanged`).",
            "Cabinet decision 2019-05-07 established Kotmale West as a new DS in Nuwara Eliya. DCS lists Kothmale West `2302`. MOHA lists Kothmale (West) `2304` with Si/Ta. English GN membership overlaps strongly (38 exact + 11 spelling variants) but components are renumbered — discovery for GNs only.",
            "CONFIRMED DS mapping `2304` → `2302` without child propagation (DS-level Si/Ta reuse only). GN children remain UNRESOLVED pending Gazette/DS GN code lists.",
            "2304 → 2302 (no child propagation)",
            "MOHA DS label on `2304` rows; Cabinet Office 2019-05-07",
            0,
            RemainingUnderDs(delta, "2302"));

        WriteArea(
            sb,
            "Norwood (`2314`) / MOHA `2316`",
            "Same district, exact English Norwood, equal GN count 35/35, different GN components.",
            "Cabinet 2019-05-07 established Norwood DS. DCS `2314` / MOHA `2316`. English GN sets show substantial spelling drift; components differ — not a bijection.",
            "CONFIRMED DS mapping `2316` → `2314` without child propagation. GN children UNRESOLVED.",
            "2316 → 2314 (no child propagation)",
            "MOHA DS label on `2316` rows; Cabinet Office 2019-05-07",
            0,
            RemainingUnderDs(delta, "2314"));

        WriteArea(
            sb,
            "Kalmunai North Sub (`5221`)",
            "DCS lists Kalmunai North Sub with 29 GNs; MOHA has no HierarchicalDsCode `5221`. Kalmunai North Sub Office labels appear on LIFe `5-2-25` mixed with Sainthamaruthu.",
            "PubAd Ampara list names Kalmunai North and Saindamarudu as separate DS. MOHA Kalmunai North Sub Office rows provide Si/Ta for the named office. GN English sets resemble DCS `5221` but LIFe codes (`5225150+`) do not match DCS `5221*` — English similarity alone is insufficient for GN mappings.",
            "CONFIRMED authoritative name overlay for DS `5221` Si/Ta from MOHA Kalmunai North Sub Office labels. GN records UNRESOLVED.",
            "(overlay) 5221",
            "MOHA filtered DS labels; PubAd Ampara DS list",
            0,
            RemainingUnderDs(delta, "5221"));

        WriteArea(
            sb,
            "DS `5225` Sainthamaruthu",
            "MOHA LIFe `5-2-25` mixes Sainthamarathu and Kalmunai North Sub Office labels → raw TRANSLATION_CONFLICT. DCS English is Sainthamaruthu; 17 Sainthamaruthu GNs exact-join by code.",
            "PubAd lists Saindamarudu separately from Kalmunai North. MOHA Sainthamaruthu-labelled rows agree on Si `සෙයින්තමරතු` and Ta `சாய்ந்தமருது`. Resolution uses filtered overlay — not majority vote, not first-row, not English similarity alone.",
            delta.Ds5225.Resolved
                ? "RESOLVED via authoritative name overlay for DCS `5225` (Sainthamaruthu-labelled MOHA rows only). Raw MOHA conflict retained in join reports."
                : "UNRESOLVED — contradictory or insufficient filtered evidence.",
            delta.Ds5225.Resolved ? "(overlay) 5225" : "(none)",
            "MOHA Sainthamaruthu-labelled rows; PubAd Ampara DS list",
            0,
            RemainingUnderDs(delta, "5225"));

        WriteArea(
            sb,
            "Hikkaduwa (`3136`)",
            "DCS and MOHA share DS code 3136 (Hikkaduwa), but all 27 GN codes fail exact LIFe join after the 2019 split/renumbering, so matched-row DS aggregation has no Si/Ta.",
            "Cabinet 2019-05-07 upgraded Hikkaduwa into Hikkaduwa/Ratgama/Madampagama. MOHA LIFe still emits a single agreed DS label on all 3-1-36-* rows (හික්කඩුව / ஹிக்கடுவை / Hikkaduwa).",
            "CONFIRMED authoritative name overlay for DS `3136`. GN children UNRESOLVED (English/component discovery only).",
            "(overlay) 3136",
            "MOHA DS labels on HierarchicalDsCode 3136 rows",
            0,
            RemainingUnderDs(delta, "3136"));

        sb.AppendLine("## Residual GN clusters");
        sb.AppendLine();
        sb.AppendLine("Outside confirmed DS recodes, residual unmatched GNs were classified. Same-DS English matches with different GN components (Mathurata, Nildandahinna, Thalawakelle, Hikkaduwa, Baddegama, Balangoda, etc.) remain **UNRESOLVED**: English similarity is discovery only; Cabinet 2019 establishes named DS splits but does not publish GN code renumbering tables used here.");
        sb.AppendLine();
        sb.AppendLine("| Classification | Treatment |");
        sb.AppendLine("| --- | --- |");
        sb.AppendLine("| GN code change (same DS, different component, English match) | Discovery only — UNRESOLVED without Gazette GN list |");
        sb.AppendLine("| GN parent transfer | Only Mount Lavinia confirmed; others lack dual-authority code evidence |");
        sb.AppendLine("| DS split residue (Hikkaduwa / Baddegama / Balangoda) | UNRESOLVED pending Gazette GN assignment lists |");
        sb.AppendLine("| Obsolete MOHA GN (Hanguranketha `2306`, Laggala-Pallegama `2224`, Weligama `3239`) | Documented as MOHA-only; not DCS targets |");
        sb.AppendLine();

        sb.AppendLine("## Confirmed mappings (Phase 3.7 frozen + Phase 3.8 additions)");
        sb.AppendLine();
        sb.AppendLine("| Type | Source | Target | Child propagation |");
        sb.AppendLine("| --- | --- | --- | --- |");
        foreach (var m in mappings)
        {
            sb.AppendLine($"| {m.Type} | {m.SourceCode} | {m.TargetCode} | {m.ChildPropagation ?? "(none)"} |");
        }

        sb.AppendLine();
        sb.AppendLine("## Authoritative name overlays");
        sb.AppendLine();
        if (overlays.Count == 0)
        {
            sb.AppendLine("None.");
        }
        else
        {
            sb.AppendLine("| Type | DCS | Source organization | URL |");
            sb.AppendLine("| --- | --- | --- | --- |");
            foreach (var o in overlays)
            {
                sb.AppendLine($"| {o.Type} | {o.DcsCode} | {Esc(o.SourceOrganization)} | {o.EvidenceUrl} |");
            }
        }

        sb.AppendLine();
        sb.AppendLine("## Gazette / government page evidence cards");
        sb.AppendLine();
        sb.AppendLine("| Organization | URL | Entity | What established | Date |");
        sb.AppendLine("| --- | --- | --- | --- | --- |");
        sb.AppendLine("| Cabinet Office | https://www.cabinetoffice.gov.lk/cab/index.php?Itemid=49&dID=9758&id=16&lang=en&option=com_content&view=article | Kotmale West, Norwood, Mathurata, Nildandahinna, Talawakale | Named new DS approved 2019-05-07 (not a LIFe code map) | 2019-05-07 |");
        sb.AppendLine("| Ministry of Public Administration | https://pubad.gov.lk/web/index.php?Itemid=116&id=106&lang=en&option=com_content&view=article | Kalmunai North, Saindamarudu | Separate Ampara DS entities | retrieved 2026-08-16 |");
        sb.AppendLine("| MOHA LIFe | http://moha.gov.lk:8090/lifecode/views/rpt_gn_list.php | Ratmalana / Sainthamaruthu / Kalmunai North Sub Office / Kothmale (West) / Norwood | Official Si/Ta labels on GN report rows | retrieved 2026-08-16 |");
        sb.AppendLine("| DCS | https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel | All current codes/English | Canonical hierarchy | 2024-03-19 |");
        sb.AppendLine("| GIC | https://gic.gov.lk/gic/index.php/en/component/org/?id=513&task=org | Ratmalana DS | Official Ratmalana DS contact listing | retrieved 2026-08-16 |");
        sb.AppendLine();

        sb.AppendLine("## Remaining unresolved (grouped by DS)");
        sb.AppendLine();
        var byDs = delta.UnresolvedGaps
            .GroupBy(g => g.Ds ?? g.DcsCode)
            .OrderBy(g => g.Key, StringComparer.Ordinal);
        foreach (var group in byDs)
        {
            var dsGap = group.FirstOrDefault(g => g.Type == "DivisionalSecretariat");
            var gnCount = group.Count(g => g.Type == "GramaNiladhariDivision");
            sb.AppendLine($"- `{group.Key}` {dsGap?.EnglishName ?? group.First().EnglishName}: DS unresolved={(dsGap is not null)}; GN unresolved={gnCount}");
        }

        sb.AppendLine();
        sb.AppendLine($"Total unresolved records: {delta.UnresolvedGaps.Count}");
        sb.AppendLine();
        sb.AppendLine("## Exact-join reminder");
        sb.AppendLine();
        sb.AppendLine($"Exact-join DS Si/Ta: {join.DsCoverage.SinhalaAvailable}/{join.DsCoverage.TamilAvailable}; GN: {join.GnCoverage.SinhalaAvailable}/{join.GnCoverage.TamilAvailable}.");
        sb.AppendLine("Projection applies mappings and overlays in memory only.");
        sb.AppendLine();

        File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));
    }

    private static void WriteArea(
        StringBuilder sb,
        string title,
        string problem,
        string evidence,
        string decision,
        string mapping,
        string siTaSource,
        int gnsResolved,
        int remaining)
    {
        sb.AppendLine($"## {title}");
        sb.AppendLine();
        sb.AppendLine($"- **Problem:** {problem}");
        sb.AppendLine($"- **Evidence:** {evidence}");
        sb.AppendLine($"- **Decision:** {decision}");
        sb.AppendLine($"- **Mapping:** {mapping}");
        sb.AppendLine($"- **Sinhala/Tamil source:** {siTaSource}");
        sb.AppendLine($"- **GNs resolved:** {gnsResolved}");
        sb.AppendLine($"- **Remaining unresolved under this DS:** {remaining}");
        sb.AppendLine();
    }

    private static int RemainingUnderDs(AdministrativeDeltaReport delta, string dsCode) =>
        delta.UnresolvedGaps.Count(g =>
            string.Equals(g.Ds, dsCode, StringComparison.Ordinal)
            || (g.Type == "DivisionalSecretariat" && string.Equals(g.DcsCode, dsCode, StringComparison.Ordinal)));

    private static string Esc(string? value) =>
        string.IsNullOrEmpty(value) ? "" : value.Replace("|", "\\|", StringComparison.Ordinal);
}
