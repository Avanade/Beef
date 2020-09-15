/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

using System;
using Beef.RefData;
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Common.Entities
{
    /// <summary>
    /// Provides for the required <b>ReferenceData</b> capabilities. 
    /// </summary>
    public partial interface IReferenceData : IReferenceDataProvider
    {
        #region Collections

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.GenderCollection"/>.
        /// </summary>
        RefDataNamespace.GenderCollection Gender { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.TerminationReasonCollection"/>.
        /// </summary>
        RefDataNamespace.TerminationReasonCollection TerminationReason { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.RelationshipTypeCollection"/>.
        /// </summary>
        RefDataNamespace.RelationshipTypeCollection RelationshipType { get; }

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.USStateCollection"/>.
        /// </summary>
        RefDataNamespace.USStateCollection USState { get; }

        #endregion
    }
}

#pragma warning restore IDE0005
#nullable restore