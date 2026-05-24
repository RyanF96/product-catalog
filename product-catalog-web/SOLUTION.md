# Frontend Solution — Design Decisions & Trade-offs

## 1. Architecture Overview

The frontend is an **Angular 21+ SPA** built entirely with **standalone components** (no NgModules). It follows a feature-based folder structure with clear separation between core services, feature routes, and shared presentational components.

```
src/app/
├── core/       # Singleton services, models, constants, interceptors
├── features/   # Lazy-loaded feature routes (products, categories)
└── shared/     # Reusable presentational components
```

### Why Standalone Components?

Angular 14+ introduced standalone components, and by Angular 21 they are the default. I chose them because:
- **Less boilerplate** — no `NgModule` declarations, exports, or imports arrays
- **Tree-shakeable** — unused components are dropped from the build
- **Explicit dependencies** — each component imports exactly what it needs

The trade-off is that large applications lose the centralized dependency grouping that modules provided. For this project, the reduced ceremony is worth it.

## 2. State Management with Signals

All component state uses Angular **Signals** (`signal`, `input`, `output`, `computed`).

### Why Signals Over RxJS / NgRx?

- **Simpler mental model** — synchronous reads, automatic fine-grained reactivity
- **No need for a store** — the domain is small (products, categories); a global store adds boilerplate without proportional benefit
- **Framework-native** — Angular handles change detection automatically, no `ChangeDetectorRef` tricks needed

### Where RxJS Is Still Used

RxJS is retained **only for HTTP calls and async operations**. Components subscribe to `HttpClient` observables and immediately map results into signals. This gives us the best of both worlds: RxJS for the network boundary, Signals for UI state.

```typescript
this.productService.getProducts(params).pipe(
  takeUntilDestroyed(this.destroyRef)
).subscribe(result => this.products.set(result.items));
```

## 3. Dependency Injection: `inject()` Function

Every service is injected via the `inject()` function instead of constructor parameters.

### Why `inject()` Over Constructor Injection?

- **Cleaner code** — no constructor boilerplate, especially when injecting 4+ services
- **Works outside constructors** — useful in standalone functions and route resolvers
- **Aligns with modern Angular** — the framework is moving toward function-based APIs

The trade-off is that dependencies are less visible in the class signature. I mitigate this by declaring injected services as `private readonly` fields at the top of the class.

## 4. Styling with Tailwind CSS v4

I used Tailwind CSS v4 via PostCSS with custom theme properties defined in `styles.scss`.

### Why Tailwind Over Component SCSS?

- **Rapid UI development** — no context-switching between HTML and CSS files for small tweaks
- **Consistent design system** — utility classes enforce a constrained set of values (spacing, colors, fonts)
- **Small bundle** — PurgeCSS removes unused utilities in production

### Trade-offs

- **Verbose HTML** — templates can become long strings of utility classes
- **Learning curve** — developers must know the Tailwind vocabulary
- **Reusability** — repeated patterns (e.g., card layouts) are extracted into shared components rather than SCSS mixins

## 5. Forms: Reactive Forms with `FormBuilder`

All forms use Angular **Reactive Forms** initialized via `inject(FormBuilder)` as `readonly` class fields.

### Why Reactive Forms Over Template-Driven Forms?

- **Type safety** — `FormGroup` structure is explicit in TypeScript
- **Testability** — logic is in the class, not scattered across template directives
- **Validation** — synchronous and asynchronous validators are easy to compose

For this project, the forms are simple enough that Reactive Forms are the clear winner.

## 6. Lazy-Loaded Routes

Features are lazy-loaded via `loadComponent` and `loadChildren` in `app.routes.ts`.

```typescript
{
  path: 'products',
  loadComponent: () => import('./features/products/product-list.component').then(m => m.ProductListComponent)
}
```

This keeps the initial bundle small and enforces feature isolation.

## 7. HTTP Interceptors

I used functional `HttpInterceptorFn` interceptors for cross-cutting concerns (error handling, logging). This approach:
- **Doesn't require a class** — pure functions are easier to test
- **Composes cleanly** — multiple interceptors are chained via `withInterceptors([...])`
- **Follows OCP** — new behavior is added without modifying existing services

## 8. Testing with Vitest

Unit tests use Vitest via `@angular/build:unit-test` instead of Karma / Jasmine.

### Why Vitest?

- **Fast** — Vite-based, instant HMR for tests
- **Modern** — native ESM support, better TypeScript integration
- **Aligns with dev server** — Angular CLI 21 uses Vite under the hood

The trade-off is that some older Angular testing utilities are optimized for Jasmine matchers. I used `@testing-library/angular` patterns where possible to keep tests framework-agnostic.

## 9. Key Trade-offs Summary

| Decision | Trade-off |
|----------|-----------|
| Standalone components (no NgModules) | Less centralized dependency grouping; but less boilerplate |
| Signals for state | Simpler reactivity but less powerful than RxJS for complex async streams |
| `inject()` vs constructor injection | Cleaner code but dependencies are less visible in the class signature |
| Tailwind CSS v4 | Verbose HTML; but rapid prototyping and consistent design system |
| Reactive Forms | More TypeScript code than template-driven; but explicit and testable |
| No NgRx / global state store | Simpler architecture; but harder to debug if the app grows significantly |
| Vitest over Karma/Jasmine | Faster and modern; but slightly less ecosystem maturity |
| No inline templates | More files; but separation of concerns and better readability |
