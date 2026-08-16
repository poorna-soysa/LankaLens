namespace LankaLens.DataBuilder.Models;

/// <summary>
/// One GN-centric row as read from the DCS GNDList sheet before normalization.
/// </summary>
internal sealed record RawAdministrativeRecord
{
    public int SourceRowNumber { get; init; }

    public string? SerialNumber { get; init; }

    public string? GnUid { get; init; }

    public string? ProvinceCode { get; init; }

    public string? ProvinceEnglish { get; init; }

    public string? ProvinceSinhala { get; init; }

    public string? ProvinceTamil { get; init; }

    public string? DistrictCode { get; init; }

    public string? DistrictEnglish { get; init; }

    public string? DistrictSinhala { get; init; }

    public string? DistrictTamil { get; init; }

    public string? DsCode { get; init; }

    public string? DsEnglish { get; init; }

    public string? DsSinhala { get; init; }

    public string? DsTamil { get; init; }

    public string? GnCode { get; init; }

    public string? GnNumber { get; init; }

    public string? GnEnglish { get; init; }

    public string? GnSinhala { get; init; }

    public string? GnTamil { get; init; }

    public string? LgCode { get; init; }

    public string? LgName { get; init; }
}
