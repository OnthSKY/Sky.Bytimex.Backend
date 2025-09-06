using Sky.Template.Backend.Core.Requests.Base;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Entities.System;
using Sky.Template.Backend.Infrastructure.Repositories.Base;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IPermissionRepository : IRepository<PermissionEntity, int>
{
   
} 

public class PermissionRepository : Repository<PermissionEntity, int>, IPermissionRepository
{
    public PermissionRepository() : base(new GridQueryConfig<PermissionEntity>
    {
        BaseSql = "SELECT * FROM sys.permissions WHERE is_deleted = FALSE",
        ColumnMappings = new Dictionary<string, ColumnMapping>
        {
            { "name",        new ColumnMapping("name",        typeof(string)) },
            { "description", new ColumnMapping("description", typeof(string)) }
        },
        LikeFilterKeys = new HashSet<string> { "name", "description" },
        SearchColumns = new List<string> { "name", "description" },
        DefaultOrderBy = "created_at DESC"
    })
    { }
}