// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;

namespace Beef.RefData
{
    /// <summary>
    /// Provides the contextual validation <see cref="Date"/> for a <see cref="ReferenceDataBase"/> (the default is todays date).
    /// </summary>
    /// <remarks>The <see cref="Date"/> is a master setting for all <see cref="ReferenceDataBase"/> <see cref="Type">types</see>. An individual
    /// <see cref="Type"/> can be overridden where required, and all dates can be <see cref="Reset"/>.
    /// </remarks>
    public class ReferenceDataContext
    {
        private DateTime? _date;
        private readonly Dictionary<Type, DateTime?> _coll = new();

        /// <summary>
        /// Initializes a new instances of the <see cref="ReferenceDataContext"/>.
        /// </summary>
        internal ReferenceDataContext()
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.StartDate"/> and <see cref="ReferenceDataBase.EndDate"/> 
        /// contextual validation date (see <see cref="ReferenceDataBase.IsValid"/>).
        /// </summary>
        public DateTime? Date
        {
            get
            {
                if (_date == null)
                    return Cleaner.Clean(ExecutionContext.SystemTime.UtcNow, DateTimeTransform.DateOnly);

                return _date;
            }

            set { _date = Cleaner.Clean(value, DateTimeTransform.DateOnly); }
        }

        /// <summary>
        /// Gets or sets a contextual validation date for a specific <see cref="ReferenceDataBase"/> <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <returns>The contextual validation date.</returns>
        public DateTime? this[Type type]
        {
            get
            {
                Check.NotNull(type, nameof(type));

                if (_coll.ContainsKey(type))
                    return _coll[type];

                return Date;
            }

            set
            {
                Check.NotNull(type, nameof(type));

                if (_coll.ContainsKey(type))
                {
                    if (value == null)
                        _coll.Remove(type);
                    else
                        _coll[type] = Cleaner.Clean(value, DateTimeTransform.DateOnly);
                }
                else
                {
                    if (value != null)
                        _coll.Add(type, Cleaner.Clean(value, DateTimeTransform.DateOnly));
                }
            }
        }

        /// <summary>
        /// Resets all dates.
        /// </summary>
        public void Reset()
        {
            _date = null;
            _coll.Clear();
        }
    }
}
