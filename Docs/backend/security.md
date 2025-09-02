# Backend Security

- Authentication uses JWT tokens with refresh token support.
- Authorization enforced via `[HasPermission]` attribute.
- Audit fields (`created_by`, `updated_by`) use user ID from JWT claim.

> Warning: never log sensitive data such as passwords or tokens.

↩ [Back to Backend Documentation](./_index.md)
