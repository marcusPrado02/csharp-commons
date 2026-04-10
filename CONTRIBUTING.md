# Contributing to MarcusPrado Platform Commons

Thank you for your interest in contributing. This guide explains how to set up the development environment, the coding standards we enforce, and the process for submitting changes.

---

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0.103+ (`global.json` pins this) | Build and test |
| [Docker](https://www.docker.com/get-started) | 24+ | Integration tests via Testcontainers |
| [Git](https://git-scm.com/) | 2.40+ | Version control |

Optional but recommended:
- [CSharpier](https://csharpier.com/) formatter extension for your IDE
- [EditorConfig](https://editorconfig.org/) plugin for your editor

---

## Setup

```bash
git clone https://github.com/MarcusPrado/csharp-commons.git
cd csharp-commons

# Install local .NET tools (CSharpier, etc.)
dotnet tool restore

# Restore NuGet packages
dotnet restore

# Build in Release mode (same as CI)
dotnet build -c Release

# Run all tests (unit + integration — Docker required for integration)
dotnet test -c Release
```

---

## Development workflow

```
main                ← protected; requires CI pass + PR review
  └── feature/<ticket>-short-description   ← your branch
```

1. Branch from `main`: `git checkout -b feature/123-add-kafka-batch-consumer`
2. Make your changes following the standards below.
3. Run CI checks locally before pushing:

```bash
dotnet format --verify-no-changes --severity error   # Roslyn style
dotnet csharpier check .                              # formatter
dotnet test -c Release                               # all tests
dotnet nuget audit --severity moderate               # security scan
```

4. Push and open a PR against `main`.

---

## Coding standards

Standards are **enforced automatically** — CI will fail if they are not met.

### Code style

- **CSharpier** (v1.2.6): run `dotnet csharpier .` to auto-format.
- **Roslyn `.editorconfig`**: IDE0, SA*, CA*, RCS* rules. Run `dotnet format` to auto-fix.
- **TreatWarningsAsErrors** is enabled for all `src/` projects.

### Architecture rules

The [layer dependency rules](docs/architecture/layer-rules.md) are enforced by:

- **Roslyn analyzers** (PLATFORM001–005): fire at build time.
- **NetArchTest** (`MarcusPrado.Platform.ArchTests`): fire in the `test` CI stage.

In practice: **Core projects must never reference EF Core, ASP.NET Core, or messaging broker SDKs.** See [ADR-003](docs/architecture/adr/ADR-003-efcore-in-extension.md).

### Error handling

All operations that can fail in expected ways must return `Result<T>` — not throw exceptions. See [ADR-001](docs/architecture/adr/ADR-001-result-type.md).

### Commit messages

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(kafka): add batch consumer with exponential back-off
fix(efcore): resolve duplicate outbox dispatch on retry
docs(adr): add ADR-007 outbox/inbox pattern
chore(deps): bump Confluent.Kafka to 2.5.0
refactor(application): extract ValidationBehavior to own file
test(abstractions): add Result<T> map/bind round-trip tests
```

Prefixes: `feat`, `fix`, `docs`, `chore`, `refactor`, `test`, `perf`.

### XML documentation

All `public` types and members in `src/core/` and `src/extensions/` require XML doc comments. `CS1591` is currently a warning (not an error) but will be promoted to an error once coverage reaches 100 %.

---

## Adding a new package

Follow this checklist when adding a new NuGet package to the solution:

- [ ] Create the project under the correct layer (`src/core/`, `src/extensions/`, or `src/kits/`).
- [ ] Add it to `MarcusPrado.Platform.Commons.slnx`.
- [ ] Register any new NuGet dependencies in `Directory.Packages.props` — do **not** add a `Version` attribute to the `<PackageReference>` in the `.csproj`.
- [ ] Add a corresponding test project under `tests/unit/` or `tests/integration/`.
- [ ] Add an entry to the package table in `README.md`.
- [ ] Add a package doc section to the relevant file in `docs/packages/`.
- [ ] If the new package introduces an architectural decision, write an ADR first and get it reviewed.

---

## Architecture Decision Records (ADRs)

When a change introduces a **significant design decision**, open an ADR pull request before implementing:

1. Copy the template from [docs/architecture/adr/README.md](docs/architecture/adr/README.md).
2. Create `docs/architecture/adr/ADR-NNN-short-title.md`.
3. Add an entry to [docs/architecture/adr/README.md](docs/architecture/adr/README.md).
4. Open a PR with the ADR **before** the implementation PR.

An ADR is required when you are: choosing a new third-party library, changing the layer dependency rules, changing the pipeline behavior order, or making a decision that would be hard to reverse.

---

## Pull request process

1. CI must pass (build, test, analyze).
2. Fill in the [PR template](.github/pull_request_template.md) — especially the checklist.
3. One review from a maintainer is required.
4. Squash-merge into `main` (the merge button enforces this).

---

## Reporting issues

- **Bugs**: use the [bug report template](.github/ISSUE_TEMPLATE/bug_report.yml).
- **Feature requests**: use the [feature request template](.github/ISSUE_TEMPLATE/feature_request.yml).
- **Security vulnerabilities**: see [SECURITY.md](SECURITY.md) — **do not open a public issue**.
