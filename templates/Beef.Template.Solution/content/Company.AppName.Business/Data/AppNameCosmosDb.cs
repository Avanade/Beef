using Microsoft.Azure.Cosmos;

namespace Company.AppName.Business.Data;

/// <summary>
/// Provides the <b>Company.AppName</b> CosmosDb client.
/// </summary>
/// <param name="database">The <see cref="Database"/>.</param>
/// <param name="mapper">The <see cref="IMapper"/>.</param>
public class AppNameCosmosDb : CosmosDb 
{
    /// <summary>
    /// Gets the <c>Person</c> container identifier.
    /// </summary>
    public const string PersonContainerId = "Person";

    private readonly Lazy<CosmosDbContainer<Person, Model.Person>> _persons;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameCosmosDb"/> class.
    /// </summary>
    /// <param name="database">The CosmosDb <see cref="Database"/>.</param>
    /// <param name="mapper">The <see cref="IMapper"/>.</param>
    public AppNameCosmosDb(Database database, IMapper mapper) : base(database, mapper)
    {
        _persons = new(() => Container<Person, Model.Person>(PersonContainerId));
    }

    /// <summary>
    /// Exposes <see cref="Person"/> entity from the <see cref="PersonContainerId"/> container.
    /// </summary>
    public CosmosDbContainer<Person, Model.Person> Persons => _persons.Value;
}