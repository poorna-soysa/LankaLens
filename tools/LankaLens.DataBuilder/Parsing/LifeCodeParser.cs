namespace LankaLens.DataBuilder.Parsing;

/// <summary>
/// Parses official MOHA LIFe codes using the verified Province-District-DSD-GND structure.
/// Example: 1-1-03-005 → join key 1103005, DCS hierarchy 1 / 11 / 1103 / 1103005.
/// Does not invent missing zero-padding.
/// </summary>
internal static class LifeCodeParser
{
    public const int ProvinceWidth = 1;
    public const int DistrictWidth = 1;
    public const int DsWidth = 2;
    public const int GnWidth = 3;

    internal sealed record ParsedLifeCode(
        string LifeCode,
        string NormalizedLifeCode,
        string ProvinceComponent,
        string DistrictComponent,
        string DsComponent,
        string GnComponent,
        string HierarchicalProvinceCode,
        string HierarchicalDistrictCode,
        string HierarchicalDsCode);

    public static bool TryParse(string? raw, out ParsedLifeCode? parsed, out string? error)
    {
        parsed = null;
        error = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "LIFe code is empty.";
            return false;
        }

        var lifeCode = raw.Trim();
        var parts = lifeCode.Split('-');
        if (parts.Length != 4)
        {
            error = $"LIFe code '{lifeCode}' does not have four hyphen-separated components (Province-District-DSD-GND).";
            return false;
        }

        var province = parts[0];
        var district = parts[1];
        var ds = parts[2];
        var gn = parts[3];

        if (!IsDigitComponent(province, ProvinceWidth))
        {
            error = $"LIFe code '{lifeCode}' has an invalid province component '{province}' (expected {ProvinceWidth} digit).";
            return false;
        }

        if (!IsDigitComponent(district, DistrictWidth))
        {
            error = $"LIFe code '{lifeCode}' has an invalid district component '{district}' (expected {DistrictWidth} digit).";
            return false;
        }

        if (!IsDigitComponent(ds, DsWidth))
        {
            error = $"LIFe code '{lifeCode}' has an invalid DSD component '{ds}' (expected {DsWidth} digits with official zero-padding).";
            return false;
        }

        if (!IsDigitComponent(gn, GnWidth))
        {
            error = $"LIFe code '{lifeCode}' has an invalid GND component '{gn}' (expected {GnWidth} digits with official zero-padding).";
            return false;
        }

        var normalized = string.Concat(province, district, ds, gn);
        parsed = new ParsedLifeCode(
            LifeCode: lifeCode,
            NormalizedLifeCode: normalized,
            ProvinceComponent: province,
            DistrictComponent: district,
            DsComponent: ds,
            GnComponent: gn,
            HierarchicalProvinceCode: province,
            HierarchicalDistrictCode: string.Concat(province, district),
            HierarchicalDsCode: string.Concat(province, district, ds));
        return true;
    }

    private static bool IsDigitComponent(string value, int expectedWidth)
    {
        if (value.Length != expectedWidth)
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (!char.IsAsciiDigit(value[i]))
            {
                return false;
            }
        }

        return true;
    }
}
