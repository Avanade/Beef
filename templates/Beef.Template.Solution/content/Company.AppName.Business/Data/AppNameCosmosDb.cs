namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the <b>Company.AppName</b> CosmosDb client.
/// </summary>
public class AppNameCosmosDb : CosmosDbBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameCosmosDb"/> class.
    /// </summary>
    /// <param name="database">The <see cref="Microsoft.Azure.Cosmos.Database"/>.</param>
    /// <param name="mapper">The <see cref="IMapper"/>.</param>
    /// <param name="invoker">The <see cref="CosmosDbInvoker"/>.</param>
    public AppNameCosmosDb(AzCosmos.Database database, IMapper mapper, CosmosDbInvoker? invoker = null) : base(database, mapper, invoker) { }
}
