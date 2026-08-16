using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Mappings;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Parsing;

namespace LankaLens.DataBuilder.Delta;

internal static class DeltaClassifications
{
    public const string ConfirmedRecode = "Confirmed recode";
    public const string ConfirmedRename = "Confirmed rename";
    public const string ConfirmedTransfer = "Confirmed transfer";
    public const string ConfirmedNewEntity = "Confirmed new entity";
    public const string ConfirmedRemovedReplaced = "Confirmed removed/replaced entity";
    public const string PotentialMatchRequiringReview = "Potential match requiring review";
    public const string NoCorrespondingMohaEntity = "No corresponding MOHA entity";
}

internal sealed record EnrichedUnmatchedDcsGn(
    string GndUid,
    string ProvinceCode,
    string DistrictCode,
    string DsCode,
    string GnComponent,
    string? EnglishProvince,
    string? EnglishDistrict,
    string? EnglishDs,
    string? EnglishGn);

internal sealed record EnrichedUnmatchedMohaGn(
    string LifeCode,
    string NormalizedLifeCode,
    string ProvinceComponent,
    string DistrictComponent,
    string DsComponent,
    string GnComponent,
    string HierarchicalProvinceCode,
    string HierarchicalDistrictCode,
    string HierarchicalDsCode,
    string? English,
    string? Sinhala,
    string? Tamil,
    string? DsEnglish,
    string? DistrictEnglish,
    string? ProvinceEnglish);

internal sealed record DeltaGroupCount(
    string ProvinceCode,
    string? ProvinceEnglish,
    string DistrictCode,
    string? DistrictEnglish,
    string DsCode,
    string? DsEnglish,
    int Count);

internal sealed record DsDeltaRow(
    string DcsCode,
    string? DcsEnglish,
    string? MohaCandidateCode,
    string? MohaEnglish,
    string? Sinhala,
    string? Tamil,
    string Classification,
    string Evidence,
    string Status);

internal sealed record DeltaCandidate(
    string DcsCode,
    string EntityType,
    string? DcsEnglish,
    string? MohaCode,
    string? MohaEnglish,
    string? MohaLifeCode,
    string DiscoveryBasis,
    string Classification);

internal sealed record GnMembershipComparison(
    string DcsDsCode,
    string MohaDsCode,
    int DcsGnCount,
    int MohaGnCount,
    bool SameGnComponents,
    bool EnglishNamesFormattingOnlyOrExact,
    bool UnchangedMembership,
    IReadOnlyList<string> OnlyDcsComponents,
    IReadOnlyList<string> OnlyMohaComponents);

internal sealed record Ds5225Findings(
    string Dcs5225English,
    string Dcs5221English,
    IReadOnlyList<string> MohaEnglishVariants,
    IReadOnlyList<string> MohaSinhalaVariants,
    IReadOnlyList<string> MohaTamilVariants,
    string Diagnosis,
    bool Resolved);

internal sealed record UnresolvedGapRecord(
    string Type,
    string DcsCode,
    string? EnglishName,
    string? Province,
    string? District,
    string? Ds,
    string ReasonUnresolved,
    IReadOnlyList<object> CandidateMohaRecords,
    string EvidenceInvestigated,
    IReadOnlyList<string> SourcesInvestigated,
    string ReasonResolutionWasRejected);

internal sealed record AdministrativeDeltaReport(
    IReadOnlyList<EnrichedUnmatchedDcsGn> UnmatchedDcsGn,
    IReadOnlyList<EnrichedUnmatchedMohaGn> UnmatchedMohaGn,
    IReadOnlyList<DeltaGroupCount> DcsGroups,
    IReadOnlyList<DeltaGroupCount> MohaGroups,
    IReadOnlyList<DsDeltaRow> DsDeltaTable,
    IReadOnlyList<DeltaCandidate> Candidates,
    IReadOnlyList<GnMembershipComparison> MembershipComparisons,
    Ds5225Findings Ds5225,
    IReadOnlyList<UnresolvedGapRecord> UnresolvedGaps,
    string CountGapExplanation);

