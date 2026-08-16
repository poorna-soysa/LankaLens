using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LankaLens.AdministrativeDivisions.Internal;

/// <summary>
/// Loads the production administrative dataset from the assembly embedded resource.
/// Responsible only for locate → open → deserialize → validate invariants → snapshot.
/// </summary>
internal static class EmbeddedAdministrativeDivisionLoader
{
    internal const string ResourceName = "LankaLens.AdministrativeDivisions.Data.administrative-divisions.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = false
    };

    public static AdministrativeDivisionSnapshot Load()
    {
        var assembly = typeof(EmbeddedAdministrativeDivisionLoader).Assembly;
        Stream? stream;
        try
        {
            stream = assembly.GetManifestResourceStream(ResourceName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"LankaLens package data failure: unable to open embedded resource '{ResourceName}'.",
                ex);
        }

        if (stream is null)
        {
            throw new InvalidOperationException(
                $"LankaLens package data failure: embedded resource '{ResourceName}' was not found in assembly '{assembly.GetName().Name}'.");
        }

        CanonicalDatasetDto dto;
        try
        {
            using (stream)
            {
                dto = JsonSerializer.Deserialize<CanonicalDatasetDto>(stream, Options)
                    ?? throw new InvalidOperationException(
                        "LankaLens package data failure: embedded administrative-divisions.json deserialized to null.");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "LankaLens package data failure: embedded administrative-divisions.json contains invalid JSON.",
                ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "LankaLens package data failure: unable to read embedded administrative-divisions.json.",
                ex);
        }

        try
        {
            return ToSnapshot(dto);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "LankaLens package data failure: embedded administrative-divisions.json violates required runtime invariants.",
                ex);
        }
    }

    private static AdministrativeDivisionSnapshot ToSnapshot(CanonicalDatasetDto dto)
    {
        if (dto.Metadata is null)
        {
            throw new InvalidOperationException(
                "LankaLens package data failure: metadata is missing from the embedded dataset.");
        }

        if (!DateOnly.TryParse(dto.Metadata.RetrievedDate, out var retrievedDate))
        {
            throw new InvalidOperationException(
                $"LankaLens package data failure: metadata.retrievedDate is invalid ('{dto.Metadata.RetrievedDate}').");
        }

        DateOnly? effectiveDate = null;
        if (!string.IsNullOrWhiteSpace(dto.Metadata.EffectiveDate))
        {
            if (!DateOnly.TryParse(dto.Metadata.EffectiveDate, out var parsed))
            {
                throw new InvalidOperationException(
                    $"LankaLens package data failure: metadata.effectiveDate is invalid ('{dto.Metadata.EffectiveDate}').");
            }

            effectiveDate = parsed;
        }

        var metadata = new DatasetMetadata(
            dto.Metadata.SourceOrganization,
            dto.Metadata.SourceName,
            dto.Metadata.SourceVersion,
            effectiveDate,
            retrievedDate);

        var provinces = MapProvinces(dto.Provinces);
        var districts = MapDistricts(dto.Districts);
        var divisionalSecretariats = MapDivisionalSecretariats(dto.DivisionalSecretariats);
        var gramaNiladhariDivisions = MapGramaNiladhariDivisions(dto.GramaNiladhariDivisions);

        if (provinces.Count == 0
            || districts.Count == 0
            || divisionalSecretariats.Count == 0
            || gramaNiladhariDivisions.Count == 0)
        {
            throw new InvalidOperationException(
                "LankaLens package data failure: embedded dataset has one or more empty hierarchy levels.");
        }

        ValidateUniqueCodes(provinces.Select(p => p.Code), "Province");
        ValidateUniqueCodes(districts.Select(d => d.Code), "District");
        ValidateUniqueCodes(divisionalSecretariats.Select(d => d.Code), "DivisionalSecretariat");
        ValidateUniqueCodes(gramaNiladhariDivisions.Select(g => g.Code), "GramaNiladhariDivision");

        var provinceCodes = provinces.Select(p => p.Code).ToHashSet(StringComparer.Ordinal);
        var districtCodes = districts.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);
        var dsCodes = divisionalSecretariats.Select(d => d.Code).ToHashSet(StringComparer.Ordinal);

        foreach (var district in districts)
        {
            if (!provinceCodes.Contains(district.ProvinceCode))
            {
                throw new InvalidOperationException(
                    $"LankaLens package data failure: district '{district.Code}' references missing province '{district.ProvinceCode}'.");
            }
        }

        foreach (var ds in divisionalSecretariats)
        {
            if (!districtCodes.Contains(ds.DistrictCode))
            {
                throw new InvalidOperationException(
                    $"LankaLens package data failure: divisional secretariat '{ds.Code}' references missing district '{ds.DistrictCode}'.");
            }
        }

        foreach (var gn in gramaNiladhariDivisions)
        {
            if (!dsCodes.Contains(gn.DivisionalSecretariatCode))
            {
                throw new InvalidOperationException(
                    $"LankaLens package data failure: Grama Niladhari division '{gn.Code}' references missing divisional secretariat '{gn.DivisionalSecretariatCode}'.");
            }
        }

        return new AdministrativeDivisionSnapshot(
            metadata,
            provinces,
            districts,
            divisionalSecretariats,
            gramaNiladhariDivisions);
    }

    private static IReadOnlyList<Province> MapProvinces(List<ProvinceDto>? items)
    {
        if (items is null || items.Count == 0)
        {
            return Array.Empty<Province>();
        }

        return new ReadOnlyCollection<Province>(
            items.Select(p =>
            {
                RejectDevelopmentCode(p.Code, "Province");
                return new Province(p.Code, MapName(p.Name, p.Code));
            }).ToList());
    }

    private static IReadOnlyList<District> MapDistricts(List<DistrictDto>? items)
    {
        if (items is null || items.Count == 0)
        {
            return Array.Empty<District>();
        }

        return new ReadOnlyCollection<District>(
            items.Select(d =>
            {
                RejectDevelopmentCode(d.Code, "District");
                return new District(d.Code, d.ProvinceCode, MapName(d.Name, d.Code));
            }).ToList());
    }

    private static IReadOnlyList<DivisionalSecretariat> MapDivisionalSecretariats(List<DivisionalSecretariatDto>? items)
    {
        if (items is null || items.Count == 0)
        {
            return Array.Empty<DivisionalSecretariat>();
        }

        return new ReadOnlyCollection<DivisionalSecretariat>(
            items.Select(d =>
            {
                RejectDevelopmentCode(d.Code, "DivisionalSecretariat");
                return new DivisionalSecretariat(d.Code, d.DistrictCode, MapName(d.Name, d.Code));
            }).ToList());
    }

    private static IReadOnlyList<GramaNiladhariDivision> MapGramaNiladhariDivisions(List<GramaNiladhariDivisionDto>? items)
    {
        if (items is null || items.Count == 0)
        {
            return Array.Empty<GramaNiladhariDivision>();
        }

        return new ReadOnlyCollection<GramaNiladhariDivision>(
            items.Select(g =>
            {
                RejectDevelopmentCode(g.Code, "GramaNiladhariDivision");
                return new GramaNiladhariDivision(g.Code, g.DivisionalSecretariatCode, MapName(g.Name, g.Code));
            }).ToList());
    }

    private static LocalizedName MapName(LocalizedNameDto? name, string code)
    {
        if (name is null || string.IsNullOrWhiteSpace(name.English))
        {
            throw new InvalidOperationException(
                $"LankaLens package data failure: entity '{code}' is missing a required English name.");
        }

        return new LocalizedName(
            name.English,
            NormalizeOptional(name.Sinhala, code, "Sinhala"),
            NormalizeOptional(name.Tamil, code, "Tamil"));
    }

    private static string? NormalizeOptional(string? value, string code, string language)
    {
        if (value is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"LankaLens package data failure: entity '{code}' has empty/whitespace {language}; use null when no verified value is bundled.");
        }

        return value;
    }

    private static void RejectDevelopmentCode(string code, string entityType)
    {
        if (code.StartsWith("DEV-", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"LankaLens package data failure: {entityType} code '{code}' is a development fixture code and must not appear in the production dataset.");
        }
    }

    private static void ValidateUniqueCodes(IEnumerable<string> codes, string entityType)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var code in codes)
        {
            if (!seen.Add(code))
            {
                throw new InvalidOperationException(
                    $"LankaLens package data failure: duplicate {entityType} code '{code}'.");
            }
        }
    }

    private sealed class CanonicalDatasetDto
    {
        public CanonicalMetadataDto? Metadata { get; set; }

        public List<ProvinceDto>? Provinces { get; set; }

        public List<DistrictDto>? Districts { get; set; }

        public List<DivisionalSecretariatDto>? DivisionalSecretariats { get; set; }

        public List<GramaNiladhariDivisionDto>? GramaNiladhariDivisions { get; set; }
    }

    private sealed class CanonicalMetadataDto
    {
        public string SourceOrganization { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public string? SourceVersion { get; set; }

        public string? EffectiveDate { get; set; }

        public string RetrievedDate { get; set; } = string.Empty;
    }

    private sealed class LocalizedNameDto
    {
        public string English { get; set; } = string.Empty;

        public string? Sinhala { get; set; }

        public string? Tamil { get; set; }
    }

    private sealed class ProvinceDto
    {
        public string Code { get; set; } = string.Empty;

        public LocalizedNameDto? Name { get; set; }
    }

    private sealed class DistrictDto
    {
        public string Code { get; set; } = string.Empty;

        public string ProvinceCode { get; set; } = string.Empty;

        public LocalizedNameDto? Name { get; set; }
    }

    private sealed class DivisionalSecretariatDto
    {
        public string Code { get; set; } = string.Empty;

        public string DistrictCode { get; set; } = string.Empty;

        public LocalizedNameDto? Name { get; set; }
    }

    private sealed class GramaNiladhariDivisionDto
    {
        public string Code { get; set; } = string.Empty;

        public string DivisionalSecretariatCode { get; set; } = string.Empty;

        public LocalizedNameDto? Name { get; set; }
    }
}
