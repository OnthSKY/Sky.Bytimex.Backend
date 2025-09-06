using System.Text;
using System.Text.RegularExpressions;
using Sky.Template.Backend.Core.Utilities;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql
{
    public class SqlServerDialect : ISqlDialect
    {
        public string ParameterPrefix => "@";
        public string Quote(string identifier) => $"[{identifier}]";
        public string LikeOperator(bool caseInsensitive) => "LIKE"; // CI için FormatLike kullan
        public string Paginate(int page, int pageSize)
            => $"OFFSET {ParameterPrefix}Offset ROWS FETCH NEXT {ParameterPrefix}PageSize ROWS ONLY";
        public string Top(int top) => $"TOP ({top})";
        public string StripOrderBy(string sql) => Utils.StripOrderByImpl(sql);

        // OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY  (param adları değişken)
        public string StripPaging(string sql)
        {
            return Regex.Replace(
                sql,
                @"\s+OFFSET\s+@[A-Za-z0-9_]+\s+ROWS\s+FETCH\s+NEXT\s+@[A-Za-z0-9_]+\s+ROWS\s+ONLY\s*$",
                "",
                RegexOptions.IgnoreCase
            ).TrimEnd();
        }

        public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";

        // İstersen CI collation uygula: Latin1_General_CI_AI – projene göre değiştirebilirsin
        public string FormatLike(string column, string parameter, bool caseInsensitive)
        {
            if (caseInsensitive)
                return $"{column} COLLATE Latin1_General_CI_AI {LikeOperator(true)} {parameter}";
            return $"{column} {LikeOperator(false)} {parameter}";
        }
    }

    public class PostgreSqlDialect : ISqlDialect
    {
        public string ParameterPrefix => "@";
        public string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
        public string LikeOperator(bool caseInsensitive) => caseInsensitive ? "ILIKE" : "LIKE";
        public string Paginate(int page, int pageSize)
            => $"LIMIT {ParameterPrefix}PageSize OFFSET {ParameterPrefix}Offset";
        public string Top(int top) => $"LIMIT {top}";
        public string StripOrderBy(string sql) => Utils.StripOrderByImpl(sql);

        // LIMIT @PageSize OFFSET @Offset   veya   OFFSET @Offset LIMIT @PageSize
        public string StripPaging(string sql)
        {
            var s = Regex.Replace(
                sql,
                @"\s+LIMIT\s+@[A-Za-z0-9_]+\s+OFFSET\s+@[A-Za-z0-9_]+\s*$",
                "",
                RegexOptions.IgnoreCase
            );
            s = Regex.Replace(
                s,
                @"\s+OFFSET\s+@[A-Za-z0-9_]+\s+LIMIT\s+@[A-Za-z0-9_]+\s*$",
                "",
                RegexOptions.IgnoreCase
            );
            return s.TrimEnd();
        }

        public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";

        public string FormatLike(string column, string parameter, bool caseInsensitive)
        {
            // Postgres'te ILIKE var, ekstra collate gerekmez
            return $"{column} {LikeOperator(caseInsensitive)} {parameter}";
        }
    }

    public class MySqlDialect : ISqlDialect
    {
        public string ParameterPrefix => "@";
        public string Quote(string identifier) => $"`{identifier}`";
        public string LikeOperator(bool caseInsensitive) => "LIKE"; // CI için FormatLike ile collate
        public string Paginate(int page, int pageSize)
            => $"LIMIT {ParameterPrefix}PageSize OFFSET {ParameterPrefix}Offset";
        public string Top(int top) => $"LIMIT {top}";
        public string StripOrderBy(string sql) => Utils.StripOrderByImpl(sql);

        // LIMIT @PageSize OFFSET @Offset  (MySQL 8+ destekler)
        public string StripPaging(string sql)
        {
            return Regex.Replace(
                sql,
                @"\s+LIMIT\s+@[A-Za-z0-9_]+\s+OFFSET\s+@[A-Za-z0-9_]+\s*$",
                "",
                RegexOptions.IgnoreCase
            ).TrimEnd();
        }

        public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";

        // Projene uygun CI collation seç (ör. utf8mb4_general_ci / utf8mb4_unicode_ci)
        public string FormatLike(string column, string parameter, bool caseInsensitive)
        {
            if (caseInsensitive)
                return $"{column} COLLATE utf8mb4_general_ci {LikeOperator(true)} {parameter}";
            return $"{column} {LikeOperator(false)} {parameter}";
        }
    }

    public class SqliteDialect : ISqlDialect
    {
        public string ParameterPrefix => "@";
        public string Quote(string identifier) => $"\"{identifier}\"";
        public string LikeOperator(bool caseInsensitive) => "LIKE";
        public string Paginate(int page, int pageSize)
            => $"LIMIT {ParameterPrefix}PageSize OFFSET {ParameterPrefix}Offset";
        public string Top(int top) => $"LIMIT {top}";
        public string StripOrderBy(string sql) => Utils.StripOrderByImpl(sql);

        // LIMIT @PageSize OFFSET @Offset
        public string StripPaging(string sql)
        {
            return Regex.Replace(
                sql,
                @"\s+LIMIT\s+@[A-Za-z0-9_]+\s+OFFSET\s+@[A-Za-z0-9_]+\s*$",
                "",
                RegexOptions.IgnoreCase
            ).TrimEnd();
        }

        public string CountWrap(string sql) => $"SELECT COUNT(*) FROM ({sql}) t";

        public string FormatLike(string column, string parameter, bool caseInsensitive)
        {
            // SQLite LIKE case-insensitive olabilir; burada ekstra collate eklemiyoruz.
            return $"{column} {LikeOperator(caseInsensitive)} {parameter}";
        }
    }
}
