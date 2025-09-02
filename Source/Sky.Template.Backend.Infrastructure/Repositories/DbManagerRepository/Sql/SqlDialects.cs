using System.Text.RegularExpressions;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;

public class SqlServerDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"[{identifier}]";
    public string LikeOperator(bool caseInsensitive) => "LIKE"; // case-insens handled via collations
    public string Paginate(int page, int pageSize)
        => $"OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
    public string Top(int top) => $"TOP ({top})";
    public string StripOrderBy(string sql)
        => Regex.Replace(sql, @"ORDER\s+BY[\s\S]*", string.Empty, RegexOptions.IgnoreCase);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}

public class PostgreSqlDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"" )}\"";
    public string LikeOperator(bool caseInsensitive) => caseInsensitive ? "ILIKE" : "LIKE";
    public string Paginate(int page, int pageSize)
        => $"LIMIT {pageSize} OFFSET {(page - 1) * pageSize}";
    public string Top(int top) => $"LIMIT {top}";
    public string StripOrderBy(string sql)
        => Regex.Replace(sql, @"ORDER\s+BY[\s\S]*", string.Empty, RegexOptions.IgnoreCase);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}

public class MySqlDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"`{identifier}`";
    public string LikeOperator(bool caseInsensitive) => caseInsensitive ? "LIKE" : "LIKE"; // MySql uses COLLATE for case insens
    public string Paginate(int page, int pageSize)
        => $"LIMIT {pageSize} OFFSET {(page - 1) * pageSize}";
    public string Top(int top) => $"LIMIT {top}";
    public string StripOrderBy(string sql)
        => Regex.Replace(sql, @"ORDER\s+BY[\s\S]*", string.Empty, RegexOptions.IgnoreCase);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}

public class SqliteDialect : ISqlDialect
{
    public string ParameterPrefix => "@";
    public string Quote(string identifier) => $"\"{identifier}\"";
    public string LikeOperator(bool caseInsensitive) => "LIKE";
    public string Paginate(int page, int pageSize)
        => $"LIMIT {pageSize} OFFSET {(page - 1) * pageSize}";
    public string Top(int top) => $"LIMIT {top}";
    public string StripOrderBy(string sql)
        => Regex.Replace(sql, @"ORDER\s+BY[\s\S]*", string.Empty, RegexOptions.IgnoreCase);
    public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";
}
