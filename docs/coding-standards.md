# .NET Coding Standards

> This document defines coding standards for this project.
> It is intended for use by all developers and AI coding assistants.
> **AI tools:** Treat these rules as mandatory for all code generation in this project.
> **Developers:** These standards are enforced in code review and CI.

---

## Target Stack

- .NET 10 / C# 14
- ASP.NET Core for web APIs and web applications
- Entity Framework Core for data access
- xUnit + FluentAssertions + NSubstitute for testing

---

## Architecture

- Follow **Clean Architecture**: separate Domain, Application, Infrastructure, and Presentation layers
- Domain and Application layers must never reference Infrastructure
- Use **CQRS** (Command Query Responsibility Segregation) for all application logic
  - Commands mutate state and return `Result<T>`
  - Queries are read-only and return DTOs
- Use the **Repository pattern** to abstract data access
- Prefer **vertical slice architecture** within feature folders over grouping by type

---

## C# Language Standards

- Always target the latest stable C# version features where they improve clarity
- Enable **nullable reference types** in all projects (`<Nullable>enable</Nullable>`)
- Enable **implicit usings** (`<ImplicitUsings>enable</ImplicitUsings>`)
- Use **file-scoped namespaces** (`namespace MyApp.Feature;`)
- Prefer **primary constructors** for simple dependency injection
- Use **records** for immutable data transfer objects and value objects
- Use **pattern matching** over type-checking with `is` and `as`
- Prefer `switch` expressions over `switch` statements
- Use **collection expressions** (`[item1, item2]`) over `new List<T> { }`

### Good ✅
```csharp
namespace MyApp.Features.Users;

public record CreateUserCommand(string Email, string Name);

public sealed class UserService(IUserRepository repository, ILogger<UserService> logger)
{
    public async Task<Result<User>> CreateAsync(CreateUserCommand command, CancellationToken ct)
    {
        var existing = await repository.FindByEmailAsync(command.Email, ct);
        return existing is not null
            ? Result.Failure<User>("Email already exists")
            : Result.Success(await repository.AddAsync(new User(command.Email, command.Name), ct));
    }
}
```

### Bad ❌
```csharp
namespace MyApp
{
    public class UserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public User Create(string email, string name)
        {
            var existing = _repository.FindByEmailAsync(email).Result; // blocks thread
            if (existing != null)
                throw new Exception("Email already exists"); // exception for control flow
            return _repository.AddAsync(new User(email, name)).Result;
        }
    }
}
```

---

## Async / Await

- Use `async`/`await` for **all I/O operations** — never `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`
- Always pass and forward `CancellationToken` through the entire call chain
- Use `ConfigureAwait(false)` in **library and infrastructure code**
- Use `ValueTask<T>` for hot paths where the result is often synchronous
- Never use `async void` — use `async Task` instead (exception: event handlers)
- Prefer `Task.WhenAll()` for parallel independent async operations

### Good ✅
```csharp
public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken ct)
{
    return await _dbContext.Products
        .Where(p => p.IsActive)
        .ToListAsync(ct)
        .ConfigureAwait(false);
}
```

### Bad ❌
```csharp
public List<Product> GetActiveProducts()
{
    return _dbContext.Products
        .Where(p => p.IsActive)
        .ToListAsync().Result; // deadlock risk
}
```

---

## Error Handling

- Use a **Result pattern** (`Result<T>`) for expected failures — do not throw exceptions for control flow
- Reserve exceptions for **truly exceptional, unrecoverable** situations
- Use **global exception middleware** in ASP.NET Core to handle unhandled exceptions
- Never swallow exceptions silently in a catch block
- Always log exceptions with sufficient context before re-throwing or returning errors
- Use `ProblemDetails` (RFC 7807) for API error responses

