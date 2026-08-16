using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LankaLens.DataBuilder.Joining;

namespace LankaLens.DataBuilder.Reporting;

internal static class MohaJoinReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void Write(MohaJoinReport report, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var dto = new
        {
            summary = new
            {
                dcs = new
                {
                    provinces = report.Summary.DcsProvinces,
                    districts = report.Summary.DcsDistricts,
                    divisionalSecretariats = report.Summary.DcsDivisionalSecretariats,
                    gramaNiladhariDivisions = report.Summary.DcsGramaNiladhariDivisions
                },
                moha = new
                {
                    provinces = report.Summary.MohaProvinces,
                    districts = report.Summary.MohaDistricts,
                    divisionalSecretariats = report.Summary.MohaDivisionalSecretariats,
                    gramaNiladhariDivisions = report.Summary.MohaGramaNiladhariDivisions
                },
                gnJoin = new
                {
                    matched = report.Summary.GnMatched,
                    dcsUnmatched = report.Summary.DcsGnUnmatched,
                    mohaUnmatched = report.Summary.MohaGnUnmatched,
                    duplicateMohaCodes = report.Summary.DuplicateMohaCodes,
                    duplicateDcsCodes = report.Summary.DuplicateDcsCodes,
                    invalidLifeCodes = report.Summary.InvalidLifeCodes,
                    hierarchyMismatches = report.Summary.HierarchyMismatches
                },
                missingMohaTranslationsOnMatchedGn = new
                {
                    english = report.Summary.MissingMohaEnglish,
                    sinhala = report.Summary.MissingMohaSinhala,
                    tamil = report.Summary.MissingMohaTamil
                },
                englishComparison = new
                {
                    exact = report.Summary.EnglishExact,
                    formattingOnly = report.Summary.EnglishFormattingOnly,
                    spellingOrSubstantive = report.Summary.EnglishSpellingOrSubstantive
                },
                freshness = new
                {
                    mohaSourceDate = report.Summary.MohaSourceDate ?? "unknown",
                    mohaRetrievedDate = report.Summary.MohaRetrievedDate,
                    dcsEffectiveDate = report.Summary.DcsEffectiveDate
                },
                coverage = new
                {
                    province = MapCoverage(report.ProvinceCoverage),
                    district = MapCoverage(report.DistrictCoverage),
                    ds = MapCoverage(report.DsCoverage),
                    gn = MapCoverage(report.GnCoverage)
                }
            },
            unmatchedDcs = report.UnmatchedDcs.Select(r => new
            {
                entityType = r.EntityType,
                code = r.Code,
                english = r.English,
                provinceCode = r.ProvinceCode,
                districtCode = r.DistrictCode,
                dsCode = r.DsCode,
                gnComponent = r.GnComponent,
                englishProvince = r.EnglishProvince,
                englishDistrict = r.EnglishDistrict,
                englishDs = r.EnglishDs
            }),
            unmatchedMoha = report.UnmatchedMoha.Select(r => new
            {
                entityType = r.EntityType,
                normalizedLifeCode = r.Code,
                lifeCode = r.LifeCode,
                english = r.English,
                sinhala = r.Sinhala,
                tamil = r.Tamil,
                provinceCode = r.ProvinceCode,
                districtCode = r.DistrictCode,
                dsCode = r.DsCode,
                gnComponent = r.GnComponent,
                englishProvince = r.EnglishProvince,
                englishDistrict = r.EnglishDistrict,
                englishDs = r.EnglishDs
            }),
            invalidLifeCodes = report.InvalidLifeCodes.Select(i => new
            {
                rawValue = i.RawValue,
                reason = i.Reason,
                sourceReportFile = i.SourceReportFile,
                sourceRowNumber = i.SourceRowNumber
            }),
            duplicateMohaCodes = report.DuplicateMohaCodes,
            hierarchyMismatches = report.HierarchyMismatches.Select(h => new
            {
                lifeCode = h.LifeCode,
                normalizedLifeCode = h.NormalizedLifeCode,
                field = h.Field,
                mohaValue = h.MohaValue,
                dcsValue = h.DcsValue
            }),
            translationConflicts = report.TranslationConflicts.Select(c => new
            {
                entityType = c.EntityType,
                code = c.Code,
                field = c.Field,
                values = c.Values
            }),
            suspiciousMultilingual = report.SuspiciousMultilingual.Select(c => new
            {
                entityType = c.EntityType,
                code = c.Code,
                field = c.Field,
                values = c.Values
            }),
            sampleFieldProvenance = report.SampleProvenance.ToDictionary(
                kv => kv.Key,
                kv => new
                {
                    code = new { value = kv.Value.Code.Value, sourceId = kv.Value.Code.SourceId },
                    english = new { value = kv.Value.English.Value, sourceId = kv.Value.English.SourceId },
                    sinhala = new { value = kv.Value.Sinhala.Value, sourceId = kv.Value.Sinhala.SourceId },
                    tamil = new { value = kv.Value.Tamil.Value, sourceId = kv.Value.Tamil.SourceId }
                })
        };

        File.WriteAllText(outputPath, JsonSerializer.Serialize(dto, JsonOptions) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static object MapCoverage(LevelCoverage coverage) => new
    {
        dcsTotal = coverage.DcsTotal,
        mohaUnique = coverage.MohaUnique,
        mohaMatched = coverage.MohaMatched,
        sinhalaAvailable = coverage.SinhalaAvailable,
        tamilAvailable = coverage.TamilAvailable,
        translationConflicts = coverage.TranslationConflicts,
        dcsWithoutMoha = coverage.DcsWithoutMoha,
        mohaWithoutDcs = coverage.MohaWithoutDcs
    };
}
