using System.Diagnostics.CodeAnalysis;

namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// Read-only access to the bundled Sri Lankan administrative division dataset.
/// Lookups are in-memory; methods are synchronous because no network or database I/O is performed.
/// </summary>
/// <remarks>
/// <para>
/// Code lookups use ordinal, case-sensitive comparison. Required string arguments are not trimmed.
/// Null arguments throw <see cref="ArgumentNullException"/>. Empty or whitespace-only arguments throw
/// <see cref="ArgumentException"/>. A non-empty code that differs only by surrounding spaces does not match
/// and is treated as unknown (lookup returns <see langword="null"/> / <see langword="false"/>).
/// </para>
/// <para>
/// Implementations returned by <see cref="AdministrativeDivisions.Default"/> are immutable after
/// construction and safe for concurrent reads from multiple threads. Search and hierarchy methods
/// allocate per call and do not mutate shared state.
/// </para>
/// </remarks>
public interface IAdministrativeDivisionProvider
{
    /// <summary>
    /// Provenance of the administrative-data snapshot currently served by this provider.
    /// </summary>
    DatasetMetadata DatasetMetadata { get; }

    /// <summary>
    /// Returns all provinces in the bundled dataset as an immutable sequence.
    /// </summary>
    /// <returns>All provinces; never <see langword="null"/>.</returns>
    IReadOnlyList<Province> GetProvinces();

    /// <summary>
    /// Looks up a province by its authoritative code.
    /// </summary>
    /// <param name="code">Province code. Must not be null, empty, or whitespace. Compared using ordinal, case-sensitive equality; not trimmed.</param>
    /// <returns>The matching province, or <see langword="null"/> when the code is unknown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    Province? GetProvinceByCode(string code);

