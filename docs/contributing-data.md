# Contributing data

Administrative-data corrections must include:

| Field | Required |
|-------|----------|
| Administrative level | Yes |
| Official code | Yes |
| Existing value | Yes |
| Proposed value | Yes |
| Language (English / Sinhala / Tamil) | Yes |
| Parent code | Yes (for non-province entities) |
| Authoritative source | Yes |
| Source date | Yes |
| Reason | Yes |

## Rules

- Do not merge corrections supported only by third-party websites (Wikipedia, Google Maps, etc.).
- Do not invent codes or translations.
- If sources disagree: document the conflict and require human review.
- Prefer DCS and other authoritative government publications.

## MOHA → DCS mapping file (`data/mappings/moha-to-dcs.json`)

Confirmed administrative code mappings only. DataBuilder validates this file and rejects:

- duplicate source or contradictory target mappings
- unknown MOHA source / DCS target codes
- missing evidence, evidence URL, reason, sourceId, or review note
- unsupported entity types
- `childPropagation=GnComponentUnchanged` when GN-component sets are not a bijection

Do **not** add speculative mappings based only on similar English names. Name similarity is discovery evidence, not proof. Sinhala/Tamil reuse requires `allowTranslationReuse: true` and evidence that the entities are the same.

## Authoritative name overlays (`data/mappings/authoritative-name-overlays.json`)

Use when Sinhala/Tamil for a **current DCS code** come from an authoritative government source that is not expressible as a MOHA→DCS code mapping (for example filtered MOHA DS labels under a known source inconsistency, or same-code MOHA DS labels when GN exact-join is empty).

Required per overlay: `type`, `dcsCode`, both `sinhala` and `tamil` (partial translations are rejected), `sourceOrganization`, `evidence`, `evidenceUrl`, `reviewNote`. Optional: `retrievedOrPublishedDate`.

Overlays never overwrite DCS English. Coverage counts an entity only when **both** Sinhala and Tamil are non-empty and script-valid.
