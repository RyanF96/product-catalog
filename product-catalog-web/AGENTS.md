# Agent Instructions — Product Catalog Web

## Project Overview

Angular 21+ SPA for a Product Catalog Management System. Communicates with a C# ASP.NET Core backend API.

## Tech Stack

- **Framework:** Angular 21+ (standalone components, no NgModules)
- **Language:** TypeScript 5.9+
- **Styling:** Tailwind CSS v4 via PostCSS (`@theme` custom properties)
- **HTTP:** Angular `HttpClient` with functional interceptors
- **State:** Angular Signals (`signal`, `input`, `output`, `computed`)
- **Forms:** Reactive Forms (`FormBuilder`, `FormGroup`, `FormControl`)
- **Testing:** Vitest via `@angular/build:unit-test` (`ng test`)
- **Build:** Angular CLI with Vite dev server

## Architecture

```
src/app/
  core/                 # Singleton services, models, constants, interceptors
    constants/
    interceptors/
    models/
    services/
  features/             # Lazy-loaded feature modules as route configs
    products/
    categories/
  shared/               # Reusable presentational components
    components/
```

## Coding Conventions

### Components
- **Always use standalone components.** No `NgModule` declarations.
- **Always separate HTML templates into `.html` files.** Never use inline `template:` strings.
- Use `templateUrl: './my-component.html'` and `styleUrl: './my-component.scss'`.

### Dependency Injection
- **Always use the `inject()` function.** Do not use constructor parameter injection.
- When injecting multiple services, declare them as `private readonly` fields at the top of the class.

```ts
// ✅ Correct
private readonly productService = inject(ProductService);
private readonly destroyRef = inject(DestroyRef);

// ❌ Incorrect
constructor(private productService: ProductService) {}
```

### Signals
- Use signals for **all component state**.
- Use `input()` for `@Input` equivalents and `output()` for `@Output` equivalents.
- Prefix signal-based fields with nothing special (the codebase uses plain names like `products = signal<Product[]>([])`).
- In templates, always call signals with parentheses: `products()`, `isLoading()`.

```ts
protected readonly products = signal<Product[]>([]);
protected readonly searchTerm = input<string>('');
protected readonly searchChange = output<string>();
```

### HTTP / RxJS
- Use RxJS only for HTTP calls and async operations.
- **Always use `takeUntilDestroyed()` on subscriptions.**
- When subscribing inside methods (not the constructor), pass `DestroyRef` explicitly:

```ts
private readonly destroyRef = inject(DestroyRef);

this.productService.getProducts(params).pipe(
  finalize(() => this.isLoading.set(false)),
  takeUntilDestroyed(this.destroyRef)
).subscribe({...});
```

- `takeUntilDestroyed()` without arguments is only safe in the **constructor** or field initializers.

### Forms
- Use `FormBuilder` via `inject(FormBuilder)`.
- Initialize forms as `readonly` class fields (not in `ngOnInit` or the constructor) unless runtime data is required.

```ts
protected readonly productForm: FormGroup = this.fb.group({
  name: ['', [Validators.required, Validators.maxLength(200)]],
  price: [0, [Validators.required, Validators.min(0)]],
});
```

### Models & API
- Keep TypeScript interfaces in `src/app/core/models/`.
- Align interfaces with the backend OpenAPI spec (e.g., `ProductDto`, `CategoryDto`).
- Query parameters sent to the API should use **PascalCase** to match the OpenAPI spec (`PageNumber`, `PageSize`, `SearchTerm`, etc.).

### Styling
- Use Tailwind utility classes. Custom theme colors are defined in `src/styles.scss`:
  - `primary` / `primary-dark`
  - `danger` / `danger-dark`
  - `success`
  - `warning`
- Shared component class naming: `bg-primary`, `text-danger`, `hover:bg-gray-50`.

### Routing
- Routes are lazy-loaded via `loadComponent` and `loadChildren` in `app.routes.ts` and feature `*.routes.ts` files.
- Use `RouterLink` and `RouterLinkActive` for navigation.

## SOLID Principles

### Single Responsibility Principle (SRP)
- **One component, one job.** A component should either display data (presentational) or orchestrate logic (container), not both.
- **One service, one domain.** `ProductService` handles products. `CategoryService` handles categories. Never mix concerns.
- Keep HTTP calls in services. Keep UI state in components. Keep business logic in utilities or services.

