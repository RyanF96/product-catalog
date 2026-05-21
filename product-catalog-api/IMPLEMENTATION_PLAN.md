# Product Catalog Management System — Implementation Plan

## Project Overview

Build a full-stack Product Catalog Management System for a small e-commerce company. The solution consists of an ASP.NET Core Web API backend and an Angular 16+ SPA frontend, delivered as a single GitHub repository with documentation and a video demo.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core Web API, C# 12, .NET 8 |
| Data | Entity Framework Core (In-Memory provider), In-memory collections (List/Dictionary) |
| Frontend | Angular 16+, TypeScript, RxJS, Reactive Forms, Standalone Components |
| Testing | xUnit (backend), Jasmine/Karma (frontend) |

---

## Phase 1: Backend Implementation (C# Web API)

### 1.1 Project Setup & Configuration
- [ ] Create ASP.NET Core Web API project with nullable reference types enabled (`<Nullable>enable</Nullable>`)
- [ ] Configure EF Core with In-Memory database provider
- [ ] Set up `Program.cs` with DI container registrations
- [ ] Enable CORS for Angular frontend (`http://localhost:4200`)

### 1.2 Core Domain Models
- [ ] `Product` entity: `Id`, `Name`, `Description`, `SKU`, `Price`, `Quantity`, `CategoryId`, `CreatedAt`, `UpdatedAt`
- [ ] `Category` entity: `Id`, `Name`, `Description`, `ParentCategoryId` (nullable for root categories)
- [ ] Configure EF Core entity relationships (Product -> Category)

### 1.3 Repository Layer (Generics + In-Memory)
- [ ] Create `IRepository<T>` interface with CRUD operations
- [ ] Implement `Repository<T>` base class using **pure in-memory collections** (`List<T>`, `Dictionary<TKey,TValue>`) — no EF
- [ ] Implement `IProductRepository` extending `IRepository<Product>`
- [ ] Implement `ICategoryRepository` extending `IRepository<Category>` with tree support
- [ ] Register repositories as Scoped services in DI

### 1.4 DTOs (C# 9 Record Types)
- [ ] `ProductDto` (read), `CreateProductRequest`, `UpdateProductRequest`
- [ ] `CategoryDto` (read), `CreateCategoryRequest`, `UpdateCategoryRequest`
- [ ] `ProductSearchResult`, `CategoryTreeNode`
- [ ] Use `init`-only properties and `with` expressions where appropriate

### 1.5 ProductSearchEngine (Core C# Challenge — No External Libraries)
- [ ] Implement generic `SearchEngine<T>` class using only .NET BCL
- [ ] **Fuzzy matching**: Levenshtein distance algorithm for typo tolerance (e.g., "lptop" -> "laptop")
- [ ] **Multi-field weighted scoring**: Search across Name (weight 0.5), Description (weight 0.2), SKU (weight 0.3)
- [ ] **Efficient in-memory indexing**: Build inverted index using `Dictionary<string, List<T>>`
- [ ] Register `ProductSearchEngine` as Singleton in DI (demonstrate DI pattern)

### 1.6 Custom LINQ Extension Methods
- [ ] `FilterByCategory(this IEnumerable<Product>, int categoryId)`
- [ ] `FilterByPriceRange(this IEnumerable<Product>, decimal min, decimal max)`
- [ ] `FilterInStock(this IEnumerable<Product>)` — Quantity > 0
- [ ] `SearchByName(this IEnumerable<Product>, string searchTerm)`

### 1.7 Category Tree Structure
- [ ] Implement `BuildTree()` method to construct hierarchical category tree
- [ ] Support unlimited nesting depth via recursive `ParentCategoryId` references
- [ ] Flatten tree utility for dropdown display
- [ ] Return `CategoryTreeNode` records with `Children` collection

### 1.8 IComparable Implementation
- [ ] `Product` implements `IComparable<Product>` for custom sorting
- [ ] Sort by: `Price` ascending, then `Name` ascending
- [ ] Support `IComparer<Product>` override for alternative sort strategies

### 1.9 API Controllers & Endpoints

#### Products Controller
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/products` | List with pagination, category filter, name search |
| `GET` | `/api/products/{id}` | Get single product |
| `POST` | `/api/products` | Create product |
| `PUT` | `/api/products/{id}` | Update product |
| `DELETE` | `/api/products/{id}` | Delete product |

#### Categories Controller
| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/categories` | Flat list of all categories |
| `GET` | `/api/categories/tree` | Hierarchical tree structure |
| `POST` | `/api/categories` | Create category |

