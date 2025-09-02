using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;

public interface IQueryBuilder
{
    IQueryBuilder From<T>(string? alias = null);
    IQueryBuilder Select(params string[] columns);
    IQueryBuilder Where(string key, string op, object? value);
    IQueryBuilder Where<T>(Expression<Func<T,bool>> predicate);
    IQueryBuilder And(string key, string op, object? value);
    IQueryBuilder Or(string key, string op, object? value);
    IQueryBuilder Join<TLeft,TRight>(string leftKey, string rightKey, string? alias = null);
    IQueryBuilder LeftJoin<TLeft,TRight>(string leftKey, string rightKey, string? alias = null);
    IQueryBuilder RightJoin<TLeft,TRight>(string leftKey, string rightKey, string? alias = null);
    IQueryBuilder GroupBy(params string[] keys);
    IQueryBuilder Having(string key, string op, object? value);
    IQueryBuilder Distinct();
    IQueryBuilder OrderBy(string key);
    IQueryBuilder OrderByDescending(string key);
    IQueryBuilder ThenBy(string key);
    IQueryBuilder ThenByDescending(string key);
    IQueryBuilder Page(int page, int pageSize);
    IQueryBuilder Top(int n);

    Task<List<T>> ToListAsync<T>(CancellationToken ct = default) where T : new();
    Task<T?> FirstOrDefaultAsync<T>(CancellationToken ct = default) where T : new();
    Task<long> CountAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(CancellationToken ct = default);
    Task<int> ExecuteAsync(CancellationToken ct = default);

    (string Sql, Dictionary<string,object> Params) BuildSql();
}
