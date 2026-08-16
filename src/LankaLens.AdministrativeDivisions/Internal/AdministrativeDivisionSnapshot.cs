namespace LankaLens.AdministrativeDivisions.Internal;

/// <summary>
/// Immutable in-memory snapshot of administrative divisions used by the provider.
/// </summary>
internal sealed class AdministrativeDivisionSnapshot
{
    public AdministrativeDivisionSnapshot(
        DatasetMetadata metadata,
        IReadOnlyList<Province> provinces,
        IReadOnlyList<District> districts,
        IReadOnlyList<DivisionalSecretariat> divisionalSecretariats,
        IReadOnlyList<GramaNiladhariDivision> gramaNiladhariDivisions)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Provinces = provinces ?? throw new ArgumentNullException(nameof(provinces));
        Districts = districts ?? throw new ArgumentNullException(nameof(districts));
        DivisionalSecretariats = divisionalSecretariats
            ?? throw new ArgumentNullException(nameof(divisionalSecretariats));
        GramaNiladhariDivisions = gramaNiladhariDivisions
            ?? throw new ArgumentNullException(nameof(gramaNiladhariDivisions));
    }

    public DatasetMetadata Metadata { get; }

    public IReadOnlyList<Province> Provinces { get; }

    public IReadOnlyList<District> Districts { get; }

    public IReadOnlyList<DivisionalSecretariat> DivisionalSecretariats { get; }

    public IReadOnlyList<GramaNiladhariDivision> GramaNiladhariDivisions { get; }
}
