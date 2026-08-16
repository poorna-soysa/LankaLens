# DATA-NOTICE — Administrative data provenance

**Package:** `LankaLens.AdministrativeDivisions`

This notice concerns administrative data bundled with LankaLens. It is separate from the MIT License that applies to LankaLens software.

## Software vs data

- **LankaLens software code** is licensed under the [MIT License](LICENSE).
- **Administrative data** in this package is derived from publicly available official Sri Lankan government sources. The MIT license applies to LankaLens software code and should not be presented as a licence grant by LankaLens over third-party source material.

## Sources

Administrative information is derived primarily from:

1. **Department of Census and Statistics (DCS), Sri Lanka** — canonical administrative hierarchy, codes, and English names  
   - https://www.statistics.gov.lk/qlink/AdminDivCode

2. **Ministry of Home Affairs (MOHA), Sri Lanka** — LIFe Location Codes and other verified official evidence for Sinhala and Tamil names where available  
   - http://moha.gov.lk:8090/lifecode/

LankaLens normalizes and structures this information for convenient use by .NET applications.

## What the package contains

The NuGet package contains a **normalized representation** of administrative information (codes, English names, Sinhala/Tamil names when available, and Province → District → Divisional Secretariat → Grama Niladhari hierarchy).

**Not included:** original DCS Excel/PDF files or MOHA HTML reports.

## Independent project

LankaLens is an independent open-source project. It is **not** affiliated with, endorsed by, or an official product of the Department of Census and Statistics, the Ministry of Home Affairs, or the Government of Sri Lanka.

Official government sources remain the reference for authoritative and current information.

## Further reading

- [docs/data-sources.md](docs/data-sources.md) — field authority and provenance
