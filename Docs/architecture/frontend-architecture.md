# Frontend Architecture

The frontend uses Next.js with React and TypeScript.

```mermaid
graph TD
  PAGES[Pages] --> COMP[Components]
  COMP --> HOOKS[Hooks]
  HOOKS --> API[API Client]
  API -->|REST| BACK[(Backend API)]
```

> Note: i18n is handled via `next-i18next` with message keys mirroring backend translations.

↩ [Back to Architecture Index](./_index.md)
