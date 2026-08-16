namespace LankaLens.DataBuilder.Models;

/// <summary>
/// One GN row as published by the MOHA LIFe official GN report, before DCS join.
/// <see cref="LifeCode"/> is the original hyphenated value; <see cref="NormalizedLifeCode"/>
/// is the structure-validated join key.
/// </summary>
internal sealed record RawMohaGnRecord
{
    public required string LifeCode { get; init; }

    public required string NormalizedLifeCode { get; init; }

    public required string ProvinceComponent { get; init; }

    public required string DistrictComponent { get; init; }

    public required string DsComponent { get; init; }

    public required string GnComponent { get; init; }

    public required string HierarchicalProvinceCode { get; init; }

    public required string HierarchicalDistrictCode { get; init; }

    public required string HierarchicalDsCode { get; init; }

    public string? EnglishName { get; init; }

    public string? SinhalaName { get; init; }

    public string? TamilName { get; init; }

    public string? MpaCode { get; init; }

    public string? ProvinceEnglish { get; init; }

    public string? ProvinceSinhala { get; init; }

    public string? ProvinceTamil { get; init; }

    public string? DistrictEnglish { get; init; }

    public string? DistrictSinhala { get; init; }

    public string? DistrictTamil { get; init; }

    public string? DsEnglish { get; init; }

    public string? DsSinhala { get; init; }

    public string? DsTamil { get; init; }

    public string? ProvinceLabelPrefix { get; init; }

    public string? DistrictLabelPrefix { get; init; }

    public string? DsLabelPrefix { get; init; }

    public string? SourceReportFile { get; init; }

    public int? SourceRowNumber { get; init; }
}

internal sealed record InvalidLifeCode(
    string RawValue,
    string Reason,
    string? SourceReportFile = null,
    int? SourceRowNumber = null);

internal sealed record MohaParseResult(
    IReadOnlyList<RawMohaGnRecord> Records,
    IReadOnlyList<InvalidLifeCode> InvalidCodes);
