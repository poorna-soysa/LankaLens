# Data permission request drafts

> **Historical / research documentation (Phase 5.1).** These drafts document outreach considered during development. They are **not** a current automated or process publication gate. A later project decision was to proceed with publishing `LankaLens.AdministrativeDivisions` `0.1.0-preview.1` without waiting for separate written permission from DCS or MOHA. This document does **not** state that such permission was obtained or that these drafts were sent.

**Status:** Drafts only — **not sent**  
**Related review:** [`docs/data-licensing-review.md`](data-licensing-review.md)  
**Investigation date:** 2026-08-16

These messages request written clarification/permission for redistributing **normalized** Sri Lankan administrative codes and names in an independent open-source .NET NuGet package. They do not claim that public website availability already grants that right.

Do **not** send automatically from CI or tooling. Maintainers should review contacts, letterhead, and any local protocol before sending.

---

## 1. Draft — Department of Census and Statistics (DCS)

**Suggested To:** dgcensus@statistics.gov.lk  
**Suggested Cc:** data.requests@statistics.gov.lk; information@statistics.gov.lk  
**Subject:** Request for clarification — redistribution of published Administrative Division Codes in an open-source .NET library

```text
Dear Director General,

I am writing to request clarification and, if appropriate, written permission regarding reuse of administrative division data published by the Department of Census and Statistics (DCS).

Project overview
LankaLens.AdministrativeDivisions is an independent open-source .NET library (not a government product and not affiliated with or endorsed by the Government of Sri Lanka). It is intended for free distribution through NuGet.org. There is no charge for the package. Downstream users may include open-source and commercial software.

Source used
We use the publicly published Administrative Division Codes workbook available from the DCS website, including:

- Page: https://www.statistics.gov.lk/qlink/AdminDivCode
- Excel: https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel
- Dataset reference: GNDList_Final.xlsx / Master Registry of Census Blocks (MRCB-2024), as at 19 March 2024
- Our retrieval date for the current snapshot: 16 August 2026

What we distribute
We do not redistribute the original Excel or PDF files. We embed a normalized JSON-derived dataset inside the NuGet package, containing:

- Official administrative codes
- English names
- Province → District → Divisional Secretariat → Grama Niladhari hierarchy

Sinhala and Tamil names in the same package come primarily from Ministry of Home Affairs LIFe Location Codes (we are contacting MOHA separately). We cite DCS as the authority for codes, hierarchy, and English names.

Questions
1. May we redistribute this normalized administrative code, English-name, and hierarchy data in an open-source NuGet package?
2. May downstream users of that package include commercial / proprietary applications?
3. What attribution, if any, does DCS require?
4. Are there restrictions on combining DCS English codes/names with Sinhala/Tamil names from other official government sources in the same derived dataset?
5. If permission is granted, may we quote or link to your written reply in our public project documentation?

We will continue to state that LankaLens is independent and is not an official DCS product. We welcome any preferred citation wording or conditions.

Thank you for your time and guidance.

Yours faithfully,
Poorna Soysa
Maintainer, LankaLens
https://github.com/poorna-soysa/LankaLens
```

---

## 2. Draft — Ministry of Home Affairs (MOHA) / Home Affairs Division

**Suggested To:** info@moha.gov.lk  
**Suggested attention:** Home Affairs Division (IT Unit) — LIFe Location Codes  
**Subject:** Request for clarification — redistribution of LIFe Location Code names in an open-source .NET library

```text
Dear Sir/Madam,

I am writing to request clarification and, if appropriate, written permission regarding reuse of administrative location names published through the Ministry of Home Affairs LIFe Location Codes application.

Project overview
LankaLens.AdministrativeDivisions is an independent open-source .NET library (not a government product and not affiliated with or endorsed by the Government of Sri Lanka). It is intended for free distribution through NuGet.org. There is no charge for the package. Downstream users may include open-source and commercial software.

Source used
We obtain Grama Niladhari division lists from the official LIFe application:

- UI: http://moha.gov.lk:8090/lifecode/
- Official generated GN reports: POST /lifecode/views/rpt_gn_list.php (per district)
- Our retrieval date for the current snapshot: 16 August 2026

Acquisition is a one-time, rate-limited snapshot of the same reports available through the public UI. We do not commit the original HTML reports to our public repository.

What we distribute
We do not redistribute the original HTML reports. We embed a normalized JSON-derived dataset inside the NuGet package. From LIFe we primarily use:

- Sinhala names
- Tamil names

joined to Department of Census and Statistics (DCS) administrative codes and English names (DCS permission is being requested separately). We cite MOHA LIFe as the source of authoritative Sinhala/Tamil names where available.

Questions
1. May we redistribute these normalized Sinhala and Tamil administrative names in an open-source NuGet package?
2. May downstream users of that package include commercial / proprietary applications?
3. What attribution, if any, does the Ministry require?
4. Are there restrictions on automated one-time retrieval of the official GN reports for the purpose of building such a derived offline dataset?
5. If permission is granted, may we quote or link to your written reply in our public project documentation?

We will continue to state that LankaLens is independent and is not an official Ministry product. We welcome any preferred citation wording or conditions.

Thank you for your time and guidance.

Yours faithfully,
Poorna Soysa
Maintainer, LankaLens
https://github.com/poorna-soysa/LankaLens
```

---

## Notes for maintainers

- Prefer official letterhead or a clear personal/professional identity when sending.
- Keep copies of any reply with the repository’s release records (without committing secrets).
- **Historical recommendation (Phase 5.1):** if either agency declines or does not respond, do not treat silence as permission; see release alternatives evaluated in [`docs/data-licensing-review.md`](data-licensing-review.md). That recommendation is research context only and is not a current publication gate.
- Do not remove attribution while awaiting a reply.
