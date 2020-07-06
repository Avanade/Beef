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
using Beef.Data.EntityFrameworkCore;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the Contact data access.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Will not always appear static depending on code-gen options")]
    public partial class ContactData : IContactData
    {
        #region Private
        #pragma warning disable CS0649 // Defaults to null by design; can be overridden in constructor.

        private readonly Func<IQueryable<EfModel.Contact>, IEfDbArgs, IQueryable<EfModel.Contact>>? _getAllOnQuery;

        #pragma warning restore CS0649
        #endregion

        /// <summary>
        /// Gets the <see cref="Contact"/> collection object that matches the selection criteria.
        /// </summary>
        /// <returns>A <see cref="ContactCollectionResult"/>.</returns>
        public Task<ContactCollectionResult> GetAllAsync()
        {
            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                ContactCollectionResult __result = new ContactCollectionResult();
                var __dataArgs = EfMapper.Default.CreateArgs();
                __result.Result = EfDb.Default.Query(__dataArgs, q => _getAllOnQuery?.Invoke(q, __dataArgs) ?? q).SelectQuery<ContactCollection>();
                return await Task.FromResult(__result).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Gets the <see cref="Contact"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <returns>The selected <see cref="Contact"/> object where found; otherwise, <c>null</c>.</returns>
        public Task<Contact?> GetAsync(Guid id)
        {
            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = EfMapper.Default.CreateArgs();
                return await EfDb.Default.GetAsync(__dataArgs, id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Creates the <see cref="Contact"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/> object.</param>
        /// <returns>A refreshed <see cref="Contact"/> object.</returns>
        public Task<Contact> CreateAsync(Contact value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = EfMapper.Default.CreateArgs();
                return await EfDb.Default.CreateAsync(__dataArgs, value).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Updates the <see cref="Contact"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/> object.</param>
        /// <returns>A refreshed <see cref="Contact"/> object.</returns>
        public Task<Contact> UpdateAsync(Contact value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = EfMapper.Default.CreateArgs();
                return await EfDb.Default.UpdateAsync(__dataArgs, value).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Deletes the <see cref="Contact"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        public Task DeleteAsync(Guid id)
        {
            return DataInvoker.Default.InvokeAsync(this, async () =>
            {
                var __dataArgs = EfMapper.Default.CreateArgs();
                await EfDb.Default.DeleteAsync(__dataArgs, id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Provides the <see cref="Contact"/> entity and Entity Framework <see cref="EfModel.Contact"/> property mapping.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "By design; as there is a direct relationship")]
        public partial class EfMapper : EfDbMapper<Contact, EfModel.Contact, EfMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EfMapper"/> class.
            /// </summary>
            public EfMapper()
            {
                Property(s => s.Id, d => d.ContactId).SetUniqueKey(true);
                Property(s => s.FirstName, d => d.FirstName);
                Property(s => s.LastName, d => d.LastName);
                AddStandardProperties();
                EfMapperCtor();
            }
            
            /// <summary>
            /// Enables the <see cref="EfMapper"/> constructor to be extended.
            /// </summary>
            partial void EfMapperCtor();
        }
    }
}

#pragma warning restore IDE0005
#nullable restore