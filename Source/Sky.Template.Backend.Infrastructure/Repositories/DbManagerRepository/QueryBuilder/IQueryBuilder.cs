using System.Collections.Generic;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;

public interface IQueryBuilder
{
    IQueryBuilder From<T>(string? alias = null);
    IQueryBuilder Select(params string[] columns);             // default: "*"

    IQueryBuilder WhereRaw(string raw, object? param = null);  // raw snippet + optional anonymous params
    IQueryBuilder WhereEq(string column, object? value);
    IQueryBuilder WhereLike(string column, string pattern, bool caseInsensitive = false);
    IQueryBuilder WhereGroup(System.Action<IQueryBuilder> groupBuilder, string boolean = "AND"); // wraps ( ... )

    IQueryBuilder WithSearch(string? searchValue, IEnumerable<string> searchColumns);
    IQueryBuilder WithFilters(IDictionary<string,string> filters,
                              IDictionary<string,string> columnMappings,
                              ISet<string>? likeFilterKeys = null);

    IQueryBuilder OrderBy(string orderBySql);
    IQueryBuilder OrderByMapped(string? requestedColumn,
                                string direction,
                                IDictionary<string,string> columnMappings,
                                string defaultOrderBy);

    IQueryBuilder Paginate(int page, int pageSize);

    (string Sql, IDictionary<string, object> Params) Build();
    IQueryBuilder Clone();
    string ToCountSql();
}
