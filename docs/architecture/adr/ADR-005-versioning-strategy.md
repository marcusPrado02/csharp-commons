# ADR-005 — Versioning strategy: MinVer + Central Package Management

> **Summary**: Package versions are derived automatically from Git tags using
> MinVer, eliminating manual version bumps. Dependency versions across all
> projects are managed centrally via `Directory.Packages.props` (CPM), ensuring
> every project in the solution uses the same dependency version with no drift.

| Field | Value |
|-------|-------|
| **Status** | Accepted |
| **Date** | 2026-03-10 |
| **Author** | Marcus Prado Silva (Platform Architect) |
| **Tags** | versioning, build, nuget, ci-cd, developer-experience |
| **Supersedes** | — |
| **Superseded by** | — |

---

## Context

A platform library that ships ~70 NuGet packages faces two versioning problems:

### Problem 1 — Package version drift

Without coordination, `MarcusPrado.Platform.EfCore` can be on `1.3.0` while
`MarcusPrado.Platform.Postgres` (which depends on it) is still on `1.2.0`.
Consumers end up with `NU1605` warnings and unpredictable dependency resolution.

The platform's packages must move together: one tag → all packages at the same
version.

### Problem 2 — Dependency version drift

Without central management, `PackageReference` versions are scattered across
dozens of `.csproj` files. Experience from pre-CPM repositories:

- `Serilog` at `3.0.1` in five projects, `3.1.0` in two others.
- `xUnit` on two minor versions simultaneously.
- `Microsoft.EntityFrameworkCore` bumped in `EfCore.csproj` but forgotten in
  `Postgres.csproj`, causing a runtime `FileLoadException` in integration tests.

Manual audits to find drift are time-consuming and error-prone.

---

## Decision

### Part A — Package versioning: MinVer

All shippable packages (`src/core/`, `src/extensions/`, `src/kits/`,
`src/tooling/`) reference MinVer as a `PrivateAssets="all"` build-time tool.

```xml
<!-- Directory.Build.props — applied to all shippable projects -->
<ItemGroup Label="Versioning"
  Condition="$(MSBuildProjectDirectory.Contains('/src/core/')) Or
             $(MSBuildProjectDirectory.Contains('/src/extensions/')) Or
             $(MSBuildProjectDirectory.Contains('/src/kits/'))       Or
             $(MSBuildProjectDirectory.Contains('/src/tooling/'))">
  <PackageReference Include="MinVer" PrivateAssets="all" />
</ItemGroup>
```

MinVer reads the nearest Git tag on the current commit (or an ancestor) and
derives the version automatically:

| Git state | MinVer output |
|-----------|--------------|
| `HEAD` is tag `v1.2.3` | `1.2.3` |
| 4 commits after `v1.2.3` | `1.2.4-alpha.0.4` (pre-release) |
| No tag in history | `0.0.0-alpha.0.<commits>` |

### Release workflow

```bash
# Cut a release — all 70 packages get version 1.3.0
git tag v1.3.0
git push origin v1.3.0
# → CI pack step: dotnet pack -p:PackageVersion=1.3.0 (tag-derived in CI)
# → CI publish: pushes to NuGet.org + GitHub Packages
```

Pre-release packages on `main` use `0.0.0-preview.<run_number>`:

```yaml
# .github/workflows/ci.yml — pack job
- name: Determine package version
  env:
    GIT_REF: ${{ github.ref }}
    RUN_NUMBER: ${{ github.run_number }}
  run: |
    if [[ "$GIT_REF" == refs/tags/v* ]]; then
      VERSION="${GITHUB_REF_NAME#v}"
    else
      VERSION="0.0.0-preview.$RUN_NUMBER"
    fi
```

### Part B — Dependency versioning: Central Package Management

`Directory.Packages.props` at the repository root declares every NuGet
dependency version exactly once. Projects use `<PackageReference>` without a
`Version` attribute; CPM injects the version from `Directory.Packages.props`.

