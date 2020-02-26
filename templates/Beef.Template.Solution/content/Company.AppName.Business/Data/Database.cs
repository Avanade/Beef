using Beef.Data.Database;
using System.Data.Common;

namespace Company.AppName.Business.Data
{
    /// <summary>
    /// Represents the <b>Company.AppName</b> database.
    /// </summary>
    public class Database : Database<Database>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Database{T}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The optional data provider.</param>
        public Database(string connectionString, DbProviderFactory? provider = null) : base(connectionString, provider) { }

        /// <summary>
        /// Set the SQL Session Context when the connection is opened.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        public override void OnConnectionOpen(DbConnection dbConnection)
        {
            SetSqlSessionContext(dbConnection);
        }
    }
}