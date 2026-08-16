using LankaLens.AdministrativeDivisions.Internal;

namespace LankaLens.AdministrativeDivisions.Tests.Fixtures;

/// <summary>
/// Synthetic fixture for ranking/lookup unit tests. Not used by <see cref="AdministrativeDivisions.Default"/>.
/// </summary>
internal static class SyntheticAdministrativeDataset
{
    public const string ProvinceAlphaCode = "DEV-P-A";
    public const string ProvinceBetaCode = "DEV-P-B";

    public const string DistrictAlpha1Code = "DEV-D-A1";
    public const string DistrictAlpha2Code = "DEV-D-A2";
    public const string DistrictBeta1Code = "DEV-D-B1";

    public const string DivisionalSecretariatAlpha1XCode = "DEV-DS-A1X";
    public const string DivisionalSecretariatAlpha1YCode = "DEV-DS-A1Y";
    public const string DivisionalSecretariatBeta1XCode = "DEV-DS-B1X";

    public const string GramaNiladhariGroveCode = "DEV-GN-GROVE";
    public const string GramaNiladhariGrovelandCode = "DEV-GN-GROVELAND";
    public const string GramaNiladhariNorthgroveCode = "DEV-GN-NORTHGROVE";
    public const string GramaNiladhariCedarCode = "DEV-GN-CEDAR";

    public const string SinhalaExactToken = "සිංහලනිරීක්ෂණ";
    public const string TamilExactToken = "தமிழ்ஆய்வு";

    public static AdministrativeDivisionSnapshot CreateSnapshot()
    {
        var metadata = new DatasetMetadata(
            SourceOrganization: "LankaLens (test fixture)",
            SourceName: "Synthetic ranking fixture",
            SourceVersion: "test-fixture-0.1",
            EffectiveDate: null,
            RetrievedDate: new DateOnly(2026, 1, 1));

        var provinces = new[]
        {
            new Province(
                ProvinceAlphaCode,
                new LocalizedName("Alpha Province", "ඇල්ෆා පළාත", "ஆல்ஃபா மாகாணம்")),
            new Province(
                ProvinceBetaCode,
                new LocalizedName("Beta Province", "බීටා පළාත", "பீட்டா மாகாணம்"))
        };

        var districts = new[]
        {
            new District(
                DistrictAlpha1Code,
                ProvinceAlphaCode,
                new LocalizedName("Alpha One District", "ඇල්ෆා එක දිස්ත්‍රික්කය", "ஆல்ஃபா ஒன் மாவட்டம்")),
            new District(
                DistrictAlpha2Code,
                ProvinceAlphaCode,
                new LocalizedName("Alpha Two District", "ඇල්ෆා දෙක දිස්ත්‍රික්කය", "ஆல்ஃபா டூ மாவட்டம்")),
            new District(
                DistrictBeta1Code,
                ProvinceBetaCode,
                new LocalizedName("Beta One District", "බීටා එක දිස්ත්‍රික්කය", "பீட்டா ஒன் மாவட்டம்"))
        };

        var divisionalSecretariats = new[]
        {
            new DivisionalSecretariat(
                DivisionalSecretariatAlpha1XCode,
                DistrictAlpha1Code,
                new LocalizedName("Alpha One X DS", "ඇල්ෆා එක එක්ස්", "ஆல்ஃபா ஒன் எக்ஸ்")),
            new DivisionalSecretariat(
                DivisionalSecretariatAlpha1YCode,
                DistrictAlpha1Code,
                new LocalizedName("Alpha One Y DS", "ඇල්ෆා එක වයි", "ஆல்ஃபா ஒன் ஒய்")),
            new DivisionalSecretariat(
                DivisionalSecretariatBeta1XCode,
                DistrictBeta1Code,
                new LocalizedName("Beta One X DS", "බීටා එක එක්ස්", "பீட்டா ஒன் எக்ஸ்"))
        };

        var gramaNiladhariDivisions = new[]
        {
            new GramaNiladhariDivision(
                GramaNiladhariGroveCode,
                DivisionalSecretariatAlpha1XCode,
                new LocalizedName("Grove", SinhalaExactToken, "குரோவ்")),
            new GramaNiladhariDivision(
                GramaNiladhariGrovelandCode,
                DivisionalSecretariatAlpha1XCode,
                new LocalizedName("Groveland", "ග්‍රෝව්ලන්ඩ්", "குரோவ்லேண்ட்")),
            new GramaNiladhariDivision(
                GramaNiladhariNorthgroveCode,
                DivisionalSecretariatAlpha1YCode,
                new LocalizedName("Northgrove", "නොර්ත්ග්‍රෝව්", "நார்த்குரோவ்")),
            new GramaNiladhariDivision(
                GramaNiladhariCedarCode,
                DivisionalSecretariatBeta1XCode,
                new LocalizedName("Cedar", "සීඩර්", TamilExactToken))
        };

        return new AdministrativeDivisionSnapshot(
            metadata,
            provinces,
            districts,
            divisionalSecretariats,
            gramaNiladhariDivisions);
    }

    public static IAdministrativeDivisionProvider CreateProvider() =>
        new AdministrativeDivisionProvider(CreateSnapshot());
}
