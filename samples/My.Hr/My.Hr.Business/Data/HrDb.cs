namespace My.Hr.Business.Data;

/// <summary>
/// Represents the <b>My.Hr</b> database.
/// </summary>
public class HrDb(Func<SqlConnection> create, ILogger<HrDb>? logger = null) : SqlServerDatabase(create, logger)
{
    /// <inheritdoc/>
    protected override Task OnConnectionOpenAsync(DbConnection connection, CancellationToken cancellationToken)
        => SetSqlSessionContextAsync(cancellationToken: cancellationToken);
}