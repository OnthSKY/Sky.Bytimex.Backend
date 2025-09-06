using System;

namespace Sky.Template.Backend.Core.Localization
{
    public class TranslationConfig
    {
        /// <summary>Full translation table name, e.g. "sys.product_translations"</summary>
        public string TranslationTable { get; set; } = default!;

        /// <summary>FK column name in translation table referencing the main table (e.g. "product_id").</summary>
        public string ForeignKeyColumn { get; set; } = default!;

        /// <summary>Language column name in translation table (e.g. "language_code").</summary>
        public string LanguageColumn { get; set; } = "language_code";

        /// <summary>Main table alias used in SQL (we default to "t" if not present).</summary>
        public string MainAlias { get; set; } = "t";

        /// <summary>Columns to project from translation table.</summary>
        public TranslationColumn[] ProjectedColumns { get; set; } = Array.Empty<TranslationColumn>();
    }
}
