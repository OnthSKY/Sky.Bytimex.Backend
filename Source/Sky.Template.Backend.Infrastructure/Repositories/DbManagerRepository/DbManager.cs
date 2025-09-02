using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Sky.Template.Backend.Core.Enums;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Core.Utilities;
using Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository.Sql;

namespace Sky.Template.Backend.Infrastructure.Repositories.DbManagerRepository;

public static class DbManager
{
    private static readonly string _connectionString;
    private static readonly DbProviderFactory _factory;
    public static ISqlDialect Dialect { get; }
    static DbManager()
    {
        IConfigurationRoot configuration = (IConfigurationRoot)Utils.GetConfiguration();

        var dbConfig = configuration.GetSection("DatabaseConnection");
        _connectionString = dbConfig["ConnectionString"];
        var provider = Enum.TryParse<DatabaseProvider>(dbConfig["Provider"], out var p) ? p : DatabaseProvider.SqlServer;

        (DbProviderFactory factory, ISqlDialect dialect) cfg = provider switch
        {
            DatabaseProvider.SqlServer => (SqlClientFactory.Instance, new SqlServerDialect()),
            DatabaseProvider.PostgreSql => (Npgsql.NpgsqlFactory.Instance, new PostgreSqlDialect()),
            DatabaseProvider.MySql => (MySqlClientFactory.Instance, new MySqlDialect()),
            _ => throw new NotSupportedException("UnsupportedProvider")
        };

        _factory = cfg.factory;
        Dialect = cfg.dialect;
    }


    #region Execute - NonQuery

    public static async Task<bool> ExecuteNonQueryAsync(string query, object parameters, string? accountSchema = null, [CallerMemberName] string? caller = null)
    {
        query = ReplaceSchemaPlaceholder(query, accountSchema);
        return await ExecuteWithCatchAsync(query, parameters, caller, async (command) =>
        {
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        });
    }

    public static bool ExecuteNonQuery(string query, object parameters, string? accountSchema = null, [CallerMemberName] string? caller = null)
    {
        query = ReplaceSchemaPlaceholder(query, accountSchema);
        return ExecuteWithCatch(query, parameters, caller, (command) =>
        {
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        });
    }
    public static async Task<bool> ExecuteTransactionNonQueryWithAsync(string query, object parameters, DbConnection? connection = null, DbTransaction? transaction = null, string? accountSchema = null, [CallerMemberName] string? caller = null)
    {
        query = ReplaceSchemaPlaceholder(query, accountSchema);

        bool shouldDispose = false;
        var internalConnection = connection;

        if (internalConnection == null)
        {
            internalConnection = _factory.CreateConnection();
            internalConnection.ConnectionString = _connectionString;
            await internalConnection.OpenAsync();
            shouldDispose = true;
        }

        try
        {
            return await ExecuteTransactionWithCatchAsync(query, parameters, internalConnection, transaction, caller, async (command) =>
            {
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            });
        }
        finally
        {
            if (shouldDispose && internalConnection != null)
            {
                await internalConnection.DisposeAsync();
            }
        }
    }


