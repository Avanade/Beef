// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Beef.Entities
{
    /// <summary>
    /// Represents the core <see cref="EntityBase"/> collection capabilities.
    /// </summary>
    public interface IEntityBaseCollection : IEditableObject, IChangeTracking, ICloneable, ICleanUp, ICollection
    {
        /// <summary>
        /// Adds the items of the specified collection to the end of the <see cref="EntityBaseCollection{TEntity}"/>.
        /// </summary>
        /// <param name="collection">The collection containing the items to add.</param>
        void AddRange(IEnumerable collection);

        /// <summary>
        /// Gets the item by the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="UniqueKey"/>.</param>
        /// <returns>The item where found; otherwise, <c>null</c>.</returns>
        object GetByUniqueKey(UniqueKey key);
    }

    /// <summary>
    /// Represents an <see cref="EntityBase"/> collection class.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="EntityBase"/> <see cref="System.Type"/>.</typeparam>
    public abstract class EntityBaseCollection<TEntity> : ObservableCollection<TEntity>, IEntityBaseCollection where TEntity : EntityBase
    {
        private object _editCopy;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBaseCollection{TEntity}" /> class.
        /// </summary>
        public EntityBaseCollection()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBaseCollection{TEntity}" /> class.
        /// </summary>
        /// <param name="collection">The entities.</param>
        public EntityBaseCollection(IEnumerable<TEntity> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="EntityBaseCollection{TEntity}"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="EntityBaseCollection{TEntity}"/>.</returns>
        public abstract object Clone();

        /// <summary>
        /// Adds the items of the specified collection to the end of the <see cref="EntityBaseCollection{TEntity}"/>.
        /// </summary>
        /// <param name="collection">The collection containing the items to add.</param>
        void IEntityBaseCollection.AddRange(IEnumerable collection)
        {
            if (collection == null)
                return;

            foreach (TEntity item in collection)
                this.Add(item);
        }

        /// <summary>
        /// Adds the items of the specified collection to the end of the <see cref="EntityBaseCollection{TEntity}"/>.
        /// </summary>
        /// <param name="collection">The collection containing the items to add.</param>
        public void AddRange(IEnumerable<TEntity> collection)
        {
            if (collection == null)
                return;

            foreach (TEntity item in collection)
                this.Add(item);
        }

        /// <summary>
        /// Gets the item by the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="UniqueKey"/>.</param>
        /// <returns>The item where found; otherwise, <c>null</c>.</returns>
        object IEntityBaseCollection.GetByUniqueKey(UniqueKey key)
        {
            return GetByUniqueKey(key);
        }

        /// <summary>
        /// Gets the first item by the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="UniqueKey"/>.</param>
        /// <returns>The first item where found; otherwise, <c>null</c>.</returns>
        public TEntity GetByUniqueKey(UniqueKey key)
        {
            Check.NotNull(key, nameof(key));
            return Items.Where(x => x.HasUniqueKey && key.Equals(x.UniqueKey)).FirstOrDefault();
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{TEntity}"/> wrapper around the <see cref="EntityBaseCollection{TEntity}"/>.
        /// </summary>
        /// <remarks>This is provided to enable the likes of <b>LINQ</b> based queries over the collection.</remarks>
        public new IEnumerable<TEntity> Items
        {
            get { return base.Items; }
        }

        /// <summary>
        /// Performs a clean-up of the <see cref="EntityBaseCollection{TEntity}"/> resetting item values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public void CleanUp()
        {
            foreach (TEntity item in this)
                item?.CleanUp();
        }

        /// <summary>
        /// Collections do not support an initial state; will always be <c>false</c>.
        /// </summary>
        bool ICleanUp.IsInitial => false;

        /// <summary>
        /// Overrides the <see cref="ObservableCollection{TEntity}.OnCollectionChanged"/> method.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/>.</param>
        /// <remarks>Sets <see cref="IsChanged"/> to <c>true</c>.</remarks>
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            IsChanged = true;
        }

        /// <summary>
        /// Begins an edit on an collection.
        /// </summary>
        public void BeginEdit()
        {
            _editCopy = this.Clone();
        }

        /// <summary>
        /// Discards the collection changes since the last <see cref="BeginEdit"/>.
        /// </summary>
        /// <remarks>Resets the entity state to unchanged (see <see cref="AcceptChanges"/>) after the changes have been discarded.</remarks>
        public void CancelEdit()
        {
            if (_editCopy != null)
            {
                this.Clear();
                this.AddRange((IEnumerable<TEntity>)_editCopy);
            }

            AcceptChanges();
        }

        /// <summary>
        /// Ends and commits the collection changes since the last <see cref="BeginEdit"/>.
        /// </summary>
        public void EndEdit()
        {
            AcceptChanges();
        }

        /// <summary>
        /// Resets the entity state to unchanged by accepting the changes.
        /// </summary>
        public virtual void AcceptChanges()
        {
            _editCopy = null;
            IsChanged = false;
        }

        /// <summary>
        /// Indicates whether the entity has changed.
        /// </summary>
        public bool IsChanged { get; private set; }
    }
}
