using Sky.Template.Backend.Core.Requests.Base;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Sky.Template.Backend.Core.Utilities;

// TODO: Remove GridQueryBuilder after migration is complete
// Legacy SQL Server specific builder. Use QueryBuilder (Q) for new code.
[Obsolete("Use QueryBuilder instead.")]
public static class GridQueryBuilder
{
    [Obsolete("Use QueryBuilder instead.")]
    public static (string Sql, Dictionary<string, object> Params) Build(
        string baseSql,
        GridRequest request,
        Dictionary<string, string> columnMappings,
        string defaultOrderBy = "created_at DESC",
        HashSet<string>? likeFilterKeys = null,
        List<string>? searchColumns = null
    )
    {
        bool hasWhere = HasMainQueryWhere(baseSql);

        var sql = new StringBuilder(baseSql);

        if (!hasWhere)
        {
            sql.Append(" WHERE 1=1");
        }

        var parameters = new Dictionary<string, object>();

        // 🔍 SearchValue - çoklu kolonlarda LIKE
        if (!string.IsNullOrWhiteSpace(request.SearchValue) && searchColumns?.Any() == true)
        {
            var searchConditions = new List<string>();
            foreach (var col in searchColumns)
            {
                var paramName = $"@Search_{col.Replace(".", "_")}";
                searchConditions.Add($"{col} LIKE {paramName}");
                parameters[paramName] = $"%{request.SearchValue}%";
            }

            sql.Append(" AND (");
            sql.Append(string.Join(" OR ", searchConditions));
            sql.Append(")");
        }

        // 🧩 Filters
        foreach (var kvp in request.Filters)
        {
            if (columnMappings.TryGetValue(kvp.Key, out var column))
            {
                var paramKey = $"@{kvp.Key}";
                if (likeFilterKeys?.Contains(kvp.Key) == true)
                {
                    sql.Append($" AND {column} LIKE {paramKey}");
                    parameters[paramKey] = $"%{kvp.Value}%";
                }
                else
                {
                    if (column.Contains("@"))
                    {
                        sql.Append($" AND {column}");
                    }
                    else
                    {
                        sql.Append($" AND {column} = {paramKey}");
                    }

                    parameters[paramKey] = kvp.Value;
                }
            }
        }

        // 🧾 Order
        string orderBy;

        if (columnMappings.TryGetValue(request.OrderColumn, out var mappedColumn))
        {
            var direction = request.OrderDirection.ToUpperInvariant() == "ASC" ? "ASC" : "DESC";
            orderBy = $"{mappedColumn} {direction}";
        }
        else
        {
            orderBy = defaultOrderBy;
        }

        sql.Append($" ORDER BY {orderBy}");


        // 📄 Paging
        var offset = (request.Page - 1) * request.PageSize;
        sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        return (sql.ToString(), parameters);
    }
    private static bool HasMainQueryWhere(string baseSql)
    {
        var sql = Regex.Replace(baseSql, @"\([^()]*\)", "");
        var fromMatch = Regex.Match(sql, @"\bFROM\b", RegexOptions.IgnoreCase);

        if (fromMatch.Success)
        {
            var fromIndex = fromMatch.Index;
            var afterFrom = sql.Substring(fromIndex);
            return Regex.IsMatch(afterFrom, @"\bWHERE\b", RegexOptions.IgnoreCase);
        }

        return false;
    }

    [Obsolete("Use QueryBuilder instead.")]
    public static string GenerateCountQuery(string baseSql)
    {
        var sql = baseSql;

        // FOR JSON PATH alt sorgusunu ve parantezlerini kaldır
        var forJsonIndex = sql.IndexOf("FOR JSON PATH", StringComparison.OrdinalIgnoreCase);
        if (forJsonIndex > 0)
        {
            var openParenIndex = sql.LastIndexOf('(', forJsonIndex);
            var closeParenIndex = sql.IndexOf(')', forJsonIndex);
            if (openParenIndex >= 0 && closeParenIndex > forJsonIndex)
            {
                sql = sql.Remove(openParenIndex, closeParenIndex - openParenIndex + 1);

                while (sql.Contains(",,"))
                {
                    sql = sql.Replace(",,", ",");
                }

                sql = sql.Trim();
                if (sql.StartsWith(","))
                {
                    sql = sql.Substring(1).TrimStart();
                }
                if (sql.EndsWith(","))
                {
                    sql = sql.Substring(0, sql.Length - 1).TrimEnd();
                }
                while (sql.Contains("  "))
                {
                    sql = sql.Replace("  ", " ");
                }
            }
        }

        var startIndex = sql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
            throw new ArgumentException("BaseSqlMustContainFrom");

        var fromClause = sql.Substring(startIndex);
        var orderIndex = fromClause.IndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
        if (orderIndex > 0)
            fromClause = fromClause.Substring(0, orderIndex);

        return $"SELECT COUNT(*) as count {fromClause}";
    }

}
