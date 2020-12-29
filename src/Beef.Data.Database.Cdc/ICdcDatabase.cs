// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the required CDC database <see cref="OperationType"/>.
    /// </summary>
    public interface ICdcDatabase
    {
        /// <summary>
        /// Gets or sets the database CDC <see cref="OperationType"/>.
        /// </summary>
        OperationType DatabaseOperationType { get; set; }

        /// <summary>
        /// Gets or sets the database tracking hash code.
        /// </summary>
        string? DatabaseTrackingHash { get; set; }
    }
}