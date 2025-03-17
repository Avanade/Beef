using Microsoft.Azure.Cosmos;

namespace AIChat.AOAI.Business.Data;

/// <summary>
/// Provides the <b>AIChat.AOAI</b> CosmosDb client.
/// </summary>
/// <param name="database">The <see cref="Database"/>.</param>
/// <param name="mapper">The <see cref="IMapper"/>.</param>
public class AOAICosmosDb : CosmosDb 
{
    /// <summary>
    /// Gets the <c>Person</c> container identifier.
    /// </summary>
    public const string ChatContainerId = "Chats";

    private readonly Lazy<CosmosDbContainer<Chat, Model.Chat>> _chats;

    /// <summary>
    /// Initializes a new instance of the <see cref="AOAICosmosDb"/> class.
    /// </summary>
    /// <param name="database">The CosmosDb <see cref="Database"/>.</param>
    /// <param name="mapper">The <see cref="IMapper"/>.</param>
    public AOAICosmosDb(Database database, IMapper mapper) : base(database, mapper)
    {
        _chats = new(() => Container<Chat, Model.Chat>(ChatContainerId));
    }

    /// <summary>
    /// Exposes <see cref="Chat"/> entity from the <see cref="ChatContainerId"/> container.
    /// </summary>
    public CosmosDbContainer<Chat, Model.Chat> Chats => _chats.Value;
}