// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the standard database columns constants.
    /// </summary>
    public static class DatabaseColumns
    {
        /// <summary>
        /// Gets or sets the <b>CreatedDate</b> name.
        /// </summary>
        public static string CreatedDateName { get; set; } = "CreatedDate";

        /// <summary>
        /// Gets or sets the <b>CreatedBy</b> name.
        /// </summary>
        public static string CreatedByName { get; set; } = "CreatedBy";

        /// <summary>
        /// Gets or sets the <b>UpdatedDate</b> name.
        /// </summary>
        public static string UpdatedDateName { get; set; } = "UpdatedDate";

        /// <summary>
        /// Gets or sets the <b>UpdatedBy</b> name.
        /// </summary>
        public static string UpdatedByName { get; set; } = "UpdatedBy";

        /// <summary>
        /// Gets or sets the <b>DeletedDate</b> name.
        /// </summary>
        public static string DeletedDateName { get; set; } = "DeletedDate";

        /// <summary>
        /// Gets or sets the <b>DeletedBy</b> name.
        /// </summary>
        public static string DeletedByName { get; set; } = "DeletedBy";

        /// <summary>
        /// Gets or sets the <b>ReselectRecord</b> name.
        /// </summary>
        public static string ReselectRecordName { get; set; } = "ReselectRecord";

        /// <summary>
        /// Gets or sets the <b>PagingSkip</b> name.
        /// </summary>
        public static string PagingSkipName { get; set; } = "PagingSkip";

        /// <summary>
        /// Gets or sets the <b>PagingTake</b> name.
        /// </summary>
        public static string PagingTakeName { get; set; } = "PagingTake";

        /// <summary>
        /// Gets or sets the <b>PagingCount</b> name.
        /// </summary>
        public static string PagingCountName { get; set; } = "PagingCount";

        /// <summary>
        /// Gets or sets the <b>RowVersion</b> name.
        /// </summary>
        public static string RowVersionName { get; set; } = "RowVersion";

        /// <summary>
        /// Gets or sets the <b>ReturnValue</b> name.
        /// </summary>
        public static string ReturnValueName { get; set; } = "ReturnValue";

        /// <summary>
        /// Gets or sets the <b>TenantId</b> name (used by <see cref="DatabaseBase.SetSqlSessionContext(System.Data.Common.DbConnection)"/>).
        /// </summary>
        public static string SessionContextTenantId { get; set; } = "TenantId";

        /// <summary>
        /// Gets or sets the <b>Username</b> name (used by <see cref="DatabaseBase.SetSqlSessionContext(System.Data.Common.DbConnection)"/>).
        /// </summary>
        public static string SessionContextUsername { get; set; } = "Username";

        /// <summary>
        /// Gets or sets the <b>Timestamp</b> name (used by <see cref="DatabaseBase.SetSqlSessionContext(System.Data.Common.DbConnection)"/>).
        /// </summary>
        public static string SessionContextTimestamp { get; set; } = "Timestamp";

        /// <summary>
        /// Gets or sets the <b>UserId</b> name (used by <see cref="DatabaseBase.SetSqlSessionContext(System.Data.Common.DbConnection)"/>).
        /// </summary>
        public static string SessionContextUserId { get; set; } = "UserId";
    }
}