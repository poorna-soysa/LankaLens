using System.Net;
using System.Text;

namespace LankaLens.DataBuilder.Normalization;

/// <summary>
/// MOHA-specific name cleanup. Preserves Sinhala/Tamil letters and ZWJ/ZWNJ conjuncts.
/// Strips bidirectional marks that appear in some official HTML cells.
/// </summary>
internal static class MohaNameNormalizer
{
    public static string? Normalize(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var decoded = WebUtility.HtmlDecode(value);
        var stripped = StripBidirectionalMarks(decoded);
        return TextNormalizer.NormalizeOptionalText(stripped);
    }

    public static bool HasSinhalaScript(string? value) => ContainsRange(value, '\u0D80', '\u0DFF');

    public static bool HasTamilScript(string? value) => ContainsRange(value, '\u0B80', '\u0BFF');

    public static bool LooksLikePlaceholderOrEmpty(string? value) =>
        string.IsNullOrWhiteSpace(Normalize(value));

    private static bool ContainsRange(string? value, char start, char end)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (var c in value)
        {
            if (c >= start && c <= end)
            {
                return true;
            }
        }

        return false;
    }

    private static string StripBidirectionalMarks(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (c is '\u200E' or '\u200F' or '\u200B' or '\uFEFF' or '\u00AD')
            {
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
