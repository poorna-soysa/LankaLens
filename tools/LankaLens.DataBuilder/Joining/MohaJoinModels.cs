using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Parsing;

namespace LankaLens.DataBuilder.Joining;

internal sealed record LanguageProvenance(
    string? Value,
    string? SourceId);

internal sealed record FieldProvenance(
    LanguageProvenance Code,
    LanguageProvenance English,
    LanguageProvenance Sinhala,
    LanguageProvenance Tamil);

internal sealed record UnmatchedRecord(
    string EntityType,
    string Code,
    string? English,
    string? LifeCode = null,
    string? ProvinceCode = null,
    string? DistrictCode = null,
    string? DsCode = null,
    string? GnComponent = null,
    string? EnglishProvince = null,
    string? EnglishDistrict = null,
    string? EnglishDs = null,
    string? Sinhala = null,
    string? Tamil = null);

internal sealed record HierarchyMismatch(
    string LifeCode,
    string NormalizedLifeCode,
    string Field,
    string? MohaValue,
    string? DcsValue);

internal sealed record EnglishNameDifference(
    string EntityType,
    string Code,
    string? DcsEnglish,
    string? MohaEnglish,
    EnglishNameDifferenceKind Kind);

internal sealed record LevelCoverage(
    string EntityType,
    int DcsTotal,
    int MohaUnique,
    int MohaMatched,
    int SinhalaAvailable,
    int TamilAvailable,
    int TranslationConflicts,
    int DcsWithoutMoha,
    int MohaWithoutDcs);

internal sealed record MohaJoinSummary(
    int DcsProvinces,
    int DcsDistricts,
    int DcsDivisionalSecretariats,
    int DcsGramaNiladhariDivisions,
    int MohaProvinces,
    int MohaDistricts,
    int MohaDivisionalSecretariats,
    int MohaGramaNiladhariDivisions,
    int GnMatched,
    int DcsGnUnmatched,
    int MohaGnUnmatched,
    int DuplicateMohaCodes,
    int DuplicateDcsCodes,
    int InvalidLifeCodes,
    int HierarchyMismatches,
    int MissingMohaEnglish,
    int MissingMohaSinhala,
    int MissingMohaTamil,
    int EnglishExact,
    int EnglishFormattingOnly,
    int EnglishSpellingOrSubstantive,
    string? MohaSourceDate,
    string MohaRetrievedDate,
    string DcsEffectiveDate);

internal sealed record MohaJoinReport(
    MohaJoinSummary Summary,
    LevelCoverage ProvinceCoverage,
    LevelCoverage DistrictCoverage,
    LevelCoverage DsCoverage,
    LevelCoverage GnCoverage,
    IReadOnlyList<UnmatchedRecord> UnmatchedDcs,
    IReadOnlyList<UnmatchedRecord> UnmatchedMoha,
    IReadOnlyList<InvalidLifeCode> InvalidLifeCodes,
    IReadOnlyList<string> DuplicateMohaCodes,
    IReadOnlyList<HierarchyMismatch> HierarchyMismatches,
    IReadOnlyList<TranslationConflict> TranslationConflicts,
    IReadOnlyList<TranslationConflict> SuspiciousMultilingual,
    IReadOnlyList<EnglishNameDifference> EnglishDifferences,
    IReadOnlyList<SourceConflict> Conflicts,
    IReadOnlyDictionary<string, FieldProvenance> SampleProvenance);
