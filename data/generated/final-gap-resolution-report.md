# Final gap resolution report (Phase 3.8)

Authoritative multilingual gap investigation. DCS English/codes remain canonical.
No AI translation. No production JSON. Runtime library unchanged.

## Coverage summary

| Level | Before 3.8 (projected) | After 3.8 (projected) | Total |
| --- | --- | --- | ---: |
| Province | 9 / 9 | 9 / 9 | 9 |
| District | 25 / 25 | 25 / 25 | 25 |
| DS | 336 / 336 | 340 / 340 | 340 |
| GN | 13722 / 13722 | 13723 / 13723 | 14008 |

Applied DS mappings: 8; GN mappings: 1; child propagations: 146; overlays: 3

## Exact uncovered DS before Phase 3.8 mappings/overlays

The 336/340 projected DS figure left exactly four current DCS DS without both Si and Ta:

| DCS | English | Cause |
| --- | --- | --- |
| 2302 | Kothmale West | Unmatched DS code vs MOHA `2304` |
| 2314 | Norwood | Unmatched DS code vs MOHA `2316` |
| 5221 | Kalmunai North Sub | No MOHA `5221`; Kalmunai North rows parked on LIFe `5225` |
| 3136 | Hikkaduwa | Same DS code, but zero GN exact-joins after 2019 renumbering |

Note: DS `5225` Sainthamaruthu is code-matched and already receives Si/Ta from matched-row English narrowing; the raw MOHA `TRANSLATION_CONFLICT` remains. Phase 3.8 still records an explicit filtered overlay for provenance clarity.

## Ratmalana (MOHA `1139` / DCS `1131`)

- **Problem:** MOHA retains Mount Lavinia under LIFe DS segment `39` (`1-1-39-005`) while DCS places Mount Lavinia at `1131005` under Ratmalana `1131`. Other Ratmalana GNs already exact-join under `1131`. MOHA DS label on the Mount Lavinia row is `31: …/ Ratmalana` (label prefix matches DCS `1131`, life segment lags).
- **Evidence:** DCS GNDList places Mount Lavinia under Ratmalana. MOHA English is Mount Lavinia with Sinhala `ගල්කිස්ස` and Tamil `மனிட்லாவனியா`. MOHA DS label names Ratmalana with numeric prefix 31. GIC lists Ratmalana DS. Dual-authority identity for the GN; LIFe DS segment `39` is source lag, not a second DS.
- **Decision:** CONFIRMED GN mapping `1139005` → `1131005` (parent transfer / LIFe segment lag). No DS mapping `1139`→`1131` (target `1131` already exists in both sources).
- **Mapping:** 1139005 → 1131005
- **Sinhala/Tamil source:** MOHA LIFe row for `1-1-39-005`
- **GNs resolved:** 1
- **Remaining unresolved under this DS:** 0

## Kothmale West (`2302`) / MOHA `2304`

- **Problem:** Same district, compatible English (`Kothmale West` / `Kothmale (West)`), equal GN count 49/49, but GN-component sets differ (not `GnComponentUnchanged`).
- **Evidence:** Cabinet decision 2019-05-07 established Kotmale West as a new DS in Nuwara Eliya. DCS lists Kothmale West `2302`. MOHA lists Kothmale (West) `2304` with Si/Ta. English GN membership overlaps strongly (38 exact + 11 spelling variants) but components are renumbered — discovery for GNs only.
- **Decision:** CONFIRMED DS mapping `2304` → `2302` without child propagation (DS-level Si/Ta reuse only). GN children remain UNRESOLVED pending Gazette/DS GN code lists.
- **Mapping:** 2304 → 2302 (no child propagation)
- **Sinhala/Tamil source:** MOHA DS label on `2304` rows; Cabinet Office 2019-05-07
- **GNs resolved:** 0
- **Remaining unresolved under this DS:** 49

## Norwood (`2314`) / MOHA `2316`

- **Problem:** Same district, exact English Norwood, equal GN count 35/35, different GN components.
- **Evidence:** Cabinet 2019-05-07 established Norwood DS. DCS `2314` / MOHA `2316`. English GN sets show substantial spelling drift; components differ — not a bijection.
- **Decision:** CONFIRMED DS mapping `2316` → `2314` without child propagation. GN children UNRESOLVED.
- **Mapping:** 2316 → 2314 (no child propagation)
- **Sinhala/Tamil source:** MOHA DS label on `2316` rows; Cabinet Office 2019-05-07
- **GNs resolved:** 0
- **Remaining unresolved under this DS:** 35

