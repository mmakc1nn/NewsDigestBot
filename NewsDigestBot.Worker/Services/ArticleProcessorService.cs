using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDigestBot.Worker.Services
{
    internal class ArticleProcessorService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly LlmService _llmService;
        private readonly ILogger<ArticleProcessorService> _logger;

        public ArticleProcessorService(
            IDbContextFactory<AppDbContext> dbFactory,
            LlmService llmService,
            ILogger<ArticleProcessorService> logger)
        {
            _dbFactory = dbFactory;
            _llmService = llmService;
            _logger = logger;
        }

        public async Task ProcessPendingAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);


            var articles = await db.Articles
                .Include(a => a.Topic)
                .Where(a => !a.IsSummarized)
                .OrderByDescending(a => a.PublishedAt)
                .Take(10) 
                .ToListAsync(ct);

            if (!articles.Any())
            {
                _logger.LogInformation("Нет статей для суммаризации");
                return;
            }

            _logger.LogInformation("Суммаризируем {Count} статей...", articles.Count);

            foreach (var article in articles)
            {
                var summary = await _llmService.SummarizeAsync(
                    article.Title,
                    article.OriginalContent,
                    article.Topic?.Name,
                    ct);

                if (summary is not null)
                {
                    article.Summary = summary;
                    article.IsSummarized = true;
                    _logger.LogInformation("Суммаризировано: {Title}", article.Title);
                }
                else
                {            
                    article.IsSummarized = true;
                    _logger.LogWarning("Не удалось суммаризировать: {Title}", article.Title);
                }
         
                await Task.Delay(500, ct);
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Суммаризация завершена");
        }
    }
}
