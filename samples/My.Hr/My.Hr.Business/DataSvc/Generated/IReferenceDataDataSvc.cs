/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Business.DataSvc;

/// <summary>
/// Provides the <b>ReferenceData</b> data services.
/// </summary>
public partial interface IReferenceDataDataSvc
{
    /// <summary>
    /// Gets the <see cref="IReferenceDataCollection"/> for the specified <see cref="IReferenceData"/> <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="IReferenceData"/> <see cref="Type"/>.</param>
    /// <returns>The corresponding <see cref="IReferenceDataCollection"/>.</returns>
    Task<Result<IReferenceDataCollection>> GetAsync(Type type);
}

#pragma warning restore
#nullable restore