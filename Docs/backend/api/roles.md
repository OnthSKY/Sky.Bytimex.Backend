# Roles API

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| `GET` | `/api/roles` | `roles.view` | List roles |
| `POST` | `/api/roles` | `roles.manage` | Create role |

### Example

```http
POST /api/roles HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json

{"name":"Admin"}
```

↩ [Back to API Index](./_index.md) | [Roles Module](../modules/roles.md)
