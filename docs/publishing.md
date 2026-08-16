# Publishing

## Intended release flow

```text
Development
  → PR
  → CI (restore, build, test, pack, validate — no push)
  → Merge main
  → Create GitHub Release / tag vX.Y.Z-preview.N (prerelease)
  → publish.yml verify job (restore, build, test, pack, safety dump)
  → GitHub Environment nuget-publish approval
  → NuGet.org Trusted Publishing (OIDC) → push .nupkg + .snupkg
  → Post-publish nuget.org smoke test
```

Manual backup: `workflow_dispatch` with `confirm=publish` (same gates apply).

## Rules

- Do **not** publish packages from ordinary pull-request or push CI runs.
- Prefer **NuGet.org Trusted Publishing** with GitHub Actions OIDC. Do **not** use a long-lived NuGet API key when Trusted Publishing is available.
- Never store credentials in source control.
- Do not publish placeholder repository URLs or temporary package icons.
- Do not publish `1.0.0` until API and data readiness are affirmed.
- Require explicit environment approval before the NuGet.org push step.

## Technical release gates

Before pushing to NuGet.org, confirm:

- Release build passes
- Tests pass
- Package validation passes
- Package contents inspected
- `DATA-NOTICE.md` included in the package
- Correct version (for this release: `0.1.0-preview.1`)
- Correct repository metadata
- Trusted Publishing configured
- Release intentionally approved (`nuget-publish` environment)

## Workflows

| Workflow | Purpose | Publishes? |
|----------|---------|------------|
| `.github/workflows/ci.yml` | Restore, build, test, pack, validate, smoke | No |
| `.github/workflows/publish.yml` | Release/tag or confirmed dispatch → Trusted Publishing push | Only after environment approval |

## Trusted Publishing setup (human steps)

Complete these **before** the first real publish:

1. **nuget.org Trusted Publishing policy**
   - Repository Owner: `poorna-soysa`
   - Repository: `LankaLens`
   - Workflow File: `publish.yml` (filename only)
   - Environment: `nuget-publish`
2. **GitHub Actions secret** `NUGET_USER` = your nuget.org **username** (profile name, not email, not an API key)
3. **GitHub Environment** `nuget-publish` with required reviewers (interactive confirmation gate)

Official guidance: [Trusted Publishing on nuget.org](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

## Secrets / permissions

| Name | Purpose |
|------|---------|
| `NUGET_USER` | nuget.org username for `NuGet/login@v1` OIDC exchange |
| Job permission `id-token: write` | Allows GitHub OIDC token issuance for Trusted Publishing |
| Environment `nuget-publish` | Required reviewer approval before push |

Do **not** configure `NUGET_API_KEY` for this workflow.

## Status

Phase 6 prepares Trusted Publishing, release notes, and verification for **`0.1.0-preview.1`**.

Package data notice: [`DATA-NOTICE.md`](../DATA-NOTICE.md).  
Draft release notes: [`docs/release-notes-0.1.0-preview.1.md`](release-notes-0.1.0-preview.1.md).

### Release checklist (this version)

1. Merge to `main`; confirm CI green.
2. Create prerelease: `gh release create v0.1.0-preview.1 --prerelease --notes-file docs/release-notes-0.1.0-preview.1.md`
3. Approve the `nuget-publish` environment when the safety dump looks correct.
4. Run `./scripts/smoke-nuget-org-package.ps1` against nuget.org (not a local feed).
5. Verify the nuget.org package page (title, README, license, repository, symbols, prerelease label).
