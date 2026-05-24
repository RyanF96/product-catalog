# Product Catalog Management System

A full-stack Product Catalog Management System built for a take-home assignment. The solution demonstrates advanced C# patterns, .NET 10 Web API architecture, and clean separation of concerns.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core Web API, C# 14, .NET 10 |
| Data | EF Core In-Memory, pure in-memory collections (`List<T>`, `Dictionary<K,V>`) |
| Testing | xUnit |

## Architecture

```
HTTP Request → Controller → Service → Repository → Data Store
                                  ↓         ↓
                        ProductSearchEngine   EF In-Memory / List<T>
                        SearchCacheService
```

**Key architectural decisions:**
- **Two-project solution**: `ProductCatalog.Core` (business logic, data) + `ProductCatalog.Api` (HTTP layer)
- **Generic Repository<T>**: Demonstrates generics, interfaces, and in-memory data access
- **Service Layer**: Thick services compose repositories, search engine, and cache via DI
- **Custom middleware**: Built from scratch (request timing, exception handling)

## Project Structure

```
ProductCatalog.sln
├── ProductCatalog.Core/         # Class library — models, repos, services, search
│   ├── Data/
│   │   ├── AppDbContext.cs      # EF Core In-Memory
│   │   └── SeedData.cs          # 23 products, 12 categories
│   ├── Models/
│   │   ├── Entities/
│   │   │   ├── Product.cs       # IComparable<Product>
│   │   │   └── Category.cs      # Hierarchical (ParentCategoryId)
│   │   └── Dtos/
│   │       ├── ProductDtos.cs   # Records: ProductDto, CreateProductRequest, etc.
│   │       └── CategoryDtos.cs  # Records: CategoryDto, CategoryTreeNode
│   ├── Repositories/
│   │   ├── IRepository.cs       # Generic interface
│   │   ├── Repository.cs        # Generic base (pure in-memory List<T>)
│   │   ├── IProductRepository.cs
│   │   ├── ProductRepository.cs # EF Core implementation
│   │   ├── ICategoryRepository.cs
│   │   └── CategoryRepository.cs # Pure in-memory (no EF)
│   ├── Services/
│   │   ├── IProductService.cs
│   │   ├── ProductService.cs    # Business logic, search orchestration
│   │   ├── ICategoryService.cs
│   │   └── CategoryService.cs
│   ├── Search/
│   │   ├── SearchEngine.cs      # Generic fuzzy search (Levenshtein)
│   │   ├── ProductSearchEngine.cs # DI-injected, multi-field weighted
│   │   └── SearchCacheService.cs # Dictionary-based TTL cache
│   └── Extensions/
│       └── LinqExtensions.cs    # FilterByCategory, FilterByPriceRange, etc.
├── ProductCatalog.Api/          # Web API
│   ├── Controllers/
│   │   ├── ProductsController.cs   # Manual model binding demo
│   │   └── CategoriesController.cs # Custom JSON serialization
│   ├── Middleware/
│   │   ├── RequestTimingMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs               # DI registration, seeding
└── ProductCatalog.Api.Tests/    # xUnit tests
    ├── ProductSearchEngineTests.cs
    ├── LinqExtensionTests.cs
    └── ProductComparableTests.cs
```

## Build & Run

```bash
# Clone / navigate to solution
cd ProductCatalog

# Build
 dotnet build ProductCatalog.sln

# Run API
cd ProductCatalog.Api
dotnet run

# Run tests (from solution root)
dotnet test ProductCatalog.Api.Tests

# API will be available at:
#   http://localhost:5000 (or check console output)
#   Swagger UI: http://localhost:5000/swagger
```

## API Endpoints

### Products
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/products` | List with pagination, filters, search |
| `GET` | `/api/products/{id}` | Get single product |
| `POST` | `/api/products` | Create product (model binding) |
| `POST` | `/api/products/manual` | Create product (manual binding demo) |
| `PUT` | `/api/products/{id}` | Update product |
| `DELETE` | `/api/products/{id}` | Delete product |

### Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/categories` | Flat list |
| `GET` | `/api/categories/tree` | Hierarchical tree (custom JSON) |
| `GET` | `/api/categories/{id}` | Get single category |
| `POST` | `/api/categories` | Create category |
| `PUT` | `/api/categories/{id}` | Update category |
| `DELETE` | `/api/categories/{id}` | Delete category |

### Query Parameters for `/api/products`
```
?SearchTerm=laptop        # Fuzzy search (e.g., "lptop" matches "laptop")
&CategoryId=9             # Filter by category
&MinPrice=100&MaxPrice=500
&InStockOnly=true
&PageNumber=1&PageSize=10
```

## Assignment Requirements Covered

| Requirement | Implementation |
|-------------|----------------|
| Custom `Repository<T>` with generics | `IRepository<T>` + `Repository<T>` + specializations |
| Pure in-memory collections | `CategoryRepository` uses `List<Category>` (no EF) |
| Custom LINQ extensions | `FilterByCategory`, `FilterByPriceRange`, `FilterInStock`, `SearchByName`, `Paginate` |
| C# 9+ record types | All DTOs: `ProductDto`, `CategoryDto`, `CreateProductRequest`, etc. |
| Pattern matching validation | `request is { Name: not null and not "", Price: > 0 }` in services |
| Nullable reference types | `<Nullable>enable</Nullable>` project-wide |
| Custom middleware from scratch | `RequestTimingMiddleware`, `ExceptionHandlingMiddleware` |
| Simple caching layer | `SearchCacheService` with `Dictionary<string, CacheEntry>` + TTL |
| Category tree structure | Recursive `BuildTreeNodes` using `ILookup<int?, Category>` |
| `IComparable<Product>` | Sort by Price asc, then Name asc |
| Manual model binding | `POST /api/products/manual` reads `Request.Body` directly |
| Custom JSON serialization | `CategoryTreeNodeJsonConverter` on `CategoryTreeNode` |
| DI with `ProductSearchEngine` | Singleton, injected into `ProductService` |
| Fuzzy matching (no libs) | Levenshtein distance in `SearchEngine<T>` |
| Multi-field weighted search | Name (50%), SKU (30%), Description (20%) |
| xUnit tests | 31 tests: fuzzy search, LINQ extensions, IComparable |
