namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// A district in the Sri Lankan administrative hierarchy.
/// Parent linkage uses <see cref="ProvinceCode"/> rather than an embedded province object.
/// </summary>
/// <param name="Code">Authoritative district code. Must not be null, empty, or whitespace.</param>
/// <param name="ProvinceCode">Code of the parent province. Must not be null, empty, or whitespace.</param>
/// <param name="Name">Multilingual district name.</param>
public sealed record District(
    string Code,
    string ProvinceCode,
    LocalizedName Name)
{
    /// <summary>
    /// Authoritative district code from the bundled administrative dataset.
    /// </summary>
    public string Code { get; } = ValidateRequired(Code, nameof(Code));

    /// <summary>
    /// Code of the parent province.
    /// </summary>
    public string ProvinceCode { get; } = ValidateRequired(ProvinceCode, nameof(ProvinceCode));

    /// <summary>
    /// Multilingual district name.
    /// </summary>
    public LocalizedName Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));

    private static string ValidateRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }
}
