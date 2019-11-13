// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections;
using System.Collections.Generic;

namespace Beef.Entities
{
    /// <summary>
    /// Provides the <see cref="Paging"/> and <see cref="Collection"/>.
    /// </summary>
    public interface IEntityCollectionResult
    {
        /// <summary>
        /// Gets the underlying item <see cref="Type"/>.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult Paging { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="ICollection"/>.
        /// </summary>
        ICollection Collection { get; }
    }

    /// <summary>
    /// Provides the typed <typeparamref name="TEntity"/> <see cref="Collection"/>.
    /// </summary>
    /// <typeparam name="TEntity">The The underlying entity <see cref="Type"/>.</typeparam>
    public interface IEntityCollectionResult<TEntity> : IEntityCollectionResult
    {
        /// <summary>
        /// Gets the underlying <see cref="ICollection{TEntity}"/>.
        /// </summary>
        new ICollection<TEntity> Collection { get; }
    }

    /// <summary>
    /// Represents an <see cref="EntityBaseCollection{TEntity}"/> class with a <see cref="PagingResult"/> and corresponding <see cref="Result"/> collection.
    /// </summary>
    /// <typeparam name="TColl">The result collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntity">The underlying entity <see cref="Type"/>.</typeparam>
    public abstract class EntityCollectionResult<TColl, TEntity> : EntityBase, IEntityCollectionResult, IEntityCollectionResult<TEntity>, IPagingResult
        where TColl : EntityBaseCollection<TEntity>, new()
        where TEntity : EntityBase
    {
        private PagingResult _paging;
        private TColl _result = new TColl();

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollectionResult{TColl, TEntity}"/> class.
        /// </summary>
        /// <param name="paging">Defaults the <see cref="Paging"/> to the requesting <see cref="PagingArgs"/>.</param>
        protected EntityCollectionResult(PagingArgs paging = null)
        {
            if (paging != null)
                _paging = new PagingResult(paging);
        }

#pragma warning disable CA2227 // Collection properties should be read only; special purpose - by-design.
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public TColl Result
#pragma warning restore CA2227
        {
            get { return _result; }
            set { SetValue<TColl>(ref _result, value, false, false, ResultProperty); }
        }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/>.
        /// </summary>
        /// <remarks>Where this value is <c>null</c> it indicates that the paging was unable to be determined.</remarks>
        public PagingResult Paging
        {
            get { return _paging; }
            set { SetValue<PagingResult>(ref _paging, value, false, false, PagingProperty); }
        }

        /// <summary>
        /// Gets or sets the item <see cref="Type"/>.
        /// </summary>
        public Type ItemType { get; } = typeof(TEntity);

        /// <summary>
        /// Gets the underlying <see cref="ICollection"/>.
        /// </summary>
        ICollection IEntityCollectionResult.Collection
        {
#pragma warning disable CA1033 // Interface methods should be callable by child types; special purpose - by-design.
            get { return _result; }
#pragma warning restore CA1033
        }

        /// <summary>
        /// Gets the underlying <see cref="ICollection{TEntity}"/>.
        /// </summary>
        ICollection<TEntity> IEntityCollectionResult<TEntity>.Collection
        {
#pragma warning disable CA1033 // Interface methods should be callable by child types; special purpose - by-design.
            get { return _result; }
#pragma warning restore CA1033
        }

        /// <summary>
        /// Performs a copy from another <see cref="EntityCollectionResult{TColl, TEntity}"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="EntityCollectionResult{TColl, Entity}"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<EntityCollectionResult<TColl, TEntity>>(from);
            base.CopyFrom(fval);
            Result = (fval.Result == null) ? null : (TColl)fval.Result.Clone();
            Paging = (fval.Paging == null) ? null : new PagingResult(fval.Paging);
        }

        /// <summary>
        /// Performs a clean-up of the <see cref="EntityBaseCollection{TEntity}"/> resetting item values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            if (_result != null)
                _result.CleanUp();
        }

        /// <summary>
        /// Indicates whether the collection is empty; i.e. has no items in the collection.
        /// </summary>
        public override bool IsInitial
        {
            get { return _result == null || _result.Count == 0; }
        }

        /// <summary>
        /// Represents the <see cref="Result"/> property name.
        /// </summary>
        public const string ResultProperty = "Result";

        /// <summary>
        /// Represents the <see cref="Paging"/> property name.
        /// </summary>
        public const string PagingProperty = "Paging";
    }
}
