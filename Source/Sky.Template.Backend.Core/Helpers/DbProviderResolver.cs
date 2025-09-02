using Microsoft.Data.SqlClient;
using Npgsql;
using Sky.Template.Backend.Core.Enums;
using System.Data.Common;

namespace Sky.Template.Backend.Core.Helpers;

public static class DbProviderResolver
{
    public static object Npgsql { get; private set; }

    public static DbProviderFactory GetFactory(DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.SqlServer => SqlClientFactory.Instance,
        DatabaseProvider.PostgreSql => NpgsqlFactory.Instance,
        DatabaseProvider.MySql => MySql.Data.MySqlClient.MySqlClientFactory.Instance,
        _ => throw new NotSupportedException("UnsupportedProvider")
    };
}
