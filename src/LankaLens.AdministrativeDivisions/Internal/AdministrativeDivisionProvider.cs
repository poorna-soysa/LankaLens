using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace LankaLens.AdministrativeDivisions.Internal;

/// <summary>
/// In-memory implementation of <see cref="IAdministrativeDivisionProvider"/>.
/// Builds Ordinal code indexes once from an immutable snapshot.
/// </summary>
internal sealed class AdministrativeDivisionProvider : IAdministrativeDivisionProvider
{
    private static readonly IReadOnlyList<District> EmptyDistricts =
        new ReadOnlyCollection<District>(Array.Empty<District>());

    private static readonly IReadOnlyList<DivisionalSecretariat> EmptyDivisionalSecretariats =
        new ReadOnlyCollection<DivisionalSecretariat>(Array.Empty<DivisionalSecretariat>());

    private static readonly IReadOnlyList<GramaNiladhariDivision> EmptyGramaNiladhariDivisions =
        new ReadOnlyCollection<GramaNiladhariDivision>(Array.Empty<GramaNiladhariDivision>());

    private readonly DatasetMetadata _metadata;
    private readonly IReadOnlyList<Province> _provinces;
    private readonly IReadOnlyList<District> _districts;
    private readonly IReadOnlyList<DivisionalSecretariat> _divisionalSecretariats;
    private readonly IReadOnlyList<GramaNiladhariDivision> _gramaNiladhariDivisions;

    private readonly Dictionary<string, Province> _provincesByCode;
    private readonly Dictionary<string, District> _districtsByCode;
    private readonly Dictionary<string, DivisionalSecretariat> _divisionalSecretariatsByCode;
    private readonly Dictionary<string, GramaNiladhariDivision> _gramaNiladhariDivisionsByCode;

    private readonly Dictionary<string, IReadOnlyList<District>> _districtsByProvinceCode;
    private readonly Dictionary<string, IReadOnlyList<DivisionalSecretariat>> _divisionalSecretariatsByDistrictCode;
    private readonly Dictionary<string, IReadOnlyList<GramaNiladhariDivision>> _gramaNiladhariByDivisionalSecretariatCode;

    public AdministrativeDivisionProvider(AdministrativeDivisionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _metadata = snapshot.Metadata;
        _provinces = AsReadOnly(snapshot.Provinces);
        _districts = AsReadOnly(snapshot.Districts);
        _divisionalSecretariats = AsReadOnly(snapshot.DivisionalSecretariats);
        _gramaNiladhariDivisions = AsReadOnly(snapshot.GramaNiladhariDivisions);

        _provincesByCode = ToCodeDictionary(_provinces, static p => p.Code);
        _districtsByCode = ToCodeDictionary(_districts, static d => d.Code);
        _divisionalSecretariatsByCode = ToCodeDictionary(_divisionalSecretariats, static d => d.Code);
        _gramaNiladhariDivisionsByCode = ToCodeDictionary(_gramaNiladhariDivisions, static d => d.Code);

        _districtsByProvinceCode = GroupByParent(
            _districts,
            static d => d.ProvinceCode);

        _divisionalSecretariatsByDistrictCode = GroupByParent(
            _divisionalSecretariats,
            static d => d.DistrictCode);

        _gramaNiladhariByDivisionalSecretariatCode = GroupByParent(
            _gramaNiladhariDivisions,
            static d => d.DivisionalSecretariatCode);
    }

    /// <inheritdoc />
    public DatasetMetadata DatasetMetadata => _metadata;

    /// <inheritdoc />
    public IReadOnlyList<Province> GetProvinces() => _provinces;

