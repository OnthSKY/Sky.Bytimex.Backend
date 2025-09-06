using Sky.Template.Backend.Core.Requests.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sky.Template.Backend.Core.Utilities
{
    public sealed record ColumnMapping
    {
        public string Column { get; init; }
        public Type DataType { get; init; }
        public bool CaseInsensitiveLike { get; init; } = true;

        public ColumnMapping(string column, Type dataType, bool caseInsensitiveLike = true)
            => (Column, DataType, CaseInsensitiveLike) = (column, dataType, caseInsensitiveLike);
    }

    public static class GridQueryBuilder
    {
        public static (string Sql, Dictionary<string, object> Params) Build(
            string baseSql,
            GridRequest request,
            Dictionary<string, ColumnMapping> columnMappings,
            string defaultOrderBy,
            HashSet<string>? likeFilterKeys,
            List<string>? searchColumns,
            ISqlDialect dialect
        )
        {
            var prefix = dialect.ParameterPrefix; // e.g. "@"
            bool hasWhere = HasMainQueryWhere(baseSql);
            var sql = new StringBuilder(baseSql);
            if (!hasWhere) sql.Append(" WHERE 1=1");

            var parameters = new Dictionary<string, object>();

            // 🔍 Global search (FormatLike kullan)
            if (!string.IsNullOrWhiteSpace(request.SearchValue) && searchColumns?.Any() == true)
            {
                var conds = new List<string>();
                foreach (var col in searchColumns)
                {
                    var p = $"{prefix}Search_{col.Replace(".", "_")}";
                    conds.Add(dialect.FormatLike(col, p, caseInsensitive: true));
                    parameters[p] = $"%{request.SearchValue}%";
                }
                sql.Append(" AND (").Append(string.Join(" OR ", conds)).Append(")");
            }

            // 🧩 Filters
            if (request.Filters != null)
            {
                foreach (var kv in request.Filters)
                {
                    if (!columnMappings.TryGetValue(kv.Key, out var mapping)) continue;

                    var colSql = mapping.Column;
                    var p = $"{prefix}{kv.Key}";

                    // LIKE filtre (FormatLike)
                    if (likeFilterKeys?.Contains(kv.Key) == true && mapping.DataType == typeof(string))
                    {
                        sql.Append(" AND ").Append(dialect.FormatLike(colSql, p, mapping.CaseInsensitiveLike));
                        parameters[p] = $"%{kv.Value}%";
                        continue;
                    }

                    // hazır koşul: "p.price >= @minPrice" gibi — parametreyi yakala ve değer bağla
                    if (colSql.Contains(prefix, StringComparison.Ordinal))
                    {
                        sql.Append($" AND {colSql}");
                        var fixedParam = ExtractFirstParamName(colSql, prefix);
                        if (!string.IsNullOrEmpty(fixedParam))
                            parameters[fixedParam] = ParseValue(mapping.DataType, kv.Value);
                        continue;
                    }

                    // CSV → IN
                    var parts = SplitCsv(kv.Value);
                    if (parts.Count > 1)
                    {
                        var (inSql, inParams) = BuildInClause(colSql, kv.Key, parts.Select(v => ParseValue(mapping.DataType, v)).ToArray(), dialect);
                        sql.Append($" AND {inSql}");
                        foreach (var pair in inParams) parameters[pair.Key] = pair.Value;
                    }
                    else
                    {
                        sql.Append($" AND {colSql} = {p}");
                        parameters[p] = ParseValue(mapping.DataType, kv.Value);
                    }
                }
            }

            // 🧾 Order
            string orderBy;
            if (!string.IsNullOrWhiteSpace(request.OrderColumn) &&
                columnMappings.TryGetValue(request.OrderColumn, out var mapped))
            {
                var dir = request.OrderDirection?.ToUpperInvariant() == "ASC" ? "ASC" : "DESC";
                orderBy = $"{mapped.Column} {dir}";
            }
            else
            {
                orderBy = defaultOrderBy;
            }
            sql.Append($" ORDER BY {orderBy}");

            // 📄 Paging (dialect)
            var offset = Math.Max(0, (request.Page - 1) * request.PageSize);
            sql.Append(" ").Append(dialect.Paginate(request.Page, request.PageSize));
            parameters[$"{prefix}Offset"] = offset;
            parameters[$"{prefix}PageSize"] = request.PageSize;

            return (sql.ToString(), parameters);
        }

        // === CTE-aware count: ORDER BY + paging temizle, tamamını sar ===
        public static string GenerateCountQuery(string fullSql, ISqlDialect dialect)
        {
            var noOrder = dialect.StripOrderBy(fullSql);
            var noPage = dialect.StripPaging(noOrder);
            return dialect.CountWrap(noPage);
        }

        // helpers
        private static object ParseValue(Type t, string raw)
        {
            if (t == typeof(Guid)) return Guid.Parse(raw);
            if (t == typeof(int)) return int.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(long)) return long.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(decimal)) return decimal.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(double)) return double.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(float)) return float.Parse(raw, CultureInfo.InvariantCulture);
            if (t == typeof(bool)) return raw.Equals("true", StringComparison.OrdinalIgnoreCase) || raw == "1";
            if (t == typeof(DateTime)) return DateTime.Parse(raw, CultureInfo.InvariantCulture);
            return raw; // string, default
        }

        private static (string Sql, Dictionary<string, object> Params) BuildInClause(
            string column, string key, object[] values, ISqlDialect dialect)
        {
            var dict = new Dictionary<string, object>();
            var names = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                var p = $"{dialect.ParameterPrefix}{key}_{i}";
                names.Add(p);
                dict[p] = values[i];
            }
            var sql = $"{column} IN ({string.Join(", ", names)})";
            return (sql, dict);
        }

        private static List<string> SplitCsv(string input) =>
            (input ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        // dialect prefix-aware
        private static string? ExtractFirstParamName(string sql, string prefix)
        {
            var esc = Regex.Escape(prefix);
            var m = Regex.Match(sql, $@"{esc}[A-Za-z0-9_]+");
            return m.Success ? m.Value : null;
        }

        private static bool HasMainQueryWhere(string baseSql)
        {
            var stripped = Regex.Replace(baseSql, @"\([^()]*\)", "");
            var from = Regex.Match(stripped, @"\bFROM\b", RegexOptions.IgnoreCase);
            if (!from.Success) return false;
            var after = stripped.Substring(from.Index);
            return Regex.IsMatch(after, @"\bWHERE\b", RegexOptions.IgnoreCase);
        }
    }
}
