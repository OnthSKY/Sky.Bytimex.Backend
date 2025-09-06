using System;

namespace Sky.Template.Backend.Core.Localization
{
    /// <summary>
    /// Represents a column projected from a translation table and its optional alias.
    /// </summary>
    public class TranslationColumn
    {
        public string Column { get; }
        public string Alias { get; }

        public TranslationColumn(string column, string? alias = null)
        {
            Column = column;
            Alias = alias ?? column;
        }
    }
}
