using LankaLens.DataBuilder.Delta;
using LankaLens.DataBuilder.Joining;
using LankaLens.DataBuilder.Mappings;
using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Parsing;
using LankaLens.DataBuilder.Reporting;
using LankaLens.DataBuilder.Tests.Fixtures;

namespace LankaLens.DataBuilder.Tests;

public sealed class MappingAndDeltaTests
{
    [Fact]
    public void Confirmed_recode_mapping_passes_validation_and_projects_coverage()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(
            Row("1-1-39-005", "005", "Mount Lavinia", "මවුන්ට්", "மவுண்ட்", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"),
            Row("1-1-39-010", "010", "Ratmalana East", "නැගෙනහිර", "கிழக்கு", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"));

        var mappings = new[]
        {
            ValidMapping(
                AdministrativeMappingTypes.DivisionalSecretariat,
                "1139",
                "1131",
                childPropagation: AdministrativeMappingTypes.ChildPropagationGnComponentUnchanged)
        };

        var validation = MappingFileValidator.Validate(mappings, dcs, moha);
        Assert.True(validation.Passed, string.Join("; ", validation.Issues.Select(i => i.Message)));

        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);
        Assert.Equal(0, join.Summary.GnMatched);
        Assert.Equal(2, join.Summary.DcsGnUnmatched);

        var projected = MappingApplicator.Apply(dcs, moha, join, mappings).Coverage;
        Assert.Equal(1, projected.AppliedDsMappings);
        Assert.Equal(2, projected.AppliedChildPropagations);
        Assert.Equal(2, projected.GnSinhala);
        Assert.Equal(2, projected.GnTamil);
        Assert.True(projected.DsSinhala >= 1);
        Assert.True(projected.DsTamil >= 1);
    }

    [Fact]
    public void Unknown_source_or_target_mapping_is_rejected()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(Row("1-1-39-005", "005", "Mount Lavinia", "ම", "ம", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"));

        var unknownSource = new[]
        {
            ValidMapping(AdministrativeMappingTypes.DivisionalSecretariat, "9999", "1131")
        };
        var unknownTarget = new[]
        {
            ValidMapping(AdministrativeMappingTypes.DivisionalSecretariat, "1139", "9999")
        };

        Assert.Contains(
            MappingFileValidator.Validate(unknownSource, dcs, moha).Issues,
            i => i.Code == "UNKNOWN_SOURCE_CODE");
        Assert.Contains(
            MappingFileValidator.Validate(unknownTarget, dcs, moha).Issues,
            i => i.Code == "UNKNOWN_TARGET_CODE");
    }

    [Fact]
    public void Duplicate_and_contradictory_mappings_are_rejected()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(
            Row("1-1-39-005", "005", "A", "අ", "அ", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"),
            Row("1-1-40-005", "005", "B", "බ", "ப", "40: වෙනත්/ மற்ற/ Other"));

        // Extend DCS with a second DS so two targets exist.
        dcs = dcs with
        {
            DivisionalSecretariats =
            [
                ..dcs.DivisionalSecretariats,
                new CanonicalDivisionalSecretariat("1140", "11", new CanonicalLocalizedName("Other", null, null))
            ]
        };

        var duplicateSource = new[]
        {
            ValidMapping(AdministrativeMappingTypes.DivisionalSecretariat, "1139", "1131", childPropagation: null),
            ValidMapping(AdministrativeMappingTypes.DivisionalSecretariat, "1139", "1140", childPropagation: null)
        };
        Assert.Contains(
            MappingFileValidator.Validate(duplicateSource, dcs, moha).Issues,
            i => i.Code == "DUPLICATE_SOURCE_MAPPING");

        var duplicateTarget = new[]
        {
            ValidMapping(AdministrativeMappingTypes.DivisionalSecretariat, "1139", "1131", childPropagation: null),
            ValidMapping(AdministrativeMappingTypes.DivisionalSecretariat, "1140", "1131", childPropagation: null)
        };
        Assert.Contains(
            MappingFileValidator.Validate(duplicateTarget, dcs, moha).Issues,
            i => i.Code == "DUPLICATE_TARGET_MAPPING");
    }

    [Fact]
    public void Mapping_provenance_fields_are_required()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(Row("1-1-39-005", "005", "A", "අ", "அ", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"));

        var incomplete = new AdministrativeCodeMapping(
            AdministrativeMappingTypes.DivisionalSecretariat,
            "1139",
            "1131",
            Reason: "",
            SourceId: "",
            Evidence: "",
            EvidenceUrl: "",
            EffectiveDate: null,
            ReviewNote: "",
            ChildPropagation: null,
            AllowTranslationReuse: false);

        var issues = MappingFileValidator.Validate([incomplete], dcs, moha).Issues;
        Assert.Contains(issues, i => i.Code == "MISSING_EVIDENCE");
    }

    [Fact]
    public void Child_propagation_requires_gn_component_bijection()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(
            Row("1-1-39-005", "005", "Mount Lavinia", "ම", "ம", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"),
            Row("1-1-39-099", "099", "Extra", "එ", "எ", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"));

        var mappings = new[]
        {
            ValidMapping(
                AdministrativeMappingTypes.DivisionalSecretariat,
                "1139",
                "1131",
                childPropagation: AdministrativeMappingTypes.ChildPropagationGnComponentUnchanged)
        };

        Assert.Contains(
            MappingFileValidator.Validate(mappings, dcs, moha).Issues,
            i => i.Code == "CHILD_PROPAGATION_NOT_BIJECTION");
    }

    [Fact]
    public void Explicit_gn_transfer_mapping_projects_without_ds_propagation()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(Row("1-1-39-005", "005", "Mount Lavinia", "මවුන්ට්", "மவுண்ட்", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"));

        var mappings = new[]
        {
            ValidMapping(
                AdministrativeMappingTypes.GramaNiladhariDivision,
                "1139005",
                "1131005",
                childPropagation: null)
        };

        var validation = MappingFileValidator.Validate(mappings, dcs, moha);
        Assert.True(validation.Passed, string.Join("; ", validation.Issues.Select(i => i.Message)));

        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);
        var projected = MappingApplicator.Apply(dcs, moha, join, mappings).Coverage;
        Assert.Equal(1, projected.AppliedGnMappings);
        Assert.Equal(0, projected.AppliedDsMappings);
        Assert.Equal(1, projected.GnSinhala);
        Assert.Equal(1, projected.GnTamil);
    }

    [Fact]
    public void Unresolved_records_are_emitted_to_gaps_json()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(Row("9-1-01-001", "001", "Elsewhere", "එ", "எ", "1: වෙනත්/ மற்ற/ Elsewhere"));
        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);
        var delta = DeltaAnalyzer.Analyze(dcs, moha, join, []);

        Assert.Contains(delta.UnresolvedGaps, g => g.DcsCode == "1131005");
        Assert.Contains(delta.UnresolvedGaps, g => g.Type == "DivisionalSecretariat" && g.DcsCode == "1131");

        var temp = Path.Combine(Path.GetTempPath(), "lankalens-gaps-" + Guid.NewGuid().ToString("n") + ".json");
        try
        {
            var projected = MappingApplicator.Apply(dcs, moha, join, []).Coverage;
            AdministrativeDeltaReportWriter.WriteUnresolvedGapsJson(delta, temp);
            var json = File.ReadAllText(temp);
            Assert.Contains("1131005", json, StringComparison.Ordinal);
            Assert.Contains("unresolvedCount", json, StringComparison.Ordinal);
            Assert.True(projected.GnSinhala == 0);
        }
        finally
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
        }
    }

    [Fact]
    public void Translation_conflict_on_mixed_ds_labels_still_reported_while_matched_rows_remain_usable()
    {
        // DCS 5225 = Sainthamaruthu; MOHA mixes Sainthamarathu + Kalmunai North on 5-2-25.
        var dcs = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("5", new CanonicalLocalizedName("Eastern", null, null))],
            [new CanonicalDistrict("52", "5", new CanonicalLocalizedName("Ampara", null, null))],
            [
                new CanonicalDivisionalSecretariat("5225", "52", new CanonicalLocalizedName("Sainthamaruthu", null, null)),
                new CanonicalDivisionalSecretariat("5221", "52", new CanonicalLocalizedName("Kalmunai North Sub", null, null))
            ],
            [
                new CanonicalGramaNiladhariDivision(
                    "5225005",
                    "5225",
                    new CanonicalLocalizedName("Sainthamaruthu 01", null, null),
                    "005"),
                new CanonicalGramaNiladhariDivision(
                    "5221070",
                    "5221",
                    new CanonicalLocalizedName("Kalmunai 01", null, null),
                    "070")
            ]);

        var moha = Parse(
            Row(
                "5-2-25-005",
                "005",
                "Sainthamaruthu 1",
                "සෙයින්තමරතු 1",
                "சாய்ந்தமருது 1",
                "25: සෙයින්තමරතු/ சாய்ந்தமருது/ Sainthamarathu",
                province: "5: නැගෙනහිර/ கிழக்கு/ Eastern",
                district: "2: අම්පාර/ அம்பாறை/ Ampara"),
            Row(
                "5-2-25-150",
                "150",
                "Kalmunai-1",
                "කල්මුණ 1",
                "கல்முனை 1",
                "25: කල්මුණ උතුර උප කාර්යාලය/ கல்முனை வடக்கு உப பிரதேச செயலகம்/ Kalmunai North Sub Office",
                province: "5: නැගෙනහිර/ கிழக்கு/ Eastern",
                district: "2: අම්පාර/ அம்பாறை/ Ampara"));

        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        Assert.Contains(
            join.TranslationConflicts,
            c => c.EntityType == "DivisionalSecretariat" && c.Code == "5225" && c.Field == "English");
        Assert.Equal(1, join.DsCoverage.TranslationConflicts);
        // Matched-row path makes DCS 5225 Sinhala/Tamil available despite raw conflict.
        Assert.Equal(1, join.DsCoverage.SinhalaAvailable);
        Assert.Equal(1, join.DsCoverage.TamilAvailable);
        Assert.Equal(1, join.Summary.GnMatched);
        Assert.Contains(join.UnmatchedDcs, r => r.Code == "5221070");
        Assert.Contains(join.UnmatchedMoha, r => r.Code == "5225150");

        var delta = DeltaAnalyzer.Analyze(dcs, moha, join, []);
        Assert.False(delta.Ds5225.Resolved);
        Assert.Contains("source inconsistency", delta.Ds5225.Diagnosis, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Unsupported_entity_type_is_rejected()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(Row("1-1-39-005", "005", "A", "අ", "அ", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"));
        var bad = ValidMapping("Village", "1139", "1131", childPropagation: null);
        Assert.Contains(
            MappingFileValidator.Validate([bad], dcs, moha).Issues,
            i => i.Code == "UNSUPPORTED_ENTITY_TYPE");
    }

    [Fact]
    public void Ds_rename_without_child_propagation_reuses_ds_names_only()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(
            Row("1-1-39-005", "005", "Mount Lavinia", "මවුන්ට්", "மவுண்ட்", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"),
            Row("1-1-39-010", "010", "Ratmalana East", "නැගෙනහිර", "கிழக்கு", "39: රත්මලාන/ இரத்மழானை/ Ratmalana"));

        // Different GN component on MOHA side → cannot use GnComponentUnchanged.
        var mappings = new[]
        {
            ValidMapping(
                AdministrativeMappingTypes.DivisionalSecretariat,
                "1139",
                "1131",
                childPropagation: null)
        };

        Assert.True(MappingFileValidator.Validate(mappings, dcs, moha).Passed);
        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);
        var projected = MappingApplicator.Apply(dcs, moha, join, mappings).Coverage;
        Assert.Equal(1, projected.AppliedDsMappings);
        Assert.Equal(0, projected.AppliedChildPropagations);
        Assert.Equal(0, projected.GnSinhala);
        Assert.True(projected.DsSinhala >= 1);
        Assert.True(projected.DsTamil >= 1);
    }

    [Fact]
    public void Partial_overlay_does_not_count_toward_coverage()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(Row("9-1-01-001", "001", "Elsewhere", "එ", "எ", "1: වෙනත්/ மற்ற/ Elsewhere"));
        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);

        var sinhalaOnly = new AuthoritativeNameOverlay(
            AdministrativeMappingTypes.DivisionalSecretariat,
            "1131",
            Sinhala: "රත්මලාන",
            Tamil: "",
            SourceOrganization: "Test",
            Evidence: "synthetic",
            EvidenceUrl: "https://example.test",
            RetrievedOrPublishedDate: "2026-08-16",
            ReviewNote: "partial");

        var validation = AuthoritativeNameOverlayValidator.Validate([sinhalaOnly], dcs);
        Assert.Contains(validation.Issues, i => i.Code == "PARTIAL_TRANSLATION_OVERLAY");

        var projected = MappingApplicator.Apply(dcs, moha, join, [], [sinhalaOnly]).Coverage;
        Assert.Equal(0, projected.AppliedOverlays);
        Assert.False(projected.CoveredDsCodes.Contains("1131"));
    }

