using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Parsing;

/// <summary>
/// Splits MOHA slash-separated hierarchy labels such as
/// "1: බස්නාහිර/ மேற்கு/ Western" into English/Sinhala/Tamil by Unicode script.
/// </summary>
internal static class MohaHierarchyLabelParser
{
    internal sealed record ParsedHierarchyLabel(
        string? NumericPrefix,
        string? English,
        string? Sinhala,
        string? Tamil);

    public static ParsedHierarchyLabel Parse(string? raw)
    {
        var normalized = MohaNameNormalizer.Normalize(raw);
        if (normalized is null)
        {
            return new ParsedHierarchyLabel(null, null, null, null);
        }

        string? prefix = null;
        var rest = normalized;
        var colon = normalized.IndexOf(':');
        if (colon >= 0)
        {
            var maybePrefix = normalized[..colon].Trim();
            if (maybePrefix.Length > 0 && maybePrefix.All(char.IsAsciiDigit))
            {
                prefix = maybePrefix;
                rest = normalized[(colon + 1)..].Trim();
            }
        }

        if (rest.Length == 0)
        {
            return new ParsedHierarchyLabel(prefix, null, null, null);
        }

        var tokens = rest
            .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        string? english = null;
        string? sinhala = null;
        string? tamil = null;

        foreach (var token in tokens)
        {
            var value = MohaNameNormalizer.Normalize(token);
            if (value is null)
            {
                continue;
            }

            if (MohaNameNormalizer.HasSinhalaScript(value))
            {
                sinhala = AppendUnique(sinhala, value);
            }
            else if (MohaNameNormalizer.HasTamilScript(value))
            {
                tamil = AppendUnique(tamil, value);
            }
            else
            {
                english = AppendUnique(english, value);
            }
        }

        return new ParsedHierarchyLabel(prefix, english, sinhala, tamil);
    }

    private static string AppendUnique(string? existing, string value)
    {
        if (existing is null)
        {
            return value;
        }

        if (string.Equals(existing, value, StringComparison.Ordinal))
        {
            return existing;
        }

        return existing + " / " + value;
    }
}