```xml
<!-- Directory.Packages.props (excerpt) -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup Label="Platform runtime">
    <PackageVersion Include="Microsoft.EntityFrameworkCore"  Version="9.0.3" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Relational" Version="9.0.3" />
    <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.3" />
    <PackageVersion Include="StackExchange.Redis"            Version="2.8.0" />
    <PackageVersion Include="Confluent.Kafka"                Version="2.5.0" />
    <PackageVersion Include="OpenTelemetry"                  Version="1.9.0" />
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.9.0" />
    <PackageVersion Include="Serilog"                        Version="4.1.0" />
    <PackageVersion Include="FluentValidation"               Version="11.10.0" />
  </ItemGroup>

  <ItemGroup Label="Testing">
    <PackageVersion Include="xunit"                          Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio"      Version="2.8.2" />
    <PackageVersion Include="FluentAssertions"               Version="7.0.0" />
    <PackageVersion Include="Testcontainers"                 Version="3.10.0" />
    <PackageVersion Include="NSubstitute"                    Version="5.3.0" />
    <PackageVersion Include="Bogus"                          Version="35.6.1" />
  </ItemGroup>

  <ItemGroup Label="Build tools">
    <PackageVersion Include="MinVer"                         Version="6.0.0" />
    <PackageVersion Include="CSharpier"                      Version="1.2.6" />
    <PackageVersion Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0" />
    <PackageVersion Include="StyleCop.Analyzers"             Version="1.2.0-beta.556" />
  </ItemGroup>
</Project>
```

```xml
<!-- Consuming project — version is NOT specified here -->
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
</ItemGroup>
```

### NuGet cache key

The CI cache key uses `Directory.Packages.props` so that any version bump
automatically invalidates the cache and re-downloads packages:

```yaml
- uses: actions/cache@v4
  with:
    path: ${{ env.NUGET_PACKAGES }}
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/Directory.Packages.props') }}
```

---

## Consequences

### Positive

- **Zero manual version bumps for packages** — `git tag v1.x.y` is the only
  required human action to produce a new release. No `.csproj` edits needed.
- **Uniform dependency versions** — `dotnet list package --outdated` on any
  project reflects the global state; one PR updates all 70+ projects at once.
- **`NU1605` eliminated** — CPM ensures every project in the solution resolves
  the same transitive version, removing downgrade warnings.
- **Reproducible builds** — MinVer's version output is deterministic from a
  given commit + tag; no floating version suffixes in production packages.
- **Reviewable upgrades** — dependency bumps produce a one-line diff in
  `Directory.Packages.props`, visible in every PR.

### Negative / Trade-offs

- **CPM adoption cost** — existing projects must remove `Version` attributes
  from all `<PackageReference>` elements. This is a one-time migration.
- **Pinned transitive versions require `<PackageVersion OverrideVersion="true">`**
  — CPM's default is to manage direct dependencies only; transitive pinning
  requires explicit opt-in.
- **`dotnet add package` appends `Version`** — the `dotnet add package` CLI
  still writes a `Version` attribute; it must be removed manually after use
  and the version added to `Directory.Packages.props`.

---

## Alternatives Considered

| Alternative | Reason rejected |
|-------------|-----------------|
| Manual `<Version>` in each `.csproj` | Error-prone; 70+ files to update per release; version drift is inevitable |
| `GitVersion` | More complex configuration (`GitVersion.yml`); branch-naming conventions required; MinVer is simpler for trunk-based development |
| `Nerdbank.GitVersioning` | Requires a `version.json` per project; good for monorepos with independent versioning, overkill here |
| `PackageReference` with `Version` in props imports | Equivalent to CPM but not the official SDK-supported mechanism; no tooling support |
| Renovate / Dependabot for dependency updates | Complementary, not a replacement; CPM makes these PRs single-file changes |

---

## References

- [MinVer — GitHub](https://github.com/adamralph/minver)
- [NuGet Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
- [Semantic Versioning 2.0.0](https://semver.org/)
- [MinVer — version calculation rules](https://github.com/adamralph/minver#how-it-works)
- `Directory.Packages.props` — root-level version registry
- `Directory.Build.props` — MinVer `PackageReference` applied to all shippable projects
- `.github/workflows/ci.yml` — pack/publish jobs that derive version from tag
