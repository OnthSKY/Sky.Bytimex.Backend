# Backend Caching

Caching uses Redis and the `[Cacheable]` attribute.

| Cache Key | TTL | Notes |
|-----------|-----|-------|
| `user:{id}` | 15m | User profiles |
| `permissions:map` | 1h | Permission lookup |

> Note: cache invalidation occurs via business events or timeout.

↩ [Back to Backend Documentation](./_index.md)
