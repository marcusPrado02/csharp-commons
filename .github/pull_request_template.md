## Summary

<!-- 1–3 bullet points describing what this PR does and why -->

-
-

## Type of Change

- [ ] Bug fix
- [ ] New feature / package
- [ ] Breaking change (changes existing public API)
- [ ] Documentation
- [ ] Refactor / internal improvement
- [ ] CI / tooling

## Checklist

### Code
- [ ] `dotnet build` passes with zero warnings (`TreatWarningsAsErrors` is enabled)
- [ ] `dotnet test` passes — all existing tests green, new tests added for new behavior
- [ ] `dotnet csharpier . --check` passes (no formatting violations)
- [ ] No new suppressions of `PLATFORM001`–`PLATFORM005` analyzer rules without justification

### Tests
- [ ] New public API is covered by unit tests
- [ ] Infrastructure adapters (EF Core, Redis, Kafka, etc.) are covered by integration tests using Testcontainers
- [ ] Edge cases and failure paths are tested (e.g., `Result.Failure`, exception paths)

### Documentation
- [ ] XML doc comments added/updated on all new public types and members
- [ ] If a new package was added: entry added to `docs/packages/` and `README.md` packages table
- [ ] If a significant architectural decision was made: ADR opened or updated in `docs/architecture/adr/`
- [ ] `CHANGELOG.md` entry added under `[Unreleased]` (user-facing changes only)

### Breaking Changes
- [ ] If this is a breaking change: `BREAKING CHANGE:` footer added to the commit message
- [ ] Existing consumers have a migration path documented in the PR description

## Related Issues

<!-- Closes #xxx -->
