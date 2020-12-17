// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the required CDC model properties.
    /// </summary>
    public interface ICdcModel : IUniqueKey
    {
        /// <summary>
        /// Gets or sets the database CDC <see cref="OperationType"/>.
        /// </summary>
        OperationType DatabaseOperationType { get; set; }
    }
}