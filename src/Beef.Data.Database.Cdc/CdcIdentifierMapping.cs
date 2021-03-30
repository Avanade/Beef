// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents the CDC (Change Data Capture) identifier mapping model.
    /// </summary>
    public class CdcIdentifierMapping : IGlobalIdentifier
    {
        /// <summary>
        /// Gets or sets the table schema.
        /// </summary>
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        public string? Table { get; set; }

        /// <summary>
        /// Gets or sets the key represented as string.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the global identifier as a string.
        /// </summary>
        public string? GlobalId { get; set; }
    }

    /// <summary>
    /// Used internally within the <see cref="CdcDataOrchestratorResult{TCdcEntityWrapperColl, TCdcEntityWrapper}"/> to link the <see cref="CdcIdentifierMapping.GlobalId"/> to the related <see cref="Value"/> for a specified <see cref="Property"/>.
    /// </summary>
    public class CdcValueIdentifierMapping : CdcIdentifierMapping
    {
        /// <summary>
        /// Gets or sets the corresponding property name.
        /// </summary>
        public string? Property { get; set; }

        /// <summary>
        /// Gets or sets the related (owning) value.
        /// </summary>
        public object? Value { get; set; }
    }

    /// <summary>
    /// Represents a <see cref="CdcValueIdentifierMapping"/> collection.
    /// </summary>
    public class CdcValueIdentifierMappingCollection : List<CdcValueIdentifierMapping> 
    {
        /// <summary>
        /// Adds the <see cref="CdcValueIdentifierMapping"/> created by the <paramref name="link"/> function only where the <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        /// <param name="condition">The condition value.</param>
        /// <param name="link">The function to the create the linked <see cref="CdcValueIdentifierMapping"/>.</param>
        public async Task AddAsync(bool condition, Func<Task<CdcValueIdentifierMapping>> link)
        {
            if (condition && link != null)
            {
                var item = await link().ConfigureAwait(false);
                Add(item);
            }
        }

        /// <summary>
        /// Gets the <see cref="CdcIdentifierMapping.GlobalId"/> for the specified <paramref name="value"/> and <paramref name="property"/> name.
        /// </summary>
        /// <param name="value">The related (owning) value.</param>
        /// <param name="property">The property name.</param>
        /// <returns>The <see cref="CdcIdentifierMapping.GlobalId"/>.</returns>
        public string GetGlobalId(object value, string property) => this.Single(x => x.Value == value && x.Property == property).GlobalId ?? throw new InvalidOperationException("The GlobalId must not be null.");

        /// <summary>
        /// Invokes the <paramref name="link"/> action where the <paramref name="condition"/> is <c>true</c>.
        /// </summary>
        /// <param name="condition">The condition value.</param>
        /// <param name="link">The action to invoke.</param>
        /// <remarks>Note that this method does not affect the underlying collection and is only enabled to simplify the coding of related conditional invoking.</remarks>
        public void Invoke(bool condition, Action link)
        {
            if (condition && link != null)
                link();
        }
    }
}