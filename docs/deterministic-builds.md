# Deterministic builds

LankaLens sets `Deterministic=true` in `Directory.Build.props`.

On GitHub Actions, `ContinuousIntegrationBuild=true` is also set so path mapping is stable for Source Link.

## What we verified (Phase 5)

- Two consecutive Release packs with `ContinuousIntegrationBuild=true` produced identical runtime DLL SHA-256 hashes.
- Portable PDBs are shipped in `.snupkg`.
- Source Link maps to `raw.githubusercontent.com/poorna-soysa/LankaLens/<commit>/*` when the package is built from a git checkout with repository metadata available.

## What can prevent byte-for-byte package reproducibility

- Building without git metadata (empty repository commit in the nuspec; Source Link content may differ).
- Local builds without `ContinuousIntegrationBuild=true` (absolute paths embedded differently).
- Changing only package metadata (version, release notes) while keeping the same DLL.
- Non-deterministic NuGet packaging timestamps in some package metadata files (DLL content can still match).

Treat DLL content equivalence under CI settings as the primary reproducibility goal, not necessarily identical `.nupkg` zip bytes across machines.
