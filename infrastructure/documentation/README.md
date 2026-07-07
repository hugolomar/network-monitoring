# DocFX documentation build

This folder configures how conceptual markdown under `docs/` and `specs/` is published to the static
site in `artifacts/docs/site/`.

## Build locally

```bash
bash infrastructure/documentation/build-docfx.sh
```

That script:

1. Runs `generate-conceptual-tocs.py` — refreshes sidebar entries for guides, notes, and ADRs
2. Runs `dotnet docfx metadata` — extracts API reference from C# projects
3. Runs `dotnet docfx build` — generates HTML under `artifacts/docs/site/`

Preview on your machine (ephemeral, for authors):

```bash
dotnet docfx serve artifacts/docs/site -p 8080
```

## Published site (CI)

In the local CI stack, documentation is served as a **static site** after a successful Jenkins build —
this is the intended team-facing view (not `docfx serve`).

**Prerequisites:**

```bash
docker compose -f docker-compose.ci.yml up -d
# Run the pipeline in Jenkins (http://localhost:8085) until "Generate & Publish Docs" succeeds
```

**Flow:**

1. Jenkins stage `Generate & Publish Docs` runs the generator, `docfx metadata`, and `docfx build`.
2. Output is copied to the shared Docker volume `docs-site` (`/var/jenkins_home/docs-site/` in the Jenkins container).
3. The `documentation` service (nginx in `docker-compose.ci.yml`) serves that volume read-only.

**URL:** [http://localhost:8090](http://localhost:8090)

Each green pipeline run refreshes the site. A failed docs stage leaves the previous publish in place until the next successful run.

| Mode | Audience | How to view |
|------|----------|-------------|
| `docfx serve` (port 8080) | Authors editing markdown/TOC | Local preview before merge |
| nginx on port 8090 | Team / reviewers after CI | Published wiki from last successful build |

Build and sidebar details for maintainers stay in this file; section READMEs under `docs/` are reader-facing only.

## Sidebar generation

| File | Role |
|------|------|
| `toc.template.yml` | Manual structure (specs, wiki, section landing pages). Edit this. |
| `toc.yml` | Generated output consumed by DocFX. Do not edit by hand. |
| `generate-conceptual-tocs.py` | Scans `docs/guides/`, `docs/notes/`, and `docs/adr/` (excluding `README.md`) and injects sidebar items from each file's `#` title. |

Adding a guide, note, or ADR:

1. Create the markdown file under the appropriate `docs/` folder with an `#` heading.
2. Run `bash infrastructure/documentation/build-docfx.sh` (or at least the Python generator before `docfx build`).

Section landing pages (`README.md` in `docs/` and `specs/`) are linked manually in `toc.template.yml`.
Their body is written for readers browsing the repo on GitHub; keep DocFX build instructions here, not
in those READMEs.

## CI

The Jenkins pipeline runs the same generator step before `docfx metadata` and `docfx build`, then publishes to the `docs-site` volume served at [http://localhost:8090](http://localhost:8090). See **Published site (CI)** above.
