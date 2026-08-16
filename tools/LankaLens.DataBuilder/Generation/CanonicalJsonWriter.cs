using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LankaLens.DataBuilder.Models;

namespace LankaLens.DataBuilder.Generation;

internal static class CanonicalJsonWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize(CanonicalDataset dataset)
    {
        var dto = new CanonicalDatasetDto
        {
            Metadata = new CanonicalMetadataDto
            {
                SourceOrganization = dataset.Metadata.SourceOrganization,
                SourceName = dataset.Metadata.SourceName,
                SourceVersion = dataset.Metadata.SourceVersion,
                EffectiveDate = dataset.Metadata.EffectiveDate?.ToString("yyyy-MM-dd"),
                RetrievedDate = dataset.Metadata.RetrievedDate.ToString("yyyy-MM-dd")
            },
            Provinces = dataset.Provinces.Select(MapProvince).ToList(),
            Districts = dataset.Districts.Select(MapDistrict).ToList(),
            DivisionalSecretariats = dataset.DivisionalSecretariats.Select(MapDs).ToList(),
            GramaNiladhariDivisions = dataset.GramaNiladhariDivisions.Select(MapGn).ToList()
        };

        // Pin LF so Windows and Linux builds produce byte-identical output.
        return JsonSerializer.Serialize(dto, Options).Replace("\r\n", "\n") + "\n";
    }

    public static void Write(CanonicalDataset dataset, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var json = Serialize(dataset);
        File.WriteAllText(outputPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public static CanonicalDataset Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<CanonicalDatasetDto>(json, Options)
            ?? throw new InvalidOperationException("Canonical JSON deserialized to null.");

        if (!DateOnly.TryParse(dto.Metadata.RetrievedDate, out var retrievedDate))
        {
            throw new InvalidOperationException(
                $"Canonical JSON metadata.retrievedDate is invalid: '{dto.Metadata.RetrievedDate}'.");
        }

        DateOnly? effectiveDate = null;
        if (!string.IsNullOrWhiteSpace(dto.Metadata.EffectiveDate))
        {
            if (!DateOnly.TryParse(dto.Metadata.EffectiveDate, out var parsedEffective))
            {
                throw new InvalidOperationException(
                    $"Canonical JSON metadata.effectiveDate is invalid: '{dto.Metadata.EffectiveDate}'.");
            }

            effectiveDate = parsedEffective;
        }

        var metadata = new CanonicalDatasetMetadata(
            dto.Metadata.SourceOrganization,
            dto.Metadata.SourceName,
            dto.Metadata.SourceVersion,
            effectiveDate,
            retrievedDate);

        return new CanonicalDataset(
            metadata,
            dto.Provinces.Select(p => new CanonicalProvince(p.Code, MapName(p.Name))).ToList(),
            dto.Districts.Select(d => new CanonicalDistrict(d.Code, d.ProvinceCode, MapName(d.Name))).ToList(),
            dto.DivisionalSecretariats.Select(d => new CanonicalDivisionalSecretariat(d.Code, d.DistrictCode, MapName(d.Name))).ToList(),
            dto.GramaNiladhariDivisions.Select(g => new CanonicalGramaNiladhariDivision(g.Code, g.DivisionalSecretariatCode, MapName(g.Name))).ToList());
    }

    private static ProvinceDto MapProvince(CanonicalProvince province) => new()
    {
        Code = province.Code,
        Name = MapName(province.Name)
    };

    private static DistrictDto MapDistrict(CanonicalDistrict district) => new()
    {
        Code = district.Code,
        ProvinceCode = district.ProvinceCode,
        Name = MapName(district.Name)
    };

    private static DivisionalSecretariatDto MapDs(CanonicalDivisionalSecretariat ds) => new()
    {
        Code = ds.Code,
        DistrictCode = ds.DistrictCode,
        Name = MapName(ds.Name)
    };

    private static GramaNiladhariDivisionDto MapGn(CanonicalGramaNiladhariDivision gn) => new()
    {
        Code = gn.Code,
        DivisionalSecretariatCode = gn.DivisionalSecretariatCode,
        Name = MapName(gn.Name)
    };

    private static LocalizedNameDto MapName(CanonicalLocalizedName name) => new()
    {
        English = name.English ?? string.Empty,
        Sinhala = NormalizeOptional(name.Sinhala),
        Tamil = NormalizeOptional(name.Tamil)
    };

    private static CanonicalLocalizedName MapName(LocalizedNameDto name) =>
        new(
            name.English,
            NormalizeOptional(name.Sinhala),
            NormalizeOptional(name.Tamil));

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed class CanonicalDatasetDto
    {
        public CanonicalMetadataDto Metadata { get; set; } = new();

        public List<ProvinceDto> Provinces { get; set; } = [];

        public List<DistrictDto> Districts { get; set; } = [];

        public List<DivisionalSecretariatDto> DivisionalSecretariats { get; set; } = [];

        public List<GramaNiladhariDivisionDto> GramaNiladhariDivisions { get; set; } = [];
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

        public LocalizedNameDto Name { get; set; } = new();
    }

    private sealed class DistrictDto
    {
        public string Code { get; set; } = string.Empty;

        public string ProvinceCode { get; set; } = string.Empty;

        public LocalizedNameDto Name { get; set; } = new();
    }

    private sealed class DivisionalSecretariatDto
    {
        public string Code { get; set; } = string.Empty;

        public string DistrictCode { get; set; } = string.Empty;

        public LocalizedNameDto Name { get; set; } = new();
    }

    private sealed class GramaNiladhariDivisionDto
    {
        public string Code { get; set; } = string.Empty;

        public string DivisionalSecretariatCode { get; set; } = string.Empty;

        public LocalizedNameDto Name { get; set; } = new();
    }
}
