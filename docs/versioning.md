# Versioning

LankaLens uses [Semantic Versioning](https://semver.org/).

## Package version vs dataset version

These are separate:

| Concept | Example | Meaning |
|---------|---------|---------|
| NuGet package version | `0.1.0-preview.1` | Library API / packaging release |
| Dataset version | `2024-03-19` | Administrative data snapshot date / identifier |

Do not tie them together unnecessarily. Data updates should be noted under a **Data** section in `CHANGELOG.md`.

## Pre-1.0

Use `0.x` (and prerelease labels such as `preview.N`) while the public API and dataset design are evolving. Do not publish `1.0.0` until the API and authoritative dataset are validated.

The planned first public version is **`0.1.0-preview.1`**.

## API compatibility baseline

The runtime project enables .NET SDK [Package Validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview) (`EnablePackageValidation=true`).

A committed baseline package lives at:

`eng/api-compat/LankaLens.AdministrativeDivisions.0.1.0-preview.1.nupkg`

When that file is present, packing validates the current public API against it via `PackageValidationBaselinePath`.

### Approving an intentional breaking change

1. Confirm the change is deliberate and update `CHANGELOG.md` / SemVer accordingly.
2. Either:
   - **Regenerate the baseline:** pack the new version, copy the new `.nupkg` into `eng/api-compat/`, and update `PackageValidationBaselinePath` / filename; or
   - **Suppress temporarily:** set `ApiCompatGenerateSuppressionFile=true`, review and commit `CompatibilitySuppressions.xml`, then clear the generate flag.
3. After the package is published to NuGet.org, prefer `PackageValidationBaselineVersion` (feed-based) over a committed local nupkg when practical.

Do not ignore ApiCompat failures casually. Suppressions must document why a break is accepted.
