using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.QueryBuilder;

public sealed class EntityMap
{
    public Type? EntityType { get; }
    public IReadOnlyDictionary<string, PropertyInfo?> Columns { get; }
    public IReadOnlyDictionary<string, string> Properties { get; }

    private EntityMap(Type type)
    {
        EntityType = type;
        var cols = type.GetProperties()
            .Select(p => (Prop: p, Attr: p.GetCustomAttribute<DbManager.mColumnAttribute>()))
            .Where(t => t.Attr != null)
            .ToList();
        Columns = cols.ToDictionary(t => t.Attr!.ColumnName, t => (PropertyInfo?)t.Prop, StringComparer.OrdinalIgnoreCase);
        Properties = cols.ToDictionary(t => t.Prop.Name, t => t.Attr!.ColumnName, StringComparer.OrdinalIgnoreCase);
    }

    private EntityMap(IEnumerable<string> columns)
    {
        EntityType = null;
        Columns = columns.ToDictionary(c => c, _ => (PropertyInfo?)null, StringComparer.OrdinalIgnoreCase);
        Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private static readonly ConcurrentDictionary<Type, EntityMap> _cache = new();

    public static EntityMap Get(Type t) => _cache.GetOrAdd(t, ty => new EntityMap(ty));

    public static EntityMap ForColumns(IEnumerable<string> columns) => new EntityMap(columns);
}
