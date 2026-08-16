namespace LankaLens.DataBuilder.Models;

/// <summary>
/// Expected GN counts by district and DS from the official DCS counts workbook.
/// </summary>
internal sealed record OfficialCountExpectation(
    string DistrictEnglish,
    string? DsEnglish,
    int GnCount,
    int? DsCount = null);

internal sealed record SourceConflict(
    string EntityType,
    string? EntityCode,
    string SourceA,
    string SourceB,
    string ConflictingField,
    string? ValueA,
    string? ValueB,
    string Message,
    string? SourceAId = null,
    string? SourceBId = null);
