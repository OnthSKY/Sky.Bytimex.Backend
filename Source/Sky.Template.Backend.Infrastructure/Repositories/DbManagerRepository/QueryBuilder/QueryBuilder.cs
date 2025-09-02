using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;

public class QueryBuilder : IQueryBuilder
{
    private readonly ISqlDialect _dialect;
    private readonly ExpressionSqlTranslator _translator;
    private readonly Dictionary<string, EntityMap> _aliases = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _select = new();
    private readonly List<(string Conj,string Sql)> _where = new();
    private readonly List<string> _joins = new();
    private readonly List<string> _groupBy = new();
    private readonly List<string> _having = new();
    private readonly List<string> _order = new();
    private readonly Dictionary<string, object?> _params = new();
    private string? _fromTable;
    private string? _fromAlias;
    private bool _distinct;
    private int? _top;
    private int? _page;
    private int? _pageSize;
    private int _paramIndex;

    public QueryBuilder(ISqlDialect dialect)
    {
        _dialect = dialect;
        _translator = new ExpressionSqlTranslator(dialect);
    }

    public IQueryBuilder From<T>(string? alias = null)
    {
        var map = EntityMap.Get(typeof(T));
        var tableAttr = typeof(T).GetCustomAttribute<TableNameAttribute>();
        _fromTable = tableAttr?.Name ?? typeof(T).Name;
        _fromAlias = alias ?? "t";
        _aliases[_fromAlias] = map;
        return this;
    }

    public IQueryBuilder From(string table, IEnumerable<string> columns, string? alias = null)
    {
        _fromTable = table;
        _fromAlias = alias ?? "t";
        _aliases[_fromAlias] = EntityMap.ForColumns(columns);
        return this;
    }

    public IQueryBuilder Select(params string[] columns)
    {
        foreach (var col in columns)
        {
            _select.Add(ResolveColumn(col));
        }
        return this;
    }

    public IQueryBuilder Where(string key, string op, object? value) => AddCondition("AND", key, op, value);
    public IQueryBuilder And(string key, string op, object? value) => AddCondition("AND", key, op, value);
    public IQueryBuilder Or(string key, string op, object? value) => AddCondition("OR", key, op, value);

    public IQueryBuilder WhereRaw(string sql, params object?[] values) => AddRaw("AND", sql, values);
    public IQueryBuilder AndRaw(string sql, params object?[] values) => AddRaw("AND", sql, values);
    public IQueryBuilder OrRaw(string sql, params object?[] values) => AddRaw("OR", sql, values);

    public IQueryBuilder Where<T>(Expression<Func<T, bool>> predicate)
    {
        var alias = _aliases.FirstOrDefault(a => a.Value.EntityType == typeof(T)).Key;
        if (alias == null) throw new InvalidOperationException("InvalidColumn");
        string Resolver(string prop)
        {
            var map = _aliases[alias];
            if (!map.Properties.TryGetValue(prop, out var col)) throw new InvalidOperationException("InvalidColumn");
            return $"{alias}.{_dialect.Quote(col)}";
        }
        var (sql, prms) = _translator.Translate(predicate, Resolver);
        for (int i = 0; i < prms.Count; i++)
        {
            var nameOld = $"{_dialect.ParameterPrefix}p{i}";
            var name = NextParam(prms[i]);
            sql = sql.Replace(nameOld, name);
        }
        _where.Add(("AND", sql));
        return this;
    }

    public IQueryBuilder Join<TLeft, TRight>(string leftKey, string rightKey, string? alias = null)
        => AddJoin("JOIN", typeof(TLeft), typeof(TRight), leftKey, rightKey, alias);

    public IQueryBuilder LeftJoin<TLeft, TRight>(string leftKey, string rightKey, string? alias = null)
        => AddJoin("LEFT JOIN", typeof(TLeft), typeof(TRight), leftKey, rightKey, alias);

    public IQueryBuilder RightJoin<TLeft, TRight>(string leftKey, string rightKey, string? alias = null)
        => AddJoin("RIGHT JOIN", typeof(TLeft), typeof(TRight), leftKey, rightKey, alias);

