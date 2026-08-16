# Data licensing review

> **Historical / research documentation (Phase 5.1).** This file records the investigation performed during development. It is **not** a current automated or process publication gate. A later project decision was to proceed with publishing `LankaLens.AdministrativeDivisions` `0.1.0-preview.1` without waiting for separate written permission from DCS or MOHA. This document does **not** state that such permission or legal advice was obtained.

**Investigation / retrieval date:** 2026-08-16  
**Package under review:** `LankaLens.AdministrativeDivisions` `0.1.0-preview.1`  
**Status:** Engineering / release-readiness review — **not formal legal advice**

This document investigates redistribution and licensing status for Sri Lankan government-derived administrative data embedded in the NuGet package. It does not publish the package, change the authoritative dataset, remove attribution, or treat public website availability as unrestricted redistribution permission.

---

## Scope

### Separate concerns

| Concern | Instrument | Applies to |
|---------|------------|------------|
| LankaLens software | MIT License ([`LICENSE`](../LICENSE)) | Source code, build tooling, samples, tests |
| Administrative data | Source-specific government terms (if any) | Codes, names, hierarchy derived from DCS, MOHA LIFe, and related evidence |

The MIT License covers LankaLens’s own source code unless otherwise stated. It does **not** automatically relicense third-party government data. This review does **not** describe the administrative dataset as MIT-licensed.

### What LankaLens redistributes

The NuGet package embeds a normalized JSON snapshot as an assembly resource:

- Path: [`src/LankaLens.AdministrativeDivisions/Data/administrative-divisions.json`](../src/LankaLens.AdministrativeDivisions/Data/administrative-divisions.json)
- Consumed via `AdministrativeDivisions.Default`

**Included fields:** administrative codes; English / Sinhala / Tamil names (Sinhala/Tamil may be `null`); Province → District → DS → GN hierarchy.

**Coverage (bundled snapshot):** 9 provinces, 25 districts, 340 DS, 14,008 GN; Sinhala/Tamil complete for Province/District/DS; GN Sinhala/Tamil 13,723/14,008 (285 unresolved as `null`).

**Not redistributed:** original DCS Excel/PDF workbooks; MOHA HTML reports; Gazette PDFs; other source binaries.

Embedding JSON in a DLL does **not** exempt the data from redistribution concerns. Consumers receive the complete administrative dataset through the package API. Normalization and merging do **not** eliminate applicable source rights.

### Legal-review boundary

Where official terms are silent or ambiguous, this review classifies status as **UNCLEAR** rather than inferring permission. If redistribution remains unclear after this review, obtain **written permission** from the source organizations and/or competent legal advice before NuGet publication. Do not guess.

---

## Production data sources

Machine-readable provenance: [`data/source/sources.json`](../data/source/sources.json).

### Sources contributing production values

#### 1. `dcs-administrative-division-codes`

| Field | Value |
|-------|-------|
| Organization | Department of Census and Statistics, Sri Lanka |
| Dataset | Administrative Division Codes (GND List / MRCB-2024) |
| Official page | https://www.statistics.gov.lk/qlink/AdminDivCode |
| Excel download | https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel |
| PDF download | https://www.statistics.gov.lk/qlink/AdminDivCodes |
| Retrieved | 2026-08-16 |
| Published / effective | 2024-03-19 (workbook footnote: “As at 19th March 2024”) |
| Raw format | Excel `.xlsx` (`GNDList_Final.xlsx` → local `dcs-gndlist-final-2024-03-19.xlsx`) |
| SHA-256 | `C4D40C4478B306EEBABA4ABA3A0BF9BE30D5F589D4D05440040BE14524F363E0` |
| Raw committed? | **No** (gitignored) |
| In runtime package? | **Yes** — codes, English names, hierarchy |
| Existing license/terms in repo | None stated in `sources.json` |

#### 2. `moha-life-location-codes`

