# ADR 0010: Frontend Testing Convention

- Status: Accepted
- Date: 2026-06-17

## Context

In `src/NetworkMonitoring.Frontend/src`, the team needed to choose how frontend tests should be
organized:

- colocated with source modules, or
- centralized under a single tests folder.

The repository already used Vitest with a global setup entrypoint (`src/test/setup.ts`) and had
feature tests next to frontend source files.

This ADR formalizes the resulting hybrid layout so contributors follow one consistent rule.

## Decision

Adopt a **hybrid frontend test layout** with test colocation as the default:

- Place feature tests close to the source they validate:
  - `src/api/*.test.ts`
  - `src/pages/*.test.tsx`
  - `src/components/*.test.tsx` (when needed)
- Keep shared/global test infrastructure in `src/test/`:
  - `src/test/setup.ts`
  - Current required setup import: `@testing-library/jest-dom/vitest`
  - Purpose: extend Vitest `expect` with Testing Library matchers (for example
    `toBeInTheDocument`) for existing page/component tests.

`src/test/` is not a second test suite; it is the global setup/utilities location.
In other words, frontend testing is intentionally hybrid: colocated feature tests + centralized shared setup.

## Rationale

- Improves discoverability and maintenance during refactors.
- Avoids duplicated setup in individual test files.
- Aligns with common React + Vitest practice.
- Matches current repository implementation.
- Keeps the layout explicit: colocation for feature intent, centralization only for global test wiring.

## Alternatives Considered

1. **Centralized frontend test folder**
   - Pros: one location to scan all tests.
   - Cons: weaker locality with source modules.
   - Rejected.

2. **No global setup file**
   - Pros: one less file.
   - Cons: repetitive matcher/bootstrap imports in test files.
   - Rejected.

## Consequences

- **Positive:** predictable test placement and cleaner frontend test files.
- **Negative:** frontend and backend test layouts differ.
- **Cross-stack note:** the difference en test layouts is intentional. Backend `.NET` tests remain in dedicated
  projects under `/tests`, are compiled separately as test assemblies, and follow the standard
  workflow for that ecosystem.
