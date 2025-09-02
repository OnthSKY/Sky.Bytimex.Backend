using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Configs.Sales;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IReturnRepository : IRepository<ReturnEntity, Guid>
{
}

public class ReturnRepository : Repository<ReturnEntity, Guid>, IReturnRepository
{
    public ReturnRepository() : base(new GridQueryConfig<ReturnEntity>
    {
        BaseSql = "SELECT * FROM sys.returns r WHERE r.is_deleted = FALSE",
        ColumnMappings = ReturnGridFilterConfig.GetColumnMappings(),
        LikeFilterKeys = ReturnGridFilterConfig.GetLikeFilterKeys(),
        SearchColumns = ReturnGridFilterConfig.GetSearchColumns(),
        DefaultOrderBy = ReturnGridFilterConfig.GetDefaultOrder()
    })
    {
    }
}
