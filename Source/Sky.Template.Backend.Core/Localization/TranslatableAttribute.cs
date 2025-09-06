using System;

namespace Sky.Template.Backend.Core.Localization
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TranslatableAttribute : Attribute
    {
        public string TranslationTable { get; }
        public string ForeignKeyColumn { get; }
        public string LanguageColumn { get; }
        public string[] ProjectedColumns { get; }

        public TranslatableAttribute(
            string translationTable,
            string foreignKeyColumn,
            string languageColumn = "language_code",
            params string[] projectedColumns)
        {
            TranslationTable = translationTable;
            ForeignKeyColumn = foreignKeyColumn;
            LanguageColumn = languageColumn;
            ProjectedColumns = projectedColumns;
        }
    }
}
