# Data sources

## Priority

1. Department of Census and Statistics (DCS), Sri Lanka
2. Relevant Sri Lankan government ministry/department
3. Official Gazette or equivalent authoritative publication
4. Other authoritative government source

## Attribution

LankaLens is an independent open-source project. It is not affiliated with or endorsed by the Government of Sri Lanka. Do not describe generated data as “official LankaLens data.”

## Field authority (Phase 4 production snapshot)

| Field | Authority |
|-------|-----------|
| Codes | DCS Administrative Division Codes |
| Hierarchy (parent relationships) | DCS |
| English names | DCS |
| Sinhala names | MOHA LIFe Location Codes, confirmed MOHA→DCS mappings, and verified authoritative overlays |
| Tamil names | MOHA LIFe Location Codes, confirmed MOHA→DCS mappings, and verified authoritative overlays |

Because multiple authoritative sources contribute fields, public `DatasetMetadata` remains concise and describes the multi-source snapshot without implying a single organization supplied every field. Full provenance lives here and in `data/source/sources.json`.

## Licensing and redistribution

The MIT License applies to LankaLens **source code** only. It does **not** relicense Department of Census and Statistics or Ministry of Home Affairs source datasets, and does not grant MIT-style rights to those originals.

Package notice: [`DATA-NOTICE.md`](../DATA-NOTICE.md). Field authority and provenance live here and in `data/source/sources.json`.

**Phase 5.1 historical finding:** redistribution terms for the embedded production dataset were classified **UNCLEAR** during investigation. Full analysis (historical): [`docs/data-licensing-review.md`](data-licensing-review.md). Unsent agency drafts (historical): [`docs/data-permission-request.md`](data-permission-request.md). Those documents are research records, not current publication gates.

DCS publishes Administrative Division Codes and related count tables on its public website. A formal open-data / redistribution licence for these published Excel/PDF files was not clearly identified on the download pages or inside the GNDList workbook during inspection. The DCS microdata dissemination policy governs microdata access and does **not** grant MIT-style reuse of published administrative code lists. MOHA LIFe reports are publicly reachable; no reuse/redistribution licence was identified on the LIFe application.

Repository practice for original materials:

- Do **not** commit the original `.xlsx` / `.pdf` workbooks to the public repository.
- Do **not** commit MOHA cached HTML reports.
- Commit provenance metadata (`data/source/sources.json`: URL, filename, retrieved date, SHA-256, purpose).
- Commit the generated canonical JSON used for embedding (derived data), mappings, overlays, and gap reports.
- Keep local snapshots under `data/source/` (gitignored binaries) for DataBuilder regeneration.
- Keep [`DATA-NOTICE.md`](../DATA-NOTICE.md) accurate and packed with the package when packing locally.

## Required provenance (per dataset release)

Document for every release:

- Source organization
- Dataset name
- Source location / URL
- Retrieved date
- Effective / reference date (when known; do not invent)
- Original filename
- SHA-256 hash
- Processing notes
- Known issues / unresolved gaps

Machine-readable provenance: [`data/source/sources.json`](../data/source/sources.json).  
Snapshot count/coverage expectations: [`data/source/snapshot-expectations.json`](../data/source/snapshot-expectations.json).

## Official DCS links (reference)

| Resource | URL |
|----------|-----|
| Administrative Division Codes (page) | https://www.statistics.gov.lk/qlink/AdminDivCode |
| Excel download | https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel |
| PDF download | https://www.statistics.gov.lk/qlink/AdminDivCodes |
| No. of GNDs by DS & District | https://www.statistics.gov.lk/ref/GNbyDistrict |
| District Statistical Handbooks | https://www.statistics.gov.lk/ref/HandbookDictionary |

## Primary workbook: `GNDList_Final.xlsx`

| Field | Value |
|-------|--------|
| Local snapshot name | `dcs-gndlist-final-2024-03-19.xlsx` |
| Server filename | `GNDList_Final.xlsx` |
| SHA-256 | `C4D40C4478B306EEBABA4ABA3A0BF9BE30D5F589D4D05440040BE14524F363E0` |
| Sheet | `GNDList` (single sheet) |
| As at | 19 March 2024 (MRCB-2024, Cartography Division) |
| Retrieved | 2026-08-16 |
| Data rows | 14,008 GN divisions |

DCS provides codes, hierarchy, and English names only (no Sinhala/Tamil columns).

## Count cross-check: `NoofGNbyDS.xlsx`

| Field | Value |
|-------|--------|
| Local snapshot name | `dcs-no-of-gn-by-ds-2024-03-19.xlsx` |
| SHA-256 | `223DEA3EAB9923DD8ABEF80E34DD4E0B243DC954026129F2D984E638306DABDF` |
| Retrieved | 2026-08-16 |
| Counts | 25 districts, 340 DS divisions, 14,008 GN divisions |

## MOHA LIFe Location Codes

| Field | Value |
|-------|--------|
| Organization | Ministry of Home Affairs, Sri Lanka — Home Affairs Division (IT Unit) |
| Endpoint | `POST /lifecode/views/rpt_gn_list.php` (per district) |
| Combined SHA-256 | `31D1BB35A3D30CC810A061A5EC65CCCC9EBE3FDF6F5A5F5637C1E3B80240CFC8` |
| Retrieved | 2026-08-16 |
| Purpose | Authoritative Sinhala/Tamil names joinable via LIFe → DCS `GND_UID` |

No published MOHA source/effective date was found on the LIFe UI; do not invent one. Raw HTML remains gitignored.

## Confirmed mappings and overlays

- [`data/mappings/moha-to-dcs.json`](../data/mappings/moha-to-dcs.json) — confirmed MOHA→DCS code mappings (DS recodes with optional GN-component propagation; GN transfers)
- [`data/mappings/authoritative-name-overlays.json`](../data/mappings/authoritative-name-overlays.json) — verified Sinhala/Tamil overlays for specific DCS codes (e.g. `5225`, `5221`, `3136`)

## Unresolved GN records

For the current snapshot, **285** GN divisions have no confirmed authoritative Sinhala/Tamil mapping. They are documented in:

- [`data/generated/unresolved-multilingual-gaps.json`](../data/generated/unresolved-multilingual-gaps.json)
- [`data/generated/final-gap-resolution-report.md`](../data/generated/final-gap-resolution-report.md)

Production JSON emits `"sinhala": null` and `"tamil": null` for these records. LankaLens does **not** fill gaps with AI translation, transliteration, or guessing.

## Production coverage (current snapshot)

| Level | English | Sinhala | Tamil |
|-------|--------:|--------:|------:|
| Province | 9/9 | 9/9 | 9/9 |
| District | 25/25 | 25/25 | 25/25 |
| DS | 340/340 | 340/340 | 340/340 |
| GN | 14,008/14,008 | 13,723/14,008 | 13,723/14,008 |

These expectations are versioned in `snapshot-expectations.json` alongside the DCS/MOHA source metadata — they are not eternal constants.

## Phase 4 status

Production canonical JSON is generated by DataBuilder `build` and embedded in the runtime package at:

- `data/generated/administrative-divisions.json`
- `src/LankaLens.AdministrativeDivisions/Data/administrative-divisions.json` (assembly resource)

`AdministrativeDivisions.Default` loads the embedded production dataset. The development fixture has been removed from the runtime library.

## DataBuilder dependencies

`LankaLens.DataBuilder` uses **ClosedXML** `0.105.0` (MIT) for `.xlsx` reading and **HtmlAgilityPack** `1.11.72` (MIT) for MOHA GN report HTML. Neither is required at runtime. `LankaLens.AdministrativeDivisions` remains package-free.
