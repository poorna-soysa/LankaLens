using System.Text;
using System.Text.RegularExpressions;

namespace LankaLens.DataBuilder.Normalization;

/// <summary>
/// Explicit, tested text normalization rules for DCS spreadsheet values.
/// Does not alter Sinhala or Tamil characters beyond ASCII whitespace / Excel control noise.
/// </summary>
internal static partial class TextNormalizer
{
    private static readonly HashSet<string> MissingPlaceholders = new(StringComparer.OrdinalIgnoreCase)
    {
        "TODO",
        "TBD",
        "N/A",
        "NA",
        "NULL",
        "Unknown",
        "-",
        "—"
    };

    /// <summary>
    /// Trims, collapses accidental repeated ASCII spaces, strips Excel CR/_x000D_ noise,
    /// and maps known missing placeholders to null.
    /// </summary>
    public static string? NormalizeOptionalText(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var cleaned = StripExcelControlNoise(value);
        cleaned = cleaned.Trim();
        cleaned = CollapseAsciiSpaces().Replace(cleaned, " ");

        if (cleaned.Length == 0)
        {
            return null;
        }

        if (MissingPlaceholders.Contains(cleaned))
        {
            return null;
        }

        return cleaned;
    }

    /// <summary>
    /// Normalizes an official code cell. Codes remain strings; leading zeros are not invented here.
    /// </summary>
    public static string? NormalizeCode(string? value)
    {
        var text = NormalizeOptionalText(value);
        if (text is null)
        {
            return null;
        }

        // Excel may emit integer-looking codes with a trailing .0
        if (text.Contains('.', StringComparison.Ordinal)
            && double.TryParse(text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var number)
            && number == Math.Floor(number)
            && number >= 0
            && number < 1_000_000_000)
        {
            return ((long)number).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return text;
    }

    private static string StripExcelControlNoise(string value)
    {
        var sb = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c is '\r' or '\n' or '\u000D' or '\u000A')
            {
                continue;
            }

            sb.Append(c);
        }

        var withoutControls = sb.ToString();
        return withoutControls.Replace("_x000D_", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@" {2,}", RegexOptions.CultureInvariant)]
    private static partial Regex CollapseAsciiSpaces();
}
