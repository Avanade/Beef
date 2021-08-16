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
        PagingResult? Paging { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="ICollection"/>.
        /// </summary>
        ICollection? Collection { get; }
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
        new ICollection<TEntity>? Collection { get; }
    }

    /// <summary>
    /// Provides the typed <typeparamref name="TEntity"/> collection with a <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="TColl">The result collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntity">The underlying entity <see cref="Type"/>.</typeparam>
    public interface IEntityCollectionResult<TColl, TEntity> : IEntityCollectionResult<TEntity>
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        TColl Result { get; set; }
    }

    /// <summary>
    /// Represents an <see cref="EntityBaseCollection{TEntity}"/> class with a <see cref="PagingResult"/> and corresponding <see cref="Result"/> collection.
    /// </summary>
    /// <typeparam name="TColl">The result collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntity">The underlying entity <see cref="Type"/>.</typeparam>

    [System.Diagnostics.DebuggerStepThrough]
    public abstract class EntityCollectionResult<TColl, TEntity> : EntityBase, IEntityCollectionResult<TColl, TEntity>, IPagingResult
        where TColl : EntityBaseCollection<TEntity>, new()
        where TEntity : EntityBase
    {
        private PagingResult? _paging;
        private TColl? _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollectionResult{TColl, TEntity}"/> class.
        /// </summary>
        /// <param name="paging">Defaults the <see cref="Paging"/> to the requesting <see cref="PagingArgs"/>.</param>
        protected EntityCollectionResult(PagingArgs? paging = null)
        {
            if (paging != null)
                _paging = new PagingResult(paging);
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public TColl Result
        {
            get { return _result ??= new TColl(); }
            set { SetValue(ref _result, value ?? throw new ArgumentNullException(nameof(value)), false, false, nameof(Result)); }
        }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/>.
        /// </summary>
        /// <remarks>Where this value is <c>null</c> it indicates that the paging was unable to be determined.</remarks>
        public PagingResult? Paging
        {
            get { return _paging; }
            set { SetValue(ref _paging, value, false, false, nameof(Paging)); }
        }

        /// <summary>
        /// Gets or sets the item <see cref="Type"/>.
        /// </summary>
        public Type ItemType { get; } = typeof(TEntity);

        /// <summary>
        /// Gets the underlying <see cref="ICollection"/>.
        /// </summary>
        ICollection? IEntityCollectionResult.Collection => _result;

        /// <summary>
        /// Gets the underlying <see cref="ICollection{TEntity}"/>.
        /// </summary>
        ICollection<TEntity>? IEntityCollectionResult<TEntity>.Collection => _result;

        /// <summary>
        /// Performs a copy from another <see cref="EntityCollectionResult{TColl, TEntity}"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="EntityCollectionResult{TColl, Entity}"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<EntityCollectionResult<TColl, TEntity>>(from);
            CopyFrom(fval);
            Result = (TColl)fval.Result.Clone();
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
        /// Indicates whether the collection is <c>null</c> (empty collections are not considered initial).
        /// </summary>
        public override bool IsInitial => _result == null; 
    }

    /// <summary>
    /// Represents a basic <see cref="IEntityCollectionResult{TEntity}"/> class (not <see cref="EntityBaseCollection{TEntity}"/>) with a <see cref="PagingResult"/> and corresponding <see cref="Result"/> collection.
    /// </summary>
    /// <typeparam name="TColl">The result collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntity">The underlying entity <see cref="Type"/>.</typeparam>
    public abstract class CollectionResult<TColl, TEntity> : IEntityCollectionResult<TColl, TEntity>, IPagingResult
        where TColl : List<TEntity>, new()
        where TEntity : class
    {
        private PagingResult? _paging;
        private TColl? _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionResult{TColl, TEntity}"/> class.
        /// </summary>
        /// <param name="paging">Defaults the <see cref="Paging"/> to the requesting <see cref="PagingArgs"/>.</param>
        protected CollectionResult(PagingArgs? paging = null)
        {
            if (paging != null)
                _paging = new PagingResult(paging);
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public TColl Result
        {
            get { return _result ??= new TColl(); }
            set { _result = value ?? throw new ArgumentNullException(nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/>.
        /// </summary>
        /// <remarks>Where this value is <c>null</c> it indicates that the paging was unable to be determined.</remarks>
        public PagingResult? Paging
        {
            get { return _paging; }
            set { _paging = value; }
        }

        /// <summary>
        /// Gets or sets the item <see cref="Type"/>.
        /// </summary>
        public Type ItemType { get; } = typeof(TEntity);

        /// <summary>
        /// Gets the underlying <see cref="ICollection"/>.
        /// </summary>
        ICollection? IEntityCollectionResult.Collection => _result;

        /// <summary>
        /// Gets the underlying <see cref="ICollection{TEntity}"/>.
        /// </summary>
        ICollection<TEntity>? IEntityCollectionResult<TEntity>.Collection => _result;
    }
}