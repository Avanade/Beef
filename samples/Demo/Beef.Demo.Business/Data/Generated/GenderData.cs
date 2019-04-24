/*
 * This file is automatically generated; any changes will be lost. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Data.Database;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the Gender data access.
    /// </summary>
    public partial class GenderData : IGenderData
    {
        #region Private

        private Func<Guid, IDatabaseArgs, Task> _getOnBeforeAsync = null;
        private Func<Gender, Guid, Task> _getOnAfterAsync = null;
        private Action<Exception> _getOnException = null;

        private Func<Gender, IDatabaseArgs, Task> _createOnBeforeAsync = null;
        private Func<Gender, Task> _createOnAfterAsync = null;
        private Action<Exception> _createOnException = null;

        private Func<Gender, IDatabaseArgs, Task> _updateOnBeforeAsync = null;
        private Func<Gender, Task> _updateOnAfterAsync = null;
        private Action<Exception> _updateOnException = null;

        #endregion

        /// <summary>
        /// Gets the <see cref="Gender"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <returns>The selected <see cref="Gender"/> object where found; otherwise, <c>null</c>.</returns>
        public Task<Gender> GetAsync(Guid id)
        {
            return DataInvoker<Gender>.Default.InvokeAsync(this, async () =>
            {
                Gender __result = null;
                var __dataArgs = DbMapper.Default.CreateArgs("[Ref].[spGenderGet]");
                if (_getOnBeforeAsync != null) await _getOnBeforeAsync(id, __dataArgs);
                __result = Database.Default.Get(__dataArgs, id);
                if (_getOnAfterAsync != null) await _getOnAfterAsync(__result, id);
                return __result;
            }, new BusinessInvokerArgs { ExceptionHandler = _getOnException });
        }

        /// <summary>
        /// Creates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <returns>A refreshed <see cref="Gender"/> object.</returns>
        public Task<Gender> CreateAsync(Gender value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return DataInvoker<Gender>.Default.InvokeAsync(this, async () =>
            {
                Gender __result = null;
                var __dataArgs = DbMapper.Default.CreateArgs("[Ref].[spGenderCreate]");
                if (_createOnBeforeAsync != null) await _createOnBeforeAsync(value, __dataArgs);
                __result = Database.Default.Create(__dataArgs, value);
                if (_createOnAfterAsync != null) await _createOnAfterAsync(__result);
                return __result;
            }, new BusinessInvokerArgs { ExceptionHandler = _createOnException });
        }

        /// <summary>
        /// Updates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <returns>A refreshed <see cref="Gender"/> object.</returns>
        public Task<Gender> UpdateAsync(Gender value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return DataInvoker<Gender>.Default.InvokeAsync(this, async () =>
            {
                Gender __result = null;
                var __dataArgs = DbMapper.Default.CreateArgs("[Ref].[spGenderUpdate]");
                if (_updateOnBeforeAsync != null) await _updateOnBeforeAsync(value, __dataArgs);
                __result = Database.Default.Update(__dataArgs, value);
                if (_updateOnAfterAsync != null) await _updateOnAfterAsync(__result);
                return __result;
            }, new BusinessInvokerArgs { ExceptionHandler = _updateOnException });
        }

        /// <summary>
        /// Provides the <see cref="Gender"/> entity and database property mapping.
        /// </summary>
        public partial class DbMapper : DatabaseMapper<Gender, DbMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbMapper"/> class.
            /// </summary>
            public DbMapper()
            {
                Property(s => s.Id, "GenderId").SetUniqueKey(true);
                Property(s => s.Code);
                Property(s => s.Text);
                Property(s => s.IsActive);
                Property(s => s.SortOrder);
                AddStandardProperties();
                DbMapperCtor();
            }
            
            /// <summary>
            /// Enables the <see cref="DbMapper"/> constructor to be extended.
            /// </summary>
            partial void DbMapperCtor();
        }
    }
}
