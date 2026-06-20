# Roadmap

## Phase 1 — Foundation (complete)

- [x] Repository structure and governance files.
- [x] Initial README with architecture overview and Mermaid diagram.
- [x] ADR-0001: public repository scope.
- [x] Architecture document: Clean Architecture layers, request flow, project structure.
- [x] Observability document: OpenTelemetry setup, logging, metrics, tracing, health checks.
- [x] Security document: authentication, authorization, input validation, secrets, audit trail.
- [x] Testing strategy: unit, integration, contract, architecture tests, CI gates.

## Phase 2 — Code scaffold

- [ ] `src/` project structure with solution file and layer projects.
- [ ] `Domain/` — sample aggregate with value objects and domain events.
- [ ] `Application/` — sample command + handler + validator + pipeline behaviours.
- [ ] `Infrastructure/` — EF Core DbContext, sample repository, outbox table.
- [ ] `Api/` — Minimal API endpoints, OpenAPI spec.
- [ ] `tests/` — unit and integration test projects with sample tests.
- [ ] `docker-compose.yml` — local dev environment (PostgreSQL, Redis, OTEL Collector, Grafana).
- [ ] ADR-0002: CQRS and MediatR.
- [ ] ADR-0003: Outbox pattern.
- [ ] ADR-0004: PostgreSQL as primary data store.

## Phase 3 — Public authority

- [ ] Link repository to a LinkedIn article.
- [ ] Add a GitHub Actions CI workflow (build, test, coverage, vulnerability scan).
- [ ] Add a Grafana dashboard JSON export.
- [ ] Publish blog-post narrative: "What I look for in a .NET critical system baseline".
- [ ] Release notes for each major improvement.