    /// <summary>
    /// Attempts to look up a province by its authoritative code.
    /// </summary>
    /// <param name="code">Province code. Must not be null, empty, or whitespace.</param>
    /// <param name="province">When this method returns <see langword="true"/>, the matching province; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    bool TryGetProvince(
        string code,
        [NotNullWhen(true)] out Province? province);

    /// <summary>
    /// Returns all districts in the bundled dataset as an immutable sequence.
    /// </summary>
    /// <returns>All districts; never <see langword="null"/>.</returns>
    IReadOnlyList<District> GetDistricts();

    /// <summary>
    /// Looks up a district by its authoritative code.
    /// </summary>
    /// <param name="code">District code. Must not be null, empty, or whitespace.</param>
    /// <returns>The matching district, or <see langword="null"/> when the code is unknown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    District? GetDistrictByCode(string code);

    /// <summary>
    /// Attempts to look up a district by its authoritative code.
    /// </summary>
    /// <param name="code">District code. Must not be null, empty, or whitespace.</param>
    /// <param name="district">When this method returns <see langword="true"/>, the matching district; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    bool TryGetDistrict(
        string code,
        [NotNullWhen(true)] out District? district);

    /// <summary>
    /// Returns districts that belong to the specified province.
    /// An unknown but syntactically valid province code yields an empty sequence.
    /// </summary>
    /// <param name="provinceCode">Parent province code. Must not be null, empty, or whitespace.</param>
    /// <returns>Matching districts; never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="provinceCode"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="provinceCode"/> is empty or whitespace.</exception>
    IReadOnlyList<District> GetDistrictsByProvince(string provinceCode);

    /// <summary>
    /// Returns all divisional secretariats in the bundled dataset as an immutable sequence.
    /// </summary>
    /// <returns>All divisional secretariats; never <see langword="null"/>.</returns>
    IReadOnlyList<DivisionalSecretariat> GetDivisionalSecretariats();

    /// <summary>
    /// Looks up a divisional secretariat by its authoritative code.
    /// </summary>
    /// <param name="code">Divisional secretariat code. Must not be null, empty, or whitespace.</param>
    /// <returns>The matching divisional secretariat, or <see langword="null"/> when the code is unknown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    DivisionalSecretariat? GetDivisionalSecretariatByCode(string code);

    /// <summary>
    /// Attempts to look up a divisional secretariat by its authoritative code.
    /// </summary>
    /// <param name="code">Divisional secretariat code. Must not be null, empty, or whitespace.</param>
    /// <param name="divisionalSecretariat">When this method returns <see langword="true"/>, the matching division; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    bool TryGetDivisionalSecretariat(
        string code,
        [NotNullWhen(true)] out DivisionalSecretariat? divisionalSecretariat);

    /// <summary>
    /// Returns divisional secretariats that belong to the specified district.
    /// An unknown but syntactically valid district code yields an empty sequence.
    /// </summary>
    /// <param name="districtCode">Parent district code. Must not be null, empty, or whitespace.</param>
    /// <returns>Matching divisional secretariats; never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="districtCode"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="districtCode"/> is empty or whitespace.</exception>
    IReadOnlyList<DivisionalSecretariat> GetDivisionalSecretariatsByDistrict(string districtCode);

    /// <summary>
    /// Returns all Grama Niladhari divisions in the bundled dataset as an immutable sequence.
    /// </summary>
    /// <returns>All Grama Niladhari divisions; never <see langword="null"/>.</returns>
    IReadOnlyList<GramaNiladhariDivision> GetGramaNiladhariDivisions();

    /// <summary>
    /// Looks up a Grama Niladhari division by its authoritative code.
    /// </summary>
    /// <param name="code">Grama Niladhari division code. Must not be null, empty, or whitespace.</param>
    /// <returns>The matching division, or <see langword="null"/> when the code is unknown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    GramaNiladhariDivision? GetGramaNiladhariDivisionByCode(string code);

    /// <summary>
    /// Attempts to look up a Grama Niladhari division by its authoritative code.
    /// </summary>
    /// <param name="code">Grama Niladhari division code. Must not be null, empty, or whitespace.</param>
    /// <param name="division">When this method returns <see langword="true"/>, the matching division; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="code"/> is empty or whitespace.</exception>
    bool TryGetGramaNiladhariDivision(
        string code,
        [NotNullWhen(true)] out GramaNiladhariDivision? division);

    /// <summary>
    /// Returns Grama Niladhari divisions that belong to the specified divisional secretariat.
    /// An unknown but syntactically valid parent code yields an empty sequence.
    /// </summary>
    /// <param name="divisionalSecretariatCode">Parent divisional secretariat code. Must not be null, empty, or whitespace.</param>
    /// <returns>Matching Grama Niladhari divisions; never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="divisionalSecretariatCode"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="divisionalSecretariatCode"/> is empty or whitespace.</exception>
    IReadOnlyList<GramaNiladhariDivision> GetGramaNiladhariDivisionsByDivisionalSecretariat(
        string divisionalSecretariatCode);

    /// <summary>
    /// Resolves the parent province for a district code.
    /// </summary>
    /// <param name="districtCode">District code. Must not be null, empty, or whitespace.</param>
    /// <returns>The parent province, or <see langword="null"/> when the district code is unknown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="districtCode"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="districtCode"/> is empty or whitespace.</exception>
    Province? GetProvinceForDistrict(string districtCode);

    /// <summary>
    /// Resolves the parent district for a divisional secretariat code.
    /// </summary>
    /// <param name="divisionalSecretariatCode">Divisional secretariat code. Must not be null, empty, or whitespace.</param>
    /// <returns>The parent district, or <see langword="null"/> when the code is unknown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="divisionalSecretariatCode"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="divisionalSecretariatCode"/> is empty or whitespace.</exception>
    District? GetDistrictForDivisionalSecretariat(string divisionalSecretariatCode);

    /// <summary>
    /// Resolves the parent divisional secretariat for a Grama Niladhari division code.
    /// </summary>
    /// <param name="gramaNiladhariDivisionCode">Grama Niladhari division code. Must not be null, empty, or whitespace.</param>
    /// <returns>The parent divisional secretariat, or <see langword="null"/> when the code is unknown.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="gramaNiladhariDivisionCode"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="gramaNiladhariDivisionCode"/> is empty or whitespace.</exception>
    DivisionalSecretariat? GetDivisionalSecretariatForGramaNiladhariDivision(
        string gramaNiladhariDivisionCode);

    /// <summary>
    /// Searches division names using exact, then prefix, then contains matching.
    /// English matching is ordinal and case-insensitive; Sinhala and Tamil matching is ordinal and case-sensitive.
    /// Language-specific search matches only that language field; records with a <see langword="null"/>
    /// Sinhala or Tamil name are not candidates for that language and there is no automatic English fallback.
    /// Results are deterministic: within equal match rank, ordered by <see cref="AdministrativeDivisionType"/>,
    /// then English name, then code.
    /// </summary>
    /// <param name="query">Search text. Must not be null, empty, or whitespace.</param>
    /// <param name="options">Optional filters for language, type, and maximum result count.</param>
    /// <returns>Ranked search results; never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="query"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="options"/>.<see cref="AdministrativeDivisionSearchOptions.MaxResults"/> is zero or negative.</exception>
    IReadOnlyList<AdministrativeDivisionSearchResult> Search(
        string query,
        AdministrativeDivisionSearchOptions? options = null);
}
