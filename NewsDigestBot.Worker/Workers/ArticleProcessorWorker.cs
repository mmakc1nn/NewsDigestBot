using NewsDigestBot.Worker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDigestBot.Worker.Workers
{
    internal class ArticleProcessorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ArticleProcessorWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public ArticleProcessorWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ArticleProcessorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ArticleProcessorWorker запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<ArticleProcessorService>();

                await processor.ProcessPendingAsync(stoppingToken);

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