- [ ] Implement **manual model binding** on at least one action (e.g., `POST /api/products` reads from `Request.Body` directly)

### 1.10 Pattern Matching for Validation
- [ ] Use C# pattern matching in request validation logic
- [ ] Example: `request is { Name: not null and not "", Price: > 0 and < 100000 }`
- [ ] Centralized validation helper using `switch` expressions

### 1.11 Custom Middleware (From Scratch)
- [ ] `RequestTimingMiddleware` — logs request duration without using `UseMiddleware<>` helpers
- [ ] `ExceptionHandlingMiddleware` — catches exceptions, returns structured error response
- [ ] Register manually via `app.Use()` in `Program.cs`

### 1.12 Simple Caching Layer
- [ ] `SearchCacheService` using `Dictionary<string, CacheEntry<IEnumerable<ProductDto>>>`
- [ ] Cache key = hash of search parameters
- [ ] TTL-based expiration (60 seconds)
- [ ] Thread-safe operations with `lock` or `ConcurrentDictionary`

### 1.13 Custom JSON Serialization
- [ ] Custom `JsonConverter` for `CategoryTreeNode` to control output shape
- [ ] Apply via `[JsonConverter]` or `JsonSerializerOptions` on one endpoint

### 1.14 Backend Testing
- [ ] Unit test for `ProductSearchEngine` (fuzzy matching, weighted scoring)
- [ ] Unit test for custom LINQ extensions
- [ ] Unit test for `IComparable<Product>` sorting logic

---

## Phase 2: Frontend Implementation (Angular SPA)

### 2.1 Project Setup
- [ ] Create Angular 16+ project with standalone components (no NgModules)
- [ ] Configure `HttpClient`, `ReactiveFormsModule` globally
- [ ] Set up environment files for API base URL

### 2.2 TypeScript Models & Interfaces
- [ ] `Product` interface matching backend DTO
- [ ] `Category` interface with optional `children?: Category[]`
- [ ] `PagedResult<T>` for paginated responses
- [ ] `SearchFilters` interface for filter state

### 2.3 Services (RxJS-based)
- [ ] `ProductService` — CRUD + search operations, returns `Observable<T>`
- [ ] `CategoryService` — category CRUD + tree retrieval
- [ ] `SearchStateService` — shared RxJS `BehaviorSubject` for search/filter state management
- [ ] Proper error handling with `catchError` and user-friendly messages

### 2.4 UI Components

| Component | Purpose |
|-----------|---------|
| `ProductListComponent` | Grid/table display with pagination |
| `ProductFormComponent` | Add/Edit form (shared), reactive validation |
| `SearchBarComponent` | Debounced search input (300ms) |
| `CategoryFilterComponent` | Dropdown populated from `/api/categories` |
| `ConfirmDialogComponent` | Reusable delete confirmation |
| `LoadingSpinnerComponent` | Async operation indicator |
| `ErrorMessageComponent` | User-friendly error display |

### 2.5 Pages / Routes
| Route | Component |
|-------|-----------|
| `/products` | ProductListComponent |
| `/products/new` | ProductFormComponent (add mode) |
| `/products/edit/:id` | ProductFormComponent (edit mode) |

### 2.6 Reactive Forms & Validation
- [ ] `ProductForm` with validators: `Required`, `min(0)`, `maxLength(100)`, `pattern(SKU format)`
- [ ] Cross-field validation (e.g., `UpdatedAt` >= `CreatedAt`)
- [ ] Real-time validation feedback in template

### 2.7 Frontend Testing
- [ ] Unit test for `ProductService` (mock `HttpClient`)
- [ ] Unit test for `SearchBarComponent` (debounce behavior)

---

## Phase 3: Integration & Polish

### 3.1 Seed Data
- [ ] Seed 20+ sample products across 5+ hierarchical categories
- [ ] Categories: Electronics > Computers > Laptops, Electronics > Phones, Home & Garden, etc.

### 3.2 End-to-End Verification
- [ ] Product CRUD flow (create -> list -> edit -> delete)
- [ ] Search with fuzzy matching demonstration
- [ ] Category filter dropdown population
- [ ] Category tree API response verification

### 3.3 Documentation
- [ ] `README.md`: build instructions, run commands, tech stack overview
- [ ] `SOLUTION.md`: design decisions, trade-offs, architecture explanation
- [ ] API endpoint documentation (can be minimal)

