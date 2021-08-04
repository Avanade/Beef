// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Beef.RefData
{
    /// <summary>
    /// Provides the base capabilities for a special purpose <see cref="ReferenceDataBase"/> collection specifically for managing a referenced list of
    /// <b>Serialization Identifiers</b> (SIDs).
    /// </summary>
    public abstract class ReferenceDataSidListBase
    {
        /// <summary>
        /// Indicates whether the collection contains invalid items (i.e. not <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.IsValid"/>).
        /// </summary>
        /// <returns><c>true</c> indicates that invalid items exist; otherwise, <c>false</c>.</returns>
        public abstract bool ContainsInvalidItems();

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets the underlying <see cref="ReferenceDataBase"/> list.
        /// </summary>
        /// <returns>The underlying <see cref="ReferenceDataBase"/> list.</returns>
        public abstract List<ReferenceDataBase> ToRefDataList();
    }

    /// <summary>
    /// Represents a special purpose <see cref="ReferenceDataBase"/> collection specifically for managing a referenced list of <b>Serialization Identifiers</b> (SIDs) versus
    /// storing instances of the <see cref="ReferenceDataBase"/> items directly. This is a required capability to enable the serialization of a list of reference data items
    /// within an entity. 
    /// </summary>
    /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> <see cref="Type"/> for the collection.</typeparam>
    /// <typeparam name="TSid">The <b>Serialization Identifier</b> (SID) <see cref="Type"/>; supports only: <see cref="String"/>, <see cref="Int32"/> and <see cref="Guid"/>.</typeparam>
    /// <remarks>This collection wraps an externally referenced list of SIDs and maintains this directly. There is no <see cref="ReferenceDataBase"/> collection being managed
    /// within, it is just managing the casting between the <see cref="ReferenceDataBase"/> items and its SID giving the appearance that it is.</remarks>
    public class ReferenceDataSidList<TItem, TSid> : ReferenceDataSidListBase, IList<TItem>, IEnumerable<TItem>, INotifyCollectionChanged where TItem : ReferenceDataBase, new()
    {
        private static readonly SidType _sidType;
        private readonly List<TSid> _sids;

        /// <summary>
        /// Static initializer.
        /// </summary>
        static ReferenceDataSidList()
        {
            if (typeof(TSid) == typeof(string))
                _sidType = SidType.String;
            else if (typeof(TSid) == typeof(int))
                _sidType = SidType.Int32;
            else if (typeof(TSid) == typeof(long))
                _sidType = SidType.Int64;
            else if (typeof(TSid) == typeof(Guid))
                _sidType = SidType.Guid;
        }

        /// <summary>
        /// Represents the supported SID Types.
        /// </summary>
        private enum SidType
        {
            Unknown,
            String,
            Int32,
            Int64,
            Guid
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataSidList{TItem, TSid}"/> class.
        /// </summary>
        public ReferenceDataSidList()
        {
            _sids = new List<TSid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataSidList{TItem, TSid}"/> class with a reference to the underlying <b>Serialization Identifier</b> (SID) list.
        /// </summary>
        /// <param name="sids">A reference to the master <b>Serialization Identifier</b> (SID) list; it is this list that will be maintained by this collection.</param>
        public ReferenceDataSidList(ref List<TSid>? sids)
        {
            if (sids == null)
                sids = new List<TSid>();

            _sids = sids;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataSidList{TItem, TSid}"/> class with a list of items.
        /// </summary>
        /// <param name="items">The list of items.</param>
        public ReferenceDataSidList(IEnumerable<TItem>? items)
        {
            _sids = new List<TSid>();
            if (items == null)
                return;

            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataSidList{TItem, TSid}"/> class with a list of <b>Serialization Identifiers</b> (SIDs).
        /// </summary>
        /// <param name="sids">The list of <b>Serialization Identifiers</b> (SIDs).</param>
        public ReferenceDataSidList(params TSid[] sids)
        {
            _sids = new List<TSid>(sids);
        }

        /// <summary>
        /// Gets the underlying <b>Serialization Identifier</b> (SID) list.
        /// </summary>
        /// <returns>The underlying <b>Serialization Identifier</b> (SID) list.</returns>
        public List<TSid> ToSidList()
        {
            return _sids;
        }

        /// <summary>
        /// Creates an <see cref="Int32"/> list containing the <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Id"/> for each item (where <see cref="ReferenceDataBase.IsValid"/>).
        /// </summary>
        /// <returns>A <see cref="List{Int32}"/>.</returns>
        /// <exception cref="InvalidOperationException">Throw where the <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Id"/> is not an <see cref="Int32"/> type.</exception>
        public List<int> ToInt32IdList()
        {
            var ids = new List<int>();
            foreach (var item in this)
            {
                if (item == null)
                    throw new InvalidOperationException("An item should not have a value of null");

                if (ids.Count == 0 && (item.Id == null || item.Id.GetType() != typeof(int)))
                    throw new InvalidOperationException("The underlying item identifier Type is not an Int32; cannot convert to Int32 list.");

                if (item.IsValid)
                    ids.Add((int)item.Id!);
            }

            return ids;
        }

        /// <summary>
        /// Creates a <see cref="Guid"/> list containing the <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Id"/> for each item (where <see cref="ReferenceDataBase.IsValid"/>).
        /// </summary>
        /// <returns>A <see cref="List{Int32}"/>.</returns>
        /// <exception cref="InvalidOperationException">Throw where the <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Id"/> is not an <see cref="Guid"/> type.</exception>
        public List<Guid> ToGuidIdList()
        {
            var ids = new List<Guid>();
            foreach (var item in this)
            {
                if (item == null)
                    throw new InvalidOperationException("An item should not have a value of null");

                if (ids.Count == 0 && (item.Id == null || item.Id.GetType() != typeof(Guid)))
                    throw new InvalidOperationException("The underlying item identifier Type is not an Guid; cannot convert to Guid list.");

                if (item.IsValid)
                    ids.Add((Guid)item.Id!);
            }

            return ids;
        }

        /// <summary>
        /// Creates a <see cref="string"/> list containing the <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Code"/> for each item (where <see cref="ReferenceDataBase.IsValid"/>).
        /// </summary>
        /// <returns>A <see cref="List{Int32}"/>.</returns>
        public List<string?> ToCodeList()
        {
            var codes = new List<string?>();
            foreach (var item in this)
            {
                if (item == null)
                    throw new InvalidOperationException("An item should not have a value of null");

                if (item.IsValid)
                    codes.Add(item.Code);
            }

            return codes;
        }

        /// <summary>
        /// Gets the underlying <see cref="ReferenceDataBase"/> list.
        /// </summary>
        /// <returns>The underlying <see cref="ReferenceDataBase"/> list.</returns>
        public override List<ReferenceDataBase> ToRefDataList() => this.ToList<ReferenceDataBase>();

        /// <summary>
        /// Gets Reference Data <typeparamref name="TItem"/> by the SID.
        /// </summary>
        private static TItem GetItem(object sid)
        {
            if (sid == null)
                return default!;

            return _sidType switch
            {
                SidType.String => ReferenceDataBase.ConvertFromCode<TItem>((string)sid),
                SidType.Int32 => ReferenceDataBase.ConvertFromId<TItem>((int)sid),
                SidType.Int64 => ReferenceDataBase.ConvertFromId<TItem>((long)sid),
                SidType.Guid => ReferenceDataBase.ConvertFromId<TItem>((Guid)sid),
                _ => default!,
            };
        }

        /// <summary>
        /// Gets the SID for an Item.
        /// </summary>
        private static TSid GetSidForItem(TItem item)
        {
            if (item == null)
                return default!;

            return _sidType switch
            {
                SidType.String => (TSid)(object)item.Code!,
                SidType.Int32 or SidType.Int64 or SidType.Guid => (TSid)item.Id!,
                _ => default!,
            };
        }

        /// <summary>
        /// Adds a <see cref="ReferenceDataBase"/> item determining its underlying SID.
        /// </summary>
        private void AddItem(TItem item)
        {
            // Callers should manage the CollectionChanged event.
            TSid sid = GetSidForItem(item);
            _sids.Add(sid);
        }

        /// <summary>
        /// Indicates whether the collection contains invalid items (i.e. not <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.IsValid"/>).
        /// </summary>
        /// <returns><c>true</c> indicates that invalid items exist; otherwise, <c>false</c>.</returns>
        public override bool ContainsInvalidItems() => this.Any(x => x != null! && !x.IsValid);

        #region IEnumerable<>

        /// <summary>
        /// Supports an iteration over the collection.
        /// </summary>
        public IEnumerator<TItem> GetEnumerator()
        {
            foreach (TSid sid in _sids)
            {
                yield return GetItem(sid!);
            }
        }

        /// <summary>
        /// Supports an iteration over the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region ICollection<>

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public override int Count => _sids.Count;

        /// <summary>
        /// Indicates whether the collection is read only; it is not (returns <c>false</c>).
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item at the specified index.</returns>
        public TItem this[int index]
        {
            get { return GetItem(_sids[index]!); }

            set
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, GetItem(_sids[index]!));
                _sids[index] = GetSidForItem(value);
                OnCollectionChanged(e);
            }
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(TItem item)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _sids.Count);
            AddItem(item);
            OnCollectionChanged(e);
        }

        /// <summary>
        /// Adds a range of items to the collection.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<TItem> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            _sids.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determines whether the item exists within the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool Contains(TItem item) => _sids.Contains(GetSidForItem(item));

        /// <summary>
        /// Copies the entire contents of an item array into the target collection starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy.</param>
        /// <param name="arrayIndex">The starting index.</param>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            if (array == null || array.Length == 0)
                return;

            var sids = new TSid[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                sids[i] = GetSidForItem(array[i]);
            }

            _sids.CopyTo(sids, arrayIndex);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, array, arrayIndex));
        }

        /// <summary>
        /// Removes the item from the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if it exists and was removed; otherwise, <c>false</c>.</returns>
        public bool Remove(TItem item)
        {
            var index = this.IndexOf(item);
            if (index < 0)
                return false;

            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
            _sids.RemoveAt(index);
            OnCollectionChanged(e);
            return true;
        }

        /// <summary>
        /// Gets the index for the item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The index for the item.</returns>
        public int IndexOf(TItem item) => _sids.IndexOf(GetSidForItem(item));

        /// <summary>
        /// Inserts an item at the specified index.
        /// </summary>
        /// <param name="index">The index where the item is to be inserted.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, TItem item)
        {
            _sids.Insert(index, GetSidForItem(item));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveAt(int index)
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, GetItem(this[index]), index);
            _sids.RemoveAt(index);
            OnCollectionChanged(e);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        #endregion
    }
}