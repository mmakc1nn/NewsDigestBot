using NewsDigestBot.Worker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDigestBot.Worker.Workers
{
    public class NewsFetcherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NewsFetcherWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(30);

        public NewsFetcherWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<NewsFetcherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NewsFetcherWorker запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Начинаем парсинг RSS...");

                // Создаём scope для каждого цикла парсинга
                await using var scope = _scopeFactory.CreateAsyncScope();
                var parser = scope.ServiceProvider.GetRequiredService<RssParserService>();

                await parser.FetchAllAsync(stoppingToken);

                _logger.LogInformation("Парсинг завершён. Следующий через {Interval}", _interval);
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