## Kalmunai North Sub (`5221`)

- **Problem:** DCS lists Kalmunai North Sub with 29 GNs; MOHA has no HierarchicalDsCode `5221`. Kalmunai North Sub Office labels appear on LIFe `5-2-25` mixed with Sainthamaruthu.
- **Evidence:** PubAd Ampara list names Kalmunai North and Saindamarudu as separate DS. MOHA Kalmunai North Sub Office rows provide Si/Ta for the named office. GN English sets resemble DCS `5221` but LIFe codes (`5225150+`) do not match DCS `5221*` — English similarity alone is insufficient for GN mappings.
- **Decision:** CONFIRMED authoritative name overlay for DS `5221` Si/Ta from MOHA Kalmunai North Sub Office labels. GN records UNRESOLVED.
- **Mapping:** (overlay) 5221
- **Sinhala/Tamil source:** MOHA filtered DS labels; PubAd Ampara DS list
- **GNs resolved:** 0
- **Remaining unresolved under this DS:** 29

## DS `5225` Sainthamaruthu

- **Problem:** MOHA LIFe `5-2-25` mixes Sainthamarathu and Kalmunai North Sub Office labels → raw TRANSLATION_CONFLICT. DCS English is Sainthamaruthu; 17 Sainthamaruthu GNs exact-join by code.
- **Evidence:** PubAd lists Saindamarudu separately from Kalmunai North. MOHA Sainthamaruthu-labelled rows agree on Si `සෙයින්තමරතු` and Ta `சாய்ந்தமருது`. Resolution uses filtered overlay — not majority vote, not first-row, not English similarity alone.
- **Decision:** RESOLVED via authoritative name overlay for DCS `5225` (Sainthamaruthu-labelled MOHA rows only). Raw MOHA conflict retained in join reports.
- **Mapping:** (overlay) 5225
- **Sinhala/Tamil source:** MOHA Sainthamaruthu-labelled rows; PubAd Ampara DS list
- **GNs resolved:** 0
- **Remaining unresolved under this DS:** 0

## Hikkaduwa (`3136`)

- **Problem:** DCS and MOHA share DS code 3136 (Hikkaduwa), but all 27 GN codes fail exact LIFe join after the 2019 split/renumbering, so matched-row DS aggregation has no Si/Ta.
- **Evidence:** Cabinet 2019-05-07 upgraded Hikkaduwa into Hikkaduwa/Ratgama/Madampagama. MOHA LIFe still emits a single agreed DS label on all 3-1-36-* rows (හික්කඩුව / ஹிக்கடுவை / Hikkaduwa).
- **Decision:** CONFIRMED authoritative name overlay for DS `3136`. GN children UNRESOLVED (English/component discovery only).
- **Mapping:** (overlay) 3136
- **Sinhala/Tamil source:** MOHA DS labels on HierarchicalDsCode 3136 rows
- **GNs resolved:** 0
- **Remaining unresolved under this DS:** 27

## Residual GN clusters

Outside confirmed DS recodes, residual unmatched GNs were classified. Same-DS English matches with different GN components (Mathurata, Nildandahinna, Thalawakelle, Hikkaduwa, Baddegama, Balangoda, etc.) remain **UNRESOLVED**: English similarity is discovery only; Cabinet 2019 establishes named DS splits but does not publish GN code renumbering tables used here.

| Classification | Treatment |
| --- | --- |
| GN code change (same DS, different component, English match) | Discovery only — UNRESOLVED without Gazette GN list |
| GN parent transfer | Only Mount Lavinia confirmed; others lack dual-authority code evidence |
| DS split residue (Hikkaduwa / Baddegama / Balangoda) | UNRESOLVED pending Gazette GN assignment lists |
| Obsolete MOHA GN (Hanguranketha `2306`, Laggala-Pallegama `2224`, Weligama `3239`) | Documented as MOHA-only; not DCS targets |

## Confirmed mappings (Phase 3.7 frozen + Phase 3.8 additions)

| Type | Source | Target | Child propagation |
| --- | --- | --- | --- |
| DivisionalSecretariat | 3157 | 3134 | GnComponentUnchanged |
| DivisionalSecretariat | 3138 | 3135 | GnComponentUnchanged |
| DivisionalSecretariat | 3336 | 3325 | GnComponentUnchanged |
| DivisionalSecretariat | 4145 | 4104 | GnComponentUnchanged |
| DivisionalSecretariat | 5142 | 5104 | GnComponentUnchanged |
| DivisionalSecretariat | 5139 | 5110 | GnComponentUnchanged |
| DivisionalSecretariat | 2304 | 2302 | (none) |
| DivisionalSecretariat | 2316 | 2314 | (none) |
| GramaNiladhariDivision | 1139005 | 1131005 | (none) |

