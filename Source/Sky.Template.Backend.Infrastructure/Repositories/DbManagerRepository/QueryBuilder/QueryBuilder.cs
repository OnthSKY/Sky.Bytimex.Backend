using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;

public class QueryBuilder : IQueryBuilder
{
    private readonly ISqlDialect _dialect;
    private readonly List<string> _select = new();
    private string? _from;
    private readonly List<(string Bool, string Sql)> _wheres = new();
    private readonly List<string> _joins = new();
    private string? _orderBy;
    private int? _page;
    private int? _pageSize;
    private readonly Dictionary<string, object> _params = new();
    private int _paramIndex;

    public QueryBuilder(ISqlDialect dialect)
    {
        _dialect = dialect;
    }

    public IQueryBuilder From<T>(string? alias = null)
    {
        var tableAttr = typeof(T).GetCustomAttribute<TableNameAttribute>();
        var table = tableAttr?.Name ?? typeof(T).Name;
        _from = $"{_dialect.Quote(table)} {(alias ?? "t")}";
        return this;
    }

    public IQueryBuilder Select(params string[] columns)
    {
        _select.AddRange(columns);
        return this;
    }

    public IQueryBuilder WhereRaw(string raw, object? param = null)
    {
        _wheres.Add(("AND", raw));
        if (param != null)
        {
            foreach (var prop in param.GetType().GetProperties())
            {
                _params[$"{_dialect.ParameterPrefix}{prop.Name}"] = prop.GetValue(param)!;
            }
        }
        return this;
    }

    public IQueryBuilder WhereEq(string column, object? value)
    {
        var p = AddParam(value);
        _wheres.Add(("AND", $"{column} = {p}"));
        return this;
    }

    public IQueryBuilder WhereLike(string column, string pattern, bool caseInsensitive = false)
    {
        var p = AddParam(pattern);
        _wheres.Add(("AND", $"{column} {_dialect.LikeOperator(caseInsensitive)} {p}"));
        return this;
    }
 
    public IQueryBuilder WhereGroup(Action<IQueryBuilder> groupBuilder, string boolean = "AND")
    {
        var sub = new QueryBuilder(_dialect);
        sub._paramIndex = _paramIndex;
        groupBuilder(sub);
        foreach (var kv in sub._params)
            _params[kv.Key] = kv.Value;
        _paramIndex = sub._paramIndex;
        var sb = new StringBuilder();
        bool first = true;
        foreach (var w in sub._wheres)
        {
            if (!first) sb.Append(' ').Append(w.Bool).Append(' ');
            sb.Append(w.Sql);
            first = false;
        }
        if (sb.Length > 0)
            _wheres.Add((boolean.ToUpperInvariant(), $"({sb})"));
        return this;
    }

    public IQueryBuilder WithSearch(string? searchValue, IEnumerable<string> searchColumns)
    {
        if (string.IsNullOrWhiteSpace(searchValue) || searchColumns == null || !searchColumns.Any())
            return this;
        var p = AddParam($"%{searchValue}%");
        var like = _dialect.LikeOperator(false);
        var parts = searchColumns.Select(c => $"{c} {like} {p}");
        _wheres.Add(("AND", $"({string.Join(" OR ", parts)})"));
        return this;
    }

    public IQueryBuilder WithFilters(IDictionary<string, string> filters, IDictionary<string, string> columnMappings, ISet<string>? likeFilterKeys = null)
    {
        foreach (var kv in filters)
        {
            if (!columnMappings.TryGetValue(kv.Key, out var mapped))
                continue;
            if (mapped.Contains(_dialect.ParameterPrefix))
            {
                _wheres.Add(("AND", mapped));
                var regex = new Regex(Regex.Escape(_dialect.ParameterPrefix) + "[A-Za-z0-9_]+");
                var match = regex.Match(mapped);
                if (match.Success)
                    _params[match.Value] = kv.Value;
            }
            else if (likeFilterKeys?.Contains(kv.Key) == true)
            {
                var p = AddParam($"%{kv.Value}%");
                _wheres.Add(("AND", $"{mapped} {_dialect.LikeOperator(false)} {p}"));
            }
            else
            {
                var p = AddParam(kv.Value);
                _wheres.Add(("AND", $"{mapped} = {p}"));
            }
        }
        return this;
    }

