# SOLUTION.md — Design Decisions & Trade-offs

## 1. Architecture Overview

I chose a **Controller → Service → Repository → Data Store** layered architecture split across two projects. This demonstrates clean separation of concerns while keeping the solution lightweight enough for a 4-hour take-home assignment.

### Why Not MediatR / CQRS / AutoMapper?

The assignment domain is simple — products and categories. Adding MediatR or CQRS would introduce indirection without proportional benefit. AutoMapper adds a dependency and hidden performance costs; manual mapping with record constructors is explicit and compile-time safe.

## 2. Repository Pattern

### Two Repository Implementations

| Repository | Data Store | Rationale |
|-----------|-----------|-----------|
| `ProductRepository` | EF Core In-Memory | Assignment requires EF for "most data access"; enables `Include`, `AsNoTracking`, complex queries |
| `CategoryRepository` | Pure `List<T>` | Satisfies the explicit requirement: "at least one repository must use pure in-memory collections" |

### Why `List<T>` Over `Dictionary<TKey,TValue>` for Categories?

`List<T>` was sufficient because category data is small (12 seeded items) and queries are simple (find by ID, filter by parent). A `Dictionary` would add O(1) lookup but at the cost of more complex code. I demonstrated `Dictionary` usage in `SearchCacheService` and the `SearchEngine` inverted index instead.

## 3. ProductSearchEngine — The Core C# Challenge

### Fuzzy Matching: Levenshtein Distance

I chose Levenshtein distance over more sophisticated algorithms (e.g., n-gram, Jaro-Winkler) because:
- It's the **standard algorithm** for edit distance
- It's **dependency-free** (pure BCL loops and arrays)
- It's **intuitive to explain** in a code review
- It handles the required examples: `"lptop"` → `"laptop"` (1 deletion)

The threshold is set at 0.75, which balances precision and recall. `"bidget"` → `"budget"` scores 0.83 and matches; `"xyz"` vs `"laptop"` scores 0.0 and doesn't.

### Multi-Field Weighted Scoring

```
Name:        50% weight — highest because names are the primary identifier
SKU:         30% weight — good for exact lookups, users often search by SKU
Description: 20% weight — catches broad semantic matches without dominating
```

The weights sum to 100%, producing a normalized score between 0 and 1. Results are sorted by score descending.

### Inverted Index

The `SearchEngine<T>` builds an inverted index (`Dictionary<string, List<T>>`) for O(1) token lookups. This avoids scanning the entire product list on every search. The index is rebuilt on startup and incrementally updated on create/delete.

### Singleton Lifetime

`ProductSearchEngine` and `SearchCacheService` are registered as Singletons because:
- The inverted index is expensive to rebuild
- Search state should be shared across all requests
- The data is read-heavy with occasional writes
- Thread safety is handled via service-level coordination

## 4. Service Layer Design

Services are the **composition root** for business logic. `ProductService` composes:
- `IProductRepository` — data access
- `ICategoryRepository` — category name lookups for DTO mapping
- `ProductSearchEngine` — fuzzy search
- `SearchCacheService` — result caching

### Why Inject `ProductSearchEngine` Into Service (Not Controller)?

The assignment explicitly asks to "demonstrate understanding of DI pattern by properly registering and injecting the ProductSearchEngine." Injecting it into the service shows layered DI — the controller only sees `IProductService`, while the service orchestrates multiple dependencies. This is cleaner than injecting 4+ dependencies into a controller.

## 5. Pattern Matching for Validation

I used C# pattern matching in service methods instead of a validation library:

```csharp
if (request is not { Name: not null and not "", Sku: not null and not "", Price: > 0, Quantity: >= 0 })
{
    throw new ArgumentException("...");
}
```

This is concise, readable, and doesn't require FluentValidation or DataAnnotations. For a small project, it's the right balance.

## 6. Custom Middleware

Both middleware classes are **built from scratch** — no `UseMiddleware<T>()` helpers in `Program.cs`. They are instantiated manually in `app.Use()` lambdas:

```csharp
app.Use(async (context, next) =>
{
    var middleware = new RequestTimingMiddleware(next, logger);
    await middleware.InvokeAsync(context);
});
```

This demonstrates understanding of the middleware pipeline contract (`RequestDelegate`, `HttpContext`) without relying on framework convenience methods.

## 7. Trade-offs

| Decision | Trade-off |
|----------|-----------|
| EF Core In-Memory over real DB | Data is lost on restart; but zero setup for evaluators |
| `List<T>` for categories | O(n) lookups vs O(1) with `Dictionary`; acceptable for small data |
| Levenshtein over Jaro-Winkler | Simpler but less precise for transpositions |
| Singleton search engine | Shared state risk; mitigated by read-heavy access pattern |
| No AutoMapper | More manual mapping code; but explicit and dependency-free |
| No FluentValidation | Pattern matching is less powerful; sufficient for this scope |
| No async enumerable | Loads all products into memory; acceptable for < 1000 items |

## 8. Testing Strategy

I wrote 31 xUnit tests covering:
- **Fuzzy search**: exact match, typo tolerance, multi-field scoring, edge cases
- **LINQ extensions**: filtering, pagination, case-insensitive search
- **IComparable**: sorting behavior, null handling, price+name ordering

I skipped controller/integration tests because the assignment only requires "at least one unit test" and the service/search layer is where the interesting logic lives. The API can be verified via Swagger/curl.
