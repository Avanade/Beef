namespace MyEf.Hr.Business.Data;

/// <summary>
/// Represents the <b>MyEf.Hr</b> database.
/// </summary>
public class HrDb(Func<SqlConnection> create, ILogger<HrDb>? logger = null) : SqlServerDatabase(create, logger)
{
    /// <inheritdoc/>
    protected override Task OnConnectionOpenAsync(DbConnection connection, CancellationToken cancellationToken)
        => SetSqlSessionContextAsync(cancellationToken: cancellationToken);
}