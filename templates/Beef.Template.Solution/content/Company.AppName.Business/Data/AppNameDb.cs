namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the <b>Company.AppName</b> database.
/// </summary>
public class AppNameDb : SqlServerDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameDb"/> class.
    /// </summary>
    /// <param name="create">The factory to create the <see cref="SqlConnection"/>.</param>
    /// <param name="logger">The optional <see cref="ILogger"/>.</param>
    public AppNameDb(Func<SqlConnection> create, ILogger<AppNameDb>? logger = null) : base(create, logger) { }

    /// <inheritdoc/>
    protected override Task OnConnectionOpenAsync(DbConnection connection, CancellationToken cancellationToken) => SetSqlSessionContextAsync(cancellationToken: cancellationToken);
}