    private IQueryBuilder AddJoin(string joinType, Type left, Type right, string leftKey, string rightKey, string? alias)
    {
        var leftAlias = _aliases.First(a => a.Value.EntityType == left).Key;
        var rightAlias = alias ?? $"t{_aliases.Count}";
        var rightMap = EntityMap.Get(right);
        _aliases[rightAlias] = rightMap;
        var leftCol = ResolveColumn($"{leftAlias}.{leftKey}");
        var rightTableAttr = right.GetCustomAttribute<TableNameAttribute>();
        var rightTable = rightTableAttr?.Name ?? right.Name;
        var rightCol = ResolveColumn($"{rightAlias}.{rightKey}");
        _joins.Add($"{joinType} {_dialect.Quote(rightTable)} {rightAlias} ON {leftCol} = {rightCol}");
        return this;
    }

    public IQueryBuilder GroupBy(params string[] keys)
    {
        foreach (var k in keys) _groupBy.Add(ResolveColumn(k));
        return this;
    }

    public IQueryBuilder Having(string key, string op, object? value)
    {
        var column = ResolveColumn(key);
        var expr = BuildOperator(column, op, value);
        _having.Add(expr);
        return this;
    }

    public IQueryBuilder Distinct() { _distinct = true; return this; }

    public IQueryBuilder OrderBy(string key) { _order.Add($"{ResolveColumn(key)} ASC"); return this; }
    public IQueryBuilder OrderByDescending(string key) { _order.Add($"{ResolveColumn(key)} DESC"); return this; }
    public IQueryBuilder ThenBy(string key) => OrderBy(key);
    public IQueryBuilder ThenByDescending(string key) => OrderByDescending(key);

