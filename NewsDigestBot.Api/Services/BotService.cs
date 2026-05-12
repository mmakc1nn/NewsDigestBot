using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Core.Entities;
using NewsDigestBot.Infrastructure.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NewsDigestBot.Api.Services
{
    public class BotService
    {
        private readonly ITelegramBotClient _bot;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BotService> _logger;

        public BotService(
            ITelegramBotClient bot,
            IServiceScopeFactory scopeFactory,
            ILogger<BotService> logger)
        {
            _bot = bot;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken ct)
        {
            if (update.Message?.Text is not { } text) return;

            var chatId = update.Message.Chat.Id;
            var username = update.Message.From?.Username ?? "";
            var firstName = update.Message.From?.FirstName ?? "";

            _logger.LogInformation("Получена команда {Text} от {Username}", text, username);

            var command = text.Split(' ')[0].ToLower();

            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            switch (command)
            {
                case "/start":
                    await HandleStart(chatId, username, firstName, db, ct);
                    break;
                case "/topics":
                    await HandleTopics(chatId, db, ct);
                    break;
                case "/subscribe":
                    await HandleSubscribe(chatId, text, db, ct);
                    break;
                case "/unsubscribe":
                    await HandleUnsubscribe(chatId, text, db, ct);
                    break;
                case "/mysubs":
                    await HandleMySubs(chatId, db, ct);
                    break;
                case "/latest":
                    await HandleLatest(chatId, db, ct);
                    break;
                case "/digest":
                    await HandleDigest(chatId, text, db, ct);
                    break;
                case "/search":
                    await HandleSearch(chatId, text, db, ct);
                    break;
                default:
                    await _bot.SendMessage(chatId,
                        "Неизвестная команда. Используй /start для начала.",
                        cancellationToken: ct);
                    break;
            }
        }

        private async Task HandleStart(long chatId, string username, string firstName, AppDbContext db, CancellationToken ct)
        {
            // Регистрируем пользователя если не существует
            var user = await db.Users.FindAsync(new object[] { chatId }, ct);
            if (user is null)
            {
                user = new Core.Entities.User
                {
                    Id = chatId,
                    Username = username,
                    FirstName = firstName
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(ct);
            }

            var welcome = $"""
                👋 Привет, {firstName}!
            
                Я бот для персональных новостных дайджестов.
            
                📋 Команды:
                /topics — список доступных тем
                /subscribe <тема> — подписаться на тему
                /unsubscribe <тема> — отписаться от темы
                /mysubs — мои подписки
                /latest — последние новости
                /digest — получить дайджест
                /search <запрос> — поиск по новостям
                """;

            await _bot.SendMessage(chatId, welcome, cancellationToken: ct);
        }

        private async Task HandleTopics(long chatId, AppDbContext db, CancellationToken ct)
        {
            var topics = await db.Topics.ToListAsync(ct);

            var text = "📚 Доступные темы:\n\n" +
                string.Join("\n", topics.Select(t => $"• {t.Name} — /subscribe {t.Slug}"));

            await _bot.SendMessage(chatId, text, cancellationToken: ct);
        }

        private async Task HandleSubscribe(long chatId, string text, AppDbContext db, CancellationToken ct)
        {
            var parts = text.Split(' ');
            if (parts.Length < 2)
            {
                await _bot.SendMessage(chatId, "Укажи тему: /subscribe tech", cancellationToken: ct);
                return;
            }

            var slug = parts[1].ToLower();
            var topic = await db.Topics.FirstOrDefaultAsync(t => t.Slug == slug, ct);

            if (topic is null)
            {
                await _bot.SendMessage(chatId, $"Тема '{slug}' не найдена. Используй /topics для списка.", cancellationToken: ct);
                return;
            }

            var exists = await db.Subscriptions
                .AnyAsync(s => s.UserId == chatId && s.TopicId == topic.Id, ct);

            if (exists)
            {
                await _bot.SendMessage(chatId, $"Ты уже подписан на «{topic.Name}».", cancellationToken: ct);
                return;
            }

            db.Subscriptions.Add(new Subscription
            {
                UserId = chatId,
                TopicId = topic.Id
            });
            await db.SaveChangesAsync(ct);

            await _bot.SendMessage(chatId, $"✅ Подписан на «{topic.Name}»!", cancellationToken: ct);
        }

        private async Task HandleUnsubscribe(long chatId, string text, AppDbContext db, CancellationToken ct)
        {
            var parts = text.Split(' ');
            if (parts.Length < 2)
            {
                await _bot.SendMessage(chatId, "Укажи тему: /unsubscribe tech", cancellationToken: ct);
                return;
            }

            var slug = parts[1].ToLower();
            var sub = await db.Subscriptions
                .Include(s => s.Topic)
                .FirstOrDefaultAsync(s => s.UserId == chatId && s.Topic.Slug == slug, ct);

            if (sub is null)
            {
                await _bot.SendMessage(chatId, $"Ты не подписан на «{slug}».", cancellationToken: ct);
                return;
            }

            db.Subscriptions.Remove(sub);
            await db.SaveChangesAsync(ct);

            await _bot.SendMessage(chatId, $"❌ Отписан от «{sub.Topic.Name}».", cancellationToken: ct);
        }

        private async Task HandleMySubs(long chatId, AppDbContext db, CancellationToken ct)
        {
            var subs = await db.Subscriptions
                .Include(s => s.Topic)
                .Where(s => s.UserId == chatId)
                .ToListAsync(ct);

            if (!subs.Any())
            {
                await _bot.SendMessage(chatId, "У тебя нет активных подписок. Используй /topics.", cancellationToken: ct);
                return;
            }

            var text = "📋 Твои подписки:\n\n" +
                string.Join("\n", subs.Select(s => $"• {s.Topic.Name} — /unsubscribe {s.Topic.Slug}"));

            await _bot.SendMessage(chatId, text, cancellationToken: ct);
        }

        private async Task HandleLatest(long chatId, AppDbContext db, CancellationToken ct)
        {
            var articles = await db.Articles
                .Include(a => a.Topic)
                .OrderByDescending(a => a.PublishedAt)
                .Take(5)
                .ToListAsync(ct);

            if (!articles.Any())
            {
                await _bot.SendMessage(chatId, "Новостей пока нет.", cancellationToken: ct);
                return;
            }

            var text = "📰 Последние новости:\n\n" +
                string.Join("\n\n", articles.Select(a =>
                    $"*{a.Title}*\n{a.Source} • {a.PublishedAt:dd.MM HH:mm}\n{a.Url}"));

            await _bot.SendMessage(chatId, text,
                cancellationToken: ct);
        }

        private async Task HandleDigest(long chatId, string text, AppDbContext db, CancellationToken ct)
        {
            var parts = text.Split(' ', 2);
            var topicSlug = parts.Length > 1 ? parts[1].Trim().ToLower() : null;

            var subs = await db.Subscriptions
                .Where(s => s.UserId == chatId)
                .Select(s => s.TopicId)
                .ToListAsync(ct);

            if (!subs.Any())
            {
                await _bot.SendMessage(chatId,
                    "У тебя нет подписок. Используй /topics чтобы подписаться.",
                    cancellationToken: ct);
                return;
            }

            var query = db.Articles
                .Include(a => a.Topic)
                .Where(a => subs.Contains(a.TopicId) && a.IsSummarized);

            // Если указана тема — фильтруем по ней
            if (topicSlug is not null)
            {
                query = query.Where(a => a.Topic.Slug == topicSlug);
            }

            var articles = await query
                .OrderByDescending(a => a.PublishedAt)
                .Take(5)
                .ToListAsync(ct);

            if (!articles.Any())
            {
                var msg = topicSlug is not null
                    ? $"Новостей по теме «{topicSlug}» пока нет."
                    : "Новостей по твоим темам пока нет. Попробуй позже.";
                await _bot.SendMessage(chatId, msg, cancellationToken: ct);
                return;
            }

            var header = topicSlug is not null
                ? $"📰 Дайджест по теме «{topicSlug}»:"
                : $"📰 Твой дайджест — {articles.Count} новостей:";

            await _bot.SendMessage(chatId, header, cancellationToken: ct);

            foreach (var article in articles)
            {
                var articleText =
                    $"📌 {article.Title}\n\n" +
                    $"Резюме статьи:\n{article.Summary}\n\n" +
                    $"{article.Source} • {article.PublishedAt:dd.MM HH:mm}\n" +
                    $"{article.Url}";

                await _bot.SendMessage(chatId, articleText, cancellationToken: ct);
                await Task.Delay(300, ct);
            }
        }

        private async Task HandleSearch(long chatId, string text, AppDbContext db, CancellationToken ct)
        {
            var parts = text.Split(' ', 2);
            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                await _bot.SendMessage(chatId,
                    "Укажи запрос: /search искусственный интеллект",
                    cancellationToken: ct);
                return;
            }

            var query = parts[1].Trim();

            var articles = await db.Articles
                .Include(a => a.Topic)
                .Where(a => EF.Functions.ILike(a.Title, $"%{query}%") && a.IsSummarized)
                .OrderByDescending(a => a.PublishedAt)
                .Take(5)
                .ToListAsync(ct);

            if (!articles.Any())
            {
                await _bot.SendMessage(chatId,
                    $"По запросу «{query}» ничего не найдено.",
                    cancellationToken: ct);
                return;
            }

            var result = $"🔍 *Результаты поиска «{query}»:*\n\n" +
                string.Join("\n\n", articles.Select(a =>
                    $"*{a.Title}*\n" +
                    $"{a.Summary?.Split('.')[0]}\n" +
                    $"{a.Source} • {a.PublishedAt:dd.MM HH:mm}\n" +
                    $"{a.Url}"));

            await _bot.SendMessage(chatId, result,
                cancellationToken: ct);
        }
    }


}