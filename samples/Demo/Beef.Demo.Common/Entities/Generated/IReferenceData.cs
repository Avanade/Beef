/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using Beef.RefData;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Provides for the required <b>ReferenceData</b> capabilities. 
    /// </summary>
    public partial interface IReferenceData : IReferenceDataProvider
    {
        #region Collections

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.CountryCollection"/>.
        /// </summary>
        RefDataNamespace.CountryCollection Country { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.USStateCollection"/>.
        /// </summary>
        RefDataNamespace.USStateCollection USState { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.GenderCollection"/>.
        /// </summary>
        RefDataNamespace.GenderCollection Gender { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.EyeColorCollection"/>.
        /// </summary>
        RefDataNamespace.EyeColorCollection EyeColor { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.PowerSourceCollection"/>.
        /// </summary>
        RefDataNamespace.PowerSourceCollection PowerSource { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.CompanyCollection"/>.
        /// </summary>
        RefDataNamespace.CompanyCollection Company { get; }

        #endregion
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore