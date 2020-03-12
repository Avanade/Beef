// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;

namespace Beef.Data.Database
{
    /// <summary>
    /// Enables the <b>Database</b> arguments capabilities.
    /// </summary>
    public interface IDatabaseArgs
    {
        /// <summary>
        /// Gets the stored procedure name.
        /// </summary>
        string StoredProcedure { get; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult? Paging { get; }

        /// <summary>
        /// Gets the <see cref="IDatabaseMapper"/>.
        /// </summary>
        IDatabaseMapper Mapper { get; }

        /// <summary>
        /// Indicates whether the data should be refreshed (reselected where applicable) after a <b>save</b> operation (defaults to <c>true</c>).
        /// </summary>
        bool Refresh { get; set; }
    }

    /// <summary>
    /// Provides the base <b>Database</b> arguments capabilities.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
    public class DatabaseArgs<T> : IDatabaseArgs where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseArgs{T}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <param name="storedProcedure">The stored procedure name.</param>
        public DatabaseArgs(IDatabaseMapper<T> mapper, string storedProcedure)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            StoredProcedure = !string.IsNullOrEmpty(storedProcedure) ? storedProcedure : throw new ArgumentNullException(nameof(storedProcedure));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseArgs{T}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <param name="storedProcedure">The stored procedure name.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public DatabaseArgs(IDatabaseMapper<T> mapper, string storedProcedure, PagingArgs paging)
            : this(mapper, storedProcedure, new PagingResult(paging ?? throw new ArgumentNullException(nameof(paging))))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseArgs{T}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <param name="storedProcedure">The stored procedure name.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        public DatabaseArgs(IDatabaseMapper<T> mapper, string storedProcedure, PagingResult paging) : this(mapper, storedProcedure)
        {
            Paging = paging ?? throw new ArgumentNullException(nameof(paging));
        }

        /// <summary>
        /// Gets the <see cref="IDatabaseMapper"/>.
        /// </summary>
        IDatabaseMapper IDatabaseArgs.Mapper => Mapper;

        /// <summary>
        /// Gets the <see cref="DatabaseMapper{TSrce, TMapper}"/>.
        /// </summary>
        public IDatabaseMapper<T> Mapper { get; private set; }

        /// <summary>
        /// Gets the stored procedure name.
        /// </summary>
        public string StoredProcedure { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult? Paging { get; private set; }

        /// <summary>
        /// Indicates whether the data should be refreshed (reselected where applicable) after a <b>save</b> operation (defaults to <c>true</c>).
        /// </summary>
        public bool Refresh { get; set; } = true;
    }
}