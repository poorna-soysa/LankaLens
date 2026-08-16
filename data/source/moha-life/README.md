# MOHA LIFe snapshots

Raw MOHA LIFe GN reports are **not** committed. No open-data / redistribution licence was identified on the official portal.

## Reproduce the snapshot

From the repository root:

```bash
dotnet run --project tools/LankaLens.DataBuilder -- acquire-moha
```

This performs a rate-limited, cache-aware download of the official generated GN reports:

- UI: http://moha.gov.lk:8090/lifecode/
- Cascade IDs: `POST /lifecode/views/fetch.php` (`action=province`, `query=<provinceId>`)
- GN reports: `POST /lifecode/views/rpt_gn_list.php` (`province`, `district`)

Cached files:

- `reports/p{provinceId}-d{districtId}.html`
- `manifest.json` (per-file SHA-256 and combined hash)

`validate` and `build` never request MOHA. They read this cache only.

Use `--force` only when intentionally refreshing the snapshot. Do not repeatedly request the same district.
