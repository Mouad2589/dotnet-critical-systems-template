# ADR-0004 — PostgreSQL as Primary Data Store

## Status

Accepted

## Context

The template requires a relational database for three distinct workloads:

1. **Identity records** — structured, strongly-typed entities with relational integrity constraints (foreign keys, unique identifiers, enum status fields).
2. **Outbox table** — append-only rows written atomically with domain changes; queried by a background publisher on a short polling interval.
3. **Workflow state** — case-level records with lifecycle transitions and query patterns by status, assignee and SLA deadline.

Several database options were evaluated:

| Option | Strengths | Weaknesses for this context |
|---|---|---|
| PostgreSQL | ACID, JSONB, row-level security, excellent .NET support, open source | None blocking |
| SQL Server | Strong .NET integration, enterprise tooling | Licensing cost, heavier footprint |
| MySQL / MariaDB | Widely available | Weaker ACID guarantees historically, less expressive SQL |
| MongoDB | Flexible schema | No multi-document ACID in all versions, joins are complex, outbox pattern harder |
| SQLite | Zero-config | Not suitable for concurrent multi-process write workloads |

## Decision

**PostgreSQL 16** is the primary data store for all workloads.

### Rationale

- **ACID compliance**: full transaction guarantees enable the outbox pattern with a single atomic `INSERT` across the aggregate table and the outbox table.
- **Row-level security (RLS)**: allows tenant or role-based data isolation at the database layer — relevant for multi-agency deployments.
- **JSONB**: stores flexible audit event payloads without schema migrations for new event types.
- **`gen_random_uuid()`**: native UUID generation for primary keys — no application-side dependency.
- **EF Core support**: `Npgsql.EntityFrameworkCore.PostgreSQL` is mature, actively maintained, and supports PostgreSQL-specific types and functions.
- **Operational maturity**: strong ecosystem (pgAdmin, pg_stat_statements, logical replication, point-in-time recovery), available on all major cloud providers as a managed service.
- **Open source**: no licensing cost, no vendor lock-in.

### Connection and pooling

- Connection pooling via **PgBouncer** in transaction mode for high-concurrency API deployments.
- Connection strings are never hardcoded; injected via environment variables or secrets manager (see [security.md](../security.md)).
- `Npgsql` connection string format: `Host=...;Port=5432;Database=...;Username=...;Password=...;Pooling=true;Minimum Pool Size=2;Maximum Pool Size=20`.

### Migration strategy

- Schema migrations managed with **EF Core Migrations** (`dotnet ef migrations add`, `dotnet ef database update`).
- Migrations run at application startup in development; applied via a dedicated migration job in production (never by the API process itself).
- Migration scripts are reviewed as part of the PR process — treated as code, not an afterthought.

### Data partitioning (future)

- The `audit_events` table is a candidate for range partitioning by `occurred_at` once volume warrants it.
- The `outbox_messages` table is pruned periodically once messages are processed.

## Consequences

- EF Core DbContext is the single entry point for all database interactions; raw SQL is permitted only for performance-critical read queries and must be reviewed.
- All writes go through the application layer — no direct database writes from external tools in production.
- Integration tests spin up a real PostgreSQL instance via **Testcontainers** (`Testcontainers.PostgreSql`) to ensure behaviour matches production.
- Teams must have operational knowledge of PostgreSQL (backup, monitoring, vacuuming, index maintenance).
