// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System.Threading.Tasks;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the ability to link a value to a global identifier mapping that is provisioned at run-time.
    /// </summary>
    public interface ICdcLinkIdentifierMapping
    {
        /// <summary>
        /// Link any new global identifiers.
        /// </summary>
        /// <param name="coll">The <see cref="CdcValueIdentifierMappingCollection"/>.</param>
        /// <param name="idgen">The <see cref="IStringIdentifierGenerator"/>.</param>
        Task LinkIdentifierMappingsAsync(CdcValueIdentifierMappingCollection coll, IStringIdentifierGenerator idgen);

        /// <summary>
        /// Re-link the new global identifiers.
        /// </summary>
        /// <param name="coll">The <see cref="CdcValueIdentifierMappingCollection"/>.</param>
        void RelinkIdentifierMappings(CdcValueIdentifierMappingCollection coll);
    }
}