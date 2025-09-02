# Developer Notes

## Column Mapping
- Each property mapped from SQL must be decorated with `[DbManager.mColumn("column_name")]`.
- Column names are treated case-insensitively and trimmed.
- Duplicate column mappings on the same entity throw `InvalidOperationException` during reflection; use unique column names across inheritance chains.

## Caching
- Apply `[Cacheable]` only at the outermost service level for a call chain.
- Nested calls to `[Cacheable]` methods bypass internal caching to prevent recursion.
- Use `CacheKeyPrefix` values defined in `CacheKeys` and choose appropriate expirations.
