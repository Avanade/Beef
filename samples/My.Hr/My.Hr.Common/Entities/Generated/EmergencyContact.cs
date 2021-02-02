/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Beef.RefData;
using Newtonsoft.Json;
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Common.Entities
{
    /// <summary>
    /// Represents the Emergency Contact entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class EmergencyContact : EntityBase, IGuidIdentifier, IUniqueKey, IEquatable<EmergencyContact>
    {
        #region Privates

        private Guid _id;
        private string? _firstName;
        private string? _lastName;
        private string? _phoneNo;
        private string? _relationshipSid;
        private string? _relationshipText;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        [Display(Name="Identifier")]
        public Guid Id
        {
            get => _id;
            set => SetValue(ref _id, value, false, false, nameof(Id));
        }

        /// <summary>
        /// Gets or sets the First Name.
        /// </summary>
        [JsonProperty("firstName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="First Name")]
        public string? FirstName
        {
            get => _firstName;
            set => SetValue(ref _firstName, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(FirstName));
        }

        /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
        [JsonProperty("lastName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Last Name")]
        public string? LastName
        {
            get => _lastName;
            set => SetValue(ref _lastName, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(LastName));
        }

        /// <summary>
        /// Gets or sets the Phone No.
        /// </summary>
        [JsonProperty("phoneNo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Phone No")]
        public string? PhoneNo
        {
            get => _phoneNo;
            set => SetValue(ref _phoneNo, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(PhoneNo));
        }

        /// <summary>
        /// Gets or sets the <see cref="Relationship"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("relationship", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Relationship")]
        public string? RelationshipSid
        {
            get => _relationshipSid;
            set => SetValue(ref _relationshipSid, value, false, StringTrim.UseDefault, StringTransform.UseDefault, nameof(Relationship));
        }

        /// <summary>
        /// Gets the corresponding {{Relationship}} text (read-only where selected).
        /// </summary>
        [JsonProperty("relationshipText", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? RelationshipText { get => _relationshipText ?? GetRefDataText(() => Relationship); set => _relationshipText = value; }

        /// <summary>
        /// Gets or sets the Relationship (see <see cref="RefDataNamespace.RelationshipType"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Relationship")]
        public RefDataNamespace.RelationshipType? Relationship
        {
            get => _relationshipSid;
            set => SetValue(ref _relationshipSid, value, false, false, nameof(Relationship)); 
        }

        #endregion

        #region IUniqueKey
        
        /// <summary>
        /// Gets the list of property names that represent the unique key.
        /// </summary>
        public string[] UniqueKeyProperties => new string[] { nameof(Id) };

        /// <summary>
        /// Creates the <see cref="UniqueKey"/>.
        /// </summary>
        /// <returns>The <see cref="Beef.Entities.UniqueKey"/>.</returns>
        /// <param name="id">The <see cref="Id"/>.</param>
        public static UniqueKey CreateUniqueKey(Guid id) => new UniqueKey(id);

        /// <summary>
        /// Gets the <see cref="UniqueKey"/> (consists of the following property(s): <see cref="Id"/>).
        /// </summary>
        public UniqueKey UniqueKey => CreateUniqueKey(Id);

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is EmergencyContact val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="EmergencyContact"/> is equal to the current <see cref="EmergencyContact"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The <see cref="EmergencyContact"/> to compare with the current <see cref="EmergencyContact"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="EmergencyContact"/> is equal to the current <see cref="EmergencyContact"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(EmergencyContact? value)
        {
            if (value == null)
                return false;
            else if (ReferenceEquals(value, this))
                return true;

            return base.Equals((object)value)
                && Equals(Id, value.Id)
                && Equals(FirstName, value.FirstName)
                && Equals(LastName, value.LastName)
                && Equals(PhoneNo, value.PhoneNo)
                && Equals(RelationshipSid, value.RelationshipSid);
        }

        /// <summary>
        /// Compares two <see cref="EmergencyContact"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="EmergencyContact"/> A.</param>
        /// <param name="b"><see cref="EmergencyContact"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (EmergencyContact? a, EmergencyContact? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="EmergencyContact"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="EmergencyContact"/> A.</param>
        /// <param name="b"><see cref="EmergencyContact"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (EmergencyContact? a, EmergencyContact? b) => !Equals(a, b);

        /// <summary>
        /// Returns the hash code for the <see cref="EmergencyContact"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="EmergencyContact"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Id);
            hash.Add(FirstName);
            hash.Add(LastName);
            hash.Add(PhoneNo);
            hash.Add(RelationshipSid);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="EmergencyContact"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="EmergencyContact"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<EmergencyContact>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="EmergencyContact"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="EmergencyContact"/> to copy from.</param>
        public void CopyFrom(EmergencyContact from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((EntityBase)from);
            Id = from.Id;
            FirstName = from.FirstName;
            LastName = from.LastName;
            PhoneNo = from.PhoneNo;
            RelationshipSid = from.RelationshipSid;

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="EmergencyContact"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="EmergencyContact"/>.</returns>
        public override object Clone()
        {
            var clone = new EmergencyContact();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="EmergencyContact"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Id = Cleaner.Clean(Id);
            FirstName = Cleaner.Clean(FirstName, StringTrim.UseDefault, StringTransform.UseDefault);
            LastName = Cleaner.Clean(LastName, StringTrim.UseDefault, StringTransform.UseDefault);
            PhoneNo = Cleaner.Clean(PhoneNo, StringTrim.UseDefault, StringTransform.UseDefault);
            RelationshipSid = Cleaner.Clean(RelationshipSid);

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
                    && Cleaner.IsInitial(FirstName)
                    && Cleaner.IsInitial(LastName)
                    && Cleaner.IsInitial(PhoneNo)
                    && Cleaner.IsInitial(RelationshipSid);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(EmergencyContact from);

        #endregion
    }

    #region Collection

    /// <summary>
    /// Represents the <see cref="EmergencyContact"/> collection.
    /// </summary>
    public partial class EmergencyContactCollection : EntityBaseCollection<EmergencyContact>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmergencyContactCollection"/> class.
        /// </summary>
        public EmergencyContactCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmergencyContactCollection"/> class with an entities range.
        /// </summary>
        /// <param name="entities">The <see cref="EmergencyContact"/> entities.</param>
        public EmergencyContactCollection(IEnumerable<EmergencyContact> entities) => AddRange(entities);

        /// <summary>
        /// Creates a deep copy of the <see cref="EmergencyContactCollection"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="EmergencyContactCollection"/>.</returns>
        public override object Clone()
        {
            var clone = new EmergencyContactCollection();
            foreach (var item in this)
            {
                clone.Add((EmergencyContact)item.Clone());
            }
                
            return clone;
        }
    }

    #endregion  
}

#pragma warning restore
#nullable restore