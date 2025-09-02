using Sky.Template.Backend.Infrastructure.Entities.Stock;
using Sky.Template.Backend.Infrastructure.Repositories.Base;
using Sky.Template.Backend.Infrastructure.Configs.Stock;

namespace Sky.Template.Backend.Infrastructure.Repositories;

public interface IStockMovementRepository : IRepository<StockMovementEntity, Guid>
{
}

public class StockMovementRepository : Repository<StockMovementEntity, Guid>, IStockMovementRepository
{
    public StockMovementRepository() : base(new GridQueryConfig<StockMovementEntity>
    {
        BaseSql = "SELECT * FROM sys.stock_movements sm WHERE sm.is_deleted = FALSE",
        ColumnMappings = StockMovementGridFilterConfig.GetColumnMappings(),
        DefaultOrderBy = StockMovementGridFilterConfig.GetDefaultOrder()
    })
    {
    }
}
