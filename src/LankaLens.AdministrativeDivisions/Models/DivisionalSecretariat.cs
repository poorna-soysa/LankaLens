namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// A divisional secretariat in the Sri Lankan administrative hierarchy.
/// Parent linkage uses <see cref="DistrictCode"/> rather than an embedded district object.
/// </summary>
/// <param name="Code">Authoritative divisional secretariat code. Must not be null, empty, or whitespace.</param>
/// <param name="DistrictCode">Code of the parent district. Must not be null, empty, or whitespace.</param>
/// <param name="Name">Multilingual divisional secretariat name.</param>
public sealed record DivisionalSecretariat(
    string Code,
    string DistrictCode,
    LocalizedName Name)
{
    /// <summary>
    /// Authoritative divisional secretariat code from the bundled administrative dataset.
    /// </summary>
    public string Code { get; } = ValidateRequired(Code, nameof(Code));

    /// <summary>
    /// Code of the parent district.
    /// </summary>
    public string DistrictCode { get; } = ValidateRequired(DistrictCode, nameof(DistrictCode));

    /// <summary>
    /// Multilingual divisional secretariat name.
    /// </summary>
    public LocalizedName Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));

    private static string ValidateRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }
}