    public IQueryBuilder OrderBy(string orderBySql)
    {
        _orderBy = orderBySql;
        return this;
    }

    public IQueryBuilder OrderByMapped(string? requestedColumn, string direction, IDictionary<string, string> columnMappings, string defaultOrderBy)
    {
        if (requestedColumn != null && columnMappings.TryGetValue(requestedColumn, out var mapped))
        {
            direction = direction.Equals("ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
            _orderBy = $"{mapped} {direction}";
        }
        else
        {
            _orderBy = defaultOrderBy;
        }
        return this;
    }

    public IQueryBuilder Paginate(int page, int pageSize)
    {
        _page = page;
        _pageSize = pageSize;
        return this;
    }

    public (string Sql, IDictionary<string, object> Params) Build()
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ");
        if (_select.Count > 0) sb.Append(string.Join(", ", _select));
        else sb.Append("*");
        sb.Append(" FROM ").Append(_from);
        foreach (var j in _joins) sb.Append(' ').Append(j);
        if (_wheres.Count > 0)
        {
            sb.Append(" WHERE ");
            bool first = true;
            foreach (var w in _wheres)
            {
                if (!first) sb.Append(' ').Append(w.Bool).Append(' ');
                sb.Append(w.Sql);
                first = false;
            }
        }
        if (!string.IsNullOrEmpty(_orderBy))
            sb.Append(" ORDER BY ").Append(_orderBy);
        if (_page.HasValue)
            sb.Append(' ').Append(_dialect.Paginate(_page.Value, _pageSize!.Value));
        var prms = new Dictionary<string, object>(_params);
        if (_page.HasValue)
        {
            prms[$"{_dialect.ParameterPrefix}Offset"] = (_page.Value - 1) * _pageSize!.Value;
            prms[$"{_dialect.ParameterPrefix}PageSize"] = _pageSize!.Value;
        }
        return (sb.ToString(), prms);
    }

    public IQueryBuilder Clone()
    {
        var clone = new QueryBuilder(_dialect)
        {
            _from = _from,
            _orderBy = _orderBy,
            _page = _page,
            _pageSize = _pageSize,
            _paramIndex = _paramIndex
        };
        clone._select.AddRange(_select);
        clone._wheres.AddRange(_wheres);
        clone._joins.AddRange(_joins);
        foreach (var kv in _params) clone._params[kv.Key] = kv.Value;
        return clone;
    }

    public string ToCountSql()
    {
        var clone = (QueryBuilder)Clone();
        clone._select.Clear();
        clone._orderBy = null;
        clone._page = null;
        clone._pageSize = null;
        var (sql, _) = clone.Build();
        var fromIndex = FindTopLevelFrom(sql);
        var from = sql.Substring(fromIndex);
        from = _dialect.StripOrderBy(from);
        return _dialect.CountWrap(from);
    }

    private string AddParam(object? value)
    {
        var name = $"{_dialect.ParameterPrefix}p{_paramIndex++}";
        _params[name] = value ?? DBNull.Value;
        return name;
    }

    private static int FindTopLevelFrom(string sql)
    {
        var upper = sql.ToUpperInvariant();
        int depth = 0;
        bool inString = false;
        for (int i = 0; i < upper.Length - 4; i++)
        {
            var c = upper[i];
            if (c == '\'' ) inString = !inString;
            if (inString) continue;
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (depth == 0 && upper.Substring(i, 4) == "FROM")
                return i;
        }
        return 0;
    }
}
