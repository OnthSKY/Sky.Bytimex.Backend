using System.Reflection;
using Sky.Template.Backend.Core.Attributes;
using Sky.Template.Backend.Core.Exceptions;

namespace Sky.Template.Backend.Core.Utilities;

public static class EntityMetadataHelper
{
    public static string GetTableNameOrThrow<T>()
    {
        var type = typeof(T);
        var attr = type.GetCustomAttribute<TableNameAttribute>();

        if (attr == null)
        {
            // "TableNameAttributeMissing" key'i dil dosyasında şu şekilde tanımlı olmalı:
            // "TableNameAttributeMissing": "{0} sınıfı için tablo adı tanımı (TableNameAttribute) eksik."
            var key = "TableNameAttributeMissing";
            var argument = type.Name;
            throw new NotFoundException($"{key}|{argument}");
        }

        return attr.Name;
    }
}

