# Users API

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| `GET` | `/api/users` | `users.view` | List users |
| `POST` | `/api/users` | `users.manage` | Create user |

### Example

```http
GET /api/users HTTP/1.1
Authorization: Bearer <token>
```

```json
[
  { "id": "1", "email": "admin@example.com" }
]
```

↩ [Back to API Index](./_index.md) | [Users Module](../modules/users.md)
