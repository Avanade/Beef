// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using Microsoft.EntityFrameworkCore;
using System;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Enables the <b>Entity Framework</b> arguments capabilities.
    /// </summary>
    public interface IEfDbArgs
    {
        /// <summary>
        /// Gets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult Paging { get; }

        /// <summary>
        /// Gets the <see cref="IEntityMapper"/>.
        /// </summary>
        IEntityMapper Mapper { get; }

        /// <summary>
        /// Indicates that the underlying <see cref="DbContext"/> <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()"/> is to be performed automatically.
        /// </summary>
        bool SaveChanges { get; }

        /// <summary>
        /// Indicates whether the data should be refreshed (reselected where applicable) after a <b>save</b> operation (defaults to <c>true</c>).
        /// </summary>
        bool Refresh { get; }

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
        /// </summary>
        DbContext DbContext { get; }
    }

    /// <summary>
    /// Provides the base <b>Entity Framework</b> arguments capabilities.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The entity framework model.</typeparam>
    public class EfDbArgs<T, TModel> : IEfDbArgs where T : class, new() where TModel : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbArgs{T, TModel}"/> class with a <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="dbContext">The <see cref="Microsoft.EntityFrameworkCore.DbContext"/> where overridding automatic default.</param>
        public EfDbArgs(IEntityMapper<T, TModel> mapper, DbContext dbContext = null)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            DbContext = dbContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbArgs{T, TModel}"/> class with a <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="dbContext">The <see cref="Microsoft.EntityFrameworkCore.DbContext"/> where overridding automatic default.</param>
        public EfDbArgs(IEntityMapper<T, TModel> mapper, PagingArgs paging, DbContext dbContext = null)
            : this(mapper, new PagingResult(paging ?? throw new ArgumentNullException(nameof(paging))), dbContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbArgs{T, TModel}"/> class with a <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="dbContext">The <see cref="Microsoft.EntityFrameworkCore.DbContext"/> where overridding automatic default.</param>
        public EfDbArgs(IEntityMapper<T, TModel> mapper, PagingResult paging, DbContext dbContext = null) : this(mapper, dbContext)
        {
            Paging = paging ?? throw new ArgumentNullException(nameof(paging));
        }

        /// <summary>
        /// Gets the <see cref="IEntityMapper"/>.
        /// </summary>
        IEntityMapper IEfDbArgs.Mapper => Mapper;

        /// <summary>
        /// Gets the <see cref="IEntityMapper{T, TModel}"/>.
        /// </summary>
        public IEntityMapper<T, TModel> Mapper { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult Paging { get; private set; }

        /// <summary>
        /// Indicates that the underlying <see cref="DbContext"/> <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()"/> is to be performed automatically (defauls to <c>true</c>);
        /// </summary>
        public bool SaveChanges { get; set; } = true;

        /// <summary>
        /// Indicates whether the data should be refreshed (reselected where applicable) after a <b>save</b> operation (defaults to <c>true</c>);
        /// is dependent on <see cref="SaveChanges"/> being performed.
        /// </summary>
        public bool Refresh { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
        /// </summary>
        public DbContext DbContext { get; set; }
    }
}
