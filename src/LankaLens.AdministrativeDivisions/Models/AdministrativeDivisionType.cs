namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// Level within the Sri Lankan administrative hierarchy.
/// </summary>
public enum AdministrativeDivisionType
{
    /// <summary>
    /// First-level administrative division (province).
    /// </summary>
    Province = 0,

    /// <summary>
    /// Second-level administrative division (district).
    /// </summary>
    District = 1,

    /// <summary>
    /// Third-level administrative division (divisional secretariat).
    /// </summary>
    DivisionalSecretariat = 2,

    /// <summary>
    /// Fourth-level administrative division (Grama Niladhari division).
    /// </summary>
    GramaNiladhariDivision = 3
}
