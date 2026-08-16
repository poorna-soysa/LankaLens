# LankaLens.AdministrativeDivisions

Sri Lanka administrative divisions for .NET — provinces, districts, divisional secretariats, and Grama Niladhari divisions with English, Sinhala, and Tamil names from authoritative government sources.

## What is LankaLens?

LankaLens is an open-source project that provides Sri Lanka’s official administrative hierarchy to .NET developers so applications do not need to manually maintain Province, District, Divisional Secretariat, and Grama Niladhari master data.

This repository publishes the NuGet package **`LankaLens.AdministrativeDivisions`**.

LankaLens is an independent open-source project and is **not** an official government product.

## Features

- Offline, read-only administrative division data (embedded in the assembly)
- Official DCS codes and hierarchy (no invented production IDs)
- English names on every record; Sinhala and Tamil when authoritative values are available
- Hierarchy navigation and multilingual search (exact / prefix / contains)
- No HTTP, telemetry, database, or DI requirement for normal use

## Current data coverage (bundled snapshot)

| Level | Count | English | Sinhala | Tamil |
|-------|------:|--------:|--------:|------:|
| Province | 9 | 9/9 | 9/9 | 9/9 |
| District | 25 | 25/25 | 25/25 | 25/25 |
| Divisional Secretariat | 340 | 340/340 | 340/340 | 340/340 |
| Grama Niladhari | 14,008 | 14,008/14,008 | **13,723/14,008** | **13,723/14,008** |

- Missing localized values are returned as `null` (never empty strings or placeholders).
- Translations are **not** machine-generated.
- Data comes from authoritative Sri Lankan government sources (DCS + MOHA LIFe + verified overlays). See [`docs/data-sources.md`](docs/data-sources.md).
- Do **not** claim 100% trilingual coverage for GN divisions for this snapshot.

> **Status:** Phase 4 production dataset is embedded. Package version remains pre-1.0 (`0.1.0`). Not yet published to NuGet.org.

## Installation

```bash
dotnet add package LankaLens.AdministrativeDivisions
```

Package publication to NuGet.org will follow after release-readiness review.

## Quick Start

```csharp
using LankaLens.AdministrativeDivisions;

var sriLanka = AdministrativeDivisions.Default;

Console.WriteLine($"Provinces: {sriLanka.GetProvinces().Count}");
Console.WriteLine($"Districts: {sriLanka.GetDistricts().Count}");

foreach (var province in sriLanka.GetProvinces())
{
    Console.WriteLine($"{province.Code}: {province.Name.English}");
    // Sinhala/Tamil may be null for some GN records; never for Province/District/DS in this snapshot.
    Console.WriteLine($"  Si: {province.Name.Sinhala ?? "(not available)"}");
}
```

## Provinces

```csharp
var provinces = sriLanka.GetProvinces();
var province = sriLanka.GetProvinceByCode(code);
sriLanka.TryGetProvince(code, out var found);
```

## Districts

```csharp
var districts = sriLanka.GetDistricts();
var byProvince = sriLanka.GetDistrictsByProvince(provinceCode);
var parent = sriLanka.GetProvinceForDistrict(districtCode);
```

## Divisional Secretariats

```csharp
var divisions = sriLanka.GetDivisionalSecretariats();
var byDistrict = sriLanka.GetDivisionalSecretariatsByDistrict(districtCode);
var parent = sriLanka.GetDistrictForDivisionalSecretariat(code);
```

## Grama Niladhari Divisions

```csharp
var divisions = sriLanka.GetGramaNiladhariDivisions();
var byParent = sriLanka.GetGramaNiladhariDivisionsByDivisionalSecretariat(dsCode);
var parent = sriLanka.GetDivisionalSecretariatForGramaNiladhariDivision(code);
```

## Multilingual Names

Every record has a mandatory English name. Sinhala and Tamil are nullable:

- Non-null values are verified authoritative localized names.
- `null` means no verified authoritative value is bundled for that language.
- Consumers must not interpret `null` as an empty official name.

## Search

```csharp
var results = sriLanka.Search(
    "Colombo",
    new AdministrativeDivisionSearchOptions
    {
        Language = Language.English,
        Type = AdministrativeDivisionType.District,
        MaxResults = 10
    });
```

Matching ranks exact hits first, then prefix, then contains. English matching is ordinal and case-insensitive; Sinhala and Tamil matching is ordinal. Language-specific search matches only that language (no automatic English fallback). Within equal rank, results are ordered by division type, then English name, then code.

## Dataset Source

- **Codes, hierarchy, English:** Department of Census and Statistics (DCS) Administrative Division Codes
- **Sinhala / Tamil:** Ministry of Home Affairs LIFe Location Codes, confirmed mappings, and verified authoritative overlays

LankaLens never invents geographic master data or machine-translates missing names for production use.

## Dataset Version

NuGet package version and dataset version are separate concepts. Dataset provenance is exposed via `IAdministrativeDivisionProvider.DatasetMetadata` and documented under [`docs/data-sources.md`](docs/data-sources.md).

## Accuracy and Data Policy

- Prefer DCS and other authoritative Sri Lankan government sources
- Do not use Wikipedia or Google Maps as authoritative data
- Do not silently correct official source values
- Code licensing (MIT) and source-data licensing are treated separately

## Contributing

See [`docs/contributing-data.md`](docs/contributing-data.md) for administrative-data correction requirements.

## Framework Support

- Current target: **.NET 8** (`net8.0`)
- Public API and implementation are kept portable enough that `netstandard2.0` could be added later if there is genuine demand

## License

Source code is licensed under the [MIT License](LICENSE).

Government source datasets may have separate redistribution terms; those will be verified before publishing redistributed data.

---

> LankaLens is an independent open-source project. Administrative data is sourced from authoritative public Sri Lankan government datasets. LankaLens is not affiliated with or endorsed by the Government of Sri Lanka.
