# LankaLens.AdministrativeDivisions

A .NET library providing Sri Lanka's administrative divisions with authoritative codes, hierarchy, and multilingual names.

LankaLens is an **independent open-source project** and is **not** an official government package. It is not affiliated with or endorsed by the Government of Sri Lanka.

## Hierarchy

```text
Country (Sri Lanka)
  → Province
    → District
      → Divisional Secretariat
        → Grama Niladhari Division
```

## Installation

```bash
dotnet add package LankaLens.AdministrativeDivisions --version 0.1.0-preview.1
```

## Quick start

```csharp
using LankaLens.AdministrativeDivisions;

var sriLanka = AdministrativeDivisions.Default;

foreach (var province in sriLanka.GetProvinces())
{
    Console.WriteLine(province.Name.English);
}
```

### Hierarchy example

```csharp
var districts = sriLanka.GetDistrictsByProvince("1"); // Western
var dsList = sriLanka.GetDivisionalSecretariatsByDistrict("11"); // Colombo
var gnList = sriLanka.GetGramaNiladhariDivisionsByDivisionalSecretariat("1103");
var parent = sriLanka.GetProvinceForDistrict("11");
```

## Multilingual names

```csharp
var province = sriLanka.GetProvinceByCode("1"); // Western
Console.WriteLine(province!.Name.English); // Western
Console.WriteLine(province.Name.Sinhala);  // බස්නාහිර
Console.WriteLine(province.Name.Tamil);    // மேற்கு
```

English is always present. Sinhala and Tamil may be `null` on some Grama Niladhari records.

- `null` means **no verified authoritative localized value is currently bundled**.
- `null` is **not** a translation failure and is never an empty placeholder string.
- This snapshot: Province / District / DS Sinhala+Tamil are complete; GN is 13,723 / 14,008.

## Search

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

Matching ranks **exact**, then **prefix**, then **contains**. English matching is ordinal and case-insensitive; Sinhala and Tamil matching is ordinal. Language-specific search matches only that language (no English fallback).

## Common operations

| Need | API |
|------|-----|
| All provinces | `GetProvinces()` |
| Districts by province | `GetDistrictsByProvince(code)` |
| DS by district | `GetDivisionalSecretariatsByDistrict(code)` |
| GN by DS | `GetGramaNiladhariDivisionsByDivisionalSecretariat(code)` |
| Lookup by code | `Get*ByCode` / `TryGet*` |
| Parent navigation | `GetProvinceForDistrict`, `GetDistrictForDivisionalSecretariat`, `GetDivisionalSecretariatForGramaNiladhariDivision` |

## Data coverage (bundled snapshot)

| Level | Count | English | Sinhala | Tamil |
|-------|------:|--------:|--------:|------:|
| Province | 9 | 9/9 | 9/9 | 9/9 |
| District | 25 | 25/25 | 25/25 | 25/25 |
| Divisional Secretariat | 340 | 340/340 | 340/340 | 340/340 |
| Grama Niladhari | 14,008 | 14,008/14,008 | 13,723/14,008 | 13,723/14,008 |

## Data sources

- **Department of Census and Statistics, Sri Lanka** — canonical hierarchy, codes, and English names
- **Ministry of Home Affairs, Sri Lanka** — Sinhala/Tamil via LIFe Location Codes and verified official evidence

Detailed provenance: [docs/data-sources.md](https://github.com/poorna-soysa/LankaLens/blob/main/docs/data-sources.md)

## License

Source code is licensed under the MIT License.

Data-source information: [`DATA-NOTICE.md`](https://github.com/poorna-soysa/LankaLens/blob/main/DATA-NOTICE.md).

> LankaLens is an independent open-source project and is not affiliated with or endorsed by the Government of Sri Lanka. Administrative data is derived from cited official government sources.
