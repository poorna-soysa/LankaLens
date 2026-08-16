# Multilingual source analysis (Phase 3.5)

**Inspection date:** 2026-08-16  
**Canonical structure source (unchanged):** DCS Administrative Division Codes workbook as at 19 March 2024 — 9 provinces, 25 districts, 340 DS divisions, 14,008 GN divisions; English names complete; Sinhala/Tamil absent.

**Phase 3.5 scope:** discover authoritative Sinhala/Tamil sources only. No runtime package changes, no AI/machine translation, no production JSON, no full national scrape, no Gazette parser.

**Decision:** `PARTIAL AUTHORITATIVE MULTILINGUAL COVERAGE`

---

## Sources investigated

| # | Organization | URL / resource |
|---|--------------|----------------|
| 1 | Department of Census and Statistics (DCS) | [AdminDivCode](https://www.statistics.gov.lk/qlink/AdminDivCode) (+ `/si`, `/ta` chrome) |
| 2 | DCS | [AdminDivCodes Excel](https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel) / PDF |
| 3 | DCS | [No. of GN by DS](https://www.statistics.gov.lk/ref/GNbyDistrict) |
| 4 | DCS | [District Statistical Handbook dictionary](https://www.statistics.gov.lk/ref/HandbookDictionary) → `https://www.statistics.gov.lk/HandBook/{District}/{Year}` |
| 5 | DCS | [CPH 2024 portal](https://www.statistics.gov.lk/Population/StaticalInformation/CPH2024) — Final Report Si/Ta/En PDFs; GN population Excel |
| 6 | DCS | [GND Reports 2020](https://www.statistics.gov.lk/Population/StaticalInformation/GNDReports) |
| 7 | DCS | [District Atlases](https://www.statistics.gov.lk/Population/StaticalInformation/DistrictAtlases) |
| 8 | DCS | [Statistical Pocket Book 2025](https://www.statistics.gov.lk/Publication/PocketBook2025) (+ Si/Ta pocket book) |
| 9 | Ministry of Home Affairs (MOHA) | [LIFe Location Codes](http://moha.gov.lk:8090/lifecode/) — EN / [Sinhala village list](http://moha.gov.lk:8090/lifecode/village_list_sinhala) / [Tamil home](http://moha.gov.lk:8090/lifecode/home_tamil) / [GN list UI](http://moha.gov.lk:8090/lifecode/gn_list) |
| 10 | MOHA | Official GN report endpoint (HTML table): `POST http://moha.gov.lk:8090/lifecode/views/rpt_gn_list.php` |
| 11 | MOHA / Resource Profile | [resourceprofile.gov.lk](https://resourceprofile.gov.lk/) |
| 12 | Ministry of Public Administration | [pubad.gov.lk](https://pubad.gov.lk/web/index.php?lang=en) |
| 13 | NSDI | [Boundaries MapServer — GND layer](https://gisapps.nsdi.gov.lk/server/rest/services/Srilanka/Boundaries/MapServer/1) |
| 14 | Department of Government Printing | [documents.gov.lk](https://documents.gov.lk/) (Extraordinary Gazettes; English / Sinhala / Tamil editions historically) |
| 15 | District Secretariats (sample) | `*.dist.gov.lk` — Colombo, Gampaha, Kandy, Galle, Jaffna, Batticaloa, Trincomalee, Anuradhapura, Badulla |

**Third-party pointers only (not production sources):** open-admin-data / OCHA COD-AB; GitHub scrapes of MOHA LIFe. Used solely to locate official endpoints and to flag count mismatch risk.

---

## Candidate source cards

### A. DCS Administrative Division Codes (canonical structure)

| Field | Value |
|-------|--------|
| Organization | Department of Census and Statistics, Sri Lanka |
| URL | https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel |
| Administrative levels | Province, District, DS, GN (+ LG columns unused by LankaLens) |
| English available? | Yes (complete) |
| Sinhala available? | No |
| Tamil available? | No |
| Official codes available? | Yes — `GND_UID` and component codes |
| Current date/version? | As at 19 March 2024 (MRCB-2024) |
| Machine readable? | Yes (Excel) |
| Downloadable? | Yes |
| Coverage | 9 / 25 / 340 / 14,008 |
| Matching strategy | N/A — defines canonical codes and English |
| Terms/licensing notes | Public download; no MIT-style redistribution licence identified (Phase 3). Keep binaries gitignored. |

**Language chrome note:** DCS pages support `/si` and `/ta` URL suffixes (e.g. `/qlink/AdminDivCode/si`). UI chrome localizes; Excel/PDF downloads remain English-only administrative lists.

---

### B. MOHA LIFe Location Codes (strongest Sinhala/Tamil candidate)

| Field | Value |
|-------|--------|
| Organization | Ministry of Home Affairs — Home Affairs Division (IT Unit) |
| URL | http://moha.gov.lk:8090/lifecode/ |
| Administrative levels | Province, District, DS, GN, Village |
| English available? | Yes |
| Sinhala available? | Yes |
| Tamil available? | Yes |
| Official codes available? | Yes — `life_code` (e.g. `1-1-03-005`) |
| Current date/version? | Live system; no published “as at” stamp on the UI |
| Machine readable? | Partially — HTML tables via POST endpoints (not a published national Excel/CSV) |
| Downloadable? | No official bulk file; interactive browse + report pages |
| Coverage | **Verified sample (Colombo district, 2026-08-16):** 557 GN rows, all with Sinhala + Tamil + English; 13 DS labels in the same rows. **National bulk count not enumerated in Phase 3.5** (full scrape deferred). Third-party scrapes of the same system have reported fewer GNs than DCS 14,008 — treat as **structural mismatch risk**, not as coverage. |
| Matching strategy | Preferred: strip dashes from `life_code` → DCS `GND_UID` (`1-1-03-005` → `1103005`). Confirmed for Sammanthranapura and Colombo sample. DS/province/district names appear trilingual in the same GN report row (slash-separated labels). |
| Terms/licensing notes | Official government portal. No open-data licence stated. Scraping would require terms/stability review before any production job. |

**Colombo probe (official endpoint only):**

- `POST .../views/rpt_gn_list.php` with `province=63` (Western) and `district=42` (Colombo)
- 557 data rows; 557 with Sinhala script; 557 with Tamil script; 557 unique `life_code`
- DCS Colombo GN count in canonical workbook: **557** (exact match for this district)
- Sample join: `1-1-03-005` / සම්මන්ත්‍රණපුර / சம்மந்திரணபுர / Sammanthranapura → `GND_UID` `1103005`

---

### C. DCS CPH 2024 GN population Excel

| Field | Value |
|-------|--------|
| Organization | DCS |
| URL | https://www.statistics.gov.lk/Population/StaticalInformation/CPH2024/GN_population_excel |
| Administrative levels | Province, District, GN (population tables) |
| English available? | Yes |
| Sinhala available? | **No** (0 Sinhala shared strings in workbook probe) |
| Tamil available? | **No** (0 Tamil shared strings) |
| Official codes available? | Present as hierarchy labels; not a replacement for AdminDivCodes |
| Current date/version? | CPH 2024 |
| Machine readable? | Yes (Excel) |
| Downloadable? | Yes |
| Coverage | National GN population stats; English names only |
| Matching strategy | English name + hierarchy only if needed — **not** a multilingual source |
| Terms/licensing notes | Same DCS public-publication posture as AdminDivCodes |

---

### D. DCS CPH 2024 Final Report (Sinhala / Tamil / English PDFs)

| Field | Value |
|-------|--------|
| Organization | DCS |
| URL | https://www.statistics.gov.lk/Population/StaticalInformation/CPH2024 — Final Report Si / Ta / En PDFs |
| Administrative levels | National / district statistical narrative (not a full GN directory) |
| English / Sinhala / Tamil | Separate language PDFs published |
| Official codes available? | Not as a joinable GN code list |
| Machine readable? | No (PDF) |
| Downloadable? | Yes |
| Coverage | Useful for **Province / District** official language forms where tables appear; **not** a complete 14,008 GN name extract |
| Matching strategy | Manual / high-level label enrichment only |
| Terms/licensing notes | DCS publication |

---

### E. DCS District Statistical Handbooks 2025

| Field | Value |
|-------|--------|
| Organization | DCS |
| URL | https://www.statistics.gov.lk/ref/HandbookDictionary → `https://www.statistics.gov.lk/HandBook/{District}/{Year}` |
| Administrative levels | District handbook chapters (admin, population, etc.) |
| English available? | Yes (chapter titles bilingual with local language) |
| Sinhala available? | Yes for Sinhala-majority districts (e.g. Colombo 2025 TOC in Sinhala + English) |
| Tamil available? | Yes for Tamil-majority districts (e.g. Jaffna 2025 TOC in Tamil + English) |
| Official codes available? | Not as a national code workbook |
| Current date/version? | 2025 editions listed for all 25 districts |
| Machine readable? | No (PDF) |
| Downloadable? | Yes (per district year) |
| Coverage | Per-district PDFs; **not** one national trilingual GN file; language of body content tends to follow district language + English |
| Matching strategy | Poor for national automated join; possible spot verification |
| Terms/licensing notes | DCS publication |

---

### F. NSDI Boundaries GIS (GND layer)

| Field | Value |
|-------|--------|
| Organization | National Spatial Data Infrastructure (gov.lk GIS) |
| URL | https://gisapps.nsdi.gov.lk/server/rest/services/Srilanka/Boundaries/MapServer/1 |
| Administrative levels | GN polygons; related DS / District / Province layers |
| English available? | Yes (`gnd_name`, `gnd_name_census`) |
| Sinhala available? | Partial — `gnd_name_gazetted` observed in Sinhala even for Jaffna sample features |
| Tamil available? | **No dedicated Tamil name field** in layer schema |
| Official codes available? | Schema includes `gnd_census_code` etc.; **sample queries returned null census codes** |
| Current date/version? | Live service; feature count **14,051** vs DCS **14,008** |
| Machine readable? | Yes (ArcGIS REST JSON / geoJSON; maxRecordCount 1000) |
| Downloadable? | Query/export via REST (paginated) |
| Coverage | Near-national polygons; **+43 features vs DCS**; unsafe as sole multilingual authority |
| Matching strategy | Prefer census code when populated; otherwise hierarchy + English — **GN name-only matching forbidden**. Currently census codes null in samples → **not production-ready for join**. |
| Terms/licensing notes | Government GIS service; redistribution terms not clearly stated on layer page |

---

### G. District Secretariat websites (sample)

| Field | Value |
|-------|--------|
| Organization | Individual District Secretariats |
| URLs | e.g. http://www.kandy.dist.gov.lk/, http://www.colombo.dist.gov.lk/, http://gampaha.dist.gov.lk/, … |
| Administrative levels | Often DS lists / GN **counts**; rarely full trilingual GN directories |
| English / Sinhala / Tamil | Some sites (e.g. Kandy) expose සිංහල / தமிழ் switchers (FaLang) |
| Official codes available? | Rarely |
| Machine readable? | No consistent national pattern |
| Downloadable? | Occasional PDFs; site-dependent |
| Coverage | Spot-check 2026-08-16: Colombo, Gampaha, Kandy, Galle, Jaffna, Trincomalee, Anuradhapura, Badulla responded HTTP 200; Batticaloa timed out. Kandy “Grama Niladhari Division” page lists **English DS names + GN counts only** (total 1,188), not individual Si/Ta GN names. |
| Matching strategy | Verification aid only |
| Terms/licensing notes | Per-site; scraping not recommended as primary strategy |

**Finding:** **No consistent national multilingual GN source pattern** across District Secretariat sites.

---

### H. Official Gazettes

| Field | Value |
|-------|--------|
| Organization | Department of Government Printing |
| URL | https://documents.gov.lk/ (language switchers: English / සිංහල / தமிழ்) |
| Administrative levels | Creation / rename / boundary notices for selected divisions over time |
| English / Sinhala / Tamil | Extraordinary Gazettes historically published in all three languages (separate files) |
| Official codes available? | Sometimes GN numbers / DS references; not a full coded directory |
| Current date/version? | Continuous publication |
| Machine readable? | No (PDF); year index pages checked on 2026-08-16 returned 404 for some legacy paths — site instability |
| Downloadable? | Yes (individual gazettes) |
| Coverage | **Not** a complete 14,008 list; realistic **fallback for recently created/renamed** divisions only |
| Matching strategy | Manual / future Gazette workflow with Province→District→DS→GN identity; do not silently map across structural change |
| Terms/licensing notes | Official legal publication |

**Phase 3.5 stance:** Gazettes are a realistic authoritative fallback for deltas. **Do not build a Gazette parser in this phase.**

---

## Strongest sources

### 1. DCS AdminDivCodes Excel (structure + English)

- **Authority:** Highest for census codes and hierarchy
- **Freshness:** 2024-03-19 (aligned to current LankaLens canonical counts)
- **Languages:** English only
- **Levels:** All four
- **Coverage:** Complete for English
- **Machine readability:** Excellent
- **Identifiers:** `GND_UID` — keep immutable

### 2. MOHA LIFe Location Codes (Sinhala + Tamil)

- **Authority:** Ministry of Home Affairs operational directory
- **Freshness:** Live; may lag or diverge from DCS MRCB-2024
- **Languages:** English + Sinhala + Tamil in one GN report response
- **Levels:** Province → Village; GN report is the key extract
- **Coverage:** Colombo sample complete and code-joinable; national completeness **unknown without controlled enumeration**
- **Machine readability:** Moderate (HTML); maintainable only with careful rate limits and schema monitoring
- **Identifiers:** `life_code` maps to `GND_UID` when dashes removed — **preferred join**

### 3. DCS CPH 2024 Si/Ta publications + District Handbooks

- **Authority:** DCS
- **Freshness:** 2024–2025
- **Languages:** Trilingual publications exist at report level; handbooks bilingual by district language
- **Levels:** Best for Province/District labels; weak for national GN automation
- **Coverage:** Not a substitute for MOHA at GN level
- **Machine readability:** Poor (PDF)

### 4. NSDI GIS

- **Authority:** Government spatial infrastructure
- **Freshness:** Live
- **Languages:** Sinhala gazetted names present; Tamil absent as a field
- **Coverage / identifiers:** Count mismatch vs DCS; census codes null in samples → **do not use as primary multilingual join source**

---

## Coverage

### Matrix (against DCS canonical 9 / 25 / 340 / 14,008)

| Level | English | Sinhala | Tamil | Source |
| ----- | ------: | ------: | ----: | ------ |
| Province | complete (9/9) | available via MOHA UI labels / DCS Si publications (not in AdminDivCodes) | available via MOHA UI labels / DCS Ta publications | DCS English; MOHA / DCS pubs for Si/Ta |
| District | complete (25/25) | available via MOHA (not bulk-counted here) | available via MOHA (not bulk-counted here) | DCS English; MOHA for Si/Ta |
| DS | complete (340/340) | **partial** — Colombo sample 13 DS labels trilingual; national MOHA DS count unverified | **partial** (same) | DCS English; MOHA for Si/Ta where joinable |
| GN | complete (14008/14008) | **partial** — **557 / 14008** verified (Colombo); remainder not Phase-3.5-enumerated | **partial** — **557 / 14008** verified (Colombo) | DCS English; MOHA LIFe for joinable subset |

### Numeric coverage (Phase 3.5 verified)

```text
Province Sinhala: not bulk-extracted in Phase 3.5 (UI indicates all 9 present in MOHA Si/Ta)
Province Tamil:   not bulk-extracted in Phase 3.5 (UI indicates all 9 present in MOHA Si/Ta)

District Sinhala: not bulk-extracted in Phase 3.5 (MOHA lists 25 districts)
District Tamil:   not bulk-extracted in Phase 3.5

DS Sinhala: 13 / 340 verified (Colombo only); national unknown
DS Tamil:   13 / 340 verified (Colombo only); national unknown

GN Sinhala: 557 / 14008 verified (Colombo only); national unknown without controlled MOHA enumeration
GN Tamil:   557 / 14008 verified (Colombo only); national unknown without controlled MOHA enumeration
```

**Do not treat third-party “~12,020 GN” figures as LankaLens coverage.** They only warn that a full MOHA extract may **not** cover every DCS `GND_UID`.

---

## Matching

### Preferred

```text
MOHA life_code (remove '-')  ==  DCS GND_UID
```

Example: `1-1-03-005` → `1103005`.

### Second-best

```text
Province + District + DS + exact English official name
```

Only when codes cannot be aligned. Still require full hierarchy for GN (duplicate English GN names exist within DCS).

### Forbidden

- Matching GN by name alone
- Replacing DCS codes/hierarchy from MOHA/NSDI/Gazette when counts diverge
- Filling gaps with LLM / Google Translate / transliteration

### Multi-source assembly (acceptable later)

```text
DCS current workbook  → codes / hierarchy / English
MOHA LIFe             → Sinhala / Tamil (joinable subset)
DCS Si/Ta pubs        → optional Province/District label cross-check
Gazette               → deltas / renamed units only
```

Every multilingual field must retain provenance (`sourceId`). Conflicts must be reported, not silently overwritten.

---

## Structural mismatch

| Source | Provinces | Districts | DS | GN | vs DCS 9/25/340/14008 |
|--------|----------:|----------:|---:|---:|------------------------|
| DCS AdminDivCodes (canonical) | 9 | 25 | 340 | 14,008 | baseline |
| DCS Pocket Book 2025 (admin table) | 9 | 25 | — | 14,007* | GN off-by-one note in pocket book footnotes |
| MOHA Colombo sample | — | 1 | 13 | 557 | **matches** DCS Colombo |
| MOHA national (not enumerated) | 9 | 25 | ? | ? | third-party scrapes suggest fewer GNs than DCS |
| NSDI GND layer | 9 | — | — | 14,051 | **+43** vs DCS; census codes null in samples |

\* Pocket Book footnote also discusses special Matale/Batticaloa cases — reinforces that publications can disagree slightly; **do not silently remap**.

Possible difference classes to flag in any future join: renamed divisions, splits/merges, new/removed GNs, code changes, English spelling aliases (already seen in Phase 3 counts file: e.g. Addalaichchenai vs Addalachchenai; Seethawaka vs Hanwella naming).

---

## Conflicts

| Conflict | Detail |
|----------|--------|
| Language vs structure authority | DCS owns codes; MOHA owns operational trilingual names — may diverge |
| NSDI vs DCS counts | 14,051 vs 14,008 |
| NSDI language | Sinhala gazetted names without Tamil field; Jaffna features still Sinhala in `gnd_name_gazetted` samples |
| DS English aliases | Phase 3 validation already warned on counts-file aliases; MOHA DS labels may use alternate English forms (e.g. Hanwella vs Seethawaka) |
| Handbook language split | Colombo handbook Si+En; Jaffna handbook Ta+En — not a single trilingual national GN workbook |
| District sites | Language chrome ≠ downloadable trilingual GN datasets |

---

## Licensing / redistribution

- **DCS / MOHA / NSDI / Gazettes:** public government websites; **no clear MIT-style open-data licence** identified for bulk redistribution of name lists.
- LankaLens MIT licence covers **code**, not government datasets.
- Continue Phase 3 policy: **do not commit** original government Excel/PDF/HTML dumps; commit provenance metadata only when a source is ingested.
- Before any scraper: review robots/terms, rate limits, and prefer asking DCS/MOHA for an official extract.

---

## Scraping maintainability (pre-scraper report)

| System | Structured consistently? | Stable identifiers? | Download permitted? | File/API alternative? | Maintainable scrape? |
|--------|--------------------------|---------------------|---------------------|------------------------|----------------------|
| MOHA LIFe | Hierarchy UI + HTML reports | `life_code` yes | Unclear for bulk automation | **No** published national file | Fragile (PHP/HTML, slow server); only after terms review |
| NSDI REST | Yes | Census codes **unreliable** today | Query API exists | REST is the interface | Possible but wrong for Tamil; count mismatch |
| District sites | **No** national pattern | Weak | Site-dependent | Occasional PDFs | **Not** recommended as primary |
| Handbooks / CPH PDFs | Per publication | Weak for GN | Yes downloads | PDFs only | OCR/parser heavy; not primary |
| Gazettes | Legal PDF pattern | Case-by-case | Yes | No bulk GN API | Fallback for deltas only |

**Prefer official files/APIs over HTML scraping.** MOHA is the only practical official trilingual GN source found; it still needs a product decision on acquisition method (request extract vs carefully governed fetch).

---

## Provenance design review

Current DataBuilder provenance is **dataset-level** (`CanonicalDatasetMetadata` + `data/source/sources.json`). That is insufficient once English/Sinhala/Tamil come from different organizations.

**Recommendation (implement later, not in Phase 3.5):** internal field-level attribution in DataBuilder only:

```text
Entity
  canonical code
  English  { value, sourceId }
  Sinhala  { value, sourceId }
  Tamil    { value, sourceId }
```

Runtime JSON may remain compact `LocalizedName(English, Sinhala, Tamil)`. Do **not** change the public API solely for provenance.

---

## Recommendation

| Level | English | Sinhala | Tamil |
|-------|---------|---------|-------|
| Province | DCS AdminDivCodes | MOHA LIFe labels (preferred) and/or DCS Sinhala publications for cross-check | MOHA LIFe labels and/or DCS Tamil publications |
| District | DCS AdminDivCodes | MOHA LIFe | MOHA LIFe |
| DS | DCS AdminDivCodes | MOHA LIFe where `life_code` / hierarchy joins cleanly | MOHA LIFe where joinable |
| GN | DCS AdminDivCodes | MOHA LIFe for joinable `GND_UID`s only | MOHA LIFe for joinable `GND_UID`s only |

**Unmatched DCS entities:** leave Sinhala/Tamil empty — never invent names.  
**Gazettes:** use later for documented creates/renames only.  
**NSDI / Wikipedia / OSM / open-admin-data / GitHub dumps:** not production authorities.

---

## Completion report

### Sources investigated

Listed above (DCS AdminDivCodes, CPH 2024 Excel/PDFs, Handbooks 2025, Pocket Book 2025, GND Reports, Atlases, MOHA LIFe + endpoints, Resource Profile, PubAd, NSDI Boundaries, documents.gov.lk Gazettes, nine District Secretariat sites).

### Strongest sources

1. **DCS AdminDivCodes** — codes, hierarchy, English  
2. **MOHA LIFe** — Sinhala + Tamil with joinable `life_code` (Colombo proven)  
3. **DCS Si/Ta publications / Handbooks** — Province/District language support and verification  
4. **Gazettes** — delta fallback only  

### Coverage

English complete. Sinhala/Tamil **partial**: **557 / 14,008** GN verified trilingual via official MOHA Colombo extract; national MOHA coverage not fully counted in Phase 3.5.

### Matching

`life_code` without dashes → `GND_UID`. Hierarchy + exact English as fallback. No GN name-only matching.

### Conflicts

Count and naming conflicts between DCS, MOHA (risk), NSDI (+43), and handbook/pocket-book footnotes — must be reported, never silently mapped.

### Licensing / redistribution

No clear open redistribution licence; keep government binaries out of git; provenance required.

### Recommendation

Feed **English** from DCS; feed **Sinhala/Tamil** from MOHA LIFe for deterministic joins; use DCS Si/Ta pubs for upper-level checks; Gazettes for deltas.

### Decision

```text
PARTIAL AUTHORITATIVE MULTILINGUAL COVERAGE
```

**Not Outcome A:** no single official downloadable file provides all 14,008 GN names in Sinhala **and** Tamil aligned to the March 2024 DCS hierarchy.  
**Not Outcome C:** official trilingual names exist (MOHA) and are code-joinable for at least Colombo; Province/District Si/Ta appear in official DCS/MOHA channels.

**Phase 4 remains blocked** until a product/data-policy decision on remaining GN/DS gaps (wait for official extract vs governed MOHA enumeration vs leave blanks vs Gazette-only deltas).

Do **not** begin Phase 4 from this document alone.

---

## Phase 3.6 national validation (2026-08-16)

**Acquisition:** official generated GN report `POST http://moha.gov.lk:8090/lifecode/views/rpt_gn_list.php` for all 25 districts. No bulk file, robots.txt, or published source date. `robots.txt` is 404. DataTables Excel/print buttons are client-side exports of the same HTML table, not a national download. Cached under `data/source/moha-life/` (gitignored). Combined SHA-256: `31D1BB35A3D30CC810A061A5EC65CCCC9EBE3FDF6F5A5F5637C1E3B80240CFC8`.

**Join:** structure-validated `Province-District-DSD-GND` (`1-1-03-005` → `1103005`). 0 invalid LIFe codes. 0 hierarchy mismatches. 0 duplicate join codes.

| Level | DCS | MOHA matched | Sinhala | Tamil | Conflicts |
| ----- | --: | -----------: | ------: | ----: | --------: |
| Province | 9 | 9 | 9 | 9 | 0 |
| District | 25 | 25 | 25 | 25 | 0 |
| DS | 340 | 331 | 330 | 330 | 1 (`5225` Kalmunai North Sub Office vs Sainthamarathu) |
| GN | 14,008 | 13,576 | 13,576 | 13,576 | 0 |

GN unmatched: 432 DCS-only, 444 MOHA-only. Nine DCS DS codes have no MOHA counterpart; nine MOHA DS codes have no DCS counterpart — mostly same English names with different codes (likely recodes after 2024-03-19).

English comparison (all levels): 11,614 exact; 278 formatting-only; 2,048 spelling/substantive. MOHA English does not overwrite DCS.

**Decision:** `CONDITIONALLY READY FOR PHASE 4`

Do **not** begin Phase 4 from this addendum alone. Review `data/generated/moha-dcs-join-report.json` first. Production `administrative-divisions.json` was not written.

---

## Phase 3.7 administrative delta resolution (2026-08-16)

**Scope:** Analyse 432 DCS-only / 444 MOHA-only GN deltas; introduce reviewable MOHA→DCS mappings; project multilingual coverage in-memory only. No runtime changes, no AI translation, no production JSON, no Phase 4.

**Mapping artifact:** [`data/mappings/moha-to-dcs.json`](../data/mappings/moha-to-dcs.json) — confirmed mappings only, validated by DataBuilder (`MappingFileValidator`). Speculative name-only joins are forbidden.

**Confirmed DS recodes (6)** with `childPropagation=GnComponentUnchanged` (GN-component bijection verified):

| MOHA | DCS | English |
| ---- | --- | ------- |
| 3157 | 3134 | Gonapeenuwala / Gonapinuwala |
| 3138 | 3135 | Madampagama |
| 3336 | 3325 | Walasmulla |
| 4145 | 4104 | Karainagar |
| 5142 | 5104 | Koralai Pattu Central |
| 5139 | 5110 | Koralai Pattu South (Kiran) |

**Not confirmed:** Kothmale West (`2302`↔`2304`) and Norwood (`2314`↔`2316`) — same GN *count* but **different GN components** (not a simple recode). Kalmunai North Sub (`5221`) — MOHA shares LIFe segment `5225` with Sainthamaruthu (source inconsistency). Ratmalana (`1139`) — partial GN transfer vs DCS `1131`. Residual unmatched GNs outside confirmed DS areas.

**DS 5225:** Raw MOHA `TRANSLATION_CONFLICT` retained. DS-level Si/Ta for DCS `5225` use exact-joined GN rows; when those still conflict, rows are narrowed to MOHA DS English compatible with DCS English (no majority vote). Conflict is **not resolved** as a structural merge.

**Projected coverage after confirmed mappings (in-memory):** see [`data/generated/multilingual-coverage-report.md`](../data/generated/multilingual-coverage-report.md) and [`data/generated/administrative-delta-report.md`](../data/generated/administrative-delta-report.md). Unresolved entities: [`data/generated/unresolved-multilingual-gaps.json`](../data/generated/unresolved-multilingual-gaps.json).

**Decision:** `CONDITIONALLY READY FOR PHASE 4`

Do **not** begin Phase 4 from this addendum alone. Remaining gaps are explicit; every published Sinhala/Tamil name must remain traceable to an authoritative source.

---

## Phase 3.8 final authoritative gap resolution (2026-08-16)

**Scope:** Resolve remaining DS/GN multilingual gaps with authoritative evidence only. No runtime changes, no AI translation, no production JSON, no Phase 4.

**Exact uncovered DS before 3.8 additions (336/340):** `2302` Kothmale West, `2314` Norwood, `5221` Kalmunai North Sub, `3136` Hikkaduwa (same code, zero GN exact-joins). DS `5225` already had matched-row Si/Ta; raw MOHA conflict retained.

**New confirmed mappings:** `2304→2302`, `2316→2314` (DS-level, no child propagation); `1139005→1131005` (Mount Lavinia GN parent/LIFe lag).

**Authoritative overlays:** [`data/mappings/authoritative-name-overlays.json`](../data/mappings/authoritative-name-overlays.json) for `5225`, `5221`, `3136`.

**Projected coverage:** Province 9/9; District 25/25; DS **340/340**; GN **13,723/14,008**. Remaining unresolved: **285 GN-only** records (see [`data/generated/final-gap-resolution-report.md`](../data/generated/final-gap-resolution-report.md)).

**DS 5225:** RESOLVED at DS-name level via filtered overlay (Sainthamaruthu-labelled MOHA rows only). Raw `TRANSLATION_CONFLICT` still reported.

**Decision:** `CONDITIONALLY READY FOR PHASE 4`

Do **not** begin Phase 4 from this addendum alone. Residual GN clusters require Gazette/DS GN code lists or a product blank-name policy.