    public IQueryBuilder Page(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > 1000) throw new ArgumentOutOfRangeException("PageOutOfRange");
        _page = page;
        _pageSize = pageSize;
        return this;
    }

    public IQueryBuilder Top(int n) { _top = n; return this; }

    private IQueryBuilder AddCondition(string conj, string key, string op, object? value)
    {
        var column = ResolveColumn(key);
        var expr = BuildOperator(column, op, value);
        _where.Add((conj, expr));
        return this;
    }

    private IQueryBuilder AddRaw(string conj, string sql, params object?[] values)
    {
        var expr = sql;
        foreach (var v in values)
        {
            var p = NextParam(v);
            expr = expr.Replace("?", p, StringComparison.Ordinal);
        }
        _where.Add((conj, expr));
        return this;
    }

    private string BuildOperator(string column, string op, object? value)
    {
        return op.ToLowerInvariant() switch
        {
            "eq" => $"{column} = {NextParam(value)}",
            "ne" => $"{column} <> {NextParam(value)}",
            "lt" => $"{column} < {NextParam(value)}",
            "gt" => $"{column} > {NextParam(value)}",
            "lte" => $"{column} <= {NextParam(value)}",
            "gte" => $"{column} >= {NextParam(value)}",
            "like" => $"{column} {_dialect.LikeOperator(false)} {NextParam(EscapeLike(value?.ToString() ?? string.Empty))} ESCAPE '\\'",
            "ilike" => $"{column} {_dialect.LikeOperator(true)} {NextParam(EscapeLike(value?.ToString() ?? string.Empty))} ESCAPE '\\'",
            "in" => BuildIn(column, value),
            "between" => BuildBetween(column, value),
            "isnull" => $"{column} IS NULL",
            "notnull" => $"{column} IS NOT NULL",
            _ => throw new NotSupportedException("InvalidOperator")
        };
    }

    private string BuildIn(string column, object? value)
    {
        if (value is not IEnumerable en) throw new ArgumentException("InvalidOperator");
        var list = new List<string>();
        foreach (var v in en)
            list.Add(NextParam(v));
        return $"{column} IN ({string.Join(",", list)})";
    }

    private string BuildBetween(string column, object? value)
    {
        if (value is IEnumerable en)
        {
            var vals = en.Cast<object?>().Take(2).ToArray();
            if (vals.Length == 2)
            {
                var p1 = NextParam(vals[0]);
                var p2 = NextParam(vals[1]);
                return $"{column} BETWEEN {p1} AND {p2}";
            }
        }
        throw new ArgumentException("InvalidOperator");
    }

    private string ResolveColumn(string key)
    {
        var alias = _fromAlias!;
        var col = key;
        if (key.Contains('.'))
        {
            var parts = key.Split('.', 2);
            alias = parts[0];
            col = parts[1];
        }
        if (!_aliases.TryGetValue(alias, out var map)) throw new InvalidOperationException("InvalidColumn");
        if (!map.Columns.ContainsKey(col)) throw new InvalidOperationException("InvalidColumn");
        return $"{alias}.{_dialect.Quote(col)}";
    }

    private string NextParam(object? value)
    {
        var name = $"{_dialect.ParameterPrefix}p{_paramIndex++}";
        _params[name] = value ?? DBNull.Value;
        return name;
    }

    private static string EscapeLike(string value)
        => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    public (string Sql, Dictionary<string, object> Params) BuildSql()
    {
        var (sql, prms) = BuildSqlCore(true, true);
        return (sql, prms);
    }

    private (string Sql, Dictionary<string, object> Params) BuildSqlCore(bool includeOrder, bool includePagination)
    {
        var sb = new StringBuilder();
        var topFragment = _top.HasValue ? _dialect.Top(_top.Value) : null;
        sb.Append("SELECT ");
        if (_distinct) sb.Append("DISTINCT ");
        if (topFragment != null && topFragment.StartsWith("TOP")) sb.Append(topFragment + " ");
        if (_select.Any()) sb.Append(string.Join(", ", _select));
        else
        {
            var map = _aliases[_fromAlias!];
            sb.Append(string.Join(", ", map.Columns.Keys.Select(c => $"{_fromAlias}.{_dialect.Quote(c)}")));
        }
        sb.Append($" FROM {_dialect.Quote(_fromTable!)} {_fromAlias}");
        foreach (var j in _joins) sb.Append(" ").Append(j);
        if (_where.Any())
        {
            sb.Append(" WHERE ");
            bool first = true;
            foreach (var w in _where)
            {
                if (!first) sb.Append(" ").Append(w.Conj).Append(" ");
                sb.Append(w.Sql);
                first = false;
            }
        }
        if (_groupBy.Any()) sb.Append(" GROUP BY ").Append(string.Join(", ", _groupBy));
        if (_having.Any()) sb.Append(" HAVING ").Append(string.Join(" AND ", _having));
        if (includeOrder && _order.Any()) sb.Append(" ORDER BY ").Append(string.Join(", ", _order));
        if (includePagination && _page.HasValue) sb.Append(" ").Append(_dialect.Paginate(_page.Value, _pageSize!.Value));
        else if (topFragment != null && topFragment.StartsWith("LIMIT") && includePagination) sb.Append(" ").Append(topFragment);
        return (sb.ToString(), new Dictionary<string, object>(_params));
    }

    public async Task<List<T>> ToListAsync<T>(CancellationToken ct = default) where T : new()
    {
        var (sql, prms) = BuildSql();
        return await DbManager.ReadAsync<T>(sql, prms, null);
    }

    public async Task<T?> FirstOrDefaultAsync<T>(CancellationToken ct = default) where T : new()
    {
        var list = await ToListAsync<T>(ct);
        return list.FirstOrDefault();
    }

    public async Task<long> CountAsync(CancellationToken ct = default)
    {
        var (sql, prms) = BuildSqlCore(false, false);
        var wrapped = _dialect.CountWrap(_dialect.StripOrderBy(sql));
        return await DbManager.ExecuteScalarAsync<long>(wrapped, prms);
    }

    public async Task<bool> ExistsAsync(CancellationToken ct = default)
        => (await CountAsync(ct)) > 0;

    public async Task<int> ExecuteAsync(CancellationToken ct = default)
    {
        var (sql, prms) = BuildSql();
        return await DbManager.ExecuteNonQueryAsync(sql, prms, null) ? 1 : 0;
    }
}

