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
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Represents the Company entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ReferenceDataInterface(typeof(IReferenceData))]
    public partial class Company : ReferenceDataBaseGuid
    {
        #region Properties

        /// <summary>
        /// Gets or sets the External Code.
        /// </summary>
        [JsonProperty("externalCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="External Code")]
        public string? ExternalCode
        {
            get => GetMapping<string>(nameof(ExternalCode));
            set { var __externalCode = GetMapping<string?>(nameof(ExternalCode)) ?? default; SetValue(ref __externalCode, value, true, StringTrim.UseDefault, StringTransform.UseDefault, nameof(ExternalCode)); SetMapping(nameof(ExternalCode), __externalCode!); }
        }

        #endregion

        #region Operator

        /// <summary>
        /// An implicit cast from an <b>Id</b> to a <see cref="Company"/>.
        /// </summary>
        /// <param name="id">The <b>Id</b>.</param>
        /// <returns>The corresponding <see cref="Company"/>.</returns>
        public static implicit operator Company(Guid id) => ConvertFromId<Company>(id);

        /// <summary>
        /// An implicit cast from a <b>Code</b> to a <see cref="Company"/>.
        /// </summary>
        /// <param name="code">The <b>Code</b>.</param>
        /// <returns>The corresponding <see cref="Company"/>.</returns>
        public static implicit operator Company(string? code) => ConvertFromCode<Company>(code);

        #endregion
    
        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="Company"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Company"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<Company>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="Company"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Company"/> to copy from.</param>
        public void CopyFrom(Company from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((ReferenceDataBaseGuid)from);
            ExternalCode = from.ExternalCode;

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="Company"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="Company"/>.</returns>
        public override object Clone()
        {
            var clone = new Company();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="Company"/> resetting property values as appropriate to ensure a basic level of data consistency.
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

                return Cleaner.IsInitial(ExternalCode);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(Company from);

        #endregion
    }

    #region Collection

    /// <summary>
    /// Represents the <see cref="Company"/> collection.
    /// </summary>
    public partial class CompanyCollection : ReferenceDataCollectionBase<Company>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyCollection"/> class.
        /// </summary>
        public CompanyCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyCollection"/> class with an entities range.
        /// </summary>
        /// <param name="entities">The <see cref="Company"/> entities.</param>
        public CompanyCollection(IEnumerable<Company> entities) => AddRange(entities);
    }

    #endregion  
}

#pragma warning restore
#nullable restore