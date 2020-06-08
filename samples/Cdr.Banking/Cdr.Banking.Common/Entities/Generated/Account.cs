/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Beef.RefData;
using Newtonsoft.Json;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Common.Entities
{
    /// <summary>
    /// Represents the Account entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class Account : EntityBase, IStringIdentifier, IEquatable<Account>
    {
        #region Privates

        private string? _id;
        private DateTime _creationDate;
        private string? _displayName;
        private string? _nickname;
        private string? _openStatusSid;
        private bool _isOwned;
        private string? _maskedNumber;
        private string? _productCategorySid;
        private string? _productName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Account"/> identifier.
        /// </summary>
        [JsonProperty("accountId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Identifier")]
        public string? Id
        {
            get => _id;
            set => SetValue(ref _id, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Id)); 
        }

        /// <summary>
        /// Gets or sets the Creation Date.
        /// </summary>
        [JsonProperty("creationDate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Creation Date")]
        [DisplayFormat(DataFormatString = Beef.Entities.StringFormat.DateOnlyFormat)]
        public DateTime CreationDate
        {
            get => _creationDate;
            set => SetValue(ref _creationDate, value, false, DateTimeTransform.DateOnly, nameof(CreationDate)); 
        }

        /// <summary>
        /// Gets or sets the Display Name.
        /// </summary>
        [JsonProperty("displayName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Display Name")]
        public string? DisplayName
        {
            get => _displayName;
            set => SetValue(ref _displayName, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(DisplayName)); 
        }

        /// <summary>
        /// Gets or sets the Nickname.
        /// </summary>
        [JsonProperty("nickname", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Nickname")]
        public string? Nickname
        {
            get => _nickname;
            set => SetValue(ref _nickname, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Nickname)); 
        }

        /// <summary>
        /// Gets or sets the <see cref="OpenStatus"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("openStatus", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Open Status")]
        public string? OpenStatusSid
        {
            get => _openStatusSid;
            set => SetValue(ref _openStatusSid, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(OpenStatus));
        }

        /// <summary>
        /// Gets or sets the Open Status (see <see cref="RefDataNamespace.OpenStatus"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Open Status")]
        public RefDataNamespace.OpenStatus? OpenStatus
        {
            get => _openStatusSid;
            set => SetValue(ref _openStatusSid, value, false, false, nameof(OpenStatus)); 
        }

        /// <summary>
        /// Gets or sets a value indicating whether Is Owned.
        /// </summary>
        [JsonProperty("isOwned", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Is Owned")]
        public bool IsOwned
        {
            get => _isOwned;
            set => SetValue(ref _isOwned, value, false, false, nameof(IsOwned)); 
        }

        /// <summary>
        /// Gets or sets the Masked Number.
        /// </summary>
        [JsonProperty("maskedNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Masked Number")]
        public string? MaskedNumber
        {
            get => _maskedNumber;
            set => SetValue(ref _maskedNumber, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(MaskedNumber)); 
        }

        /// <summary>
        /// Gets or sets the <see cref="ProductCategory"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("productCategory", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Product Category")]
        public string? ProductCategorySid
        {
            get => _productCategorySid;
            set => SetValue(ref _productCategorySid, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(ProductCategory));
        }

        /// <summary>
        /// Gets or sets the Product Category (see <see cref="RefDataNamespace.ProductCategory"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Product Category")]
        public RefDataNamespace.ProductCategory? ProductCategory
        {
            get => _productCategorySid;
            set => SetValue(ref _productCategorySid, value, false, false, nameof(ProductCategory)); 
        }

        /// <summary>
        /// Gets or sets the Product Name.
        /// </summary>
        [JsonProperty("productName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Product Name")]
        public string? ProductName
        {
            get => _productName;
            set => SetValue(ref _productName, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(ProductName)); 
        }

        #endregion

        #region UniqueKey
      
        /// <summary>
        /// Indicates whether the <see cref="Account"/> has a <see cref="UniqueKey"/> value.
        /// </summary>
        public override bool HasUniqueKey => true;
        
        /// <summary>
        /// Gets the list of property names that represent the unique key.
        /// </summary>
        public override string[] UniqueKeyProperties => new string[] { nameof(Id) };
        
        /// <summary>
        /// Creates the <see cref="UniqueKey"/>.
        /// </summary>
        /// <returns>The <see cref="Beef.Entities.UniqueKey"/>.</returns>
        /// <param name="accountId">The <see cref="Id"/>.</param>
        public static UniqueKey CreateUniqueKey(string accountId) => new UniqueKey(accountId);
          
        /// <summary>
        /// Gets the <see cref="UniqueKey"/>.
        /// </summary>
        /// <remarks>
        /// The <b>UniqueKey</b> key consists of the following property(s): <see cref="Id"/>.
        /// </remarks>
        public override UniqueKey UniqueKey => new UniqueKey(Id);

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Account val))
                return false;

            return Equals(val);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Account"/> is equal to the current <see cref="Account"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(Account? value)
        {
            if (((object)value!) == ((object)this))
                return true;
            else if (((object)value!) == null)
                return false;

            return base.Equals((object)value)
                && Equals(Id, value.Id)
                && Equals(CreationDate, value.CreationDate)
                && Equals(DisplayName, value.DisplayName)
                && Equals(Nickname, value.Nickname)
                && Equals(OpenStatusSid, value.OpenStatusSid)
                && Equals(IsOwned, value.IsOwned)
                && Equals(MaskedNumber, value.MaskedNumber)
                && Equals(ProductCategorySid, value.ProductCategorySid)
                && Equals(ProductName, value.ProductName);
        }

        /// <summary>
        /// Compares two <see cref="Account"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="Account"/> A.</param>
        /// <param name="b"><see cref="Account"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (Account? a, Account? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="Account"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="Account"/> A.</param>
        /// <param name="b"><see cref="Account"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (Account? a, Account? b) => !Equals(a, b);

        /// <summary>
        /// Returns a hash code for the <see cref="Account"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="Account"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(CreationDate);
            hash.Add(DisplayName);
            hash.Add(Nickname);
            hash.Add(OpenStatusSid);
            hash.Add(IsOwned);
            hash.Add(MaskedNumber);
            hash.Add(ProductCategorySid);
            hash.Add(ProductName);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion
        
        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="Account"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Account"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<Account>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="Account"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Account"/> to copy from.</param>
        public void CopyFrom(Account from)
        {
            CopyFrom((EntityBase)from);
            Id = from.Id;
            CreationDate = from.CreationDate;
            DisplayName = from.DisplayName;
            Nickname = from.Nickname;
            OpenStatusSid = from.OpenStatusSid;
            IsOwned = from.IsOwned;
            MaskedNumber = from.MaskedNumber;
            ProductCategorySid = from.ProductCategorySid;
            ProductName = from.ProductName;

            OnAfterCopyFrom(from);
        }
    
        #endregion
        
        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="Account"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="Account"/>.</returns>
        public override object Clone()
        {
            var clone = new Account();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="Account"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Id = Cleaner.Clean(Id, StringTrim.UseDefault, StringTransform.UseDefault);
            CreationDate = Cleaner.Clean(CreationDate, DateTimeTransform.DateOnly);
            DisplayName = Cleaner.Clean(DisplayName, StringTrim.UseDefault, StringTransform.UseDefault);
            Nickname = Cleaner.Clean(Nickname, StringTrim.UseDefault, StringTransform.UseDefault);
            OpenStatusSid = Cleaner.Clean(OpenStatusSid);
            IsOwned = Cleaner.Clean(IsOwned);
            MaskedNumber = Cleaner.Clean(MaskedNumber, StringTrim.UseDefault, StringTransform.UseDefault);
            ProductCategorySid = Cleaner.Clean(ProductCategorySid);
            ProductName = Cleaner.Clean(ProductName, StringTrim.UseDefault, StringTransform.UseDefault);

            OnAfterCleanUp();
        }
    
        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        /// <returns><c>true</c> indicates is initial; otherwise, <c>false</c>.</returns>
        public override bool IsInitial
        {
            get
            {
                return Cleaner.IsInitial(Id)
                    && Cleaner.IsInitial(CreationDate)
                    && Cleaner.IsInitial(DisplayName)
                    && Cleaner.IsInitial(Nickname)
                    && Cleaner.IsInitial(OpenStatusSid)
                    && Cleaner.IsInitial(IsOwned)
                    && Cleaner.IsInitial(MaskedNumber)
                    && Cleaner.IsInitial(ProductCategorySid)
                    && Cleaner.IsInitial(ProductName);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(Account from);

        #endregion
    } 

    /// <summary>
    /// Represents a <see cref="Account"/> collection.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Tightly coupled; OK.")]
    public partial class AccountCollection : EntityBaseCollection<Account>
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountCollection"/> class.
        /// </summary>
        public AccountCollection(){ }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountCollection"/> class with an entity range.
        /// </summary>
        /// <param name="entities">The <see cref="Account"/> entities.</param>
        public AccountCollection(IEnumerable<Account> entities) => AddRange(entities);

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="AccountCollection"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="AccountCollection"/>.</returns>
        public override object Clone()
        {
            var clone = new AccountCollection();
            foreach (Account item in this)
            {
                clone.Add((Account)item.Clone());
            }
                
            return clone;
        }
        
        #endregion

        #region Operator

        /// <summary>
        /// An implicit cast from a <see cref="AccountCollectionResult"/> to a <see cref="AccountCollection"/>.
        /// </summary>
        /// <param name="result">The <see cref="AccountCollectionResult"/>.</param>
        /// <returns>The corresponding <see cref="AccountCollection"/>.</returns>
        public static implicit operator AccountCollection(AccountCollectionResult result) => result?.Result!;

        #endregion
    }

    /// <summary>
    /// Represents a <see cref="Account"/> collection result.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Tightly coupled; OK.")]
    public class AccountCollectionResult : EntityCollectionResult<AccountCollection, Account>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountCollectionResult"/> class.
        /// </summary>
        public AccountCollectionResult() { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountCollectionResult"/> class with default <see cref="PagingArgs"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public AccountCollectionResult(PagingArgs? paging) : base(paging) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountCollectionResult"/> class with a <paramref name="collection"/> of items to add.
        /// </summary>
        /// <param name="collection">A collection containing items to add.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public AccountCollectionResult(IEnumerable<Account> collection, PagingArgs? paging = null) : base(paging) => Result.AddRange(collection);
        
        /// <summary>
        /// Creates a deep copy of the <see cref="AccountCollectionResult"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="AccountCollectionResult"/>.</returns>
        public override object Clone()
        {
            var clone = new AccountCollectionResult();
            clone.CopyFrom(this);
            return clone;
        }
    }
}

#nullable restore