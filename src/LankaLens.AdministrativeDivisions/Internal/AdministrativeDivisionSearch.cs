using System.Collections.ObjectModel;

namespace LankaLens.AdministrativeDivisions.Internal;

/// <summary>
/// Exact / prefix / contains search with deterministic ranking for administrative divisions.
/// </summary>
internal static class AdministrativeDivisionSearch
{
    private enum MatchRank
    {
        Exact = 0,
        Prefix = 1,
        Contains = 2
    }

    public static IReadOnlyList<AdministrativeDivisionSearchResult> Execute(
        string query,
        AdministrativeDivisionSearchOptions? options,
        IReadOnlyList<Province> provinces,
        IReadOnlyList<District> districts,
        IReadOnlyList<DivisionalSecretariat> divisionalSecretariats,
        IReadOnlyList<GramaNiladhariDivision> gramaNiladhariDivisions,
        IReadOnlyDictionary<string, District> districtsByCode,
        IReadOnlyDictionary<string, DivisionalSecretariat> divisionalSecretariatsByCode)
    {
        Language? language = options?.Language;
        AdministrativeDivisionType? typeFilter = options?.Type;
        int? maxResults = options?.MaxResults;

        var matches = new List<(MatchRank Rank, AdministrativeDivisionSearchResult Result)>();

        if (typeFilter is null or AdministrativeDivisionType.Province)
        {
            foreach (var province in provinces)
            {
                if (TryMatch(province.Name, query, language, out var rank))
                {
                    matches.Add((
                        rank,
                        new AdministrativeDivisionSearchResult(
                            province.Code,
                            AdministrativeDivisionType.Province,
                            province.Name,
                            ProvinceCode: null,
                            DistrictCode: null,
                            DivisionalSecretariatCode: null)));
                }
            }
        }

        if (typeFilter is null or AdministrativeDivisionType.District)
        {
            foreach (var district in districts)
            {
                if (TryMatch(district.Name, query, language, out var rank))
                {
                    matches.Add((
                        rank,
                        new AdministrativeDivisionSearchResult(
                            district.Code,
                            AdministrativeDivisionType.District,
                            district.Name,
                            district.ProvinceCode,
                            DistrictCode: null,
                            DivisionalSecretariatCode: null)));
                }
            }
        }

        if (typeFilter is null or AdministrativeDivisionType.DivisionalSecretariat)
        {
            foreach (var division in divisionalSecretariats)
            {
                if (TryMatch(division.Name, query, language, out var rank))
                {
                    string? provinceCode = districtsByCode.TryGetValue(division.DistrictCode, out var parentDistrict)
                        ? parentDistrict.ProvinceCode
                        : null;

                    matches.Add((
                        rank,
                        new AdministrativeDivisionSearchResult(
                            division.Code,
                            AdministrativeDivisionType.DivisionalSecretariat,
                            division.Name,
                            provinceCode,
                            division.DistrictCode,
                            DivisionalSecretariatCode: null)));
                }
            }
        }

        if (typeFilter is null or AdministrativeDivisionType.GramaNiladhariDivision)
        {
            foreach (var division in gramaNiladhariDivisions)
            {
                if (TryMatch(division.Name, query, language, out var rank))
                {
                    string? districtCode = null;
                    string? provinceCode = null;

                    if (divisionalSecretariatsByCode.TryGetValue(
                            division.DivisionalSecretariatCode,
                            out var parentDs)
                        && districtsByCode.TryGetValue(parentDs.DistrictCode, out var parentDistrict))
                    {
                        districtCode = parentDs.DistrictCode;
                        provinceCode = parentDistrict.ProvinceCode;
                    }

                    matches.Add((
                        rank,
                        new AdministrativeDivisionSearchResult(
                            division.Code,
                            AdministrativeDivisionType.GramaNiladhariDivision,
                            division.Name,
                            provinceCode,
                            districtCode,
                            division.DivisionalSecretariatCode)));
                }
            }
        }

        matches.Sort(static (left, right) =>
        {
            int rankCompare = left.Rank.CompareTo(right.Rank);
            if (rankCompare != 0)
            {
                return rankCompare;
            }

            int typeCompare = left.Result.Type.CompareTo(right.Result.Type);
            if (typeCompare != 0)
            {
                return typeCompare;
            }

            int englishIgnoreCase = string.Compare(
                left.Result.Name.English,
                right.Result.Name.English,
                StringComparison.OrdinalIgnoreCase);
            if (englishIgnoreCase != 0)
            {
                return englishIgnoreCase;
            }

            int englishOrdinal = string.CompareOrdinal(
                left.Result.Name.English,
                right.Result.Name.English);
            if (englishOrdinal != 0)
            {
                return englishOrdinal;
            }

            return string.CompareOrdinal(left.Result.Code, right.Result.Code);
        });

        IEnumerable<(MatchRank Rank, AdministrativeDivisionSearchResult Result)> ordered = matches;
        if (maxResults is int limit)
        {
            ordered = matches.Take(limit);
        }

        var results = ordered.Select(static m => m.Result).ToArray();
        return new ReadOnlyCollection<AdministrativeDivisionSearchResult>(results);
    }

    private static bool TryMatch(
        LocalizedName name,
        string query,
        Language? language,
        out MatchRank bestRank)
    {
        bestRank = MatchRank.Contains;
        var any = false;

        if (language is null or Language.English)
        {
            if (TryMatchField(name.English, query, StringComparison.OrdinalIgnoreCase, out var rank))
            {
                any = true;
                bestRank = rank;
            }
        }

        if (language is null or Language.Sinhala)
        {
            if (TryMatchField(name.Sinhala, query, StringComparison.Ordinal, out var rank))
            {
                if (!any || rank < bestRank)
                {
                    bestRank = rank;
                }

                any = true;
            }
        }

        if (language is null or Language.Tamil)
        {
            if (TryMatchField(name.Tamil, query, StringComparison.Ordinal, out var rank))
            {
                if (!any || rank < bestRank)
                {
                    bestRank = rank;
                }

                any = true;
            }
        }

        return any;
    }

    private static bool TryMatchField(
        string? value,
        string query,
        StringComparison comparison,
        out MatchRank rank)
    {
        if (value is null)
        {
            rank = default;
            return false;
        }

        if (string.Equals(value, query, comparison))
        {
            rank = MatchRank.Exact;
            return true;
        }

        if (value.StartsWith(query, comparison))
        {
            rank = MatchRank.Prefix;
            return true;
        }

        if (value.Contains(query, comparison))
        {
            rank = MatchRank.Contains;
            return true;
        }

        rank = default;
        return false;
    }
}
