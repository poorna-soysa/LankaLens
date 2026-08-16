namespace LankaLens.AdministrativeDivisions;

/// <summary>
/// Provenance for the administrative-data snapshot bundled with this package.
/// Identifies the source organization and retrieval details so consumers know which dataset is in use.
/// </summary>
/// <param name="SourceOrganization">Organization that published the source data.</param>
/// <param name="SourceName">Human-readable name of the source dataset.</param>
/// <param name="SourceVersion">Optional version or edition identifier from the source, when available.</param>
/// <param name="EffectiveDate">Optional date on which the source data became effective, when known.</param>
/// <param name="RetrievedDate">Date on which the source snapshot was retrieved for bundling.</param>
public sealed record DatasetMetadata(
    string SourceOrganization,
    string SourceName,
    string? SourceVersion,
    DateOnly? EffectiveDate,
    DateOnly RetrievedDate)
{
    /// <summary>
    /// Organization that published the source data.
    /// </summary>
    public string SourceOrganization { get; } = ValidateRequired(SourceOrganization, nameof(SourceOrganization));

    /// <summary>
    /// Human-readable name of the source dataset.
    /// </summary>
    public string SourceName { get; } = ValidateRequired(SourceName, nameof(SourceName));

    /// <summary>
    /// Optional version or edition identifier from the source, when available.
    /// </summary>
    public string? SourceVersion { get; } = SourceVersion;

    /// <summary>
    /// Optional date on which the source data became effective, when known.
    /// </summary>
    public DateOnly? EffectiveDate { get; } = EffectiveDate;

    /// <summary>
    /// Date on which the source snapshot was retrieved for bundling.
    /// This is not a build timestamp.
    /// </summary>
    public DateOnly RetrievedDate { get; } = RetrievedDate;

    private static string ValidateRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }
}
