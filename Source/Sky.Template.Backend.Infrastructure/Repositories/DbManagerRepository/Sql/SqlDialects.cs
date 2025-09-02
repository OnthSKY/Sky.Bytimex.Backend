using System.Text;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;

public class SqlServerDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"[{identifier}]";
    public string LikeOperator(bool caseInsensitive) => "LIKE"; // case-insens handled via collations
    public string Paginate(int page, int pageSize)
        => $"OFFSET {ParameterPrefix}Offset ROWS FETCH NEXT {ParameterPrefix}PageSize ROWS ONLY";
    public string Top(int top) => $"TOP ({top})";
    public string StripOrderBy(string sql) => StripOrderByImpl(sql);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}

public class PostgreSqlDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"" )}\"";
    public string LikeOperator(bool caseInsensitive) => caseInsensitive ? "ILIKE" : "LIKE";
    public string Paginate(int page, int pageSize)
        => $"LIMIT {ParameterPrefix}PageSize OFFSET {ParameterPrefix}Offset";
    public string Top(int top) => $"LIMIT {top}";
    public string StripOrderBy(string sql) => StripOrderByImpl(sql);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}

public class MySqlDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"`{identifier}`";
    public string LikeOperator(bool caseInsensitive) => caseInsensitive ? "LIKE" : "LIKE"; // MySql uses COLLATE for case insens
    public string Paginate(int page, int pageSize)
        => $"LIMIT {ParameterPrefix}PageSize OFFSET {ParameterPrefix}Offset";
    public string Top(int top) => $"LIMIT {top}";
    public string StripOrderBy(string sql) => StripOrderByImpl(sql);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}

public class SqliteDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"\"{identifier}\"";
    public string LikeOperator(bool caseInsensitive) => "LIKE";
    public string Paginate(int page, int pageSize)
        => $"LIMIT {ParameterPrefix}PageSize OFFSET {ParameterPrefix}Offset";
    public string Top(int top) => $"LIMIT {top}";
    public string StripOrderBy(string sql) => StripOrderByImpl(sql);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}

internal static string StripOrderByImpl(string sql)
{
    var upper = sql.ToUpperInvariant();
    var sb = new StringBuilder();
    int depth = 0;
    bool inString = false;
    for (int i = 0; i < upper.Length; i++)
    {
        var c = upper[i];
        if (c == '\'' ) inString = !inString;
        if (!inString)
        {
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (depth == 0 && i < upper.Length - 8 && upper.Substring(i, 8) == "ORDER BY")
            {
                return sb.ToString().TrimEnd();
            }
        }
        sb.Append(sql[i]);
    }
    return sb.ToString();
}
