namespace LankaLens.DataBuilder.Joining;

internal enum EnglishNameDifferenceKind
{
    Exact,
    CaseOnly,
    WhitespaceOnly,
    Punctuation,
    Spelling,
    Substantive,
    MissingMohaEnglish,
    MissingDcsEnglish
}

internal static class EnglishNameDifferenceClassifier
{
    public static EnglishNameDifferenceKind Classify(string? dcsEnglish, string? mohaEnglish)
    {
        if (string.IsNullOrWhiteSpace(dcsEnglish) && string.IsNullOrWhiteSpace(mohaEnglish))
        {
            return EnglishNameDifferenceKind.Exact;
        }

        if (string.IsNullOrWhiteSpace(dcsEnglish))
        {
            return EnglishNameDifferenceKind.MissingDcsEnglish;
        }

        if (string.IsNullOrWhiteSpace(mohaEnglish))
        {
            return EnglishNameDifferenceKind.MissingMohaEnglish;
        }

        if (string.Equals(dcsEnglish, mohaEnglish, StringComparison.Ordinal))
        {
            return EnglishNameDifferenceKind.Exact;
        }

        if (string.Equals(dcsEnglish, mohaEnglish, StringComparison.OrdinalIgnoreCase))
        {
            return EnglishNameDifferenceKind.CaseOnly;
        }

        var dcsWs = CollapseWhitespace(dcsEnglish);
        var mohaWs = CollapseWhitespace(mohaEnglish);
        if (string.Equals(dcsWs, mohaWs, StringComparison.Ordinal))
        {
            return EnglishNameDifferenceKind.WhitespaceOnly;
        }

        var dcsPunct = StripPunctuation(dcsEnglish);
        var mohaPunct = StripPunctuation(mohaEnglish);
        if (string.Equals(dcsPunct, mohaPunct, StringComparison.OrdinalIgnoreCase))
        {
            return EnglishNameDifferenceKind.Punctuation;
        }

        var dcsLetters = LettersOnly(dcsEnglish);
        var mohaLetters = LettersOnly(mohaEnglish);
        if (dcsLetters.Length > 0 && mohaLetters.Length > 0 && IsSpellingDifference(dcsLetters, mohaLetters))
        {
            return EnglishNameDifferenceKind.Spelling;
        }

        return EnglishNameDifferenceKind.Substantive;
    }

    public static bool IsFormattingOnly(EnglishNameDifferenceKind kind) =>
        kind is EnglishNameDifferenceKind.CaseOnly
            or EnglishNameDifferenceKind.WhitespaceOnly
            or EnglishNameDifferenceKind.Punctuation;

    private static string CollapseWhitespace(string value)
    {
        var chars = value.Where(c => !char.IsWhiteSpace(c)).ToArray();
        return new string(chars);
    }

    private static string StripPunctuation(string value)
    {
        var chars = value
            .Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c))
            .ToArray();
        return new string(chars);
    }

    private static string LettersOnly(string value)
    {
        var chars = value
            .Where(char.IsLetter)
            .Select(char.ToLowerInvariant)
            .ToArray();
        return new string(chars);
    }

    private static bool IsSpellingDifference(string a, string b)
    {
        var distance = Levenshtein(a, b);
        var max = Math.Max(a.Length, b.Length);
        return distance <= 2 || distance <= Math.Max(1, (int)Math.Ceiling(max * 0.2));
    }

    private static int Levenshtein(string a, string b)
    {
        var n = a.Length;
        var m = b.Length;
        var prev = new int[m + 1];
        var curr = new int[m + 1];
        for (var j = 0; j <= m; j++)
        {
            prev[j] = j;
        }

        for (var i = 1; i <= n; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= m; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }

            (prev, curr) = (curr, prev);
        }

        return prev[m];
    }
}