### 3.4 Video Demo (5-10 minutes)
- [ ] Screen record working application with voiceover
- [ ] Demonstrate: CRUD, search, category tree
- [ ] Walk through code: `ProductSearchEngine`, repository pattern, Angular services
- [ ] Explain key design decisions and trade-offs
- [ ] Highlight: custom middleware, fuzzy search, DI setup

---

## Requirements Traceability Matrix

| Requirement | Where Implemented |
|-------------|-------------------|
| Custom `Repository<T>` with generics | `Repository<T>` class, `IRepository<T>` interface |
| Pure in-memory collections (no EF) | `Repository<T>` uses `List<T>` / `Dictionary<K,V>` |
| Custom LINQ extensions | `LinqExtensions.cs` — `FilterByCategory`, `FilterByPriceRange`, etc. |
| C# 9+ record types for DTOs | All `*Dto.cs`, `*Request.cs` files |
| Pattern matching validation | Validation helper in controllers or service layer |
| Nullable reference types | `<Nullable>enable</Nullable>` project-wide |
| Custom middleware from scratch | `RequestTimingMiddleware`, `ExceptionHandlingMiddleware` |
| Simple caching layer | `SearchCacheService` with `Dictionary<TKey, TValue>` |
| Category tree (hierarchical) | `CategoryTreeNode`, `BuildTree()` recursive method |
| `IComparable<Product>` | `Product.CompareTo()` implementation |
| Manual model binding | `POST /api/products` custom binding |
| Custom JSON serialization | `CategoryTreeNodeJsonConverter` |
| DI with `ProductSearchEngine` | Singleton registration, constructor injection |
| Fuzzy matching (no libs) | Levenshtein distance in `ProductSearchEngine` |
| Multi-field weighted search | Scoring logic in `ProductSearchEngine` |
| Angular 16+ standalone components | All components use `standalone: true` |
| Reactive forms with validation | `ProductFormComponent` |
| RxJS for API calls + state | `ProductService`, `SearchStateService` |
| Unit test (component or service) | `product.service.spec.ts` or `search-bar.component.spec.ts` |

---

## Folder Structure

```
/
├── README.md
├── SOLUTION.md
├── ProductCatalog.sln
├── ProductCatalog.Core/             # Class library — Models, Repositories, Services, Search
│   ├── ProductCatalog.Core.csproj
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── SeedData.cs
│   ├── Models/
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   └── Category.cs
│   │   └── Dtos/
│   │       ├── ProductDtos.cs
│   │       └── CategoryDtos.cs
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── Repository.cs
│   │   ├── IProductRepository.cs
│   │   ├── ProductRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   └── CategoryRepository.cs
│   ├── Services/
│   │   ├── IProductService.cs
│   │   ├── ProductService.cs
│   │   ├── ICategoryService.cs
│   │   └── CategoryService.cs
│   ├── Search/
│   │   ├── SearchEngine.cs
│   │   ├── ProductSearchEngine.cs
│   │   └── SearchCacheService.cs
│   └── Extensions/
│       └── LinqExtensions.cs
├── ProductCatalog.Api/              # Web API — Controllers, Middleware, DI config
│   ├── ProductCatalog.Api.csproj
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   └── CategoriesController.cs
│   ├── Middleware/
│   │   ├── RequestTimingMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
├── ProductCatalog.Api.Tests/        # Backend tests (xUnit)
│   ├── ProductCatalog.Api.Tests.csproj
│   ├── ProductSearchEngineTests.cs
│   ├── LinqExtensionTests.cs
│   └── ProductComparableTests.cs
└── product-catalog-ui/              # Frontend (Angular CLI)
    ├── angular.json
    └── src/
        └── app/
            ├── models/
            ├── services/
            ├── components/
            └── app.routes.ts
```

---

## Time Estimate

| Phase | Estimated Time |
|-------|---------------|
| Backend implementation | 2.5 - 3 hours |
| Frontend implementation | 2 - 2.5 hours |
| Testing & documentation | 1 hour |
| Video demo recording | 30 min |
| **Total** | **~6 hours** |

---

## Key Design Decisions (Preview for SOLUTION.md)

1. **In-memory collections over EF for repositories**: Satisfies the explicit requirement; EF Core In-Memory is used only for the alternative data path and seeding convenience.
2. **Levenshtein distance for fuzzy search**: Simple, dependency-free approach within the "no external libraries" constraint.
3. **Singleton ProductSearchEngine**: Shared search state and index across requests; thread-safe for read-heavy workloads.
4. **Angular standalone components**: Modern Angular best practice, reduces boilerplate vs. NgModules.
5. **RxJS BehaviorSubject for search state**: Enables reactive search where URL params, filters, and search input all feed into a single stream.
