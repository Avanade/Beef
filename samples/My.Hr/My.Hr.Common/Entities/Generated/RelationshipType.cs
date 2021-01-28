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
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Common.Entities
{
    /// <summary>
    /// Represents the Relationship Type entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ReferenceDataInterface(typeof(IReferenceData))]
    public partial class RelationshipType : ReferenceDataBaseGuid
    {
        #region Operator

        /// <summary>
        /// An implicit cast from an <b>Id</b> to a <see cref="RelationshipType"/>.
        /// </summary>
        /// <param name="id">The <b>Id</b>.</param>
        /// <returns>The corresponding <see cref="RelationshipType"/>.</returns>
        public static implicit operator RelationshipType(Guid id) => ConvertFromId<RelationshipType>(id);

        /// <summary>
        /// An implicit cast from a <b>Code</b> to a <see cref="RelationshipType"/>.
        /// </summary>
        /// <param name="code">The <b>Code</b>.</param>
        /// <returns>The corresponding <see cref="RelationshipType"/>.</returns>
        public static implicit operator RelationshipType(string? code) => ConvertFromCode<RelationshipType>(code);

        #endregion
    
        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="RelationshipType"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="RelationshipType"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<RelationshipType>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="RelationshipType"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="RelationshipType"/> to copy from.</param>
        public void CopyFrom(RelationshipType from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((ReferenceDataBaseGuid)from);

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="RelationshipType"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="RelationshipType"/>.</returns>
        public override object Clone()
        {
            var clone = new RelationshipType();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="RelationshipType"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();

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

                return true;
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(RelationshipType from);

        #endregion
    }

    #region Collection

    /// <summary>
    /// Represents the <see cref="RelationshipType"/> collection.
    /// </summary>
    public partial class RelationshipTypeCollection : ReferenceDataCollectionBase<RelationshipType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipTypeCollection"/> class.
        /// </summary>
        public RelationshipTypeCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipTypeCollection"/> class with an entities range.
        /// </summary>
        /// <param name="entities">The <see cref="RelationshipType"/> entities.</param>
        public RelationshipTypeCollection(IEnumerable<RelationshipType> entities) => AddRange(entities);
    }

    #endregion  
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649, CA2225
#nullable restore