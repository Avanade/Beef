namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Beef.Demo</b> database.
    /// </summary>
    public class Database : SqlServerDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        public Database(Func<SqlConnection> create, ILogger<Database>? logger = null) : base(create, logger) { }

        /// <inheritdoc/>
        protected override Task OnConnectionOpenAsync(DbConnection connection, CancellationToken cancellationToken)
            => SetSqlSessionContextAsync(cancellationToken: cancellationToken);
    }
}