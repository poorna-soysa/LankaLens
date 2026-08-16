namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// A province in the Sri Lankan administrative hierarchy.
/// <see cref="Code"/> is the authoritative administrative code from the bundled dataset.
/// </summary>
/// <param name="Code">Authoritative province code. Must not be null, empty, or whitespace.</param>
/// <param name="Name">Multilingual province name.</param>
public sealed record Province(
    string Code,
    LocalizedName Name)
{
    /// <summary>
    /// Authoritative province code from the bundled administrative dataset.
    /// </summary>
    public string Code { get; } = ValidateRequired(Code, nameof(Code));

    /// <summary>
    /// Multilingual province name.
    /// </summary>
    public LocalizedName Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));

    private static string ValidateRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }
}
