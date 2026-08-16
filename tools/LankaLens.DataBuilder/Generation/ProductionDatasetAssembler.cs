using LankaLens.DataBuilder.Mappings;
using LankaLens.DataBuilder.Models;

namespace LankaLens.DataBuilder.Generation;

/// <summary>
/// Materializes a production <see cref="CanonicalDataset"/> from DCS hierarchy/English
/// plus resolved MOHA/mapping/overlay Sinhala and Tamil names.
/// Unresolved translations remain <see langword="null"/> (never empty strings or placeholders).
/// </summary>
internal static class ProductionDatasetAssembler
{
    public static CanonicalDataset Assemble(
        CanonicalDataset dcs,
        LocalizedNameMaps names,
        CanonicalDatasetMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(dcs);
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(metadata);

        var provinces = dcs.Provinces
            .Select(p => new CanonicalProvince(
                p.Code,
                MergeName(p.Name, names.Provinces.GetValueOrDefault(p.Code))))
            .OrderBy(p => p.Code, StringComparer.Ordinal)
            .ToList();

        var districts = dcs.Districts
            .Select(d => new CanonicalDistrict(
                d.Code,
                d.ProvinceCode,
                MergeName(d.Name, names.Districts.GetValueOrDefault(d.Code))))
            .OrderBy(d => d.Code, StringComparer.Ordinal)
            .ToList();

        var divisionalSecretariats = dcs.DivisionalSecretariats
            .Select(ds => new CanonicalDivisionalSecretariat(
                ds.Code,
                ds.DistrictCode,
                MergeName(ds.Name, names.DivisionalSecretariats.GetValueOrDefault(ds.Code))))
            .OrderBy(ds => ds.Code, StringComparer.Ordinal)
            .ToList();

        var gramaNiladhariDivisions = dcs.GramaNiladhariDivisions
            .Select(gn => new CanonicalGramaNiladhariDivision(
                gn.Code,
                gn.DivisionalSecretariatCode,
                MergeName(gn.Name, names.GramaNiladhariDivisions.GetValueOrDefault(gn.Code)),
                gn.GnNumber))
            .OrderBy(gn => gn.Code, StringComparer.Ordinal)
            .ToList();

        return new CanonicalDataset(
            metadata,
            provinces,
            districts,
            divisionalSecretariats,
            gramaNiladhariDivisions);
    }

    public static CanonicalDatasetMetadata CreateProductionMetadata(
        CanonicalDatasetMetadata dcsMetadata)
    {
        return new CanonicalDatasetMetadata(
            SourceOrganization: "Department of Census and Statistics, Sri Lanka; Ministry of Home Affairs, Sri Lanka",
            SourceName: "Sri Lanka administrative divisions (DCS codes/hierarchy/English; MOHA LIFe and verified overlays for Sinhala/Tamil)",
            SourceVersion: dcsMetadata.SourceVersion,
            EffectiveDate: dcsMetadata.EffectiveDate,
            RetrievedDate: dcsMetadata.RetrievedDate);
    }

    private static CanonicalLocalizedName MergeName(
        CanonicalLocalizedName dcsName,
        (string? Sinhala, string? Tamil) resolved)
    {
        // Prefer already-present DCS Sinhala/Tamil (synthetic fixtures); otherwise use resolved MOHA/overlay values.
        var sinhala = NormalizeOptional(dcsName.Sinhala) ?? NormalizeOptional(resolved.Sinhala);
        var tamil = NormalizeOptional(dcsName.Tamil) ?? NormalizeOptional(resolved.Tamil);

        return new CanonicalLocalizedName(
            NormalizeRequiredEnglish(dcsName.English),
            sinhala,
            tamil);
    }

    private static string NormalizeRequiredEnglish(string? english)
    {
        if (string.IsNullOrWhiteSpace(english))
        {
            return string.Empty;
        }

        return english.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
