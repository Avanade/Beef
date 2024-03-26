using Microsoft.Azure.Cosmos;

namespace Company.AppName.Business.Data;

/// <summary>
/// Enables the <b>Company.AppName</b> CosmosDb client.
/// </summary>
public interface ICosmos : CoreEx.Cosmos.ICosmosDb
{
    /// <summary>
    /// Exposes <see cref="Person"/> entity from the <b>Person</b> container.
    /// </summary>
    public CosmosDbContainer<Person, Model.Person> Persons => Container<Person, Model.Person>("Person");
}

/// <summary>
/// Provides the <b>Company.AppName</b> CosmosDb client.
/// </summary>
/// <param name="database">The <see cref="Database"/>.</param>
/// <param name="mapper">The <see cref="IMapper"/>.</param>
/// <param name="invoker">The optional <see cref="CosmosDbInvoker"/>.</param>
public class CosmosDb(Database database, IMapper mapper, CosmosDbInvoker? invoker = null) : CoreEx.Cosmos.CosmosDb(database, mapper, invoker), ICosmos { }