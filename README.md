# Product Catalog

A full-stack Product Catalog Management System with a C# ASP.NET Core backend and an Angular frontend.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) (with npm)
- [Angular CLI](https://angular.dev/tools/cli)

### Start the Backend

```bash
cd product-catalog-api

dotnet build ProductCatalog.sln
cd ProductCatalog.Api
dotnet run
```

The API will be available at `http://localhost:5111`  
Swagger UI: `http://localhost:5111/swagger`

### Start the Frontend

```bash
cd product-catalog-web

npm install
npm start
```

The Angular app will be available at `http://localhost:4200` and will automatically reload when you change any source files.

> **Note:** The backend must be running for the frontend to work, and CORS is already configured to allow `http://localhost:4200`.

---

## Backend Overview

The backend is an **ASP.NET Core Web API** built with .NET 10 and C# 14. It provides a RESTful API for managing products and categories using an in-memory data store.

### Key Features

- **Products:** Create, read, update, delete, search, filter, and paginate products.
- **Categories:** Manage hierarchical categories with tree-view support.
- **Search:** Fuzzy search (Levenshtein distance) across product names, SKUs, and descriptions.
- **Filtering:** Filter by category, price range, and stock availability.
- **Architecture:** Clean separation with generic repositories, services, custom middleware (request timing, exception handling), and a simple TTL cache.
- **Data:** EF Core In-Memory database for products and pure in-memory collections for categories, seeded with sample data on startup.

### Project Structure

```
product-catalog-api/
├── ProductCatalog.Api/      # Web API layer (controllers, middleware, DI registration)
├── ProductCatalog.Core/     # Business logic (models, DTOs, repositories, services, search)
└── ProductCatalog.Api.Tests/# xUnit tests
```

---

## Frontend Overview

The frontend is an **Angular 21+ SPA** that consumes the backend API to provide a user-friendly interface for managing the product catalog.

### Key Features

- **Product Management:** Browse, search, filter, and edit products through a responsive UI.
- **Category Management:** View flat and hierarchical category trees.
- **Reactive UI:** Built with Angular Signals for state management and reactive forms for data entry.
- **Styling:** Styled with Tailwind CSS v4 using custom theme properties.

### Tech Stack

- Angular 21+ (standalone components, no NgModules)
- TypeScript 5.9+
- Tailwind CSS v4
- Vitest for unit testing

### Project Structure

```
product-catalog-web/src/app/
├── core/       # Services, models, constants, HTTP interceptors
├── features/   # Lazy-loaded feature routes (products, categories)
└── shared/     # Reusable presentational components
```
