using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Normalization;

/// <summary>
/// Builds hierarchical census codes from GND_UID and collapses raw GN rows into canonical entities.
/// </summary>
internal static class CanonicalNormalizer
{
    public static CanonicalDataset Normalize(
        IReadOnlyList<RawAdministrativeRecord> rawRecords,
        CanonicalDatasetMetadata metadata)
    {
        var provinces = new Dictionary<string, CanonicalProvince>(StringComparer.Ordinal);
        var districts = new Dictionary<string, CanonicalDistrict>(StringComparer.Ordinal);
        var dsDivisions = new Dictionary<string, CanonicalDivisionalSecretariat>(StringComparer.Ordinal);
        var gnDivisions = new Dictionary<string, CanonicalGramaNiladhariDivision>(StringComparer.Ordinal);

        foreach (var raw in rawRecords)
        {
            var gnUid = TextNormalizer.NormalizeCode(raw.GnUid);
            if (string.IsNullOrWhiteSpace(gnUid) || gnUid.Length < 4)
            {
                continue;
            }

            var provinceCode = TextNormalizer.NormalizeCode(raw.ProvinceCode)
                ?? (gnUid.Length >= 1 ? gnUid[..1] : null);
            if (string.IsNullOrWhiteSpace(provinceCode))
            {
                continue;
            }

            // Approved policy: hierarchical census codes evidenced by GND_UID.
            var districtCode = gnUid.Length >= 2 ? gnUid[..2] : null;
            var dsCode = gnUid.Length >= 4 ? gnUid[..4] : null;
            if (districtCode is null || dsCode is null)
            {
                continue;
            }

            if (!provinces.ContainsKey(provinceCode))
            {
                provinces[provinceCode] = new CanonicalProvince(
                    provinceCode,
                    new CanonicalLocalizedName(
                        TextNormalizer.NormalizeOptionalText(raw.ProvinceEnglish),
                        TextNormalizer.NormalizeOptionalText(raw.ProvinceSinhala),
                        TextNormalizer.NormalizeOptionalText(raw.ProvinceTamil)));
            }

            if (!districts.ContainsKey(districtCode))
            {
                districts[districtCode] = new CanonicalDistrict(
                    districtCode,
                    provinceCode,
                    new CanonicalLocalizedName(
                        TextNormalizer.NormalizeOptionalText(raw.DistrictEnglish),
                        TextNormalizer.NormalizeOptionalText(raw.DistrictSinhala),
                        TextNormalizer.NormalizeOptionalText(raw.DistrictTamil)));
            }

            if (!dsDivisions.ContainsKey(dsCode))
            {
                dsDivisions[dsCode] = new CanonicalDivisionalSecretariat(
                    dsCode,
                    districtCode,
                    new CanonicalLocalizedName(
                        TextNormalizer.NormalizeOptionalText(raw.DsEnglish),
                        TextNormalizer.NormalizeOptionalText(raw.DsSinhala),
                        TextNormalizer.NormalizeOptionalText(raw.DsTamil)));
            }

            if (!gnDivisions.ContainsKey(gnUid))
            {
                gnDivisions[gnUid] = new CanonicalGramaNiladhariDivision(
                    gnUid,
                    dsCode,
                    new CanonicalLocalizedName(
                        TextNormalizer.NormalizeOptionalText(raw.GnEnglish),
                        TextNormalizer.NormalizeOptionalText(raw.GnSinhala),
                        TextNormalizer.NormalizeOptionalText(raw.GnTamil)),
                    TextNormalizer.NormalizeOptionalText(raw.GnNumber));
            }
        }

        return new CanonicalDataset(
            metadata,
            provinces.Values.OrderBy(p => p.Code, StringComparer.Ordinal).ToList(),
            districts.Values.OrderBy(d => d.Code, StringComparer.Ordinal).ToList(),
            dsDivisions.Values.OrderBy(d => d.Code, StringComparer.Ordinal).ToList(),
            gnDivisions.Values.OrderBy(g => g.Code, StringComparer.Ordinal).ToList());
    }

    /// <summary>
    /// Reconstructs the expected GND_UID from component codes using the verified DCS formula.
    /// </summary>
    public static string? BuildExpectedGnUid(
        string? provinceCode,
        string? districtCode,
        string? dsCode,
        string? gnCode)
    {
        var p = TextNormalizer.NormalizeCode(provinceCode);
        var d = TextNormalizer.NormalizeCode(districtCode);
        var ds = TextNormalizer.NormalizeCode(dsCode);
        var gn = TextNormalizer.NormalizeCode(gnCode);

        if (p is null || d is null || ds is null || gn is null)
        {
            return null;
        }

        if (!int.TryParse(ds, out var dsNumber) || !int.TryParse(gn, out var gnNumber))
        {
            return null;
        }

        return $"{p}{d}{dsNumber.ToString("00", System.Globalization.CultureInfo.InvariantCulture)}{gnNumber.ToString("000", System.Globalization.CultureInfo.InvariantCulture)}";
    }
}
