# API Docs Audit Report

## Summary

| File | Changes |
|---|---|
| admin.md | Completed PermissionController section and cleaned formatting |
| README.md | Added last updated date and regeneration instructions |
| openapi.yaml | Added description metadata and operationId/summary/description for all operations |
| AUDIT_REPORT.md | Initial audit report added |

## Missing Items / TODOs

- Numerous controllers exist in the codebase that are not yet documented (e.g., AdminAuthController, AdminUserController, etc.).
- Model definitions remain placeholders with `// TODO` fields.
- Endpoints requiring additional query or body parameter details should be refined.

## Validation

- OpenAPI lint: `npx @redocly/cli@latest lint docs/api/openapi.yaml` *(failed: 403 Forbidden)*
- dotnet tests: `dotnet test` *(failed: dotnet not installed)*