### Good ✅
```csharp
public async Task<Result<Order>> PlaceOrderAsync(PlaceOrderCommand command, CancellationToken ct)
{
    if (!await _inventoryService.IsAvailableAsync(command.ProductId, ct))
        return Result.Failure<Order>("Product is out of stock");

    var order = Order.Create(command);
    await _repository.AddAsync(order, ct);
    return Result.Success(order);
}
```

### Bad ❌
```csharp
public async Task<Order> PlaceOrderAsync(PlaceOrderCommand command)
{
    try
    {
        // ...
    }
    catch (Exception ex)
    {
        // swallowed — no logging, no rethrow
    }
    return null!;
}
```

---

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Classes / Records | PascalCase | `OrderService` |
| Interfaces | PascalCase with `I` prefix | `IOrderRepository` |
| Methods | PascalCase | `GetOrderByIdAsync` |
| Properties | PascalCase | `CreatedAt` |
| Local variables | camelCase | `orderTotal` |
| Parameters | camelCase | `orderId` |
| Private fields | camelCase with `_` prefix | `_repository` |
| Constants | PascalCase | `MaxRetryCount` |
| Async methods | Suffix with `Async` | `SaveChangesAsync` |
| Generic type params | PascalCase with `T` prefix | `TEntity`, `TResult` |

---

## Dependency Injection

- Register dependencies in dedicated extension methods (`IServiceCollection` extensions), not in `Program.cs` directly
- Prefer **constructor injection** — avoid service locator pattern (`IServiceProvider` in business logic)
- Match **lifetime** carefully: Singleton > Scoped > Transient (never inject shorter-lived into longer-lived)
- Use `IOptions<T>` for typed configuration — never inject `IConfiguration` directly into services
- Use `AddHttpClient<T>()` for all `HttpClient` instances — never `new HttpClient()`

### Good ✅
```csharp
// ServiceCollectionExtensions.cs
public static IServiceCollection AddOrderingModule(this IServiceCollection services, IConfiguration config)
{
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<OrderService>();
    services.Configure<OrderingOptions>(config.GetSection("Ordering"));
    return services;
}
```

---

## Entity Framework Core

- Use **migrations** for all schema changes — never modify the database manually
- Keep `DbContext` **scoped** — never singleton
- Use **`AsNoTracking()`** for read-only queries
- Avoid lazy loading — use explicit `.Include()` for related data
- Use **owned entities** for value objects
- Do not expose `DbContext` outside the Infrastructure layer — use repositories
- Use **`IQueryable`** within the repository; return materialized collections to callers

### Good ✅
```csharp
public async Task<IReadOnlyList<OrderSummaryDto>> GetRecentOrdersAsync(int userId, CancellationToken ct)
{
    return await _dbContext.Orders
        .AsNoTracking()
        .Where(o => o.UserId == userId)
        .OrderByDescending(o => o.CreatedAt)
        .Take(20)
        .Select(o => new OrderSummaryDto(o.Id, o.Total, o.CreatedAt))
        .ToListAsync(ct);
}
```

---

## ASP.NET Core API

- Use **Minimal APIs** for simple endpoints; use **Controllers** for complex, grouped endpoints
- Always return `IActionResult` or `Results<T>` with explicit HTTP status codes
- Use **`[ApiController]`** attribute on controllers for automatic model validation
- Validate input using **FluentValidation** — never validate manually in controllers
- Use **`ProblemDetails`** for all error responses
- Version your APIs from day one (`/api/v1/`)
- Use **`IEndpointRouteBuilder` extension methods** to group Minimal API endpoints

### Good ✅
```csharp
app.MapPost("/api/v1/orders", async (
    CreateOrderRequest request,
    IValidator<CreateOrderRequest> validator,
    OrderService orderService,
    CancellationToken ct) =>
{
    var validation = await validator.ValidateAsync(request, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var result = await orderService.CreateAsync(request.ToCommand(), ct);
    return result.IsSuccess
        ? Results.Created($"/api/v1/orders/{result.Value.Id}", result.Value)
        : Results.Problem(result.Error);
});
```

