namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// A Grama Niladhari division in the Sri Lankan administrative hierarchy.
/// Parent linkage uses <see cref="DivisionalSecretariatCode"/> rather than an embedded parent object.
/// </summary>
/// <param name="Code">Authoritative Grama Niladhari division code. Must not be null, empty, or whitespace.</param>
/// <param name="DivisionalSecretariatCode">Code of the parent divisional secretariat. Must not be null, empty, or whitespace.</param>
/// <param name="Name">Multilingual Grama Niladhari division name.</param>
public sealed record GramaNiladhariDivision(
    string Code,
    string DivisionalSecretariatCode,
    LocalizedName Name)
{
    /// <summary>
    /// Authoritative Grama Niladhari division code from the bundled administrative dataset.
    /// </summary>
    public string Code { get; } = ValidateRequired(Code, nameof(Code));

    /// <summary>
    /// Code of the parent divisional secretariat.
    /// </summary>
    public string DivisionalSecretariatCode { get; } = ValidateRequired(
        DivisionalSecretariatCode,
        nameof(DivisionalSecretariatCode));

    /// <summary>
    /// Multilingual Grama Niladhari division name.
    /// </summary>
    public LocalizedName Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));

    private static string ValidateRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }
}
