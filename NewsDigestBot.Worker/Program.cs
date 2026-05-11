using Microsoft.EntityFrameworkCore;
using NewsDigestBot.Infrastructure.Data;
using NewsDigestBot.Worker.Services;
using NewsDigestBot.Worker.Workers;


var builder = Host.CreateApplicationBuilder(args);

// БД
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));


// Сервисы
builder.Services.AddScoped<RssParserService>();
builder.Services.AddScoped<ArticleProcessorService>();
builder.Services.AddSingleton<LlmService>();

// Фоновый воркер
builder.Services.AddHostedService<NewsFetcherWorker>();
builder.Services.AddHostedService<ArticleProcessorWorker>();



var host = builder.Build();
host.Run();