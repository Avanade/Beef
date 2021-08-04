// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a <b>ReferenceData</b> base class.
    /// </summary>
    /// <remarks>For equality and comparision checking the <see cref="Id"/> and <see cref="Code"/> combination is used (all other properties are ignored).</remarks>
    [DebuggerDisplay("Id = {Id}, Code = {Code}, Text = {Text}, Active = {IsActive}, IsValid = {IsValid}")]
    [JsonObject(MemberSerialization.OptIn)]
#pragma warning disable CA1036 // Override methods on comparable types; support for <, <=, > and >= not supported by-design.
    public abstract class ReferenceDataBase : EntityBase, IReferenceData, IComparable<ReferenceDataBase>, IConvertible, IEquatable<ReferenceDataBase>, IETag, IChangeLog, IIdentifier, IUniqueKey
#pragma warning restore CA1036
    {
        #region RefDataKey

        /// <summary>
        /// Reference Data Key.
        /// </summary>
        private struct RefDataKey : IComparable
        {
            public object? Id;
            public string? Code;
            
            /// <summary>
            /// Initialize new.
            /// </summary>
            public RefDataKey(object? id, string? code)
            {
                Id = id;
                Code = code;
            }

            /// <summary>
            /// Compare to another.
            /// </summary>
            public int CompareTo(object obj)
            {
                if (Check.NotNull(obj, nameof(obj)) is RefDataKey rdk)
                {
                    int res = Comparer<object?>.Default.Compare(Id, rdk.Id);
                    if (res != 0)
                        return res;

                    return Comparer<string?>.Default.Compare(Code, rdk.Code);
                }
                else
                    throw new InvalidOperationException("Object must be of type RefDataKey.");
            }

            /// <summary>
            /// Gets the hash code.
            /// </summary>
            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Code);
            }
        }

        #endregion

        private static readonly object _lock = new();
        private static readonly Dictionary<Type, ReferenceDataIdTypeCode> _typeCodeDict = new();

        private RefDataKey _key = new(null, null);
        private bool _hasIdBeenUpdated = false;
        private string? _text;
        private string? _description;
        private int _sortOrder;
        private bool _isActive = true;
        private bool _isInvalid = false;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string? _etag;
        private ChangeLog? _changeLog;
        private Dictionary<string, object?>? _mappings;

        /// <summary>
        /// Validates the identifier (see <see cref="Id"/>) <see cref="Type"/>.
        /// </summary>
        /// <param name="idTypeCode">The expected <see cref="ReferenceDataIdTypeCode"/>.</param>
        /// <param name="id">The identifier to validate.</param>
        public static void ValidateId(ReferenceDataIdTypeCode idTypeCode, object? id)
        {
            ReferenceDataIdTypeCode typeCode;
            if (id is int)
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(id));

                typeCode = ReferenceDataIdTypeCode.Int32;
            }
            else if (id is long)
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(id));

                typeCode = ReferenceDataIdTypeCode.Int64;
            }
            else if (id is Guid)
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(id));

                typeCode = ReferenceDataIdTypeCode.Guid;
            }
            else if (id == null || id is string)
                typeCode = ReferenceDataIdTypeCode.String;
            else
                throw new ArgumentException("Id can only be of Type Int32, Int64, Guid or String.", nameof(id));

            if (typeCode != idTypeCode)
                throw new ArgumentException($"Reference Data identifier value has an invalid TypeCode '{typeCode}'; expected TypeCode '{idTypeCode}'.");
        }

        /// <summary>
        /// Gets the <see cref="ReferenceDataIdTypeCode"/> for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The <see cref="ReferenceDataIdTypeCode"/>.</returns>
        public static ReferenceDataIdTypeCode GetIdTypeCode(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (_typeCodeDict.ContainsKey(type))
                return _typeCodeDict[type];

            if (!type.GetTypeInfo().IsSubclassOf(typeof(ReferenceDataBase)))
                return ReferenceDataIdTypeCode.Unknown;

            // Cache for next time.
            lock (_lock)
            {
                if (_typeCodeDict.ContainsKey(type))
                    return _typeCodeDict[type];

                var rd = (ReferenceDataBase)Activator.CreateInstance(type);
                _typeCodeDict.Add(type, rd.IdTypeCode);
                return rd.IdTypeCode;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataBase"/> class.
        /// </summary>
        /// <param name="idTypeCode">The <see cref="ReferenceDataIdTypeCode"/>.</param>
        /// <param name="defaultId">The default identifier.</param>
        protected ReferenceDataBase(ReferenceDataIdTypeCode idTypeCode, object? defaultId)
        {
            IdTypeCode = idTypeCode;
            ValidateId(IdTypeCode, defaultId);
            _key.Id = defaultId;
        }

        #region Properties

        /// <summary>
        /// Gets the <see cref="ReferenceDataIdTypeCode"/> for the <see cref="Id"/>.
        /// </summary>
        public ReferenceDataIdTypeCode IdTypeCode { get; private set; }

        /// <summary>
        /// Gets or sets the identifier (<see cref="int"/> or <see cref="Guid"/>).
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable). See <see cref="ReferenceDataIdTypeCode"/> for supported <see cref="Type"/> code options.</remarks>
        [Display(Name = "Identifier")]
        public object? Id
        {
            get { return _key.Id; }

            set
            {
                ValidateId(IdTypeCode, value);

                // Exit where they are the same.
                if (Comparer<object?>.Default.Compare(value, _key.Id) == 0)
                    return;

                // Where original not the same as unspecified then it has been changed previously.
                if (_hasIdBeenUpdated)
                    throw new InvalidOperationException("Id cannot be changed once already set to a value.");

                _hasIdBeenUpdated = true;
                _key.Id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable).</remarks>
        [JsonProperty("code")]
        public string? Code
        {
            get { return _key.Code; }

            set
            {
                // Exit where they are the same.
                if (value == _key.Code)
                    return;

                // Where original not the same as unspecified then it has been changed previously.
                if (_key.Code != null)
                    throw new InvalidOperationException("Code cannot be changed once already set to a value.");

                _key.Code = value?.TrimEnd();
                OnPropertyChanged(nameof(Code));
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        [JsonProperty("text")]
        public string? Text
        {
            get => _text;
            set => SetValue(ref _text!, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(Text));
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description
        {
            get => _description;
            set => SetValue(ref _description!, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Description));
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        [JsonProperty("sortOrder", NullValueHandling = NullValueHandling.Ignore)]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetValue(ref _sortOrder, value, false, false, nameof(SortOrder));
        }

        /// <summary>
        /// Indicates whether the <see cref="ReferenceDataBase"/> is Active.
        /// </summary>
        /// <value><c>true</c> where Active; otherwise, <c>false</c>.</value>
        [JsonProperty("isActive")]
        public bool IsActive
        {
            get => _isActive;
            set => SetValue(ref _isActive, value, false, false, nameof(IsActive));
        }

        /// <summary>
        /// Gets or sets the validity start date.
        /// </summary>
        [JsonProperty("startDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate
        {
            get => _startDate;
            set => SetValue(ref _startDate, value, false, DateTimeTransform.DateOnly, nameof(StartDate));
        }

        /// <summary>
        /// Gets or sets the validity end date.
        /// </summary>
        [JsonProperty("endDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate
        {
            get => _endDate;
            set => SetValue(ref _endDate, value, false, DateTimeTransform.DateOnly, nameof(EndDate));
        }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        [JsonProperty("etag", NullValueHandling = NullValueHandling.Ignore)]
        public string? ETag
        {
            get => _etag;
            set => SetValue(ref _etag, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(ETag));
        }

        /// <summary>
        /// Gets or sets the <see cref="Beef.Entities.ChangeLog"/>.
        /// </summary>
        [JsonProperty("changeLog", NullValueHandling = NullValueHandling.Ignore)]
        public ChangeLog? ChangeLog
        {
            get => _changeLog;
            set => SetValue(ref _changeLog!, value, false, true, nameof(ChangeLog));
        }

        /// <summary>
        /// Indicates whether any mapping values have been configured.
        /// </summary>
        public bool HasMappings { get => _mappings != null && _mappings.Count > 0; }

        /// <summary>
        /// Gets the mapping dictionary.
        /// </summary>
        internal Dictionary<string, object?> Mappings
        {
            get
            {
                if (_mappings == null)
                    _mappings = new Dictionary<string, object?>();

                return _mappings;
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="ReferenceDataBase"/> is valid (<see cref="SetInvalid"/> has not been invoked).
        /// </summary>
        /// <remarks>Note to classes that inherit: the base <see cref="ReferenceDataBase.IsValid"/> should be called first and further checks should
        /// only be performed where valid (<c>true</c>); otherwise, it should always be considered invalid (<c>false</c>).
        /// <para>The checks are performed as follows to determine validity: <see cref="SetInvalid"/> has been called, the value is marked as not <see cref="IsActive"/> 
        /// or the <see cref="StartDate"/> and <see cref="EndDate"/> are outside of the set <see cref="ReferenceDataManager.Context"/> date.
        /// </para></remarks>
        public virtual bool IsValid
        {
            get
            {
                if (_isInvalid || !_isActive)
                    return false;

                if (StartDate != null && ReferenceDataManager.Context[GetType()] < StartDate)
                    return false;

                if (EndDate != null && ReferenceDataManager.Context[GetType()] > EndDate)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Overrides the standard <see cref="IsValid"/> check and flags the <see cref="ReferenceDataBase"/> as <b>Invalid</b>.
        /// </summary>
        /// <remarks>Will result in <see cref="IsActive"/> set to <c>false</c>.</remarks>
        public void SetInvalid()
        {
            _isInvalid = true;
            _isActive = false;
        }

        /// <summary>
        /// Sets the mapping <paramref name="value"/> for the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <remarks>A <paramref name="value"/> with the default value will not be set; assumed in this case that no mapping exists.</remarks>
        protected internal void SetMapping<T>(string name, T value)
        {
            if (Comparer<T>.Default.Compare(value, default!) == 0)
                return;

            if (Mappings.ContainsKey(name))
                throw new InvalidOperationException(ValueIsImmutableMessage);

            Mappings.Add(name, value);
        }

        /// <summary>
        /// Gets a mapping value for the <see cref="ReferenceDataBase"/> for the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The mapping name.</param>
        /// <returns>The mapping value where found; otherwise, the corresponding default value.</returns>
        public T GetMapping<T>(string name)
        {
            if (!HasMappings || !Mappings.TryGetValue(name, out var value))
                return default!;

            return (T)value!;
        }

        /// <summary>
        /// Gets a mapping value for the <see cref="ReferenceDataBase"/> for the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <returns><c>true</c> indicates that the name exists; otherwise, <c>false</c>.</returns>
        public bool TryGetMapping<T>(string name, out T value)
        {
            value = default!;
            if (!HasMappings || !Mappings.TryGetValue(name, out var val))
                return false;

            value = (T)val!;
            return true;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Performs a conversion from an <see cref="ReferenceDataBase.Id"/> to a <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The corresponding <see cref="ReferenceDataBase"/>.</returns>
        /// <remarks>Where the item (<see cref="ReferenceDataBase"/>) is not found it will be created and <see cref="ReferenceDataBase.SetInvalid"/> will be invoked.</remarks>
        public static T ConvertFromId<T>(int id) where T : ReferenceDataBase, new()
        {
            IReferenceDataCollection rd = ReferenceDataManager.Current[typeof(T)];
            T val;
            if (rd != null)
            {
                val = (T)rd.GetById(id)!;
                if (val != default!)
                    return val;
            }

            val = new T { Id = id };
            val.SetInvalid();
            return val;
        }

        /// <summary>
        /// Performs a conversion from an <see cref="ReferenceDataBase.Id"/> to a <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The corresponding <see cref="ReferenceDataBase"/>.</returns>
        /// <remarks>Where the item (<see cref="ReferenceDataBase"/>) is not found it will be created and <see cref="ReferenceDataBase.SetInvalid"/> will be invoked.</remarks>
        public static T ConvertFromId<T>(long id) where T : ReferenceDataBase, new()
        {
            IReferenceDataCollection rd = ReferenceDataManager.Current[typeof(T)];
            T val;
            if (rd != null)
            {
                val = (T)rd.GetById(id)!;
                if (val != default!)
                    return val;
            }

            val = new T { Id = id };
            val.SetInvalid();
            return val;
        }

        /// <summary>
        /// Performs a conversion from an <see cref="ReferenceDataBase.Id"/> to a <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The corresponding <see cref="ReferenceDataBase"/>.</returns>
        /// <remarks>Where the item (<see cref="ReferenceDataBase"/>) is not found it will be created and <see cref="ReferenceDataBase.SetInvalid"/> will be invoked.</remarks>
        public static T ConvertFromId<T>(Guid id) where T : ReferenceDataBase, new()
        {
            IReferenceDataCollection rd = ReferenceDataManager.Current[typeof(T)];
            T val;
            if (rd != null)
            {
                val = (T)rd.GetById(id)!;
                if (val != default!)
                    return val;
            }

            val = new T { Id = id };
            val.SetInvalid();
            return val;
        }

        /// <summary>
        /// Performs a conversion from an <see cref="ReferenceDataBase.Id"/> to a <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="id">The <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The corresponding <see cref="ReferenceDataBase"/>.</returns>
        /// <remarks>Where the item (<see cref="ReferenceDataBase"/>) is not found it will be created and <see cref="ReferenceDataBase.SetInvalid"/> will be invoked.</remarks>
        public static T ConvertFromId<T>(string? id) where T : ReferenceDataBase, new()
        {
            IReferenceDataCollection rd = ReferenceDataManager.Current[typeof(T)];
            T val;
            if (rd != null)
            {
                val = (T)rd.GetById(id)!;
                if (val != default!)
                    return val;
            }

            val = new T { Id = id };
            val.SetInvalid();
            return val;
        }

        /// <summary>
        /// Performs a conversion from an <see cref="ReferenceDataBase.Code"/> to a <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="code">The <see cref="ReferenceDataBase.Code"/>.</param>
        /// <returns>The corresponding <see cref="ReferenceDataBase"/>.</returns>
        /// <remarks>Where the item (<see cref="ReferenceDataBase"/>) is not found it will be created and <see cref="ReferenceDataBase.SetInvalid"/> will be invoked.</remarks>
        public static T ConvertFromCode<T>(string? code) where T : ReferenceDataBase, new()
        {
            if (code == null)
                return default!;

            IReferenceDataCollection rd = ReferenceDataManager.Current[typeof(T)];
            T val;
            if (rd != null)
            {
                val = (T)rd.GetByCode(code)!;
                if (val != default!)
                    return val;
            }

            val = new T { Code = code };
            val.SetInvalid();
            return val;
        }

        /// <summary>
        /// Performs a conversion from a specified mapping to a <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <returns>The corresponding <see cref="ReferenceDataBase"/>.</returns>
        /// <remarks>Where the item (<see cref="ReferenceDataBase"/>) is not found it will be created and <see cref="ReferenceDataBase.SetInvalid"/> will be invoked.</remarks>
        public static T ConvertFromExternalId<T>(string name, IComparable value) where T : ReferenceDataBase, new()
        {
            IReferenceDataCollection rd = ReferenceDataManager.Current[typeof(T)];
            T val;
            if (rd != null)
            {
                val = (T)rd.GetByMappingValue(name, value)!;
                if (val != default!)
                    return val;
            }

            val = new T();
            val.SetInvalid();
            return val;
        }

        /// <summary>
        /// Determines whether the <see cref="ReferenceDataBase"/> is equal to the current <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="obj">The <see cref="ReferenceDataBase"/> to compare with.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        /// <remarks>Both the <see cref="Id"/> and <see cref="Code"/> must have the same values to be considered equal.</remarks>
        public override bool Equals(object? obj)
        {
            // A null is not equal.
            if (obj == null)
                return false;

            // Ensure the two types are the same.
            if (GetType() != obj.GetType())
                return false;

            return _key.Equals(((ReferenceDataBase)obj)._key);
        }

        /// <summary>
        /// Determines whether the <see cref="ReferenceDataBase"/> is equal to the current <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/> to compare with.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        /// <remarks>Both the <see cref="Id"/> and <see cref="Code"/> must have the same values to be considered equal.</remarks>
        public bool Equals(ReferenceDataBase? value)
        {
            if (((object)value!) == ((object)this))
                return true;
            else if (((object)value!) == null)
                return false;

            return _key.Equals(value._key);
        }

        /// <summary>
        /// Returns a hash code for the <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="ReferenceDataBase"/>.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_key);
        }

        /// <summary>
        /// Compares two <see cref="ReferenceDataBase"/> types for equality.
        /// </summary>
        /// <param name="a">A <see cref="ReferenceDataBase"/>.</param>
        /// <param name="b">B <see cref="ReferenceDataBase"/>.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator ==(ReferenceDataBase? a, ReferenceDataBase? b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Compares two <see cref="ReferenceDataBase"/> types for non-equality.
        /// </summary>
        /// <param name="a">A <see cref="ReferenceDataBase"/>.</param>
        /// <param name="b">B <see cref="ReferenceDataBase"/>.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator !=(ReferenceDataBase? a, ReferenceDataBase? b)
        {
            return !(a == b);
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to an <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value; where <c>null</c> then <b>zero</b> will be returned.</returns>
        public static implicit operator int(ReferenceDataBase? value)
        {
            if (value == null)
                return 0;
            else
                return (value.Id != null && value.Id is int id) ? id : 0;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="Nullable{Int}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value.</returns>
        public static implicit operator int?(ReferenceDataBase? value)
        {
            if (value == null)
                return null;
            else
                return (value.Id != null && value.Id is int id) ? id : 0;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value; where <c>null</c> then <see cref="Guid.Empty"/> will be returned.</returns>
        public static implicit operator Guid(ReferenceDataBase? value)
        {
            if (value == null)
                return Guid.Empty;
            else
                return (value.Id != null && value.Id is Guid id) ? id : Guid.Empty;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="Nullable{Guid}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value.</returns>
        public static implicit operator Guid?(ReferenceDataBase? value)
        {
            if (value == null)
                return null;
            else
                return (value.Id != null && value.Id is Guid id) ? id : Guid.Empty;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Code"/> value; where <c>null</c> then a <c>null</c> will be returned.</returns>
        public static implicit operator string?(ReferenceDataBase? value)
        {
            if (value == null)
                return null;
            else
                return value.Code;
        }

        #endregion

        #region IComparable

        /// <summary>
        /// Compares the current <see cref="ReferenceDataBase"/> with another <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <param name="other">An <see cref="ReferenceDataBase"/> to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared (see <see cref="IComparable.CompareTo"/>).</returns>
        public int CompareTo(ReferenceDataBase other)
        {
            if (other == null || GetType() != other.GetType())
                return 1;

            return Comparer<RefDataKey>.Default.Compare(_key, other._key);
        }

        #endregion

        #region ICopyFrom

        /// <summary>
        /// Copies from another <see cref="ReferenceDataBase"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="ReferenceDataBase"/> to copy from.</param>
        public void CopyFrom(ReferenceDataBase from)
        {
            Check.NotNull(from, nameof(from));
            Id = from.Id;
            Code = from.Code;
            Text = from.Text;
            Description = from.Description;
            SortOrder = from.SortOrder;
            IsActive = from.IsActive;
            StartDate = from.StartDate;
            EndDate = from.EndDate;
            ETag = from.ETag;
            ChangeLog = (from.ChangeLog == null) ? null : (ChangeLog)from.ChangeLog.Clone();
        }

        #endregion

        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="ReferenceDataBase"/> resetting property values as appropriate to ensure a basic data consistency.
        /// </summary>
        public override void CleanUp() => base.CleanUp();

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        /// <returns><c>true</c> indicates is initial; otherwise, <c>false</c>.</returns>
        public override bool IsInitial => false;

        #endregion

        #region ToString

        /// <summary>
        /// Gets the default composite string format.
        /// </summary>
        /// <remarks>The default value is "{2}".
        /// <para>The string format supports the standard composite formatting; where the following indexes are used:
        /// <list type="table">
        ///   <listheader>
        ///     <term>Index</term>
        ///     <description>Source</description>
        ///   </listheader>
        ///   <item>
        ///     <term>{0}</term>
        ///     <description>The <see cref="Id"/>.</description>
        ///   </item>
        ///   <item>
        ///     <term>{1}</term>
        ///     <description>The <see cref="Code"/>.</description>
        ///   </item>
        ///   <item>
        ///     <term>{2}</term>
        ///     <description>The <see cref="Text"/>.</description>
        ///   </item>
        /// </list></para></remarks>
        public virtual string StringFormat => "{2}";

        /// <summary>
        /// Returns a string representation using the default <see cref="StringFormat"/>.
        /// </summary>
        /// <returns>The appropriately formatted string.</returns>
        public override string ToString() => ToString(StringFormat);

        /// <summary>
        /// Returns a string representation using the specified <paramref name="stringFormat"/>.
        /// </summary>
        /// <param name="stringFormat">A specified string format.</param>
        /// <returns>The appropriately formatted string.</returns>
        public string ToString(string stringFormat) => string.Format(System.Globalization.CultureInfo.InvariantCulture, stringFormat, Id, Code, Text);

        #endregion

        #region IConvertible

        /// <summary>
        /// Gets the <see cref="TypeCode"/> being <see cref="TypeCode.Object"/>.
        /// </summary>
        /// <returns>The <see cref="TypeCode.Object"/> value.</returns>
        TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

        /// <summary>
        /// Converts the value to a <see cref="bool"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        bool IConvertible.ToBoolean(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="byte"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        byte IConvertible.ToByte(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="char"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        char IConvertible.ToChar(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="DateTime"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="decimal"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        decimal IConvertible.ToDecimal(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="double"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        double IConvertible.ToDouble(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="short"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        short IConvertible.ToInt16(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="int"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        int IConvertible.ToInt32(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="long"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        long IConvertible.ToInt64(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="sbyte"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        sbyte IConvertible.ToSByte(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="float"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        float IConvertible.ToSingle(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="string"/>; returns the <see cref="Code"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The <see cref="Code"/>.</returns>
        string IConvertible.ToString(IFormatProvider provider) => Code!;

        /// <summary>
        /// Converts the value to an <see cref="object"/> of the specified <see cref="Type"/> that has an equivalent value.
        /// </summary>
        /// <param name="conversionType">The <see cref="Type"/> to which the value of this instance is converted.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>The converted value.</returns>
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(string))
                return Code!;

            throw new InvalidCastException();
        }

        /// <summary>
        /// Converts the value to a <see cref="ushort"/>; throws a <see crefconvert="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        ushort IConvertible.ToUInt16(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="uint"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        uint IConvertible.ToUInt32(IFormatProvider provider) => throw new InvalidCastException();

        /// <summary>
        /// Converts the value to a <see cref="ulong"/>; throws a <see cref="InvalidCastException"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/>.</param>
        /// <returns>Throws a <see cref="InvalidCastException"/>.</returns>
        ulong IConvertible.ToUInt64(IFormatProvider provider) => throw new InvalidCastException();


        #endregion

        #region IUniqueKey

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UniqueKey UniqueKey => new(Id);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string[] UniqueKeyProperties => new string[] { nameof(Id) };

        #endregion
    }
}