internal static class DeltaAnalyzer
{
    public static AdministrativeDeltaReport Analyze(
        CanonicalDataset dcs,
        MohaParseResult moha,
        MohaJoinReport join,
        IReadOnlyList<AdministrativeCodeMapping> confirmedMappings,
        ProjectedCoverageResult? projected = null,
        IReadOnlyList<AuthoritativeNameOverlay>? overlays = null)
    {
        overlays ??= [];
        var dcsDsByCode = dcs.DivisionalSecretariats.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var dcsDistrictByCode = dcs.Districts.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var dcsProvinceByCode = dcs.Provinces.ToDictionary(p => p.Code, StringComparer.Ordinal);
        var dcsGnByCode = dcs.GramaNiladhariDivisions.ToDictionary(g => g.Code, StringComparer.Ordinal);

        var mohaByNormalized = moha.Records
            .GroupBy(r => r.NormalizedLifeCode, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var unmatchedDcsGn = new List<EnrichedUnmatchedDcsGn>();
        foreach (var gn in dcs.GramaNiladhariDivisions)
        {
            if (mohaByNormalized.ContainsKey(gn.Code))
            {
                continue;
            }

            if (!dcsDsByCode.TryGetValue(gn.DivisionalSecretariatCode, out var ds)
                || !dcsDistrictByCode.TryGetValue(ds.DistrictCode, out var district)
                || !dcsProvinceByCode.TryGetValue(district.ProvinceCode, out var province))
            {
                continue;
            }

            unmatchedDcsGn.Add(new EnrichedUnmatchedDcsGn(
                gn.Code,
                province.Code,
                district.Code,
                ds.Code,
                gn.Code.Length >= 3 ? gn.Code[^3..] : gn.Code,
                province.Name.English,
                district.Name.English,
                ds.Name.English,
                gn.Name.English));
        }

        var unmatchedMohaGn = moha.Records
            .Where(r => !dcsGnByCode.ContainsKey(r.NormalizedLifeCode))
            .GroupBy(r => r.NormalizedLifeCode, StringComparer.Ordinal)
            .Select(g =>
            {
                var row = g.First();
                return new EnrichedUnmatchedMohaGn(
                    row.LifeCode,
                    row.NormalizedLifeCode,
                    row.ProvinceComponent,
                    row.DistrictComponent,
                    row.DsComponent,
                    row.GnComponent,
                    row.HierarchicalProvinceCode,
                    row.HierarchicalDistrictCode,
                    row.HierarchicalDsCode,
                    row.EnglishName,
                    row.SinhalaName,
                    row.TamilName,
                    row.DsEnglish,
                    row.DistrictEnglish,
                    row.ProvinceEnglish);
            })
            .OrderBy(r => r.NormalizedLifeCode, StringComparer.Ordinal)
            .ToList();

        var dcsGroups = unmatchedDcsGn
            .GroupBy(r => (r.ProvinceCode, r.DistrictCode, r.DsCode))
            .Select(g =>
            {
                var first = g.First();
                return new DeltaGroupCount(
                    first.ProvinceCode,
                    first.EnglishProvince,
                    first.DistrictCode,
                    first.EnglishDistrict,
                    first.DsCode,
                    first.EnglishDs,
                    g.Count());
            })
            .OrderBy(g => g.ProvinceCode, StringComparer.Ordinal)
            .ThenBy(g => g.DistrictCode, StringComparer.Ordinal)
            .ThenBy(g => g.DsCode, StringComparer.Ordinal)
            .ToList();

        var mohaGroups = unmatchedMohaGn
            .GroupBy(r => (r.HierarchicalProvinceCode, r.HierarchicalDistrictCode, r.HierarchicalDsCode))
            .Select(g =>
            {
                var first = g.First();
                return new DeltaGroupCount(
                    first.HierarchicalProvinceCode,
                    first.ProvinceEnglish,
                    first.HierarchicalDistrictCode,
                    first.DistrictEnglish,
                    first.HierarchicalDsCode,
                    first.DsEnglish,
                    g.Count());
            })
            .OrderBy(g => g.ProvinceCode, StringComparer.Ordinal)
            .ThenBy(g => g.DistrictCode, StringComparer.Ordinal)
            .ThenBy(g => g.DsCode, StringComparer.Ordinal)
            .ToList();

        var confirmedByTarget = confirmedMappings
            .Where(m => string.Equals(m.Type, AdministrativeMappingTypes.DivisionalSecretariat, StringComparison.Ordinal))
            .ToDictionary(m => m.TargetCode, StringComparer.Ordinal);
        var confirmedBySource = confirmedMappings
            .Where(m => string.Equals(m.Type, AdministrativeMappingTypes.DivisionalSecretariat, StringComparison.Ordinal))
            .ToDictionary(m => m.SourceCode, StringComparer.Ordinal);
        var confirmedGnTargets = confirmedMappings
            .Where(m => string.Equals(m.Type, AdministrativeMappingTypes.GramaNiladhariDivision, StringComparison.Ordinal))
            .Select(m => m.TargetCode)
            .ToHashSet(StringComparer.Ordinal);

        var unmatchedDcsDs = join.UnmatchedDcs
            .Where(r => r.EntityType == "DivisionalSecretariat")
            .ToList();
        var unmatchedMohaDs = join.UnmatchedMoha
            .Where(r => r.EntityType == "DivisionalSecretariat")
            .ToList();

        var mohaDsCandidates = moha.Records
            .GroupBy(r => r.HierarchicalDsCode, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => TranslationConsistencyChecker.Aggregate(
                    g.Key,
                    g.Select(r => (r.DsEnglish, r.DsSinhala, r.DsTamil))),
                StringComparer.Ordinal);

        var dsDeltaTable = new List<DsDeltaRow>();
        var candidates = new List<DeltaCandidate>();
        var memberships = new List<GnMembershipComparison>();

        foreach (var dcsDs in unmatchedDcsDs)
        {
            if (!dcsDsByCode.TryGetValue(dcsDs.Code, out var dsEntity)
                || !dcsDistrictByCode.TryGetValue(dsEntity.DistrictCode, out var district))
            {
                continue;
            }

            var discovery = DiscoverDsCandidate(
                dcsDs.Code,
                dcsDs.English,
                district.Code,
                unmatchedMohaDs,
                mohaDsCandidates,
                dcs,
                moha);

            if (confirmedByTarget.TryGetValue(dcsDs.Code, out var confirmed))
            {
                var mohaCand = mohaDsCandidates.GetValueOrDefault(confirmed.SourceCode);
                var membership = CompareMembership(dcsDs.Code, confirmed.SourceCode, dcs, moha);
                memberships.Add(membership);
                dsDeltaTable.Add(new DsDeltaRow(
                    dcsDs.Code,
                    dcsDs.English,
                    confirmed.SourceCode,
                    mohaCand?.AgreedEnglish ?? mohaCand?.EnglishVariants.FirstOrDefault(),
                    mohaCand?.AgreedSinhala ?? mohaCand?.SinhalaVariants.FirstOrDefault(),
                    mohaCand?.AgreedTamil ?? mohaCand?.TamilVariants.FirstOrDefault(),
                    DeltaClassifications.ConfirmedRecode,
                    confirmed.Evidence,
                    "Confirmed"));
                candidates.Add(new DeltaCandidate(
                    dcsDs.Code,
                    "DivisionalSecretariat",
                    dcsDs.English,
                    confirmed.SourceCode,
                    mohaCand?.AgreedEnglish,
                    null,
                    "Confirmed mapping file",
                    DeltaClassifications.ConfirmedRecode));
                continue;
            }

            var classification = discovery is null
                ? DeltaClassifications.NoCorrespondingMohaEntity
                : DeltaClassifications.PotentialMatchRequiringReview;

            dsDeltaTable.Add(new DsDeltaRow(
                dcsDs.Code,
                dcsDs.English,
                discovery?.MohaCode,
                discovery?.MohaEnglish,
                discovery?.Sinhala,
                discovery?.Tamil,
                classification,
                discovery?.Evidence ?? "No same-district MOHA DS with formatting-compatible English and GN-component bijection.",
                "Unresolved"));

            candidates.Add(new DeltaCandidate(
                dcsDs.Code,
                "DivisionalSecretariat",
                dcsDs.English,
                discovery?.MohaCode,
                discovery?.MohaEnglish,
                null,
                discovery?.DiscoveryBasis ?? "No candidate",
                classification));

            if (discovery?.MohaCode is not null)
            {
                memberships.Add(CompareMembership(dcsDs.Code, discovery.MohaCode, dcs, moha));
            }
        }

        foreach (var mohaDs in unmatchedMohaDs)
        {
            if (confirmedBySource.ContainsKey(mohaDs.Code))
            {
                continue;
            }

            if (dsDeltaTable.Any(r => string.Equals(r.MohaCandidateCode, mohaDs.Code, StringComparison.Ordinal)))
            {
                continue;
            }

            var cand = mohaDsCandidates.GetValueOrDefault(mohaDs.Code);
            dsDeltaTable.Add(new DsDeltaRow(
                "(none)",
                null,
                mohaDs.Code,
                mohaDs.English,
                cand?.AgreedSinhala ?? cand?.SinhalaVariants.FirstOrDefault(),
                cand?.AgreedTamil ?? cand?.TamilVariants.FirstOrDefault(),
                DeltaClassifications.PotentialMatchRequiringReview,
                "MOHA DS without DCS code match; not paired to an unmatched DCS DS under confirmation rules.",
                "Unresolved"));
        }

        // GN-level candidates: same district, same English (formatting-only), preserved GN component under different DS.
        foreach (var dcsRow in unmatchedDcsGn)
        {
            if (confirmedGnTargets.Contains(dcsRow.GndUid))
            {
                candidates.Add(new DeltaCandidate(
                    dcsRow.GndUid,
                    "GramaNiladhariDivision",
                    dcsRow.EnglishGn,
                    confirmedMappings.First(m => m.TargetCode == dcsRow.GndUid).SourceCode,
                    null,
                    null,
                    "Confirmed mapping file",
                    DeltaClassifications.ConfirmedRecode));
                continue;
            }

            // Covered by confirmed DS child propagation?
            if (confirmedByTarget.TryGetValue(dcsRow.DsCode, out var dsMap)
                && string.Equals(
                    dsMap.ChildPropagation,
                    AdministrativeMappingTypes.ChildPropagationGnComponentUnchanged,
                    StringComparison.Ordinal))
            {
                candidates.Add(new DeltaCandidate(
                    dcsRow.GndUid,
                    "GramaNiladhariDivision",
                    dcsRow.EnglishGn,
                    dsMap.SourceCode + dcsRow.GnComponent,
                    null,
                    null,
                    "Child propagation from confirmed DS recode",
                    DeltaClassifications.ConfirmedRecode));
                continue;
            }

            var gnCandidates = unmatchedMohaGn
                .Where(m => string.Equals(m.HierarchicalDistrictCode, dcsRow.DistrictCode, StringComparison.Ordinal))
                .Where(m =>
                    EnglishNameDifferenceClassifier.IsFormattingOnly(
                        EnglishNameDifferenceClassifier.Classify(dcsRow.EnglishGn, m.English))
                    || string.Equals(dcsRow.EnglishGn, m.English, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(dcsRow.GnComponent, m.GnComponent, StringComparison.Ordinal))
                .Take(5)
                .ToList();

            if (gnCandidates.Count == 0)
            {
                candidates.Add(new DeltaCandidate(
                    dcsRow.GndUid,
                    "GramaNiladhariDivision",
                    dcsRow.EnglishGn,
                    null,
                    null,
                    null,
                    "No same-district MOHA candidate by English/GN component",
                    DeltaClassifications.NoCorrespondingMohaEntity));
            }
            else
            {
                foreach (var c in gnCandidates)
                {
                    var basis = string.Equals(dcsRow.GnComponent, c.GnComponent, StringComparison.Ordinal)
                        ? "Same district + same GN component"
                        : "Same district + English name similarity (discovery only)";
                    candidates.Add(new DeltaCandidate(
                        dcsRow.GndUid,
                        "GramaNiladhariDivision",
                        dcsRow.EnglishGn,
                        c.NormalizedLifeCode,
                        c.English,
                        c.LifeCode,
                        basis,
                        DeltaClassifications.PotentialMatchRequiringReview));
                }
            }
        }

        var overlayDsTargets = overlays
            .Where(o => string.Equals(o.Type, AdministrativeMappingTypes.DivisionalSecretariat, StringComparison.Ordinal))
            .Select(o => o.DcsCode)
            .ToHashSet(StringComparer.Ordinal);
        var overlayGnTargets = overlays
            .Where(o => string.Equals(o.Type, AdministrativeMappingTypes.GramaNiladhariDivision, StringComparison.Ordinal))
            .Select(o => o.DcsCode)
            .ToHashSet(StringComparer.Ordinal);

        var ds5225 = Build5225Findings(dcs, moha, overlayDsTargets.Contains("5225"));
        var unresolved = BuildUnresolvedGaps(
            dcs,
            unmatchedDcsGn,
            unmatchedDcsDs,
            candidates,
            confirmedByTarget,
            confirmedGnTargets,
            unmatchedMohaGn,
            projected,
            overlayDsTargets,
            overlayGnTargets);

        var explanation =
            $"DCS unmatched GN={unmatchedDcsGn.Count}; MOHA unmatched GN={unmatchedMohaGn.Count}; "
            + $"MOHA total unique GN={mohaByNormalized.Count}; DCS total GN={dcs.GramaNiladhariDivisions.Count}. "
            + "Difference is structural (DS recodes, GN transfers/splits, dataset date skew), not a forced 1:1 map.";

        return new AdministrativeDeltaReport(
            unmatchedDcsGn,
            unmatchedMohaGn,
            dcsGroups,
            mohaGroups,
            dsDeltaTable,
            candidates,
            memberships,
            ds5225,
            unresolved,
            explanation);
    }

    private sealed record DiscoveredDs(
        string MohaCode,
        string? MohaEnglish,
        string? Sinhala,
        string? Tamil,
        string DiscoveryBasis,
        string Evidence);

    private static DiscoveredDs? DiscoverDsCandidate(
        string dcsCode,
        string? dcsEnglish,
        string districtCode,
        IReadOnlyList<UnmatchedRecord> unmatchedMohaDs,
        IReadOnlyDictionary<string, MohaEntityCandidate> mohaDsCandidates,
        CanonicalDataset dcs,
        MohaParseResult moha)
    {
        DiscoveredDs? bestWithoutBijection = null;

        foreach (var mohaDs in unmatchedMohaDs)
        {
            // Same district: MOHA hierarchical DS starts with district code.
            if (!mohaDs.Code.StartsWith(districtCode, StringComparison.Ordinal))
            {
                continue;
            }

            var cand = mohaDsCandidates.GetValueOrDefault(mohaDs.Code);
            var mohaEnglish = cand?.AgreedEnglish ?? mohaDs.English;
            var kind = EnglishNameDifferenceClassifier.Classify(dcsEnglish, mohaEnglish);
            var nameCompatible = kind == EnglishNameDifferenceKind.Exact
                || EnglishNameDifferenceClassifier.IsFormattingOnly(kind)
                || kind == EnglishNameDifferenceKind.Spelling;

            if (!nameCompatible)
            {
                continue;
            }

            var membership = CompareMembership(dcsCode, mohaDs.Code, dcs, moha);
            if (membership.SameGnComponents)
            {
                return new DiscoveredDs(
                    mohaDs.Code,
                    mohaEnglish,
                    cand?.AgreedSinhala ?? cand?.SinhalaVariants.FirstOrDefault(),
                    cand?.AgreedTamil ?? cand?.TamilVariants.FirstOrDefault(),
                    "Same district + compatible English + GN-component bijection",
                    "Discovery only — requires confirmed mapping with authoritative evidence.");
            }

            if (bestWithoutBijection is null
                && membership.DcsGnCount == membership.MohaGnCount
                && membership.DcsGnCount > 0)
            {
                bestWithoutBijection = new DiscoveredDs(
                    mohaDs.Code,
                    mohaEnglish,
                    cand?.AgreedSinhala ?? cand?.SinhalaVariants.FirstOrDefault(),
                    cand?.AgreedTamil ?? cand?.TamilVariants.FirstOrDefault(),
                    "Same district + compatible English + same GN count but different GN components (not a simple recode)",
                    "Discovery only — GN components differ; do not use GnComponentUnchanged without per-GN evidence.");
            }
        }

        return bestWithoutBijection;
    }

    public static GnMembershipComparison CompareMembership(
        string dcsDsCode,
        string mohaDsCode,
        CanonicalDataset dcs,
        MohaParseResult moha)
    {
        var dcsGns = dcs.GramaNiladhariDivisions
            .Where(g => string.Equals(g.DivisionalSecretariatCode, dcsDsCode, StringComparison.Ordinal))
            .ToList();
        var mohaGns = moha.Records
            .Where(r => string.Equals(r.HierarchicalDsCode, mohaDsCode, StringComparison.Ordinal))
            .GroupBy(r => r.GnComponent, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        var dcsComponents = dcsGns
            .Select(g => g.Code.Length >= 3 ? g.Code[^3..] : g.Code)
            .ToHashSet(StringComparer.Ordinal);
        var mohaComponents = mohaGns.Select(r => r.GnComponent).ToHashSet(StringComparer.Ordinal);

        var onlyDcs = dcsComponents.Except(mohaComponents, StringComparer.Ordinal).OrderBy(c => c).ToList();
        var onlyMoha = mohaComponents.Except(dcsComponents, StringComparer.Ordinal).OrderBy(c => c).ToList();
        var sameComponents = dcsComponents.SetEquals(mohaComponents);

        var englishOk = true;
        if (sameComponents)
        {
            var mohaByComp = mohaGns.ToDictionary(r => r.GnComponent, StringComparer.Ordinal);
            foreach (var gn in dcsGns)
            {
                var comp = gn.Code.Length >= 3 ? gn.Code[^3..] : gn.Code;
                if (!mohaByComp.TryGetValue(comp, out var mohaRow))
                {
                    englishOk = false;
                    break;
                }

                var kind = EnglishNameDifferenceClassifier.Classify(gn.Name.English, mohaRow.EnglishName);
                if (kind is not (EnglishNameDifferenceKind.Exact
                    or EnglishNameDifferenceKind.CaseOnly
                    or EnglishNameDifferenceKind.WhitespaceOnly
                    or EnglishNameDifferenceKind.Punctuation
                    or EnglishNameDifferenceKind.Spelling))
                {
                    englishOk = false;
                    break;
                }
            }
        }
        else
        {
            englishOk = false;
        }

        return new GnMembershipComparison(
            dcsDsCode,
            mohaDsCode,
            dcsGns.Count,
            mohaGns.Count,
            sameComponents,
            englishOk,
            sameComponents && englishOk,
            onlyDcs,
            onlyMoha);
    }

    private static Ds5225Findings Build5225Findings(
        CanonicalDataset dcs,
        MohaParseResult moha,
        bool resolvedByOverlay)
    {
        var dcs5225 = dcs.DivisionalSecretariats.FirstOrDefault(d => d.Code == "5225");
        var dcs5221 = dcs.DivisionalSecretariats.FirstOrDefault(d => d.Code == "5221");
        var rows = moha.Records
            .Where(r => string.Equals(r.HierarchicalDsCode, "5225", StringComparison.Ordinal))
            .ToList();
        var english = rows.Select(r => r.DsEnglish).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.Ordinal).OrderBy(v => v).ToList()!;
        var sinhala = rows.Select(r => r.DsSinhala).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.Ordinal).OrderBy(v => v).ToList()!;
        var tamil = rows.Select(r => r.DsTamil).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.Ordinal).OrderBy(v => v).ToList()!;

        var diagnosis =
            "MOHA source inconsistency: LIFe segment 5-2-25 is used for both Sainthamaruthu and Kalmunai North Sub Office rows. "
            + "Parser correctly reads per-row DS labels; aggregation by HierarchicalDsCode produces TRANSLATION_CONFLICT. "
            + "Not a parser bug. DCS keeps 5225=Sainthamaruthu and 5221=Kalmunai North Sub. "
            + "DS-level Si/Ta for DCS 5225 use exact-joined GN rows under that DS; when those rows still conflict, "
            + "rows are narrowed to MOHA DS English formatting/spelling-compatible with DCS English (no majority vote).";
        if (resolvedByOverlay)
        {
            diagnosis +=
                " Phase 3.8: DCS 5225 Sinhala/Tamil supplied via authoritative-name overlay from MOHA "
                + "Sainthamaruthu-labelled rows only (Kalmunai North Sub Office rows excluded), corroborated by PubAd "
                + "listing Saindamarudu and Kalmunai North as separate Ampara DS units.";
        }

        return new Ds5225Findings(
            dcs5225?.Name.English ?? "(missing)",
            dcs5221?.Name.English ?? "(missing)",
            english!,
            sinhala!,
            tamil!,
            diagnosis,
            Resolved: resolvedByOverlay);
    }

