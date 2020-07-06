/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Data.OData;
using Soc = Simple.OData.Client;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the Trip Person data access.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Will not always appear static depending on code-gen options")]
    public partial class TripPersonData : ITripPersonData
    {
        /// <summary>
        /// Gets the <see cref="TripPerson"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="TripPerson"/> identifier (username).</param>
        /// <returns>The selected <see cref="TripPerson"/> object where found; otherwise, <c>null</c>.</returns>
        public Task<TripPerson?> GetAsync(string? id)
        {
            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = ODataMapper.Default.CreateArgs();
                return await TripOData.Default.GetAsync(__dataArgs, id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Creates the <see cref="TripPerson"/> object.
        /// </summary>
        /// <param name="value">The <see cref="TripPerson"/> object.</param>
        /// <returns>A refreshed <see cref="TripPerson"/> object.</returns>
        public Task<TripPerson> CreateAsync(TripPerson value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = ODataMapper.Default.CreateArgs();
                return await TripOData.Default.CreateAsync(__dataArgs, value).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Updates the <see cref="TripPerson"/> object.
        /// </summary>
        /// <param name="value">The <see cref="TripPerson"/> object.</param>
        /// <returns>A refreshed <see cref="TripPerson"/> object.</returns>
        public Task<TripPerson> UpdateAsync(TripPerson value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = ODataMapper.Default.CreateArgs();
                return await TripOData.Default.UpdateAsync(__dataArgs, value).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Deletes the <see cref="TripPerson"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="TripPerson"/> identifier (username).</param>
        public Task DeleteAsync(string? id)
        {
            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = ODataMapper.Default.CreateArgs();
                await TripOData.Default.DeleteAsync(__dataArgs, id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Provides the <see cref="TripPerson"/> entity and OData property mapping.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "By design; as there is a direct relationship")]
        public partial class ODataMapper : ODataMapper<TripPerson, Model.Person, ODataMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ODataMapper"/> class.
            /// </summary>
            public ODataMapper()
            {
                Property(s => s.Id, d => d.UserName).SetUniqueKey(false);
                Property(s => s.FirstName, d => d.FirstName);
                Property(s => s.LastName, d => d.LastName);
                ODataMapperCtor();
            }
            
            /// <summary>
            /// Enables the <see cref="ODataMapper"/> constructor to be extended.
            /// </summary>
            partial void ODataMapperCtor();
        }
    }
}

#pragma warning restore IDE0005
#nullable restore