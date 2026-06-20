# Security

## Security principles

1. **Validate at the boundary** — all inputs are validated before entering the application layer.
2. **Never trust the caller** — every request is authenticated and authorized, regardless of network origin.
3. **Secrets never in code** — all secrets are injected via environment variables or a secrets manager.
4. **Least privilege for services** — each service account has only the permissions it needs.
5. **Fail secure** — on authorization failure, deny by default.

---

## Authentication

### JWT Bearer (primary)

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience  = builder.Configuration["Auth:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ClockSkew                = TimeSpan.FromSeconds(30)
        };
    });
```

- Authority is the OIDC provider (Keycloak, Azure AD, etc.).
- Token validation is performed on every request; no server-side session state.
- Token lifetime ≤ 15 minutes for sensitive operations.

### mTLS (service-to-service)

Internal service-to-service calls use mutual TLS. Certificates are short-lived and managed by a PKI or service mesh (e.g. Istio).

---

## Authorization

### Policy-based authorization

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuditReader", policy =>
        policy.RequireClaim("role", "AuditReader", "SystemAdmin"));

    options.AddPolicy("RequireOperator", policy =>
        policy.RequireClaim("role", "Operator")
              .RequireClaim("region"));  // ABAC: region attribute required
});
```

- Use `[Authorize(Policy = "...")]` on endpoints; never rely on role strings inline.
- Attribute-based constraints (ABAC) are enforced in application handlers using the claims principal.

---

## Input validation

All commands and queries are validated using **FluentValidation** as a MediatR pipeline behaviour. Invalid requests are rejected before reaching domain logic.

```csharp
public sealed class CreateIdentityCommandValidator
    : AbstractValidator<CreateIdentityCommand>
{
    public CreateIdentityCommandValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty()
            .Matches(@"^[A-Z0-9]{8,12}$");  // format enforced, never trust client

        RuleFor(x => x.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .GreaterThan(new DateOnly(1900, 1, 1));
    }
}
```

**Rules:**
- Reject on first validation failure; return `400 Bad Request` with a structured error body.
- Never return stack traces to the caller (use a global exception handler).
- SQL injection: EF Core parameterizes all queries; raw SQL is forbidden unless reviewed.

---

## Secrets management

| Secret type | Storage |
|---|---|
| Connection strings | Environment variable / Azure Key Vault / AWS Secrets Manager |
| JWT signing keys | HSM or KMS |
| API keys (third-party) | Secrets manager, rotated on a schedule |
| TLS certificates | Certificate manager (auto-renew) |

**Never:**
- Commit secrets to git (`.env`, `appsettings.Production.json` with real values).
- Log secrets, tokens or connection strings.
- Pass secrets as query parameters.

---

## Security headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=()");
    await next();
});
```

---

## Dependency and supply chain security

- `dotnet list package --vulnerable` in CI pipeline; build fails on high/critical CVEs.
- NuGet packages pinned to exact versions in `Directory.Packages.props`.
- Container images built from distroless or minimal base images.
- SBOM (Software Bill of Materials) generated on each release.
- Dependabot or Renovate for automated patch PRs.

---

## Audit trail

All sensitive operations (create, update, delete of critical entities; authentication events; authorization failures) are written to an append-only audit log:

```csharp
public record AuditEvent(
    string EventType,
    string ActorId,
    string EntityType,
    string EntityId,
    string Outcome,
    string CorrelationId,
    DateTimeOffset Timestamp);
```

- Written within the same transaction as the mutating operation (outbox pattern).
- Never updated or deleted.
- Exported to a separate, access-controlled store for long-term retention.
