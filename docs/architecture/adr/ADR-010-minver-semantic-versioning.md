# ADR-010 — MinVer for Automatic Semantic Versioning from Git Tags

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-15 |
| **Deciders** | Marcus Prado Silva |

---

## Context

A NuGet library with ~40 packages needs a versioning strategy that:
- Produces valid SemVer 2.0 package versions without manual edits to `.csproj` files.
- Supports pre-release versions for packages on feature branches.
- Integrates with the CI pipeline without requiring a paid external service.

Options considered:

1. **Manual `<Version>` in each `.csproj`** — guarantees precision but requires a human to remember to bump 40 files before every release. Drift between package versions is common.
2. **GitVersion** — comprehensive but requires a configuration file and has non-obvious behaviour around branches and merge strategies.
3. **NerdBank.GitVersioning (NBGV)** — version is stored in `version.json` files; works well for large repos but adds a per-project file.
4. **MinVer** — reads the version from the most recent annotated git tag in the format `v{major}.{minor}.{patch}`. No config files. Pre-release versions are derived automatically as `{tag}-alpha.{height}`.

---

## Decision

Use **MinVer** (`MinVer` NuGet package added to `Directory.Build.props`).

Release process:
1. `git tag v1.2.0 -m "release v1.2.0"`
2. `git push origin v1.2.0`
3. CI workflow `release.yml` fires on tag push, runs `dotnet pack` (MinVer sets `PackageVersion` automatically), and pushes to NuGet.org and GitHub Packages.

`git-cliff` generates `CHANGELOG.md` from Conventional Commit messages between tags.

---

## Consequences

**Positive:**
- Zero per-project version configuration — a single tag versions all 40 packages simultaneously.
- Pre-release versions on `main` between tags look like `1.2.0-alpha.5`, clearly communicating instability.
- CI workflow is simple: tag → pack → push.

**Negative:**
- All packages share the same version number. Independent versioning (e.g., `Kafka` at `2.0` and `Core` at `1.5`) is not supported. For this library, coarse versioning is intentional — consumers update the whole platform together.
- Developers must remember that only **annotated** tags (`git tag -m`) are picked up by MinVer. Lightweight tags are ignored.

**Neutral:**
- `Directory.Build.props` sets `<MinVerSkip>true</MinVerSkip>` for test and sample projects so their assemblies don't get a NuGet version.
