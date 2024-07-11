/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace MyEf.Hr.Business;

/// <summary>
/// Provides the <see cref="ReferenceData"/> implementation using the corresponding data services.
/// </summary>
public partial class ReferenceDataProvider : IReferenceDataProvider
{
    private readonly IReferenceDataDataSvc _dataService;
        
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceDataProvider"/> class.
    /// </summary>
    /// <param name="dataService">The <see cref="IReferenceDataDataSvc"/>.</param>
    public ReferenceDataProvider(IReferenceDataDataSvc dataService) { _dataService = dataService.ThrowIfNull(); ReferenceDataProviderCtor(); }

    partial void ReferenceDataProviderCtor(); // Enables the ReferenceDataProvider constructor to be extended.

    /// <inheritdoc/>
    public Type[] Types =>
    [
        typeof(RefDataNamespace.Gender),
        typeof(RefDataNamespace.TerminationReason),
        typeof(RefDataNamespace.RelationshipType),
        typeof(RefDataNamespace.USState),
        typeof(RefDataNamespace.PerformanceOutcome)
    ];

    /// <inheritdoc/>
    public Task<Result<IReferenceDataCollection>> GetAsync(Type type, CancellationToken cancellationToken = default) => _dataService.GetAsync(type);
}