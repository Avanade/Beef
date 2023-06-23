namespace MyEf.Hr.Business.Data;

/// <summary>
/// Represents the <b>My.Hr</b> database.
/// </summary>
public class HrDb : SqlServerDatabase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HrDb"/> class.
    /// </summary>
    public HrDb(Func<SqlConnection> create, ILogger<HrDb>? logger = null) : base(create, logger) { }

    /// <inheritdoc/>
    protected override Task OnConnectionOpenAsync(DbConnection connection, CancellationToken cancellationToken)
        => SetSqlSessionContextAsync(cancellationToken: cancellationToken);
}