# 001 - CI/CD Pipeline (High-level)

- Status: Planned

## Goal

Provide a single high-level view of the delivery path, from source checkout to published
artifacts, so the order of quality gates and their failure impact is visible without reading
the `Jenkinsfile`.

## Intended scope

The diagram should present the pipeline stages as currently defined in the `Jenkinsfile`:

1. Checkout from source control.
2. Static analysis session start (SonarQube scanner begin).
3. Backend restore, solution-wide build, and solution-wide test.
4. Frontend install, lint, and build.
5. Observability and SeedWork immutability gates.
6. Static analysis session end and quality gate result.
7. Documentation generation with DocFX and publication to the docs site volume.

It should also make explicit which stages are blocking gates and which produce published
artifacts, since that is the distinction the prose notes do not convey visually.

## Related

- `docs/notes/002-ci-pipeline-architecture.md`
- `docs/notes/004-ci-cd-local-delivery-path.md`
- `Jenkinsfile`
