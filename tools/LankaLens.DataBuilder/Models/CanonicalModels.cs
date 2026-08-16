namespace LankaLens.DataBuilder.Models;

internal sealed record CanonicalLocalizedName(
    string? English,
    string? Sinhala,
    string? Tamil);

internal sealed record CanonicalProvince(
    string Code,
    CanonicalLocalizedName Name);

internal sealed record CanonicalDistrict(
    string Code,
    string ProvinceCode,
    CanonicalLocalizedName Name);

internal sealed record CanonicalDivisionalSecretariat(
    string Code,
    string DistrictCode,
    CanonicalLocalizedName Name);

internal sealed record CanonicalGramaNiladhariDivision(
    string Code,
    string DivisionalSecretariatCode,
    CanonicalLocalizedName Name,
    string? GnNumber = null);

internal sealed record CanonicalDatasetMetadata(
    string SourceOrganization,
    string SourceName,
    string? SourceVersion,
    DateOnly? EffectiveDate,
    DateOnly RetrievedDate);

internal sealed record CanonicalDataset(
    CanonicalDatasetMetadata Metadata,
    IReadOnlyList<CanonicalProvince> Provinces,
    IReadOnlyList<CanonicalDistrict> Districts,
    IReadOnlyList<CanonicalDivisionalSecretariat> DivisionalSecretariats,
    IReadOnlyList<CanonicalGramaNiladhariDivision> GramaNiladhariDivisions);
