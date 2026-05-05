using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using NewsDigestBot.Api.Services;

namespace NewsDigestBot.Api.Workers
{
    public class BotPollingWorker : BackgroundService
    {
        private readonly ITelegramBotClient _bot;
        private readonly BotService _botService;
        private readonly ILogger<BotPollingWorker> _logger;

        public BotPollingWorker(
            ITelegramBotClient bot,
            BotService botService,
            ILogger<BotPollingWorker> logger)
        {
            _bot = bot;
            _botService = botService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telegram бот запущен");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message }
            };

            _bot.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleUpdateAsync(
            ITelegramBotClient bot,
            Update update,
            CancellationToken ct)
        {
            try
            {
                await _botService.HandleUpdateAsync(update, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке обновления");
            }
        }

        private Task HandleErrorAsync(
            ITelegramBotClient bot,
            Exception ex,
            CancellationToken ct)
        {
            _logger.LogError(ex, "Ошибка Telegram polling");
            return Task.CompletedTask;
        }
    }
}