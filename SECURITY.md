# Security Policy

## Supported versions

| Version | Supported |
|---------|-----------|
| `0.x.x-preview.*` (main branch) | Actively developed |
| Older releases | Not supported |

Once `v1.0.0` is released, the two most recent minor versions will receive security patches.

---

## Reporting a vulnerability

**Please do not open a public GitHub issue for security vulnerabilities.**

Use [GitHub's private security advisory](https://github.com/MarcusPrado/csharp-commons/security/advisories/new) to report a vulnerability confidentially.

Include in your report:

1. **Description** — what the vulnerability is and which component is affected.
2. **Steps to reproduce** — a minimal reproduction case.
3. **Impact** — potential consequences (data exposure, privilege escalation, etc.).
4. **Suggested fix** — optional, but welcome.

---

## Response timeline

| Stage | Target |
|-------|--------|
| Acknowledgement | Within 72 hours |
| Initial assessment | Within 7 days |
| Patch for critical/high CVEs | Within 30 days |
| Patch for moderate CVEs | Within 90 days |
| Public disclosure | After patch is released |

---

## Automated scanning

This repository uses:

- **`dotnet nuget audit`** in CI — scans NuGet dependencies for known CVEs on every push.
- **GitHub Dependency Review** — blocks PRs that introduce high-severity CVEs.
- **GitHub Dependabot alerts** — notifies maintainers of newly published CVEs in existing dependencies.
