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
    /// Represents the <see cref="Person"/> arguments entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class PersonArgs : EntityBase, IEquatable<PersonArgs>
    {
        #region Privates

        private string? _firstName;
        private string? _lastName;
        private List<string>? _gendersSids;

        #endregion

        #region Properties

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
        /// Gets or sets the <see cref="Genders"/> list using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonProperty("genders", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Genders")]
        public List<string>? GendersSids
        {
            get => _gendersSids;
            set => SetValue(ref _gendersSids, value, false, false, nameof(Genders));
        }

        /// <summary>
        /// Gets or sets the Genders (see <see cref="Gender"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [Display(Name="Genders")]
        public ReferenceDataSidList<Gender, string>? Genders
        {
            get => new ReferenceDataSidList<Gender, string>(ref _gendersSids);
            set => SetValue(ref _gendersSids, value?.ToSidList() ?? null, false, false, nameof(Genders)); 
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (!(obj is PersonArgs val))
                return false;

            return Equals(val);
        }

        /// <summary>
        /// Determines whether the specified <see cref="PersonArgs"/> is equal to the current <see cref="PersonArgs"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(PersonArgs? obj)
        {
            if (((object)obj!) == ((object)this))
                return true;
            else if (((object)obj!) == null)
                return false;

            return base.Equals((object)obj)
                && Equals(FirstName, obj.FirstName)
                && Equals(LastName, obj.LastName)
                && Equals(GendersSids, obj.GendersSids);
        }

        /// <summary>
        /// Compares two <see cref="PersonArgs"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="PersonArgs"/> A.</param>
        /// <param name="b"><see cref="PersonArgs"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (PersonArgs? a, PersonArgs? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="PersonArgs"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="PersonArgs"/> A.</param>
        /// <param name="b"><see cref="PersonArgs"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (PersonArgs? a, PersonArgs? b) => !Equals(a, b);

        /// <summary>
        /// Returns a hash code for the <see cref="PersonArgs"/>.
        /// </summary>
        /// <returns>A hash code for the <see cref="PersonArgs"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(FirstName);
            hash.Add(LastName);
            hash.Add(GendersSids);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion
        
        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="PersonArgs"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="PersonArgs"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<PersonArgs>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="PersonArgs"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="PersonArgs"/> to copy from.</param>
        public void CopyFrom(PersonArgs from)
        {
             if (from == null)
                 throw new ArgumentNullException(nameof(from));

            CopyFrom((EntityBase)from);
            FirstName = from.FirstName;
            LastName = from.LastName;
            GendersSids = from.GendersSids;

            OnAfterCopyFrom(from);
        }
    
        #endregion
        
        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="PersonArgs"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="PersonArgs"/>.</returns>
        public override object Clone()
        {
            var clone = new PersonArgs();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="PersonArgs"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            FirstName = Cleaner.Clean(FirstName, StringTrim.UseDefault, StringTransform.UseDefault);
            LastName = Cleaner.Clean(LastName, StringTrim.UseDefault, StringTransform.UseDefault);
            GendersSids = Cleaner.Clean(GendersSids);

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
                return Cleaner.IsInitial(FirstName)
                    && Cleaner.IsInitial(LastName)
                    && Cleaner.IsInitial(GendersSids);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(PersonArgs from);

        #endregion
    } 
}

#pragma warning restore CA2227, CA1819
#pragma warning restore IDE0005
#nullable restore