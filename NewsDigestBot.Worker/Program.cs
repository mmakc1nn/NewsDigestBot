using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Infrastructure.Data;
using NewsDigestBot.Worker.Services;
using NewsDigestBot.Worker.Workers;


var builder = Host.CreateApplicationBuilder(args);

// БД
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddHttpClient<ArticleContentFetcher>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.TryAddWithoutValidation(
        "User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
    client.DefaultRequestHeaders.TryAddWithoutValidation(
        "Accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
    client.DefaultRequestHeaders.TryAddWithoutValidation(
        "Accept-Language", "ru-RU,ru;q=0.9,en;q=0.8");
});


// Сервисы
builder.Services.AddScoped<RssParserService>();


// Фоновый воркер
builder.Services.AddHostedService<NewsFetcherWorker>();



var host = builder.Build();
host.Run();