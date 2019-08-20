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
    /// <remarks>For equality and comparision checking the <see cref="Id"/> and <see cref="Code"/> combination is used.</remarks>
    [DebuggerDisplay("Id = {Id}, Code = {Code}, Text = {Text}, Active = {IsActive}, IsValid = {IsValid}")]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ReferenceDataBase : EntityBase, IReferenceData, IComparable<ReferenceDataBase>, IETag, IChangeLog
    {
        #region RefDataKey

        /// <summary>
        /// Reference Data Key.
        /// </summary>
        private struct RefDataKey : IComparable
        {
            public object Id;
            public string Code;

            public RefDataKey(object id, string code)
            {
                Id = id;
                Code = code;
            }

            public int CompareTo(object obj)
            {
                int res = Comparer<object>.Default.Compare(Id, ((RefDataKey)obj).Id);
                if (res != 0)
                    return res;

                return Comparer<object>.Default.Compare(Code, ((RefDataKey)obj).Code);
            }
        }

        #endregion

        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, ReferenceDataIdTypeCode> _typeCodeDict = new Dictionary<Type, ReferenceDataIdTypeCode>();

        private RefDataKey _key = new RefDataKey(null, null);
        private bool _hasIdBeenUpdated = false;
        private string _text;
        private string _description;
        private int _sortOrder;
        private bool _isActive = true;
        private bool _isInvalid = false;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _etag;
        private ChangeLog _changeLog;
        private Dictionary<string, IComparable> _mappings;

        /// <summary>
        /// Validates the identifier (see <see cref="Id"/>) <see cref="Type"/>.
        /// </summary>
        /// <param name="idTypeCode">The expected <see cref="ReferenceDataIdTypeCode"/>.</param>
        /// <param name="id">The identifier to validate.</param>
        public static void ValidateId(ReferenceDataIdTypeCode idTypeCode, object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            ReferenceDataIdTypeCode typeCode = ReferenceDataIdTypeCode.Unknown;
            if (id is int)
                typeCode = ReferenceDataIdTypeCode.Int32;
            else if (id is Guid)
                typeCode = ReferenceDataIdTypeCode.Guid;
            else
                throw new ArgumentException("Id can only be of Type Int32 or Guid.", nameof(id));

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
                throw new ArgumentNullException(nameof(Type));

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
        protected ReferenceDataBase(ReferenceDataIdTypeCode idTypeCode, object defaultId)
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
        public object Id
        {
            get { return _key.Id; }

            set
            {
                ValidateId(IdTypeCode, value);

                // Exit where they are the same.
                if (Comparer<object>.Default.Compare(value, _key.Id) == 0)
                    return;

                // Where original not the same as unspecified then it has been changed previously.
                if (_hasIdBeenUpdated)
                    throw new InvalidOperationException("Id cannot be changed once already set to a value.");

                _hasIdBeenUpdated = true;
                _key.Id = value;
                OnPropertyChanged(Property_Id);
            }
        }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable).</remarks>
        [JsonProperty("code")]
        public string Code
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
                OnPropertyChanged(Property_Code);
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        [JsonProperty("text")]
        public string Text
        {
            get { return _text; }
            set { SetValue(ref _text, value, false, StringTrim.End, StringTransform.EmptyToNull, Property_Text); }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description
        {
            get { return _description; }
            set { SetValue(ref _description, value, false, StringTrim.End, StringTransform.EmptyToNull, Property_Description); }
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        [JsonProperty("sortOrder", NullValueHandling = NullValueHandling.Ignore)]
        public int SortOrder
        {
            get { return _sortOrder; }
            set { SetValue<int>(ref _sortOrder, value, false, false, Property_SortOrder); }
        }

        /// <summary>
        /// Indicates whether the <see cref="ReferenceDataBase"/> is Active.
        /// </summary>
        /// <value><c>true</c> where Active; otherwise, <c>false</c>.</value>
        [JsonProperty("isActive")]
        public bool IsActive
        {
            get { return _isActive; }
            set { SetValue<bool>(ref _isActive, value, false, false, Property_IsActive); }
        }

        /// <summary>
        /// Gets or sets the validity start date.
        /// </summary>
        [JsonProperty("startDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate
        {
            get { return _startDate; }
            set { SetValue(ref _startDate, value, false, DateTimeTransform.DateOnly, Property_StartDate); }
        }

        /// <summary>
        /// Gets or sets the validity end date.
        /// </summary>
        [JsonProperty("endDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate
        {
            get { return _endDate; }
            set { SetValue(ref _endDate, value, false, DateTimeTransform.DateOnly, Property_EndDate); }
        }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        [JsonProperty("etag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag
        {
            get { return _etag; }
            set { SetValue(ref _etag, value, false, StringTrim.End, StringTransform.EmptyToNull, Property_ETag); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Beef.Entities.ChangeLog"/>.
        /// </summary>
        [JsonProperty("changeLog", NullValueHandling = NullValueHandling.Ignore)]
        public ChangeLog ChangeLog
        {
            get { return _changeLog; }
            set { SetValue<ChangeLog>(ref _changeLog, value, false, true, Property_ChangeLog); }
        }

        /// <summary>
        /// Indicates whether any mapping values have been configured.
        /// </summary>
        public bool HasMappings { get => _mappings != null && _mappings.Count > 0; }

        /// <summary>
        /// Gets the mapping dictionary.
        /// </summary>
        internal Dictionary<string, IComparable> Mappings
        {
            get
            {
                if (_mappings == null)
                    _mappings = new Dictionary<string, IComparable>();

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

                if (StartDate != null && ReferenceDataManager.Context[this.GetType()] < StartDate)
                    return false;

                if (EndDate != null && ReferenceDataManager.Context[this.GetType()] > EndDate)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Overrides the standard <see cref="IsValid"/> check and flags the <see cref="ReferenceDataBase"/> as <b>Invalid</b>.
        /// </summary>
        public void SetInvalid()
        {
            _isInvalid = true;
        }

        /// <summary>
        /// Sets the mapping <paramref name="value"/> for the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <remarks>A <paramref name="value"/> with the default value will not be set; assumed in this case that no mapping exists.</remarks>
        protected internal void SetMapping<T>(string name, T value) where T : IComparable
        {
            if (Comparer<T>.Default.Compare(value, default(T)) == 0)
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
        public T GetMapping<T>(string name) where T : IComparable
        {
            if (!HasMappings || !Mappings.TryGetValue(name, out IComparable value))
                return default(T);

            return (T)value;
        }

        /// <summary>
        /// Gets a mapping value for the <see cref="ReferenceDataBase"/> for the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <returns><c>true</c> indicates that the name exists; otherwise, <c>false</c>.</returns>
        public bool TryGetMapping<T>(string name, out T value) where T : IComparable
        {
            IComparable val = default(T);
            value = (T)val;
            if (!HasMappings || !Mappings.TryGetValue(name, out val))
                return false;

            value = (T)val;
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
                val = (T)rd.GetById(id);
                if (val != null)
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
                val = (T)rd.GetById(id);
                if (val != null)
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
        public static T ConvertFromCode<T>(string code) where T : ReferenceDataBase, new()
        {
            if (code == null)
                return null;

            IReferenceDataCollection rd = ReferenceDataManager.Current[typeof(T)];
            T val;
            if (rd != null)
            {
                val = (T)rd.GetByCode(code);
                if (val != null)
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
                val = (T)rd.GetByMappingValue(name, value);
                if (val != null)
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
        public override bool Equals(object obj)
        {
            // A null is not equal.
            if (obj == null)
                return false;

            // Ensure the two types are the same.
            if (this.GetType() != obj.GetType())
                return false;

            return this._key.Equals(((ReferenceDataBase)obj)._key);
        }

        /// <summary>
        /// Returns a hash code for the <see cref="ReferenceDataBase"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="ReferenceDataBase"/>.</returns>
        public override int GetHashCode()
        {
            return this._key.GetHashCode();
        }

        /// <summary>
        /// Compares two <see cref="ReferenceDataBase"/> types for equality.
        /// </summary>
        /// <param name="a">A <see cref="ReferenceDataBase"/>.</param>
        /// <param name="b">B <see cref="ReferenceDataBase"/>.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator ==(ReferenceDataBase a, ReferenceDataBase b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null)
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Compares two <see cref="ReferenceDataBase"/> types for non-equality.
        /// </summary>
        /// <param name="a">A <see cref="ReferenceDataBase"/>.</param>
        /// <param name="b">B <see cref="ReferenceDataBase"/>.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator !=(ReferenceDataBase a, ReferenceDataBase b)
        {
            return !(a == b);
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to an <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value; where <c>null</c> then <b>zero</b> will be returned.</returns>
        public static implicit operator int(ReferenceDataBase value)
        {
            if (value == null)
                return 0;
            else
                return (value.Id != null && value.Id is int) ? (int)value.Id : 0;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="Nullable{Int}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value.</returns>
        public static implicit operator int?(ReferenceDataBase value)
        {
            if (value == null)
                return null;
            else
                return (value.Id != null && value.Id is int) ? (int)value.Id : 0;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value; where <c>null</c> then <see cref="Guid.Empty"/> will be returned.</returns>
        public static implicit operator Guid(ReferenceDataBase value)
        {
            if (value == null)
                return Guid.Empty;
            else
                return (value.Id != null && value.Id is Guid) ? (Guid)value.Id : Guid.Empty;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="Nullable{Guid}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Id"/> value.</returns>
        public static implicit operator Guid?(ReferenceDataBase value)
        {
            if (value == null)
                return null;
            else
                return (value.Id != null && value.Id is Guid) ? (Guid)value.Id : Guid.Empty;
        }

        /// <summary>
        /// An implicit cast from the <see cref="ReferenceDataBase"/> to a <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ReferenceDataBase"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase.Code"/> value; where <c>null</c> then a <c>null</c> will be returned.</returns>
        public static implicit operator string(ReferenceDataBase value)
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
            if (other == null || this.GetType() != other.GetType())
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
            if (from == null)
                throw new ArgumentNullException("from");

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
        /// Performs a clean-up of the <see cref="ReferenceDataBase"/> resetting property values as appropriate to ensure
        /// a basic data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Text = Cleaner.Clean<string>(_text);
            Description = Cleaner.Clean<string>(_description);
            IsActive = Cleaner.Clean<bool>(_isActive);
            StartDate = Cleaner.Clean(_startDate, DateTimeTransform.DateOnly);
            EndDate = Cleaner.Clean(_endDate, DateTimeTransform.DateOnly);
        }

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
        public virtual string StringFormat
        {
            get { return "{2}"; }
        }

        /// <summary>
        /// Returns a string representation using the default <see cref="StringFormat"/>.
        /// </summary>
        /// <returns>The appropriately formatted string.</returns>
        public override string ToString()
        {
            return ToString(StringFormat);
        }

        /// <summary>
        /// Returns a string representation using the specified <paramref name="stringFormat"/>.
        /// </summary>
        /// <param name="stringFormat">A specified string format.</param>
        /// <returns>The appropriately formatted string.</returns>
        public string ToString(string stringFormat)
        {
            return string.Format(stringFormat, Id, Code, Text);
        }

        #endregion

        #region PropertyNames

        /// <summary>
        /// Represents the <see cref="Id"/> property name.
        /// </summary>
        public const string Property_Id = "Id";

        /// <summary>
        /// Represents the <see cref="Code"/> property name.
        /// </summary>
        public const string Property_Code = "Code";

        /// <summary>
        /// Represents the <see cref="Text"/> property name.
        /// </summary>
        public const string Property_Text = "Text";

        /// <summary>
        /// Represents the <see cref="Description"/> property name.
        /// </summary>
        public const string Property_Description = "Description";

        /// <summary>
        /// Represents the <see cref="SortOrder"/> property name.
        /// </summary>
        public const string Property_SortOrder = "SortOrder";

        /// <summary>
        /// Represents the <see cref="IsActive"/> property name.
        /// </summary>
        public const string Property_IsActive = "IsActive";

        /// <summary>
        /// Represents the <see cref="IsValid"/> property name.
        /// </summary>
        public const string Property_IsValid = "IsValid";

        /// <summary>
        /// Represents the <see cref="StartDate"/> property name.
        /// </summary>
        public const string Property_StartDate = "StartDate";

        /// <summary>
        /// Represents the <see cref="EndDate"/> property name.
        /// </summary>
        public const string Property_EndDate = "EndDate";

        /// <summary>
        /// Represents the <see cref="ETag"/> property name.
        /// </summary>
        public const string Property_ETag = "ETag";

        /// <summary>
        /// Represents the <see cref="ChangeLog"/> property name.
        /// </summary>
        public const string Property_ChangeLog = "ChangeLog";

        #endregion
    }
}
