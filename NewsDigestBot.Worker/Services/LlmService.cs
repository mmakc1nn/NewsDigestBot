using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDigestBot.Worker.Services
{
    internal class LlmService
    {
        private readonly ChatClient _client;
        private readonly ILogger<LlmService> _logger;

        public LlmService(IConfiguration config, ILogger<LlmService> logger)
        {
            _logger = logger;

            var apiKey = config["Llm:ApiKey"]!;
            var baseUrl = config["Llm:BaseUrl"]!;
            var model = config["Llm:Model"]!;

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(baseUrl)
            };

            var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), options);
            _client = openAiClient.GetChatClient(model);
        }

        public async Task<string?> SummarizeAsync(string title, string? content, string? topic, CancellationToken ct = default)
        {
            var text = string.IsNullOrWhiteSpace(content) ? title : $"{title}\n\n{content}";

            var prompt = $"""
            Ты — новостной редактор. Сделай краткое резюме статьи в 2-3 предложениях на русском языке.
            Пиши нейтрально, только факты. Не используй вводные слова типа "В статье говорится".
            Тема статьи: {topic ?? "общая"}

            Статья:
            {text}
            """;

            try
            {
                var response = await _client.CompleteChatAsync(
                    new[] { new UserChatMessage(prompt) },
                    cancellationToken: ct);

                return response.Value.Content[0].Text?.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Ошибка суммаризации: {Error}", ex.Message);
                return null;
            }
        }
    }
}
