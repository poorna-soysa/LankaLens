# LankaLens.AdministrativeDivisions 0.1.0-preview.1

**This is a prerelease.** The public API and dataset may still change before 1.0.

## Summary

Sri Lanka administrative divisions for .NET — offline, embedded, and searchable.

| Level | Count |
|-------|------:|
| Provinces | 9 |
| Districts | 25 |
| Divisional Secretariats (DS) | 340 |
| Grama Niladhari divisions (GN) | 14,008 |

## Highlights

- **English coverage complete** for all bundled divisions
- **Sinhala and Tamil** names included where authoritatively verified (unresolved GN localized values remain `null`)
- **Offline embedded dataset** — no network calls required at runtime
- **Multilingual search** (English, Sinhala, Tamil)
- **Hierarchy navigation** Province → District → DS → GN

## Install

```bash
dotnet add package LankaLens.AdministrativeDivisions --version 0.1.0-preview.1
```

## Notes

- Software is MIT-licensed; bundled administrative data is **not** MIT — see `DATA-NOTICE.md` in the package.
- LankaLens is an independent open-source project and is **not** an official government product.
- First public NuGet.org preview of `LankaLens.AdministrativeDivisions`.
