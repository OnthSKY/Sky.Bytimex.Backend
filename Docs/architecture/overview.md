# Architecture Overview

```mermaid
graph TD
  A[Frontend: Next.js] -->|REST| B[Backend WebAPI]
  B --> C[Application Layer]
  C --> D[Infrastructure]
  D --> E[(PostgreSQL)]
  D --> F[(Redis Cache)]
```

> Note: For detailed database schema see [SYS Schema](../../Docs/bus_system_schema.md).

See also: [Backend Architecture](backend-architecture.md) and [Frontend Architecture](frontend-architecture.md).

↩ [Back to Architecture Index](./_index.md)
