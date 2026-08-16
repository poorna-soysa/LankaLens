namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// Optional filters for <see cref="IAdministrativeDivisionProvider.Search"/>.
/// When omitted (<see langword="null"/> options), search matches all division types and all three languages with no result cap.
/// </summary>
public sealed record AdministrativeDivisionSearchOptions
{
    /// <summary>
    /// When set, only the corresponding localized name field is matched.
    /// Records whose selected language field is <see langword="null"/> are not candidates;
    /// there is no automatic fallback to English.
    /// When <see langword="null"/>, all non-null localized name fields are considered and the best rank is kept.
    /// </summary>
    public Language? Language { get; init; }

    /// <summary>
    /// When set, only divisions of this hierarchy level are returned.
    /// When <see langword="null"/>, all levels are searched.
    /// </summary>
    public AdministrativeDivisionType? Type { get; init; }

    /// <summary>
    /// Maximum number of results to return after ranking and sorting.
    /// When set, must be greater than zero; zero or negative values cause <see cref="ArgumentOutOfRangeException"/>.
    /// When <see langword="null"/>, all matches are returned.
    /// </summary>
    public int? MaxResults { get; init; }
}
