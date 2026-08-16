using LankaLens.DataBuilder.Models;
using LankaLens.DataBuilder.Normalization;

namespace LankaLens.DataBuilder.Joining;

internal sealed record TranslationConflict(
    string EntityType,
    string Code,
    string Field,
    IReadOnlyList<string> Values);

internal static class TranslationConsistencyChecker
{
    public static IReadOnlyList<TranslationConflict> CheckRepeatedEntities(
        string entityType,
        IReadOnlyList<MohaEntityCandidate> entities)
    {
        var conflicts = new List<TranslationConflict>();
        foreach (var entity in entities)
        {
            AddIfConflict(conflicts, entityType, entity.Code, "English", entity.EnglishVariants);
            AddIfConflict(conflicts, entityType, entity.Code, "Sinhala", entity.SinhalaVariants);
            AddIfConflict(conflicts, entityType, entity.Code, "Tamil", entity.TamilVariants);
        }

        return conflicts;
    }

    public static IReadOnlyList<TranslationConflict> CheckSuspiciousMultilingual(
        string entityType,
        IReadOnlyList<MohaEntityCandidate> entities)
    {
        var issues = new List<TranslationConflict>();
        foreach (var entity in entities)
        {
            var english = entity.AgreedEnglish;
            var sinhala = entity.AgreedSinhala;
            var tamil = entity.AgreedTamil;

            if (english is not null && sinhala is not null
                && string.Equals(english, sinhala, StringComparison.Ordinal))
            {
                issues.Add(new TranslationConflict(entityType, entity.Code, "IdenticalEnglishSinhala", [english]));
            }

            if (english is not null && tamil is not null
                && string.Equals(english, tamil, StringComparison.Ordinal))
            {
                issues.Add(new TranslationConflict(entityType, entity.Code, "IdenticalEnglishTamil", [english]));
            }

            if (sinhala is not null && !MohaNameNormalizer.HasSinhalaScript(sinhala))
            {
                issues.Add(new TranslationConflict(entityType, entity.Code, "SinhalaMissingSinhalaScript", [sinhala]));
            }

            if (tamil is not null && !MohaNameNormalizer.HasTamilScript(tamil))
            {
                issues.Add(new TranslationConflict(entityType, entity.Code, "TamilMissingTamilScript", [tamil]));
            }
        }

        return issues;
    }

    public static MohaEntityCandidate Aggregate(
        string code,
        IEnumerable<(string? English, string? Sinhala, string? Tamil)> rows)
    {
        var english = DistinctValues(rows.Select(r => r.English));
        var sinhala = DistinctValues(rows.Select(r => r.Sinhala));
        var tamil = DistinctValues(rows.Select(r => r.Tamil));
        return new MohaEntityCandidate(code, english, sinhala, tamil);
    }

    private static void AddIfConflict(
        List<TranslationConflict> conflicts,
        string entityType,
        string code,
        string field,
        IReadOnlyList<string> values)
    {
        if (values.Count > 1)
        {
            conflicts.Add(new TranslationConflict(entityType, code, field, values));
        }
    }

    private static IReadOnlyList<string> DistinctValues(IEnumerable<string?> values) =>
        values
            .Select(MohaNameNormalizer.Normalize)
            .Where(v => v is not null)
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .OrderBy(v => v, StringComparer.Ordinal)
            .ToList();
}

internal sealed record MohaEntityCandidate(
    string Code,
    IReadOnlyList<string> EnglishVariants,
    IReadOnlyList<string> SinhalaVariants,
    IReadOnlyList<string> TamilVariants)
{
    public string? AgreedEnglish => EnglishVariants.Count == 1 ? EnglishVariants[0] : null;

    public string? AgreedSinhala => SinhalaVariants.Count == 1 ? SinhalaVariants[0] : null;

    public string? AgreedTamil => TamilVariants.Count == 1 ? TamilVariants[0] : null;

    public bool HasSinhala => AgreedSinhala is not null;

    public bool HasTamil => AgreedTamil is not null;
}
