using System.Text;

namespace Sky.Template.Backend.Core.Localization
{
    public static class TranslationSqlBuilder
    {
        /// <summary>
        /// Builds two LATERAL joins for preferred language and fallback (any language),
        /// and returns a projection string for COALESCE(pt_lang.col, pt_any.col) per column.
        /// </summary>
        public static (string joinsSql, string projectionSql) Build(
            TranslationConfig cfg,
            string langParamName = "@lang")
        {
            if (cfg.ProjectedColumns.Length == 0)
                return (string.Empty, string.Empty);

            var columnList = string.Join(", ", cfg.ProjectedColumns.Select(c => c.Column));

            var joins = new StringBuilder();
            joins.AppendLine($"LEFT JOIN LATERAL (\n    SELECT {columnList}\n    FROM {cfg.TranslationTable}\n    WHERE {cfg.ForeignKeyColumn} = {cfg.MainAlias}.id AND {cfg.LanguageColumn} = {langParamName}\n    LIMIT 1\n) pt_lang ON TRUE");

            joins.AppendLine($"LEFT JOIN LATERAL (\n    SELECT {columnList}\n    FROM {cfg.TranslationTable}\n    WHERE {cfg.ForeignKeyColumn} = {cfg.MainAlias}.id\n    ORDER BY {cfg.LanguageColumn}\n    LIMIT 1\n) pt_any ON TRUE");

            var proj = new StringBuilder();
            foreach (var col in cfg.ProjectedColumns)
            {
                if (proj.Length > 0) proj.Append(",\n       ");
                proj.Append($"COALESCE(pt_lang.{col.Column}, pt_any.{col.Column}) AS {col.Alias}");
            }

            return (joins.ToString(), proj.ToString());
        }
    }
}
