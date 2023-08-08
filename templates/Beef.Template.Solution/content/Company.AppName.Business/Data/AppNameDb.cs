namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the <b>Company.AppName</b> database.
/// </summary>
#if (implement_database || implement_sqlserver)
public class AppNameDb : SqlServerDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameDb"/> class.
    /// </summary>
    /// <param name="create">The factory to create the <see cref="SqlConnection"/>.</param>
    /// <param name="logger">The optional <see cref="ILogger"/>.</param>
    public AppNameDb(Func<SqlConnection> create, ILogger<AppNameDb>? logger = null) : base(create, logger) { }
#if (implement_database)

    /// <inheritdoc/>
    protected override Task OnConnectionOpenAsync(DbConnection connection, CancellationToken cancellationToken) => SetSqlSessionContextAsync(cancellationToken: cancellationToken);
#endif
}
#endif
#if (implement_mysql)
public class AppNameDb : MySqlDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameDb"/> class.
    /// </summary>
    /// <param name="create">The factory to create the <see cref="SqlConnection"/>.</param>
    /// <param name="logger">The optional <see cref="ILogger"/>.</param>
    public AppNameDb(Func<MySqlConnection> create, ILogger<AppNameDb>? logger = null) : base(create, logger) { }
}
#endif