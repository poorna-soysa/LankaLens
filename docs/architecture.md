# Architecture

LankaLens.AdministrativeDivisions is a small offline reference-data library.

## Design goals

- **Embedded JSON** — the canonical dataset ships inside the assembly; consumers do not deploy a separate data file.
- **Immutable data** — consumers cannot mutate the bundled dataset.
- **One-time loading** — deserialize once with thread-safe lazy initialization.
- **Indexes** — O(1) code lookups and pre-built hierarchy collections.
- **No database** — reference data is bundled, not queried from SQL or NoSQL stores.
- **No network** — normal runtime use performs no HTTP requests.
- **No DI requirement** — install the package and call `AdministrativeDivisions.Default`.

## What this is not

This package does not require Clean Architecture layers, CQRS, DDD aggregates, or ASP.NET Core.

## Runtime

`AdministrativeDivisions.Default` loads `Data/administrative-divisions.json` from an embedded assembly resource via an internal loader, validates basic invariants, and builds Ordinal code/hierarchy indexes once.

## DataBuilder

`tools/LankaLens.DataBuilder` is a separate CLI that:

1. Parses DCS workbooks (codes, hierarchy, English)
2. Joins cached MOHA LIFe Sinhala/Tamil names
3. Applies confirmed mappings and authoritative overlays
4. Validates counts/coverage against `data/source/snapshot-expectations.json`
5. Writes `data/generated/administrative-divisions.json` and copies it into the runtime project for embedding

Unresolved GN Sinhala/Tamil values are emitted as JSON `null` and remain documented in `data/generated/unresolved-multilingual-gaps.json`.
