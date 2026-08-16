namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// Multilingual name for an administrative division.
/// English is always available. Sinhala and Tamil contain authoritative localized names when available.
/// A <see langword="null"/> Sinhala or Tamil value means no verified authoritative value is bundled;
/// consumers must not interpret <see langword="null"/> as an empty official name.
/// This type does not perform translation.
/// </summary>
/// <param name="English">English display name. Must not be null, empty, or whitespace.</param>
/// <param name="Sinhala">
/// Authoritative Sinhala display name when available; <see langword="null"/> when no verified
/// authoritative value is bundled. When present, must not be empty or whitespace.
/// </param>
/// <param name="Tamil">
/// Authoritative Tamil display name when available; <see langword="null"/> when no verified
/// authoritative value is bundled. When present, must not be empty or whitespace.
/// </param>
public sealed record LocalizedName(
    string English,
    string? Sinhala,
    string? Tamil)
{
    /// <summary>
    /// English display name. Always available.
    /// </summary>
    public string English { get; } = ValidateRequired(English, nameof(English));

    /// <summary>
    /// Authoritative Sinhala display name when available; otherwise <see langword="null"/>.
    /// </summary>
    public string? Sinhala { get; } = ValidateOptional(Sinhala, nameof(Sinhala));

    /// <summary>
    /// Authoritative Tamil display name when available; otherwise <see langword="null"/>.
    /// </summary>
    public string? Tamil { get; } = ValidateOptional(Tamil, nameof(Tamil));

    private static string ValidateRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }

    private static string? ValidateOptional(string? value, string paramName)
    {
        if (value is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{paramName} must not be empty or whitespace when provided; use null when no verified authoritative value is available.",
                paramName);
        }

        return value;
    }
}
