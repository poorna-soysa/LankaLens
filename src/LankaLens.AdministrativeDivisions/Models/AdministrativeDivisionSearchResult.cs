namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// A single hit from <see cref="IAdministrativeDivisionProvider.Search"/>.
/// Parent code properties are ancestors only and never duplicate <see cref="Code"/>.
/// </summary>
/// <param name="Code">Code of the matched division.</param>
/// <param name="Type">Hierarchy level of the matched division.</param>
/// <param name="Name">Multilingual name of the matched division.</param>
/// <param name="ProvinceCode">Parent province code when the match is below province level; otherwise <see langword="null"/>.</param>
/// <param name="DistrictCode">Parent district code when the match is below district level; otherwise <see langword="null"/>.</param>
/// <param name="DivisionalSecretariatCode">Parent divisional secretariat code when the match is a Grama Niladhari division; otherwise <see langword="null"/>.</param>
public sealed record AdministrativeDivisionSearchResult(
    string Code,
    AdministrativeDivisionType Type,
    LocalizedName Name,
    string? ProvinceCode,
    string? DistrictCode,
    string? DivisionalSecretariatCode)
{
    /// <summary>
    /// Code of the matched division.
    /// </summary>
    public string Code { get; } = Code ?? throw new ArgumentNullException(nameof(Code));

    /// <summary>
    /// Hierarchy level of the matched division.
    /// </summary>
    public AdministrativeDivisionType Type { get; } = Type;

    /// <summary>
    /// Multilingual name of the matched division.
    /// </summary>
    public LocalizedName Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));

    /// <summary>
    /// Parent province code when the match is below province level; otherwise <see langword="null"/>.
    /// </summary>
    public string? ProvinceCode { get; } = ProvinceCode;

    /// <summary>
    /// Parent district code when the match is below district level; otherwise <see langword="null"/>.
    /// </summary>
    public string? DistrictCode { get; } = DistrictCode;

    /// <summary>
    /// Parent divisional secretariat code when the match is a Grama Niladhari division; otherwise <see langword="null"/>.
    /// </summary>
    public string? DivisionalSecretariatCode { get; } = DivisionalSecretariatCode;
}