```ts
// ❌ Wrong — component doing too much
export class ProductList {
  loadProducts() { /* HTTP + filtering + sorting + DOM manipulation */ }
}

// ✅ Correct — service handles HTTP, component handles view state
export class ProductList {
  private loadProducts() {
    this.productService.getProducts(params).subscribe(result => {
      this.products.set(result.items);
    });
  }
}
```

### Open/Closed Principle (OCP)
- **Extend behavior without modifying existing code.**
- Use Angular interceptors to add cross-cutting concerns (logging, auth, error handling) without touching individual services.
- Use composition over inheritance for shared behavior.

```ts
// ✅ Correct — new behavior added via interceptor, not by editing every service
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(catchError(err => { /* handle */ }));
};
```

### Liskov Substitution Principle (LSP)
- Subtypes must be substitutable for their base types without altering correctness.
- If a base interface defines `CategoryTreeNode`, any component consuming it should work whether the node has children or not.
- Avoid optional properties that change behavior drastically — make them safe defaults.

```ts
// ✅ Correct — level defaults to 0, so any consumer works
protected get indentation(): number {
  return (this.node().level ?? 0) * 20;
}
```

### Interface Segregation Principle (ISP)
- **Don't force clients to depend on interfaces they don't use.**
- Split large interfaces into smaller, focused ones.
- `ProductCreateRequest` and `ProductUpdateRequest` are separate because update does not need `id` in the body. `ProductSearchParams` only contains search-related fields.

```ts
// ✅ Correct — separate interfaces for separate use cases
export interface ProductCreateRequest { name: string; price: number; }
export interface ProductUpdateRequest { name: string; price: number; }
export interface ProductSearchParams { pageNumber?: number; searchTerm?: string; }
```

### Dependency Inversion Principle (DIP)
- **Depend on abstractions, not concretions.**
- Services depend on `HttpClient` (an abstraction), not a specific HTTP implementation.
- Components depend on service interfaces/contracts, not implementation details.
- Use Angular's DI system to inject abstractions. If a service needs to be swapped (e.g., mock for testing), the consumer shouldn't care.

```ts
// ✅ Correct — component depends on ProductService contract, not how it fetches data
export class ProductList {
  private readonly productService = inject(ProductService);
}
```

## DRY Principle (Don't Repeat Yourself)

- **Extract reusable logic into services, utilities, or shared components.**
- If you find yourself copying the same `pipe(takeUntilDestroyed(...))` pattern, the same validation logic, or the same template structure, extract it.
- Use shared components for common UI patterns (loading spinners, error messages, confirm dialogs).
- Use base service patterns or helper functions for repeated RxJS logic.

```ts
// ❌ Wrong — repeated error handling in every component
this.productService.getProducts().subscribe({
  next: (result) => this.products.set(result.items),
  error: () => this.notificationService.error('Failed to load products')
});

this.categoryService.getCategories().subscribe({
  next: (categories) => this.categories.set(categories),
  error: () => this.notificationService.error('Failed to load categories')
});

// ✅ Correct — centralized error handling via interceptor
// The error interceptor already shows toast notifications.
// Components only handle success state.
this.productService.getProducts().subscribe(result => this.products.set(result.items));
this.categoryService.getCategories().subscribe(categories => this.categories.set(categories));
```

```ts
// ❌ Wrong — duplicate form validation logic across components
getFieldError(fieldName: string): string {
  const control = this.form.get(fieldName);
  if (!control || !control.errors || !control.touched) return '';
  if (control.errors['required']) return `${fieldName} is required`;
  if (control.errors['min']) return `${fieldName} must be at least ${control.errors['min'].min}`;
  return 'Invalid value';
}

// ✅ Correct — extract to a shared utility or form helper
import { getFormFieldError } from '@shared/utils/form-utils';

protected getFieldError(fieldName: string): string {
  return getFormFieldError(this.productForm, fieldName);
}
```

## Testing

- Write unit tests for services using `HttpClientTestingModule`.
- Tests live next to the file under test: `product.service.ts` → `product.service.spec.ts`.
- Run tests with: `npm test` (or `ng test --watch=false` for CI).

## Build & Run

```bash
npm install
npm start      # Dev server on http://localhost:4200
npm run build  # Production build
npm test       # Run unit tests
```

## Important Notes

- **API Base URL:** Configured in `src/app/core/constants/api.constants.ts`. Update if the backend port changes.
- **CORS:** The backend must allow `http://localhost:4200`.
- **No inline templates.** Always extract HTML to a separate file.
- **No constructor injection.** Always use `inject()`.
- **No manual change detection tricks.** Signals handle reactivity automatically.