    [Fact]
    public void Authoritative_overlay_covers_ds_and_marks_5225_resolved()
    {
        var dcs = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("5", new CanonicalLocalizedName("Eastern", null, null))],
            [new CanonicalDistrict("52", "5", new CanonicalLocalizedName("Ampara", null, null))],
            [
                new CanonicalDivisionalSecretariat("5225", "52", new CanonicalLocalizedName("Sainthamaruthu", null, null)),
                new CanonicalDivisionalSecretariat("5221", "52", new CanonicalLocalizedName("Kalmunai North Sub", null, null))
            ],
            [
                new CanonicalGramaNiladhariDivision(
                    "5225005",
                    "5225",
                    new CanonicalLocalizedName("Sainthamaruthu 01", null, null),
                    "005"),
                new CanonicalGramaNiladhariDivision(
                    "5221070",
                    "5221",
                    new CanonicalLocalizedName("Kalmunai 01", null, null),
                    "070")
            ]);

        var moha = Parse(
            Row(
                "5-2-25-005",
                "005",
                "Sainthamaruthu 1",
                "සෙයින්තමරතු 1",
                "சாய்ந்தமருது 1",
                "25: සෙයින්තමරතු/ சாய்ந்தமருது/ Sainthamarathu",
                province: "5: නැගෙනහිර/ கிழக்கு/ Eastern",
                district: "2: අම්පාර/ அம்பாறை/ Ampara"),
            Row(
                "5-2-25-150",
                "150",
                "Kalmunai-1",
                "කල්මුණ 1",
                "கல்முனை 1",
                "25: කල්මුණ උතුර උප කාර්යාලය/ கல்முனை வடக்கு உப பிரதேச செயலகம்/ Kalmunai North Sub Office",
                province: "5: නැගෙනහිර/ கிழக்கு/ Eastern",
                district: "2: අම්පාර/ அம்பாறை/ Ampara"));

        var overlays = new[]
        {
            new AuthoritativeNameOverlay(
                AdministrativeMappingTypes.DivisionalSecretariat,
                "5225",
                Sinhala: "සෙයින්තමරතු",
                Tamil: "சாய்ந்தமருது",
                SourceOrganization: "MOHA filtered + PubAd",
                Evidence: "Filtered Sainthamaruthu-labelled MOHA rows only.",
                EvidenceUrl: "https://example.test/5225",
                RetrievedOrPublishedDate: "2026-08-16",
                ReviewNote: "Unit test overlay."),
            new AuthoritativeNameOverlay(
                AdministrativeMappingTypes.DivisionalSecretariat,
                "5221",
                Sinhala: "කල්මුණ උතුර උප කාර්යාලය",
                Tamil: "கல்முனை வடக்கு உப பிரதேச செயலகம்",
                SourceOrganization: "MOHA filtered + PubAd",
                Evidence: "Kalmunai North Sub Office labels.",
                EvidenceUrl: "https://example.test/5221",
                RetrievedOrPublishedDate: "2026-08-16",
                ReviewNote: "Unit test overlay.")
        };

        Assert.True(AuthoritativeNameOverlayValidator.Validate(overlays, dcs).Passed);
        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);
        var projected = MappingApplicator.Apply(dcs, moha, join, [], overlays).Coverage;
        Assert.Equal(2, projected.AppliedOverlays);
        Assert.True(projected.CoveredDsCodes.Contains("5225"));
        Assert.True(projected.CoveredDsCodes.Contains("5221"));

        var delta = DeltaAnalyzer.Analyze(dcs, moha, join, [], projected, overlays);
        Assert.True(delta.Ds5225.Resolved);
        Assert.DoesNotContain(delta.UnresolvedGaps, g => g.DcsCode == "5225");
        // GN under 5221 still unresolved.
        Assert.Contains(delta.UnresolvedGaps, g => g.DcsCode == "5221070");
    }

    [Fact]
    public void Unresolved_gaps_json_includes_rejection_fields_and_phase_38_marker()
    {
        var dcs = CreateRecodeDataset();
        var moha = Parse(Row("9-1-01-001", "001", "Elsewhere", "එ", "எ", "1: වෙනත්/ மற்ற/ Elsewhere"));
        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);
        var projected = MappingApplicator.Apply(dcs, moha, join, []).Coverage;
        var delta = DeltaAnalyzer.Analyze(dcs, moha, join, [], projected, []);

        var temp = Path.Combine(Path.GetTempPath(), "lankalens-gaps38-" + Guid.NewGuid().ToString("n") + ".json");
        try
        {
            AdministrativeDeltaReportWriter.WriteUnresolvedGapsJson(delta, temp);
            var json = File.ReadAllText(temp);
            Assert.Contains("Phase 3.8", json, StringComparison.Ordinal);
            Assert.Contains("sourcesInvestigated", json, StringComparison.Ordinal);
            Assert.Contains("reasonResolutionWasRejected", json, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
        }
    }

    [Fact]
    public void Ambiguous_5225_conflict_remains_unresolved_without_overlay()
    {
        var dcs = new CanonicalDataset(
            Metadata(),
            [new CanonicalProvince("5", new CanonicalLocalizedName("Eastern", null, null))],
            [new CanonicalDistrict("52", "5", new CanonicalLocalizedName("Ampara", null, null))],
            [
                new CanonicalDivisionalSecretariat("5225", "52", new CanonicalLocalizedName("Sainthamaruthu", null, null)),
                new CanonicalDivisionalSecretariat("5221", "52", new CanonicalLocalizedName("Kalmunai North Sub", null, null))
            ],
            [
                new CanonicalGramaNiladhariDivision(
                    "5225005",
                    "5225",
                    new CanonicalLocalizedName("Sainthamaruthu 01", null, null),
                    "005"),
                new CanonicalGramaNiladhariDivision(
                    "5221070",
                    "5221",
                    new CanonicalLocalizedName("Kalmunai 01", null, null),
                    "070")
            ]);

        var moha = Parse(
            Row(
                "5-2-25-005",
                "005",
                "Sainthamaruthu 1",
                "සෙයින්තමරතු 1",
                "சாய்ந்தமருது 1",
                "25: සෙයින්තමරතු/ சாய்ந்தமருது/ Sainthamarathu",
                province: "5: නැගෙනහිර/ கிழக்கு/ Eastern",
                district: "2: අම්පාර/ அம்பாறை/ Ampara"),
            Row(
                "5-2-25-150",
                "150",
                "Kalmunai-1",
                "කල්මුණ 1",
                "கல்முனை 1",
                "25: කල්මුණ උතුර උප කාර්යාලය/ கல்முனை வடக்கு உப பிரதேச செயலகம்/ Kalmunai North Sub Office",
                province: "5: නැගෙනහිර/ கிழக்கு/ Eastern",
                district: "2: අම්පාර/ அம்பாறை/ Ampara"));

        var join = MohaDcsJoiner.Join(dcs, moha, "2026-08-16", null);
        var projected = MappingApplicator.Apply(dcs, moha, join, []).Coverage;
        var delta = DeltaAnalyzer.Analyze(dcs, moha, join, [], projected, []);
        Assert.False(delta.Ds5225.Resolved);
        Assert.Contains(join.TranslationConflicts, c => c.Code == "5225");
    }

    private static AdministrativeCodeMapping ValidMapping(
        string type,
        string source,
        string target,
        string? childPropagation = null) =>
        new(
            type,
            source,
            target,
            Reason: "Official administrative recode",
            SourceId: MohaDcsJoiner.MohaSourceId,
            Evidence: "Synthetic dual-authority structural identity for unit test.",
            EvidenceUrl: "https://example.test/evidence",
            EffectiveDate: null,
            ReviewNote: "Unit test mapping.",
            ChildPropagation: childPropagation,
            AllowTranslationReuse: true);

    private static CanonicalDataset CreateRecodeDataset() =>
        new(
            Metadata(),
            [new CanonicalProvince("1", new CanonicalLocalizedName("Western", null, null))],
            [new CanonicalDistrict("11", "1", new CanonicalLocalizedName("Colombo", null, null))],
            [new CanonicalDivisionalSecretariat("1131", "11", new CanonicalLocalizedName("Ratmalana", null, null))],
            [
                new CanonicalGramaNiladhariDivision(
                    "1131005",
                    "1131",
                    new CanonicalLocalizedName("Mount Lavinia", null, null),
                    "005"),
                new CanonicalGramaNiladhariDivision(
                    "1131010",
                    "1131",
                    new CanonicalLocalizedName("Ratmalana East", null, null),
                    "010")
            ]);

    private static MohaParseResult Parse(params SyntheticMohaGnRow[] rows) =>
        new MohaGnReportParser().ParseHtml(SyntheticMohaHtmlFactory.GnReport(rows));

    private static SyntheticMohaGnRow Row(
        string lifeCode,
        string gn,
        string english,
        string sinhala,
        string tamil,
        string dsLabel,
        string province = "1: බස්නාහිර/ மேற்கு/ Western",
        string district = "1: කොළඹ/ கொழும்பு/ Colombo") =>
        new(
            LifeCode: lifeCode,
            GnComponent: gn,
            Sinhala: sinhala,
            Tamil: tamil,
            English: english,
            MpaCode: "",
            ProvinceLabel: province,
            DistrictLabel: district,
            DsLabel: dsLabel);

    private static CanonicalDatasetMetadata Metadata() =>
        new(
            "Department of Census and Statistics, Sri Lanka",
            "Test",
            "2024-03-19",
            new DateOnly(2024, 3, 19),
            new DateOnly(2026, 8, 16));
}
