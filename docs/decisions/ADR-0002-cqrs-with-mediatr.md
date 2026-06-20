# ADR-0002 — CQRS with MediatR

## Status

Accepted

## Context

Critical system APIs tend to accumulate complexity in two distinct directions: **commands** (operations that change state and have side effects) and **queries** (operations that read state and must be fast, cacheable, and independently scalable). Mixing both in the same service or controller methods leads to:

- Fat service classes with mixed concerns.
- Difficulty applying different cross-cutting rules (e.g. caching for queries, outbox for commands).
- Tight coupling between the API layer and business logic.
- Harder unit testing due to mixed responsibilities.

Several patterns address this, from full event sourcing with separate read/write stores (complex, high operational cost) to a lightweight logical CQRS separation within a single data store (simpler, sufficient for most critical systems).

## Decision

Adopt **logical CQRS** using **MediatR** as the in-process mediator. Commands and queries are separate request objects dispatched through MediatR to dedicated handlers.

### Structure

```
Application/
├── Commands/
│   ├── CreateIdentityCommand.cs      # Command + Handler + Validator
│   └── ActivateIdentityCommand.cs
└── Queries/
    ├── GetIdentityByIdQuery.cs       # Query + Handler
    └── SearchIdentitiesQuery.cs
```

### Rules

| Concern | Command | Query |
|---|---|---|
| Mutates state | Yes | No |
| Returns data | Minimal (ID, status) | Full read model |
| Goes through outbox | Yes (domain events) | No |
| Cache eligible | No | Yes |
| Authorization | Write scopes | Read scopes |
| Validation | FluentValidation pipeline | Minimal (input sanitisation) |

### Pipeline behaviours (applied via MediatR)

Commands pass through: `LoggingBehaviour → ValidationBehaviour → Handler`  
Queries pass through: `LoggingBehaviour → CachingBehaviour (optional) → Handler`

### What is NOT done

- No separate read database / projections at this stage. The same PostgreSQL instance serves both reads and writes. This is a deliberate simplification; the architecture supports introducing a read replica or separate read store in a future phase without changing the command/query contract.

## Consequences

- **Handlers are small, focused, and independently testable** — each handles one operation.
- **Pipeline behaviours** add cross-cutting concerns without touching handler code.
- **MediatR adds a layer of indirection** — acceptable given the testability and separation benefits.
- **No distributed consistency concerns** at this stage (single data store).
- Future migration to a fully separated CQRS model (separate read store, event sourcing) is possible without breaking the existing handler structure.
