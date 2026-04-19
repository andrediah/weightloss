# Web Security Standard

**Version:** 1.0  
**Date:** 2026-04-17  
**Scope:** WeightLossTracker web application and any future web applications in this project

---

## 1. Transport Security

### 1.1 HTTPS Enforcement
- All production deployments **must** use HTTPS (TLS 1.2 minimum, TLS 1.3 preferred).
- HTTP must be permanently redirected (301) to HTTPS. Never serve application content over plain HTTP.
- HSTS (`Strict-Transport-Security`) must be enabled with a minimum `max-age` of 1 year and `includeSubDomains`.

```
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```

**WeightLossTracker finding:** `Program.cs:474` binds to `http://localhost:5000`. Before public deployment, replace with `app.UseHttpsRedirection()` and configure HTTPS in `appsettings.json` or via a reverse proxy (nginx/Caddy) that terminates TLS.

### 1.2 TLS Configuration
- Disable TLS 1.0 and 1.1.
- Disable weak cipher suites (RC4, 3DES, NULL ciphers).
- Use strong key exchange (ECDHE preferred).
- Obtain certificates from a trusted CA (e.g., Let's Encrypt). Do not use self-signed certificates in production.

---

## 2. Authentication & Authorization

### 2.1 Authentication Requirement
All endpoints that access or mutate user data **must** require authentication before responding.

**WeightLossTracker finding:** No authentication exists. Every API endpoint (`/api/profiles`, `/api/weight`, `/api/meals`, `/api/exercise`, etc.) is publicly accessible. A malicious actor can enumerate, read, and delete all user data without any credentials. Before public deployment, implement one of:
- ASP.NET Core Identity with cookie-based sessions, or
- JWT bearer tokens, or
- An OAuth 2.0 / OIDC provider (e.g., Google, Microsoft)

Minimum recommended addition to `Program.cs`:
```csharp
builder.Services.AddAuthentication(...);
builder.Services.AddAuthorization();
app.UseAuthentication();
app.UseAuthorization();
```
Then apply `[Authorize]` or `.RequireAuthorization()` to all endpoint groups.

### 2.2 Profile Isolation
**WeightLossTracker finding:** Profile selection is controlled entirely by the client via the `X-Profile-Id` header (`Program.cs:79`). Any user can pass any profile ID and access another user's weight logs, meal logs, AI history, and health data. After authentication is added, the active profile ID must be derived from the authenticated user's identity (e.g., a claim), never from a client-supplied header alone.

### 2.3 Authorization Checks
- Every data operation must verify that the authenticated user owns the resource before returning or mutating it.
- Never trust IDs supplied by the client to scope data access. Validate ownership server-side.

### 2.4 Password Policy (if passwords are used)
- Minimum 12 characters; enforce at least one number and one special character.
- Use a strong adaptive hashing algorithm: bcrypt, Argon2id, or ASP.NET Core `PasswordHasher<T>` (PBKDF2).
- Never store plaintext or MD5/SHA1-hashed passwords.
- Implement account lockout after 5 failed attempts (unlock after 15 minutes or via email).

---

## 3. Session Management

- Session tokens must be cryptographically random (≥128 bits of entropy).
- Session cookies must have `HttpOnly`, `Secure`, and `SameSite=Strict` (or `Lax`) attributes.
- Invalidate and issue a new session token on privilege escalation (e.g., after login).
- Session lifetime should not exceed 8 hours of inactivity; absolute maximum 24 hours.
- Provide a logout endpoint that invalidates the server-side session.

---

## 4. Security Headers

All responses must include the following HTTP headers. Configure them at the reverse proxy or via ASP.NET Core middleware.

| Header | Required Value |
|---|---|
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` |
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` (or `SAMEORIGIN` if framing is needed) |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | Restrict camera, microphone, geolocation: `camera=(), microphone=(), geolocation=()` |
| `Content-Security-Policy` | See Section 5 |

Add these in ASP.NET Core with a middleware or NuGet package such as `NetEscapades.AspNetCore.SecurityHeaders`.

---

## 5. Content Security Policy (CSP)

Define a CSP that restricts resource loading to known origins.

Recommended baseline for WeightLossTracker (single-origin SPA):
```
Content-Security-Policy:
  default-src 'self';
  script-src 'self';
  style-src 'self' 'unsafe-inline';
  img-src 'self' data:;
  font-src 'self';
  connect-src 'self';
  object-src 'none';
  base-uri 'self';
  form-action 'self';
  frame-ancestors 'none';
  upgrade-insecure-requests
```

- Avoid `'unsafe-inline'` and `'unsafe-eval'` for scripts. Use nonces or hashes if inline scripts are unavoidable.
- Test CSP in report-only mode first (`Content-Security-Policy-Report-Only`) before enforcing.

---

## 6. Input Validation & Output Encoding

### 6.1 Server-Side Validation
- All input must be validated on the server regardless of client-side checks.
- Validate type, length, format, and range for every field.

**WeightLossTracker — current validations:**
- Weight: `50–500` range check (`Program.cs:238`) — good.
- Name: empty-string check (`Program.cs:89`) — good. Add a maximum length (e.g., 100 chars).
- `MealType`, `Description`: empty checks (`Program.cs:401`) — good. Add max length limits.
- `Injuries`, `Goals`, `Notes` free-text fields: no length caps. Add server-side max-length (e.g., 1000 chars) to prevent oversized payloads.
- `DayOfWeek`: range `0–6` validated in exercise endpoints (`Program.cs:322`). Apply the same check to `ScheduleUpdateItem`.

### 6.2 Output Encoding
- All data returned in HTML context must be HTML-encoded. Use framework defaults (Razor auto-encodes).
- JSON responses from the API are safe as long as the client does not inject them into `innerHTML`. Prefer `textContent` over `innerHTML` in JavaScript.

### 6.3 SQL Injection Prevention
- Use parameterized queries or an ORM exclusively. Never concatenate user input into SQL strings.
- **WeightLossTracker:** EF Core with SQLite is used throughout — parameterized by default. The startup DB probe (`Program.cs:37–48`) uses `SqliteCommand` with literal strings (no user input), which is acceptable.

---

## 7. API Security

### 7.1 Rate Limiting
All publicly accessible endpoints must be rate-limited to prevent brute-force and abuse.

Recommended limits for WeightLossTracker:
- Authentication endpoints: 10 requests / minute per IP.
- AI endpoints (`/api/exercise/generate-*`, `/api/meals/advice`): 20 requests / hour per authenticated user — these proxy to a paid AI service and must be protected against runaway usage.
- All other API endpoints: 120 requests / minute per authenticated user.

ASP.NET Core 7+ provides built-in rate limiting via `Microsoft.AspNetCore.RateLimiting`.

### 7.2 CORS Policy
- Define an explicit CORS policy. Do not use `AllowAnyOrigin()` in production.
- Allow only the specific domain(s) where the application is hosted.

```csharp
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.WithOrigins("https://yourdomain.com")
         .AllowedMethods("GET", "POST", "PUT", "DELETE")
         .AllowCredentials()));
app.UseCors();
```

### 7.3 HTTP Method Enforcement
- Ensure endpoints only accept the declared HTTP method. ASP.NET Core Minimal APIs enforce this by default.
- Do not expose `TRACE` or `CONNECT` methods.

### 7.4 Request Size Limits
- Set a maximum request body size to prevent memory exhaustion.
- Default ASP.NET Core limit is 30 MB. Reduce to an appropriate value (e.g., 64 KB for JSON APIs without file upload).

```csharp
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 65536);
```

---

## 8. Secrets Management

- API keys, connection strings, and other secrets **must not** be committed to source control.
- Use environment variables, `dotnet user-secrets` (development), or a secrets manager (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) in production.

**WeightLossTracker finding:** The Gemini API key is read from `Configuration["Gemini:ApiKey"]` (`Program.cs:70`). Ensure this is set via environment variable or secrets manager, not hardcoded in `appsettings.json` which may be committed.

Verify the repository has a `.gitignore` that excludes:
```
appsettings.*.json   # environment-specific files that may contain secrets
*.db                 # SQLite database files (contain user health data)
*.db-wal
*.db-shm
```

---

## 9. Sensitive Data Protection

### 9.1 Data Classification
WeightLossTracker stores **health and fitness data** (weight history, injuries, meal logs, AI interactions). This is sensitive personal information subject to privacy regulations (GDPR, CCPA, HIPAA where applicable).

### 9.2 Data at Rest
- The SQLite database (`tracker.db`) must not be stored in a web-accessible directory.
- Apply filesystem permissions so only the application process user can read/write the database.
- Consider encrypting the database file at rest using SQLCipher or encrypting the hosting volume.

### 9.3 Data in Transit
- All API responses containing health data must be served over HTTPS only (enforced by Section 1).

### 9.4 AI Prompt Logs
**WeightLossTracker finding:** Full AI prompts and responses (including user health details, injury descriptions, and meal information) are stored in the `AiPromptLogs` table. Apply the same access controls as other health data. Consider a data-retention policy and a purge mechanism for old AI logs.

### 9.5 Minimum Data Principle
- Collect only fields required for the feature.
- Do not log sensitive fields (weight, injury details, meal content) to application logs or error-tracking services.

---

## 10. Error Handling & Logging

### 10.1 Error Responses
- Never expose stack traces, internal exception messages, or database schema details in API responses.
- Return generic error messages to clients. Log the full detail server-side.
- Use ASP.NET Core's exception handling middleware:

```csharp
app.UseExceptionHandler("/error");
// or in development only:
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
```

### 10.2 Structured Logging
- Use structured logging (Serilog, Microsoft.Extensions.Logging) with a centralized sink.
- Log authentication events (login, logout, failed attempts), authorization failures, and data mutations at `Information` level.
- Log unhandled exceptions at `Error` level.
- Never log passwords, API keys, or full health record content.

### 10.3 Monitoring & Alerting
- Alert on elevated 4xx/5xx error rates.
- Alert on unusual AI endpoint usage spikes (potential API key abuse).

---

## 11. Dependency & Supply Chain Security

- Pin NuGet package versions. Avoid floating version ranges (`*`) in production.
- Run `dotnet list package --vulnerable` in CI to detect known vulnerable dependencies.
- Enable Dependabot or Renovate for automated dependency update PRs.
- Do not include packages from unverified or unmaintained sources.

---

## 12. Infrastructure & Deployment

### 12.1 Principle of Least Privilege
- Run the application process as a non-root, non-administrator user.
- Grant the application only the filesystem permissions it needs (read/write to the DB folder; read-only elsewhere).

### 12.2 Database File Location
**WeightLossTracker finding:** The DB is written to `%LOCALAPPDATA%\WeightLossTracker\tracker.db` (`Program.cs:10–13`). When deployed on a Linux server, ensure the equivalent path is outside the web root and has restricted permissions (`chmod 600 tracker.db`).

### 12.3 Reverse Proxy
- Deploy behind a reverse proxy (nginx, Caddy, or Azure Front Door) to:
  - Terminate TLS
  - Apply security headers
  - Rate-limit at the network edge
  - Mask internal server details from `Server` response headers

### 12.4 OS & Runtime Patching
- Apply OS and .NET runtime security patches within 30 days of release (critical patches within 72 hours).
- Subscribe to [dotnet security advisories](https://github.com/dotnet/announcements/labels/Security).

---

## 13. Security Testing

| Activity | Frequency |
|---|---|
| Dependency vulnerability scan (`dotnet list package --vulnerable`) | Every CI build |
| OWASP ZAP or Burp Suite baseline scan | Each release |
| Manual review of new authentication/authorization logic | Each PR introducing auth changes |
| Penetration test (third-party) | Annually or before major launch |

---

## 14. Compliance Checklist (Pre-Launch)

Before deploying WeightLossTracker publicly, verify each item:

- [ ] HTTPS enforced; HTTP redirects to HTTPS
- [ ] HSTS header present
- [ ] Authentication required on all endpoints
- [ ] Profile ownership validated server-side (not via `X-Profile-Id` header alone)
- [ ] Rate limiting applied to all endpoints (especially AI endpoints)
- [ ] CORS restricted to production domain
- [ ] Security headers present (`X-Content-Type-Options`, `X-Frame-Options`, CSP, etc.)
- [ ] Gemini API key loaded from environment variable or secrets manager
- [ ] `tracker.db` not in web root; filesystem permissions restricted
- [ ] `appsettings.json` and `.db` files excluded from `.gitignore`
- [ ] No stack traces returned in production error responses
- [ ] Sensitive health data excluded from application logs
- [ ] Dependency scan passing with no high/critical vulnerabilities

---

*This document should be reviewed and updated whenever the application's threat model changes significantly (new endpoints, new data types, or public launch).*