| Field | Value |
|-------|-------|
| Organization | Ministry of Home Affairs, Sri Lanka — Home Affairs Division (IT Unit) |
| Dataset | LIFe Location Codes (Grama Niladhari Division List) |
| UI | http://moha.gov.lk:8090/lifecode/ |
| Report endpoint | `POST http://moha.gov.lk:8090/lifecode/views/rpt_gn_list.php` |
| Cascade IDs | `POST .../lifecode/views/fetch.php` |
| Retrieved | 2026-08-16 |
| Published / effective | Not published on LIFe UI (do not invent) |
| Raw format | Generated HTML reports (per district) |
| Combined SHA-256 | `31D1BB35A3D30CC810A061A5EC65CCCC9EBE3FDF6F5A5F5637C1E3B80240CFC8` |
| Raw committed? | **No** (gitignored) |
| In runtime package? | **Yes** — Sinhala/Tamil names (joined / mapped / overlaid onto DCS codes) |
| Existing license/terms in repo | None identified; [`data/source/moha-life/README.md`](../data/source/moha-life/README.md) notes no open-data licence found |

### Pipeline / corroboration sources (not independent production name/code suppliers)

#### 3. `dcs-no-of-gn-by-ds`

| Field | Value |
|-------|-------|
| Organization | Department of Census and Statistics, Sri Lanka |
| Dataset | No. of GN Divisions by DS Division and District |
| URL | https://www.statistics.gov.lk/ref/GNbyDistrict |
| Retrieved | 2026-08-16 |
| Effective | 2024-03-19 |
| Raw committed? | **No** |
| In runtime package? | **No** as a separate dataset — used for **count validation only** |
| Classification relevance | Same organization as DCS codes; no additional redistribution surface beyond DCS |

#### 4. Confirmed MOHA→DCS mappings

File: [`data/mappings/moha-to-dcs.json`](../data/mappings/moha-to-dcs.json).

Production Sinhala/Tamil for mapped codes still originate from **MOHA LIFe**. The following URLs are **identity / corroboration evidence only**:

| Evidence source | URL | Role |
|-----------------|-----|------|
| Extraordinary Gazette 2493/26 (Walasmulla) | https://documents.gov.lk/view/extra-gazettes/egz_2026.html | Named-division corroboration for DS `3336→3325` |
| Cabinet Office decision 2019-05-07 | https://www.cabinetoffice.gov.lk/cab/index.php?Itemid=49&dID=9758&id=16&lang=en&option=com_content&view=article | New DS identity for Kotmale West / Norwood (`2304→2302`, `2316→2314`) |
| Government Information Centre (GIC) | https://gic.gov.lk/gic/index.php/en/component/org/?id=513&task=org | Identity evidence for Ratmalana GN `1139005→1131005` |

#### 5. Authoritative name overlays

File: [`data/mappings/authoritative-name-overlays.json`](../data/mappings/authoritative-name-overlays.json).

