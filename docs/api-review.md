# Public API review (Phase 5)

Review date: 2026-08-16  
Package: `LankaLens.AdministrativeDivisions`  
Namespace: `LankaLens.AdministrativeDivisions`  
Target framework: `net8.0`

This review documents the intentional public surface before the first public NuGet prerelease. Changes after this point should be deliberate and tracked by package validation.

## Expected public surface

| Type | Kind | Role |
|------|------|------|
| `AdministrativeDivisions` | static class | Entry point (`Default`) |
| `IAdministrativeDivisionProvider` | interface | Read-only lookup, hierarchy, search |
| `LocalizedName` | sealed record | English + optional Sinhala/Tamil |
| `Province` | sealed record | Province entity |
| `District` | sealed record | District entity |
| `DivisionalSecretariat` | sealed record | DS entity |
| `GramaNiladhariDivision` | sealed record | GN entity |
| `DatasetMetadata` | sealed record | Snapshot provenance |
| `Language` | enum | English, Sinhala, Tamil |
| `AdministrativeDivisionType` | enum | Province … GramaNiladhariDivision |
| `AdministrativeDivisionSearchOptions` | sealed record | Search filters |
| `AdministrativeDivisionSearchResult` | sealed record | Search hit |

No additional public types are intended for the first prerelease.

## Decisions — keep as-is

| Topic | Decision | Reason |
|-------|----------|--------|
| `AdministrativeDivisions.Default` | Keep | Idiomatic singleton entry; lazy, thread-safe |
| Public model constructors | Keep | Idiomatic records; useful for tests and value construction |
| Nullable `LocalizedName.Sinhala` / `Tamil` | Keep | Accurately models missing authoritative values |
| `DateOnly` on `DatasetMetadata` | Keep | Clean modern API; acceptable with `net8.0` |
| Lookup pair (`Get*ByCode` / `TryGet*`) | Keep | Matches common .NET patterns; `[NotNullWhen(true)]` is correct |
| Search without exposed match rank | Keep for now | Additive later if consumers need it |
| Case-sensitive ordinal code lookups | Keep | Matches authoritative codes; document clearly |
| No input trimming | Keep | Avoid silent normalization of codes |

## Documented behavioral contracts

- Code lookups use ordinal, case-sensitive comparison. Inputs are not trimmed.
  Whitespace-only codes throw; padded codes such as `" 1 "` do not match `"1"` and return unknown.
- Unknown entity codes: `Get*ByCode` returns `null`; `TryGet*` returns `false`.
- Unknown but non-empty parent codes on hierarchy filters return an empty sequence.
- Null required arguments → `ArgumentNullException`.
- Empty or whitespace required arguments → `ArgumentException`.
- Invalid `MaxResults` (≤ 0) → `ArgumentOutOfRangeException`.
- Search ranking: exact → prefix → contains; then division type, English name, code.
- `MaxResults` null means unbounded results.
- The provider and returned collections are immutable and safe for concurrent reads after construction.
- `Default` initialization uses `LazyThreadSafetyMode.ExecutionAndPublication`.

## Explicitly not changing (premature / stylistic)

- Case-insensitive code lookup
- Default `MaxResults` cap
- Hiding public model constructors
- Exposing search match quality on `AdministrativeDivisionSearchResult`
- Filtering internal types from the shipped XML documentation file (IDE already scopes to public members)

## 1.0 regret check

> If this API were published as 1.0.0 today, is there anything we would immediately regret?

**No breaking redesign is required before a prerelease.**

The structural lock-in is `net8.0` plus public `DateOnly`. That is accepted: the library is intended for modern .NET, and multi-targeting `netstandard2.0` would force compatibility packages or API compromises.

Minor future additive improvements (search rank, optional result limits defaults) can ship without breaking existing callers.

## Nullability summary

| Member | Annotation | Correct? |
|--------|------------|----------|
| `LocalizedName.Sinhala` / `Tamil` | `string?` | Yes |
| `Get*ByCode` | `T?` | Yes |
| `TryGet*` out parameters | `[NotNullWhen(true)] out T?` | Yes |
| Search options `Language` / `Type` / `MaxResults` | nullable | Yes |
| Search result parent codes | `string?` | Yes (level-dependent) |
| `DatasetMetadata.SourceVersion` / `EffectiveDate` | nullable | Yes |

## Framework recommendation (companion to this review)

Keep **`net8.0` only**. Do not multi-target `netstandard2.0` for the first release.
