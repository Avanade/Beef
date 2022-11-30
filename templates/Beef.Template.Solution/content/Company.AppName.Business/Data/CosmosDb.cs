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
public class CosmosDb : CoreEx.Cosmos.CosmosDb, ICosmos
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosDb"/> class.
    /// </summary>
    /// <param name="database">The <see cref="Database"/>.</param>
    /// <param name="mapper">The <see cref="IMapper"/>.</param>
    /// <param name="invoker">The optional <see cref="CosmosDbInvoker"/>.</param>
    public CosmosDb(Database database, IMapper mapper, CosmosDbInvoker? invoker = null) : base(database, mapper, invoker) { }
}