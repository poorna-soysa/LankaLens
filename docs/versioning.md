# Versioning

LankaLens uses [Semantic Versioning](https://semver.org/).

## Package version vs dataset version

These are separate:

| Concept | Example | Meaning |
|---------|---------|---------|
| NuGet package version | `0.3.0` | Library API / packaging release |
| Dataset version | `2026-07-28` | Administrative data snapshot date / identifier |

Do not tie them together unnecessarily. Data updates should be noted under a **Data** section in `CHANGELOG.md`.

## Pre-1.0

Use `0.x` while the public API and dataset design are evolving. Do not publish `1.0.0` until the API and authoritative dataset are validated.