## Authoritative name overlays

| Type | DCS | Source organization | URL |
| --- | --- | --- | --- |
| DivisionalSecretariat | 5225 | Ministry of Home Affairs (MOHA LIFe) — Sainthamaruthu-labelled rows only; corroborated by Ministry of Public Administration Ampara DS list | https://pubad.gov.lk/web/index.php?Itemid=116&id=106&lang=en&option=com_content&view=article |
| DivisionalSecretariat | 5221 | Ministry of Home Affairs (MOHA LIFe) — Kalmunai North Sub Office-labelled rows; corroborated by Ministry of Public Administration Ampara DS list | https://pubad.gov.lk/web/index.php?Itemid=116&id=106&lang=en&option=com_content&view=article |
| DivisionalSecretariat | 3136 | Ministry of Home Affairs (MOHA LIFe) — HierarchicalDsCode 3136 Hikkaduwa rows | http://moha.gov.lk:8090/lifecode/views/rpt_gn_list.php |

## Gazette / government page evidence cards

| Organization | URL | Entity | What established | Date |
| --- | --- | --- | --- | --- |
| Cabinet Office | https://www.cabinetoffice.gov.lk/cab/index.php?Itemid=49&dID=9758&id=16&lang=en&option=com_content&view=article | Kotmale West, Norwood, Mathurata, Nildandahinna, Talawakale | Named new DS approved 2019-05-07 (not a LIFe code map) | 2019-05-07 |
| Ministry of Public Administration | https://pubad.gov.lk/web/index.php?Itemid=116&id=106&lang=en&option=com_content&view=article | Kalmunai North, Saindamarudu | Separate Ampara DS entities | retrieved 2026-08-16 |
| MOHA LIFe | http://moha.gov.lk:8090/lifecode/views/rpt_gn_list.php | Ratmalana / Sainthamaruthu / Kalmunai North Sub Office / Kothmale (West) / Norwood | Official Si/Ta labels on GN report rows | retrieved 2026-08-16 |
| DCS | https://www.statistics.gov.lk/qlink/AdminDivCodes_Excel | All current codes/English | Canonical hierarchy | 2024-03-19 |
| GIC | https://gic.gov.lk/gic/index.php/en/component/org/?id=513&task=org | Ratmalana DS | Official Ratmalana DS contact listing | retrieved 2026-08-16 |

## Remaining unresolved (grouped by DS)

- `2124` Rambukwella East: DS unresolved=False; GN unresolved=1
- `2209` Andawala: DS unresolved=False; GN unresolved=5
- `2302` Doruwadeniya: DS unresolved=False; GN unresolved=49
- `2307` Udawela Pathana: DS unresolved=False; GN unresolved=48
- `2310` Thibbatugoda South: DS unresolved=False; GN unresolved=32
- `2313` Nagasena: DS unresolved=False; GN unresolved=17
- `2314` Rosella: DS unresolved=False; GN unresolved=35
- `3106` Middaramulla: DS unresolved=False; GN unresolved=1
- `3127` Mahalapitiya: DS unresolved=False; GN unresolved=11
- `3136` Wellawatta: DS unresolved=False; GN unresolved=27
- `3330` Nakulugamuwa West: DS unresolved=False; GN unresolved=1
- `5112` Iyankerny Muslim: DS unresolved=False; GN unresolved=2
- `5115` Eravur 02A: DS unresolved=False; GN unresolved=1
- `5221` Periyaneelavanai 01B: DS unresolved=False; GN unresolved=29
- `6145` Moragolla: DS unresolved=False; GN unresolved=3
- `6148` Anukkanhena: DS unresolved=False; GN unresolved=3
- `6154` Theliyagonna: DS unresolved=False; GN unresolved=2
- `7145` Thambuttegama: DS unresolved=False; GN unresolved=1
- `8130` Welikadagama: DS unresolved=False; GN unresolved=1
- `9118` Rajawaka: DS unresolved=False; GN unresolved=9
- `9206` Palliporuwa: DS unresolved=False; GN unresolved=1
- `9212` Pallewela: DS unresolved=False; GN unresolved=1
- `9218` Ethnawala: DS unresolved=False; GN unresolved=5

Total unresolved records: 285

## Exact-join reminder

Exact-join DS Si/Ta: 330/330; GN: 13576/13576.
Projection applies mappings and overlays in memory only.

