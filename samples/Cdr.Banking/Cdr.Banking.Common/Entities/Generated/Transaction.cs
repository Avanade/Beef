/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649, CA2225

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
    /// Represents the Transaction entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class Transaction : EntityBase, IStringIdentifier, IEquatable<Transaction>
    {
        #region Privates

        private string? _id;
        private string? _accountId;
        private bool _isDetailAvailable;
        private string? _typeSid;
        private string? _statusSid;
        private string? _description;
        private DateTime _postingDateTime;
        private DateTime _executionDateTime;
        private decimal _amount;
        private string? _currency;
        private string? _reference;
        private string? _merchantName;
        private string? _merchantCategoryCode;
        private string? _billerCode;
        private string? _billerName;
        private string? _apcaNumber;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Account"/> identifier.
        /// </summary>
        [JsonProperty("transactionId", DefaultValueHandling = DefaultValueHandling.Include)]
        [Display(Name="Identifier")]
        public string? Id
        {
            get => _id;
            set => SetValue(ref _id, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Id));
        }

        /// <summary>
        /// Gets or sets the Account Id.
        /// </summary>
        [JsonProperty("accountId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Account")]
        public string? AccountId
        {
            get => _accountId;
            set => SetValue(ref _accountId, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(AccountId));
        }

        /// <summary>
        /// Indicates whether Is Detail Available.
        /// </summary>
        [JsonProperty("isDetailAvailable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Is Detail Available")]
        public bool IsDetailAvailable
        {
            get => _isDetailAvailable;
            set => SetValue(ref _isDetailAvailable, value, false, false, nameof(IsDetailAvailable));
        }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Type")]
        public string? TypeSid
        {
            get => _typeSid;
            set => SetValue(ref _typeSid, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Type));
        }

        /// <summary>
        /// Gets or sets the Type (see <see cref="RefDataNamespace.TransactionType"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Type")]
        public RefDataNamespace.TransactionType? Type
        {
            get => _typeSid;
            set => SetValue(ref _typeSid, value, false, false, nameof(Type)); 
        }

        /// <summary>
        /// Gets or sets the <see cref="Status"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Status")]
        public string? StatusSid
        {
            get => _statusSid;
            set => SetValue(ref _statusSid, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Status));
        }

        /// <summary>
        /// Gets or sets the Status (see <see cref="RefDataNamespace.TransactionStatus"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Status")]
        public RefDataNamespace.TransactionStatus? Status
        {
            get => _statusSid;
            set => SetValue(ref _statusSid, value, false, false, nameof(Status)); 
        }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Description")]
        public string? Description
        {
            get => _description;
            set => SetValue(ref _description, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Description));
        }

        /// <summary>
        /// Gets or sets the Posting Date Time.
        /// </summary>
        [JsonProperty("postingDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Posting Date Time")]
        public DateTime PostingDateTime
        {
            get => _postingDateTime;
            set => SetValue(ref _postingDateTime, value, false, DateTimeTransform.UseDefault, nameof(PostingDateTime));
        }

        /// <summary>
        /// Gets or sets the Execution Date Time.
        /// </summary>
        [JsonProperty("executionDateTime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Execution Date Time")]
        public DateTime ExecutionDateTime
        {
            get => _executionDateTime;
            set => SetValue(ref _executionDateTime, value, false, DateTimeTransform.UseDefault, nameof(ExecutionDateTime));
        }

        /// <summary>
        /// Gets or sets the Amount.
        /// </summary>
        [JsonProperty("amount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Amount")]
        public decimal Amount
        {
            get => _amount;
            set => SetValue(ref _amount, value, false, false, nameof(Amount));
        }

        /// <summary>
        /// Gets or sets the Currency.
        /// </summary>
        [JsonProperty("currency", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Currency")]
        public string? Currency
        {
            get => _currency;
            set => SetValue(ref _currency, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Currency));
        }

        /// <summary>
        /// Gets or sets the Reference.
        /// </summary>
        [JsonProperty("reference", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Reference")]
        public string? Reference
        {
            get => _reference;
            set => SetValue(ref _reference, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Reference));
        }

        /// <summary>
        /// Gets or sets the Merchant Name.
        /// </summary>
        [JsonProperty("merchantName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Merchant Name")]
        public string? MerchantName
        {
            get => _merchantName;
            set => SetValue(ref _merchantName, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(MerchantName));
        }

        /// <summary>
        /// Gets or sets the Merchant Category Code.
        /// </summary>
        [JsonProperty("merchantCategoryCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Merchant Category Code")]
        public string? MerchantCategoryCode
        {
            get => _merchantCategoryCode;
            set => SetValue(ref _merchantCategoryCode, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(MerchantCategoryCode));
        }

        /// <summary>
        /// Gets or sets the Biller Code.
        /// </summary>
        [JsonProperty("billerCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Biller Code")]
        public string? BillerCode
        {
            get => _billerCode;
            set => SetValue(ref _billerCode, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(BillerCode));
        }

        /// <summary>
        /// Gets or sets the Biller Name.
        /// </summary>
        [JsonProperty("billerName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Biller Name")]
        public string? BillerName
        {
            get => _billerName;
            set => SetValue(ref _billerName, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(BillerName));
        }

        /// <summary>
        /// Gets or sets the Apca Number.
        /// </summary>
        [JsonProperty("apcaNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Apca Number")]
        public string? ApcaNumber
        {
            get => _apcaNumber;
            set => SetValue(ref _apcaNumber, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(ApcaNumber));
        }

        #endregion

        #region IUniqueKey

        /// <summary>
        /// Indicates whether the <see cref="Transaction"/> has a <see cref="UniqueKey"/> value.
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
        /// <param name="id">The <see cref="Id"/>.</param>
        public static UniqueKey CreateUniqueKey(string? id) => new UniqueKey(id);

        /// <summary>
        /// Gets the <see cref="UniqueKey"/> (consists of the following property(s): <see cref="Id"/>).
        /// </summary>
        public override UniqueKey UniqueKey => new UniqueKey(Id);

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Transaction val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="Transaction"/> is equal to the current <see cref="Transaction"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The <see cref="Transaction"/> to compare with the current <see cref="Transaction"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Transaction"/> is equal to the current <see cref="Transaction"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Transaction? value)
        {
            if (value == null)
                return false;
            else if (ReferenceEquals(value, this))
                return true;

            return base.Equals((object)value)
                && Equals(Id, value.Id)
                && Equals(AccountId, value.AccountId)
                && Equals(IsDetailAvailable, value.IsDetailAvailable)
                && Equals(TypeSid, value.TypeSid)
                && Equals(StatusSid, value.StatusSid)
                && Equals(Description, value.Description)
                && Equals(PostingDateTime, value.PostingDateTime)
                && Equals(ExecutionDateTime, value.ExecutionDateTime)
                && Equals(Amount, value.Amount)
                && Equals(Currency, value.Currency)
                && Equals(Reference, value.Reference)
                && Equals(MerchantName, value.MerchantName)
                && Equals(MerchantCategoryCode, value.MerchantCategoryCode)
                && Equals(BillerCode, value.BillerCode)
                && Equals(BillerName, value.BillerName)
                && Equals(ApcaNumber, value.ApcaNumber);
        }

        /// <summary>
        /// Compares two <see cref="Transaction"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="Transaction"/> A.</param>
        /// <param name="b"><see cref="Transaction"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (Transaction? a, Transaction? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="Transaction"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="Transaction"/> A.</param>
        /// <param name="b"><see cref="Transaction"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (Transaction? a, Transaction? b) => !Equals(a, b);

        /// <summary>
        /// Returns the hash code for the <see cref="Transaction"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="Transaction"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(AccountId);
            hash.Add(IsDetailAvailable);
            hash.Add(TypeSid);
            hash.Add(StatusSid);
            hash.Add(Description);
            hash.Add(PostingDateTime);
            hash.Add(ExecutionDateTime);
            hash.Add(Amount);
            hash.Add(Currency);
            hash.Add(Reference);
            hash.Add(MerchantName);
            hash.Add(MerchantCategoryCode);
            hash.Add(BillerCode);
            hash.Add(BillerName);
            hash.Add(ApcaNumber);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="Transaction"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Transaction"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<Transaction>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="Transaction"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Transaction"/> to copy from.</param>
        public void CopyFrom(Transaction from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((EntityBase)from);
            Id = from.Id;
            AccountId = from.AccountId;
            IsDetailAvailable = from.IsDetailAvailable;
            TypeSid = from.TypeSid;
            StatusSid = from.StatusSid;
            Description = from.Description;
            PostingDateTime = from.PostingDateTime;
            ExecutionDateTime = from.ExecutionDateTime;
            Amount = from.Amount;
            Currency = from.Currency;
            Reference = from.Reference;
            MerchantName = from.MerchantName;
            MerchantCategoryCode = from.MerchantCategoryCode;
            BillerCode = from.BillerCode;
            BillerName = from.BillerName;
            ApcaNumber = from.ApcaNumber;

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="Transaction"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="Transaction"/>.</returns>
        public override object Clone()
        {
            var clone = new Transaction();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="Transaction"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Id = Cleaner.Clean(Id, StringTrim.UseDefault, StringTransform.UseDefault);
            AccountId = Cleaner.Clean(AccountId, StringTrim.UseDefault, StringTransform.UseDefault);
            IsDetailAvailable = Cleaner.Clean(IsDetailAvailable);
            TypeSid = Cleaner.Clean(TypeSid);
            StatusSid = Cleaner.Clean(StatusSid);
            Description = Cleaner.Clean(Description, StringTrim.UseDefault, StringTransform.UseDefault);
            PostingDateTime = Cleaner.Clean(PostingDateTime, DateTimeTransform.UseDefault);
            ExecutionDateTime = Cleaner.Clean(ExecutionDateTime, DateTimeTransform.UseDefault);
            Amount = Cleaner.Clean(Amount);
            Currency = Cleaner.Clean(Currency, StringTrim.UseDefault, StringTransform.UseDefault);
            Reference = Cleaner.Clean(Reference, StringTrim.UseDefault, StringTransform.UseDefault);
            MerchantName = Cleaner.Clean(MerchantName, StringTrim.UseDefault, StringTransform.UseDefault);
            MerchantCategoryCode = Cleaner.Clean(MerchantCategoryCode, StringTrim.UseDefault, StringTransform.UseDefault);
            BillerCode = Cleaner.Clean(BillerCode, StringTrim.UseDefault, StringTransform.UseDefault);
            BillerName = Cleaner.Clean(BillerName, StringTrim.UseDefault, StringTransform.UseDefault);
            ApcaNumber = Cleaner.Clean(ApcaNumber, StringTrim.UseDefault, StringTransform.UseDefault);

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
                    && Cleaner.IsInitial(AccountId)
                    && Cleaner.IsInitial(IsDetailAvailable)
                    && Cleaner.IsInitial(TypeSid)
                    && Cleaner.IsInitial(StatusSid)
                    && Cleaner.IsInitial(Description)
                    && Cleaner.IsInitial(PostingDateTime)
                    && Cleaner.IsInitial(ExecutionDateTime)
                    && Cleaner.IsInitial(Amount)
                    && Cleaner.IsInitial(Currency)
                    && Cleaner.IsInitial(Reference)
                    && Cleaner.IsInitial(MerchantName)
                    && Cleaner.IsInitial(MerchantCategoryCode)
                    && Cleaner.IsInitial(BillerCode)
                    && Cleaner.IsInitial(BillerName)
                    && Cleaner.IsInitial(ApcaNumber);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(Transaction from);

        #endregion
    }

    #region Collection

    /// <summary>
    /// Represents the <see cref="Transaction"/> collection.
    /// </summary>
    public partial class TransactionCollection : EntityBaseCollection<Transaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionCollection"/> class.
        /// </summary>
        public TransactionCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionCollection"/> class with an entities range.
        /// </summary>
        /// <param name="entities">The <see cref="Transaction"/> entities.</param>
        public TransactionCollection(IEnumerable<Transaction> entities) => AddRange(entities);

        /// <summary>
        /// Creates a deep copy of the <see cref="TransactionCollection"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="TransactionCollection"/>.</returns>
        public override object Clone()
        {
            var clone = new TransactionCollection();
            foreach (var item in this)
            {
                clone.Add((Transaction)item.Clone());
            }
                
            return clone;
        }

        /// <summary>
        /// An implicit cast from the <see cref="TransactionCollectionResult"/> to a corresponding <see cref="TransactionCollection"/>.
        /// </summary>
        /// <param name="result">The <see cref="TransactionCollectionResult"/>.</param>
        /// <returns>The corresponding <see cref="TransactionCollection"/>.</returns>
        public static implicit operator TransactionCollection(TransactionCollectionResult result) => result?.Result!;
    }

    #endregion  

    #region CollectionResult

    /// <summary>
    /// Represents the <see cref="Transaction"/> collection result.
    /// </summary>
    public class TransactionCollectionResult : EntityCollectionResult<TransactionCollection, Transaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionCollectionResult"/> class.
        /// </summary>
        public TransactionCollectionResult() { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionCollectionResult"/> class with <paramref name="paging"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public TransactionCollectionResult(PagingArgs? paging) : base(paging) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionCollectionResult"/> class with a <paramref name="collection"/> of items to add.
        /// </summary>
        /// <param name="collection">A collection containing items to add.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public TransactionCollectionResult(IEnumerable<Transaction> collection, PagingArgs? paging = null) : base(paging) => Result.AddRange(collection);
        
        /// <summary>
        /// Creates a deep copy of the <see cref="TransactionCollectionResult"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="TransactionCollectionResult"/>.</returns>
        public override object Clone()
        {
            var clone = new TransactionCollectionResult();
            clone.CopyFrom(this);
            return clone;
        }
    }

    #endregion
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649, CA2225
#nullable restore