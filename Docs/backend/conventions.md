# Backend Conventions

- Services use constructor-based dependency injection.
- Async methods use the `Async` suffix.
- DTOs live under `Contract` project.
- Use `[ValidationAspect]`, `[HasPermission]`, and `[Cacheable]` attributes where applicable.

> See [service dependencies](../../Docs/service-dependencies.md) for helper patterns.

↩ [Back to Backend Documentation](./_index.md)
