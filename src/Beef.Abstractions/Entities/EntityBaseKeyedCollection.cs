// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Beef.Entities
{
    /// <summary>
    /// Represents an <see cref="EntityBase"/> keyed collection class.
    /// </summary>
    /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntity">The <see cref="EntityBase"/> <see cref="Type"/>.</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class EntityBaseKeyedCollection<TKey, TEntity> : KeyedCollection<TKey, TEntity>, IEntityBaseCollection, INotifyCollectionChanged, IEquatable<EntityBaseKeyedCollection<TKey, TEntity>> where TEntity : EntityBase
    {
        private object? _editCopy;
        private readonly Lazy<bool> _hasUniqueKey = new(() => typeof(IUniqueKey).IsAssignableFrom(typeof(TEntity)));
        private readonly Func<TEntity, TKey>? _getKeyForItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBaseKeyedCollection{TKey, TEntity}" /> class.
        /// </summary>
        protected EntityBaseKeyedCollection(Func<TEntity, TKey>? getKeyForItem = null)
            : base()
        {
            _getKeyForItem = getKeyForItem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBaseKeyedCollection{TKey, TEntity}" /> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        protected EntityBaseKeyedCollection(IEnumerable<TEntity> collection)
        {
            if (collection == null)
                return; 

            foreach (TEntity item in collection)
                Add(item);
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="EntityBaseKeyedCollection{TKey, TEntity}"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="EntityBaseKeyedCollection{TKey, TEntity}"/>.</returns>
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
                Add(item);
        }

        /// <summary>
        /// Adds the items of the specified collection to the <see cref="EntityBaseKeyedCollection{TKey, TEntity}"/>.
        /// </summary>
        /// <param name="collection">The collection containing the items to add.</param>
        public void AddRange(IEnumerable<TEntity> collection)
        {
            if (collection == null)
                return;

            foreach (TEntity item in collection)
                Add(item);
        }

        /// <summary>
        /// Gets the first item by the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="UniqueKey"/>.</param>
        /// <returns>The first item where found; otherwise, <c>null</c>.</returns>
        object IEntityBaseCollection.GetByUniqueKey(UniqueKey key)
        {
            return GetByUniqueKey(key);
        }

        /// <summary>
        /// Gets the first item by the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="UniqueKey"/>.</param>
        /// <returns>The first item where found; otherwise, <c>null</c>. Where the underlying entity item does not implement <see cref="IUniqueKey"/> this will always return <c>null</c>.</returns>
        public TEntity GetByUniqueKey(UniqueKey key)
        {
            if (!_hasUniqueKey.Value)
                return default!;

            return Items.Where(x => x is IUniqueKey uk && key.Equals(uk)).FirstOrDefault();
        }

        /// <summary>
        /// Performs a clean-up of the <see cref="EntityBaseCollection{TEntity}"/> resetting item values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public void CleanUp()
        {
            foreach (TEntity item in this)
                item.CleanUp();
        }

        /// <summary>
        /// Collections do not support an initial state; will always be <c>false</c>.
        /// </summary>
        bool ICleanUp.IsInitial => false;

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <param name="entity">The entity from which to extract the key.</param>
        /// <returns>The key for the specified element.</returns>
        protected override TKey GetKeyForItem(TEntity entity)
        {
            if (_getKeyForItem == null)
                throw new InvalidOperationException("The getKeyForItem function was not specified within the constructor and as such this method must be overridden to perform.");

            return _getKeyForItem(entity);
        }

        /// <summary>
        /// Replaces the entity at the specified index with the specified entity.
        /// </summary>
        /// <param name="index">The zero-based index of the entity to be replaced.</param>
        /// <param name="entity">The new entity.</param>
        protected override void SetItem(int index, TEntity entity)
        {
            base.SetItem(index, entity);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, entity, index));
        }

        /// <summary>
        /// Inserts the entity at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the entity should be inserted.</param>
        /// <param name="entity">The entity to insert.</param>
        protected override void InsertItem(int index, TEntity entity)
        {
            base.InsertItem(index, entity);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entity, index));
        }

        /// <summary>
        /// Removes the entity at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the entity to be removed.</param>
        protected override void RemoveItem(int index)
        {
            TEntity entity = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entity, index));
        }

        /// <summary>
        /// Removes all the entities from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/>.</param>
        /// <remarks>Sets <see cref="IsChanged"/> to <c>true</c>.</remarks>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);

            if (e?.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var ei = (EntityBase)item;
                    ei.PropertyChanged -= Item_PropertyChanged;
                }
            }

            if (e?.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var ei = (EntityBase)item;
                    ei.PropertyChanged += Item_PropertyChanged;

                    if (IsChangeTracking && !ei.IsChangeTracking)
                        ei.TrackChanges();
                }
            }

            IsChanged = true;
        }

        /// <summary>
        /// Updates IsChanged where required.
        /// </summary>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsChanged = true;
            //TODO: PropertyChanged event needs to be raised.
        }

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

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

            foreach (var item in this)
            {
                item.AcceptChanges();
            }

            IsChanged = false;
            IsChangeTracking = false;
        }

        /// <summary>
        /// Determines that until <see cref="AcceptChanges"/> is invoked property changes are to be logged (see <see cref="EntityBase.ChangeTracking"/>) for each item.
        /// </summary>
        public virtual void TrackChanges()
        {
            foreach (var item in this)
            {
                if (!item.IsChangeTracking)
                    item.TrackChanges();
            }

            IsChangeTracking = true;
        }

        /// <summary>
        /// Lists the properties (names of) that have been changed (note that this property is not JSON serialized). <i>Note:</i> always returns <c>null</c> as properties are not tracked for a collection.
        /// </summary>
        public StringCollection? ChangeTracking => null;

        /// <summary>
        /// Indicates whether entity is currently <see cref="ChangeTracking"/>; <see cref="TrackChanges"/> and <see cref="IChangeTracking.AcceptChanges"/>.
        /// </summary>
        public bool IsChangeTracking { get; private set; }

        /// <summary>
        /// Indicates whether the entity has changed.
        /// </summary>
        public bool IsChanged { get; private set; }

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj is not EntityBaseKeyedCollection<TKey, TEntity> val)
                return false;

            return Equals(val);
        }

        /// <summary>
        /// Determines whether the specified <see cref="EntityBaseKeyedCollection{TKey, TEntity}"/> is equal to the current <see cref="EntityBaseKeyedCollection{TKey, TEntity}"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(EntityBaseKeyedCollection<TKey, TEntity> value)
        {
            if (((object)value!) == ((object)this))
                return true;
            else if (((object)value!) == null || Count != value.Count)
                return false;

            for (int i = 0; i < Count; i++)
            {
                if (!this[i].Equals(value[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for the <see cref="EntityBaseKeyedCollection{TKey, TEntity}"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="EntityBaseKeyedCollection{TKey, TEntity}"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var item in this)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }
    }
}