    private static List<UnresolvedGapRecord> BuildUnresolvedGaps(
        CanonicalDataset dcs,
        IReadOnlyList<EnrichedUnmatchedDcsGn> unmatchedDcsGn,
        IReadOnlyList<UnmatchedRecord> unmatchedDcsDs,
        IReadOnlyList<DeltaCandidate> candidates,
        IReadOnlyDictionary<string, AdministrativeCodeMapping> confirmedDsByTarget,
        HashSet<string> confirmedGnTargets,
        IReadOnlyList<EnrichedUnmatchedMohaGn> unmatchedMohaGn,
        ProjectedCoverageResult? projected,
        IReadOnlySet<string> overlayDsTargets,
        IReadOnlySet<string> overlayGnTargets)
    {
        var gaps = new List<UnresolvedGapRecord>();
        var dcsDsByCode = dcs.DivisionalSecretariats.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var dcsDistrictByCode = dcs.Districts.ToDictionary(d => d.Code, StringComparer.Ordinal);
        var coveredDs = projected?.CoveredDsCodes ?? new HashSet<string>(StringComparer.Ordinal);
        var coveredGn = projected?.CoveredGnCodes ?? new HashSet<string>(StringComparer.Ordinal);
        var emittedDs = new HashSet<string>(StringComparer.Ordinal);

        foreach (var ds in unmatchedDcsDs)
        {
            if (confirmedDsByTarget.ContainsKey(ds.Code) || overlayDsTargets.Contains(ds.Code) || coveredDs.Contains(ds.Code))
            {
                continue;
            }

            if (!dcsDsByCode.TryGetValue(ds.Code, out var dsEntity)
                || !dcsDistrictByCode.TryGetValue(dsEntity.DistrictCode, out var district))
            {
                continue;
            }

            var cands = candidates
                .Where(c => c.EntityType == "DivisionalSecretariat" && c.DcsCode == ds.Code && c.MohaCode is not null)
                .Select(c => (object)new
                {
                    code = c.MohaCode,
                    english = c.MohaEnglish,
                    classification = c.Classification,
                    discoveryBasis = c.DiscoveryBasis
                })
                .ToList();

            gaps.Add(new UnresolvedGapRecord(
                "DivisionalSecretariat",
                ds.Code,
                ds.English,
                district.ProvinceCode,
                district.Code,
                ds.Code,
                "DCS DS has no confirmed MOHA code mapping and no authoritative name overlay.",
                cands,
                "Compared unmatched MOHA DS in same district for English compatibility and GN-component bijection; Gazette/District/DS targeted search recorded in final gap resolution report.",
                [
                    "dcs-administrative-division-codes",
                    "moha-life-location-codes",
                    "documents.gov.lk (targeted)",
                    "Relevant District/Divisional Secretariat sites"
                ],
                "No dual-authority code mapping with GN-component bijection, and no Gazette/DS-page Si/Ta pair accepted without inventing names."));
            emittedDs.Add(ds.Code);
        }

        // Include code-matched DCS DS that still lack both authoritative Sinhala and Tamil (e.g. 5225).
        // Only when projection ran — otherwise CoveredDsCodes is empty and would false-emit all DS.
        if (projected is not null)
        {
        foreach (var dsEntity in dcs.DivisionalSecretariats)
        {
            if (emittedDs.Contains(dsEntity.Code)
                || coveredDs.Contains(dsEntity.Code)
                || overlayDsTargets.Contains(dsEntity.Code)
                || confirmedDsByTarget.ContainsKey(dsEntity.Code))
            {
                continue;
            }

            if (!dcsDistrictByCode.TryGetValue(dsEntity.DistrictCode, out var district))
            {
                continue;
            }

            var reason = string.Equals(dsEntity.Code, "5225", StringComparison.Ordinal)
                ? "MOHA LIFe segment 5-2-25 carries conflicting DS labels (Sainthamaruthu vs Kalmunai North Sub Office); raw TRANSLATION_CONFLICT remains."
                : "DCS DS lacks agreed authoritative Sinhala and Tamil after exact join, mappings, and overlays.";

            gaps.Add(new UnresolvedGapRecord(
                "DivisionalSecretariat",
                dsEntity.Code,
                dsEntity.Name.English,
                district.ProvinceCode,
                district.Code,
                dsEntity.Code,
                reason,
                [],
                "Phase 3.8 coverage projection: entity not present in CoveredDsCodes (requires both Si and Ta).",
                [
                    "dcs-administrative-division-codes",
                    "moha-life-location-codes",
                    "pubad.gov.lk Ampara DS list",
                    "documents.gov.lk (targeted)"
                ],
                string.Equals(dsEntity.Code, "5225", StringComparison.Ordinal)
                    ? "Cannot assign a single MOHA DS-level translation from the mixed LIFe segment without an explicit filtered overlay; majority vote and first-row selection are disallowed."
                    : "Authoritative bilingual name not established."));
            emittedDs.Add(dsEntity.Code);
        }
        }

        foreach (var gn in unmatchedDcsGn)
        {
            if (confirmedGnTargets.Contains(gn.GndUid)
                || overlayGnTargets.Contains(gn.GndUid)
                || coveredGn.Contains(gn.GndUid))
            {
                continue;
            }

            if (confirmedDsByTarget.TryGetValue(gn.DsCode, out var dsMap)
                && string.Equals(
                    dsMap.ChildPropagation,
                    AdministrativeMappingTypes.ChildPropagationGnComponentUnchanged,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var cands = unmatchedMohaGn
                .Where(m => string.Equals(m.HierarchicalDistrictCode, gn.DistrictCode, StringComparison.Ordinal))
                .Where(m =>
                    string.Equals(m.GnComponent, gn.GnComponent, StringComparison.Ordinal)
                    || string.Equals(m.English, gn.EnglishGn, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .Select(m => (object)new
                {
                    lifeCode = m.LifeCode,
                    normalizedLifeCode = m.NormalizedLifeCode,
                    english = m.English,
                    sinhala = m.Sinhala,
                    tamil = m.Tamil,
                    ds = m.HierarchicalDsCode
                })
                .ToList();

            gaps.Add(new UnresolvedGapRecord(
                "GramaNiladhariDivision",
                gn.GndUid,
                gn.EnglishGn,
                gn.ProvinceCode,
                gn.DistrictCode,
                gn.DsCode,
                "DCS GN has no confirmed MOHA mapping; Sinhala/Tamil left empty.",
                cands,
                "Discovery candidates generated from same-district English/GN-component similarity only; not auto-approved.",
                [
                    "dcs-administrative-division-codes",
                    "moha-life-location-codes",
                    "documents.gov.lk (targeted)",
                    "Relevant District/Divisional Secretariat sites"
                ],
                "English-name similarity and equal GN counts are discovery evidence only; no Gazette or official GN code list confirmed identity without collision risk."));
        }

        return gaps;
    }
}
