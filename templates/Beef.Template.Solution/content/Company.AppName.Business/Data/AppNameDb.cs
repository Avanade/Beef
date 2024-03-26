namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the <b>Company.AppName</b> database.
/// </summary>
#if (implement_database || implement_sqlserver)
/// <param name="create">The factory to create the <see cref="SqlConnection"/>.</param>
/// <param name="logger">The optional <see cref="ILogger"/>.</param>
public class AppNameDb(Func<SqlConnection> create, ILogger<AppNameDb>? logger = null) : SqlServerDatabase(create, logger)
{
#if (implement_database)

    /// <inheritdoc/>
    protected override Task OnConnectionOpenAsync(DbConnection connection, CancellationToken cancellationToken) => SetSqlSessionContextAsync(cancellationToken: cancellationToken);
#endif
}
#endif
#if (implement_mysql)
/// <param name="create">The factory to create the <see cref="MySqlConnection"/>.</param>
/// <param name="logger">The optional <see cref="ILogger"/>.</param>
public class AppNameDb(Func<MySqlConnection> create, ILogger<AppNameDb>? logger = null) : MySqlDatabase(create, logger) { }
#endif
#if (implement_postgres)
/// <param name="create">The factory to create the <see cref="NpgsqlConnection"/>.</param>
/// <param name="logger">The optional <see cref="ILogger"/>.</param>
public class AppNameDb(Func<NpgsqlConnection> create, ILogger<AppNameDb>? logger = null) : PostgresDatabase(create, logger) { }
#endif