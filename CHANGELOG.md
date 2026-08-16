# Changelog

All notable changes to **LankaLens.AdministrativeDivisions** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0/).

## [Unreleased]

### Added

- Phase 4 production canonical dataset generation and runtime embedding
- Internal `EmbeddedAdministrativeDivisionLoader` with fail-fast package-data errors
- Production integration tests against the embedded snapshot
- `data/source/snapshot-expectations.json` for versioned counts and coverage gates

### Changed

- `LocalizedName.Sinhala` and `LocalizedName.Tamil` are nullable (`null` = no verified authoritative value)
- `AdministrativeDivisions.Default` loads the embedded production dataset once (lazy, thread-safe)
- DataBuilder `build` materializes DCS + MOHA + mappings + overlays into canonical JSON
- README and data-sources documentation describe real multilingual coverage limitations

### Removed

- Runtime `DevelopmentDataset` / `DEV-*` fixture from the library (tests retain a synthetic fixture)

### Data

- Bundled snapshot: 9 provinces, 25 districts, 340 DS, 14,008 GN
- Sinhala/Tamil: Province/District/DS complete; GN 13,723/14,008 (285 documented unresolved)

## [0.1.0] - TBD

Bootstrap through Phase 4. Remains pre-1.0; not published to NuGet.org.

### Added

- Solution and project structure for the library, tests, DataBuilder, and samples
- Shared `Directory.Build.props`, `.editorconfig`, MIT license, and README
- Minimal GitHub Actions CI (restore, build, test, pack — no publish)
- Phase 2 public domain models, `IAdministrativeDivisionProvider`, and multilingual search
- Phase 3 DataBuilder CLI (`inspect` / `validate` / `build` / `acquire-moha`)
- Phase 3.5–3.8 multilingual source discovery, MOHA join, mappings, overlays, and gap documentation
