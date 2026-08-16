# LankaLens.AdministrativeDivisions

A .NET library providing Sri Lanka's administrative divisions with authoritative codes, hierarchy, and multilingual names.

LankaLens is an independent open-source project and is **not** an official government product. It is not affiliated with or endorsed by the Government of Sri Lanka.

## Hierarchy

```text
Country (Sri Lanka)
  → Province
    → District
      → Divisional Secretariat
        → Grama Niladhari Division
```

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
- `null` means no verified authoritative localized value is currently bundled — not a translation failure.
- Translations are **not** machine-generated.
- Data comes from authoritative Sri Lankan government sources (DCS + MOHA LIFe + verified overlays). See [`docs/data-sources.md`](docs/data-sources.md).

> **Status:** First public NuGet version planned: **`0.1.0-preview.1`** (not yet published to NuGet.org). Data sources and provenance: [`DATA-NOTICE.md`](DATA-NOTICE.md).

## Installation

```bash
dotnet add package LankaLens.AdministrativeDivisions --version 0.1.0-preview.1
```

(Use after the package is published to NuGet.org, or install from a local `artifacts/packages` feed during development.)

## Quick start

```csharp
using LankaLens.AdministrativeDivisions;

var sriLanka = AdministrativeDivisions.Default;

foreach (var province in sriLanka.GetProvinces())
{
    Console.WriteLine(province.Name.English);
}
```

### Hierarchy

```csharp
var districts = sriLanka.GetDistrictsByProvince("1"); // Western
var dsList = sriLanka.GetDivisionalSecretariatsByDistrict("11"); // Colombo
var gnList = sriLanka.GetGramaNiladhariDivisionsByDivisionalSecretariat("1103");
var parent = sriLanka.GetProvinceForDistrict("11");
```

### Multilingual names

```csharp
var province = sriLanka.GetProvinceByCode("1"); // Western
Console.WriteLine(province!.Name.English); // Western
Console.WriteLine(province.Name.Sinhala);  // බස්නාහිර
Console.WriteLine(province.Name.Tamil);    // மேற்கு
```

English is always present. Sinhala and Tamil may be `null` for some Grama Niladhari records.

### Search

```csharp
var results = sriLanka.Search("Colombo");

var sinhala = sriLanka.Search(
    "බස්නාහිර",
    new AdministrativeDivisionSearchOptions
    {
        Language = Language.Sinhala,
        Type = AdministrativeDivisionType.Province,
        MaxResults = 5
    });
```

Matching ranks exact hits first, then prefix, then contains. English matching is ordinal and case-insensitive; Sinhala and Tamil matching is ordinal. Language-specific search matches only that language (no automatic English fallback).

## Dataset source

- **Codes, hierarchy, English:** Department of Census and Statistics (DCS), Sri Lanka
- **Sinhala / Tamil:** Ministry of Home Affairs, Sri Lanka (LIFe Location Codes), confirmed mappings, and verified authoritative overlays

LankaLens never invents geographic master data or machine-translates missing names for production use.

Full provenance: [`docs/data-sources.md`](docs/data-sources.md).

## Framework support

- Current target: **.NET 8** (`net8.0`)
- Kept on a modern TFM intentionally; `netstandard2.0` is not planned unless demand justifies compatibility packages or API compromises.

## License

Source code is licensed under the [MIT License](LICENSE).

Data-source information and the software-vs-data boundary: [`DATA-NOTICE.md`](DATA-NOTICE.md).

## Contributing

See [`docs/contributing-data.md`](docs/contributing-data.md) for administrative-data correction requirements. Public API notes: [`docs/api-review.md`](docs/api-review.md).

---

> LankaLens is an independent open-source project and is not affiliated with or endorsed by the Government of Sri Lanka. Administrative data is derived from cited official government sources.
