namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;

public static class Q
{
    public static IQueryBuilder From<T>(string? alias = null)
    {
        var qb = new QueryBuilder(DbManager.Dialect);
        return qb.From<T>(alias);
    }
}
