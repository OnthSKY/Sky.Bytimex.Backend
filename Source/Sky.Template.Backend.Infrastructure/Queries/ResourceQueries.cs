namespace Sky.Template.Backend.Infrastructure.Queries;

public class ResourceQueries
{
    internal const string GetActiveResources = @"
        SELECT
    r.code AS code,
    r.name AS name,
    r.description,
    '/' || LOWER(r.code) AS path,
    (
        SELECT json_agg(
            json_build_object(
                'code', p.code,
                'action', p.action
            )
        )
        FROM sys.permissions p
        WHERE p.resource_id = r.id AND p.is_deleted = false
    ) AS permissions
FROM sys.resources r
WHERE r.status = 'ACTIVE'";
}