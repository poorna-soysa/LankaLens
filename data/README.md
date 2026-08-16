# Data directory

This folder holds authoritative source materials and generated artifacts for LankaLens administrative divisions.

## Layout

| Path | Purpose |
|------|---------|
| `source/` | Original DCS/MOHA files (`.xlsx`/`.pdf`/MOHA HTML gitignored until licensing is clear) |
| `source/moha-life/` | MOHA LIFe cache (`README.md` committed; reports/manifest gitignored) |
| `source/sources.json` | Machine-readable provenance (URL, hash, dates) — committed |
| `source/snapshot-expectations.json` | Expected counts and multilingual coverage for the current source snapshot — committed |
| `mappings/` | Confirmed MOHA→DCS code mappings and authoritative name overlays — committed |
| `generated/` | DataBuilder outputs including production `administrative-divisions.json`, validation, coverage, join, deltas, and unresolved gaps |

## DataBuilder

```bash
dotnet run --project tools/LankaLens.DataBuilder -- inspect
dotnet run --project tools/LankaLens.DataBuilder -- acquire-moha
dotnet run --project tools/LankaLens.DataBuilder -- validate
dotnet run --project tools/LankaLens.DataBuilder -- build
```

`acquire-moha` downloads official MOHA LIFe GN reports once (rate-limited, cached). `validate` / `build` never call MOHA; they read the local snapshot and apply confirmed mappings from `data/mappings/moha-to-dcs.json` plus overlays from `data/mappings/authoritative-name-overlays.json`.

Successful `build` writes `data/generated/administrative-divisions.json` and copies the same bytes into `src/LankaLens.AdministrativeDivisions/Data/` for embedding.

Place gitignored snapshots under `source/` using the filenames in `sources.json`. Do not silently overwrite an existing snapshot with a different SHA-256.

## Provenance policy

- Never invent geographic master data.
- Never silently replace a source file without updating provenance.
- If original files cannot be redistributed, store download URL, filename, retrieval date, SHA-256 hash, source organization, and effective/reference date instead.
- Production records must be traceable to an authoritative source.
- Unresolved Sinhala/Tamil values are emitted as JSON `null`, never placeholders.

See also:

- [`docs/data-sources.md`](../docs/data-sources.md)
- [`docs/contributing-data.md`](../docs/contributing-data.md)
