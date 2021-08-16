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
#pragma warning disable CA1010 // Collections should implement generic interface; not-required by-design.
    /// <summary>
    /// Represents the core <see cref="EntityBase"/> collection capabilities.
    /// </summary>
    public interface IEntityBaseCollection : IEditableObject, IChangeTracking, IChangeTrackingLogging, ICloneable, ICleanUp, ICollection
#pragma warning restore CA1010 // Collections should implement generic interface
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
    /// Provides data for the <see cref="INotifyPropertyChanged.PropertyChanged"/> when an item within an <see cref="IEntityBaseCollection"/> is changed.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The item that had the property change.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        public ItemPropertyChangedEventArgs(object item, string propertyName) : base(propertyName) => Item = item;

        /// <summary>
        /// Gets the item that had the property change.
        /// </summary>
        public object Item { get; }
    }

    /// <summary>
    /// Represents an <see cref="EntityBase"/> collection class.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="EntityBase"/> <see cref="System.Type"/>.</typeparam>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class EntityBaseCollection<TEntity> : ObservableCollection<TEntity>, IEntityBaseCollection, IEquatable<EntityBaseCollection<TEntity>> where TEntity : EntityBase
    {
        private object? _editCopy;
        private readonly Lazy<bool> _hasUniqueKey = new(() => typeof(IUniqueKey).IsAssignableFrom(typeof(TEntity)));

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBaseCollection{TEntity}" /> class.
        /// </summary>
        protected EntityBaseCollection() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBaseCollection{TEntity}" /> class.
        /// </summary>
        /// <param name="collection">The entities.</param>
        protected EntityBaseCollection(IEnumerable<TEntity> collection) : base(collection) { }

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
                Add(item);
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
                Add(item);
        }

        /// <summary>
        /// Gets the item by the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="UniqueKey"/>.</param>
        /// <returns>The item where found; otherwise, <c>null</c>.</returns>
        object IEntityBaseCollection.GetByUniqueKey(UniqueKey key) => GetByUniqueKey(key);

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
        /// Gets a <see cref="IEnumerable{TEntity}"/> wrapper around the <see cref="EntityBaseCollection{TEntity}"/>.
        /// </summary>
        /// <remarks>This is provided to enable the likes of <b>LINQ</b> based queries over the collection.</remarks>
        public new IEnumerable<TEntity> Items => base.Items;

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
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

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
            OnPropertyChanged(new ItemPropertyChangedEventArgs(sender, e.PropertyName));
        }

        /// <summary>
        /// Begins an edit on an collection.
        /// </summary>
        public void BeginEdit() => _editCopy = Clone();

        /// <summary>
        /// Discards the collection changes since the last <see cref="BeginEdit"/>.
        /// </summary>
        /// <remarks>Resets the entity state to unchanged (see <see cref="AcceptChanges"/>) after the changes have been discarded.</remarks>
        public void CancelEdit()
        {
            if (_editCopy != null)
            {
                Clear();
                AddRange((IEnumerable<TEntity>)_editCopy);
            }

            AcceptChanges();
        }

        /// <summary>
        /// Ends and commits the collection changes since the last <see cref="BeginEdit"/>.
        /// </summary>
        public void EndEdit() => AcceptChanges();

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
        public StringCollection? ChangeTracking => null!;

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
            if (obj == null || obj is not EntityBaseCollection<TEntity> val)
                return false;

            return Equals(val);
        }

        /// <summary>
        /// Determines whether the specified <see cref="EntityBaseCollection{TEntity}"/> is equal to the current <see cref="EntityBaseCollection{TEntity}"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(EntityBaseCollection<TEntity> value)
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
        /// Returns a hash code for the <see cref="EntityBaseCollection{TEntity}"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="EntityBaseCollection{TEntity}"/>.</returns>
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