# Publishing

## Intended release flow

```text
Development
  → PR
  → CI
  → Merge main
  → Create version tag / GitHub Release
  → Publish workflow
  → NuGet.org Trusted Publishing (OIDC)
```

## Rules

- Do not publish packages from ordinary pull-request CI runs.
- Prefer NuGet.org Trusted Publishing with OIDC; do not rely on long-lived API keys when Trusted Publishing is available.
- Never store credentials in source control.
- Do not publish placeholder repository URLs or temporary package icons.

## Status

Phase 1 includes CI only (`ci.yml`). The NuGet publish workflow is deferred to Phase 8.