Three DS overlays (`5225`, `5221`, `3136`) apply **MOHA LIFe** Sinhala/Tamil labels to DCS codes. The Ministry of Public Administration Ampara DS list (https://pubad.gov.lk/web/index.php?Itemid=116&id=106&lang=en&option=com_content&view=article) corroborates separate DS entities; it does **not** supply the bundled name strings.

### Gazette-derived values in the runtime dataset

**Finding:** No Gazette text, Gazette name lists, or Gazette-only values are ingested into production JSON. Gazette references appear only as mapping evidence. Therefore a deep Gazette reproduction-law analysis is **not** required for this release gate. Short note: Official Gazette materials are public government publications; Sri Lanka’s Intellectual Property Act excludes certain “official text of a legislative, administrative or legal nature” from copyright protection (see Copyright section). That statutory note does **not** clear DCS Excel or MOHA LIFe redistribution.

District Statistical Handbooks and district secretariat websites were reviewed in earlier phases as reference material and are **not** production-value sources for the embedded snapshot.

---

## DCS

### Website publication of administrative codes

The Administrative Division Codes page publishes PDF and Excel downloads with no visible Creative Commons, ODbL, open-data licence badge, usage README, or redistribution statement on the page itself (retrieved 2026-08-16).

DCS Data Dissemination distinguishes:

> “Proactive Dissemination - DCS provides access to the official statistical publications and datasets readily available on its main website electronically…”

> “Reactive Dissemination - Whenever data users with advanced statistical requirements to obtain data other than those of already published material…”

Source: https://www.statistics.gov.lk/Datadessimination (retrieved 2026-08-16).

**Interpretation for this review:** The GND list is **publicly published** proactive material. Publication ≠ an express licence to redistribute a normalized copy via NuGet.

### Microdata Dissemination Policy (do not misapply)

DCS Microdata Dissemination Policy (version 1.1, effective 16 October 2014): https://www.statistics.gov.lk/Datadessimination/DataDissaPolicy_2007Oct26

Appendix B (Public Use Files), among other conditions:

> “The data and other materials provided by the Department of Census and Statistics (DCS) will not be redistributed or sold to other individuals, institutions, or organizations without the written agreement of the DCS.”

> “The data will be used for statistical and scientific research purposes only.”

**Scope of that policy:** anonymised survey/census **microdata** files (PUFs and licensed files), not the published administrative-code workbook.

**This review therefore:**

- Does **not** treat GNDList as microdata bound by Appendix B; and
- Does **not** treat inapplicability of Appendix B as a grant of MIT-style redistribution rights for the code list.

### Specific workbook inspection (2026-08-16)

Local snapshot `data/source/dcs-gndlist-final-2024-03-19.xlsx` (gitignored; SHA-256 matches `sources.json`):

- Sheets: single data sheet `GNDList` (no licence / terms / README sheet)
- Document properties: creator/editor metadata only; no licence field
- Shared-string scan for licence / copyright / redistribution keywords: **no hits**
- Footnote strings present:

> “Source : Master Regitry of Census Blocks (MRCB -2024)”  
> “Updated by : Cartography Division”  
> “Department of Census and Statistics”  
> “As at 19th March 2024”

No copyright notice or reuse terms inside the workbook beyond source attribution.

### Copyright footers on DCS properties

DCS-related pages and repositories commonly display wording such as:

> “Copyright © Department of Census and Statistics, Sri Lanka. All Rights Reserved”

(Example observed on DCS SDG and library properties; retrieved 2026-08-16.) That is a reservation of rights, not a reuse grant for NuGet redistribution.

### Intended use as coding standard (non-licence)

A DCS presentation to UNSIAP states that administration area codes are published on the DCS website “so that other users can make use of these to have a uniform system in Sri Lanka.” That supports the **purpose** of publishing codes as a national reference. It is **not** a written redistribution licence for packaging derived datasets on NuGet.org.

### DCS classification

| Field | Assessment |
|-------|------------|
| **Classification** | **UNCLEAR** |
| **Evidence** | Public Excel/PDF publication; no express open licence on page or workbook; microdata policy is a different product class; copyright footers reserve rights; coding-standard presentation is not a licence |
| **Conditions** | None located that would convert status to “permitted with conditions” |
| **Commercial use** | **Uncertain** — no express commercial clause for this published code list |
| **Attribution** | No mandatory legal wording located; good practice: cite DCS, dataset title, URL, as-at / retrieval dates |
| **Remaining uncertainty** | Whether DCS permits redistribution of a normalized, multilingual-merged administrative snapshot in an OSS NuGet package usable by commercial downstream software |

---

## MOHA LIFe

### Portal and acquisition

- UI: http://moha.gov.lk:8090/lifecode/ (public; no login observed for GN report generation)
- Acquisition (Phase 3.6): rate-limited `POST` to the same official report endpoint the UI uses (`rpt_gn_list.php`), with identifying User-Agent `LankaLens.DataBuilder/3.6 …` — see [`MohaLifeReportClient.cs`](../tools/LankaLens.DataBuilder/Acquisition/MohaLifeReportClient.cs)
- Prior project note: LIFe `robots.txt` returned 404; no robots-based permission located
- Ministry site chrome includes “All Rights Reserved” style notices (e.g. moha.gov.lk properties)

### Terms located

No copyright policy, terms of use, open-data licence, commercial-use clause, automated-access policy, or attribution mandate was located on the LIFe application or GN report pages (retrieved 2026-08-16).

### Permission to access vs permission to redistribute

| Question | Finding |
|----------|---------|
| Is the report endpoint publicly reachable without authentication? | **Yes** (as observed) |
| Does public access equal permission to redistribute transformed data via NuGet? | **No** — access and redistribution are distinct |
| Do located terms expressly permit NuGet redistribution? | **No** |
| Do located terms expressly prohibit automated one-shot snapshotting? | **No** (absence of prohibition ≠ permission to redistribute) |

### MOHA classification

| Field | Assessment |
|-------|------------|
| **Classification** | **UNCLEAR** |
| **Evidence** | Public LIFe reports; no reuse/redistribution licence found; ministry “All Rights Reserved” chrome; raw HTML not committed; transformed Si/Ta **are** in the package |
| **Conditions** | None located |
| **Commercial use** | **Uncertain** |
| **Attribution** | No mandatory wording located; good practice: cite Ministry of Home Affairs / LIFe Location Codes, UI URL, retrieval date |
| **Remaining uncertainty** | Whether MOHA permits redistribution of normalized Sinhala/Tamil administrative names (joined to DCS codes) in an OSS NuGet package with commercial downstream use |

---

## Other contributing sources

| Source | Production values? | Classification | Notes |
|--------|-------------------|----------------|-------|
| DCS GN count workbook | No (validation only) | **UNCLEAR** (same org as DCS; immaterial to package contents) | Same redistribution uncertainty class as DCS publications |
| Extraordinary Gazette (documents.gov.lk) | No | **N/A (evidence only)** | No Gazette strings in runtime JSON |
| Cabinet Office decision page | No | **N/A (evidence only)** | Identity evidence for DS mappings |
| PubAd Ampara DS list | No | **N/A (evidence only)** | Corroborates DS separation; names from MOHA |
| GIC Ratmalana listing | No | **N/A (evidence only)** | Identity evidence for one GN mapping |
| District / Divisional Secretariat sites | No | **N/A** | Not production inputs |

No additional permission-request drafts are required for corroboration-only sources unless future builds ingest their text as production values.

---

## Sri Lankan government / open-data framework

### data.gov.lk

- Portal: https://data.gov.lk/ (retrieved 2026-08-16)
- Encourages users to develop tools and applications from datasets made available through the portal
- ICTA footer observed: “All Rights Reserved”
- Individual datasets frequently list **“License Not Specified”**
- A Department of Census and Statistics publisher/group page exists; **no evidence** that the exact `GNDList_Final.xlsx` workbook or MOHA LIFe GN reports used by LankaLens are hosted there under a clear open licence that extends to LankaLens’s source copies

**Rule applied:** A general portal encouragement or another agency’s dataset licence must **not** be assigned to unrelated DCS/MOHA originals.

### National Data Sharing Policy

ICTA’s National Data Sharing Policy (2013 draft; discussed as still draft in later NSDI / research summaries) is **not** an enacted, dataset-specific redistribution licence for DCS administrative codes or MOHA LIFe names. References to “LIFe” in interoperability policy documents refer to ICTA’s Lankan Interoperability Framework, **not** the MOHA location-code application at `moha.gov.lk:8090/lifecode`.

### Right to Information (RTI)

RTI access rights (where applicable) address access to information held by public authorities. They do **not**, by themselves, grant a copyright-style or contractual right to redistribute a compiled dataset on NuGet.org.

---

## Copyright / reuse considerations

Primary statute reviewed: **Intellectual Property Act, No. 36 of 2003** (Parliament publication / NIPO references).

Relevant provisions (descriptive summary; not a legal conclusion about LankaLens’s dataset):

| Provision | Gist |
|-----------|------|
| s.8(a) | No copyright protection for ideas, procedures, concepts, discoveries, or **mere data**, even if embodied in a work |
| s.8(b) | No protection for **official text of a legislative, administrative or legal nature**, and official translations thereof |
| s.7 | **Collections of mere data (databases)** may be protected if original by reason of selection, coordination, or arrangement |

**Layers to keep distinct:**

1. **Factual administrative information** (existence of a named division under a code)
2. **Source website / workbook / HTML report as a compiled presentation**
3. **LankaLens’s transformed JSON compilation** (selection, normalization, merge of DCS + MOHA + mappings)

This review records the statutory text and **does not** conclude that the DCS workbook, MOHA reports, or LankaLens JSON are therefore free to redistribute. Ambiguity remains; permission or legal advice is the safe path.

---

## Redistribution analysis

### Per-source summary

| Source | Classification |
|--------|----------------|
| DCS Administrative Division Codes (production) | **UNCLEAR** |
| MOHA LIFe Location Codes (production) | **UNCLEAR** |
| DCS GN counts (validation only) | **UNCLEAR** (immaterial package surface) |
| Gazette / Cabinet / PubAd / GIC | **N/A (evidence only)** |

### Overall classification

### REDISTRIBUTION UNCLEAR

At least one material source (in practice: **both** DCS and MOHA) lacks sufficiently clear redistribution permission for the intended NuGet distribution of the embedded production dataset.

Not used:

- **REDISTRIBUTION CLEARED** — not supported
- **REDISTRIBUTION CLEARED WITH CONDITIONS** — conditions not located as express licence terms
- **REDISTRIBUTION BLOCKED** — no located term that clearly prohibits this specific reuse; silence is not treated as a hard block, but also not as clearance

### Transformation does not clear the gate

LankaLens normalizes codes/names, merges DCS English with MOHA Sinhala/Tamil, applies confirmed mappings, and emits `null` for unresolved locales. Those steps improve machine usability. They do **not** establish that source terms permit redistribution of the transformed compilation.

---

## Commercial downstream use

NuGet packages are routinely consumed by open-source, proprietary, and commercial applications. LankaLens itself is intended for broad .NET use.

**Finding:** No express commercial-use permission or prohibition was located for the published DCS administrative-code list or MOHA LIFe names.

**Release impact:** Commercial downstream use is **uncertain** and must be asked explicitly in permission requests. Do not claim the data is cleared for commercial redistribution.

---

## Attribution requirements

### Legally / contractually required wording

**None located** on the DCS Admin Division Codes page, DCS workbook, or MOHA LIFe UI that mandates a specific attribution sentence for derived redistributions.

### Good-practice attribution (voluntary; already used / recommended)

- Department of Census and Statistics, Sri Lanka — Administrative Division Codes (MRCB-2024 / GND list); page and download URLs; as-at 2024-03-19; retrieved 2026-08-16
- Ministry of Home Affairs, Sri Lanka — LIFe Location Codes; UI URL; retrieved 2026-08-16
- Disclaimer that LankaLens is an **independent** project, **not** affiliated with or endorsed by the Government of Sri Lanka
- Do not present LankaLens output as an official government dataset

Keep voluntary attribution in README / `DATA-NOTICE.md` / `docs/data-sources.md`. Do not invent mandatory legal wording.

### Endorsement / official-status

No located term requiring different disclaimer language was found. Existing independent-project disclaimers in the root and package READMEs are appropriate good practice and should be retained. Recommend pointing readers to [`DATA-NOTICE.md`](../DATA-NOTICE.md) for data-specific notices.

---

## Package licensing implications

| Item | Recommendation |
|------|----------------|
| `PackageLicenseExpression = MIT` | **Keep for this phase** — accurately describes **LankaLens software** |
| Bundled administrative data | **Not MIT-licensed** unless a source later grants that in writing |
| Communication | README + packed `DATA-NOTICE.md` must state the exclusion clearly |
| `LICENSE` file | Keep standard MIT text; do **not** rewrite MIT to “exclude data” inside the MIT grant itself — explain exclusions in README / DATA-NOTICE |
| Separate `NOTICE` file | **Not recommended** if `DATA-NOTICE.md` is sufficient (avoid redundancy) |

Open-source software licensing does **not** automatically make third-party government data open data.

---

## Recommended notices

| Artifact | Action |
|----------|--------|
| [`DATA-NOTICE.md`](../DATA-NOTICE.md) | Create; keep in repository; **pack into `.nupkg`** |
| Generic `NOTICE` | Skip unless future third-party code notices require it |
| [`docs/publishing.md`](publishing.md) | Explicit gate: publication requires redistribution status cleared |
| [`docs/data-permission-request.md`](data-permission-request.md) | Unsent DCS and MOHA drafts |
| README / package README | Point to DATA-NOTICE; avoid “open government data” / “public domain” / “MIT licensed data” claims |

---

## Remaining uncertainty

1. Whether DCS authorizes redistribution of a normalized administrative code/name hierarchy derived from the published GND list via NuGet.org.
2. Whether MOHA authorizes redistribution of Sinhala/Tamil names obtained from LIFe GN reports in that same package.
3. Whether commercial downstream use of that bundled data is allowed.
4. Whether any attribution text beyond good-practice citation is required.
5. Whether written permission, if granted, may be referenced publicly in project documentation.

Statutory database / official-text provisions may be relevant but are **not** treated here as a substitute for agency permission.

---

## Recommended action

1. **Do not publish** `LankaLens.AdministrativeDivisions` to NuGet.org until redistribution is cleared.
2. Send (when the maintainers choose) the drafts in [`docs/data-permission-request.md`](data-permission-request.md) to DCS and MOHA — **drafts only in-repo; this review does not send them**.
3. Keep Option A (embedded dataset) as the intended product design.
4. If permission is refused or remains unavailable, prefer safest fallbacks:
   - **Option E** — publish code documentation / samples without a NuGet data package; or
   - **Option B** — NuGet contains code only; user supplies/obtains data separately  
   Avoid treating **Option D** (separate data package) as a fix if redistribution itself is prohibited. **Option C** (build-time live download) fails offline use and still depends on source terms and endpoint availability.
5. If terms stay ambiguous after agency contact, obtain competent legal advice rather than inferring clearance.

### Release alternatives (evaluated, not implemented)

| Option | Description | Assessment |
|--------|-------------|------------|
| **A — Embedded dataset** | Current design | Best DX; **requires** redistribution permission |
| **B — User-supplied dataset** | Code-only package | Avoids bundling; weaker DX; user still needs lawful source access |
| **C — Build-time downloader** | Fetch at build/runtime | Offline/CI fragility; terms/availability risk remain |
| **D — Separate data package** | Split code vs data IDs | Does **not** solve a redistribution prohibition |
| **E — No NuGet data package** | Hold publish | Safest if permission denied |

### NuGet release decision

**WAIT FOR DATA PERMISSION**

Not used: READY FOR FIRST NUGET PRERELEASE; REQUIRES PACKAGE/DATA ARCHITECTURE CHANGE (architecture change only if permission fails and Option A is abandoned).

---

## Permission requests

| Organization | Required? |
|--------------|-----------|
| DCS | **YES** |
| MOHA | **YES** |
| Other (Gazette / Cabinet / PubAd / GIC) | **NO** (evidence only for current snapshot) |

Draft messages: [`docs/data-permission-request.md`](data-permission-request.md).

---

## Completion summary (Phase 5.1)

### DCS

- **Classification:** UNCLEAR  
- **Evidence:** Public Admin Division Codes Excel/PDF; no licence on page or in workbook; microdata policy not applicable as a grant; copyright footers reserve rights  
- **Conditions:** None located  
- **Commercial use:** Uncertain  
- **Attribution:** Good-practice citation only (no mandatory wording located)  
- **Remaining uncertainty:** NuGet redistribution of normalized derived data + commercial downstream use  

### MOHA

- **Classification:** UNCLEAR  
- **Evidence:** Public LIFe reports; no reuse licence; All Rights Reserved ministry chrome; access ≠ redistribution  
- **Conditions:** None located  
- **Commercial use:** Uncertain  
- **Attribution:** Good-practice citation only  
- **Remaining uncertainty:** Redistribution of Si/Ta names in NuGet + commercial downstream use  

### Other production sources

- DCS counts: UNCLEAR (validation only)  
- Gazette / Cabinet / PubAd / GIC: N/A (evidence only)  

### Package license

- MIT remains appropriate for **LankaLens code**  
- Bundled administrative data must be distinguished via README + `DATA-NOTICE.md` (not described as MIT)  

### Required repository / package notices

- `docs/data-licensing-review.md` (this file)  
- `docs/data-permission-request.md`  
- `DATA-NOTICE.md` (repo + packed in `.nupkg`)  
- Updates to `docs/publishing.md`, `docs/data-sources.md`, root and package READMEs  

### Overall classification

**REDISTRIBUTION UNCLEAR**

### NuGet release decision

**WAIT FOR DATA PERMISSION**

**Do not publish.**
