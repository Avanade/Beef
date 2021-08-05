// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Provides the <b>Entity Framework</b> arguments capabilities using an <i>AutoMapper</i> <see cref="IMapper"/>.
    /// </summary>
    public class EfDbArgs
    {
        /// <summary>
        /// Creates an <see cref="EfDbArgs"/> with an <i>AutoMapper</i> <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <returns>The <see cref="EfDbArgs"/>.</returns>
        public static EfDbArgs Create(IMapper mapper) => new EfDbArgs(mapper);

        /// <summary>
        /// Creates an <see cref="EfDbArgs"/> with an <i>AutoMapper</i> <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="EfDbArgs"/>.</returns>
        public static EfDbArgs Create(IMapper mapper, PagingArgs paging) => new EfDbArgs(mapper, paging);

        /// <summary>
        /// Creates an <see cref="EfDbArgs"/> with an <i>AutoMapper</i> <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <returns>The <see cref="EfDbArgs"/>.</returns>
        public static EfDbArgs Create(IMapper mapper, PagingResult paging) => new EfDbArgs(mapper, paging);

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbArgs"/> class with an <i>AutoMapper</i> <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        public EfDbArgs(IMapper mapper) => Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbArgs"/> class with an <i>AutoMapper</i> <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public EfDbArgs(IMapper mapper, PagingArgs paging) : this(mapper, new PagingResult(paging ?? throw new ArgumentNullException(nameof(paging)))) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbArgs"/> class with an <i>AutoMapper</i> <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        public EfDbArgs(IMapper mapper, PagingResult paging) : this(mapper) => Paging = paging ?? throw new ArgumentNullException(nameof(paging));

        /// <summary>
        /// Gets the <i>AutoMapper</i> <see cref="IMapper"/>.
        /// </summary>
        public IMapper Mapper { get; }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult? Paging { get; }

        /// <summary>
        /// Indicates that the underlying <see cref="DbContext"/> <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChanges()"/> is to be performed automatically (defauls to <c>true</c>);
        /// </summary>
        public bool SaveChanges { get; set; } = true;

        /// <summary>
        /// Indicates whether the data should be refreshed (reselected where applicable) after a <b>save</b> operation (defaults to <c>true</c>);
        /// is dependent on <see cref="SaveChanges"/> being performed.
        /// </summary>
        public bool Refresh { get; set; } = true;
    }
}