    /// <inheritdoc />
    public Province? GetProvinceByCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _provincesByCode.TryGetValue(code, out var province) ? province : null;
    }

    /// <inheritdoc />
    public bool TryGetProvince(string code, [NotNullWhen(true)] out Province? province)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _provincesByCode.TryGetValue(code, out province);
    }

    /// <inheritdoc />
    public IReadOnlyList<District> GetDistricts() => _districts;

    /// <inheritdoc />
    public District? GetDistrictByCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _districtsByCode.TryGetValue(code, out var district) ? district : null;
    }

    /// <inheritdoc />
    public bool TryGetDistrict(string code, [NotNullWhen(true)] out District? district)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _districtsByCode.TryGetValue(code, out district);
    }

    /// <inheritdoc />
    public IReadOnlyList<District> GetDistrictsByProvince(string provinceCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provinceCode);
        return _districtsByProvinceCode.TryGetValue(provinceCode, out var districts)
            ? districts
            : EmptyDistricts;
    }

    /// <inheritdoc />
    public IReadOnlyList<DivisionalSecretariat> GetDivisionalSecretariats() => _divisionalSecretariats;

    /// <inheritdoc />
    public DivisionalSecretariat? GetDivisionalSecretariatByCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _divisionalSecretariatsByCode.TryGetValue(code, out var division) ? division : null;
    }

    /// <inheritdoc />
    public bool TryGetDivisionalSecretariat(
        string code,
        [NotNullWhen(true)] out DivisionalSecretariat? divisionalSecretariat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _divisionalSecretariatsByCode.TryGetValue(code, out divisionalSecretariat);
    }

    /// <inheritdoc />
    public IReadOnlyList<DivisionalSecretariat> GetDivisionalSecretariatsByDistrict(string districtCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(districtCode);
        return _divisionalSecretariatsByDistrictCode.TryGetValue(districtCode, out var divisions)
            ? divisions
            : EmptyDivisionalSecretariats;
    }

    /// <inheritdoc />
    public IReadOnlyList<GramaNiladhariDivision> GetGramaNiladhariDivisions() => _gramaNiladhariDivisions;

    /// <inheritdoc />
    public GramaNiladhariDivision? GetGramaNiladhariDivisionByCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _gramaNiladhariDivisionsByCode.TryGetValue(code, out var division) ? division : null;
    }

    /// <inheritdoc />
    public bool TryGetGramaNiladhariDivision(
        string code,
        [NotNullWhen(true)] out GramaNiladhariDivision? division)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return _gramaNiladhariDivisionsByCode.TryGetValue(code, out division);
    }

    /// <inheritdoc />
    public IReadOnlyList<GramaNiladhariDivision> GetGramaNiladhariDivisionsByDivisionalSecretariat(
        string divisionalSecretariatCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(divisionalSecretariatCode);
        return _gramaNiladhariByDivisionalSecretariatCode.TryGetValue(divisionalSecretariatCode, out var divisions)
            ? divisions
            : EmptyGramaNiladhariDivisions;
    }

    /// <inheritdoc />
    public Province? GetProvinceForDistrict(string districtCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(districtCode);
        return _districtsByCode.TryGetValue(districtCode, out var district)
            ? GetProvinceByCode(district.ProvinceCode)
            : null;
    }

    /// <inheritdoc />
    public District? GetDistrictForDivisionalSecretariat(string divisionalSecretariatCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(divisionalSecretariatCode);
        return _divisionalSecretariatsByCode.TryGetValue(divisionalSecretariatCode, out var division)
            ? GetDistrictByCode(division.DistrictCode)
            : null;
    }

    /// <inheritdoc />
    public DivisionalSecretariat? GetDivisionalSecretariatForGramaNiladhariDivision(
        string gramaNiladhariDivisionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gramaNiladhariDivisionCode);
        return _gramaNiladhariDivisionsByCode.TryGetValue(gramaNiladhariDivisionCode, out var division)
            ? GetDivisionalSecretariatByCode(division.DivisionalSecretariatCode)
            : null;
    }

    /// <inheritdoc />
    public IReadOnlyList<AdministrativeDivisionSearchResult> Search(
        string query,
        AdministrativeDivisionSearchOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        if (options?.MaxResults is int maxResults)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxResults);
        }

        return AdministrativeDivisionSearch.Execute(
            query,
            options,
            _provinces,
            _districts,
            _divisionalSecretariats,
            _gramaNiladhariDivisions,
            _districtsByCode,
            _divisionalSecretariatsByCode);
    }

    private static IReadOnlyList<T> AsReadOnly<T>(IReadOnlyList<T> source)
    {
        if (source is T[] array)
        {
            return new ReadOnlyCollection<T>(array);
        }

        return new ReadOnlyCollection<T>(source.ToArray());
    }

    private static Dictionary<string, T> ToCodeDictionary<T>(
        IReadOnlyList<T> items,
        Func<T, string> codeSelector)
    {
        var dictionary = new Dictionary<string, T>(items.Count, StringComparer.Ordinal);
        foreach (var item in items)
        {
            dictionary.Add(codeSelector(item), item);
        }

        return dictionary;
    }

    private static Dictionary<string, IReadOnlyList<T>> GroupByParent<T>(
        IReadOnlyList<T> items,
        Func<T, string> parentCodeSelector)
    {
        var groups = new Dictionary<string, List<T>>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var parentCode = parentCodeSelector(item);
            if (!groups.TryGetValue(parentCode, out var list))
            {
                list = [];
                groups[parentCode] = list;
            }

            list.Add(item);
        }

        var result = new Dictionary<string, IReadOnlyList<T>>(groups.Count, StringComparer.Ordinal);
        foreach (var (parentCode, list) in groups)
        {
            result[parentCode] = new ReadOnlyCollection<T>(list.ToArray());
        }

        return result;
    }
}
