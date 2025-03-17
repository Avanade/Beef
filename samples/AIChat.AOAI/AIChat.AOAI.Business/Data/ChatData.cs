using Microsoft.Azure.Cosmos;

namespace AIChat.AOAI.Business.Data
{
    public partial class ChatData
    {
        partial void ChatDataCtor()
        {
        }

        /// <summary>
        /// Performs the query filtering
        /// </summary>
        /// <param name="completions"></param>
        /// <returns></returns>
        private async Task<Result<ChatCollectionResult>> GetChatCompletionsOnImplementationAsync(int completions)
        {
            try
            {
                string query = """
                SELECT TOP @completions *
                FROM c
                ORDER BY c._ts DESC
                """;

                var queryDef = new QueryDefinition(query)
                                    .WithParameter("@completions", completions);
                using FeedIterator<Chat> resultSet = _cosmos.Chats.Container.GetItemQueryIterator<Chat>(queryDef);
                List<Chat> chats = [];
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<Chat> response = await resultSet.ReadNextAsync();
                    chats.AddRange(response);
                }

                var result = new ChatCollectionResult(chats);

                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }

        }
    }
}
