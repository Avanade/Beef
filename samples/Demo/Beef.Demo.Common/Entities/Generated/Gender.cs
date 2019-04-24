/*
 * This file is automatically generated; any changes will be lost. 
 */

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
    /// Represents the Gender entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class Gender : ReferenceDataBaseGuid
    {
        #region Constructor
      
        /// <summary>
        /// Initializes a new instance of the <see cref="Gender"/> class.
        /// </summary>
        public Gender()
        {
            this.GenderConstructor();
        }
        
        #endregion

        #region Operator

        /// <summary>
        /// An implicit cast from an <b>Id</b> to a <see cref="Gender"/>.
        /// </summary>
        /// <param name="id">The <b>Id</b>.</param>
        /// <returns>The corresponding <see cref="Gender"/>.</returns>
        public static implicit operator Gender(Guid id)
        {
            return ConvertFromId<Gender>(id);
        }

        /// <summary>
        /// An implicit cast from a <b>Code</b> to a <see cref="Gender"/>.
        /// </summary>
        /// <param name="code">The <b>Code</b>.</param>
        /// <returns>The corresponding <see cref="Gender"/>.</returns>
        public static implicit operator Gender(string code)
        {
            return ConvertFromCode<Gender>(code);
        }
        
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="Gender"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Gender"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<Gender>(from);
            CopyFrom((Gender)fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="Gender"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Gender"/> to copy from.</param>
        public void CopyFrom(Gender from)
        {
            CopyFrom((ReferenceDataBaseGuid)from);

            this.OnAfterCopyFrom(from);
        }
    
        #endregion
        
        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="Gender"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="Gender"/>.</returns>
        public override object Clone()
        {
            var clone = new Gender();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="Gender"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();

            this.OnAfterCleanUp();
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
      
        partial void GenderConstructor();

        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(Gender from);

        #endregion
    } 

    /// <summary>
    /// Represents a <see cref="Gender"/> collection.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Tightly coupled; OK.")]
    public partial class GenderCollection : ReferenceDataCollectionBase<Gender>
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new instance of the <see cref="GenderCollection"/> class.
        /// </summary>
        public GenderCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenderCollection"/> class with an entity range.
        /// </summary>
        /// <param name="entities">The <see cref="Gender"/> entities.</param>
        public GenderCollection(IEnumerable<Gender> entities)
        {
            AddRange(entities);
        }

        #endregion
    }
}