---

## Logging

- Use **`ILogger<T>`** — never `Console.WriteLine` or `Debug.WriteLine` in production code
- Use **structured logging** with named parameters, not string interpolation
- Apply appropriate log levels: `Trace` → `Debug` → `Information` → `Warning` → `Error` → `Critical`
- Use **`LoggerMessage.Define`** or source-generated logging for high-frequency log paths
- Never log sensitive data (passwords, tokens, PII)

### Good ✅
```csharp
_logger.LogInformation("Order {OrderId} placed by user {UserId}", order.Id, userId);
```

### Bad ❌
```csharp
_logger.LogInformation($"Order {order.Id} placed by user {userId}"); // not structured
Console.WriteLine("Order placed");
```

---

## Testing

- Use **xUnit** as the test framework
- Use **FluentAssertions** for readable assertions
- Use **NSubstitute** for mocking interfaces
- Follow the **Arrange / Act / Assert** pattern
- Name tests: `MethodName_StateUnderTest_ExpectedBehavior`
- Aim for high coverage on Domain and Application layers; use integration tests for Infrastructure
- Use **`WebApplicationFactory<T>`** for API integration tests
- Do not test implementation details — test observable behavior

### Good ✅
```csharp
public class OrderServiceTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly OrderService _sut;

    public OrderServiceTests() => _sut = new OrderService(_repository);

    [Fact]
    public async Task CreateAsync_WhenProductOutOfStock_ReturnsFailureResult()
    {
        // Arrange
        var command = new CreateOrderCommand(ProductId: 1, Quantity: 5);
        _repository.IsInStockAsync(1, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _sut.CreateAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("out of stock");
    }
}
```

---

## Security

- Never store secrets in `appsettings.json` — use environment variables, Azure Key Vault, or user secrets
- Always use **parameterized queries** — never concatenate SQL strings
- Use **ASP.NET Core Data Protection** for encrypting sensitive data at rest
- Apply **`[Authorize]`** by default; explicitly allow anonymous with `[AllowAnonymous]`
- Use **HTTPS** only — redirect HTTP to HTTPS
- Set appropriate **CORS** policies — never use wildcard `*` in production
- Sanitize all user input before processing

---

## Performance

- Use `StringBuilder` for string concatenation in loops — not `+` operator
- Prefer `Span<T>` and `Memory<T>` for performance-critical buffer operations
- Use **`IAsyncEnumerable<T>`** for streaming large data sets
- Cache expensive reads with `IMemoryCache` or `IDistributedCache`
- Profile before optimizing — don't prematurely optimize

---

## Code Quality

- Keep methods **small and focused** — single responsibility
- Limit method parameters to **4 or fewer** — use parameter objects for more
- Avoid **magic numbers and strings** — use named constants or enums
- No commented-out code — use version control instead
- All public APIs must have **XML doc comments**
- Run **Roslyn analyzers** and treat warnings as errors in CI
- Use `.editorconfig` to enforce style consistently across the team

---

## Using This File With AI Coding Assistants

| Tool | What to do |
|---|---|
| **Claude Code** | Rename or symlink to `CLAUDE.md` in project root |
| **Cursor** | Rename or symlink to `.cursorrules` in project root |
| **GitHub Copilot** | Copy to `.github/copilot-instructions.md` |
| **Windsurf** | Rename or symlink to `.windsurfrules` in project root |
| **Aider** | Pass via `--system-prompt coding-standards.md` |
| **ChatGPT / Gemini** | Paste contents into the system prompt or custom instructions |

> **Tip:** Keep `coding-standards.md` as the single source of truth and use symlinks
> so all tools stay in sync automatically:
> ```bash
> ln -s coding-standards.md CLAUDE.md
> ln -s coding-standards.md .cursorrules
> ln -s coding-standards.md .windsurfrules
> ```
