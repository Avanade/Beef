using CoreEx.Cosmos;
using Microsoft.Azure.Cosmos;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Demo</b> DocumentDb/CosmosDb client.
    /// </summary>
    public class DemoCosmosDb : CosmosDb
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoCosmosDb"/> class.
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        public DemoCosmosDb(Microsoft.Azure.Cosmos.Database database, IMapper mapper, CosmosDbInvoker? invoker = null) : base(database, mapper, invoker) { }

        public CosmosDbContainer<Robot, Model.Robot> Items => Container<Robot, Model.Robot>("Items");

        ///// <summary>
        ///// System.Text.Json not supported natively; see https://github.com/Azure/azure-cosmos-dotnet-v3/issues/2533 and https://github.com/Azure/azure-cosmos-dotnet-v3/tree/master/Microsoft.Azure.Cosmos.Samples/Usage/SystemTextJson.
        ///// </summary>
        //public class SystemTextJsonSerializer : CosmosSerializer
        //{
        //    private readonly JsonObjectSerializer _serializer;

        //    public SystemTextJsonSerializer() => _serializer 
        //        = new JsonObjectSerializer(new System.Text.Json.JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });

        //    public override T FromStream<T>(Stream stream)
        //    {
        //        using (stream)
        //        {
        //            if (stream.CanSeek && stream.Length == 0)
        //                return default!;

        //            if (typeof(Stream).IsAssignableFrom(typeof(T)))
        //                return (T)(object)stream;

        //            return (T)_serializer.Deserialize(stream, typeof(T), default)!;
        //        }
        //    }

        //    public override Stream ToStream<T>(T input)
        //    {
        //        MemoryStream streamPayload = new();
        //        _serializer.Serialize(streamPayload, input, input.GetType(), default);
        //        streamPayload.Position = 0;
        //        return streamPayload;
        //    }
        //}
    }
}