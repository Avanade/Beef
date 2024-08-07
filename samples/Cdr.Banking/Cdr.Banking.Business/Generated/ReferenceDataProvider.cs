/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Cdr.Banking.Business;

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
        typeof(RefDataNamespace.OpenStatus),
        typeof(RefDataNamespace.ProductCategory),
        typeof(RefDataNamespace.AccountUType),
        typeof(RefDataNamespace.MaturityInstructions),
        typeof(RefDataNamespace.TransactionType),
        typeof(RefDataNamespace.TransactionStatus)
    ];

    /// <inheritdoc/>
    public Task<Result<IReferenceDataCollection>> GetAsync(Type type, CancellationToken cancellationToken = default) => _dataService.GetAsync(type);
}