    private static async Task<T> ExecuteTransactionWithCatchAsync<T>(
     string query, object parameters, DbConnection connection, DbTransaction? transaction,
     string? caller, Func<DbCommand, Task<T>> action)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = query;
            AddParameters(command, parameters);
            return await action(command);
        }
        catch (Exception ex)
        {
            throw CreateDbException(ex, "ExecuteTransactionFailed", query, parameters, caller ?? "UnknownCaller");
        }
    }



    #endregion

    public static async Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object> parameters, string? accountSchema = null, [CallerMemberName] string? caller = null)
    {
        query = ReplaceSchemaPlaceholder(query, accountSchema);
        return await ExecuteWithCatchAsync(query, parameters, caller, async command =>
        {
            var result = await command.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? default! : (T)Convert.ChangeType(result, typeof(T));
        });
    }

    #region Execute - Transaction

    public static async Task<bool> ExecuteTransactionalNonQueryAsync(
     string query,
     Dictionary<string, object> parameters,
     int expectedRowCount = 1,
     [CallerMemberName] string? caller = null)
    {
        query = ReplaceSchemaPlaceholder(query, null);

        await using var connection = _factory.CreateConnection();
        connection.ConnectionString = _connectionString;
        await connection.OpenAsync();

        await using var transaction = connection.BeginTransaction();

        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = query;
            AddParameters(command, parameters);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == expectedRowCount)
            {
                transaction.Commit();
                return true;
            }

            transaction.Rollback();
            return false;
        }
        catch (Exception ex)
        {
            try { transaction?.Rollback(); } catch { }
            throw CreateDbException(ex, "ExecuteTransactionFailed", query, parameters, caller);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }



    #endregion

    #region Execute - Query
    public static async Task<List<T>> ReadAsync<T>(
       string query,
       Dictionary<string, object> parameters,
       DbConnection connection,
       DbTransaction transaction,
       string? accountSchema = null) where T : new()
    {
        query = ReplaceSchemaPlaceholder(query, accountSchema);
        var columnMappings = GetColumnMappings<T>();

        DataTable table = await ExecuteTransactionWithCatchAsync(query, parameters, connection, transaction, nameof(ReadAsync), async (command) =>
        {
            await using var reader = await command.ExecuteReaderAsync();
            DataTable dt = new();
            dt.Load(reader);
            return dt;
        });

        return MapResultToModelList<T>(table, columnMappings);
    }

    public static async Task<List<T>> ReadAsync<T>(string query, Dictionary<string, object> parameters, string? accountSchema = null) where T : new()
   {
        query = ReplaceSchemaPlaceholder(query, accountSchema);
        var columnMappings = GetColumnMappings<T>();
        DataTable table = await ExecuteWithCatchAsync(query, parameters, null, async (command) =>
        {
            using var reader = await command.ExecuteReaderAsync();
            DataTable dt = new();
            dt.Load(reader);
            return dt;
        });
        return MapResultToModelList<T>(table, columnMappings);
    }

    public static List<T> Read<T>(string query, Dictionary<string, object> parameters, string? accountSchema = null) where T : new()
    {
        query = ReplaceSchemaPlaceholder(query, accountSchema);
        var columnMappings = GetColumnMappings<T>();

        return ExecuteWithCatch(query, parameters, nameof(Read), (command) =>
        {
            using var reader = command.ExecuteReader();
            DataTable dt = new();
            dt.Load(reader);
            return MapResultToModelList<T>(dt, columnMappings);
        });
    }


    #endregion

    #region Helpers

    public static Dictionary<string, object> ToParameterDictionary(List<DbParameter> dbParams)
    {
        return dbParams.ToDictionary(p => p.ParameterName, p => p.Value ?? DBNull.Value);
    }
    public static Dictionary<string, object> ToParameterDictionary(List<SqlParameter> sqlParams)
    => ToParameterDictionary(sqlParams.Cast<DbParameter>().ToList());

    private static string ReplaceSchemaPlaceholder(string query, string accountSchema)
    {
        string schema = string.IsNullOrEmpty(accountSchema) ? "sys" : accountSchema;
        return query.Replace("$db.", $"{schema}.");
    }

    private static void AddParameters(DbCommand command, object parameters)
    {
        if (parameters == null) return;

        if (parameters is Dictionary<string, object> dict)
        {
            foreach (var param in dict)
            {
                var p = command.CreateParameter();
                p.ParameterName = param.Key;
                p.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(p);
            }
        }
    }


    private static Dictionary<string, PropertyInfo> GetColumnMappings<T>() where T : new()
    {
        var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        var mappings = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<mColumnAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.ColumnName)) continue;

            var key = attr.ColumnName.Trim().ToLowerInvariant();
            if (mappings.ContainsKey(key))
            {
                var existing = mappings[key];
                throw new InvalidOperationException($"Duplicate column mapping '{key}' on type '{typeof(T).Name}'. Properties: {existing.Name}, {prop.Name}");
            }

            mappings[key] = prop;
        }

        return mappings;
    }

    private static List<T> MapResultToModelList<T>(DataTable table, Dictionary<string, PropertyInfo> mappings) where T : new()
    {
        var list = new List<T>();

        foreach (DataRow row in table.Rows)
        {
            T obj = new();

            foreach (DataColumn col in table.Columns)
            {
                var key = col.ColumnName.Trim().ToLowerInvariant();
                if (!mappings.TryGetValue(key, out var prop))
                    continue;

                object value = row[col];

                try
                {
                    if (value == DBNull.Value)
                    {
                        prop.SetValue(obj, null);
                        continue;
                    }

                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    object? converted;

                    if (targetType.IsGenericType &&
                        targetType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var jsonStr = value.ToString(); // <-- burası kritik
                        var elementType = targetType.GetGenericArguments()[0];
                        var listType = typeof(List<>).MakeGenericType(elementType);
                        converted =  JsonSerializer.Deserialize(jsonStr!, listType);
                    }
                    else
                    {
                        converted = targetType switch
                        {
                            Type t when t.IsEnum => Enum.Parse(t, value.ToString() ?? "0"),
                            Type t when t == typeof(Guid) => Guid.Parse(value.ToString()!),
                            Type t when t == typeof(DateTimeOffset) => DateTimeOffset.Parse(value.ToString()!),
                            Type t when t == typeof(TimeSpan) => TimeSpan.Parse(value.ToString()!),
                            Type t when t == typeof(string) => value.ToString(),
                            Type t when t.IsClass && value is string s =>
                                 JsonSerializer.Deserialize(s, t),
                            _ => Convert.ChangeType(value, targetType)
                        };
                    }

                    prop.SetValue(obj, converted);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Mapping error on column '{col.ColumnName}' to property '{prop.Name}' ({prop.PropertyType.Name}): {ex.Message}", ex);
                }
            }

            list.Add(obj);
        }

        return list;
    }


    private static object? ConvertToPropertyType(object value, Type propertyType)
    {
        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return Convert.ChangeType(value, targetType);
    }


    private static async Task<T> ExecuteWithCatchAsync<T>(string query, object parameters, string? caller, Func<DbCommand, Task<T>> action)
    {
        try
        {
            await using var connection = _factory.CreateConnection();
            connection.ConnectionString = _connectionString;
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = query;
            AddParameters(command, parameters);

            return await action(command);
        }
        catch (Exception ex)
        {
            throw CreateDbException(ex, "ExecuteQueryAsyncFailed", query, parameters, caller ?? "UnknownCaller");
        }
    }
    private static T ExecuteWithCatch<T>(string query, object parameters, string? caller, Func<DbCommand, T> action)
    {
        try
        {
            using var connection = _factory.CreateConnection();
            connection.ConnectionString = _connectionString;
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = query;
            AddParameters(command, parameters);

            return action(command);
        }
        catch (Exception ex)
        {
            throw CreateDbException(ex, "ExecuteQueryFailed", query, parameters, caller ?? "UnknownCaller");
        }
    }

    public static async Task ExecuteInTransactionAsync(Func<DbConnection, DbTransaction, Task> transactionalWork, [CallerMemberName] string? caller = null)
    {
        await using var connection = _factory.CreateConnection();
        connection.ConnectionString = _connectionString;
        await connection.OpenAsync();
        await using var transaction = connection.BeginTransaction();

        try
        {
            await transactionalWork(connection, transaction);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            try { transaction.Rollback(); } catch { }
            throw new DatabaseException("TransactionFailed", ex, callerMethod: caller);
        }
    }

    private static DatabaseException CreateDbException(Exception ex, string key, string query, object parameters, string caller)
    {
        return new DatabaseException(
            messageKey: key,
            innerException: ex,
            query: query,
            parameters: parameters,
            callerMethod: caller
        );
    }

    #endregion

    [AttributeUsage(AttributeTargets.Property)]
    public class mColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public mColumnAttribute(string columnName) => ColumnName = columnName;
    }

    public static dynamic GetDbValue<T>(T value)
    {
        if (value == null) return DBNull.Value;
        if (value is string str && string.IsNullOrWhiteSpace(str)) return DBNull.Value;
        if (value is DateTime dt && dt == default) return DBNull.Value;
        if (value is int i && i == default) return 0;

        return value;
    }
}
