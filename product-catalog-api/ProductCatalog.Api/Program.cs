using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Middleware;
using ProductCatalog.Core.Data;
using ProductCatalog.Core.Repositories;
using ProductCatalog.Core.Search;
using ProductCatalog.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────
// SERVICES
// ──────────────────────────────────────────────────────────

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Custom JSON options for all responses
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product Catalog API",
        Version = "v1",
        Description = "RESTful API for managing products and categories"
    });
});

// ── CORS for Angular frontend ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ── EF Core In-Memory ──
// ── EF Core In-Memory ──
// Registered as Singleton so seeded data persists across all scopes.
// Safe for InMemory provider (no real DB connection).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProductCatalogDb"), ServiceLifetime.Singleton);

// ── Repositories ──
// ProductRepository uses EF Core
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// CategoryRepository uses pure in-memory collections (no EF)
// Registered as Singleton since it manages its own in-memory state
builder.Services.AddSingleton<CategoryRepository>();
builder.Services.AddSingleton<ICategoryRepository>(sp => sp.GetRequiredService<CategoryRepository>());

// ── Search & Cache (Singleton — shared state across requests) ──
builder.Services.AddSingleton<ProductSearchEngine>();
builder.Services.AddSingleton<SearchCacheService>();

// ── Services (Scoped — business logic layer) ──
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ──────────────────────────────────────────────────────────
// BUILD APP
// ──────────────────────────────────────────────────────────

var app = builder.Build();

// ── Seed Data ──
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var categoryRepo = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
    await SeedData.InitializeAsync(dbContext, categoryRepo);

    // Build search index from seeded products
    var searchEngine = scope.ServiceProvider.GetRequiredService<ProductSearchEngine>();
    var products = await dbContext.Products.ToListAsync();
    searchEngine.BuildIndex(products);
}

// ── HTTP Pipeline ──

// Custom middleware — registered manually (not using framework helpers)
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<RequestTimingMiddleware>>();
    var middleware = new RequestTimingMiddleware(next, logger);
    await middleware.InvokeAsync(context);
});

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<ExceptionHandlingMiddleware>>();
    var middleware = new ExceptionHandlingMiddleware(next, logger);
    await middleware.InvokeAsync(context);
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();


