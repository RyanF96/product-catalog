# Agent Guidelines

## Rules

1. **Do not load entire datasets when a subset will suffice.** If a search term or filter is provided, use it to narrow the result set at the earliest opportunity (e.g., via the search engine or queryable filters). Avoid fetching all records from the database into memory only to discard most of them during later filtering or intersection.

2. **Services must never call other services.** Orchestration that requires context from multiple services must happen at the controller level. A service should only interact with its own dependencies (repositories, utilities, etc.) and must not take a dependency on another service.

3. **Follow SOLID principles.** Write code that is single-responsibility, open for extension but closed for modification, properly abstracted via interfaces, kept small and focused, and dependent on abstractions rather than concrete implementations.

4. **Follow DRY (Don't Repeat Yourself).** Extract duplicated logic into reusable methods, classes, or extensions. If you find yourself writing the same code more than once, abstract it.
