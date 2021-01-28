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
    /// Represents the Employee entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class Employee : EmployeeBase, IEquatable<Employee>
    {
        #region Privates

        private Address? _address;
        private EmergencyContactCollection? _emergencyContacts;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Address (see <see cref="Common.Entities.Address"/>).
        /// </summary>
        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Address")]
        public Address? Address
        {
            get => _address;
            set => SetValue(ref _address, value, false, true, nameof(Address));
        }

        /// <summary>
        /// Gets or sets the Emergency Contacts.
        /// </summary>
        [JsonProperty("emergencyContacts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [Display(Name="Emergency Contacts")]
        public EmergencyContactCollection? EmergencyContacts
        {
            get => _emergencyContacts;
            set => SetValue(ref _emergencyContacts, value, false, false, nameof(EmergencyContacts));
        }

        #endregion

        #region IChangeTracking

        /// <summary>
        /// Resets the entity state to unchanged by accepting the changes (resets <see cref="EntityBase.ChangeTracking"/>).
        /// </summary>
        /// <remarks>Ends and commits the entity changes (see <see cref="EntityBase.EndEdit"/>).</remarks>
        public override void AcceptChanges()
        {
            Address?.AcceptChanges();
            base.AcceptChanges();
        }

        /// <summary>
        /// Determines that until <see cref="AcceptChanges"/> is invoked property changes are to be logged (see <see cref="EntityBase.ChangeTracking"/>).
        /// </summary>
        public override void TrackChanges()
        {
            Address?.TrackChanges();
            base.TrackChanges();
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Determines whether the specified object is equal to the current object by comparing the values of all the properties.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => obj is Employee val && Equals(val);

        /// <summary>
        /// Determines whether the specified <see cref="Employee"/> is equal to the current <see cref="Employee"/> by comparing the values of all the properties.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/> to compare with the current <see cref="Employee"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Employee"/> is equal to the current <see cref="Employee"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(Employee? value)
        {
            if (value == null)
                return false;
            else if (ReferenceEquals(value, this))
                return true;

            return base.Equals((object)value)
                && Equals(Address, value.Address)
                && Equals(EmergencyContacts, value.EmergencyContacts);
        }

        /// <summary>
        /// Compares two <see cref="Employee"/> types for equality.
        /// </summary>
        /// <param name="a"><see cref="Employee"/> A.</param>
        /// <param name="b"><see cref="Employee"/> B.</param>
        /// <returns><c>true</c> indicates equal; otherwise, <c>false</c> for not equal.</returns>
        public static bool operator == (Employee? a, Employee? b) => Equals(a, b);

        /// <summary>
        /// Compares two <see cref="Employee"/> types for non-equality.
        /// </summary>
        /// <param name="a"><see cref="Employee"/> A.</param>
        /// <param name="b"><see cref="Employee"/> B.</param>
        /// <returns><c>true</c> indicates not equal; otherwise, <c>false</c> for equal.</returns>
        public static bool operator != (Employee? a, Employee? b) => !Equals(a, b);

        /// <summary>
        /// Returns the hash code for the <see cref="Employee"/>.
        /// </summary>
        /// <returns>The hash code for the <see cref="Employee"/>.</returns>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Address);
            hash.Add(EmergencyContacts);
            return base.GetHashCode() ^ hash.ToHashCode();
        }
    
        #endregion

        #region ICopyFrom
    
        /// <summary>
        /// Performs a copy from another <see cref="Employee"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Employee"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<Employee>(from);
            CopyFrom(fval);
        }
        
        /// <summary>
        /// Performs a copy from another <see cref="Employee"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="Employee"/> to copy from.</param>
        public void CopyFrom(Employee from)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            CopyFrom((EmployeeBase)from);
            Address = CopyOrClone(from.Address, Address);
            EmergencyContacts = from.EmergencyContacts;

            OnAfterCopyFrom(from);
        }

        #endregion

        #region ICloneable
        
        /// <summary>
        /// Creates a deep copy of the <see cref="Employee"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="Employee"/>.</returns>
        public override object Clone()
        {
            var clone = new Employee();
            clone.CopyFrom(this);
            return clone;
        }
        
        #endregion
        
        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="Employee"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Address = Cleaner.Clean(Address);
            EmergencyContacts = Cleaner.Clean(EmergencyContacts);

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

                return Cleaner.IsInitial(Address)
                    && Cleaner.IsInitial(EmergencyContacts);
            }
        }

        #endregion

        #region PartialMethods
      
        partial void OnAfterCleanUp();

        partial void OnAfterCopyFrom(Employee from);

        #endregion
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649, CA2225
#nullable restore