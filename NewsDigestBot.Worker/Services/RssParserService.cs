using CodeHollow.FeedReader;
using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Core.Entities;
using NewsDigestBot.Infrastructure.Data;
using NewsDigestBot.Worker.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsDigestBot.Worker.Services
{
    public class RssParserService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<RssParserService> _logger;

        public RssParserService(
            IDbContextFactory<AppDbContext> dbFactory,
            ILogger<RssParserService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async Task FetchAllAsync(CancellationToken ct = default)
        {
            foreach (var source in RssSources.Sources)
            {
                try
                {
                    await FetchSourceAsync(source, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при парсинге {Source}", source.Name);
                }
            }
        }

        private async Task FetchSourceAsync(RssSource source, CancellationToken ct)
        {
            _logger.LogInformation("Парсим {Source}...", source.Name);

            var feed = await FeedReader.ReadAsync(source.Url);

            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var topic = await db.Topics
                .FirstOrDefaultAsync(t => t.Slug == source.TopicSlug, ct);

            if (topic is null)
            {
                _logger.LogWarning("Топик '{Slug}' не найден в БД", source.TopicSlug);
                return;
            }

            int saved = 0;

            foreach (var item in feed.Items.Take(20))
            {
                var url = item.Link?.Trim();
                if (string.IsNullOrEmpty(url)) continue;

                var exists = await db.Articles.AnyAsync(a => a.Url == url, ct);
                if (exists) continue;

                var article = new Article
                {
                    Title = item.Title?.Trim() ?? "Без заголовка",
                    Url = url,
                    Source = source.Name,
                    TopicId = topic.Id,
                    PublishedAt = item.PublishingDate ?? DateTime.UtcNow,
                    OriginalContent = item.Description,
                    IsSummarized = false
                };

                db.Articles.Add(article);
                saved++;
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("{Source}: сохранено {Count} новых статей", source.Name, saved);
        }

    }
}
