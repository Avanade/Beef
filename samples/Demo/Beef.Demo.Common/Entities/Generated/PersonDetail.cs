/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options
#pragma warning disable CA2227, CA1819 // Collection/Array properties should be read only; ignored, as acceptable for a DTO.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Beef.RefData;
using Newtonsoft.Json;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Represents the <see cref="Person"/> detail entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class PersonDetail : Person, IEquatable<PersonDetail>
    {
        #region Privates

        private WorkHistoryCollection? _history;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the History.
        /// </summary>
        [JsonProperty("history", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="History")]
        public WorkHistoryCollection? History
        {
            get => _history;
            set => SetValue(ref _history, value, false, true, nameof(History));
        }

        #endregion

        #region IChangeTracking

        /// <summary>
        /// Resets the entity state to unchanged by accepting the changes (resets <see cref="EntityBase.ChangeTracking"/>).
        /// </summary>
        /// <remarks>Ends and commits the entity changes (see <see cref="EntityBase.EndEdit"/>).</remarks>
        public override void AcceptChanges()
        {
            History?.AcceptChanges();
            base.AcceptChanges();
        }

        /// <summary>
        /// Determines that until <see cref="AcceptChanges"/> is invoked property changes are to be logged (see <see cref="EntityBase.ChangeTracking"/>).
        /// </summary>
        public override void TrackChanges()
        {
            History?.TrackChanges();
            base.TrackChanges();
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is PersonDetail val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="PersonDetail"/> is equal to the current <see cref="PersonDetail"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The <see cref="PersonDetail"/> to compare with the current <see cref="PersonDetail"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="PersonDetail"/> is equal to the current <see cref="PersonDetail"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(PersonDetail? value)
        {
            if (value == null)
                return false;
            else if (ReferenceEquals(value, this))
                return true;

            return base.Equals((object)value)
                && Equals(History, value.History);
        }

        /// <summary>
        /// Compares two <see cref="PersonDetail"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="PersonDetail"/> A.</param>
        /// <param name="b"><see cref="PersonDetail"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (PersonDetail? a, PersonDetail? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="PersonDetail"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="PersonDetail"/> A.</param>
        /// <param name="b"><see cref="PersonDetail"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (PersonDetail? a, PersonDetail? b) => !Equals(a, b);

        /// <summary>
        /// Returns the hash code for the <see cref="PersonDetail"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="PersonDetail"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(History);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="PersonDetail"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="PersonDetail"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<PersonDetail>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="PersonDetail"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="PersonDetail"/> to copy from.</param>
        public void CopyFrom(PersonDetail from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((Person)from);
            History = CopyOrClone(from.History, History);

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="PersonDetail"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="PersonDetail"/>.</returns>
        public override object Clone()
        {
            var clone = new PersonDetail();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="PersonDetail"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            History = Cleaner.Clean(History);

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
                if (!base.IsInitial)
                    return false;

                return Cleaner.IsInitial(History);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(PersonDetail from);

        #endregion
    }

    #region Collection

    /// <summary>
    /// Represents the <see cref="PersonDetail"/> collection.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Tightly coupled; OK.")]
    public partial class PersonDetailCollection : EntityBaseCollection<PersonDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDetailCollection"/> class.
        /// </summary>
        public PersonDetailCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDetailCollection"/> class with an entities range.
        /// </summary>
        /// <param name="entities">The <see cref="PersonDetail"/> entities.</param>
        public PersonDetailCollection(IEnumerable<PersonDetail> entities) => AddRange(entities);

        /// <summary>
        /// Creates a deep copy of the <see cref="PersonDetailCollection"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="PersonDetailCollection"/>.</returns>
        public override object Clone()
        {
            var clone = new PersonDetailCollection();
            foreach (var item in this)
            {
                clone.Add((PersonDetail)item.Clone());
            }
                
            return clone;
        }

        /// <summary>
        /// An implicit cast from the <see cref="PersonDetailCollectionResult"/> to a corresponding <see cref="PersonDetailCollection"/>.
        /// </summary>
        /// <param name="result">The <see cref="PersonDetailCollectionResult"/>.</param>
        /// <returns>The corresponding <see cref="PersonDetailCollection"/>.</returns>
        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Improves useability")]
        public static implicit operator PersonDetailCollection(PersonDetailCollectionResult result) => result?.Result!;
    }

    #endregion  

    #region CollectionResult

    /// <summary>
    /// Represents the <see cref="PersonDetail"/> collection result.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Tightly coupled; OK.")]
    public class PersonDetailCollectionResult : EntityCollectionResult<PersonDetailCollection, PersonDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDetailCollectionResult"/> class.
        /// </summary>
        public PersonDetailCollectionResult() { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDetailCollectionResult"/> class with <paramref name="paging"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public PersonDetailCollectionResult(PagingArgs? paging) : base(paging) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDetailCollectionResult"/> class with a <paramref name="collection"/> of items to add.
        /// </summary>
        /// <param name="collection">A collection containing items to add.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public PersonDetailCollectionResult(IEnumerable<PersonDetail> collection, PagingArgs? paging = null) : base(paging) => Result.AddRange(collection);
        
        /// <summary>
        /// Creates a deep copy of the <see cref="PersonDetailCollectionResult"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="PersonDetailCollectionResult"/>.</returns>
        public override object Clone()
        {
            var clone = new PersonDetailCollectionResult();
            clone.CopyFrom(this);
            return clone;
        }
    }

    #endregion
}

#pragma warning restore CA2227, CA1819
#pragma warning restore IDE0005
#nullable restore