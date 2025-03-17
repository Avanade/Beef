using Microsoft.SemanticKernel.ChatCompletion;

namespace AIChat.AOAI.Business
{
    public partial class ChatManager
    {
        /// <summary>
        /// Chat completion
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<Result<ChatCompletionResponse>> ChatCompletionOnImplementationAsync(string? message)
        {
            ChatCompletionResponse result = new();
            try
            {
                var histories = await _dataService.GetChatCompletionsAsync(3);
                if (histories.IsFailure)
                {
                    histories = new();
                }
                var generateMessage = await GenerateCompletion(message ?? string.Empty, histories);

                await _dataService.CreateAsync(
                    new()
                    {
                        RoleSid = "User",
                        Prompt = message,
                    }
                );

                await _dataService.CreateAsync(
                    new()
                    {
                        RoleSid = "Assistant",
                        Prompt = generateMessage,
                    }
                );

                result.Message = generateMessage;
                return Result.Ok(result);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Generate completion
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<string> GenerateCompletion(string message, ChatCollectionResult? histories)
        {
            string systemPrompt = """
                You are an intelligent assistant.You are designed to provide helpful answers to user questions.
                You are friendly, and informative and can be lighthearted. Be concise in your response, but still friendly.
                """;

            // Create the completion
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            ChatHistory history = [];
            history.Add(
                new()
                {
                    Role = AuthorRole.System,
                    Content = systemPrompt,
                }
            );

            if (histories != null && histories.Items.Count > 0)
            {
                foreach (var item in histories.Items)
                {
                    if (item == null) continue;

                    history.Add(
                        new()
                        {
                            Role = item.Role?.Code switch
                            {
                                "User" => AuthorRole.User,
                                "System" => AuthorRole.System,
                                "Assistant" => AuthorRole.Assistant,
                                _ => AuthorRole.System,
                            },
                            Items = [
                                new TextContent { Text = item.Prompt }
                            ]
                        }
                    );
                }
            }

            history.Add(
                new()
                {
                    Role = AuthorRole.User,
                    AuthorName = CoreEx.ExecutionContext.Current.UserName,
                    Items = [
                        new TextContent { Text = message },
                    ]
                }
            );

            var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory: history, kernel: _kernel);

            return response.Content ?? string.Empty;
        }
    }
}
