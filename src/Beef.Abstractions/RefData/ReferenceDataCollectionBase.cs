// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Beef.RefData
{
#pragma warning disable CA1710 // Identifiers should have correct suffix; by-design, as is a CollectionBase.
    /// <summary>
    /// Represents a <see cref="ReferenceDataBase"/> collection where the primary key is the <see cref="ReferenceDataBase.Id"/> and <see cref="ReferenceDataBase.Code"/>.
    /// </summary>
    /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> <see cref="Type"/> for the collection.</typeparam>
    /// <remarks>The <see cref="ReferenceDataBase.Id"/> and <see cref="ReferenceDataBase.Code"/> must be unique in their own right within the collection;
    /// i.e. they are not combined to form a composite key.
    /// <para>This collection only supports the <see cref="Add"/> of items; no other updates are supported.</para></remarks>
    public abstract class ReferenceDataCollectionBase<TItem> : IEnumerable<TItem>, ICollection<TItem>, IReferenceDataCollection, INotifyCollectionChanged where TItem : ReferenceDataBase, new()
#pragma warning restore CA1710
    {
        private readonly object _lock = new();
        private readonly ReferenceDataIdCollection _rdcId;
        private readonly ReferenceDataCodeCollection _rdcCode;
        private readonly Dictionary<MappingsKey, string> _mappingsDict;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataCollectionBase{TItem}"/> class with a default <see cref="ReferenceDataSortOrder.SortOrder"/>.
        /// </summary>
        /// <param name="sortOrder">The <see cref="ReferenceDataSortOrder"/>.</param>
        /// <param name="isCodeCaseSensitive">Indicates whether the <see cref="ReferenceDataBase.Code"/> is case sensitive.</param>
        protected ReferenceDataCollectionBase(ReferenceDataSortOrder sortOrder = ReferenceDataSortOrder.SortOrder, bool isCodeCaseSensitive = false)
        {
            SortOrder = sortOrder;
            IsCodeCaseSensitive = isCodeCaseSensitive;

            _rdcId = new ReferenceDataIdCollection();
            _rdcCode = new ReferenceDataCodeCollection(this);
            _mappingsDict = new Dictionary<MappingsKey, string>();
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataSortOrder"/> (used by <see cref="GetList"/>).
        /// </summary>
        public ReferenceDataSortOrder SortOrder { get; }

        /// <summary>
        /// Indicates whether the <see cref="ReferenceDataBase.Code"/> is case sensitive (defaults to <c>false</c>).
        /// </summary>
        /// <remarks>Where <see cref="IsCodeCaseSensitive"/> is <c>false</c> then all <b>Code</b> related access and validation is performed using <see cref="String.ToUpperInvariant"/>.</remarks>
        public bool IsCodeCaseSensitive { get; } = false;

        /// <summary>
        /// Converts the <paramref name="code"/> value based on <see cref="IsCodeCaseSensitive"/>.
        /// </summary>
        /// <param name="code">The code value.</param>
        /// <returns>The converted code value.</returns>
        protected string? ConvertCode(string? code)
        {
            if (code == null)
                return null;
            else
                return IsCodeCaseSensitive ? code : code.ToUpperInvariant();
        }

        #region PrivateCollections

        /// <summary>
        /// Internal <see cref="KeyedCollection{Object, TItem}"/> for <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        private class ReferenceDataIdCollection : KeyedCollection<object?, TItem>
        {
            /// <summary>
            /// Gets the key (<see cref="ReferenceDataBase.Id"/>) for the <see cref="ReferenceDataBase"/> item.
            /// </summary>
            /// <param name="item">The <see cref="ReferenceDataBase"/> item.</param>
            /// <returns>The corresponding <see cref="ReferenceDataBase.Id"/>.</returns>
            protected override object? GetKeyForItem(TItem item) => Check.NotNull(item, nameof(item)).Id;
        }

        /// <summary>
        /// Internal <see cref="KeyedCollection{String, TItem}"/> for <see cref="ReferenceDataBase.Code"/>.
        /// </summary>
        private class ReferenceDataCodeCollection : KeyedCollection<string?, TItem>
        {
            private readonly ReferenceDataCollectionBase<TItem> _owner;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReferenceDataCodeCollection"/> class.
            /// </summary>
            /// <param name="owner">The owner collection.</param>
            public ReferenceDataCodeCollection(ReferenceDataCollectionBase<TItem> owner)
            {
                _owner = owner;
            }

            /// <summary>
            /// Gets the key (<see cref="ReferenceDataBase.Code"/>) for the <see cref="ReferenceDataBase"/> item.
            /// </summary>
            /// <param name="item">The <see cref="ReferenceDataBase"/> item.</param>
            /// <returns>The corresponding <see cref="ReferenceDataBase.Code"/>.</returns>
            protected override string? GetKeyForItem(TItem item) => _owner.ConvertCode(Check.NotNull(item, nameof(item)).Code!);
        }

        /// <summary>
        /// Represents a mappings key.
        /// </summary>
        private struct MappingsKey
        {
            public string Name;
            public object? Value;
        }

        #endregion

        #region Add+IEnumerable+Clear

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(TItem item)
        {
            AddItem(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        private void AddItem(TItem item)
        {
            Check.NotNull(item, nameof(item));

            if (item.Id == null)
                throw new ArgumentException("Id must not be null.", nameof(item));

            if (item.Code == null)
                throw new ArgumentException("Code must not be null.", nameof(item));

            lock (_lock)
            {
                // Check uniqueness of Id, Code and Mappings.
                if (_rdcId.Contains(item.Id))
                    throw new ArgumentException($"Item with Id '{item.Id}' already exists within the collection.", nameof(item));

                if (_rdcCode.Contains(ConvertCode(item.Code!)))
                    throw new ArgumentException($"Item with Code '{item.Code!}' already exists within the collection.", nameof(item));

                if (item.HasMappings)
                {
                    foreach (var map in item.Mappings)
                    {
                        var key = new MappingsKey { Name = map.Key, Value = map.Value };
                        if (_mappingsDict.ContainsKey(key))
                            throw new ArgumentException($"Item with Mapping Name '{key.Name}' and Value '{key.Value}' already exists within the collection.");
                    }
                }

                // Once validated they can be added to underlying collections.
                _rdcId.Add(item);
                _rdcCode.Add(item);
                if (item.HasMappings)
                {
                    foreach (var map in item.Mappings)
                    {
                        _mappingsDict.Add(new MappingsKey { Name = map.Key, Value = map.Value }, item.Code!);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the items of the specified collection to the end of the <see cref="ReferenceDataCollectionBase{TItem}"/>.
        /// </summary>
        /// <param name="collection">The collection containing the items to add.</param>
        public void AddRange(IEnumerable<TItem> collection)
        {
            if (collection == null)
                return;

            bool somethingAdded = false;
            foreach (TItem item in collection)
            {
                AddItem(item);
                somethingAdded = true;
            }

            if (somethingAdded)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
        }

        /// <summary>
        /// Supports an iteration over the collection (only items that are <see cref="IsItemValid"/> are enumerated).
        /// </summary>
        /// <remarks>There is no implied sort order; use <see cref="GetList"/> for sorted lists.</remarks>
        public IEnumerator<TItem> GetEnumerator()
        {
            foreach (TItem item in _rdcCode)
            {
                if (IsItemValid(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Supports an iteration over the collection (only items that are <see cref="IsItemValid"/> are enumerated).
        /// </summary>
        /// <remarks>There is no implied sort order; use <see cref="GetList"/> for sorted lists.</remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Supports an iteration over the collection (only items that are <see cref="IsItemValid"/> are enumerated).
        /// </summary>
        /// <returns>There is no implied sort order; use <see cref="GetList"/> for sorted lists.</returns>
        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determines whether the <paramref name="item"/> is considered valid and therefore accessible from within the collection.
        /// </summary>
        /// <param name="item">The item to validate.</param>
        /// <returns><c>true</c> indicates valid; otherwise, <c>false</c>.</returns>
        protected virtual bool IsItemValid(TItem item)
        {
            Check.NotNull(item, nameof(item));
            return item.IsValid;
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _rdcId.Clear();
                _rdcCode.Clear();
                _mappingsDict.Clear();

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #endregion

        #region GetBy+Contains+Indexers

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase IReferenceDataCollection.GetById(int id)
        {
            return GetById(id);
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem GetById(int id)
        {
            if (_rdcId.Contains(id))
                return _rdcId[id];

            return default!;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ReferenceDataBase.Id"/> exists within the collection.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool ContainsId(int id)
        {
            return _rdcId.Contains(id);
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase IReferenceDataCollection.GetById(long id)
        {
            return GetById(id);
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem GetById(long id)
        {
            if (_rdcId.Contains(id))
                return _rdcId[id];

            return default!;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ReferenceDataBase.Id"/> exists within the collection.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool ContainsId(long id)
        {
            return _rdcId.Contains(id);
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase IReferenceDataCollection.GetById(Guid id)
        {
            return GetById(id);
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem GetById(Guid id)
        {
            if (_rdcId.Contains(id))
                return _rdcId[id];

            return default!;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ReferenceDataBase.Id"/> exists within the collection.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool ContainsId(Guid id)
        {
            return _rdcId.Contains(id);
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase IReferenceDataCollection.GetById(string? id)
        {
            return GetById(id);
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem GetById(string? id)
        {
            if (_rdcId.Contains(id))
                return _rdcId[id];

            return default!;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ReferenceDataBase.Id"/> exists within the collection.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool ContainsId(string? id)
        {
            return _rdcId.Contains(id);
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Code"/>.
        /// </summary>
        /// <param name="code">The specified <see cref="ReferenceDataBase.Code"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase? IReferenceDataCollection.GetByCode(string? code)
        {
            return GetByCode(code);
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Code"/>.
        /// </summary>
        /// <param name="code">The specified <see cref="ReferenceDataBase.Code"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem GetByCode(string? code) => _rdcCode.TryGetValue(ConvertCode(code), out var val) ? val : default!;

        /// <summary>
        /// Determines whether the specified <see cref="ReferenceDataBase.Code"/> exists within the collection.
        /// </summary>
        /// <param name="code">The <see cref="ReferenceDataBase.Code"/>.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool ContainsCode(string? code) => _rdcCode.Contains(ConvertCode(code));

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified mapping (<see cref="ReferenceDataBase.SetMapping{T}(string, T)"/>) name and value.
        /// </summary>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase IReferenceDataCollection.GetByMappingValue(string name, IComparable value) => GetByMappingValue(name, value);

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified mapping (<see cref="ReferenceDataBase.SetMapping{T}(string, T)"/>) name and value.
        /// </summary>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, <c>null</c>.</returns>
        public TItem GetByMappingValue(string name, IComparable value) => _mappingsDict.TryGetValue(new MappingsKey { Name = Check.NotNull(name, nameof(name)), Value = value }, out var map) ? GetByCode(map) : default!;

        /// <summary>
        /// Determines whether the specified mapping name and value exists within the collection.
        /// </summary>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool ContainsMappingValue(string name, IComparable value) => _mappingsDict.ContainsKey(new MappingsKey { Name = Check.NotNull(name, nameof(name)), Value = value });

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Id"/> (see <see cref="GetById(int)"/>).
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem this[int id]
        {
            get { return GetById(id); }
            private set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Id"/> (see <see cref="GetById(long)"/>).
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem this[long id]
        {
            get { return GetById(id); }
            private set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Id"/> (see <see cref="GetById(Guid)"/>).
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem this[Guid id]
        {
            get { return GetById(id); }
            private set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the item for the <see cref="ReferenceDataBase.Code"/> (see <see cref="GetByCode"/>).
        /// </summary>
        /// <param name="code">The specified <see cref="ReferenceDataBase.Code"/>.</param>
        /// <returns>The item where found; otherwise, null.</returns>
        public TItem this[string code]
        {
            get { return GetByCode(code); }
            private set { throw new NotSupportedException(); }
        }

        #endregion

        #region List

        /// <summary>
        /// Gets a list of all items sorted by the <see cref="SortOrder"/> value.
        /// </summary>
        /// <value>A <see cref="IList{TItem}"/>.</value>
        /// <remarks>This is provided as a property to more easily support binding; it encapsulates the following method invocation: <c><see cref="GetList"/>(SortOrder, null, null);</c></remarks>
        public List<TItem> AllList
        {
            get { return GetList(SortOrder, null, null); }
        }

        /// <summary>
        /// Gets a list of the <see cref="IsItemValid"/> and <see cref="ReferenceDataBase.IsActive"/> items sorted by the <see cref="SortOrder"/> value.
        /// </summary>
        /// <value>A <see cref="IList{TItem}"/>.</value>
        /// <remarks>This is provided as a property to more easily support binding; it encapsulates the following method invocation: <c><see cref="GetList"/>(SortOrder, true, true);</c></remarks>
        public List<TItem> ActiveList
        {
            get { return GetList(SortOrder, true, true); }
        }

        /// <summary>
        /// Gets a list of <see cref="ReferenceDataBase"/> items from the collection using the specified criteria.
        /// </summary>
        /// <param name="sortOrder">Defines the <see cref="ReferenceDataSortOrder"/>; <c>null</c> indicates to use the defined <see cref="SortOrder"/>.</param>
        /// <param name="isValid">Indicates whether the list should include values with the same <see cref="IsItemValid"/> value; <c>null</c> indicates all.</param>
        /// <param name="isActive">Indicates whether the list should include values with the same <see cref="ReferenceDataBase.IsActive"/> value; <c>null</c> indicates all.</param>
        /// <remakes>This is leveraged by <see cref="AllList"/> and <see cref="ActiveList"/>.</remakes>
        public List<TItem> GetList(ReferenceDataSortOrder? sortOrder = null, bool? isValid = null, bool? isActive = null)
        {
            var list = from rd in _rdcId select rd;
            if (isValid != null)
                list = list.Where(x => IsItemValid(x) == isValid.Value);

            if (isActive != null)
                list = list.Where(x => x.IsActive == isActive.Value);

            list = (sortOrder ?? SortOrder) switch
            {
                ReferenceDataSortOrder.Id => list.OrderBy(x => x.Id),
                ReferenceDataSortOrder.Code => list.OrderBy(x => x.Code),
                ReferenceDataSortOrder.Text => list.OrderBy(x => x.Text),
                _ => list.OrderBy(x => x.SortOrder),
            };

            return list.ToList();
        }

        #endregion

        #region INotifyCollectionChanged

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        #endregion

        #region ICollection

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return _rdcId.Count;
            }
        }

        /// <summary>
        /// Indicates whether the collection is read only; it is not (returns <c>false</c>).
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Determines whether the item exists within the collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if it exists; otherwise, <c>false</c>.</returns>
        public bool Contains(TItem item)
        {
            return _rdcId.Contains<TItem>(item);
        }

        /// <summary>
        /// This method is not supported (a <see cref="NotSupportedException"/> is thrown).
        /// </summary>
        /// <param name="array">The item array.</param>
        /// <param name="arrayIndex">The array index.</param>
        void ICollection<TItem>.CopyTo(TItem[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method is not supported (a <see cref="NotSupportedException"/> is thrown).
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>A <see cref="NotSupportedException"/> is thrown.</returns>
        bool ICollection<TItem>.Remove(TItem item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IETag

        /// <summary>
        /// Gets the <see cref="IETag.ETag"/> for the collection contents.
        /// </summary>
        public string? ETag { get; set; }

        /// <summary>
        /// Generates (updates) an <see cref="ETag"/> as an <see cref="System.Security.Cryptography.SHA1"/> hash of the collection contents.
        /// </summary>
        public void GenerateETag()
        {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms; by-design, used for hashing (speed considered over security).
            using var md5 = System.Security.Cryptography.MD5.Create();
#pragma warning restore CA5351
            var buf = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(this));
            var hash = md5.ComputeHash(buf, 0, buf.Length);
            ETag = Convert.ToBase64String(hash);
        }

        #endregion